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
			DoctorNameCache = session.DoctorNameCache;
			DoctorTypesCache = session.DoctorTypesCache;
			ClinicNameCache = session.ClinicNameCache;
		}

		private Session session;
		private ObservableCollection<string> LabTestTypesCache;
		private ObservableCollection<string> DoctorNameCache;
		private ObservableCollection<string> DoctorTypesCache;
		private ObservableCollection<string> ClinicNameCache;
		private LiteDatabase CacheDatabaseContext;

		public void UpdateExaminationTypesCache(string newExamTypeTitle, string? oldExamTypeTitle, DocOrLabExamination docOrLab, string? comment = null)
		{
			string collectionName = "";
			ObservableCollection<string>? cache = null;

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

			if (oldExamTypeTitle != null && LabTestTypesCache.Contains(oldExamTypeTitle))
			{
				var col = CacheDatabaseContext.GetCollection<ExaminationType>(collectionName);
				var type = col.FindOne(t => t.ExaminationTypeTitle == oldExamTypeTitle);
				type.ExaminationTypeTitle = newExamTypeTitle;
				type.Comment = comment;
				col.Update(type);
			}
			else
			{
				var col = CacheDatabaseContext.GetCollection<ExaminationType>(collectionName);
				col.Insert(new ExaminationType(newExamTypeTitle, comment ?? ""));
				col.EnsureIndex(t => t.ExaminationTypeTitle);
			}

			if (oldExamTypeTitle != newExamTypeTitle && cache != null)
			{
				if (oldExamTypeTitle != null)
					cache.Remove(oldExamTypeTitle);
				cache.Add(newExamTypeTitle);
			}
		}

		public void UpdateDoctorNameCache(string newDoctorName, string? oldDoctorName, string? comment = null)
		{
			var col = CacheDatabaseContext.GetCollection<Doctor>(Doctor.DbCollectionName);
			if (oldDoctorName != null && DoctorNameCache.Contains(oldDoctorName))
			{
				var doctor = col.FindOne(doc => doc.Name == oldDoctorName);
				doctor.Name = newDoctorName;
				doctor.Comment = comment ?? "";
				col.Update(doctor);
			}
			else
			{
				col.Insert(new Doctor(newDoctorName, comment ?? ""));
				col.EnsureIndex(d => d.Name);
			}

			if (oldDoctorName != newDoctorName)
			{
				if (oldDoctorName != null)
					DoctorNameCache.Remove(oldDoctorName);
				DoctorNameCache.Add(newDoctorName);
			}
		}

		public void UpdateClinicNameCache(string newClinicName, string? oldClinicName, string? comment = null)
		{
			var col = CacheDatabaseContext.GetCollection<Clinic>(Clinic.DbCollectionName);
			if (oldClinicName != null && DoctorNameCache.Contains(oldClinicName))
			{
				var clinic = col.FindOne(clinic => clinic.Name == oldClinicName);
				clinic.Name = newClinicName;
				clinic.Comment = comment ?? "";
				col.Update(clinic);
			}
			else
			{
				col.Insert(new Clinic(newClinicName, comment ?? ""));
				col.EnsureIndex(cl => cl.Name);
			}

			if (oldClinicName != newClinicName)
			{
				if (oldClinicName != null)
					ClinicNameCache.Remove(oldClinicName);
				ClinicNameCache.Add(newClinicName);
			}
		}
	}

	public enum DocOrLabExamination
	{Lab, Doc}
}
