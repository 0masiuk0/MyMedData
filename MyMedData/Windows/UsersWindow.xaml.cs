﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Configuration;
using System.IO;
using LiteDB;
using System.Collections.ObjectModel;
using MyMedData.Controls;
using MyMedData.Windows;
using System.Windows.Markup;

namespace MyMedData.Windows
{
	/// <summary>
	/// Логика взаимодействия для UsersWindow.xaml
	/// </summary>
	public partial class UsersWindow : Window
	{
		public UsersWindow()
		{
			InitializeComponent();
		}

		public MainWindow MainWindow => (MainWindow)Owner;

		string? userDbFileName;
		ObservableCollection<User> _users = new();
		public ObservableCollection<User> Users
		{
			get => _users;
			private set { _users = value; }
		}


		private async void Window_Loaded(object sender, RoutedEventArgs e)
		{
			try
			{
				var appSettings = ConfigurationManager.AppSettings;

				userDbFileName = appSettings["UserDbName"];
				if (userDbFileName == null)
				{					
					MessageBox.Show("Адрес базы данных пользователей не настроен!", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
					ContentRendered += (o, e) => Close();
					return;
				}
				else
				{
					if (File.Exists(userDbFileName) && UsersDataBase.FastCheckUserDvValidity(userDbFileName))
					{
						//читаем
						ReadUsers((string)userDbFileName);
					}
					else
					{						
						var needToCreateNewUsersDb = MessageBox.Show($"Не найден корректный файл с базой пользователей.",
							"Ошибка!",
							MessageBoxButton.OK, MessageBoxImage.Error);
						ContentRendered += (o, e) => Close();
						return;
					}
				}
			}
			catch (ConfigurationErrorsException)
			{
				MessageBox.Show("Ошибка чтения настроек!", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
				ContentRendered += (o, e) => Close();
				return;
			}			
		}

		private void ReadUsers(string userDbFileName)
		{
			using (var usersDB = new LiteDatabase(userDbFileName))
			{
				var usersCollection = usersDB.GetCollection<User>(User.DB_COLLECTION_NAME);
				foreach (User user in usersCollection.FindAll())
				{
					Users.Add(user);
				}
			}

			if (UsersListBox.Items.Count > 0)
			{
				UsersListBox.SelectedIndex = 0;
			}
		}


		private void AddUserButton_Click(object sender, RoutedEventArgs e)
		{
			if (userDbFileName == null)
			{
				MessageBox.Show("Нет подключения к базе пользователей.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			AddUserWindow addUserWindow = new();
			addUserWindow.ShowDialog();			
			if (addUserWindow.NewUser != null)
			{
				var newUser = addUserWindow.NewUser;
				using (var usersDB = new LiteDatabase(userDbFileName))
				{
					var usersCollection = usersDB.GetCollection<User>(User.DB_COLLECTION_NAME);
					usersCollection.Insert(newUser);
					usersCollection.EnsureIndex(x => x.Name);
				}

				Users.Add(newUser);
			}
		}

		private void LoginButton_Click(object sender, RoutedEventArgs e)
		{
			if (UsersListBox.SelectedItem is User user) 
			{
				AuthorizeUser(user);
			}
		}

		private void UserPlaque_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			UserPlaque plaque = (UserPlaque)sender;
			if(plaque.DataContext is User user) AuthorizeUser(user);
		}

		private void AuthorizeUser(User user)
		{
			EnterPasswordWindow passwordWindow = new EnterPasswordWindow(user);
			passwordWindow.Owner = this;
			passwordWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

			passwordWindow.ShowDialog();
			if (passwordWindow.Password != null)
			{
				MainWindow.LogIn(new Session(user, passwordWindow.Password));
				Close();
			}
		}

		private void EditUserButton_Click(object sender, RoutedEventArgs e)
		{
			if (UsersListBox.SelectedItem is User user)
			{
				EnterPasswordWindow passwordWindow = new EnterPasswordWindow(user);
				passwordWindow.Owner = this;
				passwordWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
				passwordWindow.ShowDialog();

				if (passwordWindow.Password == null)
					return;

				EditUserWindow editUserWindow = new(user, passwordWindow.Password);
				editUserWindow.Owner = this;
				editUserWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
				editUserWindow.ShowDialog();
			}
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
        }

		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void UsersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			EditUserButton.IsEnabled = UsersListBox.SelectedItem is User;
		}
	}
}
