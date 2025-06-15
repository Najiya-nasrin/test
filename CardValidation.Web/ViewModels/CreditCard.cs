using System.ComponentModel.DataAnnotations;

namespace CardValidation.ViewModels
{
    public class CreditCard
    {
        public string? Owner { get; set; }
        public string? Number { get; set; }
        public string? Date { get; set; }
        public string? Cvv { get; set; }       
    }
}
