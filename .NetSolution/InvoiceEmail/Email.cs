using System;

namespace InvoiceEmail
{
    class Email
    {
        public String Corp { get; set; }
        public String Customer { get; set; }
        public String FullName { get; set; }
        public String EmailAddress { get; set; }
        public String CC { get; set; }
        public bool SendToCustomer { get; set; }
        public bool SendToCorp { get; set; }
    }
}
