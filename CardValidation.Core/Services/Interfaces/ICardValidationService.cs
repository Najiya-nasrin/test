using CardValidation.Core.Enums;

namespace CardValidation.Core.Services.Interfaces
{
    public interface ICardValidationService
    {
        bool ValidateOwner(string name);
        bool ValidateIssueDate(string issueDate);
        bool ValidateCvc(string cvv);
        bool ValidateNumber(string number);
        PaymentSystemType GetPaymentSystemType(string number);
    }
}
