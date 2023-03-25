using LiteDB;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MyMedData
{
	public class Session : IDisposable
	{
		public readonly User ActiveUser;
		public readonly LiteDatabase DocumentsDatabaseContext;
		//public string Password { private get; set; }
				
		public ObservableCollection<ExaminationRecord> ExaminationRecords = new();
		public List<string> DoctorNameCache = new List<string>();
		public List<string> LabTestTypes = new List<string>();
		public List<string> DoctorTypes = new List<string>();
		public List<string> ClinicNameCache = new List<string>();


		public Session(User user, string password)
		{
			ActiveUser = user;
			//Password = password;
			DocumentsDatabaseContext = new LiteDatabase(RecordsDataBase.GetConnectionString(user, password));
			ReadDataBase();	
		}

		private void ReadDataBase()
		{
			var docExaminationRecords = DocumentsDatabaseContext
				.GetCollection<DoctorExaminationRecord>(DoctorExaminationRecord.DB_COLLECTION_NAME);
			var labExaminationRecords = DocumentsDatabaseContext
				.GetCollection<LabExaminationRecord>(LabExaminationRecord.DB_COLLECTION_NAME);

			foreach(var docExam in docExaminationRecords.FindAll())
			{
				ExaminationRecords.Add(docExam);
			}
			foreach(var labExam in labExaminationRecords.FindAll())
			{
				ExaminationRecords.Add(labExam);
			}

			if (ActiveUser.RunsOwnDoctorsCollection)
			{
				CacheAutoComplete(DocumentsDatabaseContext);
			}
			else
			{
				string? dbFile = UsersDataBase.GetUsersDbFileNameFromConfig();
				if (dbFile == null)
					throw new Exception("Не найдено конфигурации файла общей базы данных.");

				using (var db = new LiteDatabase(dbFile))
				{
					CacheAutoComplete(db);
				}
			}
		}

		private void CacheAutoComplete(LiteDatabase db)
		{
			DoctorNameCache = db.GetCollection<Doctor>(Doctor.DB_COLLECTION_NAME)
					.FindAll().Select(doctor => doctor.Name).ToList();

			LabTestTypes = db.GetCollection<ExaminationType>(ExaminationType.ANALYSIS_TYPES_DB_COLLECTION_NAME)
				.FindAll().Select(type => type.ExminationTypeTitle).ToList();

			DoctorTypes = db.GetCollection<ExaminationType>(ExaminationType.DOCTOR_TYPES_DB_COLLECTION_NAME)
				.FindAll().Select(type => type.ExminationTypeTitle).ToList();

			ClinicNameCache = db.GetCollection<Clinic>(Clinic.DB_COLLECTION_NAME)
				.FindAll().Select(clinic => clinic.Name).ToList();
		}		

		public void Dispose()
		{
			DocumentsDatabaseContext.Dispose();			
		}
	}
}
