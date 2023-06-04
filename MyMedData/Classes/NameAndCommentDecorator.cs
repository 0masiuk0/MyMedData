using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMedData.Classes
{
	internal class NameAndCommentDecorator<T> where T: IHasId<string>, IHasComment
	{
		public NameAndCommentDecorator(IHasId<string> item)
		{
			if (!(item is Doctor || item is ExaminationType || item is Clinic))
				throw new ArgumentException("Не поддерживаемая декоратором сущность: " + item.GetType().Name);

			_item = (T?)item;
		}

		readonly T _item;
		public T Item => _item;

		public string Id
		{
			get
			{
				if (_item is Doctor doc)
					return doc.Name;
				else if (_item is ExaminationType type)
					return type.ExaminationTypeTitle;
				else if (_item is Clinic clinic)
					return clinic.Name;
				else throw new Exception("Impossible exception 1");
			}
		}
		
		public string Comment
		{
			get { return _item.Comment; }
			set { _item.Comment = value; }
		}
	}
}
