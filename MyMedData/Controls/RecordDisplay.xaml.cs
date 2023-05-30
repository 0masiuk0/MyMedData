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
using System.Net.Mail;
using MyMedData.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using MyMedData.Classes;
using System.Xml.Linq;

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
						
			AttachmentEditedCollection.CollectionChanged += (o, e) => UpdateRecordDisplay();
			ResetRecordView();
		}

		private void RecordDisplay_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			RecordDisplay recordDisplay = (RecordDisplay)sender;
			if (e.NewValue is not Session)
			{
				recordDisplay.ResetRecordView();
			}
		}

		private static void ItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RecordDisplay recordDisplay = (RecordDisplay)d;

            //Unsaved changes of old record
            if (e.OldValue is ExaminationRecord oldRecord)
            {
				 if(recordDisplay.HasUnsavedChanges 
					&& recordDisplay.DataContext is Session sess 
					&& sess.ExaminationRecords.Contains(oldRecord)
					&& MessageBox.Show("Сохранить предыдущие изменения?", "Несохраненные изменения", MessageBoxButton.YesNo, MessageBoxImage.Question)
						== MessageBoxResult.Yes)
                {
                    recordDisplay.ApplyChangesOfEditedRecord();
                }

				oldRecord.Documents.ForEach(d => d.ClearData());
            }
            recordDisplay.HasUnsavedChanges = false;

            //Rebind to new record
            if (e.NewValue is ExaminationRecord item && recordDisplay.DataContext is Session session)
            {				
                recordDisplay.LoadItemToRecordDisplayUI(item, session);                
            }
            else
            {
				recordDisplay.ResetRecordView();
                recordDisplay.DocOrLab = null;
                recordDisplay.ExamniantionTypeLabel.Text = "Вид обследования";
                recordDisplay.ExaminationTypeButton.Content = "Выбор исследования";
                recordDisplay.ClinicButton.Content = "Выбор мед. учереждения";
                recordDisplay.DoctorButton.Content = "Выбор врача";
            }
        }

		Popup EntityPopup;

		TrulyObservableCollection<AttachmentMetaData> _attechmentEditedCollection = new();
		public TrulyObservableCollection<AttachmentMetaData> AttachmentEditedCollection 
		{
			get => _attechmentEditedCollection;
			private set => _attechmentEditedCollection = value;
		}

		public DocOrLabExamination? DocOrLab;

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
			AttachmentEditedCollection.Clear();
			item.Documents.ForEach(document => { AttachmentEditedCollection.Add(document.DeepCopy()); });			

			ExaminationDatePicker.SelectedDate = item.Date.ToDateTime();
			ExaminationType = item.ExaminationType?.DeepCopy() ?? null;
			
			Clinic = item.Clinic?.DeepCopy() ?? null;
			ClinicButton.Content = item.Clinic?.ToString() ?? "Выбор мед. учереждения";
			CommentTextBox.Text = item.Comment;

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

			UpdateRecordDisplay();
		}

		private ExaminationRecord? ConstructRecordFormUI()
		{
			if (Item is ExaminationRecord record)
			{
				ExaminationRecord editedRecord = record.DeepCopy();

				editedRecord.Date = DateOnly.FromDateTime(ExaminationDatePicker.SelectedDate ?? DateTime.Today);
				editedRecord.Clinic = Clinic;
				editedRecord.Comment = CommentTextBox.Text ?? "";
				editedRecord.ExaminationType = ExaminationType;				

				if (editedRecord is DoctorExaminationRecord docRecord)
					docRecord.Doctor = Doctor;

				editedRecord.Documents = new List<AttachmentMetaData>(AttachmentEditedCollection);

				return editedRecord;
			}
			else
				return null;
		}

        public void ApplyChangesOfEditedRecord()
        {
            if (DataContext is Session session && ConstructRecordFormUI() is ExaminationRecord record)
            {
				bool updateSuccess;
                if (Mode == RecordEditMode.New)
				{
					updateSuccess = session.AddExaminationRecord(record);
				}
				else if (Item is ExaminationRecord existingRecord)
				{
					var idComparer = new IDequilityCompare<int>();
					var attachmentsToDelete = existingRecord.Documents.Except(record.Documents, idComparer).Cast<AttachmentMetaData>();
					var atatachmentsToUpload = record.Documents.Except(existingRecord.Documents, idComparer).Cast<AttachmentMetaData>();
					updateSuccess = session.UpdateExaminationRecord(record, attachmentsToDelete, atatachmentsToUpload);
				}
				else
					updateSuccess = false; 

                if (updateSuccess)
                    RaiseChangesSavedToDBEvent(record);
                else
                    MessageBox.Show("Не получилось обновить запись в базе.", "Ошибка", MessageBoxButton.OK,
                        MessageBoxImage.Error);
            }
        }

        private void RemoveDocumentAttachmentById(int documentID)
		{
			AttachmentMetaData? fileToRemove = AttachmentEditedCollection.FirstOrDefault(d => d.Id == documentID, null);

			if (fileToRemove != null)
			{
				AttachmentEditedCollection.Remove(fileToRemove);
			}
		}

		private void UpdateRecordDisplay()
		{
			if (Item is not ExaminationRecord initialItem)
			{
				DocumentManagementButtonsPanel.Visibility = Visibility.Collapsed;
				return;
			}

			DoctorButton.Content = Doctor?.ToString() ?? "Выбор врача";
			ExaminationTypeButton.Content = ExaminationType?.ToString() ?? "Выбор исследования";
			ClinicButton.Content = Clinic?.ToString() ?? "Выбор мед. учереждения";

			DocumentManagementButtonsPanel.Visibility = Visibility.Visible;

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

		public RecordEditMode Mode
		{
			get => (RecordEditMode)GetValue(ModeProperty);
			set => SetValue(ModeProperty, value);
		}

		public static readonly DependencyProperty ModeProperty =
			DependencyProperty.Register(nameof(Mode), typeof(RecordEditMode), typeof(RecordDisplay), new UIPropertyMetadata(RecordEditMode.Update));

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
				foreach (var fullFileName in opf.FileNames)
				{
					string extention = Path.GetExtension(fullFileName);
					DocumentType documentType;

					switch (extention)
					{
						case ".jpg": documentType = DocumentType.JPEG; break;
						case ".png": documentType = DocumentType.PNG; break;
						case ".pdf": documentType = DocumentType.PDF; break;
						default: MessageBox.Show(fullFileName, "Недопустимый файл", MessageBoxButton.OK, MessageBoxImage.Error); return;
					}

					AttachmentMetaData document = new AttachmentMetaData();
					document.FileName = Path.GetFileName(fullFileName);
					document.DocumentType = documentType;
					document.Data = File.ReadAllBytes(fullFileName);

					AttachmentEditedCollection.Add(document);
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
			ScanDocumentWindow scanDocumentWindow = new ScanDocumentWindow();
			scanDocumentWindow.ShowDialog();

			if (scanDocumentWindow.ScannedImage is BitmapSource scannedImage)
			{				
				using var stream = new MemoryStream();
				BitmapEncoder encoder = new JpegBitmapEncoder();
				encoder.Frames.Add(BitmapFrame.Create(scannedImage));
				encoder.Save(stream);

				AttachmentMetaData document = new AttachmentMetaData();
				document.FileName = scannedImage.GetHashCode().ToString().PadLeft(10, '0').Substring(0, 10) + ".jpg";
				document.DocumentType = DocumentType.JPEG;
				document.Data = stream.ToArray();

				AttachmentEditedCollection.Add(document);
			}

		}

		private void RemoveDocButton_Click(object sender, RoutedEventArgs e)
		{
			if (AttachmentListBox.SelectedItem is AttachmentMetaData document)
			{
				RemoveDocumentAttachmentById(document.Id);
			}
		}

		private void DocumentsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (AttachmentListBox.SelectedItem is AttachmentMetaData)
				RemoveDocButton.Visibility = Visibility.Visible;
			else
				RemoveDocButton.Visibility = Visibility.Hidden;
		}

		private async void DocumentPlaque_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if (AttachmentListBox.SelectedItem is AttachmentMetaData document 
				&& DataContext is Session session
				&& sender is DocumentPlaque plaque
				&& plaque.DataContext is AttachmentMetaData attachment)
			{				
				var imageBytes = await document.LoadData(session);

				if (document.DocumentType == DocumentType.JPEG || document.DocumentType == DocumentType.PNG)
				{									
					BitmapImage image = new BitmapImage();
					image.BeginInit();
					image.StreamSource = new MemoryStream(imageBytes);
					image.EndInit();

					ImageViewWindow imageViewWindow = new ImageViewWindow();
					imageViewWindow.DataContext = image;
					imageViewWindow.ShowDialog();
				}
				else if (document.DocumentType == DocumentType.PDF)
				{
					PdfToImageConverter converter = new PdfToImageConverter();
					await converter.ReadPdfFromBytes(imageBytes);

					PdfViewWindow pdfViewWindow = new PdfViewWindow();
					pdfViewWindow.DataContext = converter.Images.ToList();
					pdfViewWindow.ShowDialog();
				}
			}
		}

		//-----------------------------------------------------------EVENTS----------------------------------------------------------------
		public delegate void ChangesSavedToDBEventHandler(object sender, ChangesSavedToDBEventArgs e);

		public event ChangesSavedToDBEventHandler ChangesSavedToDB;

		protected virtual void RaiseChangesSavedToDBEvent(ExaminationRecord newRecord)
		{
			ChangesSavedToDB?.Invoke(this, new ChangesSavedToDBEventArgs(newRecord));
		}

		//----------------------------------------------------AUXILIARIES----------------------------------------------------------
		private void ResetRecordView()
		{
			ExaminationDatePicker.SelectedDate = null;
			CommentTextBox.Text = "";
		}

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

	public enum RecordEditMode
	{
		Update,
		New
	}
}
