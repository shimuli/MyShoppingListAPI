using AfricasTalkingCS;
using System;

namespace PersonalShoppingAPI.Utills
{
    public class SmsService
    {
        public static string VerifyAccount(string username, string apikey, string phoneNumber,string message)
        {
            string responseMessage = string.Empty;
            phoneNumber = "+254" + phoneNumber.Substring(1);

            var gateway = new AfricasTalkingGateway(username, apikey);

            try
            {
                var sms = gateway.SendMessage(phoneNumber, message);
                responseMessage = "Sent";
            }
            catch (AfricasTalkingGatewayException exception)
            {
                responseMessage = exception.Message;
                Console.WriteLine(exception);
            }

            return responseMessage;
        }
    }
}
