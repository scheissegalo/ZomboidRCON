using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using ZomboidRCON.UpdateSystem.GithubApi;
using System.Text.Json;
using System.Diagnostics;
using Downloader;

namespace ZomboidRCON.UpdateSystem
{
    public class Updator
    {
        const string API_URL = "https://api.github.com/repos/%repo_name%/releases";
        Version version;
        string githubRepo;
        string assetName;
        string rootPath;
        bool includePrerelease, _checking = false, _updating = false;
        Action<bool, Release, string>? beforeUpdate;

        public Updator(string githubRepo, string currentVersion, bool includePrerelease = false, string assetName = "", string rootPath = "", Action<bool, Release, string>? beforeUpdateCallback = null)
        {
            if (!string.IsNullOrWhiteSpace(assetName))
            {
                string[] an = assetName.Split('.');
                if (an.Length < 2 || an[an.Length - 1].ToLower() != "zip") throw new Exceptions.InvalidAssetNameFormatException();
                this.assetName = assetName;
                if (string.IsNullOrWhiteSpace(rootPath)) this.rootPath = Directory.GetCurrentDirectory();
                else this.rootPath = rootPath;
            }
            try
            {
                version = new Version(currentVersion);
            }
            catch (Exceptions.InvalidVersionFormatException ex)
            {
                throw ex;
            }
            this.githubRepo = githubRepo;
            this.includePrerelease = includePrerelease;
            beforeUpdate = beforeUpdateCallback;
            Debug.Write("Updator: Version '" + version.ToString() + "'\n");
        }

        public async void CheckForUpdate(Action<UpdateResult> callback)
        {
            _checking = true;
            using (var httpClient = new HttpClient())
            {
                string url = API_URL.Replace("%repo_name%", githubRepo);
                try
                {
                    httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; AcmeInc/1.0)");
                    string json = await httpClient.GetStringAsync(url);
                    List<Release>? releases = JsonSerializer.Deserialize<List<Release>>(json);
                    if (releases == null)
                    {
                        callback(new UpdateResult { UpdateStatus = UpdateStatus.Error, Message = "Unable to fetch releases!", Release = null });
                        _checking = false;
                        return;
                    }
                    if (releases.Count < 1)
                    {
                        callback(new UpdateResult { UpdateStatus = UpdateStatus.Error, Message = "No releases were found!", Release = null });
                        _checking = false;
                        return;
                    }
                    else
                    {
                        for (int i = 0; i < releases.Count; ++i)
                        {
                            Release r = releases[i];
                            if (!includePrerelease && r.prerelease) continue;
                            Version rVersion = new(r.tag_name);
                            _checking = false;
                            if (rVersion > version)
                            {
                                callback(new UpdateResult { UpdateStatus = UpdateStatus.UpdateNeeded, Message = "New release was found", Release = r });
                                return;
                            }
                            else
                            {
                                callback(new UpdateResult { UpdateStatus = UpdateStatus.Current, Message = "Using current version", Release = r });
                                return;
                            }
                        }
                    }
                }
                catch (HttpRequestException ex)
                {
                    if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        callback(new UpdateResult { UpdateStatus = UpdateStatus.Error, Message = "Repo page not found ('" + githubRepo + "')", Release = null });
                    }
                    else callback(new UpdateResult { UpdateStatus = UpdateStatus.Error, Message = ex.Message + " (" + url + ")", Release = null });
                    _checking = false;
                    return;
                }
                catch (Exception e)
                {
                    callback(new UpdateResult { UpdateStatus = UpdateStatus.Error, Message = e.Message, Release = null });
                    _checking = false;
                    return;
                }
            }
            callback(new UpdateResult { UpdateStatus = UpdateStatus.Error, Message = "Unknown error has occured", Release = null });
            _checking = false;
        }

        public async Task<UpdateResult> CheckForUpdate()
        {
            _checking = true;
            using (var httpClient = new HttpClient())
            {
                string url = API_URL.Replace("%repo_name%", githubRepo);
                try
                {
                    httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; AcmeInc/1.0)");
                    string json = await httpClient.GetStringAsync(url);
                    List<Release>? releases = JsonSerializer.Deserialize<List<Release>>(json);
                    if (releases == null)
                    {
                        _checking = false;
                        return new UpdateResult { UpdateStatus = UpdateStatus.Error, Message = "Unable to fetch releases!", Release = null };
                    }
                    if (releases.Count < 1)
                    {
                        _checking = false;
                        return new UpdateResult { UpdateStatus = UpdateStatus.Error, Message = "No releases were found!", Release = null };
                    }
                    else
                    {
                        for (int i = 0; i < releases.Count; ++i)
                        {
                            Release r = releases[i];
                            if (!includePrerelease && r.prerelease) continue;
                            Version rVersion = new(r.tag_name);
                            _checking = false;
                            if (rVersion > version)
                            {
                                return new UpdateResult { UpdateStatus = UpdateStatus.UpdateNeeded, Message = "New release was found", Release = r };
                            }
                            else
                            {
                                return new UpdateResult { UpdateStatus = UpdateStatus.Current, Message = "Using current version", Release = r };
                            }
                        }
                    }
                }
                catch (HttpRequestException ex)
                {
                    _checking = false;
                    if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return new UpdateResult { UpdateStatus = UpdateStatus.Error, Message = "Repo page not found ('" + githubRepo + "')", Release = null };
                    }
                    else return new UpdateResult { UpdateStatus = UpdateStatus.Error, Message = ex.Message + " (" + url + ")", Release = null };
                }
                catch (Exception e)
                {
                    _checking = false;
                    return new UpdateResult { UpdateStatus = UpdateStatus.Error, Message = e.Message, Release = null };
                }
            }
            _checking = false;
            return new UpdateResult { UpdateStatus = UpdateStatus.Error, Message = "Unknown error has occured", Release = null };
        }

        public Asset? GetDownloadableAsset(Release release)
        {
            foreach (Asset asset in release.assets)
            {
                if (asset.name == assetName)
                {
                    return asset;
                }
            }
            return null;
        }

        public async Task Update(Release toRelease, bool autoInstall, EventHandler<DownloadStartedEventArgs>? OnDownloadStarted = null,
            EventHandler<DownloadProgressChangedEventArgs>? OnChunkDownloadProgressChanged = null,
            EventHandler<DownloadProgressChangedEventArgs>? OnDownloadProgressChanged = null,
            EventHandler<System.ComponentModel.AsyncCompletedEventArgs>? OnDownloadFileCompleted = null)
        {
            if (string.IsNullOrWhiteSpace(assetName) || string.IsNullOrWhiteSpace(GetFileFullPath)) return;
            if (IsBusy) return;
            _updating = true;
            Asset? downloadAsset = GetDownloadableAsset(toRelease);
            if (downloadAsset != null)
            {
                await Downloader.Download(GetFileFullPath, downloadAsset.browser_download_url, OnDownloadStarted, OnChunkDownloadProgressChanged, OnDownloadProgressChanged, OnDownloadFileCompleted);
                if (autoInstall)
                {
                    _updating = false;
                    InstallUpdate();
                    Environment.Exit(0);
                }
            }
            _updating = false;
        }

        public void InstallUpdate()
        {
            if (string.IsNullOrWhiteSpace(assetName) || string.IsNullOrWhiteSpace(GetFileFullPath)) return;
            if (IsBusy) return;
            if (!File.Exists(GetFileFullPath)) return;

            try
            {
                string extractPath = rootPath;
                ZipFile.ExtractToDirectory(GetFileFullPath, extractPath, overwriteFiles: true);
                File.Delete(GetFileFullPath);

                string exeName = Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetExecutingAssembly().Location) + (OperatingSystem.IsWindows() ? ".exe" : "");
                string exePath = Path.Combine(exeName);
                if (File.Exists(exePath))
                {
                    Process.Start(new ProcessStartInfo(exePath) { UseShellExecute = false });
                }
                else
                {
                    Process.Start(new ProcessStartInfo(Process.GetCurrentProcess().MainModule?.FileName ?? exeName) { UseShellExecute = false });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Update install failed: " + ex.Message);
            }
        }

        public bool IsCheckingForUpdate { get { return _checking; } }
        public bool IsUpdating { get { return _updating; } }
        public bool IsBusy { get { return _checking || _updating; } }

        public string? GetFileFullPath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(assetName)) return null;
                if (string.IsNullOrWhiteSpace(rootPath)) return assetName;
                return Path.Combine(rootPath, assetName);
            }
        }
    }
}
