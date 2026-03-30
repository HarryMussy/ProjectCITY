using System.IO;
using System.Windows.Forms;
using System.Drawing;

public class LoadingForm : Form
{
    private PictureBox pictureBoxLoading;

    public LoadingForm()
    {
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.None;
        this.ShowInTaskbar = false;
        this.ClientSize = new Size(512, 256);

        pictureBoxLoading = new PictureBox
        {
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.Zoom
        };

        string projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."));
        string gifPath = Path.Combine(projectRoot, "gameAssets", "gameArt", "loadingScreen.gif");
        pictureBoxLoading.Image = Image.FromFile(gifPath);

        this.Controls.Add(pictureBoxLoading);
    }
}