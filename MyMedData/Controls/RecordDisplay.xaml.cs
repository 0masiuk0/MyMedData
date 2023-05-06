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
					ApplyChangesPanel.Visibility = Visibility.Visible;
				}
				else
				{
					ApplyChangesPanel.Visibility = Visibility.Collapsed;
				}
			}
		}

		public void ApplyChangesOfEditedRecord()
		{			
			if (DataContext is Session session && ConstructRecordFormUI() is ExaminationRecord record)
			{
				bool updateSuccess = session.AddOrUpdateExaminationRecord(record);
				if (updateSuccess)
					RaiseChangesSavedToDBEvent();
				else
					MessageBox.Show("Не получилось обновить запись в базе.", "Ошибка", MessageBoxButton.OK,
						MessageBoxImage.Error);
			}
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
			if (e.NewValue is ExaminationRecord item && recordDisplay.DataContext is Session session)
			{
				recordDisplay.LoadItemToRecordDisplayUI(item, session);
			}
			else
			{
				recordDisplay.DocOrLab = null;
			}
		}

		private void LoadItemToRecordDisplayUI(ExaminationRecord item, Session session)
		{
			DocumentsAttechmentEditedCollection = new ObservableCollection<DocumentAttachment>(item.Documents);
						
			//we do not know if _examinationTypesView was bound to doc types or lab types. Just unsubscribe both.
			session.LabTestTypesCache.CollectionChanged -= _onLabExamTypeCacheChanged;
			session.DoctorTypesCache.CollectionChanged -= _onDoctorTypeCacheChanged;

			if (item is LabExaminationRecord)
			{
				DocOrLab = DocOrLabExamination.Lab;
				DoctorName = "";

				DoctorLabel.Visibility = Visibility.Collapsed;
				DoctorTextBox.Visibility = Visibility.Collapsed;

				_examinationTypesView.Source = session.LabTestTypesCache;
				session.LabTestTypesCache.CollectionChanged += _onLabExamTypeCacheChanged;
			}
			else if (item is DoctorExaminationRecord docExam)
			{
				DocOrLab = DocOrLabExamination.Doc;
				DoctorName = docExam.Doctor?.Name ?? "";

				DoctorLabel.Visibility = Visibility.Visible;
				DoctorTextBox.Visibility = Visibility.Visible;

				_examinationTypesView.Source = session.DoctorTypesCache;
				session.DoctorTypesCache.CollectionChanged += _onDoctorTypeCacheChanged;
			}
		}

		private ExaminationRecord? ConstructRecordFormUI()
		{
			if (Item is ExaminationRecord record)
			{
				ExaminationRecord editedRecord = record.DeepCopy();
				
				//Date
				editedRecord.Date = ExaminationDate ?? DateOnly.FromDateTime(DateTime.Today);

				//ExaminationType
				if(ExaminationTypeTitle != null && ExaminationTypeTitle != "")
				{
					if (editedRecord.ExaminationType != null)
						editedRecord.ExaminationType.ExaminationTypeTitle = ExaminationTypeTitle;
					else
						editedRecord.ExaminationType = new ExaminationType(ExaminationTypeTitle);
				}
				else
				{
					editedRecord.ExaminationType = null;
				}

				//Doctor
				if (editedRecord is DoctorExaminationRecord docRecord)
				{
					if (DoctorName != null && DoctorName != "")
					{

						if (docRecord.Doctor != null)
							docRecord.Doctor.Name = DoctorName;
						else
							docRecord.Doctor = new Doctor(DoctorName);
					}
					else
					{
						docRecord.Doctor = null;
					}
				}

				//Clinic
				if (ClinicName != null && ClinicName != "")
				{
					if (editedRecord.Clinic != null)
						editedRecord.Clinic.Name = ClinicName;
					else
						editedRecord.Clinic = new Clinic(Name);
				}
				else
				{
					editedRecord.Clinic = null;
				}

				//Comment
				if(Comment != null)
				{
					editedRecord.Comment = Comment;
				}

				return editedRecord;
			}
			else
				return null;
		}

		private void RemoveDocumentAttachmentById(string documentID)
		{

			foreach (var document in DocumentsAttechmentEditedCollection)
			{
				if (document.Id == documentID)
					DocumentsAttechmentEditedCollection.Remove(document);
			}
		}

		private static void UserInputChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			RecordDisplay recordDisplay = (RecordDisplay)sender;
			if (recordDisplay.Item is not ExaminationRecord initialItem)
				return;
						
			bool changesFound = 
				initialItem.Date == recordDisplay.ExaminationDate
				&& initialItem.Clinic?.Name == recordDisplay.ClinicName
				&& initialItem.ExaminationType?.ExaminationTypeTitle == recordDisplay.ExaminationTypeTitle
				&& initialItem.Comment == recordDisplay.Comment;

			if (initialItem is DoctorExaminationRecord docRec)
			{
				changesFound &= docRec.Doctor?.Name == recordDisplay.DoctorName;
			}

			changesFound &= Enumerable.SequenceEqual(initialItem.Documents.OrderBy(d => d.Id), recordDisplay.DocumentsAttechmentEditedCollection.OrderBy(d => d.Id));
			
			if (changesFound)
				recordDisplay.ApplyChangesPanel.Visibility = Visibility.Visible;
			else
				recordDisplay.ApplyChangesPanel.Visibility= Visibility.Collapsed;
		}			

		private void AcceptChangesButton_Click(object sender, RoutedEventArgs e)
		{
			ApplyChangesOfEditedRecord();
		}		

		private void DiscardChangesButton_Click(object sender, RoutedEventArgs e)
		{
			if (Item is ExaminationRecord rec && DataContext is Session session)
			{
				LoadItemToRecordDisplayUI(rec, session);
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

					DocumentsAttechmentEditedCollection.Add(document);
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

		//------------------------------------------DEPENDENCY PROPERTIES----------------------------------------------------------

		//сюда привязан selected item листа записей
		public object Item
		{
			get => GetValue(ItemProperty);
			set => SetValue(ItemProperty, value);
		}

		public static readonly DependencyProperty ItemProperty =
			DependencyProperty.Register(nameof(Item), typeof(object), typeof(RecordDisplay), new UIPropertyMetadata(ItemChanged));


		public DateOnly? ExaminationDate
		{
			set => SetValue(ExaminationDateProperty, value);
			get => (DateOnly?)GetValue(ExaminationDateProperty);
		}


		public static readonly DependencyProperty ExaminationDateProperty =
			DependencyProperty.Register(nameof(ExaminationDate), typeof(DateOnly?), typeof(RecordDisplay),
				new UIPropertyMetadata(DateOnly.FromDateTime(DateTime.Today), UserInputChanged));

		public string ExaminationTypeTitle
		{
			set => SetValue(ExaminationTypeTitleProperty, value);
			get => (string)GetValue(ExaminationTypeTitleProperty);
		}

		public static readonly DependencyProperty ExaminationTypeTitleProperty =
			DependencyProperty.Register(nameof(ExaminationTypeTitle), typeof(string), typeof(RecordDisplay), new UIPropertyMetadata("", UserInputChanged));


		public string DoctorName
		{
			set => SetValue(DoctorNameProperty, value);
			get => (string)GetValue(DoctorNameProperty);
		}

		public static readonly DependencyProperty DoctorNameProperty =
			DependencyProperty.Register(nameof(DoctorName), typeof(string), typeof(RecordDisplay), new UIPropertyMetadata("", UserInputChanged));


		public string ClinicName
		{
			set => SetValue(ClinicNameProperty, value);
			get => (string)GetValue(ClinicNameProperty);
		}

		public static readonly DependencyProperty ClinicNameProperty =
			DependencyProperty.Register(nameof(ClinicName), typeof(string), typeof(RecordDisplay), new UIPropertyMetadata("", UserInputChanged));


		public string Comment
		{
			set => SetValue(CommentProperty, value);
			get => (string)GetValue(CommentProperty);
		}

		public static readonly DependencyProperty CommentProperty =
			DependencyProperty.Register(nameof(Comment), typeof(string), typeof(RecordDisplay), new UIPropertyMetadata("", UserInputChanged));
					

		//-----------------------------------------------------------EVENTS----------------------------------------------------------------
		public delegate void ChangesSavedToDBEventHandler(object sender, ChangesSavedToDBEventArgs e);

		public event ChangesSavedToDBEventHandler ChangesSavedToDB;

		protected virtual void RaiseChangesSavedToDBEvent()
		{
			ChangesSavedToDB?.Invoke(this, new ChangesSavedToDBEventArgs(ConstructRecordFormUI()));
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
