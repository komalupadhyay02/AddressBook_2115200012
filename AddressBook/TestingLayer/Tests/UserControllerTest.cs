using NUnit.Framework;
using Moq;
using Microsoft.AspNetCore.Mvc;
using BusinessLayer.Interface;
using ModelLayer.Model;
using ModelLayer.DTO;
using AddressBook.Controllers;

namespace TestingLayer.UserControllers
{
    [TestFixture]
    public class UserControllerTests
    {
        private Mock<IUserBL> _mockUserBL;
        private UserController _controller;

        [SetUp]
        public void Setup()
        {
            _mockUserBL = new Mock<IUserBL>();
            _controller = new UserController(_mockUserBL.Object);
        }

        [Test]
        public void Register_UserAlreadyExists_ReturnsBadRequest()
        {
            var registerDTO = new RegisterDTO { Email = "test@example.com" };
            _mockUserBL.Setup(bl => bl.RegisterUser(registerDTO)).Returns((User)null);

            var result = _controller.Register(registerDTO) as BadRequestObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
        }

        [Test]
        public void Register_NewUser_ReturnsOk()
        {
            var registerDTO = new RegisterDTO { Email = "newuser@example.com" };
            var user = new User { Email = "newuser@example.com" };
            _mockUserBL.Setup(bl => bl.RegisterUser(registerDTO)).Returns(user);

            var result = _controller.Register(registerDTO) as OkObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        public void Login_InvalidCredentials_ReturnsUnauthorized()
        {
            var loginDTO = new LoginDTO { Email = "wrong@example.com", Password = "wrongpass" };
            _mockUserBL.Setup(bl => bl.LoginUser(loginDTO)).Returns((UserResponseDTO)null);

            var result = _controller.Login(loginDTO) as UnauthorizedObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(401, result.StatusCode);
        }

        [Test]
        public void Login_ValidCredentials_ReturnsOk()
        {
            var loginDTO = new LoginDTO { Email = "valid@example.com", Password = "validpass" };
            var userResponse = new UserResponseDTO { Email = "valid@example.com" };
            _mockUserBL.Setup(bl => bl.LoginUser(loginDTO)).Returns(userResponse);

            var result = _controller.Login(loginDTO) as OkObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        public void ForgetPassword_EmailExists_ReturnsOk()
        {
            _mockUserBL.Setup(bl => bl.ForgetPassword("existing@example.com")).Returns(true);

            var result = _controller.ForgetPassword("existing@example.com") as OkObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        public void ForgetPassword_EmailDoesNotExist_ReturnsNotFound()
        {
            _mockUserBL.Setup(bl => bl.ForgetPassword("nonexistent@example.com")).Returns(false);

            var result = _controller.ForgetPassword("nonexistent@example.com") as NotFoundObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(404, result.StatusCode);
        }

        [Test]
        public void ResetPassword_ValidToken_ReturnsOk()
        {
            var resetDTO = new ResetPasswordDTO { ResetToken = "validToken", NewPassword = "newPass" };
            _mockUserBL.Setup(bl => bl.ResetPassword(resetDTO.ResetToken, resetDTO.NewPassword)).Returns(true);

            var result = _controller.ResetPassword(resetDTO) as OkObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        public void ResetPassword_InvalidToken_ReturnsNotFound()
        {
            var resetDTO = new ResetPasswordDTO { ResetToken = "invalidToken", NewPassword = "newPass" };
            _mockUserBL.Setup(bl => bl.ResetPassword(resetDTO.ResetToken, resetDTO.NewPassword)).Returns(false);

            var result = _controller.ResetPassword(resetDTO) as NotFoundObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(404, result.StatusCode);
        }
    }
}
