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
using System.Windows.Shapes;

namespace HTTP_Proxy
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            new HTTPPacket(@"GET http://ithappens.ru/ HTTP/1.1
Host: ithappens.ru
User-Agent: Mozilla/5.0 (Windows NT 6.1; WOW64; rv:21.0) Gecko/20100101 Firefox/21.0
Accept: text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8
Accept-Language: en-US,en;q=0.5
Accept-Encoding: gzip, deflate
Cookie: __utma=46536831.2135214079.1361979239.1369993890.1370070461.178; __utmz=46536831.1368320404.141.6.utmcsr=bash.im|utmccn=(referral)|utmcmd=referral|utmcct=/
Connection: keep-alive
");
            (new InetHandler(1)).startHandling();
        }
    }
}
