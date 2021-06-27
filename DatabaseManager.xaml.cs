using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;

namespace NoteyDictionary
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class DatabaseManager : Window
    {
        public string selectedFilePath = "";

        public DatabaseManager()
        {
            InitializeComponent();
        }

        private void bDownload_Click(object sender, RoutedEventArgs e)
        {
            Database selectedDatabase = DatabaseHelper.GetDatabaseFromTag(((Button)sender).Tag.ToString());
            string downloadURL = selectedDatabase switch
            {
                Database.Lite => "https://gist.githubusercontent.com/deekayen/4148741/raw/98d35708fa344717d8eee15d11987de6c8e26d7d/1-1000.txt",
                Database.FullAlpha => "https://raw.githubusercontent.com/dwyl/english-words/master/words_alpha.txt",
                Database.Full => "https://raw.githubusercontent.com/dwyl/english-words/master/words.txt",
                _ => throw new Exception()
            };
            string filePath = selectedDatabase switch
            {
                Database.Lite => $"{System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\words_lite.txt",
                Database.FullAlpha => $"{System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\words_alpha.txt",
                Database.Full => $"{System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\words_full.txt",
                _ => throw new Exception()
            };

            Downloader downloader = new Downloader(downloadURL, filePath);
            downloader.ShowDialog();
        }

        private void bUse_Click(object sender, RoutedEventArgs e)
        {
            Database selectedDatabase = DatabaseHelper.GetDatabaseFromTag(((Button)sender).Tag.ToString());
            string filePath = selectedDatabase switch
            {
                Database.Lite => $"{System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\words_lite.txt",
                Database.FullAlpha => $"{System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\words_alpha.txt",
                Database.Full => $"{System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\words_full.txt",
                _ => throw new Exception()
            };

            if (!File.Exists(filePath))
            {
                MessageBox.Show("Download this database before using it!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            selectedFilePath = filePath;
            this.Close();
        }
    }
}
