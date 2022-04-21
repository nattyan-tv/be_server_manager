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
using System.IO.Compression;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Diagnostics;


namespace bedrock_server_manager
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    /// 
    
    public partial class MainWindow : Window
    {
        // ダウンロードページ: https://www.minecraft.net/en-us/download/server/bedrock
        // 最新バージョン: https://minecraft.azureedge.net/bin-win/bedrock-server-XXXXX.zip
        // サーバーバージョン取得: behavior_packs/vanilla_XXXXX

        // MinecraftBedrockサーバーのダウンロード用ページリンク変数
        public string belink = "https://www.minecraft.net/en-us/download/server/bedrock";



        public int changeLog = 0;
        public string downloadLink = "";
        public string currentVersion = "";
        public string latestVersion = "";

        class ConfigData
        {
            public string name { get; set; }
            public string location { get; set; }
            public string seed { get; set; }
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

            if (sourceDirectory.Exists == false)
            {
                return;
            }

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

        public MainWindow()
        {
            InitializeComponent();
            Console.WriteLine("Components initialized.");
            if (File.Exists(@AppDomain.CurrentDomain.BaseDirectory + @"\setting.json") == false)
            {
                Console.WriteLine("FirstSession dialog show!");
                firstSession firstSession = new firstSession();
                firstSession.ShowDialog();
                Console.WriteLine("FirstSession dialog closed.");
            }
            try
            {
                ConfigData cfgDATA = null;
                using (StreamReader file = File.OpenText(@AppDomain.CurrentDomain.BaseDirectory + @"\setting.json"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    cfgDATA = (ConfigData)serializer.Deserialize(file, typeof(ConfigData));
                }
                サーバー名.Content = "サーバー名：" + cfgDATA.name;
                serverLocation.Text = cfgDATA.location;
                Console.WriteLine("Settings are loaded.");

                Console.WriteLine("Start task...");
                var dlLink = new Process
                {
                    StartInfo = new ProcessStartInfo("python/BE_Server_Manager_MAIN.exe")
                    {
                        Arguments = "-getLatest",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                dlLink.Start();
                StreamReader dl = dlLink.StandardOutput;
                downloadLink = dl.ReadLine().Replace("\r", "").Replace("\n", "");
                dlLink.WaitForExit();
                dlLink.Close();
                Console.WriteLine("Connection all rights.\n" + downloadLink);

                Match match1 = Regex.Match(downloadLink, @"https://minecraft.azureedge.net/bin-win/bedrock-server-[0-9.-]+.zip");
                downloadLink = match1.Value;
                latestVersion = downloadLink.Replace("https://minecraft.azureedge.net/bin-win/bedrock-server-", "").Replace(".zip", "");
                Console.WriteLine(latestVersion);
                string[] lb = new string[latestVersion.Split('.').Length - 1];
                Array.Copy(latestVersion.Split('.'), 0, lb, 0, latestVersion.Split('.').Length - 1);
                string lv = string.Join(".", lb);
                Console.WriteLine(lv);
                if (File.Exists(@cfgDATA.location + @"\bedrock_server.exe"))
                {
                    if (Directory.Exists(@cfgDATA.location + @"\behavior_packs\vanilla_" + lv))
                    {
                        Console.WriteLine("最新バージョンです。");
                    }
                    else
                    {
                        MessageBox.Show("サーバーのアップデートがあります。\n「更新」ボタンを押して更新してください。", "BE Server Manager", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception err)
            {
                MessageBox.Show("起動中にエラーが発生しました。\n" + err, "BE Server Manager", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void updateContent(object sender, TextChangedEventArgs e)
        {
            changeLog = 1;
        }

        private void selectionChange(object sender, SelectionChangedEventArgs e)
        {
            changeLog = 1;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (changeLog != 0)
            {
                MessageBoxResult ans = MessageBox.Show("保存していない項目があるようです。\n保存せずに終了してもよろしいですか？", "BE Server Manager", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation);
                Console.WriteLine(ans);
                if (ans != MessageBoxResult.OK)
                {
                    e.Cancel = true;
                }
            }
        }

        async private void updateServer(object sender, RoutedEventArgs e)
        {
            MessageBoxResult ans = MessageBox.Show("アップデートを実行してもよろしいですか？\nアップデート作業中はBE Server Managerは操作できなくなります。\nサーバーが実行されている場合は停止します。", "BE Server Manager", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation);
            Console.WriteLine(ans);
            if (ans != MessageBoxResult.OK)
            {
                return;
            }

            Process[] ps = Process.GetProcessesByName("bedrock_server");

            foreach (Process p in ps)
            {
                p.CloseMainWindow();

                p.WaitForExit(10000);
                if (p.HasExited)
                {
                    Console.WriteLine("Exit!!!");
                }
                else
                {
                    MessageBox.Show("サーバープログラムが終了しませんでした。\n手動で停止しなおすか、しばらく待った後に再度やり直してください。", "BE Server Manager", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
            }
            Directory.CreateDirectory(@AppDomain.CurrentDomain.BaseDirectory + @"\tmp");

            ConfigData cfgDATA = null;
            using (StreamReader file = File.OpenText(@AppDomain.CurrentDomain.BaseDirectory + @"\setting.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                cfgDATA = (ConfigData)serializer.Deserialize(file, typeof(ConfigData));
            }


            FileCopy(@cfgDATA.location + @"\permissions.json", @AppDomain.CurrentDomain.BaseDirectory + @"\permissions.json", true);
            FileCopy(@cfgDATA.location + @"\server.properties", @AppDomain.CurrentDomain.BaseDirectory + @"\server.properties", true);
            FileCopy(@cfgDATA.location + @"\allowlist.json", @AppDomain.CurrentDomain.BaseDirectory + @"\allowlist.json", true);
            FileCopy(@cfgDATA.location + @"\whitelist.json", @AppDomain.CurrentDomain.BaseDirectory + @"\whitelist.json", true);
            DirectoryCopy(@cfgDATA.location + @"\worlds", @AppDomain.CurrentDomain.BaseDirectory + @"\worlds");

            WebClient mywebClient = new WebClient();
            Console.WriteLine("Start task...");
            var dlLink = new Process
            {
                StartInfo = new ProcessStartInfo("python/BE_Server_Manager_MAIN.exe")
                {
                    Arguments = "-getLatest",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            dlLink.Start();
            StreamReader dl = dlLink.StandardOutput;
            downloadLink = dl.ReadLine().Replace("\r", "").Replace("\n", "");
            dlLink.WaitForExit();
            dlLink.Close();
            mywebClient.DownloadFile(downloadLink, @"tmp\Minecraft_BedrockServer.zip");
            DirectoryInfo gamedir = new DirectoryInfo(@cfgDATA.location);
            try
            {
                gamedir.Delete(true);
                Directory.CreateDirectory(@cfgDATA.location);
                ZipFile.ExtractToDirectory(@"tmp\Minecraft_BedrockServer.zip", @cfgDATA.location);
            }
            catch (IOException)
            {
                MessageBox.Show("アップデート中にエラーが発生しました。\nセーブデータのバックアップは「" + @AppDomain.CurrentDomain.BaseDirectory + @"\tmp" + "」内にあります。", "BE Server Manager", MessageBoxButton.OK, MessageBoxImage.Hand);
                return;
            }
            await Task.Delay(1000);
            FileCopy(@AppDomain.CurrentDomain.BaseDirectory + @"\permissions.json", @cfgDATA.location + @"\permissions.json", true);
            FileCopy(@AppDomain.CurrentDomain.BaseDirectory + @"\server.properties", @cfgDATA.location + @"\server.properties", true);
            FileCopy(@AppDomain.CurrentDomain.BaseDirectory + @"\allowlist.json", @cfgDATA.location + @"\allowlist.json", true);
            FileCopy(@AppDomain.CurrentDomain.BaseDirectory + @"\whitelist.json", @cfgDATA.location + @"\whitelist.json", true);
            DirectoryCopy(@AppDomain.CurrentDomain.BaseDirectory + @"\worlds", @cfgDATA.location + @"\worlds");
            File.Delete(@"tmp\Minecraft_BedrockServer.zip");
            MessageBox.Show("アップデートが完了しました。", "BE Server Manager", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        private void startServer(object sender, RoutedEventArgs e)
        {
            ConfigData cfgDATA = null;
            using (StreamReader file = File.OpenText(@AppDomain.CurrentDomain.BaseDirectory + @"\setting.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                cfgDATA = (ConfigData)serializer.Deserialize(file, typeof(ConfigData));
            }
            var Minecraft = new Process
            {
                StartInfo = new ProcessStartInfo(@cfgDATA.location + @"\bedrock_server.exe")
                {
                    UseShellExecute = true,
                    RedirectStandardOutput = false,
                    CreateNoWindow = false
                }
            };
            Minecraft.Start();
        }



        private void saveConfig(object sender, RoutedEventArgs e)
        {
            ConfigData cfgDATA_bf = null;
            using (StreamReader file = File.OpenText(@AppDomain.CurrentDomain.BaseDirectory + @"\setting.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                cfgDATA_bf = (ConfigData)serializer.Deserialize(file, typeof(ConfigData));
            }
            ConfigData cfgDATA = new ConfigData
            {
                name = server_name.Text,
                location = serverLocation.Text,
                seed = @cfgDATA_bf.seed
            };
            string json = JsonConvert.SerializeObject(cfgDATA, Formatting.Indented);
            File.WriteAllText(@AppDomain.CurrentDomain.BaseDirectory + @"\setting.json", json);



            changeLog = 0;
            MessageBox.Show("保存しました。", "BE Server Manager", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
    }
}
