using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Framework.Logging;

namespace HermesProxy.World.Server;

/*
 * 帐户元数据管理器
 */
public class AccountMetaDataManager
{
	private const string LAST_CHARACTER_FILE = "last_character.txt";

	private const string COMPLETED_QUESTS_FILE = "completed_quests.csv";

	private const string SETTINGS_FILE = "settings.json";

	private readonly string _accountName;

	private string GetAccountMetaDataDirectory()
	{
		string fullPath = Path.GetFullPath(Path.Combine("AccountData", _accountName));
		if (!Directory.Exists(fullPath))
		{
			Directory.CreateDirectory(fullPath);
		}
		return fullPath;
	}

	private string GetAccountCharacterMetaDataDirectory(string realm, string characterName)
	{
		string fullPath = Path.GetFullPath(Path.Combine("AccountData", _accountName, realm, characterName));
		if (!Directory.Exists(fullPath))
		{
			Directory.CreateDirectory(fullPath);
		}
		return fullPath;
	}

	public AccountMetaDataManager(string accountName)
	{
		_accountName = accountName;
	}

	public (string realmName, string charName, ulong charLowerGuid, long lastLoginUnixSec)? GetLastSelectedCharacter()
	{
		string text = Path.Combine(GetAccountMetaDataDirectory(), "last_character.txt");
		if (!File.Exists(text))
		{
			return null;
		}
		string[] array = File.ReadAllText(text, Encoding.UTF8).Split(',');
		if (array.Length != 4)
		{
			Log.Print(LogType.Error, "Invalid split size in 'GetLastSelectedCharacter' for account '" + _accountName + "'", "GetLastSelectedCharacter", "D:\\a\\HermesProxy\\HermesProxy\\World\\Server\\AccountDataManager.cs");
			return null;
		}
		return (array[0], array[1], ulong.Parse(array[3]), long.Parse(array[2]));
	}

	public void SaveLastSelectedCharacter(string realmName, string charName, ulong charLowerGuid, long lastLoginUnixSec)
	{
		string text = Path.Combine(GetAccountMetaDataDirectory(), "last_character.txt");
		File.WriteAllText(text, $"{realmName},{charName},{charLowerGuid},{lastLoginUnixSec}", Encoding.UTF8);
		Log.Print(LogType.Debug, "Saved last selected char in '" + text + "'", "SaveLastSelectedCharacter", "D:\\a\\HermesProxy\\HermesProxy\\World\\Server\\AccountDataManager.cs");
	}

	public void InvalidateLastSelectedCharacter()
	{
		string text = Path.Combine(GetAccountMetaDataDirectory(), "last_character.txt");
		if (File.Exists(text))
		{
			File.WriteAllText(text, "");
			Log.Print(LogType.Debug, "Invalidated last selected character entry in '" + text + "'", "InvalidateLastSelectedCharacter", "D:\\a\\HermesProxy\\HermesProxy\\World\\Server\\AccountDataManager.cs");
		}
	}

	public List<uint> GetAllCompletedQuests(string realmName, string charName)
	{
		string text = Path.Combine(GetAccountCharacterMetaDataDirectory(realmName, charName), "completed_quests.csv");
		if (!File.Exists(text))
		{
			return new List<uint>();
		}
		return (from x in File.ReadAllLines(text).ToList()
			select uint.Parse(x.Split(',').FirstOrDefault() ?? "0")).ToList();
	}

	public void MarkQuestAsCompleted(string realmName, string charName, uint questId)
	{
		string text = Path.Combine(GetAccountCharacterMetaDataDirectory(realmName, charName), "completed_quests.csv");
		long num = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
		File.AppendAllLines(text, new string[1] { $"{questId},{num}" }, Encoding.UTF8);
	}

	public void MarkQuestAsNotCompleted(string realmName, string charName, uint questId)
	{
		string text = Path.Combine(GetAccountCharacterMetaDataDirectory(realmName, charName), "completed_quests.csv");
		string needle = questId.ToString();
		List<string> list = File.ReadAllLines(text).ToList();
		list.RemoveAll((string l) => l.Split(',').FirstOrDefault()?.Equals(needle) ?? false);
		File.WriteAllLines(text, list);
	}

	public void SaveCharacterSettingsStorage(string realmName, string charName, PlayerSettings.InternalStorage settings)
	{
		string text = Path.Combine(GetAccountCharacterMetaDataDirectory(realmName, charName), "settings.json");
		JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
		{
			WriteIndented = true
		};
		string text2 = JsonSerializer.Serialize(settings, jsonSerializerOptions);
		File.WriteAllText(text, text2, Encoding.UTF8);
	}

	public PlayerSettings.InternalStorage LoadCharacterSettingsStorage(string realmName, string charName)
	{
		string text = Path.Combine(GetAccountCharacterMetaDataDirectory(realmName, charName), "settings.json");
		if (!File.Exists(text))
		{
			PlayerSettings.InternalStorage internalStorage = new PlayerSettings.InternalStorage();
			SaveCharacterSettingsStorage(realmName, charName, internalStorage);
			return internalStorage;
		}
		return JsonSerializer.Deserialize<PlayerSettings.InternalStorage>(File.ReadAllText(text, Encoding.UTF8));
	}
}
