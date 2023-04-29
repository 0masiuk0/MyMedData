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
		public readonly LiteDatabase RecordsDatabaseContext;
		public readonly LiteDatabase EntitiesDatbaseContext;
		//public string Password { private get; set; }
				
		public EntitiesCacheUpdateHelper EntitiesCacheUpdateHelper { get; private set; }
		public ObservableCollection<ExaminationRecord> ExaminationRecords { get; private set; }
		public ObservableCollection<string> LabTestTypesCache { get;private set; }	 
		public ObservableCollection<string> DoctorNameCache  { get; private set;} 	 
		public ObservableCollection<string> DoctorTypesCache { get; private set;}	 
		public ObservableCollection<string> ClinicNameCache  { get; private set;}	 
		
		public Session(User user, string password)
		{
			ActiveUser = user;
			//Password = password;

			EntitiesCacheUpdateHelper = new EntitiesCacheUpdateHelper(this);
			ExaminationRecords = new ObservableCollection<ExaminationRecord>();
			DoctorNameCache = new ObservableCollection<string>();
			LabTestTypesCache = new ObservableCollection<string>();
			DoctorTypesCache = new ObservableCollection<string>();
			ClinicNameCache = new ObservableCollection<string>();

			RecordsDatabaseContext = new LiteDatabase(RecordsDataBase.GetConnectionString(user, password));

			if (ActiveUser.RunsOwnDoctorsCollection)
			{
				EntitiesDatbaseContext = RecordsDatabaseContext;
			}
			else
			{
				string? dbFile = UsersDataBase.GetUsersDbFileNameFromConfig();
				if (dbFile == null)
					throw new Exception("Не найдено конфигурации файла общей базы данных.");

				EntitiesDatbaseContext = new LiteDatabase(dbFile);
			}

			ReadDataBase();
		}

		private void ReadDataBase()
		{
			var docExaminationRecords = RecordsDatabaseContext
				.GetCollection<DoctorExaminationRecord>(DoctorExaminationRecord.DbCollectionName);
			var labExaminationRecords = RecordsDatabaseContext
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
		
			CacheAutoComplete(EntitiesDatbaseContext);
		}

		private void CacheAutoComplete(LiteDatabase db)
		{
			DoctorNameCache = new (db.GetCollection<Doctor>(Doctor.DbCollectionName)
					.FindAll().Select(doctor => doctor.Name));

			LabTestTypesCache = new(db.GetCollection<ExaminationType>(ExaminationType.LabAnalysisTypesDbCollectionName)
				.FindAll().Select(type => type.ExaminationTypeTitle));

			DoctorTypesCache = new(db.GetCollection<ExaminationType>(ExaminationType.DoctorTypesDbCollectionName)
				.FindAll().Select(type => type.ExaminationTypeTitle));

			ClinicNameCache = new(db.GetCollection<Clinic>(Clinic.DbCollectionName)
				.FindAll().Select(clinic => clinic.Name));
		}

		public bool UpdateRecord(ExaminationRecord record)
		{
			int recordToUpdateIndex = -1;
			for (int i = 0; i < ExaminationRecords.Count; i++)
			{
				if (record.Id == ExaminationRecords[i].Id)
				{
					recordToUpdateIndex = i; break;
				}
			}

			if (recordToUpdateIndex >= 0)
			{
				if (RecordsDataBase.UpdateRecord(RecordsDatabaseContext, record))
				{
					ExaminationRecords.RemoveAt(recordToUpdateIndex);
					ExaminationRecords.Insert(recordToUpdateIndex, record);
					return true;
				}
			}

			return false;
		}

		public void Dispose()
		{
			if (!object.ReferenceEquals(RecordsDatabaseContext, EntitiesDatbaseContext))
				EntitiesDatbaseContext.Dispose();
			RecordsDatabaseContext.Dispose();
		}
	}
}
