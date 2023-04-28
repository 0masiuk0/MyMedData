using LiteDB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MyMedData
{
#pragma warning disable CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
	public class Doctor : INotifyPropertyChanged
	{
		private string _name;
		[BsonId]
		public string Name
		{
			get => _name;
			set
			{
				if (_name != value)
				{
					_name = value;
					OnPropertyChanged(nameof(Name));
				}
			}
		}

		private string _comment;
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

		public Doctor() { }

		public Doctor(string name, string comment = "")
		{
			Name = name;
			Comment = comment;
		}

		public override string ToString() => Name;

		public const string DbCollectionName = "Doctors";

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	public class Clinic : INotifyPropertyChanged
	{
		private string _name;
		[BsonId]
		public string Name
		{
			get => _name;
			set
			{
				if (_name != value)
				{
					_name = value;
					OnPropertyChanged(nameof(Name));
				}
			}
		}

		private string _comment;
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

		public Clinic() { }

		public Clinic(string name, string comment = "")
		{
			Name = name;
			Comment = comment;
		}

		public override string ToString() => Name;

		public const string DbCollectionName = "Clinics";

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	public class ExaminationType : INotifyPropertyChanged
	{
		private string _examinationTypeTitle;
		[BsonId]
		public string ExaminationTypeTitle
		{
			get => _examinationTypeTitle;
			set
			{
				if (_examinationTypeTitle != value)
				{
					_examinationTypeTitle = value;
					OnPropertyChanged(nameof(ExaminationTypeTitle));
				}
			}
		}

		private string _comment;
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

		public ExaminationType() { }

		public ExaminationType(string examinationTypeTitle, string comment = "")
		{
			ExaminationTypeTitle = examinationTypeTitle;
			Comment = comment;
		}

		public override string ToString() => ExaminationTypeTitle;

		public const string DoctorTypesDbCollectionName = "Specialties";
		public const string AnalysisTypesDbCollectionName = "LabTests";

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
#pragma warning restore CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
}
