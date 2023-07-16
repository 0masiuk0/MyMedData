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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MyMedData.Classes;

namespace MyMedData
{
	public class User : INotifyPropertyChanged
	{
		[BsonId]
		public int Id { get; set; }

		public string Name
		{
			get => _name;
			set
			{
				_name = value;
				NotifyPropertyChanged(nameof(Name));
			}
		}

		public Color AccountColor
		{
			get => _accountColor;
			set
			{
				_accountColor = value;
				NotifyPropertyChanged(nameof(AccountColor));
				NotifyPropertyChanged(nameof(AccountColoredBrush));
			}
		}

		public string? PasswordHash { get; private set; }

		public string DatabaseFile { get; set; }

		private string _name;
		private Color _accountColor;

		private const int KeySize = 32;
		private const int IterationsCount = 16;
		private const int SaltSize = 16;		

		public User()
		{
			Name = ""; AccountColor = Brushes.White.Color;
			DatabaseFile = "";
		}

		public User(string name, SolidColorBrush accountColor, string? password, string recordsFile)
		{
			Name = name;
			AccountColor = accountColor.Color;
			SetPassword(password ?? "");
			DatabaseFile = recordsFile;
		}

		public User(string name, Color accountColor, string? password, string recordsFile, bool runsOwnDoctorsCollection)
			: this(name, new SolidColorBrush(accountColor), password, recordsFile)
		{ }

		public void SetPassword(string password)
		{
			using (var algorithm = new Rfc2898DeriveBytes(
			  password,
			  SaltSize,
			  IterationsCount,
			  HashAlgorithmName.SHA256))
			{
				var key = Convert.ToBase64String(algorithm.GetBytes(KeySize));
				var salt = Convert.ToBase64String(algorithm.Salt);

				PasswordHash = $"{IterationsCount}.{salt}.{key}";
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
				var keyToCheck = algorithm.GetBytes(KeySize);

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
					RecordsDataBase.ChangeDbEncryptionPassword(DatabaseFile, oldPassword, newPassword);
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

//-----------------------------INTERFACES AND OVERRIDES----------------------------------
		public event PropertyChangedEventHandler? PropertyChanged;
		private void NotifyPropertyChanged(String propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}


		public override string ToString()
		{
			return Name;
		}

		public static bool UpdateUser(User updatedUser)
		{
			var userDbFilename = ConfigurationManager.AppSettings["UserDbName"];
			using (var db = new LiteDatabase(userDbFilename))
			{
				var users = db.GetCollection<User>(DbCollectionName);
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
			return copy;
		}

		public static string DbCollectionName => "Users";

		//---------------------------------------------NON-SERIALIZED INSTANCE MEMBERS-------------------------------------
		[BsonIgnore]
		public string DatabaseShortFileName => Path.GetFileName(DatabaseFile);
		
		[BsonIgnore]
		public bool PasswordIsSet => PasswordHash != null; 

		[BsonIgnore]
		public bool IsValidUser => PasswordHash != null && Authorizator.IsValidUserName(this.Name) && File.Exists(DatabaseFile);		

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
