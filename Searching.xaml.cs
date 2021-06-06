using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace FB2Kbeefwebcontroller_UWP
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    
    public sealed partial class Searching : Page
    {
        DispatcherTimer timer = new DispatcherTimer();

        private void clear_hint(object sender, object e)
        {
            textblock_hint.Text = "";
            timer.Stop();
        }


        private void show_hint(string message, int seconds)
        {
            textblock_hint.Text = message;
            timer.Interval = new TimeSpan(0, 0, seconds);
            timer.Tick += clear_hint;
            timer.Start();

        }
        public ListitemModel ViewModel { get; set; }
        public Searching()
        {
            this.InitializeComponent();
            this.ViewModel = new ListitemModel();

            refresh_search();
            if (G.Server_id != 0)
            {
                refresh_playlist();
            }

        }


        private async void listitem_selected(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (listview_playlist.SelectedItem == null)
            {
                return;
            }
            try
            {
                Listitem item = (Listitem)listview_playlist.SelectedValue;
                string id = item.id;
                string title = item.title;
                string artist = item.artist;
                HttpClient http = new HttpClient();
                string url = G.Server_addr + "/api/player/play/" + G.current_playlist + "/" + id;
                var headers = http.DefaultRequestHeaders;
                headers.TryAdd("Authorization", "Basic " + G.get_auth_phrase());
                HttpStringContent content = new HttpStringContent("{\"test\":123}", UnicodeEncoding.Utf8, "application/json");
                HttpResponseMessage response = await http.PostAsync(new Uri(url), content);
                response.EnsureSuccessStatusCode();
                show_hint("播放：" + title + " - " + artist, 2);
            }
            catch (Exception ex)
            {
                G.print(ex.HResult.ToString());
            }
            
        }

        private void refresh_search()
        {
            string search = G.db_get_search_string();
            textbox_search.Text = search;
        }

        private void playlist_selection_changed(object sender, SelectionChangedEventArgs e)
        {
            if (combobox_playlist.SelectedItem == null)
            {
                return;
            }
            String playlist = combobox_playlist.SelectedValue.ToString();
            G.current_playlist = playlist;
            G.db_set_current_playlist(playlist);
            refresh_tracks();

        }

        private void button_search_click(object sender, RoutedEventArgs e)
        {
            string search = textbox_search.Text;
            G.db_set_search_string(search);
            refresh_tracks();
        }

        private void button_search_right_click(object sender, RightTappedRoutedEventArgs e)
        {
            G.accurate_search = true;
            button_search_click(null, null);
        }

        private void refresh_playlist()
        {
            combobox_playlist.Items.Clear();
            string[] playlists = G.db_get_playlists();
            foreach (string s in playlists)
            {
                combobox_playlist.Items.Add(s);
            }
            if (combobox_playlist.Items.First() != null)
            {
                combobox_playlist.SelectedIndex = 0;
            }
        }

        private async void refresh_tracks()
        {
            ViewModel.Listitems.Clear();
            int count = await get_tracks();
            if (G.accurate_search)
            {
                show_hint("精确搜索到 " + count.ToString() + " 首相关曲目", 2);
                G.accurate_search = false;
            }
            else
            {
                show_hint("搜索到 " + count.ToString() + " 首相关曲目", 2);
            }
            
        }

        private async Task<int> get_tracks()
        {
            int count = 0;
            string search = textbox_search.Text;
            string playlist = combobox_playlist.SelectedValue.ToString();
            JsonArray array = await G.db_get_tracks_by_list(playlist,search,G.current_playlist_order);
            count = array.Count;
            for (int i = 0; i < count; i++)
            {
                JsonObject json = array.GetObjectAt((uint)i);
                string title = json.GetNamedString("title");
                string artist = json.GetNamedString("artist");
                string album = json.GetNamedString("album");
                string id = json.GetNamedString("id");
                string duration = json.GetNamedString("length");
                ViewModel.Listitems.Add(new Listitem() { title = title, artist = artist, album = album, id = id, duration = duration });
            }



            return count;
        }

        private void textbox_searcon_on_keydown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                button_search_click(null, null);
            }
        }
    }
}
