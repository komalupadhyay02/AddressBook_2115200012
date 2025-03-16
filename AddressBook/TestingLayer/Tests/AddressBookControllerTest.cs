using NUnit.Framework;
using Moq;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLayer.Interface;
using ModelLayer.DTO;
using AddressBook.Controllers;
using ModelLayer.Model;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace TestingLayer.AddressBookControllerTest
{
    [TestFixture]
    public class AddressBookControllerTests
    {
        private Mock<IAddressBookBL> _mockAddressBookBL;
        private Mock<IRabbitMqProducer> _mockRabbitMq;
        private AddressBookController _controller;

        [SetUp]
        public void Setup()
        {
            _mockAddressBookBL = new Mock<IAddressBookBL>();
            _mockRabbitMq = new Mock<IRabbitMqProducer>();
            _controller = new AddressBookController(_mockAddressBookBL.Object, _mockRabbitMq.Object);
        }

        private void SetUserClaims(string role, int userId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Role, role),
                new Claim("UserId", userId.ToString())
            };

            var identity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        [Test]
        public async Task GetAll_WhenCalledByAdmin_ReturnsOk()
        {
            SetUserClaims("Admin", 1);
            _mockAddressBookBL.Setup(bl => bl.GetAllContacts()).ReturnsAsync(new List<AddressBookDTO>());

            var result = await _controller.GetAll();
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task GetById_ValidId_ReturnsOk()
        {
            SetUserClaims("User", 1);
            var contact = new AddressBookDTO { UserId = 1 };
            _mockAddressBookBL.Setup(bl => bl.GetContactById(1)).ReturnsAsync(contact);

            var result = await _controller.GetById(1);
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task CreateContact_ValidData_ReturnsOk()
        {
            SetUserClaims("User", 1);
            var contact = new AddressBookDTO { Name = "Test", Email = "test@example.com" };
            _mockAddressBookBL.Setup(bl => bl.AddContact(contact)).ReturnsAsync(true);

            var result = await _controller.CreateContact(contact);
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task Update_ValidId_ReturnsOk()
        {
            // Arrange: Set user as "User" with UserId 1
            SetUserClaims("User", 1);

            var existingContact = new AddressBookDTO { Name = "John Doe", UserId = 1 };
            var updatedContact = new AddressBookDTO{ Name = "Updated Name", UserId = 1 };

            // Mock repository to return the existing contact
            _mockAddressBookBL.Setup(bl => bl.GetContactById(1)).ReturnsAsync(existingContact);

            // Mock update to return true (successful update)
            _mockAddressBookBL.Setup(bl => bl.Update(1, updatedContact)).ReturnsAsync(true);

            // Act
            var result = await _controller.Update(1, updatedContact);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
        }


        [Test]
        public async Task Delete_ValidId_ReturnsOk()
        {
            SetUserClaims("User", 1);
            var contact = new AddressBookDTO { UserId = 1 };
            _mockAddressBookBL.Setup(bl => bl.GetContactById(1)).ReturnsAsync(contact);
            _mockAddressBookBL.Setup(bl => bl.DeleteContact(1)).ReturnsAsync(true);

            var result = await _controller.Delete(1);
            Assert.IsInstanceOf<OkObjectResult>(result);
        }
    }
}