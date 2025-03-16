using System;
using System.ComponentModel.DataAnnotations;
namespace ModelLayer.Model
{
	public class AddressBookEntry
	{
		[Key]
		public int Id { get; set; }
		[Required]
		public string Name { get; set; }
		[EmailAddress]
		[Required]
		public string Email { get; set; }
		[Phone]
		[Required]
		public string Phone_No { get; set; }
		public int UserId { get; set; }
	}
}

