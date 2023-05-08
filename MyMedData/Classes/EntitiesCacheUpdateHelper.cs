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
			this.session = session;
			CacheDatabaseContext = session.EntitiesDatbaseContext;

			DoctorCache = new(CacheDatabaseContext.GetCollection<Doctor>(Doctor.DbCollectionName)
					.FindAll());

			LabTestTypesCache = new(CacheDatabaseContext.GetCollection<ExaminationType>(ExaminationType.LabAnalysisTypesDbCollectionName)
				.FindAll());

			DoctorTypesCache = new(CacheDatabaseContext.GetCollection<ExaminationType>(ExaminationType.DoctorTypesDbCollectionName)
				.FindAll());

			ClinicCache = new(CacheDatabaseContext.GetCollection<Clinic>(Clinic.DbCollectionName)
				.FindAll());						
		}

		private Session session;
		public ObservableCollection<ExaminationType> LabTestTypesCache;
		public ObservableCollection<Doctor> DoctorCache;
		public ObservableCollection<ExaminationType> DoctorTypesCache;
		public ObservableCollection<Clinic> ClinicCache;
		private LiteDatabase CacheDatabaseContext;

		private LiteDatabase EntitiesDb => session.EntitiesDatbaseContext;
		

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
			ILiteCollection<ExaminationType> collection;
			if (docOrLab == DocOrLabExamination.Doc)
				collection = EntitiesDb.GetCollection<ExaminationType>(ExaminationType.DoctorTypesDbCollectionName);
			else
				collection = EntitiesDb.GetCollection<ExaminationType>(ExaminationType.LabAnalysisTypesDbCollectionName);
			
			collection.Upsert(examinationType);
		}

		public void UpdateDoctorCache(Doctor doctor)
		{
			var collection = EntitiesDb.GetCollection<Doctor>(Doctor.DbCollectionName);
			collection.Upsert(doctor);
		}

		public void UpdateClinicCache(Clinic clinic)
		{
			var collection = EntitiesDb.GetCollection<Clinic>(Clinic.DbCollectionName);
			collection.Upsert(clinic);
		}

	}

	public enum DocOrLabExamination
	{Lab, Doc}
}
