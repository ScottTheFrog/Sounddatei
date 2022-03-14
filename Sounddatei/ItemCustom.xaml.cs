using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Collections.ObjectModel;

namespace Sounddatei
{
    public partial class ItemCustom : TreeViewItem
    {
        //Name of File/Directory
        public string FileName { get; set; } = "DefaultFileName.txt";
        //Path of File/Directory
        public string Path { get; set; } = "DefaultPath.txt";
        //icon of File/Directory
        public BitmapImage ImageBitMap { get; set; }
        //
        public bool isDirectory { get; set; }
        //List to hold the items inside Item if it is a Directory
        public ObservableCollection<ItemCustom> list { get; set; }
        public ItemCustom()
        {
            //initialize list
            list = new ObservableCollection<ItemCustom>();
            InitializeComponent();
            //set datacontext for xaml
            this.DataContext = this;
        }
    }
}
