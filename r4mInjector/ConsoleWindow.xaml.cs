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
using System.Windows.Shapes;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Threading;
using System.IO;
namespace r4mInjector
{
    /// <summary>
    /// ConsoleWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ConsoleWindow : Window
    {
        ConsoleContent dc = new ConsoleContent();
        Thread t;
        public ConsoleWindow()
        {
            InitializeComponent();
            DataContext = dc;
            Loaded += ConsoleWindow_Loaded;
            t = new Thread(watcher);
            t.Start();
        }

        void ConsoleWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InputBlock.KeyDown += InputBlock_KeyDown;
            InputBlock.Focus();
        }

        void InputBlock_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                dc.ConsoleInput = InputBlock.Text;
                dc.RunCommand();
                InputBlock.Focus();
                Scroller.ScrollToBottom();
            }
        }
        void watcher()
        {
            string userName = Environment.UserName;
            string fileName = @"C:\Users\"+ userName +@"\AppData\Local\Packages\Microsoft.MinecraftUWP_8wekyb3d8bbwe\RoamingState\r4m.txt";
            FileInfo fileInfo = new FileInfo(fileName);

            if (!fileInfo.Directory.Exists)
            {
                fileInfo.Directory.Create();
            }
            StreamWriter writer = File.CreateText(fileName);
            Console.SetOut(writer);
            //FileStream fileStream = fileInfo.Create();

            using (FileStream fs = new FileStream
            (fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    while (!sr.EndOfStream)
                        sr.ReadLine();
                    while (Data.run)
                    {
                        while (!sr.EndOfStream)
                            Log(sr.ReadLine());
                        while (sr.EndOfStream)
                            Thread.Sleep(100);

                        Log(sr.ReadLine());
                    }
                }
            }
        }
        void Log(string log)
        {
            dc.ConsoleInput = log;
            dc.RunCommand();
        }

        public void kill()
        {
            t.Abort();
            this.Close();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            
        }
    }
    public class ConsoleDataCollection : ObservableCollection<string>
    {
        // スレッド間の排他ロックに利用するオブジェクト
        private object _lockObject = new object();

        public ConsoleDataCollection()
        {
            // 別スレッドからのデータ変更を可能にする
            System.Windows.Data.BindingOperations.EnableCollectionSynchronization(this, _lockObject);
        }
    }
    public class ConsoleContent : INotifyPropertyChanged
    {
        string consoleInput = string.Empty;
        ConsoleDataCollection consoleOutput = new ConsoleDataCollection() {
            "__________    _____    _____   ",
            "\\______   \\  /  |  |  /     \\  ",
            " |       _/ /   |  |_/  \\ /  \\ ",
            " |    |   \\/    ^   /    Y    \\",
            " |____|_  /\\____   |\\____|__  /",
            "        \\/      |__|        \\/ ",
            ""
        };

        public string ConsoleInput
        {
            get
            {
                return consoleInput;
            }
            set
            {
                consoleInput = value;
                OnPropertyChanged("ConsoleInput");
            }
        }

        public ConsoleDataCollection ConsoleOutput
        {
            get
            {
                return consoleOutput;
            }
            set
            {
                consoleOutput = value;
                OnPropertyChanged("ConsoleOutput");
            }
        }

        public void RunCommand()
        {
                ConsoleOutput.Add(ConsoleInput);
                // do your stuff here.
                ConsoleInput = String.Empty;
        }


        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string propertyName)
        {
            if (null != PropertyChanged)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
