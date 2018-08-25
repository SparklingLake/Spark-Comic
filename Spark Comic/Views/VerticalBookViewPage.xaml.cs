using Spark_Comic.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Spark_Comic.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class VerticalBookViewPage : Page
    {
        public VerticalBookViewPage()
        {
            this.InitializeComponent();
        }
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter!=null)
            {
                StorageFolder folder = e.Parameter as StorageFolder;
                if (folder!=null)
                {
                    List<BookPage> bookPages = new List<BookPage>();
                    IReadOnlyList<StorageFile> files =await  folder.GetFilesAsync();
                    foreach (var file in files)
                    {
                        bookPages.Add(new BookPage {name=file.Name,path=file.Path,fileType=file.FileType });
                    }
                    bookContextListView.ItemsSource = bookPages;
                }
            }
        }
    }
}
