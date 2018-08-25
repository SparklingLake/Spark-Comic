using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace Spark_Comic.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ImagePage : Page
    {
        public ImagePage()
        {
            this.InitializeComponent();
        }

        string saveLocal = "";

        IReadOnlyList<StorageFile> fileList;

        Int16 nowIndex = 0;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter != null)
            {
                if(e.Parameter.GetType()== typeof(StorageFile))
                {
                    StorageFile file = (StorageFile)e.Parameter;
                    IRandomAccessStream ir = await file.OpenAsync(FileAccessMode.Read);
                    BitmapImage bi = new BitmapImage();
                    await bi.SetSourceAsync(ir);
                    imageView.Source = bi;
                }
                else if (e.Parameter.GetType() == typeof(StorageFolder))
                {

                }
            }
        }

        private  async void  openFolderSelect(object sender,RoutedEventArgs e)
        {
            try
            {
                FolderPicker folderPicker = new FolderPicker();
                folderPicker.FileTypeFilter.Add(".jpeg");
                folderPicker.FileTypeFilter.Add(".png");
                folderPicker.FileTypeFilter.Add(".jpg");
                folderPicker.FileTypeFilter.Add(".svg");
                folderPicker.FileTypeFilter.Add(".bmp");
                StorageFolder folder = await folderPicker.PickSingleFolderAsync();
                previousBtn.IsEnabled = false;
                nextBtn.IsEnabled = false;
                if (folder != null)
                {
                    fileNameTextBox.Text = folder.Path;
                    fileList = await folder.GetFilesAsync();
                    if (fileList.Count <= 0)
                    {
                        return;
                    }else if (fileList.Count > 1)
                    {
                        nextBtn.IsEnabled = true;
                    }
                    nowIndex = 0;
                    Console.WriteLine(fileList);
                    StorageFile file = fileList[nowIndex];
                    IRandomAccessStream ir = await file.OpenAsync(FileAccessMode.Read);
                    BitmapImage bi = new BitmapImage();
                    await bi.SetSourceAsync(ir);
                    imageView.Source = bi;
                }

            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine(ex.Message);
            }
            
        }

        private async void previousFile(object sender, RoutedEventArgs e)
        {
            try
            {
                if (nowIndex<=0)
                {
                    previousBtn.IsEnabled = false;
                    return;
                }
                if (0==(nowIndex -= 1))
                {
                    previousBtn.IsEnabled = false;
                }
                nextBtn.IsEnabled = true;
                StorageFile file = fileList[nowIndex];
                IRandomAccessStream ir = await file.OpenAsync(FileAccessMode.Read);
                BitmapImage bi = new BitmapImage();
                await bi.SetSourceAsync(ir);
                imageView.Source = bi;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async void nextFile(object sender, RoutedEventArgs e)
        {
            try
            {
                if (fileList.Count <= nowIndex)
                {
                    nextBtn.IsEnabled = false;
                    return;
                }
                if (fileList.Count == (nowIndex += 1))
                {
                    nextBtn.IsEnabled = false;
                }
                previousBtn.IsEnabled = true;
                StorageFile file = fileList[nowIndex];
                IRandomAccessStream ir = await file.OpenAsync(FileAccessMode.Read);
                BitmapImage bi = new BitmapImage();
                await bi.SetSourceAsync(ir);
                imageView.Source = bi;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async void openFileSelect(object sender, RoutedEventArgs e)
        {
            try
            {
                FileOpenPicker filePicker = new FileOpenPicker();
                filePicker.FileTypeFilter.Add(".jpeg");
                filePicker.FileTypeFilter.Add(".png");
                filePicker.FileTypeFilter.Add(".jpg");
                filePicker.FileTypeFilter.Add(".svg");
                filePicker.FileTypeFilter.Add(".bmp");
                filePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                IAsyncOperation<StorageFile> fileTask = filePicker.PickSingleFileAsync();
                StorageFile file = await fileTask;
                
                if (file != null)
                {
                    
                    //IAsyncOperation<StorageFolder> folderTask = 
                    StorageFolder folder = await file.GetParentAsync();
                    if (folder != null)
                    {
                        fileNameTextBox.Text += "--------------------------------" + folder.DisplayName;
                    }
                    else//如果不能访问父文件夹则直接打开当前图片
                    {
                        folder=await StorageFolder.GetFolderFromPathAsync(file.Path.Replace(file.Name,""));
                        fileNameTextBox.Text += "--------------------------------" + folder.DisplayName;
                        IRandomAccessStream ir = await file.OpenAsync(FileAccessMode.Read);
                        BitmapImage bi = new BitmapImage();
                        await bi.SetSourceAsync(ir);
                        imageView.Source = bi;
                    }
                }
               

            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

    }

}
