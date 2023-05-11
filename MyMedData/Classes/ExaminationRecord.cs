using LiteDB;
using System.Security.Cryptography;
using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.IO;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Reflection.Metadata;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace MyMedData
{
	public abstract class ExaminationRecord : INotifyPropertyChanged
	{
		[BsonId]
		private int _id;
		public int Id
		{
			get => _id;
			set
			{
				if (_id != value)
				{
					_id = value;
					OnPropertyChanged(nameof(Id));
				}
			}
		}

		private DateOnly _date;
		public DateOnly Date
		{
			get => _date;
			set
			{
				if (_date != value)
				{
					_date = value;					
					OnPropertyChanged(nameof(Date));
				}
			}
		}

		private List<DocumentAttachment> _documents = new List<DocumentAttachment>();
		[BsonRef(DocumentAttachment.DbCollectionName)]
		public List<DocumentAttachment> Documents
		{
			get => _documents;
			set
			{
				if (_documents != value)
				{
					_documents = value;
					OnPropertyChanged(nameof(Documents));
				}
			}
		}
				
		private Clinic? _clinic;
		[BsonRef(Clinic.DbCollectionName)]
		public Clinic? Clinic
		{
			get => _clinic;
			set
			{
				if (_clinic != value)
				{
					_clinic = value;
					if (_clinic != null)
						_clinic.PropertyChanged += (o, e) => OnPropertyChanged(nameof(Clinic));

					OnPropertyChanged(nameof(Clinic));
				}
			}
		}

		protected ExaminationType? _examinationType;
		public abstract ExaminationType? ExaminationType { get; set; }
		

		private string _comment = "";
		public string Comment
		{
			get => _comment;
			set
			{
				if (_comment != value)
				{
					_comment = value;
					OnPropertyChanged(nameof(Comment));
				}
			}
		}

		//--------------------------------NON-SERIALIZEABLE FIELDS----------------------------

		[BsonIgnore]
		public abstract string Title { get; }

		//------------------------------------METHODS------------------------------------------
		
		public abstract ExaminationRecord DeepCopy();
		
		public virtual bool IsDataEqual(ExaminationRecord? record, bool checkIdEqulity = false)
		{
			if (record == null) 
				return false;

			if (Date != record.Date)
				return false;

			if (!Enumerable.SequenceEqual(Documents, record.Documents))
				return false;

			if(checkIdEqulity && Id != record.Id) 
				return false;

			if (Clinic?.Name != record.Clinic?.Name || Clinic?.Comment != record.Clinic?.Comment)
				return false;

			if (_examinationType?.ExaminationTypeTitle != record.ExaminationType?.ExaminationTypeTitle || _examinationType?.Comment != record.ExaminationType?.Comment)
				return false;

			if (Comment != record.Comment) 
				return false;

			return true;
		}

		//---------------------------------EVENTS-------------------------------------------

		public event PropertyChangedEventHandler? PropertyChanged;

		protected void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void Documents_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			OnPropertyChanged(nameof(Documents));
		}
	}
	
	public class DoctorExaminationRecord : ExaminationRecord
	{		
		private Doctor? _doctor;
		[BsonRef(Doctor.DbCollectionName)]
		public Doctor? Doctor
		{
			get => _doctor;
			set
			{
				_doctor = value;
				if (_doctor != null)
					_doctor.PropertyChanged += (o, e) => Doctor_PropertyChanged();

				Doctor_PropertyChanged();

				void Doctor_PropertyChanged()
				{
					OnPropertyChanged(nameof(Doctor));
					OnPropertyChanged(nameof(Title));
				}
			}
		}		

		[BsonRef(ExaminationType.DoctorTypesDbCollectionName)]
		public override ExaminationType? ExaminationType
		{
			get => _examinationType;
			set
			{
				_examinationType = value;
				if (_examinationType != null)
					_examinationType.PropertyChanged += (o, e) => ExaminationTypePropertyChange();

				ExaminationTypePropertyChange();

				void ExaminationTypePropertyChange()
				{
					OnPropertyChanged(nameof(ExaminationType));
					OnPropertyChanged(nameof(Title));
				}
			}
		}

		public override bool IsDataEqual(ExaminationRecord? record, bool checkIdEqulity = false)
		{
			if (record is DoctorExaminationRecord docRec)
			{
				return base.IsDataEqual(record, checkIdEqulity) && Doctor?.Name == docRec.Doctor?.Name && Doctor?.Comment == docRec.Doctor?.Comment;
			}
			else
				return false;
		}

		public override ExaminationRecord DeepCopy()
		{
			var copy = new DoctorExaminationRecord 
			{ 
				Id = Id, 
				Date = Date,
				Documents = new List<DocumentAttachment>(Documents),
				ExaminationType = ExaminationType == null ? null : new ExaminationType(ExaminationType.ExaminationTypeTitle, ExaminationType.Comment),
				Clinic = Clinic == null ? null : new Clinic(Clinic.Name, Clinic.Comment),
				Doctor = Doctor == null ? null : new Doctor(Doctor.Name, Doctor.Comment),
				Comment = Comment, 
			};

			return copy;
		}

		[BsonIgnore]
		public override string Title
		{
			get
			{
				if (Doctor != null)
					return $"{ExaminationType} - {Doctor}";
				else
					return ExaminationType?.ExaminationTypeTitle ?? "...";
			}
		}

		public static string DbCollectionName => "DoctorExaminations";
	}

	public class LabExaminationRecord : ExaminationRecord
	{
		[BsonRef(ExaminationType.LabAnalysisTypesDbCollectionName)]
		public override ExaminationType? ExaminationType
		{
			get => _examinationType;
			set
			{
				_examinationType = value;
				if (_examinationType != null)
					_examinationType.PropertyChanged += (o, e) => ExaminationTypePropertyChange();

				ExaminationTypePropertyChange();

				void ExaminationTypePropertyChange()
				{
					OnPropertyChanged(nameof(ExaminationType));
					OnPropertyChanged(nameof(Title));
				}
			}
		}

		public override ExaminationRecord DeepCopy()
		{
			var copy = new LabExaminationRecord
			{
				Id = Id,
				Date = Date,
				Documents = new List<DocumentAttachment>(Documents),
				ExaminationType = ExaminationType == null ? null : new ExaminationType(ExaminationType.ExaminationTypeTitle, ExaminationType.Comment),
				Clinic = Clinic == null ? null : new Clinic(Clinic.Name, Clinic.Comment),
				Comment = Comment,
			};

			return copy;
		}

		public override bool IsDataEqual(ExaminationRecord? record, bool checkIdEqulity = false)
		{
			return record is LabExaminationRecord && base.IsDataEqual(record, checkIdEqulity);
		}

		[BsonIgnore]
		public override string Title => ExaminationType?.ToString() ?? "";

		public static string DbCollectionName => "LabExaminations";
	}
}
