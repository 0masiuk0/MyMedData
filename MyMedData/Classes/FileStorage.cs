using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace MyMedData
{
	internal class FileStorage
	{
		public FileStorage(Session session) { this.session = session; }

		Session session;

		public bool UploadFilesToStorage(DocumentAttachment document)
		{
			LiteDatabase db = session.RecordsDatabaseContext;
			var storage = db.FileStorage;
			return storage.Upload(document.Id, document.FileName, new MemoryStream(document.TemporaryStoredFile)) != null;
		}

		public bool UploadFilesToStorage(IEnumerable<DocumentAttachment> documents)
		{
			LiteDatabase db = session.RecordsDatabaseContext;
			var storage = db.FileStorage;
			bool success = true;

			foreach (var document in documents)
			{
				success &= storage.Upload(document.Id, document.FileName, new MemoryStream(document.TemporaryStoredFile)) != null;
			}

			return success;
		}

		public bool DeleteFilesFromStorage(DocumentAttachment document)
		{
			LiteDatabase db = session.RecordsDatabaseContext;
			var storage = db.FileStorage;			
			return storage.Delete(document.Id);
		}

		public bool DeleteFilesFromStorage(IEnumerable<DocumentAttachment> documents) 
		{
			LiteDatabase db = session.RecordsDatabaseContext;
			var storage = db.FileStorage;
			bool success = true;
			
			foreach ( var document in documents) 
			{
				success &= storage.Delete(document.Id);
			}

			return success;
		}
	}
}
