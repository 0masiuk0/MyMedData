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
		public ObservableCollection<ExaminationType> LabTestTypesCache { get; private set; }
		public ObservableCollection<Doctor> DoctorNameCache { get; private set; }
		public ObservableCollection<ExaminationType> DoctorTypesCache { get; private set; }
		public ObservableCollection<Clinic> ClinicNameCache { get; private set; }

		public Session(User user, string password)
		{
			ActiveUser = user;
			//Password = password;

			EntitiesCacheUpdateHelper = new EntitiesCacheUpdateHelper(this);
			ExaminationRecords = new ObservableCollection<ExaminationRecord>();

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

			foreach (var docExam in docExaminationRecords.FindAll())
			{
				ExaminationRecords.Add(docExam);
			}
			foreach (var labExam in labExaminationRecords.FindAll())
			{
				ExaminationRecords.Add(labExam);
			}

			//DEBUG
			//foreach (var rec in RecordsDataBase.GenerateSampleRecords(10))
			//	ExaminationRecords.Add(rec);
			//DEBUG

			EntitiesCacheUpdateHelper.LoadAutoCompleteCache();
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
					EntitiesCacheUpdateHelper.EnsureValuesAreCached(record.ExaminationType?.ExaminationTypeTitle, docOrLab,
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

	}
}
