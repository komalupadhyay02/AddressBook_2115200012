using System;
using Microsoft.EntityFrameworkCore;
using ModelLayer.Model;
namespace RepositoryLayer.Context
{
	public class AddressBookContext:DbContext
	{
		public AddressBookContext(DbContextOptions<AddressBookContext> options) : base(options) { }
		public DbSet<AddressBookEntry> AddressBook { get; set; }
		public DbSet<User> Users { get; set; }
	}
}

