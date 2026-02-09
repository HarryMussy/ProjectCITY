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

            await Task.Delay(1000); // simulate load or do actual loading

            mainForm = new Form1(new AudioManager());

            loadingForm.Hide();
            loadingForm.Dispose();

            mainForm.FormClosed += (s, e) => ExitThread();
            mainForm.Show();
        }
    }
}
