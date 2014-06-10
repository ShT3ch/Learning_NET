using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniMailerWPF
{
    class MIMEFormer
    {
        private readonly SMTPGraphicDataModel DataModel;

        private const string NewLine = "\r\n";

        private readonly string boundary;

        private string result;

        public MIMEFormer(SMTPGraphicDataModel dataModel)
        {
            DataModel = dataModel;
            boundary = BoundaryGenerator();
            result = "";
        }

        public string GenerateMailBody()
        {
            result = "";

            result += GenerateMailHeaders(DataModel.User, DataModel.Recievers, DataModel.Subject, boundary);
            result += NewLine;
            result += NewLine;

            result += GenerateBeginSeparator();
            result += NewLine;

            result += GenerateTextHeaders();
            result += NewLine;
            result += NewLine;

            var textData = ToBase64(DataModel.LetterText);
            for (int i = 0; i < textData.Length; i += 76)
            {
                var end = Math.Min(76, textData.Length-i);
                result += textData.Substring(i, end);
                result += NewLine;
            }
            

            foreach (var filename in DataModel.Files)
            {
                result += GenerateBeginSeparator();
                result += NewLine;
                result += GenerateFileHeaders(filename);
                result += NewLine;
                result += NewLine;

                textData = FileNameToBase64(filename);
                for (int i = 0; i < textData.Length; i += 76)
                {
                    var end = Math.Min(76, textData.Length - i);
                    result += textData.Substring(i, end);
                    result += NewLine;
                } 
            }

            result += GenerateEndSeparator();
            return result;
        }

        private string GenerateBeginSeparator()
        {
            return String.Format("--{0}", boundary);
        }

        private string GenerateEndSeparator()
        {
            return String.Format("--{0}--", boundary);
        }

        public string ToBase64(string target)
        {
            var bytes = Encoding.UTF8.GetBytes(target);
            return Convert.ToBase64String(bytes);
        }

        private string FileNameToBase64(string filename)
        {
            string result = "";
            if (!string.IsNullOrEmpty(filename))
            {
                var fs = new FileStream(filename,FileMode.Open,FileAccess.Read);
                var filebytes = new byte[fs.Length];
                fs.Read(filebytes, 0, Convert.ToInt32(fs.Length));
                string encodedData =
                Convert.ToBase64String(filebytes,
                Base64FormattingOptions.InsertLineBreaks);
                result = encodedData;
            }
            return result;
        }

        private string BoundaryGenerator()
        {
            var result = "ShtechBound";
            var rnd = new Random();

            result += rnd.Next(10000000, 99999999).ToString();
            result += rnd.Next(10000000, 99999999).ToString();
            result += rnd.Next(10000000, 99999999).ToString();
            result += rnd.Next(10000000, 99999999).ToString();
            return result;
        }

        private string GenerateFileHeaders(string filename)
        {
            return String.Format("Content-Type: application/octet-stream; charset=UTF-8; name=\"{0}\"", filename.Remove(0,1+ filename.LastIndexOf('\\'))) + NewLine +
                    String.Format("Content-Disposition: attachment; filename=\"{0}\"", filename.Remove(0,1+filename.LastIndexOf('\\'))) + NewLine +
                    "Content-Transfer-Encoding: base64";
        }

        private string GenerateTextHeaders()
        {
            return "Content-Type: text/plain; charset=UTF-8" + NewLine +
                    "Content-Transfer-Encoding: base64";
        }

        private string GenerateMailHeaders(string from, IEnumerable<string> to, string subject, string boundary)
        {
            var ans = "MIME-Version: 1.0" + NewLine +
                      String.Format("Subject: =?UTF-8?B?{0}?=" + NewLine, ToBase64(subject)) +
                      String.Format("From: MiniMailer <{0}>" + NewLine, from);

            var recievers = to.Aggregate("", (current, reciever) => current + (reciever + ", "));
            recievers = recievers.Remove(recievers.Length - 2);

            ans+=String.Format("To: <{0}>" + NewLine, recievers); 
                  ans+= String.Format("Content-Type: multipart/mixed; boundary={0}", boundary);

            return ans;
        }

    }
}
