namespace ZomboidRCON
{
    partial class Main
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            ListViewGroup listViewGroup1 = new ListViewGroup("Online Player", HorizontalAlignment.Left);
            ListViewGroup listViewGroup2 = new ListViewGroup("Offline Players", HorizontalAlignment.Left);
            ListViewItem listViewItem1 = new ListViewItem(new string[] { "0", "" }, -1);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            toolTip = new ToolTip(components);
            playerMenuStrip = new ContextMenuStrip(components);
            addToWhitelistToolStripMenuItem = new ToolStripMenuItem();
            teleportToPlayerToolStripMenuItem = new ToolStripMenuItem();
            toPlayerToolStripMenuItem = new ToolStripMenuItem();
            selectToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator3 = new ToolStripSeparator();
            openMapWebsiteToolStripMenuItem = new ToolStripMenuItem();
            godmodToolStripMenuItem = new ToolStripMenuItem();
            enableToolStripMenuItem = new ToolStripMenuItem();
            disableToolStripMenuItem = new ToolStripMenuItem();
            accessLevelToolStripMenuItem = new ToolStripMenuItem();
            noneToolStripMenuItem = new ToolStripMenuItem();
            overseerToolStripMenuItem = new ToolStripMenuItem();
            gMToolStripMenuItem = new ToolStripMenuItem();
            moderatorToolStripMenuItem = new ToolStripMenuItem();
            adminToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            spawnVehicleToolStripMenuItem = new ToolStripMenuItem();
            spawnItemToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            kickPlayerToolStripMenuItem = new ToolStripMenuItem();
            addExp = new ToolStripMenuItem();
            menuStrip = new MenuStrip();
            serverControlsToolStripMenuItem = new ToolStripMenuItem();
            commandConsoleToolStripMenuItem = new ToolStripMenuItem();
            serverOptionToolStripMenuItem = new ToolStripMenuItem();
            viewAndChangeToolStripMenuItem = new ToolStripMenuItem();
            reloadToolStripMenuItem = new ToolStripMenuItem();
            toolDatabaseToolStripMenuItem = new ToolStripMenuItem();
            createItemsPackToolStripMenuItem = new ToolStripMenuItem();
            importItemsPackToolStripMenuItem = new ToolStripMenuItem();
            importItemsPackToolStripMenuItem1 = new ToolStripMenuItem();
            mapMenuItem = new ToolStripMenuItem();
            helpToolStripMenuItem = new ToolStripMenuItem();
            informationToolStripMenuItem = new ToolStripMenuItem();
            accessLevelsToolStripMenuItem = new ToolStripMenuItem();
            reportIssueToolStripMenuItem = new ToolStripMenuItem();
            creditsToolStripMenuItem = new ToolStripMenuItem();
            playersView = new ListView();
            nameCol = new ColumnHeader();
            accessCol = new ColumnHeader();
            modeCol = new ColumnHeader();
            refreshBtn = new Button();
            cmbRefreshRate = new ComboBox();
            label1 = new Label();
            playerMenuStrip.SuspendLayout();
            menuStrip.SuspendLayout();
            SuspendLayout();
            // 
            // playerMenuStrip
            // 
            playerMenuStrip.Items.AddRange(new ToolStripItem[] { addToWhitelistToolStripMenuItem, teleportToPlayerToolStripMenuItem, godmodToolStripMenuItem, accessLevelToolStripMenuItem, toolStripSeparator1, spawnVehicleToolStripMenuItem, spawnItemToolStripMenuItem, toolStripSeparator2, kickPlayerToolStripMenuItem, addExp });
            playerMenuStrip.Name = "playerMenuStrip";
            playerMenuStrip.Size = new Size(160, 192);
            playerMenuStrip.Opening += playerMenuStrip_Opening;
            playerMenuStrip.Opened += playerMenuStrip_Opened;
            // 
            // addToWhitelistToolStripMenuItem
            // 
            addToWhitelistToolStripMenuItem.Name = "addToWhitelistToolStripMenuItem";
            addToWhitelistToolStripMenuItem.Size = new Size(159, 22);
            addToWhitelistToolStripMenuItem.Text = "Add to whitelist";
            addToWhitelistToolStripMenuItem.Click += addToWhitelistToolStripMenuItem_Click;
            // 
            // teleportToPlayerToolStripMenuItem
            // 
            teleportToPlayerToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { toPlayerToolStripMenuItem, selectToolStripMenuItem, toolStripSeparator3, openMapWebsiteToolStripMenuItem });
            teleportToPlayerToolStripMenuItem.Name = "teleportToPlayerToolStripMenuItem";
            teleportToPlayerToolStripMenuItem.Size = new Size(159, 22);
            teleportToPlayerToolStripMenuItem.Text = "Teleport Player";
            teleportToPlayerToolStripMenuItem.Click += teleportToPlayerToolStripMenuItem_Click;
            // 
            // toPlayerToolStripMenuItem
            // 
            toPlayerToolStripMenuItem.Name = "toPlayerToolStripMenuItem";
            toPlayerToolStripMenuItem.Size = new Size(173, 22);
            toPlayerToolStripMenuItem.Text = "To another player";
            toPlayerToolStripMenuItem.Click += toPlayerToolStripMenuItem_Click;
            // 
            // selectToolStripMenuItem
            // 
            selectToolStripMenuItem.Name = "selectToolStripMenuItem";
            selectToolStripMenuItem.Size = new Size(173, 22);
            selectToolStripMenuItem.Text = "To Coordinates";
            selectToolStripMenuItem.Click += selectToolStripMenuItem_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(170, 6);
            // 
            // openMapWebsiteToolStripMenuItem
            // 
            openMapWebsiteToolStripMenuItem.Name = "openMapWebsiteToolStripMenuItem";
            openMapWebsiteToolStripMenuItem.Size = new Size(173, 22);
            openMapWebsiteToolStripMenuItem.Text = "Open map website";
            openMapWebsiteToolStripMenuItem.Click += openMapWebsiteToolStripMenuItem_Click;
            // 
            // godmodToolStripMenuItem
            // 
            godmodToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { enableToolStripMenuItem, disableToolStripMenuItem });
            godmodToolStripMenuItem.Name = "godmodToolStripMenuItem";
            godmodToolStripMenuItem.Size = new Size(159, 22);
            godmodToolStripMenuItem.Text = "Godmod";
            // 
            // enableToolStripMenuItem
            // 
            enableToolStripMenuItem.Name = "enableToolStripMenuItem";
            enableToolStripMenuItem.Size = new Size(112, 22);
            enableToolStripMenuItem.Text = "Enable";
            enableToolStripMenuItem.Click += enableToolStripMenuItem_Click;
            // 
            // disableToolStripMenuItem
            // 
            disableToolStripMenuItem.Name = "disableToolStripMenuItem";
            disableToolStripMenuItem.Size = new Size(112, 22);
            disableToolStripMenuItem.Text = "Disable";
            disableToolStripMenuItem.Click += disableToolStripMenuItem_Click;
            // 
            // accessLevelToolStripMenuItem
            // 
            accessLevelToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { noneToolStripMenuItem, overseerToolStripMenuItem, gMToolStripMenuItem, moderatorToolStripMenuItem, adminToolStripMenuItem });
            accessLevelToolStripMenuItem.Name = "accessLevelToolStripMenuItem";
            accessLevelToolStripMenuItem.Size = new Size(159, 22);
            accessLevelToolStripMenuItem.Text = "Set Access Level";
            // 
            // noneToolStripMenuItem
            // 
            noneToolStripMenuItem.Name = "noneToolStripMenuItem";
            noneToolStripMenuItem.Size = new Size(130, 22);
            noneToolStripMenuItem.Text = "None";
            noneToolStripMenuItem.Click += noneToolStripMenuItem_Click;
            // 
            // overseerToolStripMenuItem
            // 
            overseerToolStripMenuItem.Name = "overseerToolStripMenuItem";
            overseerToolStripMenuItem.Size = new Size(130, 22);
            overseerToolStripMenuItem.Text = "Overseer";
            overseerToolStripMenuItem.Click += overseerToolStripMenuItem_Click;
            // 
            // gMToolStripMenuItem
            // 
            gMToolStripMenuItem.Name = "gMToolStripMenuItem";
            gMToolStripMenuItem.Size = new Size(130, 22);
            gMToolStripMenuItem.Text = "GM";
            gMToolStripMenuItem.Click += gMToolStripMenuItem_Click;
            // 
            // moderatorToolStripMenuItem
            // 
            moderatorToolStripMenuItem.Name = "moderatorToolStripMenuItem";
            moderatorToolStripMenuItem.Size = new Size(130, 22);
            moderatorToolStripMenuItem.Text = "Moderator";
            moderatorToolStripMenuItem.Click += moderatorToolStripMenuItem_Click;
            // 
            // adminToolStripMenuItem
            // 
            adminToolStripMenuItem.Name = "adminToolStripMenuItem";
            adminToolStripMenuItem.Size = new Size(130, 22);
            adminToolStripMenuItem.Text = "Admin";
            adminToolStripMenuItem.Click += adminToolStripMenuItem_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(156, 6);
            // 
            // spawnVehicleToolStripMenuItem
            // 
            spawnVehicleToolStripMenuItem.Name = "spawnVehicleToolStripMenuItem";
            spawnVehicleToolStripMenuItem.Size = new Size(159, 22);
            spawnVehicleToolStripMenuItem.Text = "Spawn Vehicle...";
            spawnVehicleToolStripMenuItem.Click += spawnVehicleToolStripMenuItem_Click;
            // 
            // spawnItemToolStripMenuItem
            // 
            spawnItemToolStripMenuItem.Name = "spawnItemToolStripMenuItem";
            spawnItemToolStripMenuItem.Size = new Size(159, 22);
            spawnItemToolStripMenuItem.Text = "Give Item...";
            spawnItemToolStripMenuItem.Click += spawnItemToolStripMenuItem_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(156, 6);
            // 
            // kickPlayerToolStripMenuItem
            // 
            kickPlayerToolStripMenuItem.Enabled = false;
            kickPlayerToolStripMenuItem.Name = "kickPlayerToolStripMenuItem";
            kickPlayerToolStripMenuItem.Size = new Size(159, 22);
            kickPlayerToolStripMenuItem.Text = "Kick Player";
            kickPlayerToolStripMenuItem.Click += kickPlayerToolStripMenuItem_Click;
            // 
            // addExp
            // 
            addExp.Name = "addExp";
            addExp.Size = new Size(159, 22);
            addExp.Text = "Add EXP";
            addExp.Click += addExp_Click;
            // 
            // menuStrip
            // 
            menuStrip.Items.AddRange(new ToolStripItem[] { serverControlsToolStripMenuItem, toolDatabaseToolStripMenuItem, mapMenuItem, helpToolStripMenuItem });
            menuStrip.Location = new Point(0, 0);
            menuStrip.Name = "menuStrip";
            menuStrip.Size = new Size(466, 24);
            menuStrip.TabIndex = 2;
            menuStrip.Text = "menuStrip1";
            // 
            // serverControlsToolStripMenuItem
            // 
            serverControlsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { commandConsoleToolStripMenuItem, serverOptionToolStripMenuItem });
            serverControlsToolStripMenuItem.Name = "serverControlsToolStripMenuItem";
            serverControlsToolStripMenuItem.Size = new Size(99, 20);
            serverControlsToolStripMenuItem.Text = "Server Controls";
            // 
            // commandConsoleToolStripMenuItem
            // 
            commandConsoleToolStripMenuItem.Name = "commandConsoleToolStripMenuItem";
            commandConsoleToolStripMenuItem.Size = new Size(177, 22);
            commandConsoleToolStripMenuItem.Text = "Command Console";
            commandConsoleToolStripMenuItem.Click += commandConsoleToolStripMenuItem_Click;
            // 
            // serverOptionToolStripMenuItem
            // 
            serverOptionToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { viewAndChangeToolStripMenuItem, reloadToolStripMenuItem });
            serverOptionToolStripMenuItem.Enabled = false;
            serverOptionToolStripMenuItem.Name = "serverOptionToolStripMenuItem";
            serverOptionToolStripMenuItem.Size = new Size(177, 22);
            serverOptionToolStripMenuItem.Text = "Server Option";
            // 
            // viewAndChangeToolStripMenuItem
            // 
            viewAndChangeToolStripMenuItem.Enabled = false;
            viewAndChangeToolStripMenuItem.Name = "viewAndChangeToolStripMenuItem";
            viewAndChangeToolStripMenuItem.Size = new Size(164, 22);
            viewAndChangeToolStripMenuItem.Text = "View and change";
            // 
            // reloadToolStripMenuItem
            // 
            reloadToolStripMenuItem.Enabled = false;
            reloadToolStripMenuItem.Name = "reloadToolStripMenuItem";
            reloadToolStripMenuItem.Size = new Size(164, 22);
            reloadToolStripMenuItem.Text = "Reload";
            // 
            // toolDatabaseToolStripMenuItem
            // 
            toolDatabaseToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { createItemsPackToolStripMenuItem, importItemsPackToolStripMenuItem, importItemsPackToolStripMenuItem1 });
            toolDatabaseToolStripMenuItem.Enabled = false;
            toolDatabaseToolStripMenuItem.Name = "toolDatabaseToolStripMenuItem";
            toolDatabaseToolStripMenuItem.Size = new Size(97, 20);
            toolDatabaseToolStripMenuItem.Text = "Database Tools";
            // 
            // createItemsPackToolStripMenuItem
            // 
            createItemsPackToolStripMenuItem.Name = "createItemsPackToolStripMenuItem";
            createItemsPackToolStripMenuItem.Size = new Size(170, 22);
            createItemsPackToolStripMenuItem.Text = "Create Items Pack";
            // 
            // importItemsPackToolStripMenuItem
            // 
            importItemsPackToolStripMenuItem.Name = "importItemsPackToolStripMenuItem";
            importItemsPackToolStripMenuItem.Size = new Size(170, 22);
            importItemsPackToolStripMenuItem.Text = "Open Items Pack";
            // 
            // importItemsPackToolStripMenuItem1
            // 
            importItemsPackToolStripMenuItem1.Name = "importItemsPackToolStripMenuItem1";
            importItemsPackToolStripMenuItem1.Size = new Size(170, 22);
            importItemsPackToolStripMenuItem1.Text = "Import Items Pack";
            // 
            // mapMenuItem
            // 
            mapMenuItem.Name = "mapMenuItem";
            mapMenuItem.Size = new Size(43, 20);
            mapMenuItem.Text = "Map";
            mapMenuItem.Click += mapMenuItem_Click;
            // 
            // helpToolStripMenuItem
            // 
            helpToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { informationToolStripMenuItem, reportIssueToolStripMenuItem, creditsToolStripMenuItem });
            helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            helpToolStripMenuItem.Size = new Size(44, 20);
            helpToolStripMenuItem.Text = "Help";
            // 
            // informationToolStripMenuItem
            // 
            informationToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { accessLevelsToolStripMenuItem });
            informationToolStripMenuItem.Name = "informationToolStripMenuItem";
            informationToolStripMenuItem.Size = new Size(138, 22);
            informationToolStripMenuItem.Text = "Information";
            // 
            // accessLevelsToolStripMenuItem
            // 
            accessLevelsToolStripMenuItem.Name = "accessLevelsToolStripMenuItem";
            accessLevelsToolStripMenuItem.Size = new Size(145, 22);
            accessLevelsToolStripMenuItem.Text = "Access Levels";
            accessLevelsToolStripMenuItem.Click += accessLevelsToolStripMenuItem_Click;
            // 
            // reportIssueToolStripMenuItem
            // 
            reportIssueToolStripMenuItem.Name = "reportIssueToolStripMenuItem";
            reportIssueToolStripMenuItem.Size = new Size(138, 22);
            reportIssueToolStripMenuItem.Text = "Report Issue";
            reportIssueToolStripMenuItem.Click += reportIssueToolStripMenuItem_Click;
            // 
            // creditsToolStripMenuItem
            // 
            creditsToolStripMenuItem.Name = "creditsToolStripMenuItem";
            creditsToolStripMenuItem.Size = new Size(138, 22);
            creditsToolStripMenuItem.Text = "Credits";
            creditsToolStripMenuItem.Click += creditsToolStripMenuItem_Click;
            // 
            // playersView
            // 
            playersView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            playersView.Columns.AddRange(new ColumnHeader[] { nameCol, accessCol, modeCol });
            playersView.ContextMenuStrip = playerMenuStrip;
            listViewGroup1.Header = "Online Player";
            listViewGroup1.Name = "OnlinePlayers";
            listViewGroup2.Header = "Offline Players";
            listViewGroup2.Name = "OfflinePlayers";
            playersView.Groups.AddRange(new ListViewGroup[] { listViewGroup1, listViewGroup2 });
            playersView.Items.AddRange(new ListViewItem[] { listViewItem1 });
            playersView.Location = new Point(12, 27);
            playersView.MultiSelect = false;
            playersView.Name = "playersView";
            playersView.Size = new Size(442, 391);
            playersView.TabIndex = 3;
            playersView.UseCompatibleStateImageBehavior = false;
            playersView.View = View.Details;
            // 
            // nameCol
            // 
            nameCol.Text = "Player Name";
            nameCol.Width = 200;
            // 
            // accessCol
            // 
            accessCol.Text = "Access Level";
            accessCol.Width = 120;
            // 
            // modeCol
            // 
            modeCol.Text = "God";
            // 
            // refreshBtn
            // 
            refreshBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            refreshBtn.BackgroundImage = Properties.Resources.refreshing;
            refreshBtn.BackgroundImageLayout = ImageLayout.Zoom;
            refreshBtn.Cursor = Cursors.Hand;
            refreshBtn.FlatAppearance.BorderSize = 0;
            refreshBtn.FlatStyle = FlatStyle.Flat;
            refreshBtn.Image = Properties.Resources.refreshing;
            refreshBtn.Location = new Point(393, 424);
            refreshBtn.Name = "refreshBtn";
            refreshBtn.Size = new Size(61, 50);
            refreshBtn.TabIndex = 4;
            refreshBtn.UseVisualStyleBackColor = true;
            refreshBtn.Click += refreshBtn_Click;
            // 
            // cmbRefreshRate
            // 
            cmbRefreshRate.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRefreshRate.FormattingEnabled = true;
            cmbRefreshRate.Items.AddRange(new object[] { "5 Sec", "30 sec", "60 sec" });
            cmbRefreshRate.Location = new Point(227, 451);
            cmbRefreshRate.Name = "cmbRefreshRate";
            cmbRefreshRate.Size = new Size(121, 23);
            cmbRefreshRate.TabIndex = 5;
            cmbRefreshRate.SelectedIndexChanged += cmbRefreshRate_SelectedIndexChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(229, 430);
            label1.Name = "label1";
            label1.Size = new Size(72, 15);
            label1.TabIndex = 6;
            label1.Text = "Refresh Rate";
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(466, 486);
            Controls.Add(label1);
            Controls.Add(cmbRefreshRate);
            Controls.Add(refreshBtn);
            Controls.Add(playersView);
            Controls.Add(menuStrip);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStrip;
            Name = "Main";
            Text = "Zomboid Admin Tool";
            FormClosing += Main_FormClosing;
            Load += Main_Load;
            Shown += Main_Shown;
            playerMenuStrip.ResumeLayout(false);
            menuStrip.ResumeLayout(false);
            menuStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private ToolTip toolTip;
        private ContextMenuStrip playerMenuStrip;
        private ToolStripMenuItem addToWhitelistToolStripMenuItem;
        private ToolStripMenuItem teleportToPlayerToolStripMenuItem;
        private ToolStripMenuItem godmodToolStripMenuItem;
        private ToolStripMenuItem accessLevelToolStripMenuItem;
        private ToolStripMenuItem noneToolStripMenuItem;
        private ToolStripMenuItem overseerToolStripMenuItem;
        private ToolStripMenuItem gMToolStripMenuItem;
        private ToolStripMenuItem moderatorToolStripMenuItem;
        private ToolStripMenuItem adminToolStripMenuItem;
        private MenuStrip menuStrip;
        private ListView playersView;
        private Button refreshBtn;
        private ToolStripMenuItem serverControlsToolStripMenuItem;
        private ToolStripMenuItem toolDatabaseToolStripMenuItem;
        private ToolStripMenuItem helpToolStripMenuItem;
        private ToolStripMenuItem informationToolStripMenuItem;
        private ToolStripMenuItem accessLevelsToolStripMenuItem;
        private ToolStripMenuItem reportIssueToolStripMenuItem;
        private ToolStripMenuItem creditsToolStripMenuItem;
        private ColumnHeader nameCol;
        private ColumnHeader accessCol;
        private ToolStripMenuItem enableToolStripMenuItem;
        private ToolStripMenuItem disableToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem kickPlayerToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem commandConsoleToolStripMenuItem;
        private ToolStripMenuItem serverOptionToolStripMenuItem;
        private ToolStripMenuItem viewAndChangeToolStripMenuItem;
        private ToolStripMenuItem reloadToolStripMenuItem;
        private ToolStripMenuItem spawnVehicleToolStripMenuItem;
        private ToolStripMenuItem spawnItemToolStripMenuItem;
        private ToolStripMenuItem toPlayerToolStripMenuItem;
        private ToolStripMenuItem selectToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripMenuItem openMapWebsiteToolStripMenuItem;
        private ToolStripMenuItem createItemsPackToolStripMenuItem;
        private ToolStripMenuItem importItemsPackToolStripMenuItem;
        private ToolStripMenuItem importItemsPackToolStripMenuItem1;
        private ToolStripMenuItem banPlayerToolStripMenuItem;
        private ToolStripMenuItem addExp;
        private ToolStripMenuItem mapMenuItem;
        private ColumnHeader modeCol;
        private ComboBox cmbRefreshRate;
        private Label label1;
    }
}