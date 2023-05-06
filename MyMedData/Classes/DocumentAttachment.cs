using LiteDB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MyMedData
{
	public class DocumentAttachment : INotifyPropertyChanged
	{
		[BsonId]
		public string Id { get; set; }

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
					OnPropertyChanged();
				}
			}
		}

		[BsonIgnore] 
		public byte[]? Data;

		public const string DbCollectionName = "ArchivedDocuments";	

		public override string ToString() => FileName;

		public DocumentAttachment Copy()
		{
			return new DocumentAttachment() { Id = Id, DocumentType = DocumentType, FileName = FileName };
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
