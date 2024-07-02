using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FieldMapperForDotNet.Tests
{
    [TestClass]
    public class FieldMapperTests
    {
        [TestMethod]
        [DeploymentItem("examples/ContactForm7/plain-text-body")]
        public void FieldMapperTests_ContactForm7_PlainTextBody()
        {
            // Arrange
            var content = File.ReadAllText("examples/ContactForm7/plain-text-body");

            var mappings = new List<string>();
            mappings.Add("From:");
            mappings.Add("Subject:");
            mappings.Add("Message Body:");
            mappings.Add(@"--"); // format the email to handle duplicate cases when necessary

            var configuration = new FieldMapperConfiguration();
            configuration.options.DeEntitizeContent = false;

            var fieldMapper = new FieldMapper(configuration);

            // Act
            var result = fieldMapper.Get(content, mappings);

            // Assert
            Assert.AreEqual(result["From:"], "Test Name <test@email.com>");
            Assert.AreEqual(result["Subject:"], "This is a test subject");
            Assert.AreEqual(result["Message Body:"], "Test message");
        }

        [TestMethod]
        [DeploymentItem("examples/ContactForm7/html-body")]
        public void FieldMapperTests_ContactForm7_HtmlBody()
        {
            // Arrange
            var content = File.ReadAllText("examples/ContactForm7/html-body");

            var mappings = new List<string>();
            mappings.Add("From:");
            mappings.Add("Subject:");
            mappings.Add("Message Body:");
            mappings.Add(@"--"); // format the email to handle duplicate cases when necessary

            var fieldMapper = new FieldMapper();

            // Act
            var result = fieldMapper.Get(content, mappings);

            // Assert
            Assert.AreEqual(result["From:"], "Test 2");
            Assert.AreEqual(result["Subject:"], "Test subject 2");
            Assert.AreEqual(result["Message Body:"], "This is a html message");
        }

        [TestMethod]
        [DeploymentItem("examples/WPFormsContactForm/plain-text-body")]
        public void FieldMapperTests_WPFormsContactForm_PlainTextBody()
        {
            // Arrange
            var content = File.ReadAllText("examples/WPFormsContactForm/plain-text-body");

            var mappings = new List<string>();
            mappings.Add("*Name*");
            mappings.Add("*Email*");
            mappings.Add("*Comment or Message*");
            mappings.Add("Sent from joe's Blog!");

            var fieldMapper = new FieldMapper();

            // Act
            var result = fieldMapper.Get(content, mappings);

            // Assert
            Assert.AreEqual(result["*Name*"], "Test Testing");
            Assert.AreEqual(result["*Email*"], "joseph.thomas.moceri@gmail.com");
            Assert.AreEqual(result["*Comment or Message*"], "Hi this is a test");
            Assert.AreEqual(result["Sent from joe's Blog!"], string.Empty);
        }

        [TestMethod]
        [DeploymentItem("examples/WPFormsContactForm/html-body")]
        public void FieldMapperTests_WPFormsContactForm_HtmlBody()
        {
            // Arrange
            var content = File.ReadAllText("examples/WPFormsContactForm/html-body");

            var mappings = new List<string>();
            mappings.Add("Name");
            mappings.Add("Email");
            mappings.Add("Comment or Message");
            mappings.Add("Sent from joe's Blog!");

            var fieldMapper = new FieldMapper();

            // Act
            var result = fieldMapper.Get(content, mappings);

            // Assert
            Assert.AreEqual(result["Name"], "Test Testing");
            Assert.AreEqual(result["Email"], "joseph.thomas.moceri@gmail.com");
            Assert.AreEqual(result["Comment or Message"], "Hi this is a test");
            Assert.AreEqual(result["Sent from joe's Blog!"], string.Empty);
        }

        [TestMethod]
        public void FieldMapperTests_NestedKeys_OnSameLine()
        {
            // Arrange
            var emailKey = "Email Name";
            var emailValue = "example@example.com";
            var phoneKey = "Phone";
            var phoneValue = "5555551234";
            var fullNameKey = "Name";
            var fullNameValue = "Joe Moceri";
            var zipKey = "Zip Name";
            var zipValue = "00000";

            var content = $"{emailKey} {emailValue} {phoneKey} {phoneValue} {fullNameKey} {fullNameValue} {zipKey} {zipValue}";
            var mappings = new List<string>();
            mappings.Add(emailKey);
            mappings.Add(fullNameKey);
            mappings.Add(phoneKey);
            mappings.Add(zipKey);

            // Act
            var parser = new FieldMapper();
            var result = parser.Get(content, mappings);

            // Assert
            Assert.AreEqual(emailValue, result[emailKey]);
            Assert.AreEqual(fullNameValue, result[fullNameKey]);
            Assert.AreEqual(phoneValue, result[phoneKey]);
            Assert.AreEqual(zipValue, result[zipKey]);
        }

        [TestMethod]
        public void FieldMapperTests_ShouldReturnEmptyResultIfNoKeysFound()
        {
            // Arrange
            string key = "First Name:";
            string value = "Joe";

            var content = $"{value}";
            var mappings = new List<string>();
            mappings.Add(key);

            // Act
            // content, mappings
            var parser = new FieldMapper();
            var result = parser.Get(content, mappings);

            // Assert
            Assert.AreEqual(result.Count, 0);
        }

        [TestMethod]
        public void FieldMapperTests_CaseInsensitiveShouldntFindValue()
        {
            // Arrange
            string key = "First Name:";
            string value = "Joe";

            var content = $"FIRst NaMe: {value}";
            var mappings = new List<string>();
            mappings.Add(key);

            // Act
            var parser = new FieldMapper();
            var result = parser.Get(content, mappings);

            // Assert
            Assert.AreEqual(result.Count, 0);
        }

            [TestMethod]
        public void FieldMapperTests_GetAndMap_FirstName()
        {
            // Arrange
            string key = "First Name:";
            string value = "Joe";

            var content = $"{key} {value}";
            var mappings = new List<string>();
            mappings.Add(key);

            // Act
            var parser = new FieldMapper();
            var result = parser.Get(content, mappings);

            // Assert
            Assert.AreEqual(value, result[key]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void FieldMapperTests_Init_NullContent_ShouldThrowArgumentException()
        {
            var mappings = Enumerable.Empty<string>().ToList();
            var parser = new FieldMapper();
            parser.Get(null, mappings);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void FieldMapperTests_Init_EmptyContent_ShouldThrowArgumentException()
        {
            var mappings = Enumerable.Empty<string>().ToList();
            var parser = new FieldMapper();
            parser.Get("", mappings);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void FieldMapperTests_Init_WhitespaceContent_ShouldThrowArgumentException()
        {
            var mappings = Enumerable.Empty<string>().ToList();
            var parser = new FieldMapper();
            parser.Get("     ", mappings);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void FieldMapperTests_Init_NullKeys_ShouldThrowArgumentException()
        {
            var content = "First Name: Joe";
            var parser = new FieldMapper();
            parser.Get(content, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void FieldMapperTests_Init_EmptyKeys_ShouldThrowArgumentException()
        {
            var content = "First Name: Joe";
            var mappings = Enumerable.Empty<string>().ToList();
            var parser = new FieldMapper();
            parser.Get(content, mappings);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void FieldMapperTests_Init_EmptyKeyInMappings_ShouldThrowArgumentException()
        {
            var mappings = Enumerable.Empty<string>().ToList();
            mappings.Add("");
            var content = "First Name: Joe";

            var parser = new FieldMapper();
            parser.Get(content, mappings);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void FieldMapperTests_Init_WhitespaceKeyInMappings_ShouldThrowArgumentException()
        {
            var mappings = Enumerable.Empty<string>().ToList();
            mappings.Add("      ");
            var content = "First Name: Joe";

            var parser = new FieldMapper();
            parser.Get(content, mappings);
        }

        [TestMethod]
        public void FieldMapperTests_LineBreakSeparatedMapping_NoSpace_CarriageReturn_FirstName()
        {
            // Arrange
            string key = "First{0}Name:";
            string value = "Joe";

            var content = $"{string.Format(key, "\r")} {value}";
            var mappings = new List<string>();
            mappings.Add(string.Format(key, " "));

            // Act
            var parser = new FieldMapper();
            var result = parser.Get(content, mappings);

            // Assert
            Assert.AreEqual(value, result[string.Format(key, " ")]);
        }

        [TestMethod]
        public void FieldMapperTests_LineBreakSeparatedMapping_NoSpace_LineFeed_FirstName()
        {
            // Arrange
            string key = "First{0}Name:";
            string value = "Joe";

            var content = $"{string.Format(key, "\n")} {value}";
            var mappings = new List<string>();
            mappings.Add(string.Format(key, " "));

            // Act
            var parser = new FieldMapper();
            var result = parser.Get(content, mappings);

            // Assert
            Assert.AreEqual(value, result[string.Format(key, " ")]);
        }

        [TestMethod]
        public void FieldMapperTests_LineBreakSeparatedMapping_NoSpace_CarriageReturnAndLineFeed_FirstName()
        {
            // Arrange
            string key = "First{0}Name:";
            string value = "Joe";

            var content = $"{string.Format(key, "\r\n")} {value}";
            var mappings = new List<string>();
            mappings.Add(string.Format(key, " "));

            // Act
            var parser = new FieldMapper();
            var result = parser.Get(content, mappings);

            // Assert
            Assert.AreEqual(value, result[string.Format(key, " ")]);
        }

        [TestMethod]
        public void FieldMapperTests_NestedKeys_Double_OnDifferentLines()
        {
            // Arrange
            var emailKey = "Email Name";
            string emailValue = "example@example.com";
            string phoneKey = "Phone";
            string phoneValue = "5551231234";

            var content = string.Format("{1} {0} {2} {0} {3} {0} {4}",
                Environment.NewLine,
                emailKey,
                emailValue,
                phoneKey,
                phoneValue
            );

            var mappings = new List<string>();

            mappings.Add(emailKey);
            mappings.Add(phoneKey);

            // Act
            var parser = new FieldMapper();
            var result = parser.Get(content, mappings);

            // Assert
            Assert.AreEqual(emailValue, result[emailKey]);
            Assert.AreEqual(phoneValue, result[phoneKey]);
        }


        [TestMethod]
        public void FieldMapperTests_NestedKeys_Triple_OnSameLines()
        {
            // Arrange
            string emailKey = "Email Name";
            string phoneKey = "Phone";
            string nameKey = "Name";
            string zipKey = "Zip Name";
            string cityKey = "City Zip Name";
            string addressKey = "Address City Zip Name";

            string emailValue = "example@example.com";
            string phoneValue = "5551231234";
            string nameValue = "Joe Moceri";
            string zipValue = "000000";
            string cityValue = "ExampleCity";
            string addressValue = "Example Address #123";

            var content = string.Format("{1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12}", Environment.NewLine, emailKey, emailValue, phoneKey, phoneValue, nameKey, nameValue, zipKey, zipValue, cityKey, cityValue, addressKey, addressValue);
            var mappings = new List<string>();
            mappings.Add(emailKey);
            mappings.Add(nameKey);
            mappings.Add(phoneKey);
            mappings.Add(zipKey);
            mappings.Add(cityKey);
            mappings.Add(addressKey);

            // Act
            var parser = new FieldMapper();
            var result = parser.Get(content, mappings);

            // Assert
            Assert.AreEqual(emailValue, result[emailKey]);
            Assert.AreEqual(phoneValue, result[phoneKey]);
            Assert.AreEqual(nameValue, result[nameKey]);
            Assert.AreEqual(zipValue, result[zipKey]);
            Assert.AreEqual(cityValue, result[cityKey]);
            Assert.AreEqual(addressValue, result[addressKey]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void FieldMapperTests_PutMapping_ShouldThrowArgumentExceptionIfTryingToAddDuplicateKey()
        {
            // Arrange
            string key = "First Name:";
            string value = "Joe";
            string secondKey = "First Name:";
            string secondValue = "Tom";

            var content = $"{key} {value} {secondKey} {secondValue}";
            var mappings = new List<string>();
            mappings.Add(key);
            mappings.Add(secondKey);

            // Act
            var parser = new FieldMapper();
            var result = parser.Get(content, mappings);

            // Assert
            Assert.AreEqual(secondValue, result[key]);
        }
    }
}
