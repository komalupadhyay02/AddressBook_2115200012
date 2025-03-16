using Microsoft.AspNetCore.Mvc;
using RepositoryLayer.Context;
using ModelLayer.DTO;
using ModelLayer.Model;
using BusinessLayer.Interface;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AddressBook.Controllers;

[ApiController]
[Route("api/addressbook")]
public class AddressBookController : ControllerBase
{
    /// <summary>
    /// Object of BusinessLayer Interface
    /// </summary>
    private readonly IAddressBookBL _addressBookBL;
    private readonly IRabbitMqProducer _rabbitMq;


    /// <summary>
    /// call the constructor of controller
    /// </summary>
    /// <param name="context">DbContext from program.cs</param>
    public AddressBookController(IAddressBookBL addressBookBL,IRabbitMqProducer rabbitMq)
    {
        _addressBookBL = addressBookBL;
        _rabbitMq = rabbitMq;
    }


    /// <summary>
    /// Get the userId from Token
    /// </summary>
    /// <returns>return user id from token if present</returns>
    private int? GetLoggedInUserId()
    {
        var userIdClaim = User.FindFirst("UserId")?.Value;
        return userIdClaim != null ? int.Parse(userIdClaim) : null;
    }


    /// <summary>
    /// function to get the Role
    /// </summary>
    /// <returns>Role of user</returns>
    private string? GetUserRole()
    {
        return User.FindFirst(ClaimTypes.Role)?.Value;
    }


    /// <summary>
    /// Get all the addressbook contacts (Admin only)
    /// </summary>
    /// <returns>Response of Success or failure</returns>
    [HttpGet]
    [Authorize(Roles="Admin")]
    public async Task<IActionResult> GetAll()
    {
        var response = new ResponseBody<List<AddressBookDTO>>();
        var data = await _addressBookBL.GetAllContacts();
        //if we get the contacts
        if(data != null)
        {
            response.Success = true;
            response.Message = "All AddressBook Entries Read Successfully.";
            response.Data = data;
            return Ok(response);
        }
        //if we do not get the contacts
        response.Success = false;
        response.Message = "Cannot Read Entries";
        return NotFound(response);

    }


    /// <summary>
    /// Get the address book contact by particular id(Admin can access all ids but user can access only its ids)
    /// </summary>
    /// <param name="id">id from user</param>
    /// <returns>Success or failure response</returns>
    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var response = new ResponseBody<AddressBookDTO>();
        var role = GetUserRole();
        var userId = GetLoggedInUserId();
        var data = await _addressBookBL.GetContactById(id);
        //contact is not present in database
        if (data == null)
        {
            response.Success = false;
            response.Message = $"Contact with id  {id} not found.";
            return NotFound(response);
        }
        //if you are not authorize to get the contact
        if(role=="User" && data.UserId != userId)
        {
            response.Message = "Not Allowed";
            return Forbid();

        }
        //you are authorize to get the contact
        response.Success = true;
        response.Message = $"AddressBook Entry with {id} Read Successfully userId {userId}.";
        response.Data = data;
        return Ok(response);

    }


    /// <summary>
    /// Add the Contact in the Address Book
    /// </summary>
    /// <param name="contact">AddressBookEntry from user in special format</param>
    /// <returns>Success or failure response</returns>
    [HttpPost]
    [Authorize(Roles ="User")]
    public async  Task<IActionResult> CreateContact([FromBody] AddressBookDTO contact)
    {
        var response = new ResponseBody<AddressBookDTO>();
        var userId = GetLoggedInUserId();
        //if you do not have userId
        if (userId==null)
        {
            return Unauthorized();
        }
        //change the default value to userId of login user
        contact.UserId = userId.Value;
        var data = await _addressBookBL.AddContact(contact);
        //if unable to create the contact
        if (!data)
        {
            response.Success = false;
            response.Message = "Unable to add Contact.";
            return BadRequest(response);
        }
        //using rabbitmq to publish the message
        var userEvent = new UserEventDTO
        {
            FirstName = contact.Name,
            Email = contact.Email,
            LastName = "",
            EventType = "Contact Created"
        };
        _rabbitMq.PublishMessage(userEvent);

        //contact is successfully added
        response.Success = true;
        response.Message = "Contact Added Successfully.";
        response.Data = contact;
        return Ok(response);


    }


    /// <summary>
    /// Update the Address book entry at particular id
    /// </summary>
    /// <param name="id">id of contact to update</param>
    /// <param name="updatedcontact">updated info of contact</param>
    /// <returns>Succes or failure response</returns>
    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id,[FromBody] AddressBookDTO updatedcontact)
    {
        var response = new ResponseBody<AddressBookDTO>();
        var role = GetUserRole();
        var userId = GetLoggedInUserId();
        //check id exists
        var existingContact = await _addressBookBL.GetContactById(id);
        if (existingContact == null)
        {
            response.Message = "Contact not found";
            return NotFound(response);
        }
        //you are not authorize to access 
        if(role=="User" && existingContact.UserId != userId)
        {
            return Forbid();
        }
        var result = await _addressBookBL.Update(id, updatedcontact);
        //unable to update contact
        if (!result)
        {
            response.Message = "Unable to update contact.";
            return BadRequest(response);
        }
        //contact updated successfully
        response.Success = true;
        response.Message = "Contact updated Successfully.";
        response.Data = updatedcontact;
        return Ok(response);
       

    }


    /// <summary>
    /// Delete the particular id Contact info if present
    /// </summary>
    /// <param name="id">id of Contact entered by user</param>
    /// <returns>Success or failure response</returns>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var response = new ResponseBody<string>();
        var role = GetUserRole();
        var userId = GetLoggedInUserId();
        var existingContact = await _addressBookBL.GetContactById(id);
        //if id does not exist
        if (existingContact == null)
        {
            response.Message = "Contact Not Found";
            return NotFound(response);
        }
        //not authorize to access
        if(role=="User" && existingContact.UserId!= userId)
        {
            return Forbid();
        }
        var data = await _addressBookBL.DeleteContact(id);
        //deleted successfully
        if (data)
        {
            response.Success = true;
            response.Message = $"Contact with {id} deleted Successfully.";
            return Ok(response);
        }
        //unable to delete
        response.Success = false;
        response.Message=$"Unable to delete contact with id {id}." ;
        return BadRequest(response);
    
             
    }
}


