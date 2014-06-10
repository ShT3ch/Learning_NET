using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MiniMailerWPF
{
    class Answer
    {
        private string Code { get; set; }
        private string Comment { get; set; }
    
        
        private static readonly Regex GoodTemplate = new Regex("^([23]|\\n[23])");

        public Answer(string code, string comment)
        {
            Code = code;
            Comment = comment;
        }

        public bool IsOk()
        {
            return (GoodTemplate.Match(Code).Success);
        }

        public bool IsContinuated()
        {
            return Code.StartsWith("250");
        }

        public bool IsReady()
        {
            return Code.StartsWith("250 ");
        }
    }

    class MiniLexer
    {
        static private bool isDigit(char sym)
        {
            return char.IsDigit(sym);
        }

        static private bool isSpace(char sym)
        {
            return char.IsWhiteSpace(sym) || sym.Equals('-');
        }

        static public Answer LexIt(string str)
        {
            string code ="";
            string comment ="";

            bool stateComment = false;

            for (int i = 0; i < str.Length; i++)
            {
                if (!stateComment)
                {
                    if (isDigit(str.ElementAt(i)))
                    {
                        code += str.ElementAt(i);
                    }
                    if (isSpace(str.ElementAt(i)))
                    {
                        stateComment = true;
                    }
                }

                if (stateComment)
                {
                    comment += str.ElementAt(i);
                }
            }

            return new Answer(code, comment);

        }

    }

    class SMTPController
    {
        private readonly SMTPGraphicDataModel   DataModel;
        private readonly MainWindow.Alarm       Alarmer;
        private readonly MIMEFormer             Mail;
        private          InetOperator           Telnet;


        private const string SuccessBeginOfRecv = "2";

        private String LastServerAnswer;

        private bool Logined;

        public SMTPController(SMTPGraphicDataModel data, MainWindow.Alarm alarm)
        {
            DataModel = data;
            Alarmer = alarm;
            Mail = new MIMEFormer(data);
        }

        private string GetLastComments()
        {
            var listOfRecv = LastServerAnswer.Split('\r');
            return listOfRecv.Aggregate("", (current, line) => current + line.Substring(Math.Max(line.IndexOf(' '),0))+"\r\n");
        }

        private void RecvLn()
        {
            LastServerAnswer = Telnet.RecieveLn();
        }

        private bool IsLastTimeSuccess()
        {

            var listOfRecv = LastServerAnswer.Split('\r');

//            listOfRecv.Aggregate((y, x) => x = x.Split('\n').Last());

            var goodTemplate = new Regex("^([23]|\\n[23])");

            if (listOfRecv.All(x => goodTemplate.Match(x).Success || x.Equals("\n")))
            {
                return true;
            }
            else
            {
                throw new Exception(GetLastComments());
                return false;
            }
        }

        public void Connect()
        {
            Telnet = new InetOperator(DataModel.Server, int.Parse(DataModel.Port), Alarmer);
            LastServerAnswer = Telnet.RecieveLn();
            Logined = false;
        }

        public bool IsConnected()
        {
            if (Telnet != null)
            {
                if (Telnet.isConnected()) return true;
            }
            return false;
        }

        private void HELO()
        {
            if (IsLastTimeSuccess())
            {
                LastServerAnswer = Telnet.SendRecieve("EHLO 1.1");
            }
        }

        public void ExportMailBody(string path)
        {
            var file = File.Open(path,FileMode.CreateNew);

            var mail = Encoding.UTF8.GetBytes(Mail.GenerateMailBody());

            file.Write(mail,0,mail.Length);
            
            file.Flush();

            file.Close();
        }

        private void AuthLogin()
        {
            if (!Logined)
            {

                if (IsLastTimeSuccess())
                {
                    LastServerAnswer = Telnet.SendRecieve("AUTH LOGIN");
                }

                if (IsLastTimeSuccess())
                {
                    LastServerAnswer = Telnet.SendRecieve(Mail.ToBase64(DataModel.User));
                }

                if (IsLastTimeSuccess())
                {
                    LastServerAnswer = Telnet.SendRecieve(Mail.ToBase64(DataModel.Pass));
                }

                if (IsLastTimeSuccess())
                {
                    Logined = true;
                }
            }
        }

        private void SetMailer()
        {
            if (IsLastTimeSuccess())
            {
                LastServerAnswer = Telnet.SendRecieve(String.Format("MAIL FROM: <{0}>", DataModel.User));
            }
        }

        private void AddReciever(string to)
        {
            if (IsLastTimeSuccess())
            {
                LastServerAnswer = Telnet.SendRecieve(String.Format("RCPT TO: <{0}>", to));
            }
        }

        public void resetPanic()
        {
            LastServerAnswer = "3";
        }

        public void Send()
        {

            if (!IsConnected())
            {
                Connect();
            }

            if (IsConnected())
            {
                HELO();
                AuthLogin();

                SetMailer();

                foreach (var reciever in DataModel.Recievers)
                {
                    AddReciever(reciever);
                }

                if (IsLastTimeSuccess())
                {
                    LastServerAnswer = Telnet.SendRecieve("DATA");
                }

                if (IsLastTimeSuccess())
                {
                    Telnet.Send(Mail.GenerateMailBody());
                }

                if (IsLastTimeSuccess())
                {
                    Telnet.SendRecieve(".");
                }

                if (IsLastTimeSuccess())
                {
                    Alarmer("MailSend");
                }
            }
        }
    }
}
