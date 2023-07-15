using LiteDB;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Xml.Linq;

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
				EntitiesCacheUpdateHelper.EnsureNewEntitiesCached(record);
				return true;
			}

			return false;
		}

		public bool AddExaminationRecord(ExaminationRecord record)
		{
			if (RecordsDataBase.InsertRecord(this, ref record))
			{
				EntitiesCacheUpdateHelper.EnsureNewEntitiesCached(record);
				ExaminationRecords.Add(record);
				return true;
			}

			return false;
		}

		internal void UpdateMedicalEntityId(IMedicalEntity oldValue, IMedicalEntity medicalEntity, EntityType entityType)
		{
			try
			{
				switch (entityType)
				{
					case EntityType.LabExaminaionType:
						if (LabTestTypesCache.FirstOrDefault(lt => lt.Id == oldValue.Id, null) is ExaminationType oldLabTestType
							&& medicalEntity is ExaminationType newLabExType)
						{
							RecordsDataBase.SubstituteLabExaminationType(oldLabTestType, newLabExType, RecordsDatabaseContext);

							var affectedRecs = ExaminationRecords.Where(rc => rc is LabExaminationRecord)
								.Cast<LabExaminationRecord>()
								.Where(labRec => labRec.ExaminationType?.Id == oldLabTestType.Id);

							foreach (var rec in affectedRecs)
							{
								rec.ExaminationType = newLabExType;
							}

							LabTestTypesCache.Remove(oldLabTestType);
							LabTestTypesCache.Insert(0, newLabExType);
						}
						break;
					case EntityType.DoctorType:
						if (DoctorTypesCache.FirstOrDefault(dt => dt.Id == oldValue.Id, null) is ExaminationType oldDocType
							&& medicalEntity is ExaminationType newDocType)
						{
							RecordsDataBase.SubstituteDocType(oldDocType, newDocType, RecordsDatabaseContext);

							var affectedRecs = ExaminationRecords.Where(rc => rc is DoctorExaminationRecord)
								.Cast<DoctorExaminationRecord>()
								.Where(docRec => docRec.ExaminationType?.Id == oldDocType.Id);

							foreach (var rec in affectedRecs)
							{
								rec.ExaminationType = newDocType;
							}

							DoctorTypesCache.Remove(oldDocType);
							DoctorTypesCache.Insert(0, newDocType);
						}
						break;
					case EntityType.Doctor:
						if (DoctorCache.FirstOrDefault(doc => doc.Id == oldValue.Id, null) is Doctor oldDoc
							&& medicalEntity is Doctor newDoctor)
						{
							RecordsDataBase.SubstituteDoctor(oldDoc, newDoctor, RecordsDatabaseContext);

							var affectedRecs = ExaminationRecords.Where(rc => rc is DoctorExaminationRecord)
								.Cast<DoctorExaminationRecord>()
								.Where(docRec => docRec.Doctor?.Id == oldDoc.Id);
							foreach (var rec in affectedRecs)
							{
								rec.Doctor = newDoctor;
							}

							DoctorCache.Remove(oldDoc);
							DoctorCache.Insert(0, newDoctor);
						}
						break;
					case EntityType.Clinic:
						if (ClinicCache.FirstOrDefault(clinic => clinic.Id == oldValue.Id, null) is Clinic oldClinic
							&& medicalEntity is Clinic newClinic)
						{
							RecordsDataBase.SubstituteClinic(oldClinic, newClinic, RecordsDatabaseContext);

							var affectedRecs = ExaminationRecords.Where(rec => rec.Clinic?.Id == oldClinic.Id);
							foreach (var rec in affectedRecs)
							{
								rec.Clinic = newClinic;
							}

							ClinicCache.Remove(oldClinic);
							ClinicCache.Insert(0, newClinic);
						}
						break;
					default: throw new Exception("Impossible exeption 9");
				}
			}
			catch(DbOperationException dbEx)
			{
				MessageBox.Show(dbEx.Message, "Возникла ошибка.", MessageBoxButton.OK, MessageBoxImage.Error);
				throw new Exception(dbEx.Message);
			}
			catch(InvalidDbIdException idEx)
			{
				MessageBox.Show($"ID {idEx.ID} уже существует или недопустимый.", "Недопустимый ID!", MessageBoxButton.OK, MessageBoxImage.Error);
				throw new Exception(idEx.Message);
			}
		}

		internal void UpdateMedicalEntityComment(IMedicalEntity oldValue, string comment, EntityType entityType)
		{
			switch (entityType)
			{
				case EntityType.LabExaminaionType:
					if (LabTestTypesCache.FirstOrDefault(lt => lt.Id == oldValue.Id, null) is ExaminationType labTestType)
					{
						RecordsDataBase.UpsertLabExaminationType(labTestType, RecordsDatabaseContext);

						var affectedRecs = ExaminationRecords.Where(rc => rc is LabExaminationRecord)
							.Cast<LabExaminationRecord>()
							.Where(labRec => labRec.ExaminationType?.Id == labTestType.Id);
						foreach (var rec in affectedRecs)
						{
							rec.ExaminationType.Comment = comment;
						}
						
						labTestType.Comment = comment;						
					}
					break;
				case EntityType.DoctorType:
					if (DoctorTypesCache.FirstOrDefault(dt => dt.Id == oldValue.Id, null) is ExaminationType docType)
					{
						docType.Comment = comment;
						RecordsDataBase.UpsertDoctorType(docType, RecordsDatabaseContext);
						var affectedRecs = ExaminationRecords.Where(rc => rc is DoctorExaminationRecord)
							.Cast<DoctorExaminationRecord>()
							.Where(docRec => docRec.ExaminationType?.Id == docType.Id);
						foreach (var rec in affectedRecs)
						{
							rec.ExaminationType.Comment = comment;
						}						
					}
					break;
				case EntityType.Doctor:
					if (DoctorCache.FirstOrDefault(doc => doc.Id == oldValue.Id, null) is Doctor doc)
					{
						doc.Comment = comment;
						RecordsDataBase.UpsertDoctor(doc, RecordsDatabaseContext);
						var affectedRecs = ExaminationRecords.Where(rc => rc is DoctorExaminationRecord)
							.Cast<DoctorExaminationRecord>()
							.Where(docRec => docRec.Doctor?.Id == doc.Id);
						foreach (var rec in affectedRecs)
						{
							rec.Doctor.Comment = comment;
						}
					}
					break;
				case EntityType.Clinic:
					if (ClinicCache.FirstOrDefault(clinic => clinic.Id == oldValue.Id, null) is Clinic clinic)
					{
						clinic.Comment = comment;
						RecordsDataBase.UpsertClinic(clinic, RecordsDatabaseContext);
						var affectedRecs = ExaminationRecords.Where(rec => rec.Clinic?.Id == clinic.Id);
						foreach (var rec in affectedRecs)
						{
							rec.Clinic.Comment = comment;
						}
					}
					break;
				default: throw new Exception("Impossible exeption 8");
			}
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

		internal bool DeleteClinic(Clinic clinic)
		{
			if (ClinicCache.FirstOrDefault(cl => cl.Id == clinic.Id, null) is Clinic cachedClinic)
			{
				if (RecordsDataBase.DeleteClinic(RecordsDatabaseContext, cachedClinic))
				{
					ClinicCache.Remove(cachedClinic);
					return true;
				}
			}

			return false;
		}

		internal bool DeleteLabTestType(ExaminationType exType)
		{
			if (LabTestTypesCache.FirstOrDefault(et => et.Id == exType.Id, null) is ExaminationType cachedExType)
			{
				if (RecordsDataBase.DeleteLabTestType(RecordsDatabaseContext, cachedExType))
				{
					LabTestTypesCache.Remove(cachedExType);
					return true;
				}
			}

			return false;
		}

		internal bool DeleteDoctorType(ExaminationType exType)
		{
			if (DoctorTypesCache.FirstOrDefault(et => et.Id == exType.Id, null) is ExaminationType cachedExType)
			{
				if (RecordsDataBase.DeleteDoctorType(RecordsDatabaseContext, cachedExType))
				{
					DoctorTypesCache.Remove(cachedExType);
					return true;
				}
			}

			return false;
		}

		internal bool DeleteDoctor(Doctor doctor)
		{
			if (DoctorCache.FirstOrDefault(d => d.Id == doctor.Id, null) is Doctor cachedDoc)
			{
				if (RecordsDataBase.DeleteDoctor(RecordsDatabaseContext, cachedDoc))
				{
					DoctorCache.Remove(cachedDoc);
					return true;
				}
			}

			return false;
		}

		public void Dispose()
		{
			RecordsDatabaseContext.Dispose();
		}		
	}
}
