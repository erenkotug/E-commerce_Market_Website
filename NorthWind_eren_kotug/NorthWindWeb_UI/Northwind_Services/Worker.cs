using System.Net.Mail;
using System.Net;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Collections.Generic;

namespace Northwind_Services
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private Timer _timer;
        private string _connectionString = "Server=DESKTOP-74JSQ3D\\SQLEXPRESS;Database=NorthWind;Trusted_Connection=True;TrustServerCertificate=true;MultipleActiveResultSets=true";
        private string _emailRecipient = "erenkotughelp@gmail.com";
        private string _smtpServer = "smtp.gmail.com";
        private string _smtpUser = "erenkotughelp@gmail.com";
        private string _smtpPassword = "fwlrfozzulwcqixk";
        private string _logFilePath = @"C:\Users\Eren\Desktop\NorthWind\StockMonitorService.log";

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("Stock Monitor Service is starting.");
                Log("Stock Monitor Service is starting.");

                _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(5));

                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while executing the service.");
                Log("An error occurred while executing the service: " + ex.Message);
                throw;
            }
        }

        private void DoWork(object state)
        {
            Log("DoWork started.");

            var allMessages = new List<string>();

            allMessages.AddRange(CheckStockLevels());
            allMessages.AddRange(CheckMemberUpdates());
            allMessages.AddRange(CheckProductUpdates());

            if (allMessages.Any())
            {
                SendEmail(allMessages, "Daily Report", "Here are the updates from today's checks:");
            }
            else
            {
                Log("No updates found to email.");
            }
        }

        private List<string> CheckStockLevels()
        {
            var products = new List<string>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    Log("Database connection opened for CheckStockLevels.");

                    string query = "SELECT ProductID, ProductName FROM Products WHERE UnitsInStock = 0";
                    SqlCommand command = new SqlCommand(query, connection);
                    SqlDataReader reader = command.ExecuteReader();
                    Log("Executed query to check stock levels.");

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var productId = reader["ProductID"].ToString();
                            var productName = reader["ProductName"].ToString();
                            products.Add($"ProductID: {productId}, ProductName: {productName} - Out of Stock");
                        }
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                Log("Error checking stock levels: " + ex.Message);
            }
            return products;
        }

        private List<string> CheckMemberUpdates()
        {
            var members = new List<string>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    Log("Database connection opened for CheckMemberUpdates.");

                    string todayDate = DateTime.Today.ToString("yyyy-MM-dd");
                    string memberQuery = $"SELECT MemberID, Name FROM Members WHERE CONVERT(date, lastmodifytime) = '{todayDate}'";
                    SqlCommand memberCommand = new SqlCommand(memberQuery, connection);
                    SqlDataReader reader = memberCommand.ExecuteReader();
                    Log("Executed query to check member updates.");

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var memberId = reader["MemberID"].ToString();
                            var memberName = reader["Name"].ToString();
                            members.Add($"MemberID: {memberId}, Name: {memberName} - Updated Today");
                        }
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                Log("Error checking member updates: " + ex.Message);
            }
            return members;
        }

        private List<string> CheckProductUpdates()
        {
            var updatedProducts = new List<string>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    Log("Database connection opened for CheckProductUpdates.");

                    string todayDate = DateTime.Today.ToString("yyyy-MM-dd");
                    string productUpdateQuery = $"SELECT ProductID, ProductName FROM Products WHERE CONVERT(date, lastmodifytime) = '{todayDate}'";
                    SqlCommand command = new SqlCommand(productUpdateQuery, connection);
                    SqlDataReader reader = command.ExecuteReader();
                    Log("Executed query to check product updates.");

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var productId = reader["ProductID"].ToString();
                            var productName = reader["ProductName"].ToString();
                            updatedProducts.Add($"ProductID: {productId}, ProductName: {productName} - Updated Today");
                        }
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                Log("Error checking product updates: " + ex.Message);
            }
            return updatedProducts;
        }

        private void SendEmail(List<string> items, string subject, string bodyPrefix)
        {
            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient smtpClient = new SmtpClient(_smtpServer);

                mail.From = new MailAddress(_smtpUser);
                mail.To.Add(_emailRecipient);
                mail.Subject = subject;

                var body = bodyPrefix + "\n\n";
                foreach (var item in items)
                {
                    body += item + "\n";
                }
                mail.Body = body;

                smtpClient.Port = 587;
                smtpClient.Credentials = new NetworkCredential(_smtpUser, _smtpPassword);
                smtpClient.EnableSsl = true;

                smtpClient.Send(mail);

                Log("Email sent successfully at {0} to {1}.", DateTime.Now, _emailRecipient);
            }
            catch (Exception ex)
            {
                Log("Error sending email: " + ex.Message);
            }
        }

        public override Task StopAsync(CancellationToken stoppingToken)
        {
            Log("Stock Monitor Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _timer?.Dispose();
            Log("Timer disposed.");
            base.Dispose();
        }

        private void Log(string message, params object[] args)
        {
            var logMessage = string.Format(message, args);
            using (StreamWriter writer = new StreamWriter(_logFilePath, true))
            {
                writer.WriteLine($"{DateTime.Now}: {logMessage}");
            }
        }
    }
}
