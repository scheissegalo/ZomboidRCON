namespace ZomboidRCON.HelperForms
{
    partial class AddExp
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
            perkComboBox = new ComboBox();
            numExp = new NumericUpDown();
            label1 = new Label();
            label2 = new Label();
            btnGiveExp = new Button();
            ((System.ComponentModel.ISupportInitialize)numExp).BeginInit();
            SuspendLayout();
            // 
            // perkComboBox
            // 
            perkComboBox.FormattingEnabled = true;
            perkComboBox.Location = new Point(12, 36);
            perkComboBox.Name = "perkComboBox";
            perkComboBox.Size = new Size(205, 23);
            perkComboBox.TabIndex = 0;
            // 
            // numExp
            // 
            numExp.Location = new Point(223, 37);
            numExp.Name = "numExp";
            numExp.Size = new Size(85, 23);
            numExp.TabIndex = 2;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 18);
            label1.Name = "label1";
            label1.Size = new Size(30, 15);
            label1.TabIndex = 3;
            label1.Text = "Perk";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(223, 19);
            label2.Name = "label2";
            label2.Size = new Size(27, 15);
            label2.TabIndex = 4;
            label2.Text = "EXP";
            // 
            // btnGiveExp
            // 
            btnGiveExp.Location = new Point(95, 83);
            btnGiveExp.Name = "btnGiveExp";
            btnGiveExp.Size = new Size(122, 23);
            btnGiveExp.TabIndex = 5;
            btnGiveExp.Text = "Give EXP";
            btnGiveExp.UseVisualStyleBackColor = true;
            btnGiveExp.Click += btnGiveExp_Click;
            // 
            // AddExp
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(320, 118);
            Controls.Add(btnGiveExp);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(numExp);
            Controls.Add(perkComboBox);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "AddExp";
            Text = "AddExp";
            ((System.ComponentModel.ISupportInitialize)numExp).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ComboBox perkComboBox;
        private NumericUpDown numExp;
        private Label label1;
        private Label label2;
        private Button btnGiveExp;
    }
}