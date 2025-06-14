namespace ZomboidRCON.HelperForms
{
    partial class ItemSpawnMenu
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            itemIDTxt = new TextBox();
            countNumeric = new NumericUpDown();
            spawnBtn = new Button();
            itemIDLabel = new Label();
            countLabel = new Label();
            comboBox1 = new ComboBox();
            label1 = new Label();
            ((System.ComponentModel.ISupportInitialize)countNumeric).BeginInit();
            SuspendLayout();
            // 
            // itemIDTxt
            // 
            itemIDTxt.Location = new Point(12, 74);
            itemIDTxt.Name = "itemIDTxt";
            itemIDTxt.Size = new Size(430, 23);
            itemIDTxt.TabIndex = 0;
            itemIDTxt.TextChanged += itemIDTxt_TextChanged;
            // 
            // countNumeric
            // 
            countNumeric.Location = new Point(12, 118);
            countNumeric.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            countNumeric.Name = "countNumeric";
            countNumeric.Size = new Size(430, 23);
            countNumeric.TabIndex = 1;
            countNumeric.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // spawnBtn
            // 
            spawnBtn.Location = new Point(12, 149);
            spawnBtn.Name = "spawnBtn";
            spawnBtn.Size = new Size(430, 23);
            spawnBtn.TabIndex = 2;
            spawnBtn.Text = "Give Item";
            spawnBtn.UseVisualStyleBackColor = true;
            spawnBtn.Click += spawnBtn_Click;
            // 
            // itemIDLabel
            // 
            itemIDLabel.AutoSize = true;
            itemIDLabel.Location = new Point(12, 56);
            itemIDLabel.Name = "itemIDLabel";
            itemIDLabel.Size = new Size(48, 15);
            itemIDLabel.TabIndex = 3;
            itemIDLabel.Text = "Item ID:";
            // 
            // countLabel
            // 
            countLabel.AutoSize = true;
            countLabel.Location = new Point(12, 100);
            countLabel.Name = "countLabel";
            countLabel.Size = new Size(43, 15);
            countLabel.TabIndex = 4;
            countLabel.Text = "Count:";
            // 
            // comboBox1
            // 
            comboBox1.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            comboBox1.AutoCompleteSource = AutoCompleteSource.ListItems;
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new Point(12, 30);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(430, 23);
            comboBox1.TabIndex = 5;
            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 12);
            label1.Name = "label1";
            label1.Size = new Size(72, 15);
            label1.TabIndex = 6;
            label1.Text = "Search Item:";
            // 
            // ItemSpawnMenu
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(454, 185);
            Controls.Add(label1);
            Controls.Add(comboBox1);
            Controls.Add(countLabel);
            Controls.Add(itemIDLabel);
            Controls.Add(spawnBtn);
            Controls.Add(countNumeric);
            Controls.Add(itemIDTxt);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ItemSpawnMenu";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Give Item";
            Load += ItemSpawnMenu_Load;
            ((System.ComponentModel.ISupportInitialize)countNumeric).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox itemIDTxt;
        private NumericUpDown countNumeric;
        private Button spawnBtn;
        private Label itemIDLabel;
        private Label countLabel;
        private ComboBox comboBox1;
        private Label label1;
    }
} 