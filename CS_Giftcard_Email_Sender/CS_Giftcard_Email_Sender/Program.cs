using System.Data.SqlClient;
using CS_Giftcard_Email_Sender;


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

    // Check to see if given checkID has giftcards
    var recipients = new List<GiftCardRecipient>();
    SqlCommand command = new SqlCommand($"SELECT * FROM [dbo].[GiftCardRecipients] WHERE CheckID = {value}", connection);
    using (SqlDataReader reader = command.ExecuteReader())
    {
        

        while (reader.Read())
        {
            var recipient = new GiftCardRecipient
            {
                GiftCardRecipientID = (int)reader["GiftCardRecipientID"],
                CheckID = (int)reader["CheckID"],
                CheckDetailID = (int)reader["CheckDetailID"],
                RecipientEmailAddress = reader["RecipientEmailAddress"].ToString(),
                Note = reader["Note"].ToString(),
                CreatedDate = (DateTime)reader["CreatedDate"],
                Sender = reader["Sender"].ToString()
            };

            recipients.Add(recipient);
        }

        foreach (var recipient in recipients)
        {
            Console.WriteLine($"GiftCardRecipientID: {recipient.GiftCardRecipientID}");
            Console.WriteLine($"CheckID: {recipient.CheckID}");
            Console.WriteLine($"CheckDetailID: {recipient.CheckDetailID}");
            Console.WriteLine($"RecipientEmailAddress: {recipient.RecipientEmailAddress}");
            Console.WriteLine($"Note: {recipient.Note}");
            Console.WriteLine($"CreatedDate: {recipient.CreatedDate}");
            Console.WriteLine($"Sender: {recipient.Sender}");
            Console.WriteLine("--------------------------");

        }
    }

    foreach (var recipient in recipients)
    {
        var checkDetailsList = new List<GiftCardCheckDetail>();
        command = new SqlCommand($"SELECT * FROM [dbo].[CheckDetails] WHERE CheckID = {recipient.CheckID} AND G_Points IS NOT NULL", connection);
        using (SqlDataReader reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                var checkDetail = new GiftCardCheckDetail
                {
                    CheckDetailID = (int)reader["CheckDetailID"],
                    CheckID = (int)reader["CheckID"],
                    // ... Map other properties similarly
                    ProductName = reader["ProductName"].ToString(),
                    // ... Continue with other properties
                    G_Points = reader["G_Points"] == DBNull.Value ? (decimal?)null : (decimal)reader["G_Points"]
                };

                checkDetailsList.Add(checkDetail);
            }

            foreach (var checkDetail in checkDetailsList)
            {
                Console.WriteLine($"CheckDetailID: {checkDetail.CheckDetailID}");
                Console.WriteLine($"CheckID: {checkDetail.CheckID}");
                Console.WriteLine($"ProductName: {checkDetail.ProductName}");
                Console.WriteLine($"G_Points: {checkDetail.G_Points}");
                Console.WriteLine("--------------------------");
            }
        }

    }

    if (recipients.Count < 1)
    {
        Console.WriteLine($"No giftcards were found for the checkID: {value}");
    }


    connection.Close();
    Console.WriteLine("Connection closed.");
}
