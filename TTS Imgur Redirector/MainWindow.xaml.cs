using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Threading;

namespace TTS_Imgur_Redirector
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        public readonly ProxyController Controller = new ProxyController();
        private bool _isProgramRunning;
        private bool _isProxyActive;


        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;
            var version = Assembly.GetEntryAssembly().GetName().Version.ToString(3);
            Title = $"TTS Imgur Redirector {version}";

            Controller.OnRequestIntercepted += AddItemToList;

            HistoryCollection = new ObservableCollection<ListItem>();

            TimerCheckProgram = new DispatcherTimer(DispatcherPriority.Background);
            TimerCheckProgram.Interval = TimeSpan.FromSeconds(1);
            TimerCheckProgram.Tick += CheckProgram;
            TimerCheckProgram.Start();
        }

        public ObservableCollection<ListItem> HistoryCollection { get; set; }

        public bool IsProxyActive
        {
            get => _isProxyActive;
            set
            {
                _isProxyActive = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsProgramRunning
        {
            get => _isProgramRunning;
            set
            {
                _isProgramRunning = value;
                NotifyPropertyChanged();
            }
        }

        public DispatcherTimer TimerCheckProgram { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void CheckProgram(object sender, EventArgs e)
        {
            var pname = Process.GetProcessesByName("Tabletop Simulator");
            IsProgramRunning = pname.Length != 0;
        }

        private void ToggleProxy(object sender, RoutedEventArgs e)
        {
            if (IsProxyActive)
            {
                Controller.Stop();
                IsProxyActive = false;
            }
            else
            {
                Controller.Start();
                IsProxyActive = true;
            }
        }

        private void AddItemToList(string s)
        {
            var item = new ListItem
            {
                Time = DateTime.Now.ToLongTimeString(),
                Url = s
            };
            Dispatcher.Invoke(() => HistoryCollection.Add(item));
        }

        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            if (IsProxyActive)
                Controller.Stop();
        }

        private void Github_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(((Hyperlink) sender).NavigateUri.ToString());
        }
    }

    public class ListItem
    {
        public string Time { get; set; }
        public string Url { get; set; }
    }
}