using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Collections.ObjectModel;
using System.Threading;

namespace MusicFolderChecker
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public string rootPath;
        public ObservableCollection<string> directories = new ObservableCollection<string>();
        public ObservableCollection<string> files = new ObservableCollection<string>();

        public Window1()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tree.ItemsSource = directories;
            contents.ItemsSource = files;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            directories.Clear();

            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.ShowDialog();
            if (dlg.SelectedPath != "")
            {
                rootPath = dlg.SelectedPath;
                
                DirectoryInfo directory = new DirectoryInfo(rootPath);
                DirectoryInfo[] allDirectories = directory.GetDirectories("*", SearchOption.AllDirectories);

                string originalButtonText = chooseFolderButton.Content as string;
                ProgressBar progress = new ProgressBar();
                chooseFolderButton.Content = progress;
                progress.Width = chooseFolderButton.ActualWidth;
                progress.Height = chooseFolderButton.ActualHeight;
                progress.Maximum = allDirectories.Length;
                progress.Minimum = 0;
                progress.Value = 0;

                new Thread(new ThreadStart(delegate
                {
                    foreach (DirectoryInfo subFolder in allDirectories)
                    {
                        if (subFolder.GetDirectories().Length == 0 &&
                            subFolder.GetFiles("*.mp3").Length == 0 &&
                            subFolder.GetFiles("*.m4a").Length == 0 &&
                            subFolder.GetFiles("*.m4p").Length == 0 &&
                            subFolder.GetFiles("*.m4r").Length == 0 &&
                            subFolder.GetFiles("*.mp4").Length == 0 &&
                            subFolder.GetFiles("*.mpg").Length == 0)
                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new VoidDelegate(delegate
                            {
                                directories.Add(subFolder.FullName.Substring(rootPath.Length + 1));
                            }));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new VoidDelegate(delegate
                        {
                            progress.Value++;
                        }));
                    }
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new VoidDelegate(delegate
                    {
                        chooseFolderButton.Content = originalButtonText;
                    }));
                })).Start();
            }
        }
        delegate void VoidDelegate();

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (tree.SelectedItems.Count > 0)
            {
                if (System.Windows.MessageBox.Show("Are you SURE?", "Verify DELETE", MessageBoxButton.OKCancel, MessageBoxImage.Warning)
                    == MessageBoxResult.OK)
                {
                    List<string> directoriesToDelete = new List<string>();
                    foreach (string path in tree.SelectedItems)
                    {
                        Directory.Delete(rootPath + "\\" + path, true);
                        directoriesToDelete.Add(path);
                    }
                    tree.SelectedItem = null;
                    foreach (string deadDirectory in directoriesToDelete)
                    {
                        directories.Remove(deadDirectory);
                    }
                }
            }
        }

        private void tree_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            files.Clear();
            bool usePath = (tree.SelectedItems.Count > 1);
            foreach (string path in tree.SelectedItems)
            {
                DirectoryInfo directory = new DirectoryInfo(rootPath + "\\" + path);
                foreach (FileInfo file in directory.GetFiles())
                {
                    if (usePath)
                        files.Add(path + file.Name);
                    else
                        files.Add(file.Name);
                }
            }
        }
    }
}
