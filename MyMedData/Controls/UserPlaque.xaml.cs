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
		public UserPlaque()
		{
			InitializeComponent();
		}

		public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
			"Text", typeof(string),
			typeof(UserPlaque)
			);

		public string Text
		{
			get => (string)GetValue(TextProperty);
			set => SetValue(TextProperty, value);
		}

		/*public static readonly DependencyProperty BorderBrushProperty = DependencyProperty.Register(
			"BorderBrush", typeof(Brush), typeof(UserPlaque));

		public Brush BorderBrush
		{
			get => (Brush)GetValue(TextProperty);
			set => SetValue(TextProperty, value);
		}*/

	}
}
