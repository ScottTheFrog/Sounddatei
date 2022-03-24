namespace Sounddatei
{
    public class ListItem
    {
        public string FileName { get; }
        public string Path { get;  }
        public string Length { get; }
        public int ItemIndex { get; }
        public ListItem(int index, string filename, string path)
        {
            FileName = filename;
            Path = path;
            ItemIndex = index;
        }
    }
}
