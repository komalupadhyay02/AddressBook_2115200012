using System;
using System.Security.Cryptography;
using System.Text;
namespace RepositoryLayer.Hashing
{
	//class for Hashing the password and verifying the password
	public class Password_Hash
	{
		private const int SaltSize = 16;
		private const int HashSize = 20;
		private const int Iterations = 10000;
		/// <summary>
		/// method to hash the password
		/// </summary>
		/// <param name="userPass">password from user</param>
		/// <returns>hashed version of version</returns>
		public string PasswordHashing(string userPass)
		{
			byte[] salt = new byte[SaltSize];
			var rng = RandomNumberGenerator.Create();
			rng.GetBytes(salt);
			var pbkdf2 = new Rfc2898DeriveBytes(userPass, salt, Iterations);
			byte[] hash = pbkdf2.GetBytes(HashSize);
			byte[] hashByte = new byte[SaltSize + HashSize];
			Array.Copy(salt, 0, hashByte, 0, SaltSize);
			Array.Copy(hash, 0, hashByte, SaltSize, HashSize);
			return Convert.ToBase64String(hashByte);
		}
		/// <summary>
		/// method to verify hashpassword and normal password
		/// </summary>
		/// <param name="userPass">password from user</param>
		/// <param name="storedHashPass">password from database</param>
		/// <returns>true or false if matches or not</returns>
		public bool VerifyPassword(String userPass,string storedHashPass)
		{
			byte[] hashByte = Convert.FromBase64String(storedHashPass);
			byte[] salt = new byte[SaltSize];
			Array.Copy(hashByte, 0, salt, 0, SaltSize);
			var pbkdf2 = new Rfc2898DeriveBytes(userPass, salt, Iterations);
			byte[] hash = pbkdf2.GetBytes(HashSize);
			for(int i = 0; i < HashSize; i++)
			{
				if (hashByte[i + SaltSize] != hash[i])
				{
					return false;
				}

			}
			return true;
		}
	}
}

