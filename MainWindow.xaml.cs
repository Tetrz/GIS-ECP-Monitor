using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using Forms = System.Windows.Forms;

namespace GIS_ECP_Monitor
{
    public partial class MainWindow : Window
    {
        private Forms.NotifyIcon TrayIcon;
        private DispatcherTimer Timer;

        private string BasePath;
        private string Holidays;
        private string FoundFilePath;

        private Icon RedIcon;
        private Icon GreenIcon;

        public MainWindow()
        {
            try
            {
                InitializeComponent();

                SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;

                BasePath = Properties.Settings.Default.BasePath;
                Holidays = Properties.Settings.Default.Holidays;

                InitTray();

                InitTimer();

                CheckFolder();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // Инициализация иконки в трее и её меню.
        private void InitTray()
        {
            RedIcon = new Icon(GetAppPath("Icons", "Red.ico"));
            GreenIcon = new Icon(GetAppPath("Icons", "Green.ico"));

            TrayIcon = new Forms.NotifyIcon();

            TrayIcon.Icon = RedIcon;
            TrayIcon.Visible = true;
            TrayIcon.Text = "ГИС ЕЦП - статус запросов";

            TrayIcon.DoubleClick += TrayIcon_DoubleClick;

            var menu = new Forms.ContextMenuStrip();

            var autoStartItem = new Forms.ToolStripMenuItem("Автозапуск");
            autoStartItem.Checked = IsAutoStartEnabled();
            autoStartItem.Click += AutoStart_Click;
            menu.Items.Add(autoStartItem);

            var chooseFolderItem = new Forms.ToolStripMenuItem("Выбрать папку с запросами");
            chooseFolderItem.Checked = !string.IsNullOrEmpty(BasePath);
            chooseFolderItem.Click += BasePathSet;
            menu.Items.Add(chooseFolderItem);

            var holidaySetItem = new Forms.ToolStripMenuItem("Задать даты праздников");
            holidaySetItem.Checked = !string.IsNullOrEmpty(Holidays);
            holidaySetItem.Click += HolidaySet;
            menu.Items.Add(holidaySetItem);

            menu.Items.Add("-");
            menu.Items.Add("Открыть папку с запросами", null, OpenRequestsFolder);
            menu.Items.Add("-");
            menu.Items.Add("Справка", null, OpenHelpDialog);
            menu.Items.Add("О программе", null, OpenAboutDialog);
            menu.Items.Add("-");
            menu.Items.Add("Выход", null, Exit_Click);

            TrayIcon.ContextMenuStrip = menu;
        }

        // Получить путь к ресурсу приложения.
        private string GetAppPath(params string[] paths)
        {
            return System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                System.IO.Path.Combine(paths)
            );
        }

        // Инициализация таймера для проверки папки с запросами каждые 30 секунд.
        private void InitTimer()
        {
            Timer = new DispatcherTimer();
            Timer.Interval = TimeSpan.FromSeconds(30);
            Timer.Tick += Timer_Tick;
            Timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            CheckFolder();
        }

        // Проверка папки на наличие новых запросов.
        private void CheckFolder()
        {
            // Проверка запросов за вчерашний день.
            DateTime checkDate = DateTime.Today.AddDays(-1);

            // Если дата выпадает на воскресенье, то корректируем её в сторону уменьшения.
            if (checkDate.DayOfWeek == DayOfWeek.Sunday) checkDate = checkDate.AddDays(-2);

            // Если дата выпадает на праздничный день, то корректируем её в сторону уменьшения.
            var holidaysDates = Holidays
                .Split(',')
                .Select(s =>
                    {
                        DateTime result;
                        DateTime.TryParseExact(
                        s.Trim(),
                        "dd.MM.yyyy",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out result);

                        return result;
                    })
                .ToArray();

            foreach (var date in holidaysDates)
            {
                if (checkDate.Date == date.Date)
                {
                    checkDate = checkDate.AddDays(-1);

                    // Если дата выпадает на воскресенье, то корректируем её в сторону уменьшения.
                    if (checkDate.DayOfWeek == DayOfWeek.Sunday) checkDate = checkDate.AddDays(-2);
                }
            }

            string folderName = checkDate.ToString("yyyy.MM.dd");
            string displayDate = checkDate.ToString("dd.MM");
            string fullPath = Path.Combine(BasePath, folderName);

            if (!Directory.Exists(fullPath))
            {
                var dayName = checkDate.ToString("dddd", new CultureInfo("ru-RU"));
                SetRed("Запросы не скачаны за " + displayDate + " (" + dayName + "). Папка не найдена.");
                return;
            }

            FoundFilePath = Directory.GetFiles(fullPath)
                .FirstOrDefault(x =>
                    Path.GetFileName(x)
                    .IndexOf("Merged.pdf", StringComparison.OrdinalIgnoreCase) >= 0);

            var dayNameResult = checkDate.ToString("dddd", new CultureInfo("ru-RU"));
            if (string.IsNullOrEmpty(FoundFilePath))
            {
                SetRed("Запросы не скачаны за " + displayDate + " (" + dayNameResult + ").");
            }
            else
            {
                SetGreen("Запросы скачаны за " + displayDate + " (" + dayNameResult + ").");

                Timer.Stop();
            }
        }

        private void SetRed(string text)
        {
            TrayIcon.Icon = RedIcon;
            TrayIcon.Text = text;
        }

        private void SetGreen(string text)
        {
            TrayIcon.Icon = GreenIcon;
            TrayIcon.Text = text;
        }

        // Открыть файл с запросами по двойному клику на иконке в трее, если его нет — открыть папку с запросами, чтобы пользователь мог проверить её содержимое.
        private void TrayIcon_DoubleClick(object sender, EventArgs e)
        {
            // Если файл найден, то открываем его.
            if (!string.IsNullOrEmpty(FoundFilePath) && File.Exists(FoundFilePath))
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = FoundFilePath,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка открытия файла: " + ex.Message);
                }
            }
            else
            {
                // Если файла нет — открываем папку с запросами, чтобы пользователь мог проверить её содержимое.
                OpenRequestsFolder(this, EventArgs.Empty);
            }
        }

        // Открыть папку с запросами.
        private void OpenRequestsFolder(object sender, EventArgs e)
        {
            try
            {
                // Открывать запросы.
                Process.Start("explorer.exe", BasePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #region Автозапуск.
        private const string AppName = "GIS_ECP_Monitor";

        // Обработчик клика по пункту меню автозапуска.
        private void AutoStart_Click(object sender, EventArgs e)
        {
            var item = sender as Forms.ToolStripMenuItem;

            if (item.Checked)
            {
                DisableAutoStart();
                item.Checked = false;
            }
            else
            {
                EnableAutoStart();
                item.Checked = true;
            }
        }

        // Включить автозапуск приложения.
        private void EnableAutoStart()
        {
            // Компьютер\HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Run
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Run", true))
            {
                string exePath = Assembly.GetExecutingAssembly().Location;
                key.SetValue(AppName, "\"" + exePath + "\"");
            }
        }

        // Выключить автозапуск приложения.
        private void DisableAutoStart()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Run", true))
            {
                key.DeleteValue(AppName, false);
            }
        }

        // Проверить, включён ли автозапуск приложения.
        private bool IsAutoStartEnabled()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Run", false))
            {
                return key.GetValue(AppName) != null;
            }
        }
        #endregion

        // Выбрать к папку с запросами.
        private void BasePathSet(object sender, EventArgs e)
        {
            string selectedPath = OpenFolderDialog("Выберите папку с запросами");

            var item = sender as Forms.ToolStripMenuItem;

            if (!string.IsNullOrEmpty(selectedPath))
            {
                BasePath = selectedPath;

                item.Checked = true;

                // Сохраняем настройки.
                Properties.Settings.Default.BasePath = BasePath;
                Properties.Settings.Default.Save();
            }
        }

        // Задать даты праздников.
        private void HolidaySet(object sender, EventArgs e)
        {
            var dialog = new HolidaySetDialog(Holidays);

            bool? result = dialog.ShowDialog();

            if (result == true) // Пользователь нажал OK.
            {
                // Сохраняем настройки.
                Holidays = dialog.Holidays;

                var item = sender as Forms.ToolStripMenuItem;

                if (!string.IsNullOrEmpty(Holidays))
                {
                    item.Checked = true;
                }
                else
                {
                    item.Checked = false;
                }

                Properties.Settings.Default.Holidays = Holidays;
                Properties.Settings.Default.Save();
            }
        }


        // Открытие диалога выбора папки.
        private string OpenFolderDialog(string title)
        {
            try
            {
                var dialog = new CommonOpenFileDialog
                {
                    IsFolderPicker = true,
                    Title = title
                };

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    return dialog.FileName;
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
                return null;
            }
        }

        // Вызов диалога "Справка".
        private void OpenHelpDialog(object sender, EventArgs e)
        {
            var dialog = new HelpDialog();

            dialog.ShowDialog();
        }

        // Вызов диалога "О программе".
        private void OpenAboutDialog(object sender, EventArgs e)
        {
            var dialog = new AboutDialog();

            dialog.ShowDialog();
        }

        // Выход из приложения.
        private bool isExit;
        private void Exit_Click(object sender, EventArgs e)
        {
            isExit = true;
            Close();
        }

        // Выход ПК из режима гибернации.
        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Resume)
            {
                // ПК проснулся.
                Timer.Start();
            }
        }

        // При закрытии приложения освобождаем ресурсы.
        protected override void OnClosing(CancelEventArgs e)
        {
            if (!isExit)
            {
                e.Cancel = true;
                Hide();
                return;
            }

            TrayIcon.Visible = false;
            TrayIcon.Dispose();

            RedIcon?.Dispose();
            GreenIcon?.Dispose();
        }
        protected override void OnClosed(EventArgs e)
        {
            // Отписываемся от события.
            SystemEvents.PowerModeChanged -= SystemEvents_PowerModeChanged;

            base.OnClosed(e);
        }
    }
}