using MyMedData.Classes;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MyMedData.Windows
{
	/// <summary>
	/// Логика взаимодействия для EnterPasswordWindow.xaml
	/// </summary>
	public partial class EnterPasswordWindow : Window
	{
		readonly User _user;
		public bool AuthenticationSuccess;
		public string? Password;
		public EnterPasswordWindow(User user)
		{
			InitializeComponent();
			DataContext = user;
			_user = user;
			Loaded += (o, e) => PasswrodBox.Focus();
		}		

		private void ReturnPassword(object sender, RoutedEventArgs e) => TryAuthorize();

		private void TryAuthorize()
		{
			if (PasswrodBox.Password is string pswrd)
			{
				AuthenticationSuccess = _user.CheckPassword(pswrd);
				Password = AuthenticationSuccess ? pswrd : null;
				AnimateResultAndCloseWindow(AuthenticationSuccess);				
			}
		}

		private void AnimateResultAndCloseWindow(bool authenticationSuccess)
		{
			//var initialColor = (this.Background as SolidColorBrush).Color;
			var targertColor = authenticationSuccess ? Colors.LightGreen : Colors.Red;
			Duration animationHalfDuration = new Duration(new TimeSpan(days: 0, hours: 0, minutes: 0, seconds: 0, milliseconds: 300));

			ColorAnimation colorAnimation = new ColorAnimation(targertColor, animationHalfDuration);			
			colorAnimation.EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut };

			//ColorAnimation clrAnimationBack = new ColorAnimation(initialColor, animationHalfDuration);

			Storyboard animationStoryboard = new();

			Storyboard.SetTarget(colorAnimation, this);
			Storyboard.SetTargetProperty(colorAnimation, new PropertyPath("Background.Color"));

			animationStoryboard.Children.Add(colorAnimation);

			animationStoryboard.Completed += (o, e) => Close();
			animationStoryboard.Begin();
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			AuthenticationSuccess = false;
			Close();
		}

		private void PasswrodBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter) 
				TryAuthorize();
		}
	}
}
