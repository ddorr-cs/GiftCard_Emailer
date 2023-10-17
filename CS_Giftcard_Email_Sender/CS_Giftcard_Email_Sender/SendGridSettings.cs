using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS_Giftcard_Email_Sender
{
    public class SendGridSettings
    {
        public bool UseSendGridSmtpForGiftCards { get; set; }
        public string SendGridUsername { get; set; }
        public string SendGridPassword { get; set; }
    }
}
