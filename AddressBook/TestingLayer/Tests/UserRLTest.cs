using Moq;
using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Context;
using RepositoryLayer.Service;
using ModelLayer.DTO;
using ModelLayer.Model;
using System.Linq;
using Microsoft.Extensions.Configuration;
using RepositoryLayer.Interface;
using RepositoryLayer.Hashing;

namespace TestingLayer.UserRLTests
{
    [TestFixture]
    public class UserRLTests
    {
        private UserRL _userRL;
        private AddressBookContext _context;
        private Mock<IEmailService> _emailServiceMock;
        private Mock<IConfiguration> _configurationMock;
        private Password_Hash _passwordHash;

        [SetUp]
        public void Setup()
        {
            // Use an in-memory database for testing
            var options = new DbContextOptionsBuilder<AddressBookContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            _context = new AddressBookContext(options);
            _emailServiceMock = new Mock<IEmailService>();
            _configurationMock = new Mock<IConfiguration>();
            _configurationMock.Setup(c => c["Jwt:SecretKey"]).Returns("MySuperLongSuperSecureSecretKey123456!");
            _configurationMock.Setup(c => c["Jwt:Issuer"]).Returns("localhost");
            _configurationMock.Setup(c => c["Jwt:Audience"]).Returns("");
            _passwordHash = new Password_Hash();

            _userRL = new UserRL(_context, _passwordHash, _emailServiceMock.Object, _configurationMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }


        [Test]
        public void RegisterUser_NewUser_ReturnsUser()
        {
            // Arrange
            var userDto = new RegisterDTO
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                UserRole =Role.User,
                Password = "password123"
            };

            // Act
            var result = _userRL.RegisterUser(userDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(userDto.Email, result.Email);
        }

        [Test]
        public void RegisterUser_ExistingUser_ReturnsNull()
        {
            // Arrange
            var existingUser = new User
            {
                FirstName = "Jane",
                LastName = "Doe",
                Email = "jane.doe@example.com",
                UserRole = Role.Admin,
                PasswordHash = _passwordHash.PasswordHashing("password123")
            };
            _context.Users.Add(existingUser);
            _context.SaveChanges();

            var userDto = new RegisterDTO
            {
                FirstName = "Jane",
                LastName = "Doe",
                Email = "jane.doe@example.com",
                UserRole = Role.Admin,
                Password = "password123"
            };

            // Act
            var result = _userRL.RegisterUser(userDto);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void LoginUser_ValidCredentials_ReturnsUserResponseDTO()
        {
            // Arrange
            var user = new User
            {
                FirstName = "Mike",
                LastName = "Smith",
                Email = "mike.smith@example.com",
                UserRole = Role.User,
                PasswordHash = _passwordHash.PasswordHashing("password123")
            };
            _context.Users.Add(user);
            _context.SaveChanges();

            var loginDto = new LoginDTO
            {
                Email = "mike.smith@example.com",
                Password = "password123"
            };

            // Act
            var result = _userRL.LoginUser(loginDto);
            Console.Write(result);
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(loginDto.Email, result.Email);
        }

        [Test]
        public void ForgetPassword_InvalidEmail_ReturnsFalse()
        {
            // Act
            var result = _userRL.ForgetPassword("invalid@example.com");

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void ForgetPassword_ValidEmail_SendsEmail_ReturnsTrue()
        {
            // Arrange
            var user = new User
            {
                FirstName = "Alex",
                LastName = "Brown",
                Email = "alex.brown@example.com",
                UserRole = Role.User,
                PasswordHash = _passwordHash.PasswordHashing("password123")
            };
            _context.Users.Add(user);
            _context.SaveChanges();

            _emailServiceMock.Setup(e => e.SendEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

            // Act
            var result = _userRL.ForgetPassword("alex.brown@example.com");

            // Assert
            Assert.IsTrue(result);
            _emailServiceMock.Verify(e => e.SendEmail("alex.brown@example.com", "Reset Password", It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void ResetPassword_ValidToken_ChangesPassword()
        {
            // Arrange
            var user = new User
            {
                FirstName = "Emma",
                LastName = "Johnson",
                Email = "emma.johnson@example.com",
                UserRole = Role.User,
                PasswordHash = _passwordHash.PasswordHashing("oldpassword"),
                ResetToken = "validtoken",
                ResetTokenExpiry = DateTime.UtcNow.AddMinutes(15)
            };
            _context.Users.Add(user);
            _context.SaveChanges();

            // Act
            var result = _userRL.ResetPassword("validtoken", "newpassword");

            // Assert
            Assert.IsTrue(result);
            var updatedUser = _context.Users.FirstOrDefault(u => u.Email == "emma.johnson@example.com");
            Assert.IsNotNull(updatedUser);
            Assert.IsTrue(_passwordHash.VerifyPassword("newpassword", updatedUser.PasswordHash));
        }

        [Test]
        public void ResetPassword_InvalidToken_ReturnsFalse()
        {
            // Act
            var result = _userRL.ResetPassword("invalidtoken", "newpassword");

            // Assert
            Assert.IsFalse(result);
        }
    }
}
