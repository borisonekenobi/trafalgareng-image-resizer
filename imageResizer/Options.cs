using System.Drawing;

namespace imageResizer
{
    public class Options
    {
        public string NamingConvention { get; set; }
        public Size MaxImageSize { get; set; }
        public string FolderName { get; set; }
        public int ReducedFolder { get; set; }
        public long ImageQuality { get; set; }
        public int Theme { get; set; }
    }
}
