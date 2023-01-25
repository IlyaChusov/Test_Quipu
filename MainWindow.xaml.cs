using System.Windows;
using System.IO;
using Microsoft.Win32;
using System;
using System.Threading.Tasks;
using System.Net.Http;
using HtmlAgilityPack;
using System.Linq;
using System.Windows.Documents;
using System.Collections.Generic;
using System.Windows.Shapes;

namespace Test_Quipu
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private readonly List<Site> sites = new List<Site>();

        private void SelectBut_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Text files (*.txt)|*.txt";
            fileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (fileDialog.ShowDialog() == true)
            {
                string filePath = fileDialog.FileName;
                PathBox.Text = filePath;
                scanFile(filePath);
                
            }
        }

        private void scanFile(string path)
        {
            sites.Clear();
            using (StreamReader reader = new StreamReader(path))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (validateUrl(line, out Uri siteUrl))
                    {
                        sites.Add(new Site() { siteUrl = siteUrl, tagsCount = 0 });
                    }
                }
            }
            ProgressBar.Maximum= sites.Count;
            UrlsView.ItemsSource = null;
            UrlsView.ItemsSource = sites;
        }

        public class Site
        {
            public Uri siteUrl { get; set; }
            public int tagsCount { get; set; }
        }

        private async void ActionBut_Click(object sender, RoutedEventArgs e)
        {
            ProgressBar.Value = 0;
            foreach (Site site in sites)
            {
                var response = await OpenUrl(site.siteUrl);

                if (response != null)
                {
                    site.tagsCount = countATags(response);
                    
                }
                else
                {
                    site.tagsCount = -1;
                }
                UrlsView.Items.Refresh();
                ProgressBar.Value += 1;
            }

            
            //MessageBox.Show("done!");

        }

        private async Task<string> OpenUrl(Uri url)
        {
            HttpClient client = new HttpClient();
            
            try 
            { 
                return await client.GetStringAsync(url);
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        private int countATags(string page)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(page);
            return document.DocumentNode.Descendants("a").ToList().Count;
            
        }

        private bool validateUrl(string url, out Uri validUrl)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out validUrl)
                && (validUrl.Scheme == Uri.UriSchemeHttp || validUrl.Scheme == Uri.UriSchemeHttps);
        }
    }
}
