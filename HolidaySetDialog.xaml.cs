using System.Windows;

namespace GIS_ECP_Monitor
{
    /// <summary>
    /// Логика взаимодействия для HolidaySetDialog.xaml
    /// </summary>
    public partial class HolidaySetDialog : Window
    {
        public string Holidays;
        public HolidaySetDialog(string _Holidays)
        {
            InitializeComponent();

            Holidays = _Holidays;
            HolidaysTextBox.Text = Holidays;
        }

        // Обработчик события для кнопки "OK".
        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            Holidays = HolidaysTextBox.Text;

            this.DialogResult = true; // Закрывает окно и возвращает true.
        }
    }
}
