using System;
using VkNet;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using VkNet.Model;
using VkNet.Model.RequestParams;
using Microsoft.Extensions.DependencyInjection;
using VkNet.AudioBypassService.Extensions;
using System.Net;
using System.Threading;
using System.Collections.Generic;

namespace VkMusicForm1
{
    public partial class Form1 : Form
    {
        
        static public VkApi api;
        static public VkNet.Utils.VkCollection<VkNet.Model.Attachments.Audio> audios;
        static public List<string> urlList = new List<string>();
        public Form1()
        {
            InitializeComponent();
        }

        // Кнопка LogIn
        public void button1_Click(object sender, EventArgs e)
        {
            ServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddAudioBypass();
            api = new VkApi(serviceCollection);
            bool auth = false;
            
            // Авторизация
            try // Обработка неправильного логина или пароля
            {
                api.Authorize(new ApiAuthParams
                {
                    Login = textBox1.Text,
                    Password = textBox2.Text,
                    TwoFactorAuthorization = () =>
                    {
                        return api.Token;
                    }
                });

                auth = true;
                label4.Visible = false;
                groupAuth.Visible = false;
            }
            catch (VkNet.AudioBypassService.Exceptions.VkAuthException)
            {
                label4.Visible = true;
            }
            if (auth != true) {
                return;
            }
            var res = api.Groups.Get(new GroupsGetParams());
            GetAudio();
        }

        // Получаем список аудиозаписей пользователя 
        private void GetAudio()
        {
            audios = api.Audio.Get(new AudioGetParams { Count = 100 });
            int n = 0;
            foreach (var audio in audios)
            {
                checkedListBox1.Items.Add($" {n} > {audio.Artist} - {audio.Title}");
                n += 1;
            }
        }

        // Кнопка Download, скачиваем выбранную музыку по номерам
        private void button3_Click(object sender, EventArgs e)
        {
            int count = checkedListBox1.CheckedItems.Count;
            
            for (int j = 0; j < count; j++)
            {

                int v = Convert.ToInt32(checkedListBox1.CheckedItems[j].ToString()[1].ToString());
                string url = UrlDec(audios[v].Url.ToString());

                //Thread myThread = new Thread(Download);
                Download(url, v);
                
            }
        }

        public void Download(string url, int v)
        {
            WebClient webClient = new WebClient();

            webClient.DownloadProgressChanged += (o, args) => progressBar1.Value = args.ProgressPercentage;
            webClient.DownloadFileCompleted += (o, args) => progressBar1.Value = 100;
            webClient.DownloadFileAsync(new Uri(url), $@"C:\Users\user\Downloads\{ audios[v].Artist}
                - { audios[v].Title}.mp3");
            webClient.DownloadFile(url, $@"C:\Users\user\Downloads\{audios[v].Artist} - {audios[v].Title}.mp3");
            progressBar1.Value = 0;
        }

        // Парсим url котрый получили и создаём рабочую ссылку 
        public string UrlDec(string vkurl)
        {
            string url;
            string temp = vkurl.Replace("/index.m3u8", ".mp3");

            Regex regex = new Regex(@"/p\w*/");
            Match match = regex.Match(temp);
            string simvol = match.Value;

            regex = new Regex(@"/p\w*/\w*/");
            url = regex.Replace(temp, simvol);

            return url;
        }
    }
}
