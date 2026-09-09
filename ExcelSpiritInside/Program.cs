namespace ExcelSpiritInside
{
    internal static class Program
    {
        public static string LogPath { get; } = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ExcelSpiritInside", "logs", "app.log");

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) => ReportUnhandled(e.Exception, "UI thread");
            AppDomain.CurrentDomain.UnhandledException += (s, e) => ReportUnhandled(e.ExceptionObject as Exception, "background thread");
            System.Threading.Tasks.TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                ReportUnhandled(e.Exception, "task");
                e.SetObserved();
            };

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }

        public static void Log(string message)
        {
            try
            {
                var dir = System.IO.Path.GetDirectoryName(LogPath)!;
                System.IO.Directory.CreateDirectory(dir);
                System.IO.File.AppendAllText(LogPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}{Environment.NewLine}");
            }
            catch
            {
                // Logging must never crash the app.
            }
        }

        private static void ReportUnhandled(Exception? ex, string source)
        {
            Log($"[UNHANDLED:{source}] {ex}");
            MessageBox.Show(
                $"An unexpected error occurred ({source}) and was caught to keep the app running.\r\n\r\n" +
                $"{ex?.Message}\r\n\r\nDetails were written to:\r\n{LogPath}",
                "Excel Spirit Inside", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}