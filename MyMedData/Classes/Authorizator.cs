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
using System.Runtime.CompilerServices;

namespace MyMedData.Classes
{
	public static class Authorizator
	{
		public static bool AuthorizeUser(User user, MainWindow mainWindow)
		{
			if (mainWindow.ActiveUser?.Name == user.Name)
			{
				RaiseUserAuthorizedEvent(user);
				return true;
			}

			string? pwrd;

			if (user.CheckPassword(""))
			{
				pwrd = "";
			}
			else
			{
				EnterPasswordWindow passwordWindow = new EnterPasswordWindow(user);
				passwordWindow.Owner = mainWindow;
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
			catch (System.IO.IOException ifileBusyEx)
			{
				MessageBox.Show("Файл базы данных занят другим приложением.", "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Непредвиденная ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			return false;
		}

		public static bool AuthorizeUser(int userId, MainWindow mainWindow)
		{
			try
			{
				User? user = AppConfigDatabase.UsersAndSettingsDatabase.GetCollection<User>(User.DbCollectionName).FindById(userId);
				if (user == null)
					return false;

				return AuthorizeUser(user, mainWindow);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
				return false;
			}
		}

		public static bool AuthorizeLastUser(MainWindow mainWindow)
		{
			int? id = GetLastUserId();
			bool lastUserAutorizationSuccess;

			if (id is int ID)
				lastUserAutorizationSuccess = AuthorizeUser(ID, mainWindow);
			else
				lastUserAutorizationSuccess = false;

			if (!lastUserAutorizationSuccess)
			{
				AppConfigDatabase.Settings.LastUser = null;
				AppConfigDatabase.Settings.SaveSettings();
			}
			
			return lastUserAutorizationSuccess;
		}

		public static int? GetLastUserId()
		{
			string? idStr = AppConfigDatabase.Settings.LastUser;
			if (Int32.TryParse(idStr, out int id))
			{
				return id;
			}
			return null;
		}

		public static event UserAuthorizedEventHandler UserAuthorized;

		static void RaiseUserAuthorizedEvent(User user)
		{
			AppConfigDatabase.Settings.LastUser = user.Id.ToString();
			AppConfigDatabase.Settings.SaveSettings();
			UserAuthorized?.Invoke(new UserAuthorizedEventArgs(user));
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
			bool validPassword = password.Length < 30;
			foreach (char c in password)
			{
				validPassword &= (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_' || char.IsDigit(c);
			}

			return validPassword;
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