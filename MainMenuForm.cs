using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace CitySkylines0._5alphabeta
{
    public partial class MainMenuForm : Form
    {
        private Button btnPlay;
        private Button btnOptions;
        private Button btnQuit;
        private PictureBox backgroundBox;

        public MainMenuForm()
        {
            InitializeComponent();

            // --- Form Settings ---
            this.Text = "PROJECT CITY - MAIN MENU";
            this.ClientSize = new Size(3, 3);
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            // --- Load background ---
            string projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."));
            string backgroundPath = Path.Combine(projectRoot, "gameAssets", "gameArt", "projectCityMain.png");

            backgroundBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = Image.FromFile(backgroundPath)
            };
            this.Controls.Add(backgroundBox);

            // --- Buttons ---
            btnPlay = CreateMenuButton("PLAY");
            btnOptions = CreateMenuButton("OPTIONS");
            btnQuit = CreateMenuButton("QUIT");

            btnPlay.Click += BtnPlay_Click;
            btnOptions.Click += BtnOptions_Click;
            btnQuit.Click += BtnQuit_Click;

            // add buttons AFTER background
            this.Controls.Add(btnPlay);
            this.Controls.Add(btnOptions);
            this.Controls.Add(btnQuit);

            backgroundBox.SendToBack();

            // handle resizing to keep buttons centered
            this.Resize += MainMenuForm_Resize;
            CenterButtons();
        }

        private Button CreateMenuButton(string text)
        {
            Button button = new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Size = new Size(200, 50),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderSize = 2;
            button.FlatAppearance.BorderColor = Color.LightBlue;
            return button;
        }

        private void MainMenuForm_Resize(object? sender, EventArgs e)
        {
            CenterButtons();
        }

        private void CenterButtons()
        {
            int centerX = (this.ClientSize.Width - btnPlay.Width) / 2;
            int startY = (this.ClientSize.Height / 2) + 100;

            btnPlay.Location = new Point(centerX, startY);
            btnOptions.Location = new Point(centerX, startY + 70);
            btnQuit.Location = new Point(centerX, startY + 140);
        }

        private void BtnPlay_Click(object? sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK; // signals "Play" to Program.cs
            this.Close();
        }

        private void BtnOptions_Click(object? sender, EventArgs e)
        {
            Form optionsForm = new Form
            {
                Text = "OPTIONS (Coming Soon)",
                ClientSize = new Size(600, 400),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.DimGray
            };

            Label comingSoon = new Label
            {
                Text = "Options Menu Coming Soon!",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(100, 150)
            };

            optionsForm.Controls.Add(comingSoon);
            optionsForm.ShowDialog();
        }

        private void BtnQuit_Click(object? sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
