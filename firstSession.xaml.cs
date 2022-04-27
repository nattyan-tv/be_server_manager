using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Newtonsoft.Json;
using System.Globalization;

namespace bedrock_server_manager
{
    /// <summary>
    /// firstSession.xaml の相互作用ロジック
    /// </summary>
    /// 
    public partial class firstSession : Window
    {
        public bool setted = false;

        public class ConfigData
        {
            public string name { get; set; }
            public string location { get; set; }
            public string seed { get; set; }
            public string update { get; set; }
            public string backup { get; set; }
            public string backupTime { get; set; }
            public bool autoupdate { get; set; }
            public bool autobackup { get; set; }
            public string botToken { get; set; }
            public string botPrefix { get; set; }
        }

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

        public firstSession()
        {
            InitializeComponent();
        }

        private void changeServerLocation(object sender, RoutedEventArgs e)
        {
            using (var cofd = new CommonOpenFileDialog()
            {
                Title = "サーバーを保存する場所を選択してください。",
                InitialDirectory = @"C:",
                // フォルダ選択モードにする
                RestoreDirectory = true,
                IsFolderPicker = true,
            })
            {
                if (cofd.ShowDialog() != CommonFileDialogResult.Ok)
                {
                    return;
                }
                serverLocation.Text = cofd.FileName;
            }
        }

        private void saveAndStart(object sender, RoutedEventArgs e)
        {
            DateTime dt;

            if (updateTime.Text.Length == 4)
            {
                updateTime.Text = "0" + updateTime.Text;
            }

            if (!DateTime.TryParseExact(updateTime.Text, "HH:mm", null, DateTimeStyles.AssumeLocal, out dt))
            {
                MessageBox.Show("バックアップの時間指定が異常です。", "BE Server Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            backupTime.Text = backupTime.Text.Replace(":", "");

            ConfigData cfgDATA = new ConfigData
            {
                name = server_name.Text,
                location = serverLocation.Text,
                seed = server_seed.Text,
                update = updateTime.Text,
                backup = serverBackup.Text,
                backupTime = backupTime.Text,
                autoupdate = (bool)AutoUpdate.IsChecked,
                autobackup = (bool)AutoBackup.IsChecked,
                botToken = "",
                botPrefix = ""
            };
            string json = JsonConvert.SerializeObject(cfgDATA, Formatting.Indented);
            File.WriteAllText(@AppDomain.CurrentDomain.BaseDirectory + @"\setting.json", json);
            setted = true;
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (setted)
            {
                return;
            }
            else
            {
                e.Cancel = true;
                Environment.Exit(0);
            }
            
        }

        private void changeBackupLocation(object sender, RoutedEventArgs e)
        {
            using (var cofd = new CommonOpenFileDialog()
            {
                Title = "バックアップデータを保存する場所を選択してください。",
                InitialDirectory = @"C:",
                RestoreDirectory = true,
                IsFolderPicker = true,
            })
            {
                if (cofd.ShowDialog() != CommonFileDialogResult.Ok)
                {
                    return;
                }
                serverBackup.Text = cofd.FileName;
            }
        }

        private void AutoUpdate_Clicked(object sender, RoutedEventArgs e)
        {
            updateTime.IsEnabled = !updateTime.IsEnabled;
        }

        private void AutoBackup_Clicked(object sender, RoutedEventArgs e)
        {
            backupButton.IsEnabled = !backupButton.IsEnabled;
            backupTime.IsEnabled = !backupTime.IsEnabled;
        }
    }
}
