using Aviad.WPF.Controls;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.IO;
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

			HasUnsavedChanges = false;
		}

		private readonly CollectionViewSource _doctorsView;
		private readonly CollectionViewSource _examinationTypesView;
		private readonly CollectionViewSource _clinicsView;
		
		private readonly NotifyCollectionChangedEventHandler _onDoctorCacheChanged;
		private readonly NotifyCollectionChangedEventHandler _onClinicCacheChanged;
		private readonly NotifyCollectionChangedEventHandler _onLabExamTypeCacheChanged;
		private readonly NotifyCollectionChangedEventHandler _onDoctorTypeCacheChanged;

		private void RecordDisplay_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
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

		//сюда привязан selected item листа записей
		public object Item
		{
			get => GetValue(ItemProperty);
			set => SetValue(ItemProperty, value);
		}

		public static readonly DependencyProperty ItemProperty =
			DependencyProperty.Register(nameof(Item), typeof(object), typeof(RecordDisplay), new UIPropertyMetadata(ItemChanged));

		//shall only be changed within ItemChanged handler.
		public ExaminationRecord? EditedRecord
		{
			get => (ExaminationRecord?)GetValue(EditedRecordProperty);
			set => SetValue(EditedRecordProperty, value);
		}

		public static readonly DependencyProperty EditedRecordProperty =
			DependencyProperty.Register(nameof(EditedRecord), typeof(ExaminationRecord), typeof(RecordDisplay), new UIPropertyMetadata());

		public ObservableCollection<DocumentAttachment> DocumentsAttechmentEditedCollection;

		public DocOrLabExamination? DocOrLab;

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

		public void ApplyChangesOfEditedRecord()
		{			
			if (DataContext is Session session && EditedRecord != null)
			{
				bool updateSuccess = session.AddOrUpdateExaminationRecord(EditedRecord);
				if (updateSuccess)
					RaiseChangesSavedToDBEvent();
				else
					MessageBox.Show("Не получилось обновить запись в базе.", "Ошибка", MessageBoxButton.OK,
						MessageBoxImage.Error);
			}

			//if (Item is ExaminationRecord rec)
			//{
			//	EditedRecord = rec.DeepCopy();
			//	EditedRecord.PropertyChanged += EditedRecord_PropertyChanged;
			//	HasUnsavedChanges = false;
			//}
		}

		private static void ItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			RecordDisplay recordDisplay = (RecordDisplay)d;

			//Unsaved changes of old record
			if (e.OldValue is ExaminationRecord oldRecord && recordDisplay.HasUnsavedChanges && recordDisplay.DataContext is Session sess)
			{
				if (MessageBox.Show("Сохранить предыдущие изменения?", "Несохраненные изменения", MessageBoxButton.YesNo, MessageBoxImage.Question)
				    == MessageBoxResult.Yes)
				{
					recordDisplay.ApplyChangesOfEditedRecord();
				}
			}
			recordDisplay.HasUnsavedChanges = false;

			//Rebind to new record
			if (e.NewValue is ExaminationRecord record && recordDisplay.DataContext is Session session)
			{
				recordDisplay.EditedRecord = record.DeepCopy();
				recordDisplay.DocumentsAttechmentEditedCollection = new ObservableCollection<DocumentAttachment>(recordDisplay.EditedRecord.Documents);				

				//we do not know if _examinationTypesView was bounbd to doc types or lab types. Just unsubscribe both
				session.LabTestTypesCache.CollectionChanged -= recordDisplay._onLabExamTypeCacheChanged;
				session.DoctorTypesCache.CollectionChanged -= recordDisplay._onDoctorTypeCacheChanged;

				if (record is LabExaminationRecord)
				{
					recordDisplay.DocOrLab = DocOrLabExamination.Lab;

					recordDisplay.DoctorLabel.Visibility = Visibility.Collapsed;
					recordDisplay.DoctorTextBox.Visibility = Visibility.Collapsed;

					recordDisplay._examinationTypesView.Source = session.LabTestTypesCache;
					session.LabTestTypesCache.CollectionChanged += recordDisplay._onLabExamTypeCacheChanged;
				}
				else if (record is DoctorExaminationRecord docExam)
				{
					recordDisplay.DocOrLab = DocOrLabExamination.Doc;

					recordDisplay.DoctorLabel.Visibility = Visibility.Visible;
					recordDisplay.DoctorTextBox.Visibility = Visibility.Visible;

					recordDisplay._examinationTypesView.Source = session.DoctorTypesCache;
					session.DoctorTypesCache.CollectionChanged += recordDisplay._onDoctorTypeCacheChanged;
				}

				recordDisplay.EditedRecord.PropertyChanged += recordDisplay.EditedRecord_PropertyChanged;
			}
			else
			{
				recordDisplay.EditedRecord = null;
				recordDisplay.DocOrLab = null;
			}
		}

		private void AddDocumentAttachment(DocumentAttachment document)
		{
			if (EditedRecord is ExaminationRecord)
			{
				DocumentsAttechmentEditedCollection.Add(document);
				EditedRecord.Documents.Add(document);
			}
		}

		private void RemoveDocumentAttachmentById(string documentID) 
		{
			if (EditedRecord is ExaminationRecord)
			{
				foreach(var document in DocumentsAttechmentEditedCollection)
				{
					if (document.Id == documentID)
						DocumentsAttechmentEditedCollection.Remove(document);
				}

				foreach(var document in EditedRecord.Documents)
				{
					if (document.Id == documentID)
						DocumentsAttechmentEditedCollection.Remove(document);
				}
			}
		}		

		private void EditedRecord_PropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			if (Item is ExaminationRecord record)
				HasUnsavedChanges = EditedRecord?.IsEqualTo(record) ?? false;
			else
				HasUnsavedChanges = false;
		}			

		private void AcceptChangesButton_Click(object sender, RoutedEventArgs e)
		{
			ApplyChangesOfEditedRecord();
		}		

		private void DiscardChangesButton_Click(object sender, RoutedEventArgs e)
		{
			if (Item is ExaminationRecord rec)
			{
				EditedRecord = rec.DeepCopy();
				EditedRecord.PropertyChanged += EditedRecord_PropertyChanged;
				HasUnsavedChanges = false;
			}
		}

		private void UploadDocButton_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog opf = new OpenFileDialog();
			opf.Filter = "Availible types|*.pdf;*.jpg;*.png";
			opf.Multiselect = true;
			
			if (opf.ShowDialog() == true)
			{
				foreach(var fileName in opf.FileNames) 
				{
					string extention = fileName.Substring(fileName.Length - 4, 3);
					DocumentType documentType;

					switch(extention)
					{
						case "jpg": documentType = DocumentType.JPEG; break;
						case "png": documentType = DocumentType.PNG; break;
						case "pdf": documentType = DocumentType.PDF; break;
						default: MessageBox.Show(fileName, "Недопустимый файл", MessageBoxButton.OK, MessageBoxImage.Error); return;
					}

					DocumentAttachment document = new DocumentAttachment();
					document.FileName = fileName;
					document.DocumentType = documentType;
					document.Data = File.ReadAllBytes(fileName);

					AddDocumentAttachment(document);
				}
			}
        }

		private void ScanDocButton_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("Функционал не готов.", "", MessageBoxButton.OK, MessageBoxImage.Exclamation);
		}

		private void RemoveDocButton_Click(object sender, RoutedEventArgs e)
		{
			if (DocumentsListBox.SelectedItem is DocumentAttachment document)
			{
				RemoveDocumentAttachmentById(document.Id);
			}
		}

		//EVENTS
		public delegate void ChangesSavedToDBEventHandler(object sender, ChangesSavedToDBEventArgs e);

		public event ChangesSavedToDBEventHandler ChangesSavedToDB;

		protected virtual void RaiseChangesSavedToDBEvent()
		{
			ChangesSavedToDB?.Invoke(this, new ChangesSavedToDBEventArgs(EditedRecord));
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

	public class ChangesSavedToDBEventArgs
	{
		public ExaminationRecord? NewRecord;

		public ChangesSavedToDBEventArgs() { }

		public ChangesSavedToDBEventArgs(ExaminationRecord? newRecord)
		{
			NewRecord = newRecord;
		}
	}

}
