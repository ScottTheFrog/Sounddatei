using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Sounddatei
{
    public class ListItem
    {
        //Name of File/Directory
        public string FileName { get; set; } = "DefaultFileName.txt";
        //Path of File/Directory
        public string Path { get; set; } = "DefaultPath.txt";
        public string TextBackgroundColor { get; set; } = "Transparent";
        public string Length { get; set; } = "00:00";
        public ListItem(string filename, string path)
        {
            FileName = filename;
            Path = path;
        }
    }
}
