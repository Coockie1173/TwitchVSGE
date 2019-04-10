namespace TwitchChatReader
{
    partial class Form1
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
            this.components = new System.ComponentModel.Container();
            this.CommandList = new System.Windows.Forms.TextBox();
            this.CheckChat = new System.Windows.Forms.Timer(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // CommandList
            // 
            this.CommandList.Location = new System.Drawing.Point(60, 9);
            this.CommandList.Multiline = true;
            this.CommandList.Name = "CommandList";
            this.CommandList.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.CommandList.Size = new System.Drawing.Size(807, 304);
            this.CommandList.TabIndex = 0;
            // 
            // CheckChat
            // 
            this.CheckChat.Enabled = true;
            this.CheckChat.Interval = 400;
            this.CheckChat.Tick += new System.EventHandler(this.CheckChat_Tick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(42, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Debug:";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(879, 325);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.CommandList);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox CommandList;
        private System.Windows.Forms.Timer CheckChat;
        private System.Windows.Forms.Label label1;
    }
}

