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
using Microsoft.CodeAnalysis.CSharp.Scripting;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Threading;

/// 
/// BE Server Manager
/// Powered by: nattyan-tv
/// 
/// The C# Based Minecraft: Bedrock Edition Server Manager.
/// (And Python using.)
///


namespace bedrock_server_manager
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    /// 
    
    public partial class MainWindow : Window
    {
        /// ダウンロードページ: https://www.minecraft.net/en-us/download/server/bedrock
        /// 最新バージョン: https://minecraft.azureedge.net/bin-win/bedrock-server-XXXXX.zip
        /// サーバーバージョン取得: behavior_packs/vanilla_XXXXX
        ///
        /// MinecraftBedrockサーバーのダウンロード用ページリンク変数
        public string belink = "https://www.minecraft.net/en-us/download/server/bedrock";

        public string ApplicationVersion = @"v0.9.1";

        public int changeLog = 0;
        public string downloadLink = "";
        public string currentVersion = "";
        public string latestVersion = "";

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

        private void BackupServerForeground(){
            try
            {
                ConfigData cfgDATA = null;
                using (StreamReader file = File.OpenText(@AppDomain.CurrentDomain.BaseDirectory + @"\setting.json"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    cfgDATA = (ConfigData)serializer.Deserialize(file, typeof(ConfigData));
                }
                if(!Directory.Exists(@cfgDATA.backup))
                {
                    Directory.CreateDirectory(@cfgDATA.backup);
                }
                DateTime dt = DateTime.Now;
                DirectoryCopy(@cfgDATA.location, @cfgDATA.backup + @"\" + @dt.ToString("yyyy_MM_dd-HH_mm_ss"));
                MessageBox.Show("バックアップが完了しました。\n\n・場所\n" + @cfgDATA.backup + @"\" + @dt.ToString("yyyy_MM_dd-HH_mm_ss"), "BE Server Manager", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            catch(Exception err)
            {
                MessageBox.Show("バックアップ中にエラーが発生しました。\n" + err, "BE Server Manager", MessageBoxButton.OK, MessageBoxImage.Hand);
            }
            return;
            
        }

        private void BackupServerBackground(){
            try
            {
                ConfigData cfgDATA = null;
                using (StreamReader file = File.OpenText(@AppDomain.CurrentDomain.BaseDirectory + @"\setting.json"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    cfgDATA = (ConfigData)serializer.Deserialize(file, typeof(ConfigData));
                }
                if(!Directory.Exists(@cfgDATA.backup))
                {
                    Directory.CreateDirectory(@cfgDATA.backup);
                }
                DateTime dt = DateTime.Now;
                DirectoryCopy(@cfgDATA.location, @cfgDATA.backup + @"\" + @dt.ToString("yyyy_MM_dd-HH_mm_ss"));
                Console.WriteLine("バックアップが完了しました。\n\n・場所\n" + @cfgDATA.location, @cfgDATA.backup + @"\" + @dt.ToString("yyyy_MM_dd-HH_mm_ss"));
                return;
            }
            catch(Exception err)
            {
                Console.WriteLine("バックアップ中にエラーが発生しました。\n" + err);
            }
            return;
            
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

            if (sourceDirectory.Exists == false) { return; }

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

        public async static void CopyToCache(){
            /// Copy private files to cache.
            await Task.Run(() =>
            {
                ConfigData cfgDATA = null;
                using (StreamReader file = File.OpenText(@AppDomain.CurrentDomain.BaseDirectory + @"\setting.json"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    cfgDATA = (ConfigData)serializer.Deserialize(file, typeof(ConfigData));
                }

                if (Directory.Exists(@AppDomain.CurrentDomain.BaseDirectory + @"\temp") == false){
                    Directory.CreateDirectory(@AppDomain.CurrentDomain.BaseDirectory + @"\temp");
                }
                FileCopy(@cfgDATA.location + @"\permissions.json", @AppDomain.CurrentDomain.BaseDirectory + @"\temp\permissions.json", true);
                FileCopy(@cfgDATA.location + @"\server.properties", @AppDomain.CurrentDomain.BaseDirectory + @"\temp\server.properties", true);
                FileCopy(@cfgDATA.location + @"\allowlist.json", @AppDomain.CurrentDomain.BaseDirectory + @"\temp\allowlist.json", true);
                FileCopy(@cfgDATA.location + @"\whitelist.json", @AppDomain.CurrentDomain.BaseDirectory + @"\temp\whitelist.json", true);
                FileCopy(@cfgDATA.location + @"\valid_known_packs.json", @AppDomain.CurrentDomain.BaseDirectory + @"\temp\valid_known_packs.json", true);
                DirectoryCopy(@cfgDATA.location + @"\worlds", @AppDomain.CurrentDomain.BaseDirectory + @"\temp\worlds");
                DirectoryCopy(@cfgDATA.location + @"\world_templates", @AppDomain.CurrentDomain.BaseDirectory + @"\temp\world_templates");
                DirectoryCopy(@cfgDATA.location + @"\development_behavior_packs", @AppDomain.CurrentDomain.BaseDirectory + @"\temp\development_behavior_packs");
                DirectoryCopy(@cfgDATA.location + @"\development_resource_packs", @AppDomain.CurrentDomain.BaseDirectory + @"\temp\development_resource_packs");
                DirectoryCopy(@cfgDATA.location + @"\development_skin_packs", @AppDomain.CurrentDomain.BaseDirectory + @"\temp\development_skin_packs");
            }
        }

        public async static void CopyFromCache(){
            /// Copy private files from cache.
            await Task.Run(() =>
            {
                ConfigData cfgDATA = null;
                using (StreamReader file = File.OpenText(@AppDomain.CurrentDomain.BaseDirectory + @"\setting.json"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    cfgDATA = (ConfigData)serializer.Deserialize(file, typeof(ConfigData));
                }
                FileCopy(@AppDomain.CurrentDomain.BaseDirectory + @"\temp\permissions.json", @cfgDATA.location + @"\permissions.json", true);
                FileCopy(@AppDomain.CurrentDomain.BaseDirectory + @"\temp\server.properties", @cfgDATA.location + @"\server.properties", true);
                FileCopy(@AppDomain.CurrentDomain.BaseDirectory + @"\temp\allowlist.json", @cfgDATA.location + @"\allowlist.json", true);
                FileCopy(@AppDomain.CurrentDomain.BaseDirectory + @"\temp\whitelist.json", @cfgDATA.location + @"\whitelist.json", true);
                FileCopy(@AppDomain.CurrentDomain.BaseDirectory + @"\temp\valid_known_packs.json", @cfgDATA.location + @"\valid_known_packs.json", true);
                DirectoryCopy(@AppDomain.CurrentDomain.BaseDirectory + @"\temp\worlds", @cfgDATA.location + @"\worlds");
                DirectoryCopy(@AppDomain.CurrentDomain.BaseDirectory + @"\temp\world_templates", @cfgDATA.location + @"\world_templates");
                DirectoryCopy(@AppDomain.CurrentDomain.BaseDirectory + @"\temp\development_behavior_packs", @cfgDATA.location + @"\development_behavior_packs");
                DirectoryCopy(@AppDomain.CurrentDomain.BaseDirectory + @"\temp\development_resource_packs", @cfgDATA.location + @"\development_resource_packs");
                DirectoryCopy(@AppDomain.CurrentDomain.BaseDirectory + @"\temp\development_skin_packs", @cfgDATA.location + @"\development_skin_packs");
            });

        }

        public static void AllBackUp()
        {

        }

        public void LoadServerSetting(string fileLocation)
        {
            /// Load setting file and place.
            
            StreamReader file = new StreamReader(@fileLocation);
            string[] content = file.ReadToEnd().Replace("\r\n","\n").Split(new[] {'\n','\r'});
            file.Close();
            foreach(string item in content)
            {

                if (item.IndexOf("=") != -1 && !item.StartsWith("#"))
                {
                    string settingItem = item.Split('=')[0].Replace("-", "_").Replace(".", "_");
                    string settingValue = item.Split('=')[1];

                    TextBox tb = FindName("config_" + settingItem) as TextBox;
                    ComboBox cb = FindName("config_" + settingItem) as ComboBox;
                    if (tb != null)
                    {
                        Console.WriteLine("TB: " + item);
                        tb.Text = settingValue;
                    }
                    else if (cb != null)
                    {
                        Console.WriteLine("CB: " + item);
                        cb.Text = settingValue;
                    }
                    else
                    {
                        Console.WriteLine("Unsupported: " + item);
                    }

                }
            }
        }



        public void SaveServerSetting(string fileLocation)
        {
            /// Save file
            
            var items = new List<string>(){
                "server-name",
                "gamemode",
                "view-distance",
                "pvp",
                "difficulty",
                "max-players",
                "server-port",
                "server-portv6",
                "level-seed",
                "player-idle-timeout",
                "max-threads",
                "tick-distance",
                "allow-cheats",
                "white-list",
                "online-mode",
                "content-log-file-enabled",
                "default-player-permission-level",
                "texturepack-required",
                "compression-threshold",
                "correct-player-movement",
                "server-authoritative-movement",
                "player-movement-distance-threshold",
                "player-movement-duration-threshold-in-ms",
                "player-movement-score-threshold",
                "level-name"
            };
            if (!File.Exists(fileLocation))
            {
                StreamWriter bfile = new StreamWriter(@fileLocation, false);
                bfile.WriteLine("");
                bfile.Close();
            }
            StreamReader rfile = new StreamReader(@fileLocation);
            string all_content = rfile.ReadToEnd();
            string[] content = rfile.ReadToEnd().Replace("\r\n", "\n").Split(new[] { '\n', '\r' });
            rfile.Close();
            foreach (string item in content)
            {

                if (item.IndexOf("=") != -1 && !item.StartsWith("#") && items.Contains(item))
                {
                    string settingItem = item.Split('=')[0].Replace("-", "_").Replace(".", "_");

                    TextBox tb = FindName("config_" + settingItem) as TextBox;
                    ComboBox cb = FindName("config_" + settingItem) as ComboBox;
                    if (tb != null)
                    {
                        Console.WriteLine("TB: " + item);
                        all_content.Replace(item, settingItem + "=" + tb.Text);
                    }
                    else if (cb != null)
                    {
                        Console.WriteLine("CB: " + item);
                        all_content.Replace(item, settingItem + "=" + cb.Text);
                    }
                    items.Remove(settingItem);
                }
            }
            if (items.Count() != 0)
            {
                foreach (string item in items)
                {
                    string settingItem = item.Split('=')[0].Replace("-", "_").Replace(".", "_");

                    TextBox tb = FindName("config_" + settingItem) as TextBox;
                    ComboBox cb = FindName("config_" + settingItem) as ComboBox;
                    if (tb != null)
                    {
                        Console.WriteLine("TB: " + item);
                        all_content += "\n" + settingItem + "=" + tb.Text;
                    }
                    else if (cb != null)
                    {
                        Console.WriteLine("CB: " + item);
                        all_content += "\n" + settingItem + "=" + cb.Text;
                    }
                }
            }

            StreamWriter wfile = new StreamWriter(@fileLocation, false);
            wfile.WriteLine(all_content);
            wfile.Close();
        }



        /// MainWindow Program
        /// This program (below this comment) will execution when Program launced.

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

                Console.WriteLine("Settings are loaded.");

                if (!Directory.Exists(@cfgDATA.location)){ Directory.CreateDirectory(@cfgDATA.location); }

                Console.WriteLine("Start task...");
                var dlLink = new Process
                {
                    StartInfo = new ProcessStartInfo("python/getLatestVersion.exe")
                    {
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
                最新バージョン.Content = "配信中バージョン：" + lv;
                if (File.Exists(@cfgDATA.location + @"\bedrock_server.exe"))
                {
                    LoadServerSetting(@cfgDATA.location + @"\server.properties");
                    launchButton.IsEnabled = true;

                    サーバー名.Content = "サーバー名：" + cfgDATA.name;
                    config_server_name.Text = cfgDATA.name;
                    serverLocation.Text = cfgDATA.location;
                    config_level_seed.Text = cfgDATA.seed;
                    if (Directory.Exists(@cfgDATA.location + @"\behavior_packs\vanilla_" + lv)){ Console.WriteLine("最新バージョンです。"); }
                    else{ MessageBox.Show("サーバーのアップデートがあります。\n「更新」ボタンを押して更新してください。", "BE Server Manager", MessageBoxButton.OK, MessageBoxImage.Information); }
                }
                changeLog = 0;
            }
            catch (Exception err)
            {
                MessageBox.Show("起動中にエラーが発生しました。\n下記エラーログを開発者に見せると、何かを教えてくれるかもしれません。\n\n" + err, "BE Server Manager", MessageBoxButton.OK, MessageBoxImage.Error);
                Console.WriteLine("Sorry. An error has occurred.\n\n" + err);
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
                if (cofd.ShowDialog() != CommonFileDialogResult.Ok){ return; }
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
                if (ans != MessageBoxResult.OK){ e.Cancel = true; }
            }
        }

        private void Window_Closing(object sender, RoutedEventArgs e)
        {
            Close();
        }

        async private void updateServer(object sender, RoutedEventArgs e)
        {
            if (changeLog != 0)
            {
                MessageBoxResult ans_c = MessageBox.Show("保存していない項目があるようです。\n保存せずに続行してもよろしいですか？", "BE Server Manager", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation);
                Console.WriteLine(ans_c);
                if (ans_c != MessageBoxResult.OK) { return; }
            }
            MessageBoxResult ans = MessageBox.Show("アップデートを実行してもよろしいですか？\nアップデート作業中はBE Server Managerは操作できなくなります。\nサーバーが実行されている場合は停止します。", "BE Server Manager", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation);
            Console.WriteLine(ans);
            if (ans != MessageBoxResult.OK) { return; }

            Process[] ps = Process.GetProcessesByName("bedrock_server");

            foreach (Process p in ps)
            {
                p.CloseMainWindow();

                p.WaitForExit(10000);
                if (p.HasExited) { Console.WriteLine("Exit!!!"); }
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

            CopyToCache();


            WebClient mywebClient = new WebClient();
            Console.WriteLine("Start task...");
            var dlLink = new Process
            {
                StartInfo = new ProcessStartInfo("python/getLatestVersion.exe")
                {
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
            CopyFromCache();
            File.Delete(@"tmp\Minecraft_BedrockServer.zip");
            MessageBox.Show("アップデートが完了しました。", "BE Server Manager", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        private void startServer(object sender, RoutedEventArgs e)
        {
            if (changeLog != 0)
            {
                MessageBoxResult ans_c = MessageBox.Show("保存していない項目があるようです。\n現在の設定を適応するには保存する必要があります。\n保存せずに続行しますか？", "BE Server Manager", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation);
                Console.WriteLine(ans_c);
                if (ans_c != MessageBoxResult.OK) { return; }
            }
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
                name = config_server_name.Text,
                location = serverLocation.Text,
                seed = @cfgDATA_bf.seed
            };
            サーバー名.Content = "サーバー名：" + config_server_name.Text;
            string json = JsonConvert.SerializeObject(cfgDATA, Formatting.Indented);
            File.WriteAllText(@AppDomain.CurrentDomain.BaseDirectory + @"\setting.json", json);

            SaveServerSetting(serverLocation.Text + @"\server.properties");

            if (File.Exists(@cfgDATA.location + @"\bedrock_server.exe"))
            {
                launchButton.IsEnabled = true;
            }

            changeLog = 0;
            MessageBox.Show("保存しました。", "BE Server Manager", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        private void openConfigBackup(object sender, RoutedEventArgs e)
        {
            settingBackup window = new settingBackup();
            window.ShowDialog();
        }

        private void openConfigUpdate(object sender, RoutedEventArgs e)
        {
            settingUpdate window = new settingUpdate();
            window.ShowDialog();
        }

        private void openInfo(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(@"BE Server Manager
Minecraft Vanilla Dedicated Server Manager for Bedrock Edition
Version: " + ApplicationVersion + @"
Developed by NattyanTV

This application is not provided by Microsoft or Mojang in any way and has been developed independently.", "BE Server Manager", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void openConfigDiscord(object sender, RoutedEventArgs e)
        {
            settingDiscord window = new settingDiscord();
            window.ShowDialog();
        }
    }
}
