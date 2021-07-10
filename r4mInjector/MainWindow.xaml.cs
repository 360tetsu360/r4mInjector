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
using System.Collections.Specialized;

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

        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter, int x,
    int y, int width, int height, uint flags);

        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr
    lParam);

        const int GWL_EXSTYLE = -20;
        const int WS_EX_DLGMODALFRAME = 0x0001;
        const int SWP_NOSIZE = 0x0001;
        const int SWP_NOMOVE = 0x0002;
        const int SWP_NOZORDER = 0x0004;
        const int SWP_FRAMECHANGED = 0x0020;
        const uint WM_SETICON = 0x0080;

        private ObservableCollection<Dto> _dtos = new ObservableCollection<Dto>();
        ConsoleWindow console;
        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            Dlllist.ItemsSource = _dtos;
            console = new ConsoleWindow();
            console.Hide();

            StringCollection files = Properties.Settings.Default.RecentFiles;
            if(files == null)
            {
                Properties.Settings.Default.RecentFiles= new StringCollection();
            }
            if(files != null)
            {
                foreach (string file in files)
                {
                    FileInfo fileInfo = new FileInfo(file);

                    if (fileInfo.Directory.Exists)
                    {
                        string fileName = Path.GetFileName(file);
                        _dtos.Add(new Dto(fileName, file));
                    }
                }
            }
            this.Icon = BitmapSource.Create(1, 1, 96, 96, PixelFormats.Bgra32, null, new byte[] { 0, 0, 0, 0 }, 4);
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
                _dtos.Insert(0,new Dto(fileName, ofd.FileName));
                bool isinprop = false;
                StringCollection files = Properties.Settings.Default.RecentFiles;
                if (files != null)
                {
                    foreach (string file in files)
                    {
                        if (fileName == file)
                        {
                            isinprop = true;
                        }
                    }
                }
                if (isinprop)
                {
                    Properties.Settings.Default.RecentFiles.Remove(fileName);
                }
                Properties.Settings.Default.RecentFiles.Insert(0,ofd.FileName);
                Properties.Settings.Default.Save();
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
            Data.run = false;
            console.kill();
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            Data.run = false;

        }

        private void Window_Closed(object sender, EventArgs e)
        {
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
