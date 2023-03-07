using System;
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
using Microsoft.VisualBasic.ApplicationServices;

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
						var needToCreateNewUsersDb = MessageBox.Show($"Не найден файл с базой пользователей. Создать пустой по адресу {userDbFileName}?",
							"Ошибка!",
							MessageBoxButton.YesNo, MessageBoxImage.Error);

						if (needToCreateNewUsersDb == MessageBoxResult.Yes)
						{
							//Создаем новую базу пользователей
							using (var usersDB = new LiteDatabase(userDbFileName))
							{ 
								
							}
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
					UserPlaque userPlaque = new UserPlaque(user);
					UsersListBox.Items.Add(userPlaque);
					userPlaque.MouseDoubleClick += AuthorizeUser;
				}
			}
		}

		private void AddUserButton_Click(object sender, RoutedEventArgs e)
		{
			AddUserWindow addUserWindow = new();
			addUserWindow.ShowDialog();			
			if (addUserWindow.NewUser != null)
			{
				var newUser = addUserWindow.NewUser;
				using (var usersDB = new LiteDatabase(newUser.RecordsDbFullPath))
				{
					var usersCollection = usersDB.GetCollection<User>();
					usersCollection.Insert(newUser);
				}

				UserPlaque userPlaque = new UserPlaque(newUser);
				UsersListBox.Items.Add(userPlaque);
				userPlaque.MouseDoubleClick += AuthorizeUser;
			}
		}

		private void AuthorizeUser(object sender, MouseButtonEventArgs e)
		{
			throw new NotImplementedException();
		}

		private void LoginButton_Click(object sender, RoutedEventArgs e)
		{

		}
	}
}
