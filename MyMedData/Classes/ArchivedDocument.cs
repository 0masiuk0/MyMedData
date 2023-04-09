using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMedData
{
	public class ArchivedDocument
	{
		[BsonId]
		public string Id { get; set; }
		public DocumentType DocumentType { get; set; }
		public string FileName { get; set; }

		public const string DbCollectionName = "ArchivedDocuments";

		public override string ToString() => FileName;

		public ArchivedDocument Copy()
		{
			return new ArchivedDocument() { Id = Id, DocumentType = DocumentType, FileName = FileName };
		}
	}

	public enum DocumentType
	{
		JPEG,
		PNG,
		PDF
	}
}
