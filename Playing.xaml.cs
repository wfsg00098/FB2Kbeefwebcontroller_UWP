using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Data.Json;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Web.Http;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace FB2Kbeefwebcontroller_UWP
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Playing : Page
    {
        public DispatcherTimer timer = new DispatcherTimer();
        public DispatcherTimer timer2 = new DispatcherTimer();


        public Playing()
        {
            this.InitializeComponent();
            
            refresh_playing(null,null);
            timer.Stop();
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += refresh_playing;
            timer.Start();

        }

        private void clear_hint(object sender, object e)
        {
            textblock_hint.Text = "";
            timer2.Stop();
        }


        private void show_hint(string message, int seconds)
        {
            textblock_hint.Text = message;
            timer2.Interval = new TimeSpan(0, 0, seconds);
            timer2.Tick += clear_hint;
            timer2.Start();

        }

        public async void refresh_playing(object sender, object e)
        {
            if (G.Server_addr == "" || G.Server_addr == null)
            {
                return;
            }
            textblock_current_server.Text = "当前服务器：" + G.Server_addr;
            int current_server_id = G.Server_id;
            HttpClient http = new HttpClient();
            string url = G.Server_addr + "/api/player?columns=%title%,%artist%,%album%";
            var headers = http.DefaultRequestHeaders;
            headers.TryAdd("Authorization", "Basic " + G.get_auth_phrase());
            string body = "";
            try
            {
                HttpResponseMessage response = await http.GetAsync(new Uri(url));
                response.EnsureSuccessStatusCode();
                body = await response.Content.ReadAsStringAsync();
                JsonObject json = JsonObject.Parse(body);
                json = json.GetNamedObject("player");
                json = json.GetNamedObject("activeItem");
                int song_id = (int)json.GetNamedValue("index").GetNumber();
                string song_playlist = json.GetNamedString("playlistId");
                if (song_id == G.current_playing_song_id && song_playlist.Equals(G.current_playing_song_playlist) && !G.changed_frame)
                {
                    return;
                }
                G.current_playing_song_id = song_id;
                G.current_playing_song_playlist = song_playlist;

                JsonArray columns = json.GetNamedArray("columns");
                textblock_title.Text = columns.GetStringAt(0);
                textblock_artist.Text = columns.GetStringAt(1);
                textblock_album.Text = columns.GetStringAt(2);
                url = G.Server_addr + "/api/artwork/" + G.current_playing_song_playlist + "/" + G.current_playing_song_id;
                response = await http.GetAsync(new Uri(url));
                try
                {
                    response.EnsureSuccessStatusCode();
                    IBuffer buffer = await response.Content.ReadAsBufferAsync();
                    BitmapImage img = new BitmapImage();
                    using (IRandomAccessStream stream = new InMemoryRandomAccessStream())
                    {
                        await stream.WriteAsync(buffer);
                        stream.Seek(0);
                        await img.SetSourceAsync(stream);
                        img_album_art.Source = img;
                    }
                }catch (Exception ex)
                {
                    BitmapImage i = new BitmapImage(new Uri("ms-appx:Assets/default_album_art.jpg"));
                    img_album_art.Source = i;
                    G.print(ex.HResult.ToString());
                }
                
                
                
            }
            catch (Exception ex)
            {
                G.print(ex.HResult.ToString());
                if (G.current_playing_song_id == -1)
                {
                    return;
                }
                if (current_server_id == G.Server_id)
                {
                    G.current_playing_song_id = -1;
                    textblock_title.Text = "标题";
                    textblock_artist.Text = "艺术家";
                    textblock_album.Text = "专辑";
                    BitmapImage i = new BitmapImage(new Uri("ms-appx:Assets/default_album_art.jpg"));
                    img_album_art.Source = i;
                }
                
            }
        }

        private async void button_stop_click(object sender, RoutedEventArgs e)
        {
            try
            {
                HttpClient http = new HttpClient();
                string url = G.Server_addr + "/api/player/stop";
                var headers = http.DefaultRequestHeaders;
                headers.TryAdd("Authorization", "Basic " + G.get_auth_phrase());
                HttpStringContent content = new HttpStringContent("{\"test\":123}", UnicodeEncoding.Utf8, "application/json");
                HttpResponseMessage response = await http.PostAsync(new Uri(url), content);
                response.EnsureSuccessStatusCode();
                textblock_title.Text = "标题";
                textblock_artist.Text = "艺术家";
                textblock_album.Text = "专辑";
                BitmapImage i = new BitmapImage(new Uri("ms-appx:Assets/default_album_art.jpg"));
                img_album_art.Source = i;
                show_hint("停止", 1);
            }catch (Exception ex)
            {
                G.print(ex.HResult.ToString());
                show_hint("无法连接到服务器", 1);
            }
                
            
        }

        private async void button_play_click(object sender, RoutedEventArgs e)
        {
            try
            {
                HttpClient http = new HttpClient();
                string url = G.Server_addr + "/api/player/play";
                var headers = http.DefaultRequestHeaders;
                headers.TryAdd("Authorization", "Basic " + G.get_auth_phrase());
                HttpStringContent content = new HttpStringContent("{\"test\":123}", UnicodeEncoding.Utf8, "application/json");
                HttpResponseMessage response = await http.PostAsync(new Uri(url), content);
                response.EnsureSuccessStatusCode();
                Thread.Sleep(100);
                refresh_playing(null, null);
                show_hint("播放", 1);
            }
            catch (Exception ex)
            {
                G.print(ex.HResult.ToString());
                show_hint("无法连接到服务器", 1);
            }
            
        }

        private async void button_pause_click(object sender, RoutedEventArgs e)
        {
            try
            {
                HttpClient http = new HttpClient();
                string url = G.Server_addr + "/api/player/pause/toggle";
                var headers = http.DefaultRequestHeaders;
                headers.TryAdd("Authorization", "Basic " + G.get_auth_phrase());
                HttpStringContent content = new HttpStringContent("{\"test\":123}", UnicodeEncoding.Utf8, "application/json");
                HttpResponseMessage response = await http.PostAsync(new Uri(url), content);
                response.EnsureSuccessStatusCode();
                Thread.Sleep(100);
                refresh_playing(null, null);
                show_hint("播放/暂停", 1);
            }
            catch (Exception ex)
            {
                G.print(ex.HResult.ToString());
                show_hint("无法连接到服务器", 1);
            }
            
        }

        private async void button_previous_click(object sender, RoutedEventArgs e)
        {
            try
            {
                HttpClient http = new HttpClient();
                string url = G.Server_addr + "/api/player/previous";
                var headers = http.DefaultRequestHeaders;
                headers.TryAdd("Authorization", "Basic " + G.get_auth_phrase());
                HttpStringContent content = new HttpStringContent("{\"test\":123}", UnicodeEncoding.Utf8, "application/json");
                HttpResponseMessage response = await http.PostAsync(new Uri(url), content);
                response.EnsureSuccessStatusCode();
                Thread.Sleep(100);
                refresh_playing(null, null);
                show_hint("上一曲", 1);
            }
            catch (Exception ex)
            {
                G.print(ex.HResult.ToString());
                show_hint("无法连接到服务器", 1);
            }
            
        }

        private async void button_next_click(object sender, RoutedEventArgs e)
        {
            try
            {
                HttpClient http = new HttpClient();
                string url = G.Server_addr + "/api/player/next";
                var headers = http.DefaultRequestHeaders;
                headers.TryAdd("Authorization", "Basic " + G.get_auth_phrase());
                HttpStringContent content = new HttpStringContent("{\"test\":123}", UnicodeEncoding.Utf8, "application/json");
                HttpResponseMessage response = await http.PostAsync(new Uri(url), content);
                response.EnsureSuccessStatusCode();
                Thread.Sleep(100);
                refresh_playing(null, null);
                show_hint("下一曲", 1);
            }
            catch (Exception ex)
            {
                G.print(ex.HResult.ToString());
                show_hint("无法连接到服务器", 1);
            }
            
        }

        private async void button_replay_click(object sender, RoutedEventArgs e)
        {
            try
            {
                HttpClient http = new HttpClient();
                string url = G.Server_addr + "/api/player/stop";
                var headers = http.DefaultRequestHeaders;
                headers.TryAdd("Authorization", "Basic " + G.get_auth_phrase());
                HttpStringContent content = new HttpStringContent("{\"test\":123}", UnicodeEncoding.Utf8, "application/json");
                HttpResponseMessage response = await http.PostAsync(new Uri(url), content);
                response.EnsureSuccessStatusCode();
                url = G.Server_addr + "/api/player/play";
                response = await http.PostAsync(new Uri(url), content);
                response.EnsureSuccessStatusCode();
                show_hint("重新播放", 1);
            }
            catch (Exception ex)
            {
                G.print(ex.HResult.ToString());
                show_hint("无法连接到服务器", 1);
            }
            
        }
    }
}
