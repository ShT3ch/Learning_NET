using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HTTP_Proxy
{
    class HTTPPacket
    {
        private string text;
        private HttpRequestHeader Packet;
        
        public HTTPPacket(string body)
        {
            text = body;

            HttpRequestHeader pack;

            HttpRequestHeader.TryParse(body, true, out pack);

            var b = HttpRequestHeader.Cookie;
            
            var a = 1;

        }


    }
}
