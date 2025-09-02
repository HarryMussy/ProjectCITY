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

            // Use ApplicationContext to manage both forms
            var context = new CustomApplicationContext();
            Application.Run(context); // ? Only ONE message loop
        }
    }

    public class CustomApplicationContext : ApplicationContext
    {
        private LoadingForm loadingForm;
        private Form1 mainForm;

        public CustomApplicationContext()
        {
            loadingForm = new LoadingForm();
            loadingForm.Shown += async (s, e) => await LoadMainFormAsync();
            loadingForm.FormClosed += (s, e) => ExitThread(); // fallback if user closes early
            loadingForm.Show();
        }

        private async Task LoadMainFormAsync()
        {
            // Simulate loading delay or do non-UI loading here
            await Task.Delay(0); // Or your actual loading logic

            // Now create the main form on the UI thread
            mainForm = new Form1();

            loadingForm.Hide();
            loadingForm.Dispose();

            mainForm.FormClosed += (s, e) => ExitThread();
            mainForm.Show();
        }
    }
}
