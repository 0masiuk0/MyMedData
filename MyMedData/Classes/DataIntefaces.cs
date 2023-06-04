using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
