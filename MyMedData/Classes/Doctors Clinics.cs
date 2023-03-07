using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMedData.Classes
{
	public class Doctor
	{
		[BsonId]
		public int DoctorID { get; set;}
		public string Name { get; set; }
		public string Comment { get; set; }

		public static string DB_COLLECTION_NAME = "Doctors";
	}

	public class Clinic
	{
		[BsonId]
		public int ClinicID { get; set; }
		public string Name { get; set; }
		public string Comment { get; set; }

		public static string DB_COLLECTION_NAME = "Clinics";
	}

	public class DoctorSpecialty
	{
		[BsonId]
		public string Specialty { get; set; }

		public static string DB_COLLECTION_NAME = "Specialties";
	}
}
