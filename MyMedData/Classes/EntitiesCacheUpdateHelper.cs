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

		internal void EnsureNewEntitiesCached(ExaminationRecord record)
		{
			if (record is DoctorExaminationRecord docExam)
				EnsureDocExamEnitiesCached(docExam);
			else
				EnsureLabExamEntitiesCached((LabExaminationRecord)record);
		}

		private void EnsureDocExamEnitiesCached(DoctorExaminationRecord docExam)
		{
			if (docExam.Doctor != null && DoctorCache.FirstOrDefault(d => d.Id == docExam.Doctor?.Id, null) == null)
				DoctorCache.Add(docExam.Doctor);

			if (docExam.ExaminationType != null && DoctorTypesCache.FirstOrDefault(dt => dt.Id == docExam.ExaminationType?.Id, null) == null)
				DoctorTypesCache.Add(docExam.ExaminationType);

			if (docExam.Clinic != null && ClinicCache.FirstOrDefault(cl => cl.Id == docExam.Clinic?.Id, null) == null)
				ClinicCache.Add(docExam.Clinic);
		}

		private void EnsureLabExamEntitiesCached(LabExaminationRecord labExam)
		{
			if (labExam.ExaminationType != null && LabTestTypesCache.FirstOrDefault(dt => dt.Id == labExam.ExaminationType?.Id, null) == null)
				LabTestTypesCache.Add(labExam.ExaminationType);

			if (labExam.Clinic != null && ClinicCache.FirstOrDefault(cl => cl.Id == labExam.Clinic?.Id, null) == null)
				ClinicCache.Add(labExam.Clinic);
		}
	}

	public enum DocOrLabExamination
	{Lab, Doc}
}
