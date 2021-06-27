using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Threading;

namespace NoteyDictionary
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<string> words;
        bool wordsLoaded = false;
        int numberWords = 0;
        int results = 0;
        int tenthMillisecondsTaken = 0;
        uint maxResults = 800;
        byte minChars = 2;
        bool maxResultsOverride = false;
        DispatcherTimer searchTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(0.1f) };

        public MainWindow()
        {
            InitializeComponent();

            searchTimer.Tick += searchTimer_Tick;

            string filePath = DatabaseHelper.DetermineBestDatabase();
            LoadWords(filePath);
        }

        

        private async void LoadWords(string filePath)
        {
            SetupProgressBarFileLoading(filePath);
            words = new List<string>();
            await Task.Run(() =>
            {
                string line;
                using (var reader = File.OpenText(filePath))
                {
                    while ((line = reader.ReadLine()) != null)
                    {
                        words.Add(line);
                        Dispatcher.Invoke(() => {
                            pbarProgress.Value++;
                            textStats.Text = $"{pbarProgress.Value}/{numberWords} entries loaded)";
                        });
                    }
                }
            });
            pbarProgress.Visibility = Visibility.Hidden;
            textStatus.Text = "Enter a search term to get started";
            wordsLoaded = true;
            textStats.Text = "";
        }

        private async void SetupProgressBarFileLoading(string filePath)
        {
            await Task.Run(() => { numberWords = File.ReadLines(filePath).Count(); });
            pbarProgress.Visibility = Visibility.Visible;
            pbarProgress.Value = 0;
            pbarProgress.Maximum = numberWords;
        }

        private void tbSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            tbSearch.Text = "";
        }

        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            Search(tbSearch.Text.ToLower());
        }

        private async void Search(string searchTerm)
        {
            if (!wordsLoaded) return;
            bSearchAnyway.Visibility = Visibility.Hidden;
            if (tbSearch.Text.Length < minChars)
            {
                listboxWords.Items.Clear();
                textStatus.Visibility = Visibility.Visible;
                textStatus.Text = $"Enter at least {minChars} characters to search";
                textStats.Text = "";
                return;
            }

            results = 0;
            tenthMillisecondsTaken = 0;
            searchTimer.Start();

            textStatus.Visibility = Visibility.Hidden;
            string[] queueResult = null;
            await Task.Run(() => { queueResult = Array.FindAll<string>(words.ToArray(), element => element.ToLower().Contains(searchTerm)); }); //Search the words array for all references of the search term
            results = queueResult.Length;
            listboxWords.Items.Clear();
            if (queueResult == null || queueResult.Length <= 0)
            {
                textStatus.Visibility = Visibility.Visible;
                textStatus.Text = "No results :(";
                textStats.Text = "";
                return;
            }
            if (queueResult.Length > maxResults && !maxResultsOverride)
            {
                textStatus.Visibility = Visibility.Visible;
                textStatus.Text = "Too many results";
                textStats.Text = "";
                bSearchAnyway.Visibility = Visibility.Visible;
                return;
            }
            maxResultsOverride = false;
            listboxWords.Items.Add("Loading..."); //Temporary "Loading..." entry, gets replaced when an absolute match is found
            foreach (string word in queueResult)
            {
                //Set up a Grid to be added to the ListBox
                Grid grid = new Grid() { HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0) };
                Button bCopy = new Button() { Content = "Copy", HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(1, 1, 46, 1), Width = 32 };
                bCopy.Click += bCopy_Click;
                Button bDefine = new Button() { Content = "Define", HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(1), Width = 40 };
                bDefine.Click += bDefine_Click;
                grid.Children.Add(new TextBlock() { Text = word, HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(1), VerticalAlignment = VerticalAlignment.Center });
                grid.Children.Add(bCopy);
                grid.Children.Add(bDefine);

                //Check if the search term is still the same, terminate the search if it isn't (user might still be typing)
                string currentSearchTerm = searchTerm;
                currentSearchTerm = tbSearch.Text.ToLower();
                if (searchTerm != currentSearchTerm)
                {
                    Search(currentSearchTerm);
                    return;
                }

                await Task.Run(() =>
                {
                    if (searchTerm.ToLower() == word.ToLower()) //absolute match
                    {
                        Dispatcher.Invoke(() => {
                            ((TextBlock)grid.Children[0]).Inlines.Clear();
                            ((TextBlock)grid.Children[0]).Inlines.Add(new Bold(new Run(word)));
                            listboxWords.Items[0] = grid;
                        });
                    }
                    else if (word.StartsWith(searchTerm)) //"similar" match
                        Dispatcher.Invoke(() => { listboxWords.Items.Insert(1, grid); });
                    else //contains the search term somewhere
                        Dispatcher.Invoke(() => { listboxWords.Items.Add(grid); });
                });
            }
            if (listboxWords.Items[0].ToString() == "Loading...")
                listboxWords.Items.RemoveAt(0);

            searchTimer.Stop();
            textStats.Text = $"{results} results (in {tenthMillisecondsTaken / 10f}ms)";
            GC.Collect();
        }

        private void bDefine_Click(object sender, RoutedEventArgs e)
        {
            //Search Wiktionary for the word in question
            Grid gridSender = (Grid)((Button)sender).Parent;
            TextBlock textBlock = (TextBlock)gridSender.Children[0];
            Process process = new Process();
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.FileName = @"https://en.wiktionary.org/w/index.php?search=" + textBlock.Text;
            process.Start();
        }

        private void bCopy_Click(object sender, RoutedEventArgs e)
        {
            Button bSender = (Button)sender;
            bSender.Content = "Copied!";
            bSender.Width = 46;
            Grid gridSender = (Grid)bSender.Parent;
            TextBlock textBlock = (TextBlock)gridSender.Children[0];
            Clipboard.SetText(textBlock.Text);
        }

        private void tbSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            if (listboxWords.Items.Count <= 0)
            {
                textStatus.Text = "Enter a search term to get started";
                textStatus.Visibility = Visibility.Visible;
            }
        }

        private void searchTimer_Tick(object sender, EventArgs e)
        {
            tenthMillisecondsTaken++;
        }

        private void bConfigure_Click(object sender, RoutedEventArgs e)
        {
            words.Clear();
            listboxWords.Items.Clear();
            tbSearch.Text = "Search...";
            textStatus.Text = "Enter a search term to get started";
            textStatus.Visibility = Visibility.Visible;
            string filePath = DatabaseHelper.DetermineBestDatabase();
            LoadWords(filePath);
        }

        private void bSearchAnyway_Click(object sender, RoutedEventArgs e)
        {
            maxResultsOverride = true;
            Search(tbSearch.Text.ToLower());
            bSearchAnyway.Visibility = Visibility.Hidden;
        }
    }
}
