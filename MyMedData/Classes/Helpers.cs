using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMedData
{ 

	public static class DateTimeExtensions
	{
		public static DateOnly ToDateOnly(this DateTime dateTime)
		{
			return new DateOnly(dateTime.Year, dateTime.Month, dateTime.Day);
		}
	}

}
