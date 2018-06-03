using System;
using System.Collections.Generic;
using System.Linq;
using MailKit.Net.Smtp;
using MimeKit;
using System.Threading.Tasks;
using MimeKit.Text;
using System.IO;

namespace MentorAPI.Services
{
    public class EmailService
    {

        private void SendEmail(string html,string subject, List<string> recipients) {
            //From Address  
            string FromAddress = "wademartin909@gmail.com";

            string Subject = subject;

            //Smtp Server  
            string SmtpServer = "smtp.gmail.com";
            //Smtp Port Number  
            int SmtpPortNumber = 587;

            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress(FromAddress));
            foreach (var item in recipients) {
                mimeMessage.To.Add(new MailboxAddress(item));
            }
            
            mimeMessage.Subject = Subject;
            mimeMessage.Body = new TextPart(TextFormat.Html){
                Text = html

            };

            

            using (var client = new SmtpClient()) {

                client.Connect(SmtpServer, SmtpPortNumber, false);
                // Note: only needed if the SMTP server requires authentication  
                // Error 5.5.1 Authentication   
                client.Authenticate("wademartin909@gmail.com", "1234wade");
                client.Send(mimeMessage);
                Console.WriteLine("The mail has been sent successfully !!");
                client.Disconnect(true);

            }
        }

        public void PrepareEmail(string htmlChoice,string subject, List<string> recipients, List<dynamic> objs = null) {
            string emailText = "";
            switch (htmlChoice) {
                case "Startup Request":
                    break;
                case "New Login":
                    break;
                case "Subscription Update":
                    break;
                case "MentorApplication":
                    emailText = File.ReadAllText(@"EmailTemplates/newMentorApplicationTemplate.html"); 
                    emailText = emailText.Replace("{startup}", objs[1].CompanyName);
                    emailText = emailText.Replace("{firstname}", objs[0].FirstName);
                    emailText = emailText.Replace("{lastname}", objs[0].LastName);
                    SendEmail(emailText, subject, recipients);
                    break;
                case "Report":
                    emailText = File.ReadAllText(@"EmailTemplates/reportTemplate.html");
                    emailText = emailText.Replace("{reporter}",objs[0].Reporter);
                    emailText = emailText.Replace("{reported}", objs[0].Reported);
                    emailText= emailText.Replace("{reason}", objs[0].Description);
                    SendEmail(emailText, subject, recipients);
                    break;
            }
        }
    }
}
