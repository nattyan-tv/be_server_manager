﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
    /// settingDiscord.xaml の相互作用ロジック
    /// </summary>
    public partial class settingDiscord : Window
    {
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

        public bool setted = false;

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.ToString());
        }

        public settingDiscord()
        {
            InitializeComponent();
            ConfigData cfgDATA = null;
            using (StreamReader file = File.OpenText(@AppDomain.CurrentDomain.BaseDirectory + @"\setting.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                cfgDATA = (ConfigData)serializer.Deserialize(file, typeof(ConfigData));
            }
            bottoken.Password = cfgDATA.botToken;
            botprefix.Text = cfgDATA.botPrefix;
            setted = false;
        }

        private void BoxChanged(object sender, RoutedEventArgs e)
        {
            botprefix.IsEnabled = (bool)botbox.IsChecked;
            bottoken.IsEnabled = (bool)botbox.IsChecked;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
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
                update = BASEcfgDATA.update,
                backup = BASEcfgDATA.backup,
                backupTime = BASEcfgDATA.backupTime,
                autoupdate = BASEcfgDATA.autoupdate,
                autobackup = BASEcfgDATA.autobackup,
                botToken = bottoken.Password,
                botPrefix = botprefix.Text
            };
            Console.WriteLine(bottoken.Password);
            Console.WriteLine(botprefix.Text);
            string json = JsonConvert.SerializeObject(cfgDATA, Formatting.Indented);
            File.WriteAllText(@AppDomain.CurrentDomain.BaseDirectory + @"\setting.json", json);
            setted = false;
            Close();
        }

        private void changed(object sender, RoutedEventArgs e)
        {
            setted = true;
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
