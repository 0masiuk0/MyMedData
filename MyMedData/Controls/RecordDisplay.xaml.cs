using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.IO;
using AutoCompleteTextBox;

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
			
			_examinationTypesProvider = (AutocompleteExaminationTypesSuggestionProvider)TryFindResource("ExTypesSuggProvider");
			_doctorsProvider = (AutocompleteDoctorSuggestionProvider)TryFindResource("DocSuggProvider");
			_clinicsProvider = (AutocompleteClinicSuggestionProvider)TryFindResource("ClinicSuggProvider");

			HasUnsavedChanges = false;

			RecordDatePicker.SelectedDateChanged += (o, e) => UserInputChanged();			
			CommentTextBox.TextChanged += (o, e) => UserInputChanged();

			//ExaminationTypeTextBox.TextChanged += (o, e) => UserInputChanged();
			//DoctorTextBox.TextChanged += (o, e) => UserInputChanged();
			//ClinicTextBox.TextChanged += (o, e) => UserInputChanged();
		}		

		private readonly AutocompleteDoctorSuggestionProvider _doctorsProvider;
		private readonly AutocompleteExaminationTypesSuggestionProvider _examinationTypesProvider;
		private readonly AutocompleteClinicSuggestionProvider _clinicsProvider;		

		private void RecordDisplay_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (e.NewValue is Session session)
			{
				_doctorsProvider.Source = session.DoctorCache;
				_clinicsProvider.Source = session.ClinicCache;
			}
			else
			{
				_doctorsProvider.Source = new List<Doctor>();
				_clinicsProvider.Source = new List<Clinic>();
			}
		}

		public ObservableCollection<DocumentAttachment> DocumentsAttechmentEditedCollection { get; private set; }

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
			if (e.OldValue is ExaminationRecord oldRecord && recordDisplay.HasUnsavedChanges && recordDisplay.DataContext is Session sess && sess.ExaminationRecords.Contains(oldRecord))
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
				recordDisplay.ExamniantionTypeLabel.Text = item is LabExaminationRecord ? "Вид обследования" : "Врачебная специальность";
			}
			else
			{
				recordDisplay.DocOrLab = null;
				recordDisplay.ExamniantionTypeLabel.Text = "Вид обследования";
			}
		}

		private void LoadItemToRecordDisplayUI(ExaminationRecord item, Session session)
		{
			DocumentsAttechmentEditedCollection = new ObservableCollection<DocumentAttachment>(item.Documents);
			DocumentsAttechmentEditedCollection.CollectionChanged += (o, e) => UserInputChanged(); 

			ExaminationDate = item.Date;
			ExaminationType = item.ExaminationType?.DeepCopy() ?? null;
			Clinic = item.Clinic?.DeepCopy() ?? null;
			Comment = item.Comment;

			if (item is LabExaminationRecord)
			{
				DocOrLab = DocOrLabExamination.Lab;
				Doctor = null;

				DoctorLabel.Visibility = Visibility.Collapsed;
				DoctorTextBox.Visibility = Visibility.Collapsed;

				_examinationTypesProvider.Source= session.LabTestTypesCache;
			}
			else if (item is DoctorExaminationRecord docExam)
			{
				DocOrLab = DocOrLabExamination.Doc;
				Doctor = docExam.Doctor?.DeepCopy() ?? null;

				DoctorLabel.Visibility = Visibility.Visible;
				DoctorTextBox.Visibility = Visibility.Visible;

				_examinationTypesProvider.Source = session.DoctorTypesCache;
			}
		}

		private ExaminationRecord? ConstructRecordFormUI()
		{
			if (Item is ExaminationRecord record)
			{
				ExaminationRecord editedRecord = record.DeepCopy();
				
				editedRecord.Date = ExaminationDate;
				editedRecord.Clinic = Clinic;
				editedRecord.Comment = Comment;
				editedRecord.ExaminationType = ExaminationType;				

				if (editedRecord is DoctorExaminationRecord docRecord)
					docRecord.Doctor = Doctor;						

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

		private void UserInputChanged()
		{
			if (Item is not ExaminationRecord initialItem)
				return;

			HasUnsavedChanges = !initialItem.IsDataEqual(ConstructRecordFormUI());
		}

		#region DepdencyPropertires
		//------------------------------------------DEPENDENCY PROPERTIES----------------------------------------------------------
		//сюда привязан selected item листа записей
		public object Item
		{
			get => GetValue(ItemProperty);
			set => SetValue(ItemProperty, value);
		}

		public static readonly DependencyProperty ItemProperty =
			DependencyProperty.Register(nameof(Item), typeof(object), typeof(RecordDisplay), new UIPropertyMetadata(ItemChanged));


		public DateOnly ExaminationDate
		{
			set => SetValue(ExaminationDateProperty, value);
			get => (DateOnly)GetValue(ExaminationDateProperty);
		}


		public static readonly DependencyProperty ExaminationDateProperty =
			DependencyProperty.Register(nameof(ExaminationDate), typeof(DateOnly), typeof(RecordDisplay),
				new UIPropertyMetadata(DateOnly.FromDateTime(DateTime.Today)));

		public ExaminationType? ExaminationType
		{
			set => SetValue(ExaminationTypeTitleProperty, value);
			get => (ExaminationType?)GetValue(ExaminationTypeTitleProperty);
		}

		public static readonly DependencyProperty ExaminationTypeTitleProperty =
			DependencyProperty.Register(nameof(ExaminationType), typeof(ExaminationType), typeof(RecordDisplay), new UIPropertyMetadata());


		public Doctor? Doctor
		{
			set => SetValue(DoctorNameProperty, value);
			get => (Doctor?)GetValue(DoctorNameProperty);
		}

		public static readonly DependencyProperty DoctorNameProperty =
			DependencyProperty.Register(nameof(Doctor), typeof(Doctor), typeof(RecordDisplay), new UIPropertyMetadata());


		public Clinic? Clinic
		{
			set => SetValue(ClinicNameProperty, value);
			get => (Clinic?)GetValue(ClinicNameProperty);
		}

		public static readonly DependencyProperty ClinicNameProperty =
			DependencyProperty.Register(nameof(Clinic), typeof(Clinic), typeof(RecordDisplay), new UIPropertyMetadata());


		public string Comment
		{
			set => SetValue(CommentProperty, value);
			get => (string)GetValue(CommentProperty);
		}

		public static readonly DependencyProperty CommentProperty =
			DependencyProperty.Register(nameof(Comment), typeof(string), typeof(RecordDisplay), new UIPropertyMetadata(""));
		#endregion

		//----------------------------------------------------UI EVENTS--------------------------------------------------------------------
		private void UploadDocButton_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog opf = new OpenFileDialog();
			opf.Filter = "Availible types|*.pdf;*.jpg;*.png";
			opf.Multiselect = true;

			if (opf.ShowDialog() == true)
			{
				foreach (var fileName in opf.FileNames)
				{
					string extention = fileName.Substring(fileName.Length - 4, 3);
					DocumentType documentType;

					switch (extention)
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

		private void ClinicTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			//AutoCompleteTextBox clinicTB = sender as AutoCompleteTextBox;
			//string clinicName = clinicTB?.Text ?? "";

			//if (string.IsNullOrEmpty(clinicName))
			//	Clinic = null;
			//else if (DataContext is Session session && session.ClinicCache.FirstOrDefault(cl => cl.Name == clinicName) is Clinic clinicFromCache)
			//	Clinic = clinicFromCache;
			//else if (Clinic != null)
			//	Clinic.Name = clinicName;
			//else
			//	Clinic = new Clinic(clinicName);
				
		}
		
		private void ExaminationTypeTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			//if (DocOrLab is not DocOrLabExamination docOrLab)
			//	return;

			//AutoCompleteTextBox examTypeTB = (AutoCompleteTextBox)sender;
			//string examTypeTitle = examTypeTB?.Text ?? "";						

			//if (string.IsNullOrEmpty(examTypeTitle))
			//	ExaminationType = null;
			//else if (DataContext is Session session && FindExaminationTypeInBothCases(session, examTypeTitle, docOrLab) is ExaminationType examinationType)
			//	ExaminationType = examinationType;
			//else if (ExaminationType != null)
			//	ExaminationType.ExaminationTypeTitle = examTypeTitle;
			//else
			//	ExaminationType = new ExaminationType(examTypeTitle);
		}

		private void DoctorTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			//AutoCompleteTextBox docTB = (AutoCompleteTextBox)sender;
			//string docName = docTB?.Text ?? "";

			//if (string.IsNullOrEmpty(docName))
			//	Doctor = null;
			//else if (DataContext is Session session && session.DoctorCache.FirstOrDefault(doc => doc.Name == docName) is Doctor doctor)
			//	Doctor = doctor;
			//else if (Doctor != null)
			//	Doctor.Name = docName;
			//else
			//	Doctor = new Doctor(docName);
		}

		private void CommentTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			TextBox commentTB = (TextBox)sender;
			Comment = commentTB?.Text ?? "";
		}

		private void RecordDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
		{
			DatePicker dp = (DatePicker)sender;
			ExaminationDate = DateOnly.FromDateTime(dp.SelectedDate ?? DateTime.Today);
		}

		//-----------------------------------------------------------EVENTS----------------------------------------------------------------
		public delegate void ChangesSavedToDBEventHandler(object sender, ChangesSavedToDBEventArgs e);

		public event ChangesSavedToDBEventHandler ChangesSavedToDB;

		protected virtual void RaiseChangesSavedToDBEvent()
		{
			ChangesSavedToDB?.Invoke(this, new ChangesSavedToDBEventArgs(ConstructRecordFormUI()));
		}

		//----------------------------------------------------AUXILIARIES----------------------------------------------------------
		public ExaminationType? FindExaminationTypeInBothCases(Session session, string examinationTypeTitle, DocOrLabExamination docOrLab)
		{
			if (docOrLab == DocOrLabExamination.Doc)
				return session.DoctorTypesCache.FirstOrDefault(docType => docType.ExaminationTypeTitle == examinationTypeTitle);
			else
				return session.LabTestTypesCache.FirstOrDefault(labType => labType.ExaminationTypeTitle == examinationTypeTitle);
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
