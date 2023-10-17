using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS_Giftcard_Email_Sender
{
    internal class SMTPSettings
    {
        public string SMTPServer { get; set; }
        public string SMTPServerPort { get; set; }
        public string SMTPServerUseAuthentiation { get; set; }
        public string SMTPServerAuthenticationUserName { get; set; }
        public string SMTPServerAuthenticationPassword { get; set; }
        public string SMTPServerUseSSL { get; set; }
    }
}
