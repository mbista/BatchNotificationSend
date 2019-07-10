using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;

namespace InvoiceEmail
{
    class DB
    {
        String connString = ConfigurationManager.ConnectionStrings["cnEmailList"].ToString();
        public List<Email> GetEmail()
        {
            string query = "exec dbo.s_GetInvoiceEmails";// select all the rows from the email list. 
            List<Email> emailList = new List<Email>();

            using (SqlConnection connection = new SqlConnection(connString))
            {
                SqlCommand cmd = new SqlCommand(query, connection);
                connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    while (reader.Read())
                    {
                        // Create new email
                        Email e = new Email();
                        e.Corp = (String)reader[0];
                        e.Customer = reader[1].ToString();
                        e.FullName = reader[2].ToString();
                        e.EmailAddress = (String)reader[3];
                        e.CC = reader[4].ToString();
                        e.SendToCustomer = (bool)reader[5];
                        e.SendToCorp = (bool)reader[6];
                        // Add product to list
                        emailList.Add(e);
                    }
                }
                catch(Exception ex)
                {
                    String errorMessage = ex.ToString();
                    PostalService.ErrorEmail(errorMessage);
                }
                finally
                {
                    reader.Close();
                }
            }
            return emailList;
        }
        public List<InvoiceCustomer> GetInvoiceCustomer()
        {
            string query = "exec [dbo].[s_GetInvoicesToSend] 0";
            List<InvoiceCustomer> invoiceList = new List<InvoiceCustomer>();

            using (SqlConnection connection = new SqlConnection(connString))
            {

                SqlCommand cmd = new SqlCommand(query, connection);
                connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    while (reader.Read())
                    {
                        //create new invoice
                        InvoiceCustomer i = new InvoiceCustomer();
                        i.ClaimCount = (int)reader[0];
                        i.InvoiceBatchID = (String)reader[1];
                        i.Date = (String)reader[2];
                        i.CustomerNum = reader[3].ToString().Trim();
                        i.CusCorpID = reader[4].ToString().Trim();
                        invoiceList.Add(i);
                    }
                }
                catch (Exception ex)
                {
                    String errorMessage = ex.ToString();
                    PostalService.ErrorEmail(errorMessage);
                }
                finally
                {
                    reader.Close();
                }
            }
            return invoiceList;
        }
        public List<InvoiceCorp> GetInvoiceCorp()
        {
            string query = "exec [dbo].[s_GetInvoicesToSend] 1";
            List<InvoiceCorp> invoiceCorpList = new List<InvoiceCorp>();

            using (SqlConnection connection = new SqlConnection(connString))
            {

                SqlCommand cmd = new SqlCommand(query, connection);
                connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                try
                {
                    while (reader.Read())
                    {
                        //create new invoice
                        InvoiceCorp i = new InvoiceCorp();
                        i.ClaimCount = (int)reader[0];
                        i.InvoiceBatchID = (String)reader[1];
                        i.Date = (String)reader[2];
                        i.CusCorpID = reader[3].ToString().Trim();
                        invoiceCorpList.Add(i);
                    }
                }
                catch (Exception ex)
                {
                    String errorMessage = ex.ToString();
                    PostalService.ErrorEmail(errorMessage);
                }
                finally
                {
                    reader.Close();
                }
            }
            return invoiceCorpList;
        }
    }
}
