using LiteDB;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace MyMedData
{
	internal static class UsersDataBase
	{
		public static bool CreateNewUsersDb(string filename, DbCreationOptions dbCreationOptions = DbCreationOptions.Ask)
		{
			if (File.Exists(filename))
			{
				//есть файл
				if (dbCreationOptions == DbCreationOptions.Ask)
				{
					var answer = MessageBox.Show("Найден существующий файл.\n Да - использовать его.\n Нет - файл будет очищен.", "Ошибка!",
									MessageBoxButton.YesNoCancel, MessageBoxImage.Error);
					if (answer == MessageBoxResult.Yes)
					{
						return CreateNewUsersDb(filename, DbCreationOptions.UseExistingIfFound);
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
				else if (dbCreationOptions == DbCreationOptions.UseExistingIfFound)
				{
					if (!UsersDataBase.FastCheckUserDvValidity(filename))
					{
						if (MessageBox.Show("Указанная база не соответствует формату. Отформатировать с очисткой?", "Ошибка!",
							MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
						{
							//очищаем файл						
							return CreateNewUsersDb(filename, DbCreationOptions.Override);								
						}
						else
						{
							//отмена
							return false;
						}
					}
					else
					{
						//хороший файл, берем
						return true;
					}
				}
				else
				{
					//очистка
					try
					{
						File.Delete(filename);
						return true;
					}
					catch
					{
						if (MessageBox.Show("Не удается удалить файл. Повторить?", "Ошибка!",
										MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
						{
							return CreateNewUsersDb(filename, DbCreationOptions.Override);
						}
						else
							return false;
					}
				}
			}
			else
			{
				//нет файла
				GetFreshUsersDb(filename);
				return true;
			}
		}

		private static void GetFreshUsersDb(string filename)
		{
			using var db = new LiteDatabase(filename);
		}

		public static bool FastCheckUserDvValidity(string filename)
		{
			try
			{
				using (var db = new LiteDatabase(filename))
				{
					var collections = db.GetCollectionNames();
					return collections.All(name => AllowedCollectionNames.Contains(name));
				}
			}
			catch(LiteException) { return false; }
		}

		public static bool UpdateUser(User user, string usersDBfilename)
		{
			try
			{
				using (var db = new LiteDatabase(usersDBfilename))
				{
					var usersCollection = db.GetCollection<User>(User.DB_COLLECTION_NAME);
					return usersCollection.Update(user);
				}
			}
			catch (LiteException) { return false; }
		}

		static string[] AllowedCollectionNames = new string[]
		{
			User.DB_COLLECTION_NAME, Doctor.DB_COLLECTION_NAME, Clinic.DB_COLLECTION_NAME,DoctorSpecialty.DB_COLLECTION_NAME
		};
	}
}
