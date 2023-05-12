using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MyMedData
{
	internal class FileStorage
	{
		public FileStorage(Session session) { db = session.RecordsDatabaseContext; storage = db.FileStorage; }

		public FileStorage(LiteDatabase db) { this.db = db; storage = db.FileStorage; }

		readonly LiteDatabase db;

		readonly ILiteStorage<string> storage;

		public bool UploadFileToStorage(AttachmentMetaData attachment)
		{
			if (attachment == null)
				throw new ArgumentNullException("Попытка загурзить в базу приложение в которое не загружен файл.");

			return storage.Upload(attachment.Id.ToString(), attachment.CustomName, new MemoryStream(attachment.Data)) != null;
		}			

		public bool DeleteFileFromStorage(AttachmentMetaData document)
		{
			return storage.Delete(document.Id.ToString());
		}

		internal byte[] GetFileBytes(int id)
		{
			var stream = new MemoryStream();
			storage.Download(id.ToString(), stream);
			return stream.ToArray();
		}
	}
}
