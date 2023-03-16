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

		MainWindow mainWindow => (MainWindow)Owner;
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
					await Task.Delay(200);
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
				var usersCollection = usersDB.GetCollection<User>();
				foreach (User user in usersCollection.FindAll())
				{
					Users.Add(user);
				}
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

		private void UserPlaqueMouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			//
		}

		private void LoginButton_Click(object sender, RoutedEventArgs e)
		{

		}

		private void AuthorizeUser(User user)
		{
			EnterPasswordWindow passwordWindow = new EnterPasswordWindow(user);
			passwordWindow.ShowDialog();
			if (passwordWindow.Password != null)
			{
				mainWindow.LogIn(new Session(user, passwordWindow.Password));
			}
		}

		private void EditUserButton_Click(object sender, RoutedEventArgs e)
		{
			//спроси пароль
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
        }

		private void UsersWindowInstance_Closed(object sender, EventArgs e)
		{
			
		}
	}
}
