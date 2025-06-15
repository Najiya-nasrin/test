using CardValidation.Core.Services.Interfaces;
using CardValidation.Infrustructure;
using CardValidation.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace CardValidation.Tests.Filters
{
    [TestFixture]
    public class CreditCardValidationFilterTests
    {
        private Mock<ICardValidationService> _mockCardValidationService;
        private CreditCardValidationFilter _filter;
        private ActionExecutingContext _actionExecutingContext;
        private ActionExecutedContext _actionExecutedContext;

        [SetUp]
        public void SetUp()
        {
            _mockCardValidationService = new Mock<ICardValidationService>();
            _filter = new CreditCardValidationFilter(_mockCardValidationService.Object);

            // Setup ActionExecutingContext
            var httpContext = new DefaultHttpContext();
            var actionContext = new ActionContext(
                httpContext,
                new RouteData(),
                new ActionDescriptor(),
                new ModelStateDictionary()
            );

            _actionExecutingContext = new ActionExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                new Dictionary<string, object>(),
                Mock.Of<Controller>()
            );

            _actionExecutedContext = new ActionExecutedContext(
                actionContext,
                new List<IFilterMetadata>(),
                Mock.Of<Controller>()
            );
        }

        [Test]
        public void OnActionExecuting_ValidCreditCard_NoModelStateErrors()
        {
            // Arrange
            var creditCard = new CreditCard
            {
                Owner = "John Doe",
                Date = "12/25",
                Cvv = "123",
                Number = "4111111111111111"
            };

            _actionExecutingContext.ActionArguments["creditCard"] = creditCard;

            _mockCardValidationService.Setup(x => x.ValidateOwner("John Doe")).Returns(true);
            _mockCardValidationService.Setup(x => x.ValidateIssueDate("12/25")).Returns(true);
            _mockCardValidationService.Setup(x => x.ValidateCvc("123")).Returns(true);
            _mockCardValidationService.Setup(x => x.ValidateNumber("4111111111111111")).Returns(true);

            // Act
            _filter.OnActionExecuting(_actionExecutingContext);

            // Assert
            Assert.That(_actionExecutingContext.ModelState.IsValid, Is.True);
            Assert.That(_actionExecutingContext.ModelState.ErrorCount, Is.EqualTo(0));
        }

        [Test]
        public void OnActionExecuting_NullOwner_AddsRequiredError()
        {
            // Arrange
            var creditCard = new CreditCard
            {
                Owner = null,
                Date = "12/25",
                Cvv = "123",
                Number = "4111111111111111"
            };

            _actionExecutingContext.ActionArguments["creditCard"] = creditCard;

            _mockCardValidationService.Setup(x => x.ValidateIssueDate("12/25")).Returns(true);
            _mockCardValidationService.Setup(x => x.ValidateCvc("123")).Returns(true);
            _mockCardValidationService.Setup(x => x.ValidateNumber("4111111111111111")).Returns(true);

            // Act
            _filter.OnActionExecuting(_actionExecutingContext);

            // Assert
            Assert.That(_actionExecutingContext.ModelState.IsValid, Is.False);
            Assert.That(_actionExecutingContext.ModelState.ContainsKey("Owner"), Is.True);
            Assert.That(_actionExecutingContext.ModelState["Owner"].Errors[0].ErrorMessage, Is.EqualTo("Owner is required"));
        }

        [Test]
        public void OnActionExecuting_EmptyOwner_AddsRequiredError()
        {
            // Arrange
            var creditCard = new CreditCard
            {
                Owner = string.Empty,
                Date = "12/25",
                Cvv = "123",
                Number = "4111111111111111"
            };

            _actionExecutingContext.ActionArguments["creditCard"] = creditCard;

            _mockCardValidationService.Setup(x => x.ValidateIssueDate("12/25")).Returns(true);
            _mockCardValidationService.Setup(x => x.ValidateCvc("123")).Returns(true);
            _mockCardValidationService.Setup(x => x.ValidateNumber("4111111111111111")).Returns(true);

            // Act
            _filter.OnActionExecuting(_actionExecutingContext);

            // Assert
            Assert.That(_actionExecutingContext.ModelState.IsValid, Is.False);
            Assert.That(_actionExecutingContext.ModelState.ContainsKey("Owner"), Is.True);
            Assert.That(_actionExecutingContext.ModelState["Owner"].Errors[0].ErrorMessage, Is.EqualTo("Owner is required"));
        }

        [Test]
        public void OnActionExecuting_InvalidOwner_AddsWrongParameterError()
        {
            // Arrange
            var creditCard = new CreditCard
            {
                Owner = "Invalid Owner",
                Date = "12/25",
                Cvv = "123",
                Number = "4111111111111111"
            };

            _actionExecutingContext.ActionArguments["creditCard"] = creditCard;

            _mockCardValidationService.Setup(x => x.ValidateOwner("Invalid Owner")).Returns(false);
            _mockCardValidationService.Setup(x => x.ValidateIssueDate("12/25")).Returns(true);
            _mockCardValidationService.Setup(x => x.ValidateCvc("123")).Returns(true);
            _mockCardValidationService.Setup(x => x.ValidateNumber("4111111111111111")).Returns(true);

            // Act
            _filter.OnActionExecuting(_actionExecutingContext);

            // Assert
            Assert.That(_actionExecutingContext.ModelState.IsValid, Is.False);
            Assert.That(_actionExecutingContext.ModelState.ContainsKey("Owner"), Is.True);
            Assert.That(_actionExecutingContext.ModelState["Owner"].Errors[0].ErrorMessage, Is.EqualTo("Wrong owner"));
        }

        [Test]
        public void OnActionExecuting_InvalidDate_AddsWrongParameterError()
        {
            // Arrange
            var creditCard = new CreditCard
            {
                Owner = "John Doe",
                Date = "invalid-date",
                Cvv = "123",
                Number = "4111111111111111"
            };

            _actionExecutingContext.ActionArguments["creditCard"] = creditCard;

            _mockCardValidationService.Setup(x => x.ValidateOwner("John Doe")).Returns(true);
            _mockCardValidationService.Setup(x => x.ValidateIssueDate("invalid-date")).Returns(false);
            _mockCardValidationService.Setup(x => x.ValidateCvc("123")).Returns(true);
            _mockCardValidationService.Setup(x => x.ValidateNumber("4111111111111111")).Returns(true);

            // Act
            _filter.OnActionExecuting(_actionExecutingContext);

            // Assert
            Assert.That(_actionExecutingContext.ModelState.IsValid, Is.False);
            Assert.That(_actionExecutingContext.ModelState.ContainsKey("Date"), Is.True);
            Assert.That(_actionExecutingContext.ModelState["Date"].Errors[0].ErrorMessage, Is.EqualTo("Wrong date"));
        }

        [Test]
        public void OnActionExecuting_InvalidCvv_AddsWrongParameterError()
        {
            // Arrange
            var creditCard = new CreditCard
            {
                Owner = "John Doe",
                Date = "12/25",
                Cvv = "invalid-cvv",
                Number = "4111111111111111"
            };

            _actionExecutingContext.ActionArguments["creditCard"] = creditCard;

            _mockCardValidationService.Setup(x => x.ValidateOwner("John Doe")).Returns(true);
            _mockCardValidationService.Setup(x => x.ValidateIssueDate("12/25")).Returns(true);
            _mockCardValidationService.Setup(x => x.ValidateCvc("invalid-cvv")).Returns(false);
            _mockCardValidationService.Setup(x => x.ValidateNumber("4111111111111111")).Returns(true);

            // Act
            _filter.OnActionExecuting(_actionExecutingContext);

            // Assert
            Assert.That(_actionExecutingContext.ModelState.IsValid, Is.False);
            Assert.That(_actionExecutingContext.ModelState.ContainsKey("Cvv"), Is.True);
            Assert.That(_actionExecutingContext.ModelState["Cvv"].Errors[0].ErrorMessage, Is.EqualTo("Wrong cvv"));
        }

        [Test]
        public void OnActionExecuting_InvalidNumber_AddsWrongParameterError()
        {
            // Arrange
            var creditCard = new CreditCard
            {
                Owner = "John Doe",
                Date = "12/25",
                Cvv = "123",
                Number = "invalid-number"
            };

            _actionExecutingContext.ActionArguments["creditCard"] = creditCard;

            _mockCardValidationService.Setup(x => x.ValidateOwner("John Doe")).Returns(true);
            _mockCardValidationService.Setup(x => x.ValidateIssueDate("12/25")).Returns(true);
            _mockCardValidationService.Setup(x => x.ValidateCvc("123")).Returns(true);
            _mockCardValidationService.Setup(x => x.ValidateNumber("invalid-number")).Returns(false);

            // Act
            _filter.OnActionExecuting(_actionExecutingContext);

            // Assert
            Assert.That(_actionExecutingContext.ModelState.IsValid, Is.False);
            Assert.That(_actionExecutingContext.ModelState.ContainsKey("Number"), Is.True);
            Assert.That(_actionExecutingContext.ModelState["Number"].Errors[0].ErrorMessage, Is.EqualTo("Wrong number"));
        }

        [Test]
        public void OnActionExecuting_MultipleInvalidFields_AddsMultipleErrors()
        {
            // Arrange
            var creditCard = new CreditCard
            {
                Owner = null,
                Date = "invalid-date",
                Cvv = "",
                Number = "invalid-number"
            };

            _actionExecutingContext.ActionArguments["creditCard"] = creditCard;

            _mockCardValidationService.Setup(x => x.ValidateIssueDate("invalid-date")).Returns(false);
            _mockCardValidationService.Setup(x => x.ValidateNumber("invalid-number")).Returns(false);

            // Act
            _filter.OnActionExecuting(_actionExecutingContext);

            // Assert
            Assert.That(_actionExecutingContext.ModelState.IsValid, Is.False);
            Assert.That(_actionExecutingContext.ModelState.ErrorCount, Is.EqualTo(4));
            Assert.That(_actionExecutingContext.ModelState.ContainsKey("Owner"), Is.True);
            Assert.That(_actionExecutingContext.ModelState.ContainsKey("Date"), Is.True);
            Assert.That(_actionExecutingContext.ModelState.ContainsKey("Cvv"), Is.True);
            Assert.That(_actionExecutingContext.ModelState.ContainsKey("Number"), Is.True);
        }

        [Test]
        public void OnActionExecuting_NoCreditCardArgument_DoesNothing()
        {
            // Arrange
            _actionExecutingContext.ActionArguments["someOtherArgument"] = "value";

            // Act
            _filter.OnActionExecuting(_actionExecutingContext);

            // Assert
            Assert.That(_actionExecutingContext.ModelState.IsValid, Is.True);
            _mockCardValidationService.VerifyNoOtherCalls();
        }

        [Test]
        public void OnActionExecuting_NoActionArguments_DoesNothing()
        {
            // Arrange
            _actionExecutingContext.ActionArguments.Clear();

            // Act
            _filter.OnActionExecuting(_actionExecutingContext);

            // Assert
            Assert.That(_actionExecutingContext.ModelState.IsValid, Is.True);
            _mockCardValidationService.VerifyNoOtherCalls();
        }

        [Test]
        public void OnActionExecuting_CreditCardArgumentIsNotCreditCardType_ThrowsInvalidOperationException()
        {
            // Arrange
            _actionExecutingContext.ActionArguments["creditCard"] = "not a credit card object";

            // Act & Assert
            Assert.That(() => _filter.OnActionExecuting(_actionExecutingContext), 
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void OnActionExecuting_CreditCardArgumentIsNull_ThrowsInvalidOperationException()
        {
            // Arrange
            _actionExecutingContext.ActionArguments["creditCard"] = null;

            // Act & Assert
            Assert.That(() => _filter.OnActionExecuting(_actionExecutingContext), 
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void OnActionExecuted_DoesNothing()
        {
            // Act
            _filter.OnActionExecuted(_actionExecutedContext);

            // Assert - No exceptions should be thrown, method should complete successfully
            Assert.Pass("OnActionExecuted completed without errors");
        }

        [Test]
        [TestCase("Owner", "owner")]
        [TestCase("Date", "date")]
        [TestCase("Cvv", "cvv")]
        [TestCase("Number", "number")]
        public void OnActionExecuting_WrongParameterErrorMessage_UsesLowercaseParameterName(string parameterName, string expectedLowercase)
        {
            // Arrange
            var creditCard = new CreditCard
            {
                Owner = "Invalid Owner",
                Date = "Invalid Date", 
                Cvv = "Invalid CVV",
                Number = "Invalid Number"
            };

            _actionExecutingContext.ActionArguments["creditCard"] = creditCard;

            _mockCardValidationService.Setup(x => x.ValidateOwner(It.IsAny<string>())).Returns(false);
            _mockCardValidationService.Setup(x => x.ValidateIssueDate(It.IsAny<string>())).Returns(false);
            _mockCardValidationService.Setup(x => x.ValidateCvc(It.IsAny<string>())).Returns(false);
            _mockCardValidationService.Setup(x => x.ValidateNumber(It.IsAny<string>())).Returns(false);

            // Act
            _filter.OnActionExecuting(_actionExecutingContext);

            // Assert
            Assert.That(_actionExecutingContext.ModelState.ContainsKey(parameterName), Is.True);
            Assert.That(_actionExecutingContext.ModelState[parameterName].Errors[0].ErrorMessage, 
                Is.EqualTo($"Wrong {expectedLowercase}"));
        }
    }
}