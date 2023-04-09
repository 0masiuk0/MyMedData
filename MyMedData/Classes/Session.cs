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
				
		public ObservableCollection<ExaminationRecord> ExaminationRecords { get; private set; }
		public ObservableCollection<string> LabTestTypesCache { get;private set; }	 
		public ObservableCollection<string> DoctorNameCache  { get; private set;} 	 
		public ObservableCollection<string> DoctorTypesCache { get; private set;}	 
		public ObservableCollection<string> ClinicNameCache  { get; private set;}	 


		public Session(User user, string password)
		{
			ActiveUser = user;
			//Password = password;
			
			ExaminationRecords = new();
			DoctorNameCache = new ObservableCollection<string>();
			LabTestTypesCache = new ObservableCollection<string>();
			DoctorTypesCache = new ObservableCollection<string>();
			ClinicNameCache = new ObservableCollection<string>();

			DocumentsDatabaseContext = new LiteDatabase(RecordsDataBase.GetConnectionString(user, password));
			ReadDataBase();
		}

		private void ReadDataBase()
		{
			var docExaminationRecords = DocumentsDatabaseContext
				.GetCollection<DoctorExaminationRecord>(DoctorExaminationRecord.DbCollectionName);
			var labExaminationRecords = DocumentsDatabaseContext
				.GetCollection<LabExaminationRecord>(LabExaminationRecord.DbCollectionName);

			foreach(var docExam in docExaminationRecords.FindAll())
			{
				ExaminationRecords.Add(docExam);
			}
			foreach(var labExam in labExaminationRecords.FindAll())
			{
				ExaminationRecords.Add(labExam);
			}

//-----------------------------------DEBUG------------------------------------------------------------------------
			foreach(var rec in RecordsDataBase.GenerateSampleRecords(10))
			{ ExaminationRecords.Add(rec);}
//----------------------------------DEBUG------------------------------------------------------------------------

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
			DoctorNameCache = new (db.GetCollection<Doctor>(Doctor.DbCollectionName)
					.FindAll().Select(doctor => doctor.Name));

			LabTestTypesCache = new(db.GetCollection<ExaminationType>(ExaminationType.AnalysisTypesDbCollectionName)
				.FindAll().Select(type => type.ExminationTypeTitle).ToList());

			DoctorTypesCache = new(db.GetCollection<ExaminationType>(ExaminationType.DoctorTypesDbCollectionName)
				.FindAll().Select(type => type.ExminationTypeTitle).ToList());

			ClinicNameCache = new(db.GetCollection<Clinic>(Clinic.DbCollectionName)
				.FindAll().Select(clinic => clinic.Name).ToList());
		}		

		public void Dispose()
		{
			DocumentsDatabaseContext.Dispose();			
		}
	}
}
