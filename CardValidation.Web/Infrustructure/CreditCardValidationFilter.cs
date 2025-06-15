using CardValidation.Core.Services.Interfaces;
using CardValidation.ViewModels;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;

namespace CardValidation.Infrustructure
{
    public class CreditCardValidationFilter : IActionFilter
    {
        private readonly ICardValidationService cardValidationService;

        private static void AddParameterIsRequiredError(ActionExecutingContext context, string parameterName)
            => context.ModelState.AddModelError(parameterName, $"{parameterName} is required");

        private static void AddWrongParameterError(ActionExecutingContext context, string parameterName)
             => context.ModelState.AddModelError(parameterName, $"Wrong {parameterName.ToLowerInvariant()}");

        public CreditCardValidationFilter(ICardValidationService cardValidationService)
        {
            this.cardValidationService = cardValidationService;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ActionArguments != null && context.ActionArguments.Count > 0)
            {
                CreditCard? card;
                if (context.ActionArguments.TryGetValue("creditCard", out object? value))
                {
                    card = value as CreditCard;

                    if (card != null)
                    {
                        ValidateParameter(context, nameof(card.Owner), card.Owner, cardValidationService.ValidateOwner);
                        ValidateParameter(context, nameof(card.Date), card.Date, cardValidationService.ValidateIssueDate);
                        ValidateParameter(context, nameof(card.Cvv), card.Cvv, cardValidationService.ValidateCvc);
                        ValidateParameter(context, nameof(card.Number), card.Number, cardValidationService.ValidateNumber);
                    }
                    else
                    {
                        throw new InvalidOperationException();
                    }
                }
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {

        }

        private static void ValidateParameter(ActionExecutingContext context, string name, string? value, Func<string, bool> isParameterValid)
        {
            if (value == null || value == string.Empty)
            {
                AddParameterIsRequiredError(context, name);
            }
            else
            {
                if (!isParameterValid(value)) { AddWrongParameterError(context, name); }
            }
        }
    }
}
