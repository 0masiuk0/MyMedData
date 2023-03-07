using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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

		private static void CreateFreshRecordDb(string filename)
		{
			using (var db = new LiteDatabase(filename))
			{
				var dcExamins = db.GetCollection<DoctorExamination>(DoctorExamination.DB_COLLECTION_NAME);
				dcExamins.DeleteAll();

				var lbExamins = db.GetCollection<LabExaminationRecord>(LabExaminationRecord.DB_COLLECTION_NAME);
			}
		}

		public static bool FastCheckRecordDbValidity(string filename)
		{
			using (var db = new LiteDatabase(filename))
			{
				var collections = db.GetCollectionNames();
				return collections.Contains(DoctorExamination.DB_COLLECTION_NAME)
					&& collections.Contains(LabExaminationRecord.DB_COLLECTION_NAME);
			}
		}
	}
}
