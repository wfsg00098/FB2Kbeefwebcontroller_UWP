using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace FB2Kbeefwebcontroller_UWP
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            contentFrame.Navigate(typeof(Playing));
        }

        private void nv_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {

            if (args.IsSettingsInvoked)
            {
                contentFrame.Navigate(typeof(Setting));
            }
            else
            {
                //选中项的内容
                switch (args.InvokedItem)
                {
                    case "正在播放":
                        G.changed_frame = true;
                        contentFrame.Navigate(typeof(Playing));
                        break;
                    case "搜索":
                        contentFrame.Navigate(typeof(Searching));
                        break;

                }

            }
        }
    }
}
