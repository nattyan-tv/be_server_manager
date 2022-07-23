using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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

        public bool setted = true;
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

        public settingUpdate()
        {
            InitializeComponent();
            ConfigData cfgDATA = null;
            using (StreamReader file = File.OpenText(@AppDomain.CurrentDomain.BaseDirectory + @"\setting.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                cfgDATA = (ConfigData)serializer.Deserialize(file, typeof(ConfigData));
            }
            Console.WriteLine(cfgDATA.update);
            if (cfgDATA.autoupdate)
            {
                updateTime.Text = cfgDATA.update;
                updateTime.IsEnabled = true;
                AutoUpdate.IsChecked = true;
            }
            else
            {
                updateTime.IsEnabled = false;
                AutoUpdate.IsChecked = false;
            }
            setted = true;
        }

        private void saveSettings(object sender, RoutedEventArgs e)
        {
            DateTime dt;
            Console.WriteLine(updateTime.Text);

            if (updateTime.Text.Length == 4)
            {
                updateTime.Text = "0" + updateTime.Text;
            }

            if (!DateTime.TryParseExact(updateTime.Text, "HH:mm", null, DateTimeStyles.AssumeLocal, out dt))
            {
                MessageBox.Show("アップデートの時間指定が異常です。", "BE Server Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ConfigData BASEcfgDATA = null;
            using (StreamReader file = File.OpenText(@AppDomain.CurrentDomain.BaseDirectory + @"\setting.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                BASEcfgDATA = (ConfigData)serializer.Deserialize(file, typeof(ConfigData));
            }
            ConfigData cfgDATA = new ConfigData
            {
                name = BASEcfgDATA.name,
                location = @BASEcfgDATA.location,
                seed = BASEcfgDATA.seed,
                update = updateTime.Text,
                backup = BASEcfgDATA.backup,
                backupTime = BASEcfgDATA.backupTime,
                autoupdate = (bool)AutoUpdate.IsChecked,
                autobackup = BASEcfgDATA.autobackup,
                botToken = BASEcfgDATA.botToken,
                botPrefix = BASEcfgDATA.botPrefix
            };
            Console.WriteLine(updateTime.Text);
            Console.WriteLine(AutoUpdate.IsChecked);
            string json = JsonConvert.SerializeObject(cfgDATA, Formatting.Indented);
            File.WriteAllText(@AppDomain.CurrentDomain.BaseDirectory + @"\setting.json", json);
            setted = true;
            Close();
        }

        private void AutoBackup_Clicked(object sender, RoutedEventArgs e)
        {
            updateTime.IsEnabled = !updateTime.IsEnabled;
            setted = false;
        }

        private void updateTime_TextChanged(object sender, TextChangedEventArgs e)
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
    }
}
