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

namespace bedrock_server_manager
{
    /// <summary>
    /// firstSession.xaml の相互作用ロジック
    /// </summary>
    /// 
    public partial class firstSession : Window
    {
        public bool setted = false;
        class ConfigData
        {
            public string name { get; set; }
            public string location { get; set; }
            public string seed { get; set; }
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
            ConfigData cfgDATA = new ConfigData
            {
                name = server_name.Text,
                location = serverLocation.Text,
                seed = server_seed.Text
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
    }
}
