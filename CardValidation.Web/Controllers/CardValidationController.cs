using CardValidation.Core.Services.Interfaces;
using CardValidation.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace CardValidation.Controllers;

[ApiController]
[Route("[controller]")]
public class CardValidationController : ControllerBase
{
    private readonly ICardValidationService cardValidatior;

    public CardValidationController(ICardValidationService cardValidationService)
    {
        this.cardValidatior = cardValidationService;
    }

    [HttpPost]
    [Route("card/credit/validate")]
    public IActionResult ValidateCreditCard(CreditCard creditCard)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = cardValidatior.GetPaymentSystemType(creditCard.Number ?? string.Empty);        

        return Ok(result);
    }
}

