namespace FBPhotoDownloader
{
    partial class FBPhotoDownloader
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
            this.outputSelect = new System.Windows.Forms.Button();
            this.downloadPhotos = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.outputText = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // outputSelect
            // 
            this.outputSelect.Location = new System.Drawing.Point(12, 12);
            this.outputSelect.Name = "outputSelect";
            this.outputSelect.Size = new System.Drawing.Size(144, 23);
            this.outputSelect.TabIndex = 0;
            this.outputSelect.Text = "Choose Output Directory";
            this.outputSelect.UseVisualStyleBackColor = true;
            this.outputSelect.Click += new System.EventHandler(this.outputSelect_Click);
            // 
            // downloadPhotos
            // 
            this.downloadPhotos.Location = new System.Drawing.Point(12, 67);
            this.downloadPhotos.Name = "downloadPhotos";
            this.downloadPhotos.Size = new System.Drawing.Size(144, 23);
            this.downloadPhotos.TabIndex = 1;
            this.downloadPhotos.Text = "Download Photos";
            this.downloadPhotos.UseVisualStyleBackColor = true;
            this.downloadPhotos.Click += new System.EventHandler(this.downloadPhotos_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(12, 96);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(260, 125);
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(12, 227);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(260, 23);
            this.progressBar1.TabIndex = 3;
            // 
            // outputText
            // 
            this.outputText.Location = new System.Drawing.Point(12, 41);
            this.outputText.Name = "outputText";
            this.outputText.Size = new System.Drawing.Size(260, 20);
            this.outputText.TabIndex = 4;
            // 
            // FBPhotoDownloader
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.outputText);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.downloadPhotos);
            this.Controls.Add(this.outputSelect);
            this.Name = "FBPhotoDownloader";
            this.Text = "FB Photo Downloader";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button outputSelect;
        private System.Windows.Forms.Button downloadPhotos;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.TextBox outputText;
    }
}

