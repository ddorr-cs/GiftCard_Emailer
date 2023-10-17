using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net.Mime;
using System.Drawing;
using System.Drawing.Imaging;
using Gma.QrCodeNet.Encoding.Windows.Render;
using Gma.QrCodeNet.Encoding;

namespace CS_Giftcard_Email_Sender
{
    public class GiftCardTemplateHelperSendGrid
    {
        private readonly GiftCardTemplateRequest _giftCardTemplateRequest;
        public GiftCardTemplateHelperSendGrid(GiftCardTemplateRequest giftCardTemplateRequest)
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

        private Bitmap GenerateQRCode(string text)
        {
            QrEncoder qrEncoder = new QrEncoder(ErrorCorrectionLevel.H);
            QrCode qrCode = qrEncoder.Encode(text);

            GraphicsRenderer renderer = new GraphicsRenderer(new FixedModuleSize(4, QuietZoneModules.Two), Brushes.Black, Brushes.White);
            MemoryStream stream = new MemoryStream();
            renderer.WriteToStream(qrCode.Matrix, ImageFormat.Png, stream);
            return new Bitmap(stream);
        }

        private AlternateView GetAlternateView()
        {
            var template = _giftCardTemplateRequest.Template;
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
            template = template.Replace("{{giftCardImage}}", @"<img src=cid:qrCode>");
            AlternateView alternateView = AlternateView.CreateAlternateViewFromString(template, null, MediaTypeNames.Text.Html);

            Bitmap qrCode = GenerateQRCode(_giftCardTemplateRequest.GiftCardId);

            // Save the QR code image to a memory stream
            MemoryStream stream = new MemoryStream();
            qrCode.Save(stream, ImageFormat.Png);
            stream.Position = 0;

            alternateView.LinkedResources.Add(new LinkedResource(stream, "image/png") { ContentId = "qrCode" });

            return alternateView;
        }

    }
}
