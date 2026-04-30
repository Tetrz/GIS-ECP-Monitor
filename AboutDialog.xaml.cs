using System.Windows;

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

        // Обработчик события для кнопки "OK".
        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true; // Закрывает окно и возвращает true.
        }
    }
}
