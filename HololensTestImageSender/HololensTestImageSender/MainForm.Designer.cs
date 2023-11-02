namespace HololensTestImageSender
{
    partial class MainForm
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
            this.FormClosing += MainForm_FormClosing;

            // Initialize components
            this.ipTextBox = new System.Windows.Forms.TextBox();
            this.portTextBox = new System.Windows.Forms.TextBox();
            this.browseButton = new System.Windows.Forms.Button();
            this.sendButton = new System.Windows.Forms.Button();
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.richTextBox = new System.Windows.Forms.RichTextBox();
            // 
            // ipTextBox
            // 
            this.ipTextBox.Location = new System.Drawing.Point(12, 12);
            this.ipTextBox.Name = "ipTextBox";
            this.ipTextBox.Size = new System.Drawing.Size(150, 23);
            this.ipTextBox.TabIndex = 0;
            this.ipTextBox.Text = "127.0.0.1";

            // 
            // portTextBox
            // 
            this.portTextBox.Location = new System.Drawing.Point(168, 12);
            this.portTextBox.Name = "portTextBox";
            this.portTextBox.Size = new System.Drawing.Size(60, 23);
            this.portTextBox.TabIndex = 1;
            this.portTextBox.Text = "8000";

            // 
            // browseButton
            // 
            this.browseButton.Location = new System.Drawing.Point(12, 41);
            this.browseButton.Name = "browseButton";
            this.browseButton.Size = new System.Drawing.Size(75, 23);
            this.browseButton.TabIndex = 2;
            this.browseButton.Text = "Browse";
            this.browseButton.UseVisualStyleBackColor = true;
            this.browseButton.Click += new System.EventHandler(this.browseButton_Click);

            // 
            // sendButton
            // 
            this.sendButton.Location = new System.Drawing.Point(93, 41);
            this.sendButton.Name = "sendButton";
            this.sendButton.Size = new System.Drawing.Size(75, 23);
            this.sendButton.TabIndex = 3;
            this.sendButton.Text = "Send";
            this.sendButton.UseVisualStyleBackColor = true;
            this.sendButton.Click += new System.EventHandler(this.sendButton_Click);



            // 
            // clearTextureButton
            // 
            // clearTextureButton initialization
            this.clearTextureButton = new System.Windows.Forms.Button();
            this.clearTextureButton.Location = new System.Drawing.Point(174, 41); // position next to the sendButton
            this.clearTextureButton.Name = "clearTextureButton";
            this.clearTextureButton.Size = new System.Drawing.Size(100, 23);
            this.clearTextureButton.TabIndex = 5;
            this.clearTextureButton.Text = "Clear Texture";
            this.clearTextureButton.UseVisualStyleBackColor = true;
            this.clearTextureButton.Click += new System.EventHandler(this.clearTextureButton_Click);

            // 
            // pictureBox
            // 
            this.pictureBox.Location = new System.Drawing.Point(12, 70);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(200, 150);
            this.pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox.TabIndex = 4;
            this.pictureBox.TabStop = false;

            //
            // RichTextBox Configuration
            // 
            this.richTextBox.Location = new System.Drawing.Point(12, 230); // You can adjust the location as needed
            this.richTextBox.Name = "richTextBox";
            this.richTextBox.Size = new System.Drawing.Size(760, 200); // Adjust the size as per your design requirements
            this.richTextBox.ReadOnly = true; // This ensures users can't modify its content directly

            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(this.ipTextBox);
            Controls.Add(this.portTextBox);
            Controls.Add(this.browseButton);
            Controls.Add(this.sendButton);
            Controls.Add(this.pictureBox);
            Controls.Add(this.richTextBox);
            Controls.Add(this.clearTextureButton);
            Name = "MainForm";
            Text = "HoloLens Image Sender";
            ResumeLayout(false);
            PerformLayout();
        }

        private System.Windows.Forms.TextBox ipTextBox;
        private System.Windows.Forms.TextBox portTextBox;
        private System.Windows.Forms.Button browseButton;
        private System.Windows.Forms.Button sendButton;
        private System.Windows.Forms.Button clearTextureButton; 
        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.RichTextBox richTextBox;

        #endregion
    }
}