using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using MiniMailerWPF.Annotations;
using Timer = System.Timers.Timer;

namespace MiniMailerWPF
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>

    public class SMTPGraphicDataModel : INotifyPropertyChanged
    {

        private string port;
        public string Port
        {
            get { return port; }
            set
            {
                port = value;
                OnPropertyChanged();
            }
        }

        private string server;
        public string Server
        {
            get { return server; }
            set
            {
                server = value;
                OnPropertyChanged();
            }
        }

        private string subject;
        public string Subject
        {
            get { return subject; }
            set
            {
                subject = value;
                OnPropertyChanged();
            }
        }

        private string to;
        public string To
        {
            get { return to; }
            set
            {
                to = value;
                Recievers.RemoveAll(x => true);
                Recievers.AddRange(to.Split(new string[]{" ", ","}, StringSplitOptions.RemoveEmptyEntries));
                OnPropertyChanged();
            }
        }

        private string user;
        public string User {
            get { return user; }
            set
            {
                user = value;
                OnPropertyChanged();
            }
        }

        private string pass;
        public string Pass
        {
            get { return pass; }
            set
            {
                pass = value;
                OnPropertyChanged();
            }
        }


        private string _letterText;
        public string LetterText
        {
            get { return _letterText; }
            set
            {
                _letterText = value;
                OnPropertyChanged();
            }
        }

        public new ObservableCollection<String> Files = new ObservableCollection<string>();
        public List<string> Recievers = new List<string>(); 

        public string NewFileAdd {
            get { return Files.Last(); }
            set
            {
                if (File.Exists(value))
                {
                    Files.Add(value);
                }
                else
                {
                    System.Windows.MessageBox.Show("Всё тлен.");
                }
            }   
        }
 
        public SMTPGraphicDataModel()
        {
            Server = @"SMTP.mail.ru";
            Port = "587";
            Subject = "Something important";
            To = "testofpop3@mail.ru";
            User = "testofpop3@mail.ru";
            Pass = "testpop3";
            LetterText = "Ur text";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public partial class MainWindow : Window
    {

        private readonly SMTPGraphicDataModel data;
        private readonly SMTPController ProtocolController;
        private readonly Timer indicator = new Timer(1000);

        public delegate void Alarm(string message);

        public void createAlarm(string message)
        {
            var messageBox = MessageBox.Show(message);
        }

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = data = new SMTPGraphicDataModel();
            FileList.ItemsSource = data.Files;
            ProtocolController = new SMTPController(data,new Alarm(createAlarm));
            indicator.Elapsed+=IndicatorOnElapsed;
            indicator.Start();
        }

        private void IndicatorOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            ConnectIndicatorHandler();
        }

        private void ConnectIndicatorHandler()
        {
            if (ProtocolController != null)
            {
                if (ProtocolController.IsConnected())
                {
                    lock (ConnectedIndicator)
                    {

                        ConnectedIndicator.Dispatcher.BeginInvoke(new ThreadStart(() => ConnectedIndicator.Fill = Brushes.Green));
                    }
                }
                else
                {
                    lock (ConnectedIndicator)
                    {

                        ConnectedIndicator.Dispatcher.BeginInvoke(new ThreadStart(() => ConnectedIndicator.Fill = Brushes.Red));
                    }
                }
            }
        }

        private void Pass_PasswordChanged(object sender, RoutedEventArgs e)
        {
            data.Pass = this.Pass.Password;
        }

        private void LoadTextFile(object sender, RoutedEventArgs e)
        {
            var messageBox = new Microsoft.Win32.OpenFileDialog();
            if (messageBox.ShowDialog().Value)
            {
                var file = new StreamReader(messageBox.OpenFile());
                var result = file.ReadToEnd();
                data.LetterText = result;
            }
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            ProtocolController.Connect();
        }

        private void AddFile_Click(object sender, RoutedEventArgs e)
        {
            var messageBox = new Microsoft.Win32.OpenFileDialog {Multiselect = true};
            if (messageBox.ShowDialog().Value)
            {
                foreach (var file in messageBox.FileNames)
                {
                    data.Files.Add(file);
                }
            }
        }

        private void FileList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var list = (System.Windows.Controls.ListBox) sender;

            data.Files.Remove(list.SelectedItems[0].ToString());
        }

        private void Send_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                ProtocolController.Send();
            }
            catch (Exception exc)
            {
                createAlarm("Mailing failed: "+exc.Message);
                ProtocolController.resetPanic();
            }
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            var messageSaveFileBox = new Microsoft.Win32.SaveFileDialog();

            messageSaveFileBox.ShowDialog();

            ProtocolController.ExportMailBody(messageSaveFileBox.FileName);
        }
    }
}
