using System.Data.SqlClient;
using CS_Giftcard_Email_Sender;
using System.Globalization;
using System.Net.Mail;
using System.Net;
using Newtonsoft.Json;
using System.Linq;

// Using Windows Authentication (Integrated Security=True)
string connectionString = @"Data Source=.;Initial Catalog=clubspeedv8;Integrated Security=True;MultipleActiveResultSets=True";

using (SqlConnection connection = new SqlConnection(connectionString))
{
    connection.Open();
    Console.WriteLine("Connected to the database.");

    // Value to be passed into query
    int value;

    // Keep dialog open until a valid input is received
    while (true)
    {
        // Get user input for checkID
        Console.WriteLine("Please input a checkID");
        string checkID = Console.ReadLine();

        if (checkID != null && checkID.Length > 1 && int.TryParse(checkID, out value))
        {
            Console.WriteLine($"Collecting giftcard details attached to checkID: {checkID}");
            break;
        }
    }


    // Get list of gift card check details
    var giftCardDetailsList = new List<GiftCardCheckDetail>();

    string query = "";
    query = $@"SELECT 
	            GCR.GiftCardRecipientID,
	            GCR.CheckID,
	            GCR.CheckDetailID,
	            GCR.RecipientEmailAddress,
	            GCR.Note,
	            GCR.CreatedDate,
	            GCR.Sender,
	            CD.ProductName,
	            CD.G_Points,
                CD.G_CustID,
                CD.ProductID,
	            (SELECT TOP 1 LocationName FROM Locations) AS LocationName,
	            (SELECT TOP 1 SettingValue FROM ControlPanel WHERE SettingName = 'CurrentCulture') AS Culture
            FROM [dbo].[GiftCardRecipients] GCR 
	            LEFT JOIN [dbo].[CheckDetails] CD 
	            ON CD.CheckDetailID = GCR.CheckDetailID 
            WHERE GCR.CheckID = {value} AND CD.G_Points IS NOT NULL";

    SqlCommand command = new SqlCommand(query, connection);
    using (SqlDataReader reader = command.ExecuteReader())
    {
        while (reader.Read())
        {
            var giftcard_detail = new GiftCardCheckDetail
            {
                GiftCardRecipientID = (int)reader["GiftCardRecipientID"],
                CheckID = (int)reader["CheckID"],
                CheckDetailID = (int)reader["CheckDetailID"],
                RecipientEmailAddress = reader["RecipientEmailAddress"].ToString(),
                Note = reader["Note"].ToString(),
                CreatedDate = (DateTime)reader["CreatedDate"],
                Sender = reader["Sender"].ToString(),
                LocationName = reader["LocationName"].ToString(),
                Culture = reader["Culture"].ToString(),
                ProductName = reader["ProductName"].ToString(),
                ProductID = (int)reader["ProductID"],
                G_Points = reader["G_Points"] == DBNull.Value ? (decimal?)0 : (decimal)reader["G_Points"],
                G_CustID = reader["G_CustID"] == DBNull.Value ? 0 : (int)reader["G_CustID"]
            };

            giftCardDetailsList.Add(giftcard_detail);
        }

        foreach (var giftcard_detail in giftCardDetailsList)
        {
            Console.WriteLine($"GiftCardRecipientID: {giftcard_detail.GiftCardRecipientID}");
            Console.WriteLine($"CheckID: {giftcard_detail.CheckID}");
            Console.WriteLine($"CheckDetailID: {giftcard_detail.CheckDetailID}");
            Console.WriteLine($"RecipientEmailAddress: {giftcard_detail.RecipientEmailAddress}");
            Console.WriteLine($"Note: {giftcard_detail.Note}");
            Console.WriteLine($"CreatedDate: {giftcard_detail.CreatedDate}");
            Console.WriteLine($"Sender: {giftcard_detail.Sender}");
            Console.WriteLine($"LocationName: {giftcard_detail.LocationName}");
            Console.WriteLine($"Culture: {giftcard_detail.Culture}");
            Console.WriteLine($"ProductName: {giftcard_detail.ProductName}");
            Console.WriteLine($"ProductID: {giftcard_detail.ProductID}");
            Console.WriteLine($"G_Points: {giftcard_detail.G_Points}");
            Console.WriteLine("--------------------------");
        }
    }


    // Begin handling email creation and sending for each gift card
    if (giftCardDetailsList.Count < 1)
    {
        Console.WriteLine($"No giftcards were found for checkID: {value}");
    }
    else
    {
        Console.WriteLine("Creating email template to be sent...");
        //Send out the emails
        foreach (var giftcard_detail in giftCardDetailsList)
        {
            //Currency information
            CultureInfo culture = new CultureInfo(giftcard_detail.Culture);
            var currencySymbol = CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol;

            // Checking to see if we should add credits to the gift card
            bool shouldAssignCreditsToGiftCard = false;

            var productCustomCreditList = new List<ProductCustomCredit>();

            query = $"SELECT " +
                $"[ProductId]," +
                $"PC.[CustomCreditTypeId]," +
                $"CT.CustomCreditDescription," +
                $"[IsExpires]," +
                $"[DaysToExpiration]," +
                $"[Points]" +
                $" FROM [ClubspeedV8].[dbo].[ProductCustomCredit] PC " +
                $"LEFT JOIN [ClubspeedV8].[dbo].[CustomCreditType] CT ON PC.CustomCreditTypeId = CT.CustomCreditTypeId" +
                $" Where ProductId = {giftcard_detail.ProductID}";
            command = new SqlCommand(query, connection);

            //Get Products Custom Credits for credits on giftcards
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var productCustomCredit = new ProductCustomCredit()
                    {
                      Points = (decimal?)reader["Points"],
                      CustomCreditDescription = (string)reader["CustomCreditDescription"],
                      CustomCreditTypeId = (int)reader["CustomCreditTypeId"],
                      DaysToExpiration = (int)reader["DaysToExpiration"],
                      IsExpires = (bool)reader["IsExpires"],
                      ProductId = (int)reader["ProductId"]
                    };

                    productCustomCreditList.Add(productCustomCredit);
                }
            }

            // Recipient copies list (bcc)
            var RecipientCopyList = new List<string>();

            query = @"SELECT 
                    SettingValue
                    FROM ControlPanel
                    WHERE SettingName like 'sendReceiptCopyTo'";

            command = new SqlCommand(query, connection);

            //Get Products Custom Credits for credits on giftcards
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var recipientCopy = (string)reader["SettingValue"];
                    string[] temp = recipientCopy.Split(',');

                    foreach (string r in temp)
                    {
                        RecipientCopyList.Add(r.Trim());
                    }                    
                }
            }


            // Get sender's email
            string senderEmail = "";

            query = $@"select SettingValue from ControlPanel where SettingName like 'EmailWelcomeFrom'";
            command = new SqlCommand(query, connection);

            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var temp_senderEmail = (string)reader["SettingValue"];

                    senderEmail = temp_senderEmail;
                }
            }
            if (senderEmail.Length < 3)
            {
                senderEmail = "support@clubspeed.com";
            }


            // Get SendGrid Settings, If any
            SendGridSettings SendGridSettings = new SendGridSettings();

            query = $@"SELECT TOP 1
                    (SELECT TOP 1 SettingValue FROM ControlPanel WHERE SettingName = 'UseSendGridSmtpForGiftCards') AS UseSendGridSmtpForGiftCards,
                    (SELECT TOP 1 SettingValue FROM ControlPanel WHERE SettingName = 'SendGridUsername') AS SendGridUsername,
                    (SELECT TOP 1 SettingValue FROM ControlPanel WHERE SettingName = 'SendGridPassword') AS SendGridPassword
                    FROM ControlPanel";
            command = new SqlCommand(query, connection);

            using (SqlDataReader reader = command.ExecuteReader())
            {

                while (reader.Read())

                {
                    var temp_SendGridSettings = new SendGridSettings()
                    {
                        
                        UseSendGridSmtpForGiftCards = (string)reader["UseSendGridSmtpForGiftCards"] == "0" ? false: true,
                        SendGridUsername = (string)reader["SendGridUsername"],
                        SendGridPassword = (string)reader["SendGridPassword"]
                    };

                    SendGridSettings = temp_SendGridSettings;
                }
            }
            if (senderEmail.Length < 3)
            {
                senderEmail = "support@clubspeed.com";
            }


            // Serialize SMTP client
            var smtpClient = new SmtpClient();

            query = $@"SELECT TOP 1
                    (SELECT TOP 1 SettingValue FROM ControlPanel WHERE SettingName = 'SMTPServer') AS SMTPServer,
                    (SELECT TOP 1 SettingValue FROM ControlPanel WHERE SettingName = 'SMTPServerPort') AS SMTPServerPort,
                    (SELECT TOP 1 SettingValue FROM ControlPanel WHERE SettingName = 'SMTPServerUseAuthentiation') AS SMTPServerUseAuthentiation,
                    (SELECT TOP 1 SettingValue FROM ControlPanel WHERE SettingName = 'SMTPServerAuthenticationUserName') AS SMTPServerAuthenticationUserName,
                    (SELECT TOP 1 SettingValue FROM ControlPanel WHERE SettingName = 'SMTPServerAuthenticationPassword') AS SMTPServerAuthenticationPassword,
                    (SELECT TOP 1 SettingValue FROM ControlPanel WHERE SettingName = 'SMTPServerUseSSL') AS SMTPServerUseSSL
                    FROM ControlPanel";
            command = new SqlCommand(query, connection);

            //Get Products Custom Credits for credits on giftcards
            SMTPSettings smtpSettings = new SMTPSettings();
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var temp_smtpSettings = new SMTPSettings()
                    {
                        SMTPServer = (string)reader["SMTPServer"],
                        SMTPServerPort = (string)reader["SMTPServerPort"],
                        SMTPServerUseAuthentiation = (string)reader["SMTPServerUseAuthentiation"],
                        SMTPServerAuthenticationUserName = (string)reader["SMTPServerAuthenticationUserName"],
                        SMTPServerAuthenticationPassword = (string)reader["SMTPServerAuthenticationPassword"],
                        SMTPServerUseSSL = (string)reader["SMTPServerUseSSL"]
                    };
                    smtpSettings = temp_smtpSettings;
                }
            }
            smtpClient.Host = smtpSettings.SMTPServer;
            smtpClient.Port = Convert.ToInt32(smtpSettings.SMTPServerPort);
            var useAuthentication = smtpSettings.SMTPServerUseAuthentiation == "1" ? true : false;
            smtpClient.UseDefaultCredentials = !useAuthentication;
            if (useAuthentication)
            {
                smtpClient.Credentials = new NetworkCredential(smtpSettings.SMTPServerAuthenticationUserName, smtpSettings.SMTPServerAuthenticationPassword);
            }
            smtpClient.EnableSsl = smtpSettings.SMTPServerUseSSL == "1" ? true : false;
            smtpClient.Timeout = 10000;

            // Serialize SMTP client
            var serializedSMTP = JsonConvert.SerializeObject(smtpClient);

            // Get giftcard email template
            GiftcardEmailTemplate giftcardEmailTemplate = new GiftcardEmailTemplate();

            query = $@"SELECT TOP 1
                    (SELECT TOP 1 Value FROM Settings WHERE NAME = 'giftCardEmailBodyHtmlModern') AS TemplateBody,
                    (SELECT TOP 1 Value FROM Settings WHERE NAME = 'giftCardEmailSubject') AS TemplateSubject
                    FROM Settings";
            command = new SqlCommand(query, connection);
            //Get Products Custom Credits for credits on giftcards
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var temp_giftcardEmailTemplate = new GiftcardEmailTemplate()
                    {
                        TemplateBody = (string)reader["TemplateBody"],
                        TemplateSubject = (string)reader["TemplateSubject"]
                    };
                    giftcardEmailTemplate = temp_giftcardEmailTemplate;
                }
                
            }


            // Get shouldAssignCreditsToGiftCard, this is only for the boolean value, giftcard value should already be assigned.
            if (productCustomCreditList == null || productCustomCreditList.Count == 0 || productCustomCreditList[0].Points == 0)
            {
                shouldAssignCreditsToGiftCard = false;
            } else
            {
                shouldAssignCreditsToGiftCard = true;
            }

            new GiftCardEmailer(new GiftCardEmailerRequest()
            {
                GiftCardId = giftcard_detail.G_CustID.ToString(),
                DefaultDriveLetter = "C",//_giftCardItemDetails.GiftCardSettings.DefaultDriveLetter,
                SerializedSMTP = serializedSMTP,
                SendGridSettings = SendGridSettings,
                GiftCardTemplateRequest = new GiftCardTemplateRequest()
                {
                    Amount = giftcard_detail.G_Points.Value,
                    MoneyBalanceOnGiftCard = giftcard_detail.G_Points.Value.ToString(),
                    CreditBalanceOnGiftCard = shouldAssignCreditsToGiftCard ? productCustomCreditList.First().Points.Value + " " + productCustomCreditList.First().CustomCreditDescription : string.Empty,
                    Business = giftcard_detail.LocationName,
                    Template = giftcardEmailTemplate.TemplateBody,
                    EmailSubjectTemplate = giftcardEmailTemplate.TemplateSubject,
                    RecipientEmailAddress = giftcard_detail.RecipientEmailAddress,
                    FromEmailAddress = senderEmail,//giftcard_detail.Sender, //???
                    GiftCardFilePath = @"C:\clubspeedapps\assets\GiftCards\" + DateTime.Now.Month + DateTime.Now.Day + DateTime.Now.Year + DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second + DateTime.Now.Millisecond + ".png",
                    RecipientCopies = RecipientCopyList,
                    GiftCardId = giftcard_detail.G_CustID.ToString(),
                    SenderName = giftcard_detail.Sender,
                    ProductDescription = giftcard_detail.ProductName,
                    Note = giftcard_detail.Note,
                    CurrencySymbol = currencySymbol.ToString()

                }
            }).SendGiftCardViaEmail();
            Console.WriteLine("Pretend emails have been sent...");
        }
        
    }


    connection.Close();
    Console.WriteLine("Connection closed.");
}
