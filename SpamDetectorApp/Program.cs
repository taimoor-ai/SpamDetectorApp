using SpamDetectorApp.Forms;
using System;
using System.Windows.Forms;

namespace SpamDetectorApp
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.ThreadException += (sender, e) =>
            {
                MessageBox.Show(
                    $"An unexpected error occurred:\n\n{e.Exception.Message}\n\n" +
                    "The application will attempt to continue.",
                    "Unhandled Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            };

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                var ex = e.ExceptionObject as Exception;
                MessageBox.Show(
                    $"A fatal error occurred:\n\n{ex?.Message ?? "Unknown error"}\n\nThe application must close.",
                    "Fatal Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Stop);
            };

            Application.Run(new MainForm());
        }
    }
}