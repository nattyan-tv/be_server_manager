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
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    /// 
    
    public partial class MainWindow : Window
    {
        // https://www.minecraft.net/en-us/download/server/bedrock
        // https://minecraft.azureedge.net/bin-win/bedrock-server-XXXXX.zip
        class ConfigData
        {
            public string name { get; set; }
            public string location { get; set; }
        }

        private void textBoxPrice_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !new Regex("[0-9.]").IsMatch(e.Text);
        }
        private void textBoxPrice_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;
            }
        }
        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.ToString());
        }

        public MainWindow()
        {
            InitializeComponent();
            if (File.Exists(@AppDomain.CurrentDomain.BaseDirectory + @"\setting.json") == false)
            {
                ConfigData cfgDATA = new ConfigData
                {
                    name = "Dedicated Server",
                    location = @"C:\",
                };
                string json = JsonConvert.SerializeObject(cfgDATA, Formatting.Indented);
                File.WriteAllText(@AppDomain.CurrentDomain.BaseDirectory + @"\setting.json", json);
            }
            try
            {
                ConfigData cfgDATA = null;
                using (StreamReader file = File.OpenText(@AppDomain.CurrentDomain.BaseDirectory + @"\setting.json"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    cfgDATA = (ConfigData)serializer.Deserialize(file, typeof(ConfigData));
                }
                サーバー名.Content = cfgDATA.name;
                serverLocation.Text = cfgDATA.location;
                // Pythonでバージョン系を取得するプログラム
                if (File.Exists(@cfgDATA.location + @"\bedrock_server.exe"))
                {
                    ;
                }
            }
            catch (Exception err)
            {
                MessageBox.Show("設定ファイル読み込み時にエラーが発生しました。\n" + err, "BE Server Manager", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
            
            


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
    }
}
