using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Moq;
using AutoMapper;
using StackExchange.Redis;
using Microsoft.Extensions.Configuration;
using BusinessLayer.Service;
using RepositoryLayer.Interface;
using BusinessLayer.Interface;
using ModelLayer.DTO;
using ModelLayer.Model;

namespace TestingLayer.AddressBookBLTest
{
    [TestFixture]
    public class AddressBookBLTest
    {
        private Mock<IAddressBookRL> _mockAddressBookRL;
        private Mock<IMapper> _mockMapper;
        private Mock<IConnectionMultiplexer> _mockRedis;
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<IDatabase> _mockDatabase;
        private IAddressBookBL _addressBookBL;

        [SetUp]
        public void Setup()
        {
            _mockAddressBookRL = new Mock<IAddressBookRL>();
            _mockMapper = new Mock<IMapper>();
            _mockRedis = new Mock<IConnectionMultiplexer>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockDatabase = new Mock<IDatabase>();

            // Mock IConfiguration["Redis:CacheDuration"]
            _mockConfiguration.Setup(c => c["Redis:CacheDuration"]).Returns("300");

            // Mock Redis Database
            _mockRedis.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(_mockDatabase.Object);

            // Initialize AddressBookBL with all mocked dependencies
            _addressBookBL = new AddressBookBL(
                _mockAddressBookRL.Object,
                _mockMapper.Object,
                _mockRedis.Object,
                _mockConfiguration.Object
            );
        }


        [Test]
        public async Task GetAllContacts_ReturnsListOfContacts_Success()
        {
            // Arrange
            var contacts = new List<AddressBookEntry>
            {
                new AddressBookEntry { Id = 1, Name = "John Doe", Email = "john@example.com" }
            };

            var contactsDTO = new List<AddressBookDTO>
            {
                new AddressBookDTO { Name = "John Doe", Email = "john@example.com" }
            };

            _mockAddressBookRL.Setup(repo => repo.GetAll()).Returns(contacts);
            _mockMapper.Setup(m => m.Map<List<AddressBookDTO>>(contacts)).Returns(contactsDTO);

            // Act
            var result = await _addressBookBL.GetAllContacts();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public async Task GetAllContacts_ReturnsEmptyList_WhenNoContacts()
        {
            // Arrange
            _mockAddressBookRL.Setup(repo => repo.GetAll()).Returns(new List<AddressBookEntry>());

            // Act
            var result = await _addressBookBL.GetAllContacts();

            // Assert
            Assert.IsNull(result);
            
        }


        [Test]
        public async Task GetContactById_ReturnsContact_WhenExists()
        {
            // Arrange
            int contactId = 1;
            var contact = new AddressBookEntry { Id = contactId, Name = "John Doe", Email = "john@example.com" };
            var contactDTO = new AddressBookDTO { Name = "John Doe", Email = "john@example.com" };

            _mockAddressBookRL.Setup(repo => repo.GetById(contactId)).Returns(contact);
            _mockMapper.Setup(m => m.Map<AddressBookDTO>(contact)).Returns(contactDTO);

            // Act
            var result = await _addressBookBL.GetContactById(contactId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("John Doe", result.Name);
        }

        [Test]
        public async Task GetContactById_ReturnsNull_WhenContactDoesNotExist()
        {
            // Arrange
            int contactId = 1;
            _mockAddressBookRL.Setup(repo => repo.GetById(contactId)).Returns((AddressBookEntry)null);

            // Act
            var result = await _addressBookBL.GetContactById(contactId);

            // Assert
            Assert.IsNull(result);
        }


        [Test]
        public async Task AddContact_AddsContactSuccessfully()
        {
            // Arrange
            var contactDTO = new AddressBookDTO { Name = "John Doe", Email = "john@example.com" };
            var contactEntry = new AddressBookEntry { Id = 1, Name = "John Doe", Email = "john@example.com" };

            _mockMapper.Setup(m => m.Map<AddressBookEntry>(contactDTO)).Returns(contactEntry);
            _mockAddressBookRL.Setup(repo => repo.AddEntry(contactEntry)).Returns(true);

            // Act
            var result = await _addressBookBL.AddContact(contactDTO);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public async Task AddContact_FailsToAddContact()
        {
            // Arrange
            var contactDTO = new AddressBookDTO { Name = "John Doe", Email = "john@example.com" };
            var contactEntry = new AddressBookEntry { Id = 1, Name = "John Doe", Email = "john@example.com" };

            _mockMapper.Setup(m => m.Map<AddressBookEntry>(contactDTO)).Returns(contactEntry);
            _mockAddressBookRL.Setup(repo => repo.AddEntry(contactEntry)).Returns(false);

            // Act
            var result = await _addressBookBL.AddContact(contactDTO);

            // Assert
            Assert.IsFalse(result);
        }


        [Test]
        public async Task Update_UpdatesContactSuccessfully()
        {
            // Arrange
            int contactId = 1;
            var contactDTO = new AddressBookDTO { Name = "Updated Name", Email = "updated@example.com" };
            var existingContact = new AddressBookEntry { Id = contactId, Name = "John Doe", Email = "john@example.com" };
            var updatedContact = new AddressBookEntry { Id = contactId, Name = "Updated Name", Email = "updated@example.com" };

            _mockAddressBookRL.Setup(repo => repo.GetById(contactId)).Returns(existingContact);
            _mockMapper.Setup(m => m.Map<AddressBookEntry>(contactDTO)).Returns(updatedContact);
            _mockAddressBookRL.Setup(repo => repo.UpdateEntry(contactId, updatedContact)).Returns(true);

            // Act
            var result = await _addressBookBL.Update(contactId, contactDTO);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public async Task Update_FailsToUpdateContact_WhenContactDoesNotExist()
        {
            // Arrange
            int contactId = 1;
            var contactDTO = new AddressBookDTO { Name = "Updated Name", Email = "updated@example.com" };

            _mockAddressBookRL.Setup(repo => repo.GetById(contactId)).Returns((AddressBookEntry)null);

            // Act
            var result = await _addressBookBL.Update(contactId, contactDTO);

            // Assert
            Assert.IsFalse(result);
        }


        [Test]
        public async Task DeleteContact_DeletesContactSuccessfully()
        {
            // Arrange
            int contactId = 1;
            _mockAddressBookRL.Setup(repo => repo.DeleteEntry(contactId)).Returns(true);

            // Act
            var result = await _addressBookBL.DeleteContact(contactId);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public async Task DeleteContact_FailsToDeleteContact_WhenContactDoesNotExist()
        {
            // Arrange
            int contactId = 1;
            _mockAddressBookRL.Setup(repo => repo.DeleteEntry(contactId)).Returns(false);

            // Act
            var result = await _addressBookBL.DeleteContact(contactId);

            // Assert
            Assert.IsFalse(result);
        }
    }
}
