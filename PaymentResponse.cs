using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace VT.DomainModel.MockInterview
{
    public class PaymentResponse
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public string Data { get; set; }
    }
}
