using System;
using RepositoryLayer.Context;
using ModelLayer.Model;
using RepositoryLayer.Interface;
using Azure;

namespace RepositoryLayer.Service
{
	public class AddressBookRL : IAddressBookRL
	{
		private readonly AddressBookContext _context;

		//Constructor of class
		public AddressBookRL(AddressBookContext context)
		{
			_context = context;
		}


		/// <summary>
		/// method to get all the contacts from database
		/// </summary>
		/// <returns>List of contacts </returns>
		public List<AddressBookEntry> GetAll()
		{
			return _context.AddressBook.ToList<AddressBookEntry>();
		}


		/// <summary>
		/// method to get the particular contact
		/// </summary>
		/// <param name="id">input id from user</param>
		/// <returns>Contact on Particular id</returns>
		public AddressBookEntry? GetById(int id)
		{

			return _context.AddressBook.Find(id);
		}


		/// <summary>
		/// method to add the contact in addressbook
		/// </summary>
		/// <param name="entry">Contact info to be added</param>
		/// <returns>Returns True or false if added or not</returns>
		public bool AddEntry(AddressBookEntry entry)
		{
			_context.AddressBook.Add(entry);
			try
			{
				_context.SaveChanges();
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}


		/// <summary>
		/// method to update the contact info on id
		/// </summary>
		/// <param name="id">id to be updated</param>
		/// <param name="updatedentry">Updated contact info from user</param>
		/// <returns>True or false if contact is updated</returns>
		public bool UpdateEntry(int id,AddressBookEntry updatedentry)
		{
            var contact = _context.AddressBook.Find(id);
            if (contact != null)
            {
                contact.Name = updatedentry.Name;
                contact.Email = updatedentry.Email;
                contact.Phone_No = updatedentry.Phone_No;
                _context.SaveChanges();
                return true;
            }
           
            return false;
        }


		/// <summary>
		/// delete the contact on particular id
		/// </summary>
		/// <param name="id">id to be deleted</param>
		/// <returns>Returns true or false if deleted or not</returns>
		public bool DeleteEntry(int id)
		{
			var contact = _context.AddressBook.Find(id);
			if (contact != null)
			{
				_context.AddressBook.Remove(contact);
				_context.SaveChanges();
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}

