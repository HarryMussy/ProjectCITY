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
            this.MainForm = mainMenu;

            mainMenu.FormClosed += async (s, e) =>
            {
                if (mainMenu.DialogResult == DialogResult.OK)
                {
                    await ShowLoadingAndLaunchGameAsync(mainMenu);
                }
                else
                {
                    ExitThread();
                }
            };

            mainMenu.Show();
        }

        private async Task ShowLoadingAndLaunchGameAsync(MainMenuForm menu)
        {
            loadingForm = new LoadingForm();
            this.MainForm = loadingForm;

            loadingForm.Show();
            loadingForm.Refresh();

            if (menu.IsNewGame)
            {
                mainForm = new Form1(menu.SelectedDifficulty, new AudioManager());
            }
            else
            {
                mainForm = new Form1(menu.LoadedSave, new AudioManager());
            }

            // Key change: instead of ExitThread on close, go back to menu
            mainForm.FormClosed += (s, e) =>
            {
                if (mainForm.ReturnToMenu)
                {
                    ShowMainMenu();
                }
                else
                {
                    ExitThread();
                }
            };

            loadingForm.Hide();
            this.MainForm = mainForm;
            mainForm.Show();
        }
    }
}
