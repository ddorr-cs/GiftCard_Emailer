using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;

namespace CS_Giftcard_Email_Sender
{
    public class Mailer
    {
        private readonly SmtpClient _smtpClient;
        public Mailer(SmtpClient smtpClient)
        {
            _smtpClient = smtpClient;
        }

        public void SendMail(MailMessage mailMessage)
        {
            _smtpClient.Send(mailMessage);
        }
    }
}
