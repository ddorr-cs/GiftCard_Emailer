using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS_Giftcard_Email_Sender
{
    public class GiftCardTemplateRequest
    {
        public decimal Amount { get; set; }
        public string FromEmailAddress { get; set; }
        public List<string> RecipientCopies { get; set; }
        public string Template { get; set; }
        public string Business { get; set; }
        public string RecipientEmailAddress { get; set; }
        public string MoneyBalanceOnGiftCard { get; set; }
        public string CreditBalanceOnGiftCard { get; set; }
        public string GiftCardFilePath { get; set; }
        public string GiftCardId { get; set; }
        public string ProductDescription { get; set; }
        public string SenderName { get; set; }
        public string Note { get; set; }
        public string EmailSubjectTemplate { get; set; }
        public string CurrencySymbol { get; set; }
    }
}
