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
			LabTestTypesCache = session.LabTestTypesCache;
			DoctorCache = session.DoctorNameCache;
			DoctorTypesCache = session.DoctorTypesCache;
			ClinicCache = session.ClinicNameCache;
		}

		private Session session;
		private ObservableCollection<ExaminationType> LabTestTypesCache;
		private ObservableCollection<Doctor> DoctorCache;
		private ObservableCollection<ExaminationType> DoctorTypesCache;
		private ObservableCollection<Clinic> ClinicCache;
		private LiteDatabase CacheDatabaseContext;

		public void LoadAutoCompleteCache()
		{
			DoctorCache = new(CacheDatabaseContext.GetCollection<Doctor>(Doctor.DbCollectionName)
					.FindAll());

			LabTestTypesCache = new(CacheDatabaseContext.GetCollection<ExaminationType>(ExaminationType.LabAnalysisTypesDbCollectionName)
				.FindAll());

			DoctorTypesCache = new(CacheDatabaseContext.GetCollection<ExaminationType>(ExaminationType.DoctorTypesDbCollectionName)
				.FindAll());

			ClinicCache = new(CacheDatabaseContext.GetCollection<Clinic>(Clinic.DbCollectionName)
				.FindAll());
		}

		internal void EnsureValuesAreCached(string? examinationTypeTitle, DocOrLabExamination docOrLab, string? doctorName, string? clinicName)
		{
			//ExaminationType
			if (examinationTypeTitle != null && examinationTypeTitle.Length > 0)
			{
				UpdateExaminationTypesCache(examinationTypeTitle, null, docOrLab);
			}

			//Doctor
			if (docOrLab == DocOrLabExamination.Doc && doctorName != null && doctorName.Length > 0)
			{
				UpdateDoctorNameCache(doctorName, null);
			}

			//Clinic			
			if (clinicName != null && clinicName.Length > 0)
			{
				UpdateClinicNameCache(clinicName, null);
			}
		}	

		public void UpdateExaminationTypesCache(string newExamTypeTitle, string? oldExamTypeTitle, DocOrLabExamination docOrLab, string? comment = null)
		{
			if (newExamTypeTitle.Length == 0)
				throw new ArgumentException("Пустое название недопустимо");
			
			string collectionName = "";
			ObservableCollection<ExaminationType>? cache = null;
			ExaminationType examinationType;

			switch (docOrLab)
			{
				case DocOrLabExamination.Lab: 
					collectionName = ExaminationType.LabAnalysisTypesDbCollectionName;
					cache = LabTestTypesCache;
					break;
				case DocOrLabExamination.Doc: 
					collectionName = ExaminationType.DoctorTypesDbCollectionName;
					cache = DoctorTypesCache;
					break;
			}

			oldExamTypeTitle = oldExamTypeTitle ?? newExamTypeTitle;

			if (LabTestTypesCache.FirstOrDefault(t => t?.ExaminationTypeTitle == oldExamTypeTitle, null) is ExaminationType cachedExaminationType)
			{
				var col = CacheDatabaseContext.GetCollection<ExaminationType>(collectionName);				
				cache.Remove(cachedExaminationType);
				examinationType = cachedExaminationType;

				cachedExaminationType.ExaminationTypeTitle = newExamTypeTitle;
				cachedExaminationType.Comment = comment ?? "";
				col.Update(cachedExaminationType);
			}
			else
			{
				var col = CacheDatabaseContext.GetCollection<ExaminationType>(collectionName);
				examinationType = new ExaminationType(newExamTypeTitle, comment ?? "");
				col.Insert(examinationType);
				col.EnsureIndex(t => t.ExaminationTypeTitle);
			}
			
			cache.Add(examinationType);
		}

		public void UpdateDoctorNameCache(string newDoctorName, string? oldDoctorName, string? comment = null)
		{
			var col = CacheDatabaseContext.GetCollection<Doctor>(Doctor.DbCollectionName);
			Doctor doc;

			oldDoctorName = oldDoctorName ?? newDoctorName;

			if (DoctorCache.FirstOrDefault(dc => dc.Name == oldDoctorName, null) is Doctor cachedDoc)
			{
				DoctorCache.Remove(cachedDoc);
				cachedDoc.Name = newDoctorName;
				cachedDoc.Comment = comment ?? "";
				col.Update(cachedDoc);
				doc = cachedDoc;
			}
			else
			{
				doc = new Doctor(newDoctorName, comment ?? "");
				col.Insert(doc);
				col.EnsureIndex(d => d.Name);
			}
				
			DoctorCache.Add(doc);
			
		}

		public void UpdateClinicNameCache(string newClinicName, string? oldClinicName, string? comment = null)
		{
			var col = CacheDatabaseContext.GetCollection<Clinic>(Clinic.DbCollectionName);
			Clinic clinic;

			oldClinicName = oldClinicName ?? newClinicName;

			if (ClinicCache.FirstOrDefault(cl => cl.Name == oldClinicName, null) is Clinic cachedClinic)
			{
				ClinicCache.Remove(cachedClinic);
				cachedClinic.Name = newClinicName;
				cachedClinic.Comment = comment ?? "";
				col.Update(cachedClinic);
				clinic = cachedClinic;
			}
			else
			{
				clinic = new Clinic(newClinicName, comment ?? "");
				col.Insert(clinic);
				col.EnsureIndex(cl => cl.Name);
			}
			
			ClinicCache.Add(clinic);
		}

	}

	public enum DocOrLabExamination
	{Lab, Doc}
}
