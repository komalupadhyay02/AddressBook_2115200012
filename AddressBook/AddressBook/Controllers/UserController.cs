using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BusinessLayer.Interface;
using ModelLayer.Model;
using ModelLayer.DTO;
using System;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AddressBook.Controllers
{
	[Route("api/auth")]
	[ApiController]
	public class UserController:ControllerBase
	{
		private readonly IUserBL _userBL;
		/// <summary>
		/// Constructor to Initialize the instance
		/// </summary>
		/// <param name="userBL">The Business layer service for user</param>
		public UserController(IUserBL userBL)
		{
			_userBL = userBL;
		}

		/// <summary>
		/// Registers a new user
		/// </summary>
		/// <param name="registerDTO">User Details</param>
		/// <returns> Success or Failure Response</returns>
		[HttpPost("register")]
		public IActionResult Register(RegisterDTO registerDTO)
		{
			var response = new ResponseBody<User>();
			var user = _userBL.RegisterUser(registerDTO);
			if (user == null)
			{
				response.Message = "User Already Exist";
				response.Data = user;

                return BadRequest(response);
			}
			response.Success = true;
			response.Message = "User Registered Successfully";
			response.Data = user;
            return Ok(response);
		}

		/// <summary>
		/// Logs in an existing user
		/// </summary>
		/// <param name="login">User credentials for login</param>
		/// <returns>Success or Failure Message</returns>
		[HttpPost("login")]
		public IActionResult Login(LoginDTO login)
		{
            
            var user = _userBL.LoginUser(login);
			if (user == null)
			{
                var response = new ResponseBody<LoginDTO>();
				response.Message = "Invalid Credentials";
				response.Data = login;
                return Unauthorized(response);
			}
			var response2 = new ResponseBody<UserResponseDTO>();
			response2.Success = true;
			response2.Message = "User Login Successfully";
			response2.Data = user;
			return Ok(response2);
		}

		/// <summary>
		/// Method to check the Admin Role
		/// </summary>
		/// <returns>Returns message(only for admin)</returns>
		[Authorize(Roles="Admin")]
		[HttpGet("all-user")]
		public IActionResult GetAllUSer()
		{
			return Ok("Only admin can access this.");
		}

		/// <summary>
		/// Sends a password reset token to users email
		/// </summary>
		/// <param name="email">the email address of the user</param>
		/// <returns>Returns success message if email exists</returns>
		[HttpPost("forget-password")]
		public IActionResult ForgetPassword([FromBody] string email)
		{
			var response = new ResponseBody<string>();
			bool success = _userBL.ForgetPassword(email);
			if (success)
			{
				response.Success = true;
				response.Message = "Reset Link Sent to your Email";
				

				return Ok(response);
			}
			response.Message = "Email not found.";
			return NotFound(response);
		}

		/// <summary>
		/// Resets the password for user
		/// </summary>
		/// <param name="resetPassword">the reset token and new password</param>
		/// <returns>success message if password reset successfully</returns>
		[HttpPost("reset-password")]
		public IActionResult ResetPassword([FromBody] ResetPasswordDTO resetPassword)
		{
			var response = new ResponseBody<string>();
			bool success = _userBL.ResetPassword(resetPassword.ResetToken, resetPassword.NewPassword);
			if (success)
			{
				response.Success = true;
				response.Message = "Password reset Successfully";

                return Ok(response);
			}
			response.Message = "Invalid or Expired token";

            return NotFound(response);
		}

        

    }
}

