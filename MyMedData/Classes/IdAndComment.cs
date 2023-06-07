using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace MyMedData
{
	public class IdAndComment
	{
		private IdAndComment(string Id, string Comment)
		{ }

		public IdAndComment(IMedicalEntity item)
		{
			if (!(item is Doctor || item is ExaminationType || item is Clinic))
				throw new ArgumentException("Не поддерживаемая декоратором сущность: " + item.GetType().Name);

			if (item is Doctor doc)
			{
				_id = doc.Name;
				_type = DataType.Doctor;
			}
			else if (item is ExaminationType type)
			{
				_id = type.ExaminationTypeTitle;
				_type = DataType.ExaminaionType;
			}
			else if (item is Clinic clinic)
			{
				_id = clinic.Name;
				_type = DataType.Clinic;
			}
			else throw new Exception("Impossible exception 1");

			_comment = item.Comment;
		}

		DataType _type;

		string _id;
		public string Id
		{
			get { return _id; }
			set { _id = value; }
		}

		string _comment;
		public string Comment
		{
			get { return _comment; }
			set { _comment = value; }
		}

		public IMedicalEntity GenerateEntity() 
		{ 
			switch(_type)
			{
				case DataType.Doctor: return new Doctor(Id, Comment);
				case DataType.ExaminaionType: return new ExaminationType(Id, Comment);
				case DataType.Clinic: return new Clinic(Id, Comment);
				default: throw new Exception("Impossible exception 3");
			}
		}

		public IdAndComment Copy()
		{
			IdAndComment idAndComment = new(_id, _comment);
			idAndComment._type = _type;
			return idAndComment;
		}

		enum DataType 
		{
			Doctor,
			ExaminaionType,
			Clinic
		}
	}

	public enum EntityType
	{
		Doctor,
		DoctorType,
		LabExaminaionType,
		Clinic
	}
}
