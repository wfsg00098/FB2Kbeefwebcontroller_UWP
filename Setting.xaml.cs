using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace FB2Kbeefwebcontroller_UWP
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Setting : Page
    {
        DispatcherTimer timer = new DispatcherTimer();
        public Setting()
        {
            this.InitializeComponent();
            refresh_server_list(G.Server_id);
            refresh_playlist_order();
            refresh_track_limit();

            textblock_copyright.Text = "版权所有 © 2017-" + DateTime.Now.Year + ". 王悄悄 保留所有权利";
        }

        private void clear_hint(object sender, object e)
        {
            textblock_hint.Text = "";
            timer.Stop();
        }


        private void show_hint(string message,int seconds)
        {
            textblock_hint.Text = message;
            if (seconds > 0)
            {
                timer.Interval = new TimeSpan(0, 0, seconds);
                timer.Tick += clear_hint;
                timer.Start();
            }

        }

        private void refresh_server_list(int selection_id)
        {
            combobox_server_list.Items.Clear();
            JsonArray array = G.db_get_servers();
            int index = 1;
            for (int i = 1; i <= array.Count; i++)
            {
                JsonObject temp = array.GetObjectAt((uint)(i - 1));
                int id = (int)temp.GetNamedNumber("id");
                if (id == selection_id || i==1)
                {
                    index = id;
                }
                combobox_server_list.Items.Add(id.ToString());
            }
            combobox_server_list.SelectedItem = index.ToString();

            
        }

        private void server_changed(object sender, SelectionChangedEventArgs e)
        {
            if (combobox_server_list.SelectedItem == null)
            {
                textbox_server_addr.Text = "";
                textbox_server_user.Text = "";
                textbox_server_pass.Text = "";
                return;
            }
            int server_id = Int32.Parse(combobox_server_list.SelectedItem.ToString());
            string[] server = G.db_get_server(server_id);
            textbox_server_addr.Text = server[0];
            textbox_server_user.Text = server[1];
            textbox_server_pass.Text = server[2];

        }

        private void refresh_playlist_order()
        {
            if (G.current_playlist_order == G.ORDER_DEFAULT)
            {
                textblock_order.Text = "列表排序：默认";
            }
            if (G.current_playlist_order == G.ORDER_TITLE_ASC)
            {
                textblock_order.Text = "列表排序：标题 (升序)";
            }
            if (G.current_playlist_order == G.ORDER_TITLE_DESC)
            {
                textblock_order.Text = "列表排序：标题 (降序)";
            }
            if (G.current_playlist_order == G.ORDER_ARTIST_ASC)
            {
                textblock_order.Text = "列表排序：艺术家 (升序)";
            }
            if (G.current_playlist_order == G.ORDER_ARTIST_DESC)
            {
                textblock_order.Text = "列表排序：艺术家 (降序)";
            }
            if (G.current_playlist_order == G.ORDER_ALBUM_ASC)
            {
                textblock_order.Text = "列表排序：专辑 (升序)";
            }
            if (G.current_playlist_order == G.ORDER_ALBUM_DESC)
            {
                textblock_order.Text = "列表排序：专辑 (降序)";
            }
            if (G.current_playlist_order == G.ORDER_RANDOM)
            {
                textblock_order.Text = "列表排序：随机";
            }
        }

        private void refresh_track_limit()
        {
            textbox_tracks_limit.Text = G.tracks_limit.ToString();
        }

        private void button_server_add_click(object sender, RoutedEventArgs e)
        {
            int id = G.db_add_server();
            refresh_server_list(id);
            show_hint("已新增服务器 " + id.ToString(), 2);
        }

        private async void button_server_del_click(object sender, RoutedEventArgs e)
        {
            ContentDialog confirm = new ContentDialog
            {
                Title = "确认？",
                PrimaryButtonText = "删除",
                CloseButtonText = "取消",
                DefaultButton = ContentDialogButton.Primary
            };
            ContentDialogResult result = await confirm.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                int server_id = Int32.Parse(combobox_server_list.SelectedItem.ToString());
                G.db_del_server(server_id);
                refresh_server_list(0);
                show_hint("已删除服务器 "+server_id.ToString(), 2);
            }
        }

        private void button_server_save_click(object sender, RoutedEventArgs e)
        {
            int server_id = Int32.Parse(combobox_server_list.SelectedItem.ToString());
            string addr = textbox_server_addr.Text;
            string user = textbox_server_user.Text;
            string pass = textbox_server_pass.Text;
            G.db_save_server(server_id, addr, user, pass);
            show_hint("已保存服务器 " + server_id.ToString(), 2);
        }

        private void button_server_use_click(object sender, RoutedEventArgs e)
        {
            int server_id = Int32.Parse(combobox_server_list.SelectedItem.ToString());
            G.db_select_server(server_id);
            show_hint("已使用服务器 " + server_id.ToString(), 2);
        }

        private async void button_update_playlist_click(object sender, RoutedEventArgs e)
        {
            if (G.updateing_playlist)
            {
                return;
            }
            G.updateing_playlist = true;
            show_hint("正在更新列表，请等待", 0);
            this.IsEnabled = false;

            if (await G.db_update_playlists())
            {
                show_hint("列表已更新", 2);
            }
            else
            {
                show_hint("列表更新失败", 2);
            }
            this.IsEnabled = true;
            G.updateing_playlist = false;
        }

        private void button_order_default_click(object sender, RoutedEventArgs e)
        {
            G.db_set_playlist_order(G.ORDER_DEFAULT);
            refresh_playlist_order();
            show_hint("已设置为默认排序", 2);
        }

        private void button_order_random_click(object sender, RoutedEventArgs e)
        {
            G.db_set_playlist_order(G.ORDER_RANDOM);
            refresh_playlist_order();
            show_hint("已设置为随机排序", 2);
        }

        private void button_order_title_asc_click(object sender, RoutedEventArgs e)
        {
            G.db_set_playlist_order(G.ORDER_TITLE_ASC);
            refresh_playlist_order();
            show_hint("已设置为标题(升序)排序", 2);
        }

        private void button_order_title_desc_click(object sender, RoutedEventArgs e)
        {
            G.db_set_playlist_order(G.ORDER_TITLE_DESC);
            refresh_playlist_order();
            show_hint("已设置为标题(降序)排序", 2);
        }

        private void button_order_artist_asc_click(object sender, RoutedEventArgs e)
        {
            G.db_set_playlist_order(G.ORDER_ARTIST_ASC);
            refresh_playlist_order();
            show_hint("已设置为艺术家(升序)排序", 2);
        }

        private void button_order_artist_desc_click(object sender, RoutedEventArgs e)
        {
            G.db_set_playlist_order(G.ORDER_ARTIST_DESC);
            refresh_playlist_order();
            show_hint("已设置为艺术家(降序)排序", 2);
        }

        private void button_order_album_asc_click(object sender, RoutedEventArgs e)
        {
            G.db_set_playlist_order(G.ORDER_ALBUM_ASC);
            refresh_playlist_order();
            show_hint("已设置为专辑(升序)排序", 2);
        }

        private void button_order_album_desc_click(object sender, RoutedEventArgs e)
        {
            G.db_set_playlist_order(G.ORDER_ALBUM_DESC);
            refresh_playlist_order();
            show_hint("已设置为专辑(降序)排序", 2);
        }

        private void button_track_limit_save_click(object sender, RoutedEventArgs e)
        {
            try
            {
                int limit = Int32.Parse(textbox_tracks_limit.Text);
                G.db_set_list_limit(limit);
                G.tracks_limit = limit;
                refresh_track_limit();
                show_hint("已保存曲目显示限制", 2);
            } catch(Exception ex)
            {
                show_hint("曲目显示限制输入不正确", 2);
            }
            
        }
    }
}
