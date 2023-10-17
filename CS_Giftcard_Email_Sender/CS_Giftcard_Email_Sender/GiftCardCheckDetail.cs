using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS_Giftcard_Email_Sender
{
    
    internal class GiftCardCheckDetail : GiftCardRecipient
    {
        public int CheckDetailID { get; set; }
        public int CheckID { get; set; }
        public int Status { get; set; }
        public int Type { get; set; }
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Qty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal UnitPrice2 { get; set; }
        public decimal DiscountApplied { get; set; }
        public int TaxID { get; set; }
        public decimal? TaxPercent { get; set; }
        public string VoidNotes { get; set; }
        public string CID { get; set; }
        public string VID { get; set; }
        public decimal? BonusValue { get; set; }
        public decimal? PaidValue { get; set; }
        public decimal? ComValue { get; set; }
        public int? Entitle1 { get; set; }
        // ... Similarly for other Entitle columns
        public decimal? G_Points { get; set; }
        public int? G_CustID { get; set; }
        // ... Continue with other properties
        public int CadetQty { get; set; }
        public string CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public int? HeatNo { get; set; }
        public Guid? TransferID { get; set; }
        public bool? P_IsCadet { get; set; }
        public int? M_ProductId { get; set; }
    }
}
