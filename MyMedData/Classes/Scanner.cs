using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAPS2;
using NAPS2.Wia;

namespace MyMedData.Classes
{
	internal class ScannerManager
	{
		public ScannerManager() { }

		public static string[] GetDeviceNames()
		{
			using var deviceManager = new WiaDeviceManager();
			return deviceManager.GetDeviceInfos().Select(info => info.Name()).ToArray();
		}

		public const string DEFAULT_SCANNER_NAME_SETTING_KEY = "scanner_name";

		public static string? GetSetScannerName()
		{
			return SettingsManager.AppSettings[DEFAULT_SCANNER_NAME_SETTING_KEY]?.Value;
		}


	}
}
