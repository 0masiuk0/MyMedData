using Aviad.WPF.Controls;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
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
	/// Логика взаимодействия для RecordDisplay.xaml
	/// </summary>
	public partial class RecordDisplay : UserControl
	{
		public RecordDisplay()
		{
			InitializeComponent();
			_doctorsView = (CollectionViewSource)TryFindResource("Doctors");
			_examinationTypesView = (CollectionViewSource)TryFindResource("ExaminationTypes");
			_clinicsView = (CollectionViewSource)TryFindResource("Clinics");

			_doctorsView.Source = new List<Doctor>();
			_examinationTypesView.Source = new List<ExaminationRecord>();
			_clinicsView.Source = new List<Clinic>();

			_onClinicCacheChanged = (o, e) => _clinicsView.View.Refresh();
			_onDoctorCacheChanged = (o, e) => _doctorsView.View.Refresh();
			_onLabExamTypeCacheChanged = (o, e) => _examinationTypesView.View.Refresh();
			_onDoctorTypeCacheChanged = (o, e) => _examinationTypesView.View.Refresh();
		}

		private readonly CollectionViewSource _doctorsView;
		private readonly CollectionViewSource _examinationTypesView;
		private readonly CollectionViewSource _clinicsView;

		//сюда привязан selected item листа записей
		public object Item
		{
			get => GetValue(ItemProperty);
			set => SetValue(ItemProperty, value);
		}

		public static readonly DependencyProperty ItemProperty =
			DependencyProperty.Register(nameof(Item), typeof(object), typeof(RecordDisplay), new UIPropertyMetadata(SelectedItemChanged));

		public ExaminationRecord? EditedRecord
		{
			get => (ExaminationRecord?)GetValue(EditedRecordProperty);
			set => SetValue(EditedRecordProperty, value);
		}

		public static readonly DependencyProperty EditedRecordProperty =
			DependencyProperty.Register(nameof(EditedRecord), typeof(ExaminationRecord), typeof(RecordDisplay), new UIPropertyMetadata());

		private bool _hasUnsavedChanges;
		public bool HasUnsavedChanges
		{
			get => _hasUnsavedChanges;
			set
			{
				_hasUnsavedChanges = value;
				if (value)
				{
					AcknowledgeChangesPanel.Visibility = Visibility.Visible;
				}
				else
				{
					AcknowledgeChangesPanel.Visibility = Visibility.Collapsed;
				}
			}
		}

		private static void SelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			RecordDisplay recordDisplay = (RecordDisplay)d;
			if (e.NewValue is ExaminationRecord record)
			{
				recordDisplay.EditedRecord = record.Copy();

				Session session = recordDisplay.DataContext as Session;

				session.LabTestTypesCache.CollectionChanged -= recordDisplay._onLabExamTypeCacheChanged;
				session.DoctorTypesCache.CollectionChanged -= recordDisplay._onDoctorTypeCacheChanged;

				if (record is LabExaminationRecord)
				{
					recordDisplay.DoctorLabel.Visibility = Visibility.Collapsed;
					recordDisplay.DoctorTextBox.Visibility = Visibility.Collapsed;

					recordDisplay._examinationTypesView.Source = session.LabTestTypesCache;
					session.LabTestTypesCache.CollectionChanged += recordDisplay._onLabExamTypeCacheChanged;
				}
				else if (record is DoctorExaminationRecord docExam)
				{
					recordDisplay.DoctorLabel.Visibility = Visibility.Visible;
					recordDisplay.DoctorTextBox.Visibility = Visibility.Visible;

					recordDisplay._examinationTypesView.Source = session.DoctorTypesCache;
					session.DoctorTypesCache.CollectionChanged += recordDisplay._onDoctorTypeCacheChanged;
				}
			}
			else
			{
				recordDisplay.EditedRecord=null;
			}
		}

		private readonly NotifyCollectionChangedEventHandler _onDoctorCacheChanged;
		private readonly NotifyCollectionChangedEventHandler _onClinicCacheChanged;
		private readonly NotifyCollectionChangedEventHandler _onLabExamTypeCacheChanged;
		private readonly NotifyCollectionChangedEventHandler _onDoctorTypeCacheChanged;

		private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (e.NewValue is Session session) 
			{
				// Насколько я понимаю принципы сбора мусора, отписываться не обязательно, поскольку хэндлеры никогда собирать и не надо
				// т.к. RecordDisplay один и живет все время.
				_doctorsView.Source = session.DoctorNameCache;				
				session.DoctorNameCache.CollectionChanged += _onDoctorCacheChanged;
								
				_clinicsView.Source = session.ClinicNameCache;				
				session.ClinicNameCache.CollectionChanged += _onClinicCacheChanged;
			}
			else
			{
				_clinicsView.Source = new List<Clinic>();
				_doctorsView.Source = new List<Doctor>();
			}
		}
	}

	[ValueConversion(typeof(DateTime), typeof(DateOnly))]
	public class DateTimeToDateOnlyConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is DateTime dateTime)
			{
				return new DateOnly(dateTime.Year, dateTime.Month, dateTime.Day);
			}

			return DependencyProperty.UnsetValue;
	}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is DateOnly dateOnly)
			{
				return new DateTime(dateOnly.Year, dateOnly.Month, dateOnly.Day);
			}

			return DependencyProperty.UnsetValue;
		}
	}

}
