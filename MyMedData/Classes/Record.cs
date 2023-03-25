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

		private List<ArchivedDocument> _documents = new List<ArchivedDocument>();
		[BsonRef(ArchivedDocument.DB_COLLECTION_NAME)]
		public List<ArchivedDocument> Documents
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
				
		private Clinic _clinic;
		[BsonRef(Clinic.DB_COLLECTION_NAME)]
		public Clinic Clinic
		{
			get => _clinic;
			set
			{
				if (_clinic != value)
				{
					_clinic = value;
					OnPropertyChanged(nameof(Clinic));
				}
			}
		}

		private ExaminationType _examinationType;
		[BsonRef(ExaminationType.DB_COLLECTION_NAME)]
		public ExaminationType ExaminationType
		{
			get { return _examinationType; }
			set
			{
				_examinationType = value;
				OnPropertyChanged(nameof(ExaminationType));
			}
		}

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

		public event PropertyChangedEventHandler PropertyChanged;

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
		private Doctor _doctor;
		[BsonRef(Doctor.DB_COLLECTION_NAME)]
		public Doctor Doctor
		{
			get { return _doctor; }
			set
			{
				_doctor = value;
				OnPropertyChanged(nameof(Doctor));
			}
		}	

		public static string DB_COLLECTION_NAME => "DoctorExaminations";
	}

	public class LabExaminationRecord : ExaminationRecord
	{
		public static string DB_COLLECTION_NAME => "LabExaminations";
	}
}
