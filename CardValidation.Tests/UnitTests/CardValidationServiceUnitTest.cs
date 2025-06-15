using NUnit.Framework;
using CardValidation.Core.Services;
using CardValidation.Core.Enums;
using System;

namespace CardValidation.Tests
{
    [TestFixture]
    public class CardValidationServiceTests
    {
        private CardValidationService _service;

        [SetUp]
        public void SetUp()
        {
            _service = new CardValidationService();
        }

        [Test]
        [TestCase("John Doe", true)]
        [TestCase("Anna-Marie", false)]
        [TestCase("", false)]
        public void ValidateOwner_Tests(string owner, bool expected)
        {
            var result = _service.ValidateOwner(owner);
            Assert.That(expected, Is.EqualTo(result));
        }

        [Test]
        [TestCase("12/30", true)]
        [TestCase("01/20", false)]
        [TestCase("13/25", false)]
        [TestCase("00/99", false)]
        public void ValidateIssueDate_Tests(string issueDate, bool expected)
        {
            var result = _service.ValidateIssueDate(issueDate);
            Assert.That(expected, Is.EqualTo(result));
        }

        [Test]
        [TestCase("123", true)]
        [TestCase("1234", true)]
        [TestCase("12", false)]
        [TestCase("abcd", false)]
        public void ValidateCvc_Tests(string cvc, bool expected)
        {
            var result = _service.ValidateCvc(cvc);
            Assert.That(expected, Is.EqualTo(result));
        }

        [Test]
        [TestCase("4111111111111111", true)]  // Visa
        [TestCase("5500000000000004", true)]  // MasterCard
        [TestCase("340000000000009", true)]   // AmEx
        [TestCase("1234567890123456", false)] // Invalid
        public void ValidateNumber_Tests(string cardNumber, bool expected)
        {
            var result = _service.ValidateNumber(cardNumber);
            Assert.That(expected, Is.EqualTo(result));
        }

        [Test]
        [TestCase("4111111111111111", PaymentSystemType.Visa)]
        [TestCase("5500000000000004", PaymentSystemType.MasterCard)]
        [TestCase("340000000000009", PaymentSystemType.AmericanExpress)]
        public void GetPaymentSystemType_ValidNumbers_Tests(string cardNumber, PaymentSystemType expectedType)
        {
            var result = _service.GetPaymentSystemType(cardNumber);
            Assert.That(expectedType, Is.EqualTo(result));
        }

        [Test]
        public void GetPaymentSystemType_InvalidNumber_Throws()
        {
            Assert.Throws<NotImplementedException>(() => _service.GetPaymentSystemType("1234567890123456"));
        }
    }
}
