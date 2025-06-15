using CardValidation.Core.Enums;
using CardValidation.Core.Services.Interfaces;
using System.Text.RegularExpressions;

namespace CardValidation.Core.Services
{
    public class CardValidationService : ICardValidationService
    {
        private static bool IsVisa(string cardNumber) => Regex.Match(cardNumber, @"^4[0-9]{12}(?:[0-9]{3})?$").Success;

        private static bool IsMasterCard(string cardNumber) =>
            Regex.Match(cardNumber, @"^(?:5[1-5][0-9]{2}|222[1-9]|22[3-9][0-9]|2[3-6][0-9]{2}|27[01][0-9]|2720)[0-9]{12}$").Success;

        private static bool IsAmericanExpress(string cardNumber) => Regex.Match(cardNumber, @"^3[47][0-9]{13}$").Success;


        public bool ValidateOwner(string owner) => Regex.Match(owner, @"^((?:[A-Za-z]+ ?){1,3})$").Success;     

        public bool ValidateIssueDate(string issueDate)
        {
            var pattern = @"^(0[1-9]|1[0-2])\/?([0-9]{4}|[0-9]{2})$";

            if (Regex.Match(issueDate, pattern).Success)
            {
                var dateValues = Regex.Split(issueDate, pattern).Where(t=>t != string.Empty).ToList();

                var month = int.Parse(dateValues[0]);
                var year = int.Parse(dateValues[1]);

                var issueDateTime = new DateTime(year / 1000 > 0 ? year : 2000 + year, month, 1);

                return DateTime.UtcNow < issueDateTime;
            }

            return false;
        }

        public bool ValidateCvc(string cvc) => Regex.Match(cvc, @"^[0-9]{3,4}$").Success;

        public bool ValidateNumber(string cardNumber)
        {
            if (IsVisa(cardNumber)) { return true; }

            if (IsMasterCard(cardNumber)) { return true; }

            if (IsAmericanExpress(cardNumber)) { return true; }

            return false;
        }

        public PaymentSystemType GetPaymentSystemType(string cardNumber)
        {
            if (IsVisa(cardNumber))
            { return PaymentSystemType.Visa; }

            if (IsMasterCard(cardNumber))
            { return PaymentSystemType.MasterCard; }

            if (IsAmericanExpress(cardNumber))
            { return PaymentSystemType.AmericanExpress; }

            throw new NotImplementedException();
        }
    }
}
