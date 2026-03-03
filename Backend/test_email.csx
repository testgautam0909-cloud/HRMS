using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

var message = new MimeMessage();
message.From.Add(MailboxAddress.Parse("testgautam0909@gmail.com"));
message.To.Add(MailboxAddress.Parse("malaviyagautam0942@gmail.com"));
message.Subject = "Test Email";
message.Body = new TextPart("plain") { Text = "Test" };

try {
    using var smtp = new SmtpClient();
    await smtp.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
    await smtp.AuthenticateAsync("testgautam0909@gmail.com", "vjvu wjsw eota gzlb");
    await smtp.SendAsync(message);
    await smtp.DisconnectAsync(true);
    Console.WriteLine("Sent Successfully!");
} catch (Exception ex) {
    Console.WriteLine($"Error: {ex.Message}");
}
