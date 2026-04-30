using GIS_ECP_Monitor;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GIS_ECP_Folder_Watcher
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        private MainWindow mainWindow;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            mainWindow = new MainWindow();

            // НЕ вызываем Show()
            // окно не появляется

            /// mainWindow.Hide(); // на всякий случай
        }
    }
}
