using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ProjectCity
{
    public partial class MainMenuForm : Form
    {
        private Button btnPlay;
        private Button btnOptions;
        private Button btnQuit;
        private PictureBox backgroundBox;
        public AudioManager audioManager;
        public int SelectedDifficulty { get; private set; }
        public bool IsNewGame { get; private set; }
        public SaveManager.SaveData? LoadedSave { get; private set; }

        public MainMenuForm()
        {
            InitializeComponent();

            this.Text = "PROJECT CITY - MAIN MENU";
            this.ClientSize = new Size(3, 3);
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            audioManager = new AudioManager();

            //load and stretch the background art
            string projectRoot = AppContext.BaseDirectory;
            string backgroundPath = Path.Combine(projectRoot, "gameAssets", "gameArt", "projectCityMain.png");

            backgroundBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = Image.FromFile(backgroundPath)
            };
            this.Controls.Add(backgroundBox);

            btnPlay = CreateMenuButton("PLAY");
            btnOptions = CreateMenuButton("OPTIONS");
            btnQuit = CreateMenuButton("QUIT");

            btnPlay.Click += BtnPlay_Click;
            btnOptions.Click += BtnOptions_Click;
            btnQuit.Click += BtnQuit_Click;

            //add buttons AFTER background so they render on top
            this.Controls.Add(btnPlay);
            this.Controls.Add(btnOptions);
            this.Controls.Add(btnQuit);

            backgroundBox.SendToBack();

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

        //repositions the three buttons to the horizontal centre of the form
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
            //open a sub-menu that lets the player choose between new game and load game
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
                    using (DifficultySelectForm diffForm = new DifficultySelectForm())
                    {
                        if (diffForm.ShowDialog() == DialogResult.OK)
                        {
                            SelectedDifficulty = diffForm.SelectedDifficulty;
                            IsNewGame = true;

                            //close both the sub-menu and the main menu with OK so Program.cs launches the game
                            playMenu.DialogResult = DialogResult.OK;
                            playMenu.Close();

                            this.DialogResult = DialogResult.OK;
                            this.Close();
                        }
                    }
                };

                btnLoad.Click += (s, e2) =>
                {
                    var save = SaveManager.LoadGameFromFile();
                    if (save != null)
                    {
                        LoadedSave = save;
                        IsNewGame = false;

                        playMenu.DialogResult = DialogResult.OK;
                        playMenu.Close();

                        this.DialogResult = DialogResult.OK;
                        this.Close();
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

            string projectRoot = AppContext.BaseDirectory;
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

        //creates an image button for a difficulty level and wires it to set SelectedDifficulty and close
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