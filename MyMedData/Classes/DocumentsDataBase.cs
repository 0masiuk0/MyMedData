using LiteDB;
using MyMedData.Classes;
using System.IO;
using System.Linq;
using System.Windows;

namespace MyMedData
{
	public static class DocumentsDataBase
	{
		public static bool CreateUserDocumnetDb(User user, string password, DbCreationOptions options = DbCreationOptions.Ask)
		{
			if (Directory.Exists(user.RecordsFolder))
			{
				//есть директория
				if (File.Exists(user.RecordsDbFullPath))
				{
					//есть файл
					if (options == DbCreationOptions.UseExistingIfFound)
					{
						if (!DocumentsDataBase.FastCheckRecordDbValidity(user.RecordsDbFullPath, password))
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
							File.Delete(user.RecordsDbFullPath);
							DocumentsDataBase.CreateFreshRecordDb(user.RecordsDbFullPath, password);
							return true;
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
					//нет файла
					DocumentsDataBase.CreateFreshRecordDb(user.RecordsDbFullPath, password);
					return true;
				}
			}
			else
			{
				//папки нет
				MessageBox.Show($"Папка {user.RecordsFolder} не найдена", "Ошибка!",
								MessageBoxButton.OK, MessageBoxImage.Error);
				return false;
			}
		}

		public static bool FastCheckRecordDbValidity(string filename, string password)
		{
			using (var db = new LiteDatabase(GetConnectionString(filename, password)))
			{
				return db.GetCollectionNames().All(name => AllowedCollectionNames.Contains(name));
			}
		}

		private static void CreateFreshRecordDb(string filename, string password, bool keepsPrivateDoctorsDb = false)
		{
			new LiteDatabase(GetConnectionString(filename, password));			
		}

		public static string GetConnectionString(string filename, string password)
		{
			return $"Filename={filename};Password=\"{password}\"";
		}

		public static string GetConnectionString(User user, string password)
		{
			return $"Filename={user.RecordsDbFullPath};Password=\"{password}\"";
		}

		static string[] AllowedCollectionNames = new string[]
		{
			DoctorExamination.DB_COLLECTION_NAME, LabExaminationRecord.DB_COLLECTION_NAME,
			Doctor.DB_COLLECTION_NAME, Clinic.DB_COLLECTION_NAME,DoctorSpecialty.DB_COLLECTION_NAME
		};
	}
}
