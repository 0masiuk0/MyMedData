using MyMedData.Controls;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
using Microsoft.Win32;

namespace MyMedData.Windows
{
	/// <summary>
	/// Логика взаимодействия для AddDocExamWindow.xaml
	/// </summary>
	public partial class AddExaminationWindow : Window
	{
		public AddExaminationWindow(Session session, DocOrLabExamination docOrLab)
		{
			InitializeComponent();
			Session = session;
			DocOrLab = docOrLab;
		}

		public readonly Session Session;

		public DocOrLabExamination DocOrLab { get; private set; }

		private void AddUserWindowInstance_Loaded(object sender, RoutedEventArgs e)
		{
			theRecordDisplay.DataContext = Session;

			if (DocOrLab == DocOrLabExamination.Doc)
				theRecordDisplay.Item = new DoctorExaminationRecord() { Date = DateOnly.FromDateTime(DateTime.Today) };
			else
				theRecordDisplay.Item = new LabExaminationRecord() { Date = DateOnly.FromDateTime(DateTime.Today) };

			theRecordDisplay.ChangesSavedToDB += (o, e) => Close();
		}

		private void AddUserWindowInstance_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Escape)
				Close();
		}

		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
	}
}
