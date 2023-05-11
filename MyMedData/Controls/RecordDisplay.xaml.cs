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
using System.Windows.Controls.Primitives;

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

			HasUnsavedChanges = false;

			ExaminationDatePicker.SelectedDateChanged += (o, e) => UpdateRecordDisplay();			
			CommentTextBox.TextChanged += (o, e) => UpdateRecordDisplay();

			EntityPopup = (Popup)TryFindResource("EntityChoicePopup");
			EntityPopup.Closed += EntityPopup_Closed;
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
            }
            else
            {
                recordDisplay.DocOrLab = null;
                recordDisplay.ExamniantionTypeLabel.Text = "Вид обследования";
                recordDisplay.ExaminationTypeButton.Content = "Выбор исследования";
                recordDisplay.ClinicButton.Content = "Выбор мед. учереждения";
                recordDisplay.DoctorButton.Content = "Выбор врача";
            }
        }

		Popup EntityPopup;
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

		private void LoadItemToRecordDisplayUI(ExaminationRecord item, Session session)
		{
			DocumentsAttechmentEditedCollection = new ObservableCollection<DocumentAttachment>(item.Documents);
			DocumentsAttechmentEditedCollection.CollectionChanged += (o, e) => UpdateRecordDisplay(); 

			ExaminationDate = item.Date;
			ExaminationType = item.ExaminationType?.DeepCopy() ?? null;
			
			Clinic = item.Clinic?.DeepCopy() ?? null;
			ClinicButton.Content = item.Clinic?.ToString() ?? "Выбор мед. учереждения";
			Comment = item.Comment;

			if (item is LabExaminationRecord)
			{
                ExaminationTypeButton.Content = item.ExaminationType?.ToString() ?? "Выбор исследования";

                DocOrLab = DocOrLabExamination.Lab;
				Doctor = null;

				DoctorLabel.Visibility = Visibility.Collapsed;
				DoctorButton.Visibility = Visibility.Collapsed;
			}
			else if (item is DoctorExaminationRecord docExam)
			{
                ExaminationTypeButton.Content = item.ExaminationType?.ToString() ?? "Выбор специальности";

                DocOrLab = DocOrLabExamination.Doc;
				Doctor = docExam.Doctor?.DeepCopy() ?? null;
				DoctorButton.Content = docExam.Doctor?.ToString() ?? "Выбор врача";

				DoctorLabel.Visibility = Visibility.Visible;
                DoctorButton.Visibility = Visibility.Visible;
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

				editedRecord.Documents = new List<DocumentAttachment>(DocumentsAttechmentEditedCollection);

				return editedRecord;
			}
			else
				return null;
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

        private void RemoveDocumentAttachmentById(string documentID)
		{

			foreach (var document in DocumentsAttechmentEditedCollection)
			{
				if (document.Id == documentID)
					DocumentsAttechmentEditedCollection.Remove(document);
			}
		}

		private void UpdateRecordDisplay()
		{
			if (Item is not ExaminationRecord initialItem)
				return;

			DoctorButton.Content = Doctor?.ToString() ?? "Выбор врача";
			ExaminationTypeButton.Content = ExaminationType?.ToString() ?? "Выбор исследования";
			ClinicButton.Content = Clinic?.ToString() ?? "Выбор мед. учереждения";

			HasUnsavedChanges = !initialItem.IsDataEqual(ConstructRecordFormUI());
		}

        private void ShowPopup(Button placementTargert, object cache)
        {
            if (cache is ObservableCollection<Doctor> || cache is ObservableCollection<ExaminationType> || cache is ObservableCollection<Clinic>)
            {               
                EntityPopup.PlacementTarget = placementTargert;
                EntityPopup.VerticalOffset = placementTargert.ActualHeight;
                EntityPopup.HorizontalOffset = 0;

                var entityManager = (EntityManager)EntityPopup.Child;
                
                entityManager.DataContext = cache;
				entityManager.Clear();

				entityManager.SelectionDone += (o, e) => EntityPopup.IsOpen = false;
				
                EntityPopup.IsOpen = true;
				EntityPopup.Focus();
            }
        }

		private void EntityPopup_Closed(object? sender, EventArgs e)
		{
			if (EntityPopup.Child is EntityManager entityManager)
			{
				switch (entityManager.SelectedItem)
				{
					case ExaminationType exType: ExaminationType = exType; break;
					case Doctor doc: Doctor = doc; break;
					case Clinic clinic: Clinic = clinic; break;
					default: return;
				}
				UpdateRecordDisplay();
			}
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
            get => (DateOnly)GetValue(ExaminationDateProperty);
            set => SetValue(ExaminationDateProperty, value);
        }

		public static readonly DependencyProperty ExaminationDateProperty =
			DependencyProperty.Register(nameof(ExaminationDate), typeof(DateOnly), typeof(RecordDisplay),
				new UIPropertyMetadata());

        public ExaminationType? ExaminationType
		{
			get; private set;
		}

		public Doctor? Doctor
		{
            get; private set;
        }

		Clinic _newClinicInstance = new Clinic();
		public Clinic? Clinic
		{
			get; private set;
		}

		public string Comment
		{
            get => (string)GetValue(CommentProperty);
            set => SetValue(CommentProperty, value);
        }

        public static readonly DependencyProperty CommentProperty =
            DependencyProperty.Register(nameof(Comment), typeof(string), typeof(RecordDisplay), new UIPropertyMetadata(""));
        #endregion

        //----------------------------------------------------UI EVENTS--------------------------------------------------------------------       
        

        private void ExaminationTypeButton_Click(object sender, RoutedEventArgs e)
        {
			Button button = (Button)sender;

			if (DataContext is Session session) 
			{
				if (DocOrLab == DocOrLabExamination.Doc)
					ShowPopup(button, session.DoctorTypesCache);
				else
                    ShowPopup(button, session.LabTestTypesCache);
            }
        }

		private void DoctorButton_Click(object sender, RoutedEventArgs e)
		{
			Button button = (Button)sender;

			if (DataContext is Session session)
			{
				ShowPopup(button, session.DoctorCache);
			}
		}

		private void ClinicButton_Click(object sender, RoutedEventArgs e)
		{
			Button button = (Button)sender;

			if (DataContext is Session session)
			{
				ShowPopup(button, session.ClinicCache);
			}
		}

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

		private void CommentTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			TextBox commentTB = (TextBox)sender;
			Comment = commentTB?.Text ?? "";
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
