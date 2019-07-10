using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Configuration;
using System.Linq;

namespace InvoiceEmail
{
    public static class PostalService
    {
        public static string InvoiceEmailFrom
        {
            get
            {
                return ConfigurationManager.AppSettings["InvoiceEmailFrom"] ?? "Noreply@domain.com";
            }
        }
        public static string SMTPServer
        {
            get
            {
                return ConfigurationManager.AppSettings["SMTPServer"] ?? "yourSmtpserver"
            }
        }
        public static string ErrorTo
        {
            get
            {
                return ConfigurationManager.AppSettings["ErrorTo"] ?? "GroupToBeNotified";
            }
        }
        public static string EmailSubject
        {
            get
            {
                return ConfigurationManager.AppSettings["EmailSubject"] ?? "Invoice batch Notification";
            }
        }
        public static void sendEmailInvoice(String to, String cc, String replaceThis)
        {
            MailMessage mail = new MailMessage();
            SmtpClient SmtpServer = new SmtpClient(SMTPServer);
            string emailTemplatePath = "InvoiceTemplate.html";
            string htmlBody = System.IO.File.ReadAllText(emailTemplatePath);

            mail.IsBodyHtml = true;
            mail.From = new MailAddress(InvoiceEmailFrom);
            //mail.To.Add("bistamonish@gmail.com");
            foreach (var addressTo in to.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))//split addresses by ; and add them to array.
            {
                mail.To.Add(addressTo);
            }
            foreach (var addressCC in cc.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
            {
                mail.CC.Add(addressCC);
            }
            mail.Subject = EmailSubject;
            mail.Attachments.Add(new Attachment("CompanyLogo.jpg"));
            if (!string.IsNullOrWhiteSpace(htmlBody))
            {
                htmlBody = htmlBody.Replace("<%%ReplaceThis%%>", replaceThis);
                htmlBody = htmlBody.Replace("<%%Year%%>", DateTime.Today.Year.ToString());
                mail.Body = htmlBody;
                SmtpServer.Send(mail);
            }
        }

        public static void ErrorEmail(String errorMessage)
        {
            MailMessage mail = new MailMessage();
            SmtpClient SmtpServer = new SmtpClient(SMTPServer);
            mail.To.Add(ErrorTo);
            mail.From = new MailAddress(InvoiceEmailFrom);
            mail.Subject = "Error in Invoice Batch Notification";
            mail.Body = errorMessage;
            SmtpServer.Send(mail);
        }

        static void Main(string[] args)
        {
            DB db = new DB();

            List<Email> emailList = db.GetEmail();
            List<InvoiceCustomer> invoiceCustomerList = db.GetInvoiceCustomer();
            List<InvoiceCorp> invoiceCorpList = db.GetInvoiceCorp();

            foreach (Email e in emailList)
            {
                if (e.SendToCorp == true && e.SendToCustomer == false)//send entire batch to corp
                {
					//This LINQ Query gets the batch for corp.
                    var joined = from i1 in emailList
                                    join i2 in invoiceCorpList
                                    on i1.Corp equals i2.CusCorpID
                                    where i2.CusCorpID==e.Corp
                                    select new { i1, i2 };
                    var CorpsInvoice = joined.ToList();
                    foreach (var j in joined)
                    {
                        try
                        {
                            String ReplaceWithPath = "ReplaceWith.html";
                            String replacePiece = System.IO.File.ReadAllText(ReplaceWithPath);
                            replacePiece = replacePiece.Replace("<%%CustomerName%%>", j.i1.FullName);
                            replacePiece = replacePiece.Replace("<%%Date%%>", j.i2.Date);
                            replacePiece = replacePiece.Replace("<%%BatchID%%>", j.i2.InvoiceBatchID);
                            replacePiece = replacePiece.Replace("<%%TotalClaim%%>", j.i2.ClaimCount.ToString());
                            sendEmailInvoice(e.EmailAddress, e.CC, replacePiece);
                        }
                        catch (Exception ex)
                        {
                            String errorMessage = ex.ToString();
                            ErrorEmail(errorMessage);
                        }
                    }
                }
                if (e.SendToCorp == false && e.SendToCustomer == true)//send batches to proper customer
                {
					//LINQ to get batch per customer
                    var joined = from i1 in emailList
                                    join i2 in invoiceCustomerList
                                    on i1.Corp equals i2.CusCorpID
                                    where i2.CustomerNum == e.Customer
                                    select new { i1, i2 };
                    var CorpsInvoice = joined.ToList();
                    foreach (var j in joined)
                    {
                        try
                        {
                            String ReplaceWithPath = "ReplaceWith.html";
                            String replacePiece = System.IO.File.ReadAllText(ReplaceWithPath);
                            replacePiece = replacePiece.Replace("<%%CustomerName%%>", j.i1.FullName);
                            replacePiece = replacePiece.Replace("<%%Date%%>", j.i2.Date);
                            replacePiece = replacePiece.Replace("<%%BatchID%%>", j.i2.InvoiceBatchID);
                            replacePiece = replacePiece.Replace("<%%TotalClaim%%>", j.i2.ClaimCount.ToString());
                            sendEmailInvoice(e.EmailAddress, e.CC, replacePiece);
                        }
                        catch (Exception ex)
                        {
                            String errorMessage = ex.ToString();
                            ErrorEmail(errorMessage);
                        }
                    }
                }
                if(e.SendToCorp == true && e.SendToCustomer == true)//send batches to corp with each customer info
                {   
					//LINQ Query that gets batch of Customer in the corp
                    var joined = from i1 in emailList
                                    join i2 in invoiceCustomerList
                                    on i1.Corp equals i2.CusCorpID
                                    where i2.CusCorpID == e.Corp
                                    select new { i1, i2 };
                    var CorpsInvoice = joined.ToList();

                    String ReplaceWithPath = "ReplaceWith.html";
                    String ConcatReplace = "";
                    if (CorpsInvoice.Count>0)//if there is no invoice do not send any email. 
                    {
                        for (int i = 0; i <= CorpsInvoice.Count() - 1; i++)
                        {
                            String replacePiece = System.IO.File.ReadAllText(ReplaceWithPath);
                            try
                            {
                                replacePiece = replacePiece.Replace("<%%CustomerName%%>", CorpsInvoice[i].i2.CustomerNum);
                                replacePiece = replacePiece.Replace("<%%Date%%>", CorpsInvoice[i].i2.Date);
                                replacePiece = replacePiece.Replace("<%%BatchID%%>", CorpsInvoice[i].i2.InvoiceBatchID);
                                replacePiece = replacePiece.Replace("<%%TotalClaim%%>", CorpsInvoice[i].i2.ClaimCount.ToString());
                                ConcatReplace = ConcatReplace + replacePiece;
                            }
                            catch (Exception ex)
                            {
                                String errorMessage = ex.ToString();
                                ErrorEmail(errorMessage);
                            }
                        }
                        sendEmailInvoice(e.EmailAddress, e.CC, ConcatReplace);
                    }
                }
            }
        }
    }
}
