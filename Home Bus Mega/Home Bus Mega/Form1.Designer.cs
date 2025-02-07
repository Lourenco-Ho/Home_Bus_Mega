
namespace Home_Bus_Mega
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.label_headtitle = new System.Windows.Forms.Label();
            this.header_bg_label = new System.Windows.Forms.PictureBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label_headtitle_2 = new System.Windows.Forms.Label();
            this.button_to_mode_1 = new System.Windows.Forms.Button();
            this.button_to_mode_2 = new System.Windows.Forms.Button();
            this.flowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.panel_mode_1A = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.header_bg_label)).BeginInit();
            this.header_bg_label.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panel_mode_1A.SuspendLayout();
            this.SuspendLayout();
            // 
            // label_headtitle
            // 
            this.label_headtitle.AutoSize = true;
            this.label_headtitle.BackColor = App_Colors.Green;
            this.label_headtitle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.label_headtitle.Font = new System.Drawing.Font("微軟正黑體", 41.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label_headtitle.ForeColor = App_Colors.Text_white;
            this.label_headtitle.Location = new System.Drawing.Point(20, 9);
            this.label_headtitle.Margin = new System.Windows.Forms.Padding(0);
            this.label_headtitle.Name = "label_headtitle";
            this.label_headtitle.Size = new System.Drawing.Size(305, 70);
            this.label_headtitle.TabIndex = 0;
            this.label_headtitle.Text = "選擇上車站";
            // 
            // header_bg_label
            // 
            this.header_bg_label.BackColor = App_Colors.Green;
            this.header_bg_label.Controls.Add(this.label_headtitle);
            this.header_bg_label.Controls.Add(this.pictureBox1);
            this.header_bg_label.Controls.Add(this.label_headtitle_2);
            this.header_bg_label.Location = new System.Drawing.Point(0, 0);
            this.header_bg_label.Margin = new System.Windows.Forms.Padding(0);
            this.header_bg_label.Name = "header_bg_label";
            this.header_bg_label.Size = new System.Drawing.Size(640, 79);
            this.header_bg_label.TabIndex = 1;
            this.header_bg_label.TabStop = false;
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(514, 4);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(25, 25);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            // 
            // label_headtitle_2
            // 
            this.label_headtitle_2.AutoSize = true;
            this.label_headtitle_2.BackColor = App_Colors.Green;
            this.label_headtitle_2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.label_headtitle_2.Font = new System.Drawing.Font("微軟正黑體", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label_headtitle_2.ForeColor = System.Drawing.Color.White;
            this.label_headtitle_2.Location = new System.Drawing.Point(400, 0);
            this.label_headtitle_2.Margin = new System.Windows.Forms.Padding(0);
            this.label_headtitle_2.Name = "label_headtitle_2";
            this.label_headtitle_2.Size = new System.Drawing.Size(129, 72);
            this.label_headtitle_2.TabIndex = 2;
            this.label_headtitle_2.Text = "在左方地圖\n或\n下方站名 選擇";
            this.label_headtitle_2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // button_to_mode_1
            // 
            this.button_to_mode_1.BackColor = App_Colors.Mode_button_red;
            this.button_to_mode_1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button_to_mode_1.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.button_to_mode_1.FlatAppearance.BorderSize = 2;
            this.button_to_mode_1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_to_mode_1.Font = new System.Drawing.Font("Microsoft JhengHei UI", 36F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.button_to_mode_1.ForeColor = App_Colors.Text_white;
            this.button_to_mode_1.Location = new System.Drawing.Point(640, 650);
            this.button_to_mode_1.Name = "button_to_mode_1";
            this.button_to_mode_1.Size = new System.Drawing.Size(320, 70);
            this.button_to_mode_1.TabIndex = 2;
            this.button_to_mode_1.Text = "選擇上車站";
            this.button_to_mode_1.UseVisualStyleBackColor = false;
            this.button_to_mode_1.Click += new System.EventHandler(this.button_to_mode_1_Click);
            // 
            // button_to_mode_2
            // 
            this.button_to_mode_2.BackColor = App_Colors.Mode_button_blue;
            this.button_to_mode_2.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button_to_mode_2.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.button_to_mode_2.FlatAppearance.BorderSize = 2;
            this.button_to_mode_2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_to_mode_2.Font = new System.Drawing.Font("Microsoft JhengHei UI", 36F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.button_to_mode_2.ForeColor = App_Colors.Text_white;
            this.button_to_mode_2.Location = new System.Drawing.Point(960, 650);
            this.button_to_mode_2.Name = "button_to_mode_2";
            this.button_to_mode_2.Size = new System.Drawing.Size(320, 70);
            this.button_to_mode_2.TabIndex = 3;
            this.button_to_mode_2.Text = "選擇目的地";
            this.button_to_mode_2.UseVisualStyleBackColor = false;
            this.button_to_mode_2.Click += new System.EventHandler(this.button_to_mode_2_Click);
            // 
            // flowLayoutPanel
            // 
            this.flowLayoutPanel.AutoScroll = true;
            this.flowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel.Location = new System.Drawing.Point(0, 79);
            this.flowLayoutPanel.Name = "flowLayoutPanel";
            this.flowLayoutPanel.Size = new System.Drawing.Size(640, 571);
            this.flowLayoutPanel.TabIndex = 4;
            this.flowLayoutPanel.WrapContents = false;
            // 
            // panel_mode_1A
            // 
            this.panel_mode_1A.Controls.Add(this.header_bg_label);
            this.panel_mode_1A.Controls.Add(this.flowLayoutPanel);
            this.panel_mode_1A.Location = new System.Drawing.Point(640, 0);
            this.panel_mode_1A.Name = "panel_mode_1A";
            this.panel_mode_1A.Size = new System.Drawing.Size(640, 650);
            this.panel_mode_1A.TabIndex = 6;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.ClientSize = new System.Drawing.Size(1280, 720);
            this.ControlBox = false;
            this.Controls.Add(this.panel_mode_1A);
            this.Controls.Add(this.button_to_mode_2);
            this.Controls.Add(this.button_to_mode_1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "MainForm";
            this.Text = "Form1";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            ((System.ComponentModel.ISupportInitialize)(this.header_bg_label)).EndInit();
            this.header_bg_label.ResumeLayout(false);
            this.header_bg_label.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panel_mode_1A.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label_headtitle;
        private System.Windows.Forms.PictureBox header_bg_label;
        private System.Windows.Forms.Label label_headtitle_2;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button button_to_mode_1;
        private System.Windows.Forms.Button button_to_mode_2;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel;
        private System.Windows.Forms.Panel panel_mode_1A;
    }
}

