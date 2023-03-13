using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMedData
{
	public class Session : IDisposable
	{
		public readonly User ActiveUser;
		public readonly LiteDatabase DocumentsDatabaseContext;
		//public string Password { private get; set; }

		public Session(User user, string password)
		{
			ActiveUser = user;
			//Password = password;
			DocumentsDatabaseContext = new LiteDatabase(DocumentsDataBase.GetConnectionString(user, password));
		}

		public void Dispose()
		{
			DocumentsDatabaseContext.Dispose();			
		}
	}
}
