using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using Framework.Logging;

namespace HermesProxy.Configuration;

public class ConfigurationParser
{
	private readonly KeyValueConfigurationCollection _settingsCollection;

	public ConfigurationParser(KeyValueConfigurationCollection configCollection)
	{
		_settingsCollection = configCollection;
	}

	public static ConfigurationParser ParseFromFile(string configFile, Dictionary<string, string> overwrittenValues)
	{
		KeyValueConfigurationCollection settings;
		try
		{
			if (!File.Exists(configFile))
			{
				throw new FileNotFoundException("File '" + configFile + "' was not found.");
			}
			settings = ((AppSettingsSection)ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap
			{
				ExeConfigFilename = configFile
			}, ConfigurationUserLevel.None).Sections.Get("appSettings")).Settings;
		}
		catch
		{
			Log.Print(LogType.Error, "Fail to load config file '" + configFile + "'", "ParseFromFile", "D:\\a\\HermesProxy\\HermesProxy\\Configuration\\ConfigurationParser.cs");
			throw;
		}
		foreach (KeyValuePair<string, string> overwrittenValue in overwrittenValues)
		{
			settings.Remove(overwrittenValue.Key);
			settings.Add(overwrittenValue.Key, overwrittenValue.Value);
		}
		return new ConfigurationParser(settings);
	}

	public string GetString(string key, string defValue)
	{
		return _settingsCollection[key]?.Value ?? defValue;
	}

	public string[] GetStringList(string key, string[] defValue)
	{
		KeyValueConfigurationElement keyValueConfigurationElement = _settingsCollection[key];
		if (keyValueConfigurationElement == null || keyValueConfigurationElement.Value == null)
		{
			return defValue;
		}
		string[] array = keyValueConfigurationElement.Value.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = array[i].Trim();
		}
		return array;
	}

	public bool GetBoolean(string key, bool defValue)
	{
		KeyValueConfigurationElement keyValueConfigurationElement = _settingsCollection[key];
		if (keyValueConfigurationElement == null || keyValueConfigurationElement.Value == null)
		{
			return defValue;
		}
		if (bool.TryParse(keyValueConfigurationElement.Value, out var result))
		{
			return result;
		}
		Console.WriteLine("Warning: \"{0}\" is not a valid boolean value for key \"{1}\"", keyValueConfigurationElement.Value, key);
		return defValue;
	}

	public int GetInt(string key, int defValue)
	{
		KeyValueConfigurationElement keyValueConfigurationElement = _settingsCollection[key];
		if (string.IsNullOrEmpty(keyValueConfigurationElement?.Value))
		{
			return defValue;
		}
		if (int.TryParse(keyValueConfigurationElement.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
		{
			return result;
		}
		Console.WriteLine("Warning: \"{0}\" is not a valid integer value for key \"{1}\"", keyValueConfigurationElement.Value, key);
		return defValue;
	}

	public TEnum GetEnum<TEnum>(string key, TEnum defValue) where TEnum : struct
	{
		KeyValueConfigurationElement keyValueConfigurationElement = _settingsCollection[key];
		if (string.IsNullOrEmpty(keyValueConfigurationElement?.Value))
		{
			return defValue;
		}
		if (!int.TryParse(keyValueConfigurationElement.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
		{
			Console.WriteLine("Warning: \"{0}\" is not a valid integer value for key \"{1}\"", keyValueConfigurationElement.Value, key);
			return defValue;
		}
		if (Enum.IsDefined(typeof(TEnum), result))
		{
			return (TEnum)(object)result;
		}
		if (Enum.TryParse<TEnum>(result.ToString(), out var result2))
		{
			return result2;
		}
		Console.WriteLine("Warning: \"{0}\" is not a valid enum value for key \"{1}\", enum \"{2}\"", keyValueConfigurationElement.Value, key, typeof(TEnum).Name);
		return defValue;
	}

	public byte[] GetByteArray(string key, byte[] defValue)
	{
		KeyValueConfigurationElement keyValueConfigurationElement = _settingsCollection[key];
		if (string.IsNullOrWhiteSpace(keyValueConfigurationElement?.Value))
		{
			return defValue;
		}
		return keyValueConfigurationElement.Value.ParseAsByteArray();
	}
}
