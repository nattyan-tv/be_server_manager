using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

        public string PythonExe = @AppDomain.CurrentDomain.BaseDirectory + @"\python3\python.exe";


        private void BackupServerForeground()
        {
            try
            {
                BaseConfig[] cfgDATA = null;
                using (StreamReader file = File.OpenText(@AppDomain.CurrentDomain.BaseDirectory + @"\setting.json"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    cfgDATA = (BaseConfig[])serializer.Deserialize(file, typeof(BaseConfig[]));
                }
                if (!Directory.Exists(@cfgDATA[0].backup))
                {
                    Directory.CreateDirectory(@cfgDATA[0].backup);
                }
                DateTime dt = DateTime.Now;
                DirectoryCopy(@cfgDATA[0].location, @cfgDATA[0].backup + @"\" + @dt.ToString("yyyy_MM_dd-HH_mm_ss"));
                MessageBox.Show("バックアップが完了しました。\n\n・場所\n" + @cfgDATA[0].backup + @"\" + @dt.ToString("yyyy_MM_dd-HH_mm_ss"), "BE Server Manager", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            catch (Exception err)
            {
                MessageBox.Show("バックアップ中にエラーが発生しました。\n" + err, "BE Server Manager", MessageBoxButton.OK, MessageBoxImage.Hand);
            }
            return;

        }

        private void BackupServerBackground()
        {
            try
            {
                BaseConfig[] cfgDATA = null;
                using (StreamReader file = File.OpenText(@AppDomain.CurrentDomain.BaseDirectory + @"\setting.json"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    cfgDATA = (BaseConfig[])serializer.Deserialize(file, typeof(BaseConfig[]));
                }
                if (!Directory.Exists(@cfgDATA[0].backup))
                {
                    Directory.CreateDirectory(@cfgDATA[0].backup);
                }
                DateTime dt = DateTime.Now;
                DirectoryCopy(@cfgDATA[0].location, @cfgDATA[0].backup + @"\" + @dt.ToString("yyyy_MM_dd-HH_mm_ss"));
                Console.WriteLine("バックアップが完了しました。\n\n・場所\n" + @cfgDATA[0].location, @cfgDATA[0].backup + @"\" + @dt.ToString("yyyy_MM_dd-HH_mm_ss"));
                return;
            }
            catch (Exception err)
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

        public async static void CopyToCache()
        {
            /// Copy private files to cache.
            await Task.Run(() =>
            {
                BaseConfig[] cfgDATA = null;
                using (StreamReader file = File.OpenText(@AppDomain.CurrentDomain.BaseDirectory + @"\setting.json"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    cfgDATA = (BaseConfig[])serializer.Deserialize(file, typeof(BaseConfig[]));
                }

                if (Directory.Exists(@AppDomain.CurrentDomain.BaseDirectory + @"\temp") == false)
                {
                    Directory.CreateDirectory(@AppDomain.CurrentDomain.BaseDirectory + @"\temp");
                }
                FileCopy(@cfgDATA[0].location + @"\permissions.json", @AppDomain.CurrentDomain.BaseDirectory + @"\temp\permissions.json", true);
                FileCopy(@cfgDATA[0].location + @"\server.properties", @AppDomain.CurrentDomain.BaseDirectory + @"\temp\server.properties", true);
                FileCopy(@cfgDATA[0].location + @"\allowlist.json", @AppDomain.CurrentDomain.BaseDirectory + @"\temp\allowlist.json", true);
                FileCopy(@cfgDATA[0].location + @"\whitelist.json", @AppDomain.CurrentDomain.BaseDirectory + @"\temp\whitelist.json", true);
                FileCopy(@cfgDATA[0].location + @"\valid_known_packs.json", @AppDomain.CurrentDomain.BaseDirectory + @"\temp\valid_known_packs.json", true);
                DirectoryCopy(@cfgDATA[0].location + @"\worlds", @AppDomain.CurrentDomain.BaseDirectory + @"\temp\worlds");
                DirectoryCopy(@cfgDATA[0].location + @"\world_templates", @AppDomain.CurrentDomain.BaseDirectory + @"\temp\world_templates");
                DirectoryCopy(@cfgDATA[0].location + @"\development_behavior_packs", @AppDomain.CurrentDomain.BaseDirectory + @"\temp\development_behavior_packs");
                DirectoryCopy(@cfgDATA[0].location + @"\development_resource_packs", @AppDomain.CurrentDomain.BaseDirectory + @"\temp\development_resource_packs");
                DirectoryCopy(@cfgDATA[0].location + @"\development_skin_packs", @AppDomain.CurrentDomain.BaseDirectory + @"\temp\development_skin_packs");
            });
        }

        public async static void CopyFromCache()
        {
            /// Copy private files from cache.
            await Task.Run(() =>
            {
                BaseConfig[] cfgDATA = null;
                using (StreamReader file = File.OpenText(@AppDomain.CurrentDomain.BaseDirectory + @"\setting.json"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    cfgDATA = (BaseConfig[])serializer.Deserialize(file, typeof(BaseConfig[]));
                }
                FileCopy(@AppDomain.CurrentDomain.BaseDirectory + @"\temp\permissions.json", @cfgDATA[0].location + @"\permissions.json", true);
                FileCopy(@AppDomain.CurrentDomain.BaseDirectory + @"\temp\server.properties", @cfgDATA[0].location + @"\server.properties", true);
                FileCopy(@AppDomain.CurrentDomain.BaseDirectory + @"\temp\allowlist.json", @cfgDATA[0].location + @"\allowlist.json", true);
                FileCopy(@AppDomain.CurrentDomain.BaseDirectory + @"\temp\whitelist.json", @cfgDATA[0].location + @"\whitelist.json", true);
                FileCopy(@AppDomain.CurrentDomain.BaseDirectory + @"\temp\valid_known_packs.json", @cfgDATA[0].location + @"\valid_known_packs.json", true);
                DirectoryCopy(@AppDomain.CurrentDomain.BaseDirectory + @"\temp\worlds", @cfgDATA[0].location + @"\worlds");
                DirectoryCopy(@AppDomain.CurrentDomain.BaseDirectory + @"\temp\world_templates", @cfgDATA[0].location + @"\world_templates");
                DirectoryCopy(@AppDomain.CurrentDomain.BaseDirectory + @"\temp\development_behavior_packs", @cfgDATA[0].location + @"\development_behavior_packs");
                DirectoryCopy(@AppDomain.CurrentDomain.BaseDirectory + @"\temp\development_resource_packs", @cfgDATA[0].location + @"\development_resource_packs");
                DirectoryCopy(@AppDomain.CurrentDomain.BaseDirectory + @"\temp\development_skin_packs", @cfgDATA[0].location + @"\development_skin_packs");
            });

        }

        public static void AllBackUp()
        {

        }

        public void LoadServerSetting(string fileLocation)
        {
            /// Load setting file and place.

            StreamReader file = new StreamReader(@fileLocation);
            string[] content = file.ReadToEnd().Replace("\r\n", "\n").Split(new[] { '\n', '\r' });
            file.Close();
            foreach (string item in content)
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
                "level-name",
                "allow-list"
            };
            if (!File.Exists(fileLocation))
            {
                StreamWriter bfile = new StreamWriter(@fileLocation, false);
                bfile.WriteLine("");
                bfile.Close();
            }
            StreamReader rfile = new StreamReader(@fileLocation);
            string all_content = rfile.ReadToEnd();
            string[] content = all_content.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            rfile.Close();
            List<string> contents = new List<string>(content);
            foreach (string item in contents)
            {
                if (item.IndexOf("=") != -1 && !item.StartsWith("#") && items.Contains(item.Split('=')[0]))
                {
                    string settingItem = item.Split('=')[0].Replace("-", "_").Replace(".", "_");
                    TextBox tb = FindName("config_" + settingItem) as TextBox;
                    ComboBox cb = FindName("config_" + settingItem) as ComboBox;
                    if (tb != null)
                    {
                        string replaceText = settingItem.Replace("_", "-") + "=" + tb.Text;
                        all_content = all_content.Replace(item, replaceText);
                        Console.WriteLine("TB: " + item);
                        items.Remove(settingItem.Replace("_", "-"));
                    }
                    else if (cb != null)
                    {
                        string replaceText = settingItem.Replace("_", "-") + "=" + cb.Text;
                        all_content = all_content.Replace(item, replaceText);
                        Console.WriteLine("CB: " + item);
                        items.Remove(settingItem.Replace("_", "-"));
                    }
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
                        all_content += "\n" + settingItem.Replace("_", "-") + "=" + tb.Text;
                    }
                    else if (cb != null)
                    {
                        Console.WriteLine("CB: " + item);
                        all_content += "\n" + settingItem.Replace("_", "-") + "=" + cb.Text;
                    }
                }
            }
            all_content = all_content.Replace("\r\n", "\n");
            StreamWriter wfile = new StreamWriter(@fileLocation, false);
            wfile.WriteLine(all_content);
            wfile.Close();
        }

        private void Initview(BaseConfig[] cfgDATA)
        {
            /// 画面の更新をする
            try
            {
                Console.WriteLine("Start task...");
                var dlLink = new Process
                {
                    StartInfo = new ProcessStartInfo(PythonExe)
                    {
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true,
                        Arguments = AppDomain.CurrentDomain.BaseDirectory + "/python/getLatestVersion.py"
                    }
                };
                dlLink.Start();
                StreamReader dl = dlLink.StandardOutput;
                downloadLink = dl.ReadLine().Replace("\r", "").Replace("\n", "");
                dlLink.WaitForExit();
                dlLink.Close();
                Console.WriteLine("Connection all rights.\n" + downloadLink);
                latestVersion = downloadLink.Replace("https://minecraft.azureedge.net/bin-win/bedrock-server-", "").Replace(".zip", "");
                Console.WriteLine("配信されている最新バージョン:" + latestVersion);
                string[] lb = new string[latestVersion.Split('.').Length - 1];
                Array.Copy(latestVersion.Split('.'), 0, lb, 0, latestVersion.Split('.').Length - 1);
                string lv = string.Join(".", lb);
                if (File.Exists(@cfgDATA[0].location + @"\bedrock_server.exe"))
                {
                    LoadServerSetting(@cfgDATA[0].location + @"\server.properties");
                    launchButton.IsEnabled = true;
                    serverConfigMainpanel.IsEnabled = true;

                    サーバー名.Content = "サーバー名：" + cfgDATA[0].name;
                    config_server_name.Text = cfgDATA[0].name;
                    serverLocation.Text = cfgDATA[0].location;
                    config_level_seed.Text = cfgDATA[0].seed;
                    if (File.Exists(@cfgDATA[0].location + @"\version.txt"))
                    {
                        using (StreamReader sr = new StreamReader(
                            @cfgDATA[0].location + @"\version.txt", Encoding.GetEncoding("UTF-8")))
                        {
                            currentVersion = sr.ReadToEnd().Replace("\r", "").Replace("\n", "");
                        }
                        Console.WriteLine("Latest Version: " + latestVersion);
                        Console.WriteLine("Current Version: " + currentVersion);
                        バージョン.Content = "バージョン：" + currentVersion;
                        if (latestVersion == currentVersion) { Console.WriteLine("最新バージョンです。"); }
                        else { MessageBox.Show("サーバーのアップデートがあります。\n「更新」ボタンを押して更新してください。", "BE Server Manager", MessageBoxButton.OK, MessageBoxImage.Information); }
                    }
                    else
                    {
                        バージョン.Content = "バージョン不明";
                        MessageBox.Show("バージョンファイルが見つかりませんでした。\n現在のバージョンは不明ですが「更新」ボタンを押すことで、最新バージョンへのアップデート及びバージョンファイルの作成が行われます。", "BE Server Manager", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    updateButton.Content = "インストール";
                    サーバー名.Content = "BE Server Manager";
                    バージョン.Content = "サーバーがインストールされていません。";
                    serverLocation.Text = cfgDATA[0].location;
                    serverConfigMainpanel.IsEnabled = false;
                }
            }
            catch (Exception err)
            {
                MessageBox.Show($"画面更新にエラーが発生しました。\n下記エラーログを開発者に見せると、何かを教えてくれるかもしれません。\n{err.Message}\n{err.StackTrace}\n\n{err.Data}", "BE Server Manager", MessageBoxButton.OK, MessageBoxImage.Error);
                Console.WriteLine($"Error handle (When refresh)\n{err.Message}\n{err.StackTrace}\n\n{err.Data}");
                Close();
            }
        }

        /// MainWindow Program
        /// This program (below this comment) will execution when Program launced.

        public MainWindow()
        {
            Console.WriteLine("Waiting initializing compotens...");
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
                BaseConfig[] cfgDATA = null;
                using (StreamReader file = File.OpenText(@AppDomain.CurrentDomain.BaseDirectory + @"\setting.json"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    cfgDATA = (BaseConfig[])serializer.Deserialize(file, typeof(BaseConfig[]));
                }

                Console.WriteLine("Settings are loaded.");

                if (!Directory.Exists(@cfgDATA[0].location)) { Directory.CreateDirectory(@cfgDATA[0].location); }

                Microsoft.Win32.RegistryKey regkey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                Console.WriteLine(regkey.GetValue("BEServerManagerBG"));
                if (regkey.GetValue("BEServerManagerBG") == null)
                {
                    regkey.SetValue("BEServerManagerBG", "\"" + @AppDomain.CurrentDomain.BaseDirectory + "\\python3\\python.exe\" \"" + @AppDomain.CurrentDomain.BaseDirectory + "\\python\\be_server_manager_bg.py\"");
                    regkey.Close();
                }

                var bsm_bg = new Process
                {
                    StartInfo = new ProcessStartInfo(PythonExe)
                    {
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true,
                        Arguments = @AppDomain.CurrentDomain.BaseDirectory + @"\python\be_server_manager_bg.py",
                    }
                };
                bsm_bg.Start();

                Initview(cfgDATA);
                changeLog = 0;
            }
            catch (Exception err)
            {
                MessageBox.Show($"起動中にエラーが発生しました。\n下記エラーログを開発者に見せると、何かを教えてくれるかもしれません。\n{err.Message}\n{err.StackTrace}\n\n{err.Data}", "BE Server Manager", MessageBoxButton.OK, MessageBoxImage.Error);
                Console.WriteLine($"Error handle (When launch)\n{err.Message}\n{err.StackTrace}\n\n{err.Data}");
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
                if (cofd.ShowDialog() != CommonFileDialogResult.Ok) { return; }
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
                if (ans != MessageBoxResult.OK) { e.Cancel = true; }
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

            BaseConfig[] cfgDATA = null;
            using (StreamReader file = File.OpenText(@AppDomain.CurrentDomain.BaseDirectory + @"\setting.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                cfgDATA = (BaseConfig[])serializer.Deserialize(file, typeof(BaseConfig[]));
            }

            CopyToCache();


            WebClient mywebClient = new WebClient();
            Console.WriteLine("Start task...");
            var dlLink = new Process
            {
                StartInfo = new ProcessStartInfo(PythonExe)
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    Arguments = "python/getLatestVersion.py"
                }
            };
            dlLink.Start();
            StreamReader dl = dlLink.StandardOutput;
            downloadLink = dl.ReadLine().Replace("\r", "").Replace("\n", "");
            dlLink.WaitForExit();
            dlLink.Close();
            Console.WriteLine("Download link: " + downloadLink);
            latestVersion = downloadLink.Replace("https://minecraft.azureedge.net/bin-win/bedrock-server-", "").Replace(".zip", "");
            Console.WriteLine("Latest version: " + latestVersion);
            IsEnabled = false;

            await Task.Run(async () =>
            {
                mywebClient.DownloadFile(downloadLink, @"tmp\Minecraft_BedrockServer.zip");
                DirectoryInfo gamedir = new DirectoryInfo(@cfgDATA[0].location);
                try
                {
                    gamedir.Delete(true);
                    Directory.CreateDirectory(@cfgDATA[0].location);
                    ZipFile.ExtractToDirectory(@"tmp\Minecraft_BedrockServer.zip", @cfgDATA[0].location);
                }
                catch (IOException)
                {
                    MessageBox.Show("アップデート中にエラーが発生しました。\nセーブデータのバックアップは「" + @AppDomain.CurrentDomain.BaseDirectory + @"\tmp" + "」内にあります。", "BE Server Manager", MessageBoxButton.OK, MessageBoxImage.Hand);
                    return;
                }

                Encoding utf8 = Encoding.GetEncoding("UTF-8");
                using (StreamWriter writer = new StreamWriter(@cfgDATA[0].location + @"\version.txt", false, utf8))
                {
                    writer.WriteLine(latestVersion);
                }

                await Task.Delay(1000);
                CopyFromCache();
                File.Delete(@"tmp\Minecraft_BedrockServer.zip");
                MessageBox.Show("アップデートが完了しました。", "BE Server Manager", MessageBoxButton.OK, MessageBoxImage.Information);
                Dispatcher.Invoke(() =>
                {
                    IsEnabled = true;
                    Initview(cfgDATA);
                });
                return;
            });


        }

        private void startServer(object sender, RoutedEventArgs e)
        {
            if (changeLog != 0)
            {
                MessageBoxResult ans_c = MessageBox.Show("保存していない項目があるようです。\n現在の設定を適応するには保存する必要があります。\n保存せずに続行しますか？", "BE Server Manager", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation);
                Console.WriteLine(ans_c);
                if (ans_c != MessageBoxResult.OK) { return; }
            }
            BaseConfig[] cfgDATA = null;
            using (StreamReader file = File.OpenText(@AppDomain.CurrentDomain.BaseDirectory + @"\setting.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                cfgDATA = (BaseConfig[])serializer.Deserialize(file, typeof(BaseConfig[]));
            }
            var Minecraft = new Process
            {
                StartInfo = new ProcessStartInfo(@cfgDATA[0].location + @"\bedrock_server.exe")
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
            try
            {
                BaseConfig[] cfgDATA_bf = null;
                using (StreamReader file = File.OpenText(@AppDomain.CurrentDomain.BaseDirectory + @"\setting.json"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    cfgDATA_bf = (BaseConfig[])serializer.Deserialize(file, typeof(BaseConfig[]));
                }
                BaseConfig[] cfgDATA = {new BaseConfig{
                    name = config_server_name.Text,
                    location = serverLocation.Text,
                    seed = config_level_seed.Text,
                    update = @cfgDATA_bf[0].update,
                    backup = @cfgDATA_bf[0].backup,
                    backupTime = @cfgDATA_bf[0].backupTime,
                    autoupdate = @cfgDATA_bf[0].autoupdate,
                    autobackup = @cfgDATA_bf[0].autobackup,
                    botToken = @cfgDATA_bf[0].botToken,
                    botPrefix = @cfgDATA_bf[0].botPrefix
                }};

                サーバー名.Content = "サーバー名：" + config_server_name.Text;
                string json = JsonConvert.SerializeObject(cfgDATA, Formatting.Indented);
                File.WriteAllText(@AppDomain.CurrentDomain.BaseDirectory + @"\setting.json", json);

                SaveServerSetting(serverLocation.Text + @"\server.properties");

                if (File.Exists(@cfgDATA[0].location + @"\bedrock_server.exe"))
                {
                    launchButton.IsEnabled = true;
                }

                changeLog = 0;
                MessageBox.Show("保存しました。", "BE Server Manager", MessageBoxButton.OK, MessageBoxImage.Information);
                Initview(cfgDATA);
                return;
            }
            catch (Exception err)
            {
                MessageBox.Show("保存中にエラーが発生しました。\n\n" + err, "BE Server Manager", MessageBoxButton.OK, MessageBoxImage.Hand);
                return;
            }
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
@nattyan-tv (NattyanTV)

This application is not provided by Microsoft or Mojang in any way and has been developed independently.

Copyright © 2001- Python Software Foundation; All Rights Reserved", "BE Server Manager", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void openConfigDiscord(object sender, RoutedEventArgs e)
        {
            settingDiscord window = new settingDiscord();
            window.ShowDialog();
        }

        private void launchDiscordBOT(object sender, RoutedEventArgs e)
        {
            var DiscordBOT = new Process
            {
                StartInfo = new ProcessStartInfo(PythonExe)
                {
                    Arguments = "python/DiscordBot.py",
                    UseShellExecute = true,
                    RedirectStandardOutput = false,
                    CreateNoWindow = false
                }
            };
            DiscordBOT.Start();
        }

        private void openConfigWebhook(object sender, RoutedEventArgs e)
        {
            settingWebhook window = new settingWebhook();
            window.ShowDialog();
        }
    }
}
