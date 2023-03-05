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

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			try
			{
				var appSettings = ConfigurationManager.AppSettings;

				string? userDbFileName = appSettings["UserDbName"];
				if (userDbFileName == null)
				{
					MessageBox.Show("Адрес базы данных пользователей не настроен!", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
					this.Close();
				}
				else
				{
					if (File.Exists(userDbFileName))
					{
						//читаем
						ReadUsers((string)userDbFileName);
					}
					else
					{
						var needToCreateNewUsersDb = MessageBox.Show($"Не найден файл с базой пользователей. Создать пустой по адексу {userDbFileName}?",
							"Ошибка!",
							MessageBoxButton.YesNo, MessageBoxImage.Error);

						if (needToCreateNewUsersDb == MessageBoxResult.Yes)
						{
							//Создаем новую базу пользователей
						}
					}
				}
			}
			catch (ConfigurationErrorsException)
			{
				MessageBox.Show("Ошибка чтения настроек!", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
				this.Close();
			}
		}

		private void ReadUsers(string userDbFileName)
		{
			using (var usersDB = new LiteDatabase(userDbFileName))
			{
				var usersCollection = usersDB.GetCollection<User>();
				UsersListBox.Items.Clear();
				foreach (User user in usersCollection.FindAll())
				{
					UsersListBox.Items.Add(new UserPlaque(user));
				}
			}
		}
	}
}
