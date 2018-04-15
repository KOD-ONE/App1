using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace App1
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }
        private async System.Threading.Tasks.Task openMediaFile()
        {
            FileOpenPicker fileOpenPicker = new FileOpenPicker();
            fileOpenPicker.ViewMode = PickerViewMode.Thumbnail;
            fileOpenPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            fileOpenPicker.FileTypeFilter.Add(".mp3");
            fileOpenPicker.FileTypeFilter.Add(".mp4");

            StorageFile file = await fileOpenPicker.PickSingleFileAsync();
            if (file != null)
            {
                // Application now has read/write access to the picked file
                MediaPlayer.Source = MediaSource.CreateFromStorageFile(file);
                MediaPlayer.Visibility = Visibility.Visible;
                Button.Visibility = Visibility.Collapsed;

                MediaPlayer.MediaPlayer.MediaEnded += new TypedEventHandler<Windows.Media.Playback.MediaPlayer, object>(async (player, resource) => {
                    await Button.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        Button.Visibility = Visibility.Visible;
                        MediaPlayer.Visibility = Visibility.Collapsed;
                    });
                });
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            openMediaFile();
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            Button.Visibility =Visibility.Visible;
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem btn = sender as MenuFlyoutItem;
            switch (btn.Text.ToString())
            {
                case "在线播放":
                    ShowconDlg();
                    break;
            }
        }
        public TextBox txtbox = new TextBox
        {
            Width = 240,
            PlaceholderText = "Type your URL",
        };
        private async void ShowconDlg()
        {
            var conDlg = new Windows.UI.Xaml.Controls.ContentDialog
            {
                Title = "输入URL",
                PrimaryButtonText = "确定",
                SecondaryButtonText = "取消"
            };
            conDlg.Content = txtbox;
            conDlg.PrimaryButtonClick += ConDlg_PrimaryButtonClick;
            await conDlg.ShowAsync();
        }
        private void ConDlg_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            MediaPlayer.Source = MediaSource.CreateFromUri(new Uri(txtbox.Text));
        }
        public async Task Gets(Uri uri)
        {

            try
            {
                StorageFile destinationFile = await KnownFolders.MusicLibrary.CreateFileAsync("Music.mp3");
                try
                {
                    HttpClient httpClient = new HttpClient();
                    var response = await httpClient.GetAsync(uri);
                    var buffer = await response.Content.ReadAsBufferAsync();
                    var sourceStream = await response.Content.ReadAsInputStreamAsync();

                    using (var destinationStream = await destinationFile.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        using (var destinationOutputStream = destinationStream.GetOutputStreamAt(0))
                        {
                            await RandomAccessStream.CopyAndCloseAsync(sourceStream, destinationStream);
                        }
                    }
                    MessageDialog msg = new MessageDialog("下载完毕");
                    await msg.ShowAsync();
                }
                catch (Exception e)
                {
                    MessageDialog msg = new MessageDialog("出了点问题");
                    await msg.ShowAsync();
                }
            }
            catch
            {
                MessageDialog msg = new MessageDialog("文件已存在");
                await msg.ShowAsync();
            }     
        }
        private async void Button3_Click(object sender, RoutedEventArgs e)
        {
            await Gets(new Uri(txtbox.Text));
        }
    }
}
