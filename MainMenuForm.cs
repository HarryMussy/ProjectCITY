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
            using (Form playMenu = new Form())
            {
                playMenu.Text = "PLAY";
                playMenu.ClientSize = new Size(400, 300);
                playMenu.StartPosition = FormStartPosition.CenterParent;
                playMenu.BackColor = Color.FromArgb(40, 40, 40);
                playMenu.ForeColor = Color.White;
                playMenu.Font = new Font("Segoe UI", 11, FontStyle.Bold);

                Button btnNew = new Button { Text = "NEW GAME", Size = new Size(200, 50), Location = new Point(100, 60) };
                Button btnLoad = new Button { Text = "LOAD GAME", Size = new Size(200, 50), Location = new Point(100, 130) };

                btnNew.Click += (s, e2) =>
                {
                    playMenu.DialogResult = DialogResult.OK;
                    playMenu.Close();
                    this.Close();
                    Form1 form = new Form1();
                    form.Show();
                };

                btnLoad.Click += (s, e2) =>
                {
                    var save = SaveManager.LoadGameFromFile();
                    if (save != null)
                    {
                        playMenu.DialogResult = DialogResult.OK;
                        playMenu.Close();
                        this.Close();
                        Form1 form = new Form1(save);
                        form.Show();
                    }
                    else
                    {
                        // load was cancelled or failed - keep the play menu open (optional)
                        Console.WriteLine("Load cancelled or failed.");
                    }
                };

                playMenu.Controls.Add(btnNew);
                playMenu.Controls.Add(btnLoad);

                playMenu.ShowDialog();
            }
        }



        private void BtnOptions_Click(object? sender, EventArgs e)
        {
            OptionsForm optionsForm = new OptionsForm(false);
            optionsForm.Show();
        }

        private void BtnQuit_Click(object? sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void MainMenuForm_Load(object sender, EventArgs e)
        {

        }
    }
}
