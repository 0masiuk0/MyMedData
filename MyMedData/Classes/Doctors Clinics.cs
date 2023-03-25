using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MyMedData
{
#pragma warning disable CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
	public class Doctor
	{
		[BsonId]
		public string Name { get; set; }
		public string Comment { get; set; }

		public Doctor() { }

		public Doctor(string name, string comment="")
		{
			Name = name;
			Comment = comment;
		}

		public override string ToString() => Name;

		public const string DB_COLLECTION_NAME = "Doctors";
	}

	public class Clinic
	{
		[BsonId]
		public string Name { get; set; }
		public string Comment { get; set; }

		public Clinic() { }

		public Clinic(string name, string comment="")
		{
			Name = name;
			Comment = comment;
		}

		public override string ToString() => Name;		

		public const string DB_COLLECTION_NAME = "Clinics";
	}

	public class ExaminationType
	{
		[BsonId]
		public string ExminationTypeTitle { get; set; }
		public string Comment { get; set; }

		public ExaminationType() { }

		public ExaminationType(string exminationTypeTitle, string comment = "")
		{
			ExminationTypeTitle = exminationTypeTitle;
			Comment = comment;
		}

		public override string ToString() => ExminationTypeTitle;

		public const string DOCTOR_TYPES_DB_COLLECTION_NAME = "Specialties";
		public const string ANALYSIS_TYPES_DB_COLLECTION_NAME = "LabTests";
	}
#pragma warning restore CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
}
