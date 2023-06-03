using LiteDB;
using MyMedData.Windows;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Threading;

namespace MyMedData.Classes
{
	public static class Authorizator
	{
		public static LiteDatabase GetUsersDatabase()
		{
			try
			{
				var appSettings = SettingsManager.AppSettings;

				string? UsersDbFileName = appSettings["UserDbName"]?.Value;
				if (UsersDbFileName == null)
				{
					MessageBox.Show("Адрес базы данных пользователей не настроен!", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
					throw new UserDbAccessException();
				}
				else
				{
					//Нет активной сессии или она не заниает UsersDb
					if (File.Exists(UsersDbFileName) && UsersDataBase.FastCheckUserDvValidity(UsersDbFileName))
					{
						return new LiteDatabase(UsersDbFileName);
					}
					else
					{
						var needToCreateNewUsersDb = MessageBox.Show($"Не найден корректный файл с базой пользователей.",
							"Ошибка!",
							MessageBoxButton.OK, MessageBoxImage.Error);
						throw new UserDbAccessException();
					}
				}
			}
			catch (Exception ex) 
			{
				throw new UserDbAccessException("Не удалось прочитать базу пользвателей", ex);
			}

		}

		public static bool AuthorizeUser(User user, MainWindow mainWindow)
		{
			if(mainWindow.ActiveUser == user)
				return true;

			string? pwrd;

			if (user.CheckPassword(""))
			{
				pwrd = "";
			}
			else
			{
				EnterPasswordWindow passwordWindow = new EnterPasswordWindow(user);
				passwordWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

				passwordWindow.ShowDialog();
				if (passwordWindow.Password is string passwordToCheck
					&& user.CheckPassword(passwordToCheck))
				{
					RaiseUserAuthorizedEvent(user);
					pwrd = passwordToCheck;
				}
				else
					return false;
			}

			try
			{
				Session session = new(user, pwrd);

				mainWindow.LogIn(session);
				RaiseUserAuthorizedEvent(user);
				return true;
			}
			catch(System.IO.IOException ifileBusyEx)
			{
				MessageBox.Show("Файл занят", ifileBusyEx.Message, MessageBoxButton.OK, MessageBoxImage.Error);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Непредвиденная ошибка", ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
			}
			return false;
		}

		public static bool AuthorizeUser(int userId, MainWindow mainWindow)
		{
			try
			{
				using (var userDB = GetUsersDatabase())
			{
				User? user = userDB.GetCollection<User>(User.DbCollectionName).FindById(userId);
				if (user == null) 
					return false;
				
				return AuthorizeUser(user, mainWindow);
			}
		}
			catch (Exception)
			{
				MessageBox.Show("Ошибка чтения настроек!", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
				return false;
			}
		}

		public static bool AuthorizeLastUser(MainWindow mainWindow)
		{
			int? id = GetLastUserId();
			
			if (id is int ID)
				return AuthorizeUser(ID, mainWindow);
			else 
				return false;
		}

		public static int? GetLastUserId()
		{
			string idStr = SettingsManager.AppSettings["last_user"].Value;
			if (Int32.TryParse(idStr, out int id))
			{
				return id;
			}
			return null;
		}

		public static event UserAuthorizedEventHandler UserAuthorized;

		static void RaiseUserAuthorizedEvent(User user)
		{
			SettingsManager.UpsertSetting("last_user", user.Id.ToString());
			UserAuthorized?.Invoke(new UserAuthorizedEventArgs(user));
		}
	}

	public class UserDbAccessException : Exception
	{
		public UserDbAccessException()
		{
		}

		public UserDbAccessException(string message)
			: base(message)
		{
		}

		public UserDbAccessException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}

	public delegate void UserAuthorizedEventHandler(UserAuthorizedEventArgs userAuthorizedEventArgs);

	public class UserAuthorizedEventArgs
	{
		public User User { get; set; }

		public UserAuthorizedEventArgs(User user) { User = user; }
	}
}