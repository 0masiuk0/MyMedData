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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MyMedData.Controls
{
	/// <summary>
	/// Логика взаимодействия для UserPlaque.xaml
	/// </summary>
	public partial class UserPlaque : UserControl
	{
		User _user;
		public User User
		{
			get => _user;
			private set
			{
				_user = value;
				Foreground = _user.AccountColoredBrush;
				Text = _user.Name;
			}
		}

		public UserPlaque()
		{
			InitializeComponent();
			_user = new User();
		}

#pragma warning disable CS8618 
		public UserPlaque(User user)
		{
			InitializeComponent();
			User = user;
		}
#pragma warning restore CS8618

		public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
			"Text", typeof(string),
			typeof(UserPlaque)
			);

		public string Text
		{
			get => (string)GetValue(TextProperty);
			set => SetValue(TextProperty, value);
		}
	}
}
