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
using static bedrock_server_manager.BaseConfig;

namespace bedrock_server_manager
{
    /// <summary>
    /// settingUpdate.xaml の相互作用ロジック
    /// </summary>
    public partial class settingUpdate : Window
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

        public settingUpdate()
        {
            InitializeComponent();
            BaseConfig[] cfgDATA = null;
            using (StreamReader file = File.OpenText(@AppDomain.CurrentDomain.BaseDirectory + @"\setting.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                cfgDATA = (BaseConfig[])serializer.Deserialize(file, typeof(BaseConfig[]));
            }
            Console.WriteLine(cfgDATA[0].update);
            if (cfgDATA[0].autoupdate)
            {
                updateTime.Text = cfgDATA[0].update;
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
            Console.WriteLine(updateTime.Text);

            BaseConfig[] BASEcfgDATA = null;
            using (StreamReader file = File.OpenText(@AppDomain.CurrentDomain.BaseDirectory + @"\setting.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                BASEcfgDATA = (BaseConfig[])serializer.Deserialize(file, typeof(BaseConfig[]));
            }
            BaseConfig[] cfgDATA = { new BaseConfig
            {
                name = BASEcfgDATA[0].name,
                location = @BASEcfgDATA[0].location,
                seed = BASEcfgDATA[0].seed,
                update = updateTime.Text,
                backup = BASEcfgDATA[0].backup,
                backupTime = BASEcfgDATA[0].backupTime,
                autoupdate = (bool)AutoUpdate.IsChecked,
                autobackup = BASEcfgDATA[0].autobackup,
                botToken = BASEcfgDATA[0].botToken,
                botPrefix = BASEcfgDATA[0].botPrefix
            } };
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
