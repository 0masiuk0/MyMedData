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

namespace MyMedData.Windows
{
	/// <summary>
	/// Логика взаимодействия для AddDocExamWindow.xaml
	/// </summary>
	public partial class AddDocExamWindow : Window
	{
		public AddDocExamWindow(Session session)
		{
			InitializeComponent();
			this.session = session;

			_doctorsView = (CollectionViewSource)TryFindResource("Doctors");
			_doctorTypesView = (CollectionViewSource)TryFindResource("ExaminationTypes");
			_clinicsView = (CollectionViewSource)TryFindResource("Clinics");

			_doctorsView.Source = session.DoctorNameCache;
			_doctorTypesView.Source = session.DoctorTypesCache;
			_clinicsView.Source = session.ClinicNameCache;

			_onClinicCacheChanged = (o, e) => _clinicsView.View.Refresh();
			_onDoctorCacheChanged = (o, e) => _doctorsView.View.Refresh();
			_onDoctorTypeCacheChanged = (o, e) => _doctorTypesView.View.Refresh();
		}

		private readonly Session session;
		private readonly CollectionViewSource _doctorsView;
		private readonly CollectionViewSource _doctorTypesView;
		private readonly CollectionViewSource _clinicsView;

		private readonly NotifyCollectionChangedEventHandler _onDoctorCacheChanged;
		private readonly NotifyCollectionChangedEventHandler _onClinicCacheChanged;
		private readonly NotifyCollectionChangedEventHandler _onDoctorTypeCacheChanged;

		private void AcceptChangesButton_OnClick(object sender, RoutedEventArgs e)
		{
			throw new NotImplementedException();
		}

		private void DiscardChangesButton_OnClick(object sender, RoutedEventArgs e)
		{
			throw new NotImplementedException();
		}

		private void AddDocumentButton_Click(object sender, RoutedEventArgs e)
		{

		}

		private void ScanDocumentButton_Click(object sender, RoutedEventArgs e)
		{

		}

		private void RemoveDocumentButton_Click(object sender, RoutedEventArgs e)
		{

		}
	}
}
