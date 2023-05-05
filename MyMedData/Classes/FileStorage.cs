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
		public FileStorage(Session session) { db = session.RecordsDatabaseContext; }

		public FileStorage(LiteDatabase db) { this.db = db; }

		LiteDatabase db;

		public bool UploadFilesToStorage(DocumentAttachment document)
		{
			var storage = db.FileStorage;
			return storage.Upload(document.Id, document.FileName, new MemoryStream(document.Data)) != null;
		}

		public bool UploadFilesToStorage(IEnumerable<DocumentAttachment> documents)
		{
			var storage = db.FileStorage;
			bool success = true;

			foreach (var document in documents)
			{
				success &= storage.Upload(document.Id, document.FileName, new MemoryStream(document.Data)) != null;
			}

			return success;
		}

		public bool DeleteFilesFromStorage(DocumentAttachment document)
		{
			var storage = db.FileStorage;			
			return storage.Delete(document.Id);
		}

		public bool DeleteFilesFromStorage(IEnumerable<DocumentAttachment> documents) 
		{
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
