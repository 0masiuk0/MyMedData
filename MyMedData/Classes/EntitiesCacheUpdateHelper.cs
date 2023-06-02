using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;

namespace MyMedData
{
	public class EntitiesCacheUpdateHelper
	{
		public EntitiesCacheUpdateHelper(Session session)
		{
			var EntitiesDb = session.RecordsDatabaseContext;
			this.session = session;

			DoctorCache = new(EntitiesDb.GetCollection<Doctor>(Doctor.DbCollectionName)
					.FindAll());

			LabTestTypesCache = new(EntitiesDb.GetCollection<ExaminationType>(ExaminationType.LabAnalysisTypesDbCollectionName)
				.FindAll());

			DoctorTypesCache = new(EntitiesDb.GetCollection<ExaminationType>(ExaminationType.DoctorTypesDbCollectionName)
				.FindAll());

			ClinicCache = new(EntitiesDb.GetCollection<Clinic>(Clinic.DbCollectionName)
				.FindAll());						
		}

		private Session session;
		public ObservableCollection<ExaminationType> LabTestTypesCache;
		public ObservableCollection<Doctor> DoctorCache;
		public ObservableCollection<ExaminationType> DoctorTypesCache;
		public ObservableCollection<Clinic> ClinicCache;

		internal void EnsureValuesAreCached(ExaminationRecord record)
		{
			if (record is DoctorExaminationRecord docRecord)
				EnsureValuesAreCached(DocOrLabExamination.Doc, docRecord.ExaminationType, docRecord.Clinic, docRecord.Doctor);
			else
				EnsureValuesAreCached(DocOrLabExamination.Lab, record.ExaminationType, record.Clinic, null);
		}

		internal void EnsureValuesAreCached(DocOrLabExamination docOrLab,
			ExaminationType? examinationType,
			Clinic? clinic,
			Doctor? doctor)
		{
			if (examinationType != null)
				UpdateExaminationTypesCache(docOrLab, examinationType);

			if (clinic != null)
				UpdateClinicCache(clinic);

			if (doctor != null)
				if (docOrLab == DocOrLabExamination.Doc )
					UpdateDoctorCache(doctor);
		}	

		public void UpdateExaminationTypesCache(DocOrLabExamination docOrLab, ExaminationType examinationType)
		{
			var EntitiesDb = session.RecordsDatabaseContext;
			ILiteCollection<ExaminationType> collection;
			if (docOrLab == DocOrLabExamination.Doc)
				collection = EntitiesDb.GetCollection<ExaminationType>(ExaminationType.DoctorTypesDbCollectionName);
			else
				collection = EntitiesDb.GetCollection<ExaminationType>(ExaminationType.LabAnalysisTypesDbCollectionName);
			
			collection.Upsert(examinationType);
		}

		public void UpdateDoctorCache(Doctor doctor)
		{
			var EntitiesDb = session.RecordsDatabaseContext;
			var collection = EntitiesDb.GetCollection<Doctor>(Doctor.DbCollectionName);
			collection.Upsert(doctor);
		}

		public void UpdateClinicCache(Clinic clinic)
		{
			var EntitiesDb = session.RecordsDatabaseContext;
			var collection = EntitiesDb.GetCollection<Clinic>(Clinic.DbCollectionName);
			collection.Upsert(clinic);
		}

	}

	public enum DocOrLabExamination
	{Lab, Doc}
}
