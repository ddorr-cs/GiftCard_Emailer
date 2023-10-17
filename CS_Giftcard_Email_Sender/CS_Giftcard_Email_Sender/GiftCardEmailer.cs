using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Drawing.Imaging;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CS_Giftcard_Email_Sender
{
    public class GiftCardEmailer
    {
        const string giftCardPathMinusDriveLetter = @":\clubspeedapps\assets\GiftCards\";
        private readonly GiftCardEmailerRequest _giftCardEmailerRequest;
        public GiftCardEmailer(GiftCardEmailerRequest giftCardEmailerRequest)
        {
            _giftCardEmailerRequest = giftCardEmailerRequest;
        }
        public void SendGiftCardViaEmail()
        {

            var useSendGridSmtpForGiftCards = _giftCardEmailerRequest.SendGridSettings.UseSendGridSmtpForGiftCards;

            if (!useSendGridSmtpForGiftCards)
            {

                var QRImage = new QRGenerator(_giftCardEmailerRequest.GiftCardId).GetQRData();
                Directory.CreateDirectory(_giftCardEmailerRequest.DefaultDriveLetter + giftCardPathMinusDriveLetter);
                QRImage.Save(_giftCardEmailerRequest.GiftCardTemplateRequest.GiftCardFilePath, ImageFormat.Png);
                var serializedSMTP = JsonConvert.DeserializeObject<SmtpClient>(_giftCardEmailerRequest.SerializedSMTP); // this mailer already has the SMTP Settings baked into it
                var mailMessage = new GiftCardTemplateHelper(_giftCardEmailerRequest.GiftCardTemplateRequest).GetMailMessage();// this essentially gets the body of the email
                // Logger.Log("About to send a $" + Math.Round(_giftCardEmailerRequest.GiftCardTemplateRequest.Amount, 2) + " gift card to " + mailMessage.To);
                new Mailer(serializedSMTP).SendMail(mailMessage);
                RemoveGeneratedQRImages();
            }
            else
            {
                // need to fix this method
                var mailMessage = new GiftCardTemplateHelperSendGrid(_giftCardEmailerRequest.GiftCardTemplateRequest).GetMailMessage();// this essentially gets the body of the email
                // Logger.Log("About to send a $" + Math.Round(_giftCardEmailerRequest.GiftCardTemplateRequest.Amount, 2) + " gift card to " + mailMessage.To);
                using (SmtpClient client = new SmtpClient("smtp.sendgrid.net"))
                {
                    var sendGridUsername = _giftCardEmailerRequest.SendGridSettings.SendGridUsername;
                    var sendGridPassword = _giftCardEmailerRequest.SendGridSettings.SendGridPassword;
                    client.Port = 587;
                    client.Credentials = new NetworkCredential(sendGridUsername, sendGridPassword);
                    client.Send(mailMessage);
                }
            }
        }
        /// <summary>
        ///The process before this method adds a QR code gift card image in clubspeedapps/assets/giftcards. This is exposed to the outside world, 
        ///so if someone did a web scrape they could find all of the gift cards QR Images. That would be bad. 
        ///This will remove all images in the clubspeedapps/assets/giftcards directory. The current image generated in the code above cannot be removed because it is still in memory, 
        ///but the next gift card that goes through this process will remove the previous images etc. Once the image is reomved, the QR code/image in the email will still be visible.
        /// </summary>
        private void RemoveGeneratedQRImages()
        {
            DirectoryInfo di = new DirectoryInfo(_giftCardEmailerRequest.DefaultDriveLetter + giftCardPathMinusDriveLetter);
            foreach (FileInfo file in di.GetFiles())
            {
                try
                {
                    file.Delete();
                }
                catch (Exception ex)
                {

                }
            }
        }
    }
}
