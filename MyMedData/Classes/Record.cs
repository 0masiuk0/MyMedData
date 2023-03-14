using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;

namespace MyMedData
{
	public abstract class ExaminationRecord
	{
		[BsonId]
		public int Id { get; set; }
		public DateOnly Date { get; set; }	
		public List<string> Documents { get; set; }
		
		[BsonRef(Clinic.DB_COLLECTION_NAME)]
		public Clinic Clinic { get; set; }
		public string Comment { get; set; } = "";
	}
	
	public class DoctorExamination : ExaminationRecord
	{
		[BsonId]
		public int DoctorID;

		[BsonRef(Clinic.DB_COLLECTION_NAME)]
		public DoctorSpecialty DoctorSpecialty;

		public static string DB_COLLECTION_NAME => "DoctorExaminations";
	}

	public class LabExaminationRecord : ExaminationRecord
	{
		public static string DB_COLLECTION_NAME => "LabExaminations";
	}
}
