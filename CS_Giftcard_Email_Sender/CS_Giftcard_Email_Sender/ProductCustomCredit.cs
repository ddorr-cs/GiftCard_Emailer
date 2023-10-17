using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS_Giftcard_Email_Sender
{
    internal class ProductCustomCredit
    {
        public int ProductId { get; set; }
        public int CustomCreditTypeId { get; set; }
        public string CustomCreditDescription { get; set; }
        public bool IsExpires { get; set; }
        public int DaysToExpiration { get; set; }
        public decimal? Points { get; set; }
    }
}
