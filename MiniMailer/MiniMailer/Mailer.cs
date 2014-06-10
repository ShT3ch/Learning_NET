using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniMailer
{
    class Mailer
    {
        private string pass;
        private string user;
        private string server;
        private string port;

        private int state;
        private Dictionary<bool, Dictionary<int, int>> automata; 

        private string data;

        public Mailer(string user, string pass, string server, string port)
        {
            this.user = user;
            this.pass = pass;
            this.server = server;
            this.port = port;

            
        }
    }
}
