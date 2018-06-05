namespace TradeBot.GUIForms
{
    partial class MainWindowForm
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
            this.startButton = new System.Windows.Forms.Button();
            this.botConfigButton = new System.Windows.Forms.Button();
            this.bitStampConfigButton = new System.Windows.Forms.Button();
            this.logsTextBox = new System.Windows.Forms.TextBox();
            this.inputTextBox = new System.Windows.Forms.TextBox();
            this.sendTextButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // startButton
            // 
            this.startButton.Location = new System.Drawing.Point(713, 12);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(75, 23);
            this.startButton.TabIndex = 0;
            this.startButton.Text = "Start";
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.Click += new System.EventHandler(this.startButton_Click);
            // 
            // botConfigButton
            // 
            this.botConfigButton.Location = new System.Drawing.Point(713, 68);
            this.botConfigButton.Name = "botConfigButton";
            this.botConfigButton.Size = new System.Drawing.Size(75, 40);
            this.botConfigButton.TabIndex = 1;
            this.botConfigButton.Text = "Bot Config";
            this.botConfigButton.UseVisualStyleBackColor = true;
            this.botConfigButton.Click += new System.EventHandler(this.botConfigButton_Click);
            // 
            // bitStampConfigButton
            // 
            this.bitStampConfigButton.Location = new System.Drawing.Point(713, 114);
            this.bitStampConfigButton.Name = "bitStampConfigButton";
            this.bitStampConfigButton.Size = new System.Drawing.Size(75, 38);
            this.bitStampConfigButton.TabIndex = 2;
            this.bitStampConfigButton.Text = "BitStamp Config";
            this.bitStampConfigButton.UseVisualStyleBackColor = true;
            this.bitStampConfigButton.Click += new System.EventHandler(this.bitStampConfigButton_Click);
            // 
            // logsTextBox
            // 
            this.logsTextBox.Location = new System.Drawing.Point(12, 12);
            this.logsTextBox.Multiline = true;
            this.logsTextBox.Name = "logsTextBox";
            this.logsTextBox.ReadOnly = true;
            this.logsTextBox.Size = new System.Drawing.Size(695, 410);
            this.logsTextBox.TabIndex = 3;
            // 
            // inputTextBox
            // 
            this.inputTextBox.Location = new System.Drawing.Point(12, 436);
            this.inputTextBox.Name = "inputTextBox";
            this.inputTextBox.Size = new System.Drawing.Size(695, 20);
            this.inputTextBox.TabIndex = 4;
            // 
            // sendTextButton
            // 
            this.sendTextButton.Location = new System.Drawing.Point(713, 436);
            this.sendTextButton.Name = "sendTextButton";
            this.sendTextButton.Size = new System.Drawing.Size(75, 23);
            this.sendTextButton.TabIndex = 5;
            this.sendTextButton.Text = "Send";
            this.sendTextButton.UseVisualStyleBackColor = true;
            this.sendTextButton.Click += new System.EventHandler(this.sendTextButton_Click);
            // 
            // MainWindowForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 468);
            this.Controls.Add(this.sendTextButton);
            this.Controls.Add(this.inputTextBox);
            this.Controls.Add(this.logsTextBox);
            this.Controls.Add(this.bitStampConfigButton);
            this.Controls.Add(this.botConfigButton);
            this.Controls.Add(this.startButton);
            this.Name = "MainWindowForm";
            this.Text = "MainWindowForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.Button botConfigButton;
        private System.Windows.Forms.Button bitStampConfigButton;
        private System.Windows.Forms.TextBox logsTextBox;
        public System.Windows.Forms.Button sendTextButton;
        public System.Windows.Forms.TextBox inputTextBox;
    }
}