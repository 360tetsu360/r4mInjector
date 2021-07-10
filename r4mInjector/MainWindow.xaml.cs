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
using System.Runtime.InteropServices;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;

namespace r4mInjector
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
        }
        private ObservableCollection<Dto> _dtos = new ObservableCollection<Dto>();
        ConsoleWindow console;
        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            Dlllist.ItemsSource = _dtos;
            console = new ConsoleWindow();
            console.Hide();
            //MessageBox.Show(Properties.Settings.Default["b"].ToString());
            //Properties.Settings.Default["b"] = "async";
            //Properties.Settings.Default.Save();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.FileName = "";
            ofd.DefaultExt = "*.*";
            ofd.Filter = "DLL|*.dll";
            if (ofd.ShowDialog() == true)
            {
                string fileName = Path.GetFileName(ofd.FileName);
                _dtos.Add(new Dto(fileName,ofd.FileName));


            }
        }

        private void injectClick(object sender, RoutedEventArgs e)
        {
            if(Dlllist.SelectedItem != null)
            {
                Injector.awaitProcess("Minecraft.Windows");
                string dllpath = (Dlllist.SelectedItem as Dto).FilePath;
                if (File.Exists(dllpath))
                {
                    Injector.applyAppPackages(dllpath);
                    Injector.InjectDll(dllpath);
                }
                else
                {
                    MessageBox.Show("Select a dll");
                }
            }
            else
            {
                MessageBox.Show("Select a dll");
            }
        }

        private void OpenLogger(object sender, RoutedEventArgs e)
        {
            console.Show();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            console.Close();
            Data.run = false;
        }
    }
    public sealed class Dto
    {
        public Dto(string name,string path)
        {
            Name = name;
            FilePath = path;
        }

        public string Name { get; set; }
        public string FilePath { get; set; }
    }
}
