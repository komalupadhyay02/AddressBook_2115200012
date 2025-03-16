using System;
using FluentValidation;
using ModelLayer.DTO;
namespace BusinessLayer.Validators
{
	public class AddressBookValidators:AbstractValidator<AddressBookDTO>
	{
		//Adding validation on addressbookdto from user
		public AddressBookValidators()
		{
			RuleFor(a => a.Name).NotEmpty().WithMessage("Name is Required.");
			RuleFor(a => a.Email).NotEmpty().EmailAddress().WithMessage("Invalid Email.");
			RuleFor(a => a.Phone_No).NotEmpty().Matches(@"^\d{10}$").WithMessage("Phone no. must be 10 digits");
		}
	}
}

