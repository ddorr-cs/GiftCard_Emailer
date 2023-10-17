using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS_Giftcard_Email_Sender
{
    public class GiftCardEmailerRequest
    {
        public string GiftCardId { get; set; }
        public string SerializedSMTP { get; set; }
        public GiftCardTemplateRequest GiftCardTemplateRequest { get; set; }
        public string DefaultDriveLetter { get; set; }
        public SendGridSettings SendGridSettings { get; set; }
    }
}
