using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace VT.DomainModel.MockInterview
{
    public class PaymentRequest
    {
        public string MerchantId { get; set; }
        public string TransactionId { get; set; }
        public string Amount { get; set; }
        public string CallbackUrl { get; set; }
    }
}
