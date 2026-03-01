using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CitySkylines0._5alphabeta
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new CustomApplicationContext());
        }
    }

    public class CustomApplicationContext : ApplicationContext
    {
        private LoadingForm loadingForm;
        private Form1 mainForm;

        public CustomApplicationContext()
        {
            ShowMainMenu();
        }

        private void ShowMainMenu()
        {
            MainMenuForm mainMenu = new MainMenuForm();
            DialogResult result = mainMenu.ShowDialog();

            if (result == DialogResult.OK)
            {
                _ = ShowLoadingAndLaunchGameAsync();
            }
            else
            {
                ExitThread();
            }
        }

        private async Task ShowLoadingAndLaunchGameAsync()
        {
            loadingForm = new LoadingForm();
            loadingForm.Show();
            loadingForm.Refresh();

            // Create Form1 on UI thread (light initialization only)
            mainForm = new Form1(new AudioManager());
            
            // Defer heavy background rendering to after the main window is visible
            mainForm.FormClosed += (s, e) => ExitThread();
            mainForm.Show();
            
            // Hide loading form after main window appears
            await Task.Delay(500);
            loadingForm.Hide();
            loadingForm.Dispose();
        }
    }
}
