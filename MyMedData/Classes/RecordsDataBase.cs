﻿using LiteDB;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Security.Cryptography;

namespace MyMedData
{
	public static class RecordsDataBase
	{
		public static bool CreateUserDocumnetDb(User user, string password, DbCreationOptions options = DbCreationOptions.Ask)
		{
			if (File.Exists(user.DatabaseFile))
			{
				if (!user.CheckPassword(password))
					throw new InvalidDataException($"Попытка создать файл базы данных с ключом шифрования, отличным от пароля пользователя {user.Name}");

				//есть файл
				if (options == DbCreationOptions.UseExistingIfFound)
				{
					if (!RecordsDataBase.FastCheckRecordDbValidity(user.DatabaseFile, password))
					{
						if (MessageBox.Show("Указанная база не соответствует формату. Отформатировать с очисткой?", "Ошибка!",
							MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
						{
							return CreateUserDocumnetDb(user, password, DbCreationOptions.Override);
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
				else if (options == DbCreationOptions.Override)
				{
					try
					{
						File.Delete(user.DatabaseFile);
						return CreateFreshRecordDb(user.DatabaseFile, password);
					}
					catch
					{
						if (MessageBox.Show("Не удается удалить существующий файл. Повторить попытку?", "Ошибка!",
							MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
						{
							return CreateUserDocumnetDb(user, password, options);
						}
						else
						{
							return false;
						}
					}
				}
				else
				{
					var answer = MessageBox.Show("Найден существующий файл.\n Да - использовать его.\n Нет - файл будет очищен.", "Ошибка!",
							MessageBoxButton.YesNoCancel, MessageBoxImage.Error);
					if (answer == MessageBoxResult.Yes)
					{
						return CreateUserDocumnetDb(user, password, DbCreationOptions.UseExistingIfFound);
					}
					else if (answer == MessageBoxResult.No)
					{
						return CreateUserDocumnetDb(user, password, DbCreationOptions.Override);
					}
					else
					{
						return false;
					}
				}
			}
			else
			{
				return CreateFreshRecordDb(user.DatabaseFile, password);
			}
		}

		public static bool FastCheckRecordDbValidity(string filename, string password)
		{
			try
			{
				using (var db = new LiteDatabase(GetConnectionString(filename, password)))
				{
					return db.GetCollectionNames().All(name => AllowedCollectionNames.Contains(name));
				}
			}
			catch(LiteException ex) { return false; }
		}

		public static bool ChangeDBEncryptionPassword(string filename, string oldPassword, string newPassword)
		{
			using (var tempDbFile = new TemporaryFile(Environment.CurrentDirectory, "~temp " + Path.GetFileName(filename)))
			{
				File.Copy(filename, tempDbFile.FilePath, true);


				LiteDB.Engine.RebuildOptions rebuildOptions = new LiteDB.Engine.RebuildOptions();
				rebuildOptions.Password = newPassword;
				try
				{
					using (var db = new LiteDatabase(GetConnectionString(tempDbFile.FilePath, oldPassword)))
					{
						db.Rebuild(rebuildOptions);						
					}			

					File.Copy(tempDbFile.FilePath, filename, true);
					return true;
				}
				catch
				{
					return false;
				}
			}
		}

		private static bool CreateFreshRecordDb(string filename, string password, bool keepsPrivateDoctorsDb = false)
		{
			try
			{
				using var newDB = new LiteDatabase(GetConnectionString(filename, password));
				return true;
			}
			catch(System.Exception ex)
			{
				MessageBox.Show("При создании базы данных возникла ошибка.\n" + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
				return false;
			}
		}

		public static string GetConnectionString(string filename, string password)
		{
			return $"Filename={filename};Password=\"{HashString16(password)}\"";
		}

		public static string GetConnectionString(User user, string password)
		{
			return GetConnectionString(user.DatabaseFile, password);
		}

		static string[] AllowedCollectionNames = new string[]
		{
			DoctorExaminationRecord.DB_COLLECTION_NAME, LabExaminationRecord.DB_COLLECTION_NAME,
			Doctor.DB_COLLECTION_NAME, Clinic.DB_COLLECTION_NAME,DoctorSpecialty.DB_COLLECTION_NAME
		};

		public static string HashString16(string text)
		{			
			byte[] bytes = Encoding.UTF8.GetBytes(text);
			char[] allowedPasswordHashChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();
			int allowedCharsCount = allowedPasswordHashChars.Length;

			using SHA256 sha256 = SHA256.Create();
			byte[] hash = sha256.ComputeHash(bytes);
			StringBuilder passwordStringHash16 = new StringBuilder();
			for (int i = 0; i < 16; i++)
			{
				passwordStringHash16.Append(allowedPasswordHashChars[hash[i] % allowedCharsCount]);	
			}

			return passwordStringHash16.ToString();
		}
	}
}