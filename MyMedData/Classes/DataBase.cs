using LiteDB;
using Microsoft.VisualBasic.ApplicationServices;
using MyMedData.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace MyMedData
{
	internal static class DataBase
	{
		public static bool CreateUserDocumnetDb(User user, RecordsDbCreationOptions options)
		{
			if (Directory.Exists(user.RecordsFolder))
			{
				//есть директория
				if (File.Exists(user.RecordsDbFullPath))
				{
					if (options == RecordsDbCreationOptions.UseExistingIfFound)
					{
						if (!DataBase.FastCheckRecordDbValidity(user.RecordsDbFullPath))
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
							DataBase.CreateFreshRecordDb(user.RecordsDbFullPath);
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
						var answer = MessageBox.Show("Найден существующий файл.\n Да - использовать его.\n Нет - файл будет очищен.", "Ошибка!",
								MessageBoxButton.YesNoCancel, MessageBoxImage.Error);
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
					DataBase.CreateFreshRecordDb(user.RecordsDbFullPath);
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

		public static bool CreateNewUserstDb(string filename)
		{
			if (File.Exists(filename))
			{
				var answer = MessageBox.Show("Найден существующий файл.\n Да - использовать его.\n Нет - файл будет очищен.", "Ошибка!",
								MessageBoxButton.YesNoCancel, MessageBoxImage.Error);
				if (answer == MessageBoxResult.Yes)
				{
					if (!DataBase.FastCheckUserDvValidity(filename))
					{
						if (MessageBox.Show("Указанная база не соответствует формату. Отформатировать с очисткой?", "Ошибка!",
							MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
						{
							deletionAttempt:
							try
							{
								File.Delete(filename);								
							}
							catch
							{
								if (MessageBox.Show("Не удается удалить файл. Повторить?", "Ошибка!",
									MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
								{
									goto deletionAttempt;
								}
								else
									return false;
							}

							GetFreshUsersDb(filename);
							return true;
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
				else if (answer == MessageBoxResult.No)
				{
					GetFreshUsersDb(filename);
					return true;
				}
				else
				{
					return false;
				}
			}
			else
			{
				GetFreshUsersDb(filename);
				return true;
			}
		}

		private static void CreateFreshRecordDb(string filename)
		{
			using (var db = new LiteDatabase(filename))
			{
				var dcExamins = db.GetCollection<DoctorExamination>(DoctorExamination.DB_COLLECTION_NAME);
				dcExamins.DeleteAll();

				var lbExamins = db.GetCollection<LabExaminationRecord>(LabExaminationRecord.DB_COLLECTION_NAME);
			}
		}

		private static void GetFreshUsersDb(string filename)
		{
			using (var db = new LiteDatabase(filename))
			{
				var users = db.GetCollection<User>(User.DB_COLLECTION_NAME);
				users.DeleteAll();
				var docs = db.GetCollection<Doctor>(Doctor.DB_COLLECTION_NAME);
				docs.DeleteAll();
				var clinics = db.GetCollection<Clinic>(Clinic.DB_COLLECTION_NAME);
				clinics.DeleteAll();
				db.GetCollection<DoctorSpecialty>(DoctorSpecialty.DB_COLLECTION_NAME).DeleteAll();
			}
		}

		public static bool FastCheckRecordDbValidity(string filename)
		{
			using (var db = new LiteDatabase(filename))
			{
				return db.CollectionExists(DoctorExamination.DB_COLLECTION_NAME)
					&& db.CollectionExists(LabExaminationRecord.DB_COLLECTION_NAME);					
			}
		}

		public static bool FastCheckUserDvValidity(string filename)
		{
			using (var db = new LiteDatabase(filename))
			{
				var collections = db.GetCollectionNames();
				return db.CollectionExists(DoctorExamination.DB_COLLECTION_NAME)
					&& db.CollectionExists(LabExaminationRecord.DB_COLLECTION_NAME);
			}
		}
	}
}
