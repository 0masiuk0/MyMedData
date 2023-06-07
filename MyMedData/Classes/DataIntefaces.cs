using System;
using System.ComponentModel;

namespace MyMedData
{
	public interface IHasId<T> where T : IComparable
	{
		public T Id { get; }
	}

	public interface IHasComment
	{
		public string Comment { get; set; }
	}

	public interface IMedicalEntity : IHasId<string>, IHasComment, INotifyPropertyChanged {	}
}
