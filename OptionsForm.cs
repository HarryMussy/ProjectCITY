using System;
using System.Drawing;
using System.Windows.Forms;

namespace CitySkylines0._5alphabeta
{
    // Single non-partial OptionsForm. If you previously had another partial OptionsForm remove it.
    public partial class OptionsForm : Form
    {
        private TrackBar masterVolume;
        private TrackBar musicVolume;
        private TrackBar effectsVolume;

        private CheckBox chkResizable;
        private CheckBox chkMaximize;
        private CheckBox chkFullscreen;

        private bool openedInGame;

        private Button btnSave;
        private Button btnLoad;
        private Button btnQuitToMenu;
        private Button btnQuitGame;

        private AudioManager audioManager;
        private Form1 gameFormReference;

        public OptionsForm(bool fromGame, AudioManager audio, Form1 gameForm)
        {
            openedInGame = fromGame;
            audioManager = audio;
            gameFormReference = gameForm;

            Text = "OPTIONS";
            ClientSize = new Size(620, 420);
            BackColor = Color.FromArgb(40, 40, 40);
            StartPosition = FormStartPosition.CenterParent;

            InitializeLayout();
            PopulateValuesFromAudio();
        }

        public OptionsForm(bool fromGame)
        {
            openedInGame = fromGame;

            Text = "OPTIONS";
            ClientSize = new Size(620, 420);
            BackColor = Color.FromArgb(40, 40, 40);
            StartPosition = FormStartPosition.CenterParent;

            InitializeLayout();
        }

        private void OptionsForm_Load(object sender, EventHandler e) { }
        private void InitializeLayout()
        {
            var lblAudio = new Label()
            {
                Text = "Audio",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(20, 20)
            };
            Controls.Add(lblAudio);

            masterVolume = CreateSlider("Master Volume", 60);
            musicVolume = CreateSlider("Music Volume", 120);
            effectsVolume = CreateSlider("Effects Volume", 180);

            var lblWindow = new Label()
            {
                Text = "Window",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(20, 240)
            };
            Controls.Add(lblWindow);

            chkResizable = CreateCheckbox("Allow Resizing", 280);
            chkMaximize = CreateCheckbox("Enable Maximize Button", 310);
            chkFullscreen = CreateCheckbox("Fullscreen Mode (borderless)", 340);

            chkResizable.CheckedChanged += (s, e) =>
            {
                if (gameFormReference != null)
                    gameFormReference.FormBorderStyle = chkResizable.Checked ? FormBorderStyle.Sizable : FormBorderStyle.FixedSingle;
            };
            chkMaximize.CheckedChanged += (s, e) =>
            {
                if (gameFormReference != null)
                    gameFormReference.MaximizeBox = chkMaximize.Checked;
            };
            chkFullscreen.CheckedChanged += (s, e) =>
            {
                if (gameFormReference == null) return;
                if (chkFullscreen.Checked)
                {
                    gameFormReference.WindowState = FormWindowState.Maximized;
                    gameFormReference.FormBorderStyle = FormBorderStyle.None;
                }
                else
                {
                    gameFormReference.FormBorderStyle = chkResizable.Checked ? FormBorderStyle.Sizable : FormBorderStyle.FixedSingle;
                    gameFormReference.WindowState = FormWindowState.Normal;
                }
            };

            // If opened from inside the game, show game options (save/load/quit)
            if (openedInGame)
            {
                btnSave = CreateGameButton("Save Game", 60, 380);
                btnSave.Click += (s, e) => OpenSaveDialogAndSave();

                btnLoad = CreateGameButton("Load Game", 60, 420);
                btnLoad.Click += (s, e) => OpenLoadDialogAndLoad();

                btnQuitToMenu = CreateGameButton("Quit to Main Menu", 340, 380);
                btnQuitToMenu.Click += (s, e) =>
                {
                    gameFormReference?.ReturnToMainMenu();
                    Close();
                };

                btnQuitGame = CreateGameButton("Quit Game", 340, 420);
                btnQuitGame.Click += (s, e) =>
                {
                    Close();
                    Application.Exit();
                };
            }

            // Close button
            var btnClose = new Button()
            {
                Text = "Close",
                Size = new Size(100, 36),
                Location = new Point(ClientSize.Width - 120, ClientSize.Height - 76),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White
            };
            btnClose.Click += (s, e) => Close();
            Controls.Add(btnClose);
        }

        private void PopulateValuesFromAudio()
        {
            if (audioManager != null)
            {
                masterVolume.Value = Math.Clamp((int)(audioManager.masterVolume * 100), 0, 100);
                musicVolume.Value = Math.Clamp((int)(audioManager.musicVolume * 100), 0, 100);
                effectsVolume.Value = Math.Clamp((int)(audioManager.efxVolume * 100), 0, 100);
            }
            else
            {
                masterVolume.Value = 50;
                musicVolume.Value = 50;
                effectsVolume.Value = 50;
            }
        }

        private TrackBar CreateSlider(string label, int y)
        {
            var lbl = new Label
            {
                Text = label,
                Location = new Point(40, y - 8),
                ForeColor = Color.White,
                AutoSize = true
            };
            Controls.Add(lbl);

            var slider = new TrackBar
            {
                Minimum = 0,
                Maximum = 100,
                TickFrequency = 10,
                Size = new Size(420, 45),
                Location = new Point(160, y - 12)
            };

            slider.Scroll += (s, e) =>
            {
                float v = slider.Value / 100f;
                if (audioManager != null)
                {
                    if (label.Contains("Master"))
                        audioManager.SetMasterVolume(v);
                    else if (label.Contains("Music"))
                        audioManager.SetMusicVolume(v);
                    else if (label.Contains("Effects"))
                        audioManager.SetEffectsVolume(v);
                }
            };

            Controls.Add(slider);
            return slider;
        }

        private CheckBox CreateCheckbox(string text, int y)
        {
            var chk = new CheckBox
            {
                Text = text,
                Location = new Point(40, y),
                ForeColor = Color.White,
                AutoSize = true
            };
            Controls.Add(chk);
            return chk;
        }

        private Button CreateGameButton(string text, int x, int y)
        {
            var b = new Button
            {
                Text = text,
                Size = new Size(260, 36),
                Location = new Point(x, y),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            Controls.Add(b);
            return b;
        }

        private void OpenSaveDialogAndSave()
        {
            using (var dlg = new LoadSaveForm(true))
            {
                var res = dlg.ShowDialog(this);
                if (res == DialogResult.OK && !string.IsNullOrEmpty(dlg.SelectedPath))
                {
                    // call Save manager with game form's state
                    try
                    {
                        SaveManager.SaveGameToFile(dlg.SelectedPath, gameFormReference.grid, gameFormReference.calendar, gameFormReference.background);
                        //MessageBox.Show("Game saved to: " + dlg.SelectedPath, "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        //MessageBox.Show("Save failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void OpenLoadDialogAndLoad()
        {
            using (var dlg = new LoadSaveForm(false))
            {
                var res = dlg.ShowDialog(this);
                if (res == DialogResult.OK && !string.IsNullOrEmpty(dlg.SelectedPath))
                {
                    try
                    {
                        var data = SaveManager.LoadGameFromFile();
                    }
                    catch (Exception ex)
                    {
                        //MessageBox.Show("Load failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
