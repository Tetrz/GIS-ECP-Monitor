using System.Diagnostics;
using System.Windows.Navigation;

namespace GIS_ECP_Monitor
{
    // Переход на веб-страницу при нажатии на гиперссылку.
    public static class HyperlinkHelper
    {
        public static void Navigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = e.Uri.AbsoluteUri,
                UseShellExecute = true
            });

            e.Handled = true;
        }
    }
}
