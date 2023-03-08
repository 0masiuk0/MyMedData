using LiteDB;
using System.Security.Cryptography;
using System;
using System.Windows.Media;
using System.Linq;
using System.Windows.Media.Animation;
using System.IO;
using Microsoft.VisualBasic.ApplicationServices;
using System.Windows;

namespace MyMedData
{
	public class User
	{
		public static User? ActiveUser { get; private set; }

		[BsonId]
		public int Id { get; set; }
		public string Name { get; set; }
		public Brush AccountColor { get; set; }
		public string? PasswordHash { get; private set; }
		public string RecordsFolder { get; set; }

		const int KEY_SIZE = 32;
		const int ITERATIONS_COUNT = 16;
		const int SALT_SIZE = 16;

		public User()
		{
			Name = ""; AccountColor = Brushes.White;
			RecordsFolder = "";
		}	

		public User(string name, Brush accountColor, string password, string recordsFolder)
		{
			Name = name;
			AccountColor = accountColor;
			SetPassword(password);
			RecordsFolder = recordsFolder;
		}

		public void SetPassword(string password)
		{
			PasswordHash = GetPasswordHash(password);
		}

		public override string ToString()
		{
			return Name;
		}


//----------------------------STATIC MEMBERS---------------------------------------------
		internal static void LogOff()
		{
			ActiveUser = null;
		}
		
		

		public static bool IsValidUserName(string name)
		{			
			bool validName = name.Length > 0 || name.Length < 30;
			foreach (char c in name)
			{
				validName |= char.IsLetterOrDigit(c) | c == '_';
			}

			return validName;
		}

		public static bool IsValidPassword(string password)
		{
			bool validName = password.Length > 3 || password.Length < 30;
			foreach (char c in password)
			{
				validName |= char.IsLetterOrDigit(c) | c == '_';
			}

			return validName;
		}

		private static string GetPasswordHash(string password)
		{
			using (var algorithm = new Rfc2898DeriveBytes(
			  password,
			  SALT_SIZE,
			  ITERATIONS_COUNT,
			  HashAlgorithmName.SHA256))
			{
				var key = Convert.ToBase64String(algorithm.GetBytes(KEY_SIZE));
				var salt = Convert.ToBase64String(algorithm.Salt);

				return $"{32}.{salt}.{key}";
			}
		}

		private static bool CheckPassword(string hash, string password)
		{
			var parts = hash.Split('.', 3);

			if (parts.Length != 3)
			{
				throw new FormatException("Unexpected hash format. " +
				  "Should be formatted as `{iterations}.{salt}.{hash}`");
			}

			var iterations = Convert.ToInt32(parts[0]);
			var salt = Convert.FromBase64String(parts[1]);
			var key = Convert.FromBase64String(parts[2]);	

			using (var algorithm = new Rfc2898DeriveBytes(
			  password,
			  salt,
			iterations,
			  HashAlgorithmName.SHA256))
			{
				var keyToCheck = algorithm.GetBytes(KEY_SIZE);

				var verified = keyToCheck.SequenceEqual(key);

				return verified;
			}
		}

		public static string DB_COLLECTION_NAME => "Users";

//---------------------------------------------NON-SERIALIZED INSTANCE MEMBERS-------------------------------------
		[BsonIgnore]
		public bool PasswordIsSet => PasswordHash != null; 

		[BsonIgnore]
		public bool IsValidUser => PasswordHash != null && IsValidUserName(this.Name) && File.Exists(RecordsDbFullPath);

		[BsonIgnore]
		public string RecordsDbFullPath => Path.Combine(RecordsFolder, $"{Name} MedData.db");
	}

	public enum RecordsDbCreationOptions
	{
		Override, UseExistingIfFound, Ask
	}
}
