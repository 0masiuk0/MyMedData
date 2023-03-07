using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;

namespace MyMedData
{
	public class Record
	{
		[BsonId]
		public int Id { get; set; }

	}
}
