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
		public List<int> DocumentIDs { get; set; } = new();
		public int? ClinicId { get; set; }
		public string Comment { get; set; } = "";
	}
	
	public class DoctorExamination : ExaminationRecord
	{
		public int? DoctorID;
		public int? DoctorSpecialistID;

		public static string DB_COLLECTION_NAME => "DoctorExaminations";
	}

	public class LabExaminationRecord : ExaminationRecord
	{
		public static string DB_COLLECTION_NAME => "LabExaminations";
	}
}
