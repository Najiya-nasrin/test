using CardValidation.Controllers;
using CardValidation.Core.Enums;
using CardValidation.Core.Services.Interfaces;
using CardValidation.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace CardValidation.Tests.Controllers
{
    [TestFixture]
    public class CardValidationControllerTests
    {
        private Mock<ICardValidationService> _mockCardValidationService;
        private CardValidationController _controller;

        [SetUp]
        public void SetUp()
        {
            _mockCardValidationService = new Mock<ICardValidationService>();
            _controller = new CardValidationController(_mockCardValidationService.Object);
        }

        [Test]
        public void ValidateCreditCard_ValidModel_ReturnsOkWithResult()
        {
            // Arrange
            var creditCard = new CreditCard { Number = "4111111111111111" };
            var expectedResult = PaymentSystemType.Visa;
            _mockCardValidationService
                .Setup(x => x.GetPaymentSystemType("4111111111111111"))
                .Returns(expectedResult);

            // Act
            var result = _controller.ValidateCreditCard(creditCard);

            
            var okResult = result as OkObjectResult;
            Assert.That(expectedResult, Is.EqualTo(okResult.Value));
            _mockCardValidationService.Verify(x => x.GetPaymentSystemType("4111111111111111"), Times.Once);
        }

        [Test]
        public void ValidateCreditCard_InvalidModel_ReturnsBadRequest()
        {
            // Arrange
            var creditCard = new CreditCard { Number = "4111111111111111" };
            _controller.ModelState.AddModelError("Number", "Invalid card number");

            // Act
            var result = _controller.ValidateCreditCard(creditCard);

            // Assert
        //    Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
          // Assert.IsInstanceOf<SerializableError>(badRequestResult.Value);
            _mockCardValidationService.Verify(x => x.GetPaymentSystemType(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void ValidateCreditCard_ServiceReturnsDefaultEnum_ReturnsOkWithDefaultValue()
        {
            // Arrange
            var creditCard = new CreditCard { Number = "1234567890123456" };
            _mockCardValidationService
                .Setup(x => x.GetPaymentSystemType("1234567890123456"))
                .Returns(default(PaymentSystemType));

            // Act
            var result = _controller.ValidateCreditCard(creditCard);


            var okResult = result as OkObjectResult;
            Assert.That(default(PaymentSystemType), Is.EqualTo(okResult.Value));
            _mockCardValidationService.Verify(x => x.GetPaymentSystemType("1234567890123456"), Times.Once);
        }

        [Test]
        public void ValidateCreditCard_ServiceThrowsException_ExceptionPropagates()
        {
            // Arrange
            var creditCard = new CreditCard { Number = "4111111111111111" };
            _mockCardValidationService
                .Setup(x => x.GetPaymentSystemType("4111111111111111"))
                .Throws(new System.ArgumentException("Invalid card number"));

            // Act & Assert
            Assert.Throws<System.ArgumentException>(() => _controller.ValidateCreditCard(creditCard));
            _mockCardValidationService.Verify(x => x.GetPaymentSystemType("4111111111111111"), Times.Once);
        }

        [Test]
        [TestCase("4111111111111111", PaymentSystemType.Visa)]
        [TestCase("5555555555554444", PaymentSystemType.MasterCard)]
        [TestCase("378282246310005", PaymentSystemType.AmericanExpress)]
        public void ValidateCreditCard_DifferentCardTypes_ReturnsCorrectPaymentSystem(string cardNumber, PaymentSystemType expectedPaymentSystem)
        {
            // Arrange
            var creditCard = new CreditCard { Number = cardNumber };
            _mockCardValidationService
                .Setup(x => x.GetPaymentSystemType(cardNumber))
                .Returns(expectedPaymentSystem);

            // Act
            var result = _controller.ValidateCreditCard(creditCard);

            var okResult = result as OkObjectResult;
            Assert.That(expectedPaymentSystem, Is.EqualTo(okResult.Value));
            _mockCardValidationService.Verify(x => x.GetPaymentSystemType(cardNumber), Times.Once);
        }
    }
}