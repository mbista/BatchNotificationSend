# BatchNotificationSend
This is a console application that I created to send emails to our clients with invoicing informations. 
The process was created because some poor soul in our operations team was sending emails to multiple clients every morning with the batchID, date and counts of bills, by getting the information from a reporting service. This task would take anywhere from 15 to 45 minutes depending on the amount of sends.
The application uses a few stored procedures to grab the data and add them to some object. LINQ is used to then do a query of the list of clients that require electronic confirmation and the availability of batches to send.
The data is taken and pushed into the HTML file after proper replace has been made.

The replace can be tricky as there are conditions to consider. 
The email can be sent to each client individually, or it can be sent to corporation or it can be sent to a corporation with batch for each client. This will determined by boolean values in (send to customer, send to corp or check if both values are true) I will add a UML to clarify this at a later time.

The console application will have to be scheduled to send the email notification at certain time of the day or can be used as a Windows service.
