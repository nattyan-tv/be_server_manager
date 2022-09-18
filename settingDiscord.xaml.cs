using Newtonsoft.Json;
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
using static bedrock_server_manager.BaseConfig;

namespace bedrock_server_manager
{
    /// <summary>
    /// settingDiscord.xaml の相互作用ロジック
    /// </summary>
    public partial class settingDiscord : Window
    {
        public class DiscordBotSetting
        {
            public string[] guild_ids { get; set; }
            public string[] bot_admins { get; set; }
        }

        public bool setted = false;

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.ToString());
        }

        public settingDiscord()
        {
            InitializeComponent();
            BaseConfig[] cfgDATA = null;
            using (StreamReader file = File.OpenText(@AppDomain.CurrentDomain.BaseDirectory + @"\setting.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                cfgDATA = (BaseConfig[])serializer.Deserialize(file, typeof(BaseConfig[]));
            }
            DiscordBotSetting disSet = null;

            if (File.Exists(@AppDomain.CurrentDomain.BaseDirectory + @"\discord.json")) {
                botbox.IsChecked = true;
                botprefix.IsEnabled = true;
                bottoken.IsEnabled = true;
                bot_guilds.IsEnabled = true;
                bot_users.IsEnabled = true;
                using (StreamReader file = File.OpenText(@AppDomain.CurrentDomain.BaseDirectory + @"\discord.json"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    disSet = (DiscordBotSetting)serializer.Deserialize(file, typeof(DiscordBotSetting));
                }
                bot_guilds.Text = string.Join(",", disSet.guild_ids);
                bot_users.Text = string.Join(",", disSet.bot_admins);
            }

            bottoken.Password = cfgDATA[0].botToken;
            botprefix.Text = cfgDATA[0].botPrefix;

            setted = false;
        }

        private void BoxChanged(object sender, RoutedEventArgs e)
        {
            botprefix.IsEnabled = (bool)botbox.IsChecked;
            bottoken.IsEnabled = (bool)botbox.IsChecked;
            bot_guilds.IsEnabled = (bool)botbox.IsChecked;
            bot_users.IsEnabled = (bool)botbox.IsChecked;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
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
                backup = BASEcfgDATA[0].backup,
                backupTime = BASEcfgDATA[0].backupTime,
                autoupdate = BASEcfgDATA[0].autoupdate,
                autobackup = BASEcfgDATA[0].autobackup,
                botToken = bottoken.Password,
                botPrefix = botprefix.Text
            } };

            DiscordBotSetting DisData = new DiscordBotSetting
            {
                guild_ids = bot_guilds.Text.Split(','),
                bot_admins = bot_users.Text.Split(',')
            };

            Console.WriteLine(bottoken.Password);
            Console.WriteLine(botprefix.Text);
            string jsonConfig = JsonConvert.SerializeObject(cfgDATA, Formatting.Indented);
            string jsonDiscord = JsonConvert.SerializeObject(DisData, Formatting.Indented);
            File.WriteAllText(@AppDomain.CurrentDomain.BaseDirectory + @"\setting.json", jsonConfig);
            File.WriteAllText(@AppDomain.CurrentDomain.BaseDirectory + @"\discord.json", jsonDiscord);
            setted = false;
            Close();
        }

        private void changed(object sender, RoutedEventArgs e)
        {
            setted = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (setted)
            {
                MessageBoxResult ans_c = MessageBox.Show("保存していない項目があるようです。\n保存しなくてもよろしいですか？", "BE Server Manager", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation);
                Console.WriteLine(ans_c);
                if (ans_c != MessageBoxResult.OK) { e.Cancel = true; }
            }
        }
    }
}
