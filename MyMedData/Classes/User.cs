using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using LiteDB;

namespace MyMedData
{
	public class User
	{
		public static User? ActiveUser { get; private set; }

		//public int Id { get; set; }
		public string Name { get; set; }
		public Brush AccountColor { get; set; }
		public byte[] PasswordHash { get; set; }
		public string RecordsDBFileName;

		public User()
		{
			Name = ""; AccountColor = Brushes.White;
			PasswordHash = new byte[32];
			RecordsDBFileName = "";
		}

		public User(string name, Brush accountColor, byte[] passwordHash, string recordsDBFileName)
		{
			Name = name;
			AccountColor = accountColor;
			PasswordHash = passwordHash;
			RecordsDBFileName = recordsDBFileName;
		}

		internal static void LogOff()
		{
			ActiveUser = null;
		}
	}
}
