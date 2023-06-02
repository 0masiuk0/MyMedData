using LiteDB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace MyMedData
{
	public class AttachmentMetaData : INotifyPropertyChanged, IHasId<int>
	{
		[BsonId]
		public int Id { get; set; }

		private DocumentType _documentType;
		public DocumentType DocumentType
		{
			get => _documentType;
			set
			{
				if (_documentType != value)
				{
					_documentType = value;
					OnPropertyChanged();
				}
			}
		}

		private string _fileName;
		public string FileName
		{
			get => _fileName;
			set
			{
				if (_fileName != value)
				{
					_fileName = value;
					_customName = value;
					OnPropertyChanged();
					OnPropertyChanged(nameof(CustomName));
				}
			}
		}

		private string _customName;
		public string CustomName
		{
			get
			{
					return _customName;
			}
			set
			{
				if (value != _customName)
				{
					_customName = value;
					OnPropertyChanged();
				}
			}
		}


		[BsonIgnore]
		public byte[]? Data;

		public const string DbCollectionName = "ArchivedDocuments";

		public override string ToString() => FileName;

		public void ClearData()
		{
			Data = null;
		}

		public async Task<byte[]> LoadData(Session session)
		{
			if (Data == null)
			{
				using FileStorage fs = new FileStorage(session);
				Task<byte[]> task = new Task<byte[]>( 
					() => fs.GetFileBytes(Id));	
				task.Start();
				return await task;
			}
			else
				return await Task.FromResult(Data);
		}
		
		public AttachmentMetaData DeepCopy()
		{
			return new AttachmentMetaData() { Id = Id, DocumentType = DocumentType, FileName = FileName, CustomName = CustomName };
		}

		public bool MetaDataEqual(AttachmentMetaData other)
		{
			return other.FileName == FileName && other.CustomName == CustomName;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	public enum DocumentType
	{
		JPEG,
		PNG,
		PDF
	}
}
