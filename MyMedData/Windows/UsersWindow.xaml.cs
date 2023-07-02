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
using MyMedData.Classes;

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
			Authorizator.UserAuthorized += e => Close();
		}

		public MainWindow MainWindow => (MainWindow)Owner;

		private ObservableCollection<User> _users = new();
		public ObservableCollection<User> Users
		{
			get => _users;
			private set => _users = value;
		}


		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			try
			{
				using (var userDb = Authorizator.GetUsersDatabase())
					ReadUsers(userDb);
				if (MainWindow.ActiveUser is User user)
				{
					User? dbUser = _users.FirstOrDefault(u => u.Id == user.Id, null);
					if (dbUser != null)
					{
						foreach(User u in UsersListBox.Items)
						{
							if (u.Id == user.Id) 
							{
								UsersListBox.SelectedItem = u;
								return;
							}
						}
					}
				}
				UsersListBox.SelectedIndex = 0;
			}
			catch (UserDbAccessException ex) 
			{
				MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
				this.Dispatcher.BeginInvoke(Close);
				return;
			}

			UsersListBox.Focus();
		}

		private void ReadUsers(LiteDatabase usersDb)
		{
			var usersCollection = usersDb.GetCollection<User>(User.DbCollectionName);
			foreach (User user in usersCollection.FindAll())
			{
				Users.Add(user);
			}
		}


		private void AddUserButton_Click(object sender, RoutedEventArgs e)
		{
			AddUserWindow addUserWindow = new();
			addUserWindow.ShowDialog();
			if (addUserWindow.DialogResult == true)
			{
				var newUser = addUserWindow.NewUser;
				using (var usersDb = new LiteDatabase(UsersDataBase.GetUsersDbFileNameFromConfig()))
				{
					var usersCollection = usersDb.GetCollection<User>(User.DbCollectionName);
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
				Authorizator.AuthorizeUser(user, MainWindow);
			}
		}

		private void UserPlaque_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			UserPlaque plaque = (UserPlaque)sender;
			if (plaque.DataContext is User user) Authorizator.AuthorizeUser(user, MainWindow);
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

				if (MainWindow.ActiveUser != null && MainWindow.ActiveUser.Id == user.Id)
				{
					MessageBox.Show("Пользователь будет разлогинен для редактирования.", "Редакция текцщего пользователя", MessageBoxButton.OK, MessageBoxImage.Exclamation);
					MainWindow.LogOff();
				}

				EditUserWindow editUserWindow = new(user, passwordWindow.Password);
				editUserWindow.Owner = this;
				editUserWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
				editUserWindow.ShowDialog();
			}
		}

		private void DeleteUserButton_Click(object sender, RoutedEventArgs e)
		{
			if (UsersListBox.SelectedItem is User user)
			{
				EnterPasswordWindow passwordWindow = new EnterPasswordWindow(user);
				passwordWindow.Owner = this;
				passwordWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
				passwordWindow.ShowDialog();

				if (passwordWindow.Password == null)
					return;

				if (MainWindow.ActiveUser != null && MainWindow.ActiveUser.Id == user.Id)
				{
					MainWindow.LogOff();
				}

				MessageBoxResult databaseDeletionAnswer = MessageBox.Show(
					"Удалить базу данных пользователя?", "Удаление данных",
					MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
				bool databaseDeletion;

				switch (databaseDeletionAnswer)
				{
					case MessageBoxResult.Yes:
						{
							switch (MessageBox.Show("Вы уверены, что требуется удалить все данные пользвателя?", "Удаления данных",
										MessageBoxButton.YesNoCancel))
							{
								case MessageBoxResult.Yes: databaseDeletion = true; break;
								case MessageBoxResult.No: databaseDeletion = false; break;
								default: return;
							}
							break;
						}
					case MessageBoxResult.No: databaseDeletion = false; break;
					default: return;
				}

				bool success = UsersDataBase.DeleteUser(user, databaseDeletion, UsersDataBase.GetUsersDbFileNameFromConfig());

				if (success)
				{
					Users.Remove(user);
					if (Users.Count > 0)
						UsersListBox.SelectedIndex = 0;
				}
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
			LoginButton.IsEnabled = DeleteUserButton.IsEnabled = EditUserButton.IsEnabled = UsersListBox.SelectedItem is User;
		}

		private void UsersListBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter
				&& UsersListBox.SelectedItem is User user)
			{
				Authorizator.AuthorizeUser(user, MainWindow);
			}
		}

		private void UsersWindowInstance_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Escape)
				Close();
		}
	}
}
