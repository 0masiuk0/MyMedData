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

		public bool AddOrUpdateExaminationRecord(ExaminationRecord record)
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
				if (RecordsDataBase.UpdateOrInsertExaminationRecord(RecordsDatabaseContext, record))
				{
					//this is to trigger ObservableCollection.CollectionChanged and all the bound views to update.
					ExaminationRecords.RemoveAt(recordToUpdateIndex);
					ExaminationRecords.Insert(recordToUpdateIndex, record);

					//Cache Update
					string? doctorName = (record is DoctorExaminationRecord docRec) ? docRec.Doctor?.Name : null;
					DocOrLabExamination docOrLab = record is DoctorExaminationRecord ? DocOrLabExamination.Doc : DocOrLabExamination.Lab;
					EnsureValuesAreCached(record.ExaminationType?.ExaminationTypeTitle, docOrLab,
						doctorName,
						record.Clinic?.Name);

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

		internal void EnsureValuesAreCached(string? examinationTypeTitle, DocOrLabExamination? docOrLab, string? doctorName, string? clinicName)
		{
			//ExaminationType
			if (examinationTypeTitle != null && examinationTypeTitle.Length > 0) 
			{
				if (docOrLab == DocOrLabExamination.Doc)
				{
					if (!DoctorTypesCache.Contains(examinationTypeTitle))
					{
						var docTypesCol = EntitiesDatbaseContext.GetCollection<ExaminationType>(ExaminationType.DoctorTypesDbCollectionName);
						docTypesCol.Insert(new ExaminationType(examinationTypeTitle, ""));
						docTypesCol.EnsureIndex(t => t.ExaminationTypeTitle);
						DoctorTypesCache.Add(examinationTypeTitle);
					}
				}
				else
				{
					//Lab
					if (!LabTestTypesCache.Contains(examinationTypeTitle))
					{
						var labTypesCol = EntitiesDatbaseContext.GetCollection<ExaminationType>(ExaminationType.LabAnalysisTypesDbCollectionName);
						labTypesCol.Insert(new ExaminationType(examinationTypeTitle, ""));
						labTypesCol.EnsureIndex(t => t.ExaminationTypeTitle);
						LabTestTypesCache.Add(examinationTypeTitle);
					}
				}
			}

			//Doctor
			if (docOrLab == DocOrLabExamination.Doc && doctorName != null && doctorName.Length > 0)
			{
				if (!DoctorNameCache.Contains(doctorName))
				{
					var docNamesCol = EntitiesDatbaseContext.GetCollection<Doctor>(Doctor.DbCollectionName);
					docNamesCol.Insert(new Doctor(doctorName, ""));
					docNamesCol.EnsureIndex(d => d.Name);
					DoctorNameCache.Add(doctorName);
				}
			}

			//Clinic			
			if (clinicName != null && clinicName.Length > 0 && !ClinicNameCache.Contains(clinicName))
			{
				var clinicCol = EntitiesDatbaseContext.GetCollection<Clinic>(Clinic.DbCollectionName);
				clinicCol.Insert(new Clinic(clinicName, ""));
				clinicCol.EnsureIndex(cl => cl.Name);
				ClinicNameCache.Add(clinicName);
			}
		}
	}
}
