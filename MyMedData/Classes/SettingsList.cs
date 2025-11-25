using LiteDB;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Emit;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;

namespace MyMedData
{
	internal static partial class AppConfigDatabase
	{
		internal static class Settings
		{
			public static string? LastUser
			{
				get => settingsCache.GetValueOrDefault(nameof(LastUser), null);
				set => settingsCache[nameof(LastUser)] = value;
			}

			public static string? DefaultScannerName
			{
				get => settingsCache.GetValueOrDefault(nameof(DefaultScannerName), null);
				set => settingsCache[nameof(DefaultScannerName)] = value;
			}

			public static string? DPI 
			{
				get => settingsCache.GetValueOrDefault(nameof(DPI), null);
				set => settingsCache[nameof(DPI)] = value;
			}

			public static string? AutoLogin
			{
				get => settingsCache.GetValueOrDefault(nameof(AutoLogin), null);
				set => settingsCache[nameof(AutoLogin)] = value;
			}

			static Dictionary<string, string?> settingsCache = new Dictionary<string, string?>();
			static Dictionary<string, string?> uneditedSettingsCache = new Dictionary<string, string?>();

			static Settings()
			{
				LoadSettings();
			}

			private static void LoadSettings()
			{
				settingsCache = UsersAndSettingsDatabase.GetCollection<Setting>(SettingsCollection).FindAll().ToDictionary(s => s.Key, s => s.Value);
				uneditedSettingsCache = settingsCache.ToDictionary(s => s.Key, s => s.Value);
			}

			/// <param name="key">Use like UpsertSettings(nameof(UsersDatabase.Settigns.Someting))</param>
			/// <param name="value"></param>
			internal static void UpsertSetting(string key, string value)
			{
				settingsCache[key] = value;
			}

			internal static void SaveSettings()
			{
				var settingsCol = UsersAndSettingsDatabase.GetCollection<Setting>(SettingsCollection);
				foreach (var key in settingsCache.Keys) 
				{ 
					settingsCol.Upsert(new Setting(key, settingsCache[key]));
					string? oldValue = uneditedSettingsCache.GetValueOrDefault(key, null);
					RaiseSettingsChangedEvent(
						new Setting(key, oldValue),
						new Setting(key, settingsCache[key]));
				}
				uneditedSettingsCache = settingsCache.ToDictionary(s => s.Key, s => s.Value);
			}

			/// <param name="key">Use like UpsertSettings(nameof(UsersDatabase.Settigns.Someting))</param>
			internal static void CancelSettingUpdate(string key)
			{
				settingsCache[key] = uneditedSettingsCache[key];
			}

			internal static void CancelSettingUpdate() 
			{ 
				settingsCache = UsersAndSettingsDatabase.GetCollection<Setting>(SettingsCollection).FindAll().ToDictionary(s => s.Key, s => s.Value);
			}

			const string SettingsCollection = "Settings";

			internal static event SettingsChangedEventHandler SettingsChanged;

			static void RaiseSettingsChangedEvent(Setting oldValue, Setting newValue)
			{
				SettingsChanged?.Invoke(oldValue, newValue);
			}
		}		
	}

	public class Setting
	{
		[BsonId]
		public string Key { get; set; }
		public string? Value { get; set; }

		public Setting(string key, string? value)
		{
			Key = key;
			Value = value;
		}		
	}

	internal delegate void SettingsChangedEventHandler(Setting oldValue, Setting newValue);
}