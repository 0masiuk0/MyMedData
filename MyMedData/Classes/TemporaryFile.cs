using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MyMedData
{
	public sealed class TemporaryFile : IDisposable
	{
		public TemporaryFile() :
		  this(Path.GetTempPath())
		{ }

		public TemporaryFile(string directory) : this(directory, Path.GetRandomFileName())
		{
		}

		public TemporaryFile(string directory, string filename)
		{
			Create(Path.Combine(directory, filename));
		}

		~TemporaryFile()
		{
			Delete();
		}

		public void Dispose()
		{
			Delete();
			GC.SuppressFinalize(this);
		}

		public string FilePath { get; private set; }

		private void Create(string path)
		{
			FilePath = path;
			using (File.Create(FilePath)) { };
		}

		private void Delete()
		{
			if (FilePath == null) return;
			File.Delete(FilePath);
			FilePath = null;
		}
	}
}
