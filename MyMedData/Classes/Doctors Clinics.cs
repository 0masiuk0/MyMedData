using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMedData
{
#pragma warning disable CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
	public class Doctor
	{
		[BsonId]
		public int DoctorID { get; set;}
		public string Name { get; set; }
		public string Comment { get; set; }

		public const string DB_COLLECTION_NAME = "Doctors";
	}

	public class Clinic
	{
		[BsonId]
		public int ClinicID { get; set; }
		public string Name { get; set; }
		public string Comment { get; set; }

		public const string DB_COLLECTION_NAME = "Clinics";
	}

	public class DoctorSpecialty
	{
		[BsonId]
		public string Specialty { get; set; }
		public string Comment { get; set; }

		public const string DB_COLLECTION_NAME = "Specialties";
	}
#pragma warning restore CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
}
