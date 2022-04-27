using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace bedrock_server_manager
{
    /// <summary>
    /// settingUpdate.xaml の相互作用ロジック
    /// </summary>
    public partial class settingUpdate : Window
    {
        private void textBoxTime_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !new Regex("[0-9:]").IsMatch(e.Text);
        }
        private void textBoxTime_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;
            }
        }

        public settingUpdate()
        {
            InitializeComponent();
        }

        private void saveSettings(object sender, RoutedEventArgs e)
        {
            DateTime dt;

            if (updateTime.Text.Length == 4)
            {
                updateTime.Text = "0" + updateTime.Text;
            }

            if (!DateTime.TryParseExact(updateTime.Text, "HH:mm", null, DateTimeStyles.AssumeLocal, out dt))
            {
                MessageBox.Show("アップデートの時間指定が異常です。", "BE Server Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        private void AutoBackup_Clicked(object sender, RoutedEventArgs e)
        {
            updateTime.IsEnabled = !updateTime.IsEnabled;
        }
    }
}
