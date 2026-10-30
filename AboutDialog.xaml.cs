using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Navigation;

namespace GIS_ECP_Monitor
{
    /// <summary>
    /// Логика взаимодействия для AboutDialog.xaml
    /// </summary>
    public partial class AboutDialog : Window
    {
        public AboutDialog()
        {
            InitializeComponent();
        }

        // Переход на веб-страницу при нажатии на гиперссылку.
        private void Hyperlink_RequestNavigate(
            object sender,
            RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = e.Uri.AbsoluteUri,
                UseShellExecute = true
            });

            e.Handled = true;
        }

        // Обработчик события для кнопки "OK".
        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true; // Закрывает окно и возвращает true.
        }
    }

    // Версия приложения.
    public static class AppInfo
    {
        public static string Version
        {
            get
            {
                Assembly assembly = Assembly.GetExecutingAssembly();

                string informationalVersion = assembly
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                    .InformationalVersion;

                if (!string.IsNullOrWhiteSpace(informationalVersion))
                {
                    return GetShortVersion(informationalVersion.Split('+')[0]);
                }

                var assemblyVersion = assembly.GetName().Version;

                return assemblyVersion != null
                    ? GetShortVersion(assemblyVersion.ToString())
                    : "Неизвестно";
            }
        }

        // Оставляет первые три группы цифр и завершающую точку (1.0.2.0 -> 1.0.2.).
        private static string GetShortVersion(string version)
        {
            string[] parts = version.Split('.');
            string shortVersion = parts.Length > 3
                ? string.Join(".", parts, 0, 3)
                : version.TrimEnd('.');

            return shortVersion + ".";
        }
    }
}
