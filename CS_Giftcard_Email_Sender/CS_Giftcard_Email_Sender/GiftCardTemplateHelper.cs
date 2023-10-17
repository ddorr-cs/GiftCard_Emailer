using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net.Mime;

namespace CS_Giftcard_Email_Sender
{
    public class GiftCardTemplateHelper
    {
        private readonly GiftCardTemplateRequest _giftCardTemplateRequest;
        public GiftCardTemplateHelper(GiftCardTemplateRequest giftCardTemplateRequest)
        {
            _giftCardTemplateRequest = giftCardTemplateRequest;
        }

        public MailMessage GetMailMessage()
        {
            var mailMessage = new MailMessage(_giftCardTemplateRequest.FromEmailAddress, _giftCardTemplateRequest.RecipientEmailAddress);
            mailMessage.IsBodyHtml = true;
            var subject = _giftCardTemplateRequest.EmailSubjectTemplate.Replace("{{business}}", _giftCardTemplateRequest.Business);
            mailMessage.Subject = subject;
            mailMessage.AlternateViews.Add(GetAlternateView());
            foreach (var recipient in _giftCardTemplateRequest.RecipientCopies)
            {
                mailMessage.Bcc.Add(recipient);
            }
            return mailMessage;
        }

        private string Round(decimal amount)
        {
            return Math.Round(amount, 2).ToString();
        }

        private AlternateView GetAlternateView()
        {
            var template = _giftCardTemplateRequest.Template;
            LinkedResource res = new LinkedResource(_giftCardTemplateRequest.GiftCardFilePath);
            res.ContentId = Guid.NewGuid().ToString();
            template = template.Replace("{{note}}", _giftCardTemplateRequest.Note);
            template = template.Replace("{{customer}}", _giftCardTemplateRequest.SenderName);

            template = template.Replace("{{note}}", _giftCardTemplateRequest.Note);
            template = template.Replace("{{to}}", _giftCardTemplateRequest.RecipientEmailAddress);
            template = template.Replace("{{from}}", _giftCardTemplateRequest.SenderName);

            template = template.Replace("{{business}}", _giftCardTemplateRequest.Business);
            template = template.Replace("{{giftCardNo}}", _giftCardTemplateRequest.GiftCardId);
            template = template.Replace("{{description}}", _giftCardTemplateRequest.ProductDescription);



            try
            {
                if (_giftCardTemplateRequest.MoneyBalanceOnGiftCard == "0.0000" && _giftCardTemplateRequest.CreditBalanceOnGiftCard != string.Empty)
                {
                    template = template.Replace("{{balance}}", _giftCardTemplateRequest.CreditBalanceOnGiftCard);
                }
                else if (_giftCardTemplateRequest.MoneyBalanceOnGiftCard != "0.0000" && _giftCardTemplateRequest.CreditBalanceOnGiftCard == string.Empty)
                {
                    template = template.Replace("{{balance}}", _giftCardTemplateRequest.CurrencySymbol + Round(Convert.ToDecimal(_giftCardTemplateRequest.MoneyBalanceOnGiftCard)));
                }
                else if (_giftCardTemplateRequest.MoneyBalanceOnGiftCard != "0.0000" && _giftCardTemplateRequest.CreditBalanceOnGiftCard != string.Empty)
                {
                    template = template.Replace("{{balance}}", _giftCardTemplateRequest.CurrencySymbol + Round(Convert.ToDecimal(_giftCardTemplateRequest.MoneyBalanceOnGiftCard)) + " + " + _giftCardTemplateRequest.CreditBalanceOnGiftCard);
                }
            }
            catch (Exception ex)
            {

            }
            template = template.Replace("{{date:Y-m-d}}", DateTime.Now.ToShortDateString());
            template = template.Replace("{{giftCardImage}}", @"<img src='cid:" + res.ContentId + @"'/>");
            AlternateView alternateView = AlternateView.CreateAlternateViewFromString(template, null, MediaTypeNames.Text.Html);
            alternateView.LinkedResources.Add(res);
            return alternateView;
        }

    }
}
