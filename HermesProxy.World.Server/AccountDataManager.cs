using System.IO;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server;

/*
 * 账户数据管理器
 */
public class AccountDataManager
{
	public AccountData[] Data;

	private string _accountName;

	private string _realmName;

	public AccountDataManager(string accountName, string realmName)
	{
		_accountName = accountName;
		_realmName = realmName.Trim();
	}

	public static bool IsGlobalDataType(uint type)
	{
		switch ((AccountDataType)type)
		{
		case AccountDataType.GlobalConfigCache:
		case AccountDataType.GlobalBindingsCache:
		case AccountDataType.GlobalMacrosCache:
		case AccountDataType.GlobalTTSCache:
		case AccountDataType.GlobalFlaggedCache:
			return true;
		default:
			return false;
		}
	}

	public string GetAccountDataDirectory()
	{
		string fullPath = Path.GetFullPath(Path.Combine("AccountData", _accountName, _realmName));
		if (!Directory.Exists(fullPath))
		{
			Directory.CreateDirectory(fullPath);
		}
		return fullPath;
	}

	public string GetFullFileName(WowGuid128 guid, uint type)
	{
		string text = ((!IsGlobalDataType(type)) ? $"data-{type}-{guid.GetLowValue()}-{guid.GetHighValue()}.bin" : $"data-{type}.bin");
		return Path.Combine(GetAccountDataDirectory(), text);
	}

	public void LoadAllData(WowGuid128 guid)
	{
		Data = new AccountData[ModernVersion.GetAccountDataCount()];
		for (uint num = 0u; num < ModernVersion.GetAccountDataCount(); num++)
		{
			Data[num] = LoadData(guid, num);
		}
	}

	public AccountData LoadData(WowGuid128 guid, uint type)
	{
		AccountData accountData = null;
		if (File.Exists(GetFullFileName(guid, type)))
		{
			using (File.OpenRead(GetFullFileName(guid, type)))
			{
				using BinaryReader binaryReader = new BinaryReader(File.OpenRead(GetFullFileName(guid, type)));
				accountData = new AccountData();
				ulong low = binaryReader.ReadUInt64();
				ulong high = binaryReader.ReadUInt64();
				accountData.Guid = new WowGuid128(high, low);
				IsGlobalDataType(type);
				accountData.Timestamp = binaryReader.ReadInt64();
				accountData.Type = binaryReader.ReadUInt32();
				accountData.UncompressedSize = binaryReader.ReadUInt32();
				int num = binaryReader.ReadInt32();
				accountData.CompressedData = binaryReader.ReadBytes(num);
			}
		}
		return accountData;
	}

	public void SaveData(WowGuid128 guid, long timestamp, uint type, uint uncompressedSize, byte[] compressedData)
	{
		if (compressedData == null)
		{
			return;
		}
		if (Data[type] == null)
		{
			Data[type] = new AccountData();
		}
		Data[type].Guid = guid;
		Data[type].Timestamp = timestamp;
		Data[type].Type = type;
		Data[type].UncompressedSize = uncompressedSize;
		Data[type].CompressedData = compressedData;
		using BinaryWriter binaryWriter = new BinaryWriter(File.Open(GetFullFileName(guid, type), FileMode.Create));
		binaryWriter.Write(guid.GetLowValue());
		binaryWriter.Write(guid.GetHighValue());
		binaryWriter.Write(timestamp);
		binaryWriter.Write(type);
		binaryWriter.Write(uncompressedSize);
		binaryWriter.Write(compressedData.Length);
		binaryWriter.Write(compressedData);
	}

	public byte[] LoadCUFProfiles()
	{
		string text = Path.Combine(GetAccountDataDirectory(), "cuf.bin");
		if (File.Exists(text))
		{
			using (FileStream fileStream = File.OpenRead(text))
			{
				using (new BinaryReader(fileStream))
				{
					return File.ReadAllBytes(text);
				}
			}
		}
		return new byte[4];
	}

	public void SaveCUFProfiles(byte[] data)
	{
		using BinaryWriter binaryWriter = new BinaryWriter(File.Open(Path.Combine(GetAccountDataDirectory(), "cuf.bin"), FileMode.Create));
		binaryWriter.Write(data);
	}
}
