using NUnit.Framework;
using Moq;
using BusinessLayer.Service;
using RepositoryLayer.Interface;
using ModelLayer.DTO;
using ModelLayer.Model;
using BusinessLayer.Interface;
using System;

namespace BusinessLayer.UserBLTest
{
    [TestFixture]
    public class UserBLTests
    {
        private Mock<IUserRL> _mockUserRL;
        private Mock<IRabbitMqProducer> _mockRabbitMqProducer;
        private IUserBL _userBL;

        [SetUp]
        public void Setup()
        {
            _mockUserRL = new Mock<IUserRL>();
            _mockRabbitMqProducer = new Mock<IRabbitMqProducer>();
            _userBL = new UserBL(_mockUserRL.Object, _mockRabbitMqProducer.Object);
        }

        [Test]
        public void RegisterUser_ValidUser_ReturnsUser()
        {
            // Arrange
            var registerDto = new RegisterDTO { FirstName = "John", LastName = "Doe", Email = "john.doe@example.com" };
            var expectedUser = new User { FirstName = "John", LastName = "Doe", Email = "john.doe@example.com" };
            _mockUserRL.Setup(rl => rl.RegisterUser(registerDto)).Returns(expectedUser);

            // Act
            var result = _userBL.RegisterUser(registerDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedUser.Email, result.Email);
            _mockRabbitMqProducer.Verify(mq => mq.PublishMessage(It.IsAny<UserEventDTO>()), Times.Once);
        }

        [Test]
        public void RegisterUser_NullUser_ReturnsNull()
        {
            // Arrange
            var registerDto = new RegisterDTO { FirstName = "John", LastName = "Doe", Email = "john.doe@example.com" };
            _mockUserRL.Setup(rl => rl.RegisterUser(registerDto)).Returns((User)null);

            // Act
            var result = _userBL.RegisterUser(registerDto);

            // Assert
            Assert.IsNull(result);
            _mockRabbitMqProducer.Verify(mq => mq.PublishMessage(It.IsAny<UserEventDTO>()), Times.Never);
        }

        [Test]
        public void LoginUser_ValidCredentials_ReturnsUserResponseDTO()
        {
            // Arrange
            var loginDto = new LoginDTO { Email = "john.doe@example.com", Password = "password123" };
            var expectedResponse = new UserResponseDTO { Email = "john.doe@example.com", Token = "mockToken" };
            _mockUserRL.Setup(rl => rl.LoginUser(loginDto)).Returns(expectedResponse);

            // Act
            var result = _userBL.LoginUser(loginDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedResponse.Email, result.Email);
        }

        [Test]
        public void ForgetPassword_ValidEmail_ReturnsTrue()
        {
            // Arrange
            string email = "john.doe@example.com";
            _mockUserRL.Setup(rl => rl.ForgetPassword(email)).Returns(true);

            // Act
            var result = _userBL.ForgetPassword(email);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void ForgetPassword_InvalidEmail_ReturnsFalse()
        {
            // Arrange
            string email = "invalid@example.com";
            _mockUserRL.Setup(rl => rl.ForgetPassword(email)).Returns(false);

            // Act
            var result = _userBL.ForgetPassword(email);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void ResetPassword_ValidToken_ReturnsTrue()
        {
            // Arrange
            string token = "valid-token";
            string newPassword = "newPassword123";
            _mockUserRL.Setup(rl => rl.ResetPassword(token, newPassword)).Returns(true);

            // Act
            var result = _userBL.ResetPassword(token, newPassword);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void ResetPassword_InvalidToken_ReturnsFalse()
        {
            // Arrange
            string token = "invalid-token";
            string newPassword = "newPassword123";
            _mockUserRL.Setup(rl => rl.ResetPassword(token, newPassword)).Returns(false);

            // Act
            var result = _userBL.ResetPassword(token, newPassword);

            // Assert
            Assert.IsFalse(result);
        }
    }
}
