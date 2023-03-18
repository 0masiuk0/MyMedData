using LiteDB;
using System.Security.Cryptography;
using System;
using System.Windows.Media;
using System.Linq;
using System.Windows.Media.Animation;
using System.IO;
using System.Windows;
using System.Configuration;
using System.Windows.Controls;

namespace MyMedData
{
	public class User
	{
		[BsonId]
		public int Id { get; set; }
		public string Name { get; set; }
		public Color AccountColor { get; set; }
		public string? PasswordHash { get; private set; }
		public string DatabaseFile { get; set; }
		public bool RunsOwnDoctorsCollection { get; set; }

		const int KEY_SIZE = 32;
		const int ITERATIONS_COUNT = 16;
		const int SALT_SIZE = 16;

		public User()
		{
			Name = ""; AccountColor = Brushes.White.Color;
			DatabaseFile = "";
		}

		public User(string name, SolidColorBrush accountColor, string? password, string recordsFile, bool runsOwnDoctorsCollection)
		{
			Name = name;
			AccountColor = accountColor.Color;
			SetPassword(password ?? "");
			DatabaseFile = recordsFile;
			RunsOwnDoctorsCollection = runsOwnDoctorsCollection;
		}

		public void SetPassword(string password)
		{
			using (var algorithm = new Rfc2898DeriveBytes(
			  password,
			  SALT_SIZE,
			  ITERATIONS_COUNT,
			  HashAlgorithmName.SHA256))
			{
				var key = Convert.ToBase64String(algorithm.GetBytes(KEY_SIZE));
				var salt = Convert.ToBase64String(algorithm.Salt);

				PasswordHash = $"{ITERATIONS_COUNT}.{salt}.{key}";
			}
		}

		public bool CheckPassword(string password)
		{
			var hash = PasswordHash ?? "";

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

		public bool ChangePasswordAndRebuildDb(string oldPassword, string newPassword)
		{
			if (MessageBox.Show("Смена пароля может занять значительное время по причине перешифровки базы данных.", "Внимание!",
					MessageBoxButton.OKCancel, MessageBoxImage.Exclamation) == MessageBoxResult.OK)
			{
				try
				{
					SetPassword(newPassword);
					RecordsDataBase.ChnageDBEncryptionPassword(DatabaseFile, oldPassword, newPassword);
					return true;
				}
				catch (Exception ex)
				{
					SetPassword(oldPassword);
					MessageBox.Show("Сбой при смене пароля в базе.\n " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);					
					return false;
				}

			}
			else
				return false;
		}


		public override string ToString()
		{
			return Name;
		}


//----------------------------STATIC MEMBERS---------------------------------------------
		public static bool IsValidUserName(string name)
		{			
			bool validName = name.Length > 1 && name.Length < 30;
			foreach (char c in name)
			{
				validName &= char.IsLetterOrDigit(c) || c == '_' || char.IsWhiteSpace(c);
			}

			return validName;
		}

		public static bool IsValidPassword(string password)
		{
			bool validName = password.Length > 3 && password.Length < 30;
			foreach (char c in password)
			{
				validName &= (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_' || char.IsDigit(c);
			}

			return validName;
		}		

		public static bool UpdateUser(User updatedUser)
		{
			var userDbFilename = ConfigurationManager.AppSettings["UserDbName"];
			using (var db = new LiteDatabase(userDbFilename))
			{
				var users = db.GetCollection<User>(DB_COLLECTION_NAME);
				return users.Update(updatedUser);				
			}
		}

		public static User Copy(User user)
		{
			var copy = new User();
			copy.Id = user.Id;
			copy.Name = user.Name;
			copy.AccountColor = user.AccountColor;
			copy.DatabaseFile = user.DatabaseFile;
			copy.PasswordHash = user.PasswordHash;
			copy.RunsOwnDoctorsCollection = user.RunsOwnDoctorsCollection;
			return copy;
		}

		public static string DB_COLLECTION_NAME => "Users";

		//---------------------------------------------NON-SERIALIZED INSTANCE MEMBERS-------------------------------------
		[BsonIgnore]
		public string DatabaseShortFileName => Path.GetFileName(DatabaseFile);
		
		[BsonIgnore]
		public bool PasswordIsSet => PasswordHash != null; 

		[BsonIgnore]
		public bool IsValidUser => PasswordHash != null && IsValidUserName(this.Name) && File.Exists(DatabaseFile);		

		[BsonIgnore]
		public SolidColorBrush AccountColoredBrush
		{
			get => new SolidColorBrush(AccountColor);
			set => AccountColor = value.Color;
		}
	}

	public enum DbCreationOptions
	{
		Override, UseExistingIfFound, Ask
	}
}
