using LiteDB;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Transactions;

namespace MyMedData
{
	public static class RecordsDataBase
	{
		public static bool CreateUserDocumnetDb(User user, string password, DbCreationOptions options = DbCreationOptions.Ask)
		{
			if (File.Exists(user.DatabaseFile))
			{
				if (!user.CheckPassword(password))
					throw new InvalidDataException($"Попытка создать файл базы данных с ключом шифрования, отличным от пароля пользователя {user.Name}");

				//есть файл
				if (options == DbCreationOptions.UseExistingIfFound)
				{
					if (!RecordsDataBase.FastCheckRecordDbValidity(user.DatabaseFile, password))
					{
						if (MessageBox.Show("Указанная база не соответствует формату. Отформатировать с очисткой?", "Ошибка!",
							MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
						{
							return CreateUserDocumnetDb(user, password, DbCreationOptions.Override);
						}
						else
						{
							return false;
						}
					}
					else
					{
						return true;
					}
				}
				else if (options == DbCreationOptions.Override)
				{
					try
					{
						File.Delete(user.DatabaseFile);
						return CreateFreshRecordDb(user.DatabaseFile, password);
					}
					catch
					{
						if (MessageBox.Show("Не удается удалить существующий файл. Повторить попытку?", "Ошибка!",
							MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
						{
							return CreateUserDocumnetDb(user, password, options);
						}
						else
						{
							return false;
						}
					}
				}
				else
				{
					var answer = MessageBox.Show("Найден существующий файл.\n Да - использовать его.\n Нет - файл будет очищен.", "Ошибка!",
							MessageBoxButton.YesNoCancel, MessageBoxImage.Error);
					if (answer == MessageBoxResult.Yes)
					{
						return CreateUserDocumnetDb(user, password, DbCreationOptions.UseExistingIfFound);
					}
					else if (answer == MessageBoxResult.No)
					{
						return CreateUserDocumnetDb(user, password, DbCreationOptions.Override);
					}
					else
					{
						return false;
					}
				}
			}
			else
			{
				return CreateFreshRecordDb(user.DatabaseFile, password);
			}
		}

		public static bool FastCheckRecordDbValidity(string filename, string password)
		{
			try
			{
				using (var db = new LiteDatabase(GetConnectionString(filename, password)))
				{
					return db.GetCollectionNames().All(name => _allowedCollectionNames.Contains(name));
				}
			}
			catch(LiteException ex) { return false; }
		}

		public static bool ChangeDbEncryptionPassword(string filename, string oldPassword, string newPassword)
		{
			using (var tempDbFile = new TemporaryFile(Environment.CurrentDirectory, "~temp " + Path.GetFileName(filename)))
			{
				File.Copy(filename, tempDbFile.FilePath, true);


				LiteDB.Engine.RebuildOptions rebuildOptions = new LiteDB.Engine.RebuildOptions();
				rebuildOptions.Password = newPassword;
				try
				{
					using (var db = new LiteDatabase(GetConnectionString(tempDbFile.FilePath, oldPassword)))
					{
						db.Rebuild(rebuildOptions);						
					}			

					File.Copy(tempDbFile.FilePath, filename, true);
					return true;
				}
				catch
				{
					return false;
				}
			}
		}

		private static bool CreateFreshRecordDb(string filename, string password, bool keepsPrivateDoctorsDb = false)
		{
			try
			{
				using var newDb = new LiteDatabase(GetConnectionString(filename, password));
				return true;
			}
			catch(System.Exception ex)
			{
				MessageBox.Show("При создании базы данных возникла ошибка.\n" + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
				return false;
			}
		}

		public static string GetConnectionString(string filename, string password)
		{
			return $"Filename={filename};Password=\"{HashString16(password)}\"";
		}

		public static string GetConnectionString(User user, string password)
		{
			return GetConnectionString(user.DatabaseFile, password);
		}

		private static string[] _allowedCollectionNames = new string[]
		{
			DoctorExaminationRecord.DbCollectionName, LabExaminationRecord.DbCollectionName,
			Doctor.DbCollectionName, Clinic.DbCollectionName,
			ExaminationType.LabAnalysisTypesDbCollectionName, ExaminationType.DoctorTypesDbCollectionName
		};

		public static string HashString16(string text)
		{			
			byte[] bytes = Encoding.UTF8.GetBytes(text);
			char[] allowedPasswordHashChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();
			int allowedCharsCount = allowedPasswordHashChars.Length;

			using SHA256 sha256 = SHA256.Create();
			byte[] hash = sha256.ComputeHash(bytes);
			StringBuilder passwordStringHash16 = new StringBuilder();
			for (int i = 0; i < 16; i++)
			{
				passwordStringHash16.Append(allowedPasswordHashChars[hash[i] % allowedCharsCount]);	
			}

			return passwordStringHash16.ToString();
		}

		public static bool UpdateRecord(Session session, ref ExaminationRecord record, IEnumerable<AttachmentMetaData> docsToDeleteFromDB, IEnumerable<AttachmentMetaData> docsToUpload)
		{
			LiteDatabase db = session.RecordsDatabaseContext;
			using FileStorage fs = new FileStorage(session);

			db.BeginTrans();
			try
			{
				var docMetaDataCol = db.GetCollection<AttachmentMetaData>(AttachmentMetaData.DbCollectionName);
				foreach (AttachmentMetaData doc in docsToDeleteFromDB) 
				{
					docMetaDataCol.Delete(doc.Id);
					fs.DeleteFileFromStorage(doc);
				}
				foreach (AttachmentMetaData doc in docsToUpload)
				{
					var id = docMetaDataCol.Insert(doc);
					doc.Id = id.AsInt32;
					fs.UploadFileToStorage(doc);
				}

				switch (record)
				{
					case DoctorExaminationRecord docRecord:
					{
						var col = db.GetCollection<DoctorExaminationRecord>(DoctorExaminationRecord.DbCollectionName);
						if (col.Update(docRecord))
							goto Commit;
						else
							throw new Exception();
					}
					case LabExaminationRecord labRecord:
					{
						var col = db.GetCollection<LabExaminationRecord>(LabExaminationRecord.DbCollectionName);
						if (col.Update(labRecord))
							goto Commit;
						else
							throw new Exception();
						}
					default:
						throw new Exception();
				}

				Commit:
				db.Commit();
				return true;
			}
			catch (Exception)
			{
				db.Rollback();
				return false;
			}			
		}

		public static bool InsertRecord(Session session, ref ExaminationRecord record)
		{
			LiteDatabase db = session.RecordsDatabaseContext;
			using FileStorage fs = new(session);

			db.BeginTrans();
			try
			{
				var docMetaDataCol = db.GetCollection<AttachmentMetaData>(AttachmentMetaData.DbCollectionName);
				foreach (AttachmentMetaData doc in record.Documents)
				{
					doc.Id = docMetaDataCol.Insert(doc).AsInt32;
					fs.UploadFileToStorage(doc);
				}

				switch (record)
				{
					case DoctorExaminationRecord docRecord:
						{
							var col = db.GetCollection<DoctorExaminationRecord>(DoctorExaminationRecord.DbCollectionName);
							docRecord.Id = col.Insert(docRecord).AsInt32;
							goto Commit;
							
						}
					case LabExaminationRecord labRecord:
						{
							var col = db.GetCollection<LabExaminationRecord>(LabExaminationRecord.DbCollectionName);
							labRecord.Id = col.Insert(labRecord).AsInt32;
							goto Commit;
						}
					default:
						throw new Exception();
				}

				Commit:
				db.Commit();
				return true;
			}
			catch (Exception)
			{
				db.Rollback();
				return false;
			}
		}

		internal static bool DeleteRecord(LiteDatabase db, ExaminationRecord record)
		{
			using FileStorage fs = new(db);

			db.BeginTrans();
			try
			{
				var docMetaDataCol = db.GetCollection<AttachmentMetaData>(AttachmentMetaData.DbCollectionName);
				foreach (AttachmentMetaData doc in record.Documents)
				{
					docMetaDataCol.Delete(doc.Id);
					fs.DeleteFileFromStorage(doc);
				}

				switch (record)
				{
					case DoctorExaminationRecord docRecord:
						{
							var col = db.GetCollection<DoctorExaminationRecord>(DoctorExaminationRecord.DbCollectionName);
							if (col.Delete(record.Id))
								return db.Commit();
							else 
								throw new Exception();
						}
					case LabExaminationRecord labRecord:
						{
							var col = db.GetCollection<LabExaminationRecord>(LabExaminationRecord.DbCollectionName);							
							if (col.Delete(labRecord.Id))
								return db.Commit();
							else
								throw new Exception();
						}
					default:
						return false;
				}
			}
			catch (Exception)
			{
				db.Rollback();
				return false;
			}			
		}

		//public static IEnumerable<ExaminationRecord> UploadFilesToRecords(this IEnumerable<ExaminationRecord> records, Session session)
		//{
		//	foreach (ExaminationRecord record in records)
		//	{
		//		foreach (AttachmentMetaData attachment in record.Documents)
		//		{
		//			int id = attachment.Id;
		//			attachment.Data = session.FileStorage.GetFileBytes(id);
		//		}
		//		yield return record;
		//	}
		//}

		public static List<ExaminationRecord> GenerateSampleExaminationRecordList(int count) => GenerateSampleRecords(count).ToList();
		
		public static IEnumerable<ExaminationRecord> GenerateSampleRecords(int count)
		{
			List<ExaminationRecord> records = new List<ExaminationRecord>();
			records.Add(new DoctorExaminationRecord
			{
				Date = new DateOnly(1989, 01, 20),
				Comment = "TestComment1",
				Doctor = new Doctor("Test doc 1", "comemnt for a doctor"),
				Documents = new(),
				ExaminationType = new ExaminationType("Sample examiunation type", "Comemnt for type")
			}
			);

			records.Add(new DoctorExaminationRecord
			{
				Date = new DateOnly(2002, 12, 31),
				Comment = "TestComment2",
				Doctor = new Doctor("Иванов И. И.", "comemnt for a Иванов"),
				Clinic = new Clinic("Тестовая клиника", "Тестовый комменарий для клиники"),
				Documents = new(),
				ExaminationType = new ExaminationType("Терапевт", "Commеnt for терапевт")
			}
			);

			records.Add(new LabExaminationRecord
			{
				Date = new DateOnly(2020, 10, 1),
				Comment = "Lab test comment",				
				Documents = new(),
				ExaminationType = new ExaminationType("ОАК", "Comemnt for ОАК")
			}
			);

			int n = 0;
			while(true)
			{
				foreach(var rec in records)
				{
					yield return rec.DeepCopy();
					n++;
					if (n == count)
						yield break;
				}
			}
		}		
	}
}
