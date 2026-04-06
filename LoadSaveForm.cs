using System;
using System.Drawing;
using System.Windows.Forms;

namespace CitySkylines0._5alphabeta
{
    //simple dialog wrapper that exposes the chosen path via SelectedPath and DialogResult
    public partial class LoadSaveForm : Form
    {
        private Button btnChoose;
        private Button btnCancel;
        public string SelectedPath { get; private set; } = "";
        private readonly bool isSaving; //true when saving, false when loading - controls which file dialog opens

        public LoadSaveForm(bool isSaving)
        {
            this.isSaving = isSaving;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            Text = isSaving ? "Save Game" : "Load Game";
            ClientSize = new Size(420, 130);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(40, 40, 40);

            btnChoose = new Button
            {
                Text = isSaving ? "Choose Save Location..." : "Choose Save File...",
                Size = new Size(360, 36),
                Location = new Point(30, 20),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };
            btnChoose.Click += BtnChoose_Click;

            btnCancel = new Button
            {
                Text = "Cancel",
                Size = new Size(100, 30),
                Location = new Point(160, 70),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            Controls.Add(btnChoose);
            Controls.Add(btnCancel);
        }

        private void BtnChoose_Click(object sender, EventArgs e)
        {
            if (isSaving)
            {
                //open a save file dialog and store the chosen path on OK
                using (var sfd = new SaveFileDialog())
                {
                    sfd.Filter = "City Save (*.citysave)|*.citysave";
                    sfd.DefaultExt = "citysave";
                    sfd.AddExtension = true;
                    sfd.Title = "Save Game";
                    if (sfd.ShowDialog(this) == DialogResult.OK)
                    {
                        SelectedPath = sfd.FileName;
                        DialogResult = DialogResult.OK;
                        Close();
                    }
                }
            }
            else
            {
                //open a load file dialog and store the chosen path on OK
                using (var ofd = new OpenFileDialog())
                {
                    ofd.Filter = "City Save (*.citysave)|*.citysave";
                    ofd.DefaultExt = "citysave";
                    ofd.AddExtension = true;
                    if (ofd.ShowDialog(this) == DialogResult.OK)
                    {
                        SelectedPath = ofd.FileName;
                        DialogResult = DialogResult.OK;
                        Close();
                    }
                }
            }
        }
    }
}