using System;
using AutoMapper;
using ModelLayer.DTO;
using ModelLayer.Model;
namespace BusinessLayer.Helper
{
	public class AutoMapperProfile:Profile
	{
		public AutoMapperProfile()
		{
			//helps to maps the AddressBookEntry to AddressBookDTO or viceversa.
			CreateMap<AddressBookEntry, AddressBookDTO>().ReverseMap();
		}
	}
}

