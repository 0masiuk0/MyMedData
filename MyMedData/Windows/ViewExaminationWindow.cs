using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MyMedData.Windows
{
    public class ViewExaminationWindow: AddExaminationWindow
    {
		ExaminationRecord examinationRecord;

		public bool ChangesMade { get; private set; }

        public ViewExaminationWindow(Session session, ExaminationRecord record) 
            : base(session, record is DoctorExaminationRecord ? DocOrLabExamination.Doc : DocOrLabExamination.Lab)
		{
			examinationRecord = record;
			theRecordDisplay.ChangesSavedToDB += (o, e) => ChangesMade = true;
			ChangesMade = false;
			theRecordDisplay.Mode = Controls.RecordEditMode.Update;
        }

		protected override void AddUserWindowInstance_Loaded(object sender, RoutedEventArgs e)
		{
			theRecordDisplay.DataContext = Session;
			theRecordDisplay.Item = examinationRecord;			
		}
	}
}
