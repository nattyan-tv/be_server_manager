﻿using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static bedrock_server_manager.BaseConfig;

namespace bedrock_server_manager
{
    /// <summary>
    /// settingBackup.xaml の相互作用ロジック
    /// </summary>
    public partial class settingBackup : Window
    {
        public bool setted = true;

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

        public static void FileCopy(string sourcePath, string destinationPath, bool rewrite)
        {

            if (Directory.Exists(sourcePath) == false)
            {
                return;
            }

            File.Copy(sourcePath, destinationPath, rewrite);

        }

        public static void DirectoryCopy(string sourcePath, string destinationPath)
        {
            DirectoryInfo sourceDirectory = new DirectoryInfo(sourcePath);
            DirectoryInfo destinationDirectory = new DirectoryInfo(destinationPath);

            if (sourceDirectory.Exists == false) { throw new IOException("元ディレクトリ（ゲームデータ保存場所）が見つかりませんでした。"); }


            if (destinationDirectory.Exists == false)
            {
                destinationDirectory.Create();
                destinationDirectory.Attributes = sourceDirectory.Attributes;
            }

            foreach (FileInfo fileInfo in sourceDirectory.GetFiles())
            {
                fileInfo.CopyTo(destinationDirectory.FullName + @"\" + fileInfo.Name, true);
            }

            foreach (System.IO.DirectoryInfo directoryInfo in sourceDirectory.GetDirectories())
            {
                DirectoryCopy(directoryInfo.FullName, destinationDirectory.FullName + @"\" + directoryInfo.Name);
            }
        }


        public settingBackup()
        {
            InitializeComponent();
            /// jsonファイルから設定を読み込んで、Textとかに埋め込む。
            /// その際、nullだったらなにも入れないみたいな...
            BaseConfig[] cfgDATA = null;
            using (StreamReader file = File.OpenText(@AppDomain.CurrentDomain.BaseDirectory + @"\setting.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                cfgDATA = (BaseConfig[])serializer.Deserialize(file, typeof(BaseConfig[]));
            }
            if (cfgDATA[0].backup != null)
            {
                serverBackup.Text = @cfgDATA[0].backup;
                backupTime.Text = cfgDATA[0].backupTime;
            }
            if (cfgDATA[0].autobackup)
            {
                AutoBackup.IsChecked = true;
                backupTime.IsEnabled = true;
                backupButton.IsEnabled = true;
            }
            else
            {
                AutoBackup.IsChecked = false;
                backupTime.IsEnabled = false;
                backupButton.IsEnabled = false;
            }

            setted = true;
        }

        private void saveSettings(object sender, RoutedEventArgs e)
        {
            backupTime.Text = backupTime.Text.Replace(":", "");
            BaseConfig[] BASEcfgDATA = null;
            using (StreamReader file = File.OpenText(@AppDomain.CurrentDomain.BaseDirectory + @"\setting.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                BASEcfgDATA = (BaseConfig[])serializer.Deserialize(file, typeof(BaseConfig[]));
            }
            BaseConfig[] cfgDATA = {new BaseConfig
            {
                name = BASEcfgDATA[0].name,
                location = @BASEcfgDATA[0].location,
                seed = BASEcfgDATA[0].seed,
                update = BASEcfgDATA[0].update,
                backup = serverBackup.Text,
                backupTime = backupTime.Text,
                autoupdate = BASEcfgDATA[0].autoupdate,
                autobackup = (bool)AutoBackup.IsChecked,
                botToken = BASEcfgDATA[0].botToken,
                botPrefix = BASEcfgDATA[0].botPrefix
            } };
            string json = JsonConvert.SerializeObject(cfgDATA, Formatting.Indented);
            File.WriteAllText(@AppDomain.CurrentDomain.BaseDirectory + @"\setting.json", json);
            setted = true;
            Close();
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

        private void changeBackupNOW(object sender, RoutedEventArgs e)
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
                backupNOWLocation.Text = cofd.FileName;
            }
        }

        public async static void BackUpThread(string base_location, string backup_location, string time)
        {
            await Task.Run(() =>
            {
                DirectoryCopy(@base_location, @backup_location + @"\" + @time);
            });

        }

        private void backupNOW(object sender, RoutedEventArgs e)
        {
            if (backupNOWLocation.Text == "/")
            {
                MessageBox.Show("バックアップを保存する場所が指定されていません。", "BE Server Manager", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            BaseConfig[] cfgDATA = null;
            using (StreamReader file = File.OpenText(@AppDomain.CurrentDomain.BaseDirectory + @"\setting.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                cfgDATA = (BaseConfig[])serializer.Deserialize(file, typeof(BaseConfig[]));
            }
            DateTime dt = DateTime.Now;
            try
            {
                BackUpThread(@cfgDATA[0].location, @backupNOWLocation.Text, @dt.ToString("yyyy_MM_dd-HH_mm_ss"));
                MessageBox.Show("バックアップに完了しました。", "BE Server Manager", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("バックアップ時にエラーが発生しました。\n" + ex, "BE Server Manager", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void serverBackup_TextChanged(object sender, TextChangedEventArgs e)
        {
            setted = false;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!setted)
            {
                MessageBoxResult ans_c = MessageBox.Show("保存していない項目があるようです。\n保存しなくてもよろしいですか？", "BE Server Manager", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation);
                Console.WriteLine(ans_c);
                if (ans_c != MessageBoxResult.OK) { e.Cancel = true; }
            }
        }

        private void AutoBackup_Clicked(object sender, RoutedEventArgs e)
        {
            backupButton.IsEnabled = !backupButton.IsEnabled;
            backupTime.IsEnabled = !backupTime.IsEnabled;
        }
    }
}
