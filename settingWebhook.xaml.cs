using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace bedrock_server_manager
{
    /// <summary>
    /// settingWebhook.xaml の相互作用ロジック
    /// </summary>
    public partial class settingWebhook : Window
    {
        public bool setted;
        public settingWebhook()
        {
            InitializeComponent();
            string webhook = "https://discord.com/api/webhooks/1234567890/AbCdEfGhIj_KlMnOp";

            if (File.Exists(@AppDomain.CurrentDomain.BaseDirectory + @"\webhook.txt"))
            {
                using (StreamReader file = File.OpenText(@AppDomain.CurrentDomain.BaseDirectory + @"\webhook.txt"))
                {
                    webhook = file.ReadToEnd();
                }
            }

            webhookUrl.Text = webhook;

            setted = true;
        }

        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            setted = false;
        }

        private void saveSettings(object sender, RoutedEventArgs e)
        {
            setted = true;
            using (StreamWriter file = new StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + @"\webhook.txt", false))
            {
                file.WriteLine(webhookUrl.Text);
            }

            Close();
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
