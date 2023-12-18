using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using Microsoft.Win32;

namespace BookInfoReader
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoadFileButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Binary Files (*.bin)|*.bin"
            };

            if (openFileDialog.ShowDialog() != true)
            {
                return;
            }

            var filePath = openFileDialog.FileName;
            DisplayBookInfo(filePath);
        }

        private void DisplayBookInfo(string filePath)
        {
            try
            {
                using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                using var binaryReader = new BinaryReader(fileStream);
                
                int bookType = binaryReader.ReadUInt16();
                int pageSizeFlag = binaryReader.ReadByte();
                var publicationDateInSeconds = binaryReader.ReadInt32();
                    
                var titleBytes = new List<byte>();
                byte currentByte;
                
                while ((currentByte = binaryReader.ReadByte()) != 0)
                {
                    titleBytes.Add(currentByte);
                }
                
                var title = Encoding.UTF8.GetString(titleBytes.ToArray());
                var circulation = binaryReader.ReadInt32();

                circulation = circulation < 0 ? 0 : circulation;

                var isArchived = (bookType & 0x01) != 0;
                var isGrayscale = (bookType & 0x02) != 0;
                var hasImages = (bookType & 0x04) != 0;
                var isHardback = (bookType & 0x08) != 0;
                var isBestseller = (bookType & 0x10) != 0;

                var typeStringBuilder = new StringBuilder();
                
                if (isArchived) typeStringBuilder.Append("Archived, ");
                if (isGrayscale) typeStringBuilder.Append("Grayscale, ");
                if (hasImages) typeStringBuilder.Append("Has Images, ");
                if (isHardback) typeStringBuilder.Append("Hardback, ");
                if (isBestseller) typeStringBuilder.Append("Bestseller");

                var typeText = typeStringBuilder.ToString().TrimEnd(',', ' ');

                var pageSize = pageSizeFlag switch
                {
                    0x1 => "A5",
                    0x2 => "A4",
                    0x3 => "Letter",
                    0x4 => "Legal",
                    _ => ""
                };

                TitleTextBox.Text = title;
                CirculationTextBox.Text = circulation.ToString();
                PublicationDateTextBox.Text = DateTimeOffset.FromUnixTimeSeconds(publicationDateInSeconds).ToString("yyyy-MM-dd HH:mm:ss");
                TypeTextBox.Text = typeText;
                PageSizeTextBox.Text = pageSize;
                
                if(pageSize == string.Empty) Debug.WriteLine("Page size not specified!");
                if(typeText == string.Empty) Debug.WriteLine("Book doesn't have any type!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
