using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GIS_ECP_Monitor
{
    /// <summary>
    /// Логика взаимодействия для HelpDialog.xaml
    /// </summary>
    public partial class HelpDialog : Window
    {
        public HelpDialog()
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
