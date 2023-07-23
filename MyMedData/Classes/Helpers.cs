using System;
using System.Collections.Generic;
using System.IO;
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

		public static DateTime ToDateTime(this DateOnly date)
		{
			return new DateTime(date.Year, date.Month, date.Day);
		}
	}

	class IDequilityCompare<T> : IEqualityComparer<IHasId<T>> where T : IComparable
	{
		public bool Equals(IHasId<T> x, IHasId<T> y)
		{
			if (Object.ReferenceEquals(x, y)) return true;

			if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
				return false;

			if (x.GetType() != y.GetType())
				return false;

			return x.Id.CompareTo(y.Id) == 0;
		}

		// If Equals() returns true for a pair of objects
		// then GetHashCode() must return the same value for these objects.

		public int GetHashCode(IHasId<T> x)
		{
			//Check whether the object is null
			if (Object.ReferenceEquals(x, null)) return 0;

			return x.Id.GetHashCode();
		}

	}

	internal static class FileChecker
	{
		internal static bool IsFileLocked(string file)
		{
			if (!File.Exists(file))
				return false;
			var fileInfo = new FileInfo(file);

			try
			{
				using (FileStream stream = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.None))
				{
					stream.Close();
				}
			}
			catch (IOException)
			{
				//the file is unavailable because it is:
				//still being written to
				//or being processed by another thread
				//or does not exist (has already been processed)
				return true;
			}

			//file is not locked
			return false;
		}
	}
}
