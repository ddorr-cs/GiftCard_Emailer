using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS_Giftcard_Email_Sender
{
    public class GiftCardRecipient
    {
        public int GiftCardRecipientID { get; set; }
        public int CheckID { get; set; }
        public int CheckDetailID { get; set; }
        public string RecipientEmailAddress { get; set; }
        public string Note { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Sender { get; set; }
        public string LocationName { get; set; }
        public string Culture { get; set; }
    }
}
