namespace Chess
{
    partial class Chess
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Chess));
            menup = new Panel();
            menul = new Label();
            menuleftp = new Panel();
            playasb = new Button();
            currentplayerl = new Label();
            mainmenub = new Button();
            saveb = new Button();
            backb = new Button();
            AIdifficultyl = new Label();
            settingl = new Label();
            AIdifficultybar = new TrackBar();
            vsAIb = new Button();
            loadb = new Button();
            playb = new Button();
            exitb = new Button();
            menup.SuspendLayout();
            menuleftp.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)AIdifficultybar).BeginInit();
            SuspendLayout();
            // 
            // menup
            // 
            menup.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            menup.BackColor = Color.FromArgb(134, 171, 97);
            menup.Controls.Add(menul);
            menup.Location = new Point(0, 0);
            menup.Margin = new Padding(2);
            menup.Name = "menup";
            menup.Size = new Size(1280, 137);
            menup.TabIndex = 0;
            // 
            // menul
            // 
            menul.Anchor = AnchorStyles.None;
            menul.AutoSize = true;
            menul.Font = new Font("Calibri", 64F, FontStyle.Bold, GraphicsUnit.Point);
            menul.Location = new Point(558, 19);
            menul.Margin = new Padding(2, 0, 2, 0);
            menul.Name = "menul";
            menul.Size = new Size(248, 105);
            menul.TabIndex = 0;
            menul.Text = "Chess";
            // 
            // menuleftp
            // 
            menuleftp.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            menuleftp.BackColor = Color.Silver;
            menuleftp.Controls.Add(playasb);
            menuleftp.Controls.Add(currentplayerl);
            menuleftp.Controls.Add(mainmenub);
            menuleftp.Controls.Add(saveb);
            menuleftp.Controls.Add(backb);
            menuleftp.Controls.Add(AIdifficultyl);
            menuleftp.Controls.Add(settingl);
            menuleftp.Controls.Add(AIdifficultybar);
            menuleftp.Controls.Add(vsAIb);
            menuleftp.Controls.Add(loadb);
            menuleftp.Location = new Point(0, 137);
            menuleftp.Margin = new Padding(2);
            menuleftp.Name = "menuleftp";
            menuleftp.Size = new Size(210, 720);
            menuleftp.TabIndex = 1;
            // 
            // playasb
            // 
            playasb.Anchor = AnchorStyles.None;
            playasb.Font = new Font("Calibri", 20F, FontStyle.Regular, GraphicsUnit.Point);
            playasb.Location = new Point(11, 239);
            playasb.Margin = new Padding(2);
            playasb.Name = "playasb";
            playasb.Size = new Size(192, 80);
            playasb.TabIndex = 10;
            playasb.Text = "Play as white";
            playasb.UseVisualStyleBackColor = true;
            playasb.Visible = false;
            playasb.Click += playasb_Click;
            // 
            // currentplayerl
            // 
            currentplayerl.Anchor = AnchorStyles.None;
            currentplayerl.AutoSize = true;
            currentplayerl.Font = new Font("Calibri", 26.25F, FontStyle.Bold, GraphicsUnit.Point);
            currentplayerl.Location = new Point(7, 185);
            currentplayerl.Name = "currentplayerl";
            currentplayerl.Size = new Size(200, 42);
            currentplayerl.TabIndex = 9;
            currentplayerl.Text = "White's turn";
            currentplayerl.TextAlign = ContentAlignment.MiddleCenter;
            currentplayerl.Visible = false;
            // 
            // mainmenub
            // 
            mainmenub.Anchor = AnchorStyles.None;
            mainmenub.Font = new Font("Calibri", 24F, FontStyle.Regular, GraphicsUnit.Point);
            mainmenub.Location = new Point(11, 491);
            mainmenub.Margin = new Padding(2);
            mainmenub.Name = "mainmenub";
            mainmenub.Size = new Size(192, 80);
            mainmenub.TabIndex = 8;
            mainmenub.Text = "Main menu";
            mainmenub.UseVisualStyleBackColor = true;
            mainmenub.Visible = false;
            mainmenub.Click += mainmenub_Click;
            // 
            // saveb
            // 
            saveb.Anchor = AnchorStyles.None;
            saveb.Font = new Font("Calibri", 24F, FontStyle.Regular, GraphicsUnit.Point);
            saveb.Location = new Point(11, 239);
            saveb.Margin = new Padding(2);
            saveb.Name = "saveb";
            saveb.Size = new Size(192, 80);
            saveb.TabIndex = 7;
            saveb.Text = "Save game";
            saveb.UseVisualStyleBackColor = true;
            saveb.Visible = false;
            saveb.Click += saveb_Click;
            // 
            // backb
            // 
            backb.Anchor = AnchorStyles.None;
            backb.Enabled = false;
            backb.Font = new Font("Calibri", 24F, FontStyle.Regular, GraphicsUnit.Point);
            backb.Location = new Point(11, 407);
            backb.Margin = new Padding(2);
            backb.Name = "backb";
            backb.Size = new Size(192, 80);
            backb.TabIndex = 6;
            backb.Text = "Back";
            backb.UseVisualStyleBackColor = true;
            backb.Visible = false;
            backb.Click += backb_Click;
            // 
            // AIdifficultyl
            // 
            AIdifficultyl.Anchor = AnchorStyles.None;
            AIdifficultyl.AutoSize = true;
            AIdifficultyl.Font = new Font("Calibri", 21F, FontStyle.Bold, GraphicsUnit.Point);
            AIdifficultyl.Location = new Point(18, 170);
            AIdifficultyl.Name = "AIdifficultyl";
            AIdifficultyl.Size = new Size(175, 35);
            AIdifficultyl.TabIndex = 5;
            AIdifficultyl.Text = "AI difficulty: 1";
            // 
            // settingl
            // 
            settingl.Anchor = AnchorStyles.None;
            settingl.AutoSize = true;
            settingl.Font = new Font("Calibri", 42F, FontStyle.Bold, GraphicsUnit.Point);
            settingl.Location = new Point(18, 2);
            settingl.Margin = new Padding(2, 0, 2, 0);
            settingl.Name = "settingl";
            settingl.Size = new Size(166, 68);
            settingl.TabIndex = 4;
            settingl.Text = "Menu";
            // 
            // AIdifficultybar
            // 
            AIdifficultybar.Anchor = AnchorStyles.None;
            AIdifficultybar.BackColor = Color.Silver;
            AIdifficultybar.Location = new Point(11, 207);
            AIdifficultybar.Margin = new Padding(2);
            AIdifficultybar.Maximum = 5;
            AIdifficultybar.Minimum = 1;
            AIdifficultybar.Name = "AIdifficultybar";
            AIdifficultybar.Size = new Size(192, 45);
            AIdifficultybar.TabIndex = 3;
            AIdifficultybar.Value = 1;
            AIdifficultybar.Scroll += AIdifficultybar_Scroll;
            // 
            // vsAIb
            // 
            vsAIb.Anchor = AnchorStyles.None;
            vsAIb.Font = new Font("Calibri", 24F, FontStyle.Regular, GraphicsUnit.Point);
            vsAIb.Location = new Point(11, 82);
            vsAIb.Margin = new Padding(2);
            vsAIb.Name = "vsAIb";
            vsAIb.Size = new Size(192, 86);
            vsAIb.TabIndex = 2;
            vsAIb.Text = "Player vs player";
            vsAIb.UseVisualStyleBackColor = true;
            vsAIb.Click += vsAIb_Click;
            // 
            // loadb
            // 
            loadb.Anchor = AnchorStyles.None;
            loadb.Font = new Font("Calibri", 24F, FontStyle.Regular, GraphicsUnit.Point);
            loadb.Location = new Point(11, 323);
            loadb.Margin = new Padding(2);
            loadb.Name = "loadb";
            loadb.Size = new Size(192, 80);
            loadb.TabIndex = 1;
            loadb.Text = "Load game";
            loadb.UseVisualStyleBackColor = true;
            loadb.Click += loadb_Click;
            // 
            // playb
            // 
            playb.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            playb.AutoSize = true;
            playb.BackColor = Color.Silver;
            playb.Font = new Font("Calibri", 42F, FontStyle.Bold, GraphicsUnit.Point);
            playb.Location = new Point(214, 228);
            playb.Margin = new Padding(2);
            playb.Name = "playb";
            playb.Size = new Size(1058, 174);
            playb.TabIndex = 2;
            playb.Text = "Play game";
            playb.UseVisualStyleBackColor = false;
            playb.Click += playb_Click;
            // 
            // exitb
            // 
            exitb.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            exitb.AutoSize = true;
            exitb.BackColor = Color.Silver;
            exitb.Font = new Font("Calibri", 42F, FontStyle.Bold, GraphicsUnit.Point);
            exitb.Location = new Point(214, 480);
            exitb.Margin = new Padding(2);
            exitb.Name = "exitb";
            exitb.Size = new Size(1058, 174);
            exitb.TabIndex = 3;
            exitb.Text = "Exit game";
            exitb.UseVisualStyleBackColor = false;
            exitb.Click += exitb_Click;
            // 
            // Chess
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(224, 224, 224);
            ClientSize = new Size(1280, 720);
            Controls.Add(exitb);
            Controls.Add(playb);
            Controls.Add(menuleftp);
            Controls.Add(menup);
            FormBorderStyle = FormBorderStyle.None;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(2);
            Name = "Chess";
            Text = "Chess";
            WindowState = FormWindowState.Maximized;
            menup.ResumeLayout(false);
            menup.PerformLayout();
            menuleftp.ResumeLayout(false);
            menuleftp.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)AIdifficultybar).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel menup;
        private Label menul;
        private Panel menuleftp;
        private Button loadb;
        private Button vsAIb;
        private Button playb;
        private Button exitb;
        private Label settingl;
        private TrackBar AIdifficultybar;
        private Label AIdifficultyl;
        private Button saveb;
        private Button backb;
        private Button mainmenub;
        private Label currentplayerl;
        private Button playasb;
    }
}