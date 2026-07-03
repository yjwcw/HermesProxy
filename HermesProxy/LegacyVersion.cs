using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Framework;
using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Enums.V1_12_1_5875;
using HermesProxy.World.Enums.V2_4_3_8606;

namespace HermesProxy;

public static class LegacyVersion
{
	private static readonly Dictionary<uint, HermesProxy.World.Enums.Opcode> CurrentToUniversalOpcodeDictionary;

	private static readonly Dictionary<HermesProxy.World.Enums.Opcode, uint> UniversalToCurrentOpcodeDictionary;

	private static readonly Dictionary<Type, SortedList<int, UpdateFieldInfo>> UpdateFieldDictionary;

	private static readonly Dictionary<Type, Dictionary<string, int>> UpdateFieldNameDictionary;

	public static byte ExpansionVersion { get; private set; }

	public static byte MajorVersion { get; private set; }

	public static byte MinorVersion { get; private set; }

	public static ClientVersionBuild Build { get; private set; }

	public static int BuildInt => (int)Build;

	public static string VersionString => Build.ToString();

	static LegacyVersion()
	{
		CurrentToUniversalOpcodeDictionary = new Dictionary<uint, HermesProxy.World.Enums.Opcode>();
		UniversalToCurrentOpcodeDictionary = new Dictionary<HermesProxy.World.Enums.Opcode, uint>();
		Build = Settings.ServerBuild;
		ExpansionVersion = GetExpansionVersion();
		MajorVersion = GetMajorPatchVersion();
		MinorVersion = GetMinorPatchVersion();
		UpdateFieldDictionary = new Dictionary<Type, SortedList<int, UpdateFieldInfo>>();
		UpdateFieldNameDictionary = new Dictionary<Type, Dictionary<string, int>>();
		if (!LoadUFDictionariesInto(UpdateFieldDictionary, UpdateFieldNameDictionary))
		{
			Log.Print(LogType.Error, "Could not load update fields for current legacy version.", ".cctor", "D:\\a\\HermesProxy\\HermesProxy\\VersionChecker.cs");
		}
		if (!LoadOpcodeDictionaries())
		{
			Log.Print(LogType.Error, "Could not load opcodes for current legacy version.", ".cctor", "D:\\a\\HermesProxy\\HermesProxy\\VersionChecker.cs");
		}
	}

	private static bool LoadOpcodeDictionaries()
	{
		Type opcodesEnumForVersion = Opcodes.GetOpcodesEnumForVersion(Build);
		if (opcodesEnumForVersion == null)
		{
			return false;
		}
		foreach (object value in Enum.GetValues(opcodesEnumForVersion))
		{
			string name = Enum.GetName(opcodesEnumForVersion, value);
			HermesProxy.World.Enums.Opcode universalOpcode = Opcodes.GetUniversalOpcode(name);
			if (universalOpcode == HermesProxy.World.Enums.Opcode.MSG_NULL_ACTION && name != "MSG_NULL_ACTION")
			{
				Log.Print(LogType.Error, "Opcode " + name + " is missing from the universal opcode enum!", "LoadOpcodeDictionaries", "D:\\a\\HermesProxy\\HermesProxy\\VersionChecker.cs");
				continue;
			}
			CurrentToUniversalOpcodeDictionary.Add((uint)value, universalOpcode);
			UniversalToCurrentOpcodeDictionary.Add(universalOpcode, (uint)value);
		}
		if (CurrentToUniversalOpcodeDictionary.Count < 1)
		{
			return false;
		}
		Log.Print(LogType.Server, $"Loaded {CurrentToUniversalOpcodeDictionary.Count} legacy opcodes.", "LoadOpcodeDictionaries", "D:\\a\\HermesProxy\\HermesProxy\\VersionChecker.cs");
		return true;
	}

	public static HermesProxy.World.Enums.Opcode GetUniversalOpcode(uint opcode)
	{
		if (CurrentToUniversalOpcodeDictionary.TryGetValue(opcode, out var value))
		{
			return value;
		}
		return HermesProxy.World.Enums.Opcode.MSG_NULL_ACTION;
	}

	public static uint GetCurrentOpcode(HermesProxy.World.Enums.Opcode universalOpcode)
	{
		if (UniversalToCurrentOpcodeDictionary.TryGetValue(universalOpcode, out var value))
		{
			return value;
		}
		return 0u;
	}

	public static ClientVersionBuild GetUpdateFieldsDefiningBuild()
	{
		return GetUpdateFieldsDefiningBuild(Build);
	}

	public static ClientVersionBuild GetUpdateFieldsDefiningBuild(ClientVersionBuild version)
	{
		switch (version)
		{
		case ClientVersionBuild.V1_12_1_5875:
		case ClientVersionBuild.V1_12_2_6005:
		case ClientVersionBuild.V1_12_3_6141:
			return ClientVersionBuild.V1_12_1_5875;
		case ClientVersionBuild.V2_4_3_8606:
			return ClientVersionBuild.V2_4_3_8606;
		case ClientVersionBuild.V3_3_5a_12340:
			return ClientVersionBuild.V3_3_5a_12340;
		default:
			return ClientVersionBuild.Zero;
		}
	}

	private static bool LoadUFDictionariesInto(Dictionary<Type, SortedList<int, UpdateFieldInfo>> dicts, Dictionary<Type, Dictionary<string, int>> nameToValueDict)
	{
		Type[] obj = new Type[28]
		{
			typeof(HermesProxy.World.Enums.ObjectField),
			typeof(HermesProxy.World.Enums.ItemField),
			typeof(HermesProxy.World.Enums.ContainerField),
			typeof(AzeriteEmpoweredItemField),
			typeof(AzeriteItemField),
			typeof(HermesProxy.World.Enums.UnitField),
			typeof(HermesProxy.World.Enums.PlayerField),
			typeof(ActivePlayerField),
			typeof(HermesProxy.World.Enums.GameObjectField),
			typeof(HermesProxy.World.Enums.DynamicObjectField),
			typeof(HermesProxy.World.Enums.CorpseField),
			typeof(AreaTriggerField),
			typeof(SceneObjectField),
			typeof(ConversationField),
			typeof(ObjectDynamicField),
			typeof(ItemDynamicField),
			typeof(ContainerDynamicField),
			typeof(AzeriteEmpoweredItemDynamicField),
			typeof(AzeriteItemDynamicField),
			typeof(UnitDynamicField),
			typeof(PlayerDynamicField),
			typeof(ActivePlayerDynamicField),
			typeof(GameObjectDynamicField),
			typeof(DynamicObjectDynamicField),
			typeof(CorpseDynamicField),
			typeof(AreaTriggerDynamicField),
			typeof(SceneObjectDynamicField),
			typeof(ConversationDynamicField)
		};
		ClientVersionBuild updateFieldsDefiningBuild = GetUpdateFieldsDefiningBuild(Build);
		bool result = false;
		Type[] array = obj;
		foreach (Type type in array)
		{
			string text = "HermesProxy.World.Enums." + updateFieldsDefiningBuild.ToString() + "." + type.Name;
			Type type2 = Assembly.GetExecutingAssembly().GetType(text);
			if (type2 == null)
			{
				text = "HermesProxy.World.Enums." + updateFieldsDefiningBuild.ToString() + "." + type.Name;
				type2 = Assembly.GetExecutingAssembly().GetType(text);
				if (type2 == null)
				{
					continue;
				}
			}
			Array values = Enum.GetValues(type2);
			string[] names = Enum.GetNames(type2);
			SortedList<int, UpdateFieldInfo> sortedList = new SortedList<int, UpdateFieldInfo>(values.Length);
			Dictionary<string, int> dictionary = new Dictionary<string, int>(names.Length);
			for (int j = 0; j < values.Length; j++)
			{
				UpdateFieldType format = (from attribute in type.GetMember(names[j]).SelectMany((MemberInfo member) => member.GetCustomAttributes(typeof(UpdateFieldAttribute), false))
					where ((UpdateFieldAttribute)attribute).Version <= Build
					orderby ((UpdateFieldAttribute)attribute).Version descending
					select ((UpdateFieldAttribute)attribute).UFAttribute).DefaultIfEmpty(UpdateFieldType.Default).First();
				sortedList.Add((int)values.GetValue(j), new UpdateFieldInfo
				{
					Value = (int)values.GetValue(j),
					Name = names[j],
					Size = 0,
					Format = format
				});
				dictionary.Add(names[j], (int)values.GetValue(j));
			}
			for (int k = 0; k < sortedList.Count - 1; k++)
			{
				sortedList.Values[k].Size = sortedList.Keys[k + 1] - sortedList.Keys[k];
			}
			dicts.Add(type, sortedList);
			nameToValueDict.Add(type, dictionary);
			result = true;
		}
		return result;
	}

	public static int GetUpdateField<T>(T field)
	{
		if (UpdateFieldNameDictionary.TryGetValue(typeof(T), out var value) && value.TryGetValue(field.ToString(), out var value2))
		{
			return value2;
		}
		return -1;
	}

	public static string GetUpdateFieldName<T>(int field)
	{
		if (UpdateFieldDictionary.TryGetValue(typeof(T), out var value) && value.Count != 0)
		{
			int num = value.BinarySearch(field);
			if (num >= 0)
			{
				return value.Values[num].Name;
			}
			num = ~num - 1;
			int num2 = value.Keys[num];
			return value.Values[num].Name + " + " + (field - num2);
		}
		return field.ToString(CultureInfo.InvariantCulture);
	}

	public static UpdateFieldInfo GetUpdateFieldInfo<T>(int field)
	{
		if (UpdateFieldDictionary.TryGetValue(typeof(T), out var value) && value.Count != 0)
		{
			int num = value.BinarySearch(field);
			if (num >= 0)
			{
				return value.Values[num];
			}
			return value.Values[~num - 1];
		}
		return null;
	}

	public static Type GetResponseCodesEnum()
	{
		switch (Opcodes.GetOpcodesDefiningBuild(Build))
		{
		case ClientVersionBuild.V1_12_1_5875:
			return typeof(HermesProxy.World.Enums.V1_12_1_5875.ResponseCodes);
		case ClientVersionBuild.V2_4_3_8606:
		case ClientVersionBuild.V3_3_5a_12340:
			return typeof(HermesProxy.World.Enums.V2_4_3_8606.ResponseCodes);
		default:
			return null;
		}
	}

	private static byte GetExpansionVersion()
	{
		string versionString = VersionString;
		versionString = versionString.Replace("V", "");
		versionString = versionString.Substring(0, versionString.IndexOf("_"));
		return (byte)uint.Parse(versionString);
	}

	private static byte GetMajorPatchVersion()
	{
		string versionString = VersionString;
		versionString = versionString.Substring(versionString.IndexOf('_') + 1);
		versionString = versionString.Substring(0, versionString.IndexOf("_"));
		return (byte)uint.Parse(versionString);
	}

	private static byte GetMinorPatchVersion()
	{
		string versionString = VersionString;
		versionString = versionString.Substring(versionString.IndexOf('_') + 1);
		versionString = versionString.Substring(versionString.IndexOf('_') + 1);
		versionString = versionString.Substring(0, versionString.IndexOf("_"));
		return (byte)uint.Parse(versionString);
	}

	public static bool InVersion(ClientVersionBuild build1, ClientVersionBuild build2)
	{
		if (AddedInVersion(build1))
		{
			return RemovedInVersion(build2);
		}
		return false;
	}

	public static bool AddedInVersion(ClientVersionBuild build)
	{
		return Build >= build;
	}

	public static bool RemovedInVersion(ClientVersionBuild build)
	{
		return Build < build;
	}

	public static int GetPowersCount()
	{
		if (RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			return 5;
		}
		return 7;
	}

	public static byte GetMaxLevel()
	{
		if (RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			return 60;
		}
		if (RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			return 70;
		}
		return 80;
	}

	public static HitInfo ConvertHitInfoFlags(uint hitInfo)
	{
		if (RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			return ((HitInfoVanilla)hitInfo).CastFlags<HitInfo>();
		}
		return (HitInfo)hitInfo;
	}

	public static uint ConvertSpellCastResult(uint result)
	{
		if (AddedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			Type typeFromHandle = typeof(SpellCastResultClassic);
			SpellCastResultWotLK spellCastResultWotLK = (SpellCastResultWotLK)result;
			return (uint)Enum.Parse(typeFromHandle, spellCastResultWotLK.ToString());
		}
		if (AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			Type typeFromHandle2 = typeof(SpellCastResultClassic);
			SpellCastResultTBC spellCastResultTBC = (SpellCastResultTBC)result;
			return (uint)Enum.Parse(typeFromHandle2, spellCastResultTBC.ToString());
		}
		Type typeFromHandle3 = typeof(SpellCastResultClassic);
		SpellCastResultVanilla spellCastResultVanilla = (SpellCastResultVanilla)result;
		return (uint)Enum.Parse(typeFromHandle3, spellCastResultVanilla.ToString());
	}

	public static QuestGiverStatusModern ConvertQuestGiverStatus(byte status)
	{
		if (AddedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			Type typeFromHandle = typeof(QuestGiverStatusModern);
			QuestGiverStatusWotLK questGiverStatusWotLK = (QuestGiverStatusWotLK)status;
			return (QuestGiverStatusModern)Enum.Parse(typeFromHandle, questGiverStatusWotLK.ToString());
		}
		if (AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			Type typeFromHandle2 = typeof(QuestGiverStatusModern);
			QuestGiverStatusTBC questGiverStatusTBC = (QuestGiverStatusTBC)status;
			return (QuestGiverStatusModern)Enum.Parse(typeFromHandle2, questGiverStatusTBC.ToString());
		}
		Type typeFromHandle3 = typeof(QuestGiverStatusModern);
		QuestGiverStatusVanilla questGiverStatusVanilla = (QuestGiverStatusVanilla)status;
		return (QuestGiverStatusModern)Enum.Parse(typeFromHandle3, questGiverStatusVanilla.ToString());
	}

	public static InventoryResult ConvertInventoryResult(uint result)
	{
		if (RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			Type typeFromHandle = typeof(InventoryResult);
			InventoryResultVanilla inventoryResultVanilla = (InventoryResultVanilla)result;
			return (InventoryResult)Enum.Parse(typeFromHandle, inventoryResultVanilla.ToString());
		}
		if (RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			Type typeFromHandle2 = typeof(InventoryResult);
			InventoryResultTBC inventoryResultTBC = (InventoryResultTBC)result;
			return (InventoryResult)Enum.Parse(typeFromHandle2, inventoryResultTBC.ToString());
		}
		return (InventoryResult)result;
	}

	public static int GetQuestLogSize()
	{
		if (!AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			return 20;
		}
		return 25;
	}

	public static int GetAuraSlotsCount()
	{
		if (!AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			return 48;
		}
		return 56;
	}
}
