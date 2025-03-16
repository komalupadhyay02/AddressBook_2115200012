using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Context;
using RepositoryLayer.Service;
using ModelLayer.Model;
using System.Collections.Generic;
using System.Linq;
using BusinessLayer.Service;

namespace TestingLayer.AddressBookRLTest
{
    [TestFixture]
    public class AddressBookRLTests
    {
        private AddressBookContext _context;
        private AddressBookRL _addressBookRL;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AddressBookContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Fresh DB for each test
                .Options;

            _context = new AddressBookContext(options);

            // Initialize Repository Layer
            _addressBookRL = new AddressBookRL(_context);

            // Ensure the database is clean
            _context.AddressBook.RemoveRange(_context.AddressBook);
            _context.SaveChanges();

            // Add test data
            _context.AddressBook.AddRange(new List<AddressBookEntry>
    {
        new AddressBookEntry { Id = 1, Name = "John Doe", Email = "john@example.com", Phone_No = "1234567890" },
        new AddressBookEntry { Id = 2, Name = "Jane Doe", Email = "jane@example.com", Phone_No = "9876543210" }
    });

            _context.SaveChanges();
        }



        [Test]
        public void GetAll_ShouldReturnAllContacts()
        {
            var result = _addressBookRL.GetAll();
            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void GetById_ExistingId_ShouldReturnContact()
        {
            var result = _addressBookRL.GetById(1);
            Assert.IsNotNull(result);
            Assert.AreEqual("John Doe", result.Name);
        }

        [Test]
        public void GetById_NonExistingId_ShouldReturnNull()
        {
            var result = _addressBookRL.GetById(99);
            Assert.IsNull(result);
        }

        [Test]
        public void AddEntry_ShouldReturnTrue()
        {
            var newEntry = new AddressBookEntry { Id = 3, Name = "New Contact", Email = "new@example.com", Phone_No = "1112223333" };
            var result = _addressBookRL.AddEntry(newEntry);
            Assert.IsTrue(result);
            Assert.AreEqual(3, _context.AddressBook.Count());
        }

        [Test]
        public void UpdateEntry_ExistingId_ShouldReturnTrue()
        {
            var updatedEntry = new AddressBookEntry { Name = "Updated Name", Email = "updated@example.com", Phone_No = "5556667777" };
            var result = _addressBookRL.UpdateEntry(1, updatedEntry);
            Assert.IsTrue(result);

            var updatedContact = _context.AddressBook.Find(1);
            Assert.AreEqual("Updated Name", updatedContact.Name);
        }

        [Test]
        public void UpdateEntry_NonExistingId_ShouldReturnFalse()
        {
            var updatedEntry = new AddressBookEntry { Name = "Updated Name", Email = "updated@example.com", Phone_No = "5556667777" };
            var result = _addressBookRL.UpdateEntry(99, updatedEntry);
            Assert.IsFalse(result);
        }

        [Test]
        public void DeleteEntry_ExistingId_ShouldReturnTrue()
        {
            var result = _addressBookRL.DeleteEntry(1);
            Assert.IsTrue(result);
            Assert.AreEqual(1, _context.AddressBook.Count());
        }

        [Test]
        public void DeleteEntry_NonExistingId_ShouldReturnFalse()
        {
            var result = _addressBookRL.DeleteEntry(99);
            Assert.IsFalse(result);
        }
    }
}
