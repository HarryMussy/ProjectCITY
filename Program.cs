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

    //custom application context manages the form lifecycle so the app can transition
    //between the main menu, loading screen and game without closing the process
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
                    await ShowLoadingAndLaunchGameAsync(mainMenu); //player pressed play - show loading screen then start game
                }
                else
                {
                    ExitThread(); //player pressed quit - close the application
                }
            };

            mainMenu.Show();
        }

        //shows a loading screen while the game constructs its world, then swaps to Form1
        private async Task ShowLoadingAndLaunchGameAsync(MainMenuForm menu)
        {
            try
            {
                loadingForm = new LoadingForm();
                this.MainForm = loadingForm;

                loadingForm.Show();
                loadingForm.Refresh(); //force a repaint so the GIF is visible during world generation

                if (menu.IsNewGame)
                {
                    mainForm = new Form1(menu.SelectedDifficulty, new AudioManager());
                }
                else
                {
                    mainForm = new Form1(menu.LoadedSave, new AudioManager());
                }

                mainForm.FormClosed += (s, e) =>
                {
                    if (mainForm.ReturnToMenu)
                    {
                        ShowMainMenu(); //player used "quit to main menu" - return rather than closing the app
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}