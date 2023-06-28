using LiteDB;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace MyMedData
{
	// DB password for users with no password
	// P0keCeCuEd6oDX9K

	public class Session : IDisposable
	{
		public readonly User ActiveUser;
		public readonly LiteDatabase RecordsDatabaseContext;

		public readonly bool OccupiesUsersDb;

		public EntitiesCacheUpdateHelper EntitiesCacheUpdateHelper { get; private set; }
		public ObservableCollection<ExaminationRecord> ExaminationRecords { get; private set; }
		public ObservableCollection<ExaminationType> LabTestTypesCache { get; private set; } = new();
		public ObservableCollection<Doctor> DoctorCache { get; private set; } = new();
		public ObservableCollection<ExaminationType> DoctorTypesCache { get; private set; } = new();
		public ObservableCollection<Clinic> ClinicCache { get; private set; } = new();

		public Session(User user, string password)
		{
			ActiveUser = user;
			
			ExaminationRecords = new ObservableCollection<ExaminationRecord>();			

			//Getting DB context
			RecordsDatabaseContext = new LiteDatabase(RecordsDataBase.GetConnectionString(user, password));
			
			//Building medical entities cashe
			EntitiesCacheUpdateHelper = new EntitiesCacheUpdateHelper(this);
			LabTestTypesCache = EntitiesCacheUpdateHelper.LabTestTypesCache;
			DoctorCache = EntitiesCacheUpdateHelper.DoctorCache;
			DoctorTypesCache = EntitiesCacheUpdateHelper.DoctorTypesCache;
			ClinicCache = EntitiesCacheUpdateHelper.ClinicCache;

			//Reading records
			var docExaminationRecords = RecordsDatabaseContext
				.GetCollection<DoctorExaminationRecord>(DoctorExaminationRecord.DbCollectionName)
				.Include(x => x.Doctor)
				.Include(x => x.Clinic)
				.Include(x => x.ExaminationType)
				.Include(x => x.Documents);
			
			var labExaminationRecords = RecordsDatabaseContext
				.GetCollection<LabExaminationRecord>(LabExaminationRecord.DbCollectionName)
				.Include(x => x.Clinic)
				.Include(x => x.ExaminationType)
				.Include(x => x.Documents);  

			foreach (var docExam in docExaminationRecords.FindAll())
			{
				ExaminationRecords.Add(docExam);
			}
			foreach (var labExam in labExaminationRecords.FindAll())
			{
				ExaminationRecords.Add(labExam);
			}
		}

		public bool UpdateExaminationRecord(ExaminationRecord record, IEnumerable<AttachmentMetaData> atatachmentsToDelete, IEnumerable<AttachmentMetaData> atatachmentsToUpload)
		{
			int recordToUpdateIndex = -1;
			for (int i = 0; i < ExaminationRecords.Count; i++)
			{
				if (record.Id == ExaminationRecords[i].Id && record.GetType() == ExaminationRecords[i].GetType())
				{
					recordToUpdateIndex = i;
					break;
				}
			}

			if (recordToUpdateIndex == -1)
				throw new ArgumentException("Не удалось найти запись в кэше");

			if (RecordsDataBase.UpdateRecord(this, ref record, atatachmentsToDelete, atatachmentsToUpload))
			{
				//ExaminationRecords.RemoveAt(recordToUpdateIndex);
				//ExaminationRecords.Insert(recordToUpdateIndex, record);
				ExaminationRecords[recordToUpdateIndex] = record;
				return true;
			}

			return false;
		}

		public bool AddExaminationRecord(ExaminationRecord record)
		{
			if (RecordsDataBase.InsertRecord(this, ref record))
			{
				ExaminationRecords.Add(record);
				return true;
			}

			return false;
		}

		public bool DeleteRecord(ExaminationRecord record)
		{
			if (RecordsDataBase.DeleteRecord(RecordsDatabaseContext, record))
			{
				ExaminationRecords.Remove(record);
				return true;
			}
			
			MessageBox.Show("Не удалось удалит запись.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			return false;
		}
	
		public void Dispose()
		{
			RecordsDatabaseContext.Dispose();
		}

	}
}
