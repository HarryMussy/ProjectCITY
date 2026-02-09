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
        private AudioManager audioManager;

        public MainMenuForm()
        {
            InitializeComponent();

            // --- Form Settings ---
            this.Text = "PROJECT CITY - MAIN MENU";
            this.ClientSize = new Size(3, 3);
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            audioManager = new AudioManager();

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
                    playMenu.Close();

                    // Show difficulty selector
                    using (DifficultySelectForm diffForm = new DifficultySelectForm())
                    {
                        if (diffForm.ShowDialog() == DialogResult.OK)
                        {
                            // Close main menu
                            this.Close();

                            // Start the game with selected difficulty
                            Form1 form = new Form1(diffForm.SelectedDifficulty, audioManager);
                            form.Show();
                        }
                    }
                };

                btnLoad.Click += (s, e2) =>
                {
                    var save = SaveManager.LoadGameFromFile();
                    if (save != null)
                    {
                        playMenu.DialogResult = DialogResult.OK;
                        playMenu.Close();
                        this.Close();
                        Form1 form = new Form1(save, audioManager);
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
            OptionsForm optionsForm = new OptionsForm(false, audioManager);
            optionsForm.Show();
        }

        private void BtnQuit_Click(object? sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
            Application.Exit();
        }

        private void MainMenuForm_Load(object sender, EventArgs e)
        {

        }
    }

    public class DifficultySelectForm : Form
    {
        public int SelectedDifficulty { get; private set; } = 1;

        public DifficultySelectForm()
        {
            this.Text = "Select Difficulty";
            this.ClientSize = new Size(800, 300);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(40, 40, 40);

            string projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."));
            string diffPath = Path.Combine(projectRoot, "gameAssets", "gameArt", "difficulties");

            Label title = new Label
            {
                Text = "SELECT DIFFICULTY",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(250, 15)
            };
            
            Controls.Add(title);

            AddLabel("EASY", 120);
            AddLabel("NORMAL", 370);
            AddLabel("HARD", 630);

            AddButton(1, Path.Combine(diffPath, "diff1.png"), 50);
            AddButton(2, Path.Combine(diffPath, "diff2.png"), 300);
            AddButton(3, Path.Combine(diffPath, "diff3.png"), 550);
        }

        private void AddButton(int difficulty, string imagePath, int x)
        {
            Button btn = new Button();
            btn.Size = new Size(200, 200);
            btn.Location = new Point(x, 50);
            btn.BackgroundImage = Image.FromFile(imagePath);
            btn.BackgroundImageLayout = ImageLayout.Stretch;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;

            btn.Click += (s, e) =>
            {
                SelectedDifficulty = difficulty;
                DialogResult = DialogResult.OK;
                Close();
            };

            Controls.Add(btn);
        }
        private void AddLabel(string text, int centerX)
        {
            Label label = new Label
            {
                Text = text,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                AutoSize = true
            };

            label.Location = new Point(centerX - (label.PreferredWidth / 2), 70);
            Controls.Add(label);
        }
    }

}
