using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MyMedData.Classes
{
	internal static class SettingsManager
	{
		public static KeyValueConfigurationCollection AppSettings;
		static System.Configuration.Configuration ConfigFile;

		static SettingsManager()
		{
			ConfigFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
			AppSettings = ConfigFile.AppSettings.Settings;
		}

		public static void UpsertSetting(string settingName, string newValue)
		{
			try
			{
				var settings = AppSettings;

				if (settings[settingName] == null)
				{
					settings.Add(settingName, newValue);
				}
				else
				{
					settings[settingName].Value = newValue;
				}

				ConfigFile.Save(ConfigurationSaveMode.Modified);
				ConfigurationManager.RefreshSection(ConfigFile.AppSettings.SectionInformation.Name);
			}
			catch (ConfigurationErrorsException)
			{
				System.Windows.MessageBox.Show("Ошибка сохранения настроек!", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		public static string GetOrInsertDefaultValue(string settingName, string defaultValue)
		{
			try
			{
				if (AppSettings[settingName]?.Value is string value)
					return value;
				else
				{
					AppSettings.Add(settingName, defaultValue);
					return defaultValue;
				}
			}
			catch (ConfigurationErrorsException e)
			{
				System.Windows.MessageBox.Show("Ошибка сохранения настроек!", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
				throw e;
			}
		}
	}
}
