using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Storage.Streams;

namespace MyMedData
{
	internal class FileStorage: IDisposable
	{
		LiteDatabase db;

		public FileStorage(Session session) { db = session.RecordsDatabaseContext; storage = db.FileStorage; }

		public FileStorage(LiteDatabase db) { this.db = db; storage = db.FileStorage; } 		

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
			using var stream = new MemoryStream();
			storage.Download(id.ToString(), stream);
			return stream.ToArray();
		}

		internal IInputStream GetFileAsStream(int id)
		{
			return storage.OpenRead(id.ToString()).AsInputStream();
		}

		public void Dispose()
		{
			db = null;
		}
	}
}
