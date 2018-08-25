using Spark_Comic.Models;
using Spark_Comic.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace Spark_Comic
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        StorageFolder bookRepositoryFolder = null;
        String repositoryName = "";
        String bookRepositoryFilePath = "";
        public MainPage()
        {
            this.InitializeComponent();
            ContentFrame.Navigate(typeof(WelcomePage));
            LoadGlobalVariables();
        }

        private async void openFolderSelect(object sender, RoutedEventArgs e)
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
                if (folder != null)
                {
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    repositoryName = "BookRepository_" + Guid.NewGuid().ToString("N");
                    bookRepositoryFolder = await GlobalVar.localFolder.CreateFolderAsync(repositoryName,CreationCollisionOption.OpenIfExists);
                    StorageFile serializeFile =  await bookRepositoryFolder.CreateFileAsync(folder.Name,CreationCollisionOption.ReplaceExisting);
                    bookRepositoryFilePath = serializeFile.Path;
                    var stream = await serializeFile.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);
                    List<BookItem> bookList = new List<BookItem>();
                    bookList=await  ScanningFolder(folder);
                    binaryFormatter.Serialize(stream.AsStream(), bookList);
                    using (var outputStream = stream.GetOutputStreamAt(0))
                    {
                        // We'll add more code here in the next step.
                    }
                    stream.Dispose();
                    if (bookList!=null&& bookList.Count>0)
                    {
                        ContentFrame.Navigate(typeof(BookRepositoryPage), bookRepositoryFilePath);
                        savePermissionsTokenToLocal(folder);
                        saveBookLibraryToLocal(folder);
                    }

                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async Task<List<BookItem>>  ScanningFolder(StorageFolder storageFolder)
        {
            Int16 i = 0;
            List<BookItem> bookItems = new List<BookItem>();
            IReadOnlyList<StorageFolder> folders = await storageFolder.GetFoldersAsync();
            foreach (StorageFolder folder in folders)
            {
                var thumbnail = await folder.GetScaledImageAsThumbnailAsync(ThumbnailMode.PicturesView);
                BitmapImage bitmapImage = new BitmapImage();
                InMemoryRandomAccessStream randomAccessStream = new InMemoryRandomAccessStream();
                await RandomAccessStream.CopyAsync(thumbnail, randomAccessStream);
                randomAccessStream.Seek(0);
                bitmapImage.SetSource(randomAccessStream);
                BookItem book=await ScanningBookItem(folder);
                bookItems.Add(book);
            }

            IReadOnlyList<StorageFile> files = await storageFolder.GetFilesAsync();
            foreach (StorageFile file in files)
            {
                var thumbnail = await file.GetScaledImageAsThumbnailAsync(ThumbnailMode.PicturesView);
                BitmapImage bitmapImage = new BitmapImage();
                InMemoryRandomAccessStream randomAccessStream = new InMemoryRandomAccessStream();
                await RandomAccessStream.CopyAsync(thumbnail, randomAccessStream);
                randomAccessStream.Seek(0);
                bitmapImage.SetSource(randomAccessStream);
                Debug.WriteLine(file.Name);
                bookItems.Add(new BookItem { Id = (i++).ToString(), BookName = file.DisplayName, BookCoverPath = "", ReadStatusMsg = "未读", Path = "", ReadPageNumber = 0, ReadStatus = 0 });
            }
            return bookItems;
        }

        public async Task<BookItem>  ScanningBookItem(StorageFolder storageFolder)
        {
            BookItem book = new BookItem();
            IReadOnlyList<StorageFolder> folders= await storageFolder.GetFoldersAsync();
            if (folders.Count>0)
            {
                foreach (StorageFolder folder in folders)
                {
                    book=await ScanningBookItem(folder);
                    if (book!=null)
                    {
                        break;
                    }
                }
            }
            else
            {
                book.Id= Guid.NewGuid().ToString("N");
                book.IsRepo = false;
                IReadOnlyList<StorageFile> files = await storageFolder.GetFilesAsync();
                StorageFile file = null;
                foreach (StorageFile f in files)
                {
                    if (f.ContentType.IndexOf("image/") >= 0)
                    {
                        book.IsRepo = true;
                        file = f;
                        break;
                    }
                    return null;
                }
                IRandomAccessStream ir = await file.OpenAsync(FileAccessMode.Read);
                //BitmapImage bi = new BitmapImage();
                String fileName = storageFolder.Name+"BookCover" + file.Name;
                StorageFile sampleFile = await bookRepositoryFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                Debug.WriteLine(sampleFile.Path);
                var stream = await sampleFile.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(ir);
                BitmapEncoder encoder = await BitmapEncoder.CreateForTranscodingAsync(stream, decoder);
                encoder.BitmapTransform.ScaledWidth = 170*2;
                encoder.BitmapTransform.ScaledHeight = 237*2;
                encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Fant;
                await encoder.FlushAsync();
                book.BookCoverPath = sampleFile.Path;
                book.BookName = storageFolder.DisplayName;
                book.Path = storageFolder.Path;
                book.ReadStatus = 0;
                book.ReadPageNumber = 0;
                book.ReadStatusMsg = "未读";
                book.PermissionsToken = StorageApplicationPermissions.FutureAccessList.Add(storageFolder);
                
            }
            return book;
        }

        public async void savePermissionsTokenToLocal(IStorageItem item)
        {
            GlobalVar.permissionsTokens.Add(new PermissionsToken { id=Guid.NewGuid().ToString("N"),name=item.Name,path=item.Path,token= StorageApplicationPermissions.FutureAccessList.Add(item)});
            String fileName  = "PermissionsTokensFile";
            StorageFile serializeFile = await GlobalVar.localFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            using (Stream stream=new FileStream(serializeFile.CreateSafeFileHandle(),FileAccess.ReadWrite))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(stream, GlobalVar.permissionsTokens);
            }
        }

        public async void saveBookLibraryToLocal(StorageFolder folder)
        {
            GlobalVar.bookLibLibraries.Add(new BookLibrary { id = Guid.NewGuid().ToString("N"), name = folder.Name, path = folder.Path, token = StorageApplicationPermissions.FutureAccessList.Add(folder) });
            String fileName = "BookLibLibrariesFile";
            StorageFile serializeFile = await GlobalVar.localFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            using (Stream stream = new FileStream(serializeFile.CreateSafeFileHandle(), FileAccess.ReadWrite))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(stream, GlobalVar.bookLibLibraries);
            }
        }

        private async void LoadGlobalVariables()
        {
            GlobalVar.localFolder = ApplicationData.Current.LocalFolder;
            IReadOnlyList<StorageFile> files = await GlobalVar.localFolder.GetFilesAsync();
            if (files.Count == 0)
            {
                return;
            }
            StorageFile BookLibLibrariesFile = await GlobalVar.localFolder.GetFileAsync("BookLibLibrariesFile");
            StorageFile PermissionsTokensFile = await GlobalVar.localFolder.GetFileAsync("PermissionsTokensFile");
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (FileStream stream = new FileStream(BookLibLibrariesFile.Path, FileMode.Open))
            {
                GlobalVar.bookLibLibraries = binaryFormatter.Deserialize(stream) as List<BookLibrary>;
                bookLibLibraryListView.ItemsSource = GlobalVar.bookLibLibraries;
            }
            using (FileStream stream = new FileStream(PermissionsTokensFile.Path, FileMode.Open))
            {
                GlobalVar.permissionsTokens = binaryFormatter.Deserialize(stream) as List<PermissionsToken>;

            }
        }

        private async void bookLibLibraryListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            BookLibrary bookLibLibrary= e.ClickedItem as BookLibrary;

        }
    }

}
