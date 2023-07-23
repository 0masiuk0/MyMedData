using LiteDB;
using System;
using System.IO;
using System.Windows;

namespace MyMedData
{
	internal static partial class AppConfigDatabase
	{
#if DEBUG
		static AppConfigDatabase()
		{
			string usersDBfilename = "Users Database debug.db";
			string userDbfullPath = Path.Combine(AppContext.BaseDirectory, usersDBfilename);
			string connectionString = $"Filename={@userDbfullPath};Connection=shared";
			usersDatabase = new LiteDatabase(connectionString);
		}
#else
		static string CONFIG_DB_FILENAME = "Users Database.db";
		static string APP_DATA_FOLDER_NAME = "MyMedData";
		static AppConfigDatabase()
		{
			string localAppDataFolder =
				  Environment.GetFolderPath(
				  Environment.SpecialFolder.LocalApplicationData);
			localAppDataFolder = Path.Combine(localAppDataFolder, APP_DATA_FOLDER_NAME);
			string userDBfilePath = Path.Combine(localAppDataFolder, CONFIG_DB_FILENAME);
			if (!Directory.Exists(localAppDataFolder))			
				Directory.CreateDirectory(localAppDataFolder);
			if (!File.Exists(userDBfilePath))
			{
				string defaultFilePath = Path.Combine(AppContext.BaseDirectory, CONFIG_DB_FILENAME);
				if (!File.Exists(@defaultFilePath))
					throw new Exception($"Не могу найти файл с настройками по умолчанию в папке программы: {defaultFilePath}");

				File.Copy(defaultFilePath, userDBfilePath, true);
			}

			string connectionString = $"Filename={@userDBfilePath};Connection=shared";
			usersDatabase = new LiteDatabase(connectionString);
		}
#endif

		static LiteDatabase usersDatabase;
		public static LiteDatabase UsersAndSettingsDatabase => usersDatabase;

		public static bool AddNewUser(User user)
		{
			try
			{
				var usersCollection = usersDatabase.GetCollection<User>(User.DbCollectionName);
				var id = usersCollection.Insert(user).AsInt32;
				return true;
			}
			catch { return false; }
		}

		public static bool UpdateUser(User user)
		{
			try
			{
				var usersCollection = usersDatabase.GetCollection<User>(User.DbCollectionName);
				return usersCollection.Update(user);
			}
			catch (LiteException) { return false; }
		}

		public static bool DeleteUser(User user, bool deleteFile)
		{
			bool dbDeletionSuccess;

			try
			{
				var usersCollection = usersDatabase.GetCollection<User>(User.DbCollectionName);
				dbDeletionSuccess = usersCollection.Delete(user.Id);
			}
			catch (LiteException) { dbDeletionSuccess = false; }

			if (dbDeletionSuccess)
			{
				if (deleteFile)
				{
				deletionAttempt:
					try
					{
						File.Delete(user.DatabaseFile);
					}
					catch
					{
						if (MessageBox.Show($"Не удалось удалить файл {user.DatabaseFile}. Попытаться еще раз?", "Неудача",
								MessageBoxButton.OKCancel, MessageBoxImage.Error) == MessageBoxResult.OK)
						{
							goto deletionAttempt;
						}
						else { return false; }
					}
				}
				else
				{
					MessageBox.Show($"База данных зашифрована. Базу данных можно будет в будущем использвать только если у нового пользователя"+
						" будет такой же пароль!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
				}
				return true;
			}
			else
			{
				MessageBox.Show($"Не удалось удалить пользователя {user}.", "Неудача", MessageBoxButton.OK, MessageBoxImage.Error);
				return false;
			}
		}		
	}
}
