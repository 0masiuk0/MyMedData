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
		
		public static bool CreateUserDocumnetDb(User user, RecordsDbCreationOptions options)
		{
			if (Directory.Exists(user.RecordsFolder))
			{
				//есть директория
				if (File.Exists(user.RecordsDbFullPath))
				{
					if (options == RecordsDbCreationOptions.UseExistingIfFound)
					{
						if (!FastCheckRecordDbValidity(user.RecordsDbFullPath))
						{
							if (MessageBox.Show("Указанная база не соответствует формату. Отформатировать с очисткой?", "Ошибка!",
								MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
							{
								return CreateUserDocumnetDb(user, RecordsDbCreationOptions.Override);
							}
							else
							{
								return false;
							}
						}
						else
						{
							return true;
						}
					}
					else if (options == RecordsDbCreationOptions.Override)
					{
						try
						{
							File.Delete(user.RecordsDbFullPath);
							CreateFreshDb(user.RecordsDbFullPath);
							return true;
						}
						catch
						{
							if (MessageBox.Show("Не удается удалить существующий файл. Повторить попытку?", "Ошибка!",
								MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
							{
								return CreateUserDocumnetDb(user, options);
							}
							else
							{
								return false;
							}
						}
					}
					else 
					{
						var answer = MessageBox.Show("Найден сузествующий файл.\n Да - использовать его.\n Нет - файл будет очищен.", "Ошибка!",
								MessageBoxButton.YesNo, MessageBoxImage.Error);
						if (answer == MessageBoxResult.Yes)
						{
							return CreateUserDocumnetDb(user, RecordsDbCreationOptions.UseExistingIfFound);
						}
						else if (answer == MessageBoxResult.No)
						{
							return CreateUserDocumnetDb(user, RecordsDbCreationOptions.Override);
						}
						else
						{
							return false;
						}
					}
				}
				else
				{
					// Папка правильная, файла такого нет.
					CreateFreshDb(user.RecordsDbFullPath);
					return true;
				}
			}
			else
			{
				//папки такой нет
				MessageBox.Show($"Папка {user.RecordsFolder} не найдена", "Ошибка!",
								MessageBoxButton.OK, MessageBoxImage.Error);
				return false;
			}
		}

		private static void CreateFreshDb(string filename)
		{
			using (var db = new LiteDatabase(filename))
			{
				var dcExamins = db.GetCollection<DoctorExamination>(DoctorExamination.DB_COLLECTION_NAME);
				dcExamins.DeleteAll();

				var lbExamins = db.GetCollection<LabExaminationRecord>(LabExaminationRecord.DB_COLLECTION_NAME);
			}
		}

		private static bool FastCheckRecordDbValidity(string filename)
		{
			using (var db = new LiteDatabase(filename))
			{
				var collections = db.GetCollectionNames();
				return collections.Contains(DoctorExamination.DB_COLLECTION_NAME)
					&& collections.Contains(LabExaminationRecord.DB_COLLECTION_NAME);
			}
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

		public static bool IsValidPasswrod(string password)
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
