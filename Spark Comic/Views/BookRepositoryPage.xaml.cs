using Spark_Comic.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.FileProperties;
using Windows.Storage.Search;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Spark_Comic.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class BookRepositoryPage : Page
    {
        public BookRepositoryPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            

            if (e.Parameter != null)
            {
                String bookRepositoryFilePath = e.Parameter as String;
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                using (FileStream stream = new FileStream(bookRepositoryFilePath, FileMode.Open))
                {
                    BookGridView.ItemsSource = binaryFormatter.Deserialize(stream) as List<BookItem>;
                }

            }
        }

        private async void BookGridView_ItemClickAsync(object sender, ItemClickEventArgs e)
        {
            BookItem book = e.ClickedItem as BookItem;
            Debug.WriteLine(book.Path);
            StorageFolder folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(book.PermissionsToken);
            Debug.WriteLine(folder.DisplayName);
            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(typeof(VerticalBookViewPage),folder);
        }

    }
}
