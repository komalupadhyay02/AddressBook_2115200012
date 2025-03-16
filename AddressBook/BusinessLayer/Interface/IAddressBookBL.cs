﻿using System;
using ModelLayer.DTO;

namespace BusinessLayer.Interface
{
    /// <summary>
    /// Interface of AddressBookBL
    /// </summary>
	public interface IAddressBookBL
	{
        Task<List<AddressBookDTO>> GetAllContacts();
        Task<AddressBookDTO?> GetContactById(int id);
        Task<bool> AddContact(AddressBookDTO contact);
        Task<bool> Update(int id, AddressBookDTO contact);
        Task<bool> DeleteContact(int id);
    }
}

