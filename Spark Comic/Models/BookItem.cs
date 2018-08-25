using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Spark_Comic.Models
{
    [Serializable]
    public class BookItem
    {
        public String Id { get; set; }//ID
        public String BookName { get; set; }//文件夹的名称
        public String BookCoverPath { get; set; }//封面图片的文件路径
        public String Path { get; set; }//文件夹的路径
        public Int16 ReadStatus { get; set; }//阅读状态
        public Int16 ReadPageNumber { get; set; }//当前阅读的页数
        public String ReadStatusMsg { get; set; }//状态描述
        public Boolean IsRepo { get; set; }//是否有子文件夹
        public String PermissionsToken { get; set; }//文件夹权限Token

        // 用于数据绑定列表
        public ObservableCollection<BookItem> list { get; set; }
    }
}
