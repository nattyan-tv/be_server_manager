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
using static bedrock_server_manager.BaseConfig;

namespace bedrock_server_manager
{
    /// <summary>
    /// firstSession.xaml の相互作用ロジック
    /// </summary>
    /// 
    public partial class firstSession : Window
    {
        public bool setted = false;

        private void textBoxTime_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !new Regex("[0-9]").IsMatch(e.Text);
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
            if (server_name.Text == "" || serverLocation.Text == "")
            {
                MessageBox.Show("サーバー名かファイル場所が指定されていません。\nこれらは必須項目です。", "BE Server Manager", MessageBoxButton.OK, MessageBoxImage.Hand);
                return;
            }
            else if ((bool)AutoBackup.IsChecked && (backupTime.Text == "" || serverBackup.Text == ""))
            {
                MessageBox.Show("バックアップ間隔と場所が指定されていません。\nバックアップの設定を有効にする場合はこれらは必須項目です。", "BE Server Manager", MessageBoxButton.OK, MessageBoxImage.Hand);
                return;
            }
            else if ((bool)AutoUpdate.IsChecked && updateTime.Text == "")
            {
                MessageBox.Show("アップデート確認間隔が指定されていません。\nアップデートの設定を有効にする場合はこれらは必須項目です。", "BE Server Manager", MessageBoxButton.OK, MessageBoxImage.Hand);
                return;
            }

            backupTime.Text = backupTime.Text.Replace(":", "");

            BaseConfig[] cfgDATA = {new BaseConfig
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
            } };
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
