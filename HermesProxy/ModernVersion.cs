using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Framework;
using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Enums.V1_14_1_40688;
using HermesProxy.World.Enums.V2_5_2_39570;

namespace HermesProxy;

public static class ModernVersion
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

	static ModernVersion()
	{
		CurrentToUniversalOpcodeDictionary = new Dictionary<uint, HermesProxy.World.Enums.Opcode>();
		UniversalToCurrentOpcodeDictionary = new Dictionary<HermesProxy.World.Enums.Opcode, uint>();
		Build = Settings.ClientBuild;
		ExpansionVersion = GetExpansionVersion();
		MajorVersion = GetMajorPatchVersion();
		MinorVersion = GetMinorPatchVersion();
		UpdateFieldDictionary = new Dictionary<Type, SortedList<int, UpdateFieldInfo>>();
		UpdateFieldNameDictionary = new Dictionary<Type, Dictionary<string, int>>();
		if (!LoadUFDictionariesInto(UpdateFieldDictionary, UpdateFieldNameDictionary))
		{
			Log.Print(LogType.Error, "Could not load update fields for current modern version.", ".cctor", "D:\\a\\HermesProxy\\HermesProxy\\VersionChecker.cs");
		}
		if (!LoadOpcodeDictionaries())
		{
			Log.Print(LogType.Error, "Could not load opcodes for current modern version.", ".cctor", "D:\\a\\HermesProxy\\HermesProxy\\VersionChecker.cs");
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
		Log.Print(LogType.Server, $"Loaded {CurrentToUniversalOpcodeDictionary.Count} modern opcodes.", "LoadOpcodeDictionaries", "D:\\a\\HermesProxy\\HermesProxy\\VersionChecker.cs");
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
		case ClientVersionBuild.V1_14_0_39802:
		case ClientVersionBuild.V1_14_0_39958:
		case ClientVersionBuild.V1_14_0_40140:
		case ClientVersionBuild.V1_14_0_40179:
		case ClientVersionBuild.V1_14_0_40237:
		case ClientVersionBuild.V1_14_0_40347:
		case ClientVersionBuild.V1_14_0_40441:
		case ClientVersionBuild.V1_14_0_40618:
			return ClientVersionBuild.V1_14_0_40237;
		case ClientVersionBuild.V1_14_1_40487:
		case ClientVersionBuild.V1_14_1_40594:
		case ClientVersionBuild.V1_14_1_40666:
		case ClientVersionBuild.V1_14_1_40688:
		case ClientVersionBuild.V1_14_1_40800:
		case ClientVersionBuild.V1_14_1_40818:
		case ClientVersionBuild.V1_14_1_40926:
		case ClientVersionBuild.V1_14_1_40962:
		case ClientVersionBuild.V1_14_1_41009:
		case ClientVersionBuild.V1_14_1_41030:
		case ClientVersionBuild.V1_14_1_41077:
		case ClientVersionBuild.V1_14_1_41137:
		case ClientVersionBuild.V1_14_1_41243:
		case ClientVersionBuild.V1_14_1_41511:
		case ClientVersionBuild.V1_14_1_41794:
		case ClientVersionBuild.V1_14_2_41858:
		case ClientVersionBuild.V1_14_2_41959:
		case ClientVersionBuild.V1_14_1_42032:
		case ClientVersionBuild.V1_14_2_42065:
		case ClientVersionBuild.V1_14_2_42082:
		case ClientVersionBuild.V1_14_2_42214:
		case ClientVersionBuild.V1_14_2_42597:
			return ClientVersionBuild.V1_14_1_40688;
		case ClientVersionBuild.V2_5_2_39570:
		case ClientVersionBuild.V2_5_2_39618:
		case ClientVersionBuild.V2_5_2_39926:
		case ClientVersionBuild.V2_5_2_40011:
		case ClientVersionBuild.V2_5_2_40045:
		case ClientVersionBuild.V2_5_2_40203:
		case ClientVersionBuild.V2_5_2_40260:
		case ClientVersionBuild.V2_5_2_40422:
		case ClientVersionBuild.V2_5_2_40488:
		case ClientVersionBuild.V2_5_2_40617:
		case ClientVersionBuild.V2_5_2_40892:
		case ClientVersionBuild.V2_5_2_41446:
		case ClientVersionBuild.V2_5_2_41510:
			return ClientVersionBuild.V2_5_2_39570;
		case ClientVersionBuild.V2_5_3_41402:
		case ClientVersionBuild.V2_5_3_41531:
		case ClientVersionBuild.V2_5_3_41750:
		case ClientVersionBuild.V2_5_3_41812:
		case ClientVersionBuild.V2_5_3_42083:
		case ClientVersionBuild.V2_5_3_42328:
		case ClientVersionBuild.V2_5_3_42598:
			return ClientVersionBuild.V2_5_3_41750;
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
			typeof(HermesProxy.World.Enums.ActivePlayerField),
			typeof(HermesProxy.World.Enums.GameObjectField),
			typeof(HermesProxy.World.Enums.DynamicObjectField),
			typeof(HermesProxy.World.Enums.CorpseField),
			typeof(HermesProxy.World.Enums.AreaTriggerField),
			typeof(HermesProxy.World.Enums.SceneObjectField),
			typeof(HermesProxy.World.Enums.ConversationField),
			typeof(HermesProxy.World.Enums.ObjectDynamicField),
			typeof(HermesProxy.World.Enums.ItemDynamicField),
			typeof(HermesProxy.World.Enums.ContainerDynamicField),
			typeof(AzeriteEmpoweredItemDynamicField),
			typeof(AzeriteItemDynamicField),
			typeof(HermesProxy.World.Enums.UnitDynamicField),
			typeof(HermesProxy.World.Enums.PlayerDynamicField),
			typeof(HermesProxy.World.Enums.ActivePlayerDynamicField),
			typeof(HermesProxy.World.Enums.GameObjectDynamicField),
			typeof(HermesProxy.World.Enums.DynamicObjectDynamicField),
			typeof(HermesProxy.World.Enums.CorpseDynamicField),
			typeof(HermesProxy.World.Enums.AreaTriggerDynamicField),
			typeof(HermesProxy.World.Enums.SceneObjectDynamicField),
			typeof(HermesProxy.World.Enums.ConversationDynamicField)
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
		case ClientVersionBuild.V2_5_2_39570:
			return typeof(HermesProxy.World.Enums.V2_5_2_39570.ResponseCodes);
		case ClientVersionBuild.V1_14_1_40688:
		case ClientVersionBuild.V2_5_3_41750:
			return typeof(HermesProxy.World.Enums.V1_14_1_40688.ResponseCodes);
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

	public static bool AddedInVersion(byte expansion, byte major, byte minor)
	{
		if (ExpansionVersion < expansion)
		{
			return false;
		}
		if (ExpansionVersion > expansion)
		{
			return true;
		}
		if (MajorVersion < major)
		{
			return false;
		}
		if (MajorVersion > major)
		{
			return true;
		}
		return MinorVersion >= minor;
	}

	public static bool AddedInVersion(byte retailExpansion, byte retailMajor, byte retailMinor, byte classicEraExpansion, byte classicEraMajor, byte classicEraMinor, byte classicExpansion, byte classicMajor, byte classicMinor)
	{
		if (ExpansionVersion == 1)
		{
			return AddedInVersion(classicEraExpansion, classicEraMajor, classicEraMinor);
		}
		if (ExpansionVersion == 2 || ExpansionVersion == 3)
		{
			return AddedInVersion(classicExpansion, classicMajor, classicMinor);
		}
		return AddedInVersion(retailExpansion, retailMajor, retailMinor);
	}

	public static bool RemovedInVersion(byte retailExpansion, byte retailMajor, byte retailMinor, byte classicEraExpansion, byte classicEraMajor, byte classicEraMinor, byte classicExpansion, byte classicMajor, byte classicMinor)
	{
		return !AddedInVersion(retailExpansion, retailMajor, retailMinor, classicEraExpansion, classicEraMajor, classicEraMinor, classicExpansion, classicMajor, classicMinor);
	}

	public static bool AddedInClassicVersion(byte classicEraExpansion, byte classicEraMajor, byte classicEraMinor, byte classicExpansion, byte classicMajor, byte classicMinor)
	{
		if (ExpansionVersion == 1)
		{
			return AddedInVersion(classicEraExpansion, classicEraMajor, classicEraMinor);
		}
		if (ExpansionVersion == 2 || ExpansionVersion == 3)
		{
			return AddedInVersion(classicExpansion, classicMajor, classicMinor);
		}
		return false;
	}

	public static bool RemovedInClassicVersion(byte classicEraExpansion, byte classicEraMajor, byte classicEraMinor, byte classicExpansion, byte classicMajor, byte classicMinor)
	{
		return !AddedInClassicVersion(classicEraExpansion, classicEraMajor, classicEraMinor, classicExpansion, classicMajor, classicMinor);
	}

	public static bool IsVersion(byte expansion, byte major, byte minor)
	{
		if (ExpansionVersion == expansion && MajorVersion == major)
		{
			return MinorVersion == minor;
		}
		return false;
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

	public static bool IsClassicVersionBuild()
	{
		if ((ExpansionVersion != 1 || MajorVersion < 13) && (ExpansionVersion != 2 || MajorVersion < 5))
		{
			if (ExpansionVersion == 3)
			{
				return MajorVersion >= 4;
			}
			return false;
		}
		return true;
	}

	public static int GetAccountDataCount()
	{
		if (ExpansionVersion == 1 && MajorVersion >= 14)
		{
			if (AddedInVersion(1, 14, 1))
			{
				return 13;
			}
			return 10;
		}
		if ((ExpansionVersion == 2 && MajorVersion >= 5) || (ExpansionVersion == 3 && MajorVersion >= 4))
		{
			if (AddedInVersion(2, 5, 3))
			{
				return 13;
			}
		}
		else if (!IsClassicVersionBuild())
		{
			if (AddedInVersion(9, 2, 0))
			{
				return 13;
			}
			if (AddedInVersion(9, 1, 5))
			{
				return 12;
			}
		}
		return 8;
	}

	public static int GetPowerCountForClientVersion()
	{
		if (IsClassicVersionBuild())
		{
			if (AddedInClassicVersion(1, 14, 1, 2, 5, 3))
			{
				return 7;
			}
			return 6;
		}
		if (RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			return 5;
		}
		if (RemovedInVersion(ClientVersionBuild.V4_0_6_13596))
		{
			return 7;
		}
		if (RemovedInVersion(ClientVersionBuild.V6_0_2_19033))
		{
			return 5;
		}
		if (RemovedInVersion(ClientVersionBuild.V9_1_5_40772))
		{
			return 6;
		}
		return 7;
	}

	public static uint GetGameObjectStateAnimId()
	{
		if (IsVersion(1, 14, 0) || IsVersion(2, 5, 2))
		{
			return 1556u;
		}
		if (IsVersion(1, 14, 1))
		{
			return 1618u;
		}
		if (IsVersion(1, 14, 2) || IsVersion(2, 5, 3))
		{
			return 1672u;
		}
		return 0u;
	}

	public static byte AdjustInventorySlot(byte slot)
	{
		byte b = 0;
		if (slot >= 47 && slot < 75)
		{
			b = (byte)(LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180) ? 8 : ((!LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056)) ? 8 : 8));
		}
		else if (slot >= 75 && slot < 82)
		{
			b = (byte)(LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180) ? 12 : ((!LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056)) ? 8 : 8));
		}
		else if (slot >= 82 && slot < 94)
		{
			b = (byte)(LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180) ? 13 : ((!LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056)) ? 8 : 8));
		}
		else if (slot >= 94 && slot < 126)
		{
			b = (byte)(LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180) ? 13 : ((!LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056)) ? 8 : 8));
		}
		return (byte)(slot - b);
	}

	public static void ConvertAuraFlags(ushort oldFlags, byte slot, out AuraFlagsModern newFlags, out uint activeFlags)
	{
		if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			activeFlags = 0u;
			newFlags = AuraFlagsModern.None;
			if (slot >= 32)
			{
				newFlags |= AuraFlagsModern.Negative;
			}
			else
			{
				newFlags |= AuraFlagsModern.Positive;
			}
			if (oldFlags.HasAnyFlag(AuraFlagsVanilla.Cancelable))
			{
				newFlags |= AuraFlagsModern.Cancelable;
			}
			if (oldFlags.HasAnyFlag(AuraFlagsVanilla.EffectIndex0))
			{
				activeFlags |= 1u;
			}
			if (oldFlags.HasAnyFlag(AuraFlagsVanilla.EffectIndex1))
			{
				activeFlags |= 2u;
			}
			if (oldFlags.HasAnyFlag(AuraFlagsVanilla.EffectIndex2))
			{
				activeFlags |= 4u;
			}
			return;
		}
		if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			activeFlags = 1u;
			newFlags = AuraFlagsModern.None;
			if (oldFlags.HasAnyFlag(AuraFlagsTBC.NotCancelable))
			{
				newFlags |= AuraFlagsModern.Negative;
			}
			else if (oldFlags.HasAnyFlag(AuraFlagsTBC.Cancelable))
			{
				newFlags |= AuraFlagsModern.Cancelable | AuraFlagsModern.Positive;
			}
			else if (slot >= 40)
			{
				newFlags |= AuraFlagsModern.Negative;
			}
			if (oldFlags.HasAnyFlag(AuraFlagsTBC.EffectIndex0))
			{
				activeFlags |= 1u;
			}
			if (oldFlags.HasAnyFlag(AuraFlagsTBC.EffectIndex1))
			{
				activeFlags |= 2u;
			}
			if (oldFlags.HasAnyFlag(AuraFlagsTBC.EffectIndex2))
			{
				activeFlags |= 4u;
			}
			return;
		}
		activeFlags = 0u;
		newFlags = AuraFlagsModern.None;
		if (oldFlags.HasAnyFlag(AuraFlagsWotLK.Negative))
		{
			newFlags |= AuraFlagsModern.Negative;
		}
		else if (oldFlags.HasAnyFlag(AuraFlagsWotLK.Positive))
		{
			newFlags |= AuraFlagsModern.Cancelable | AuraFlagsModern.Positive;
		}
		if (oldFlags.HasAnyFlag(AuraFlagsWotLK.NoCaster))
		{
			newFlags |= AuraFlagsModern.NoCaster;
		}
		if (oldFlags.HasAnyFlag(AuraFlagsWotLK.Duration))
		{
			newFlags |= AuraFlagsModern.Duration;
		}
		if (oldFlags.HasAnyFlag(AuraFlagsWotLK.EffectIndex0))
		{
			activeFlags |= 1u;
		}
		if (oldFlags.HasAnyFlag(AuraFlagsWotLK.EffectIndex1))
		{
			activeFlags |= 2u;
		}
		if (oldFlags.HasAnyFlag(AuraFlagsWotLK.EffectIndex2))
		{
			activeFlags |= 4u;
		}
	}

	public static uint GetArenaTeamSizeFromIndex(uint index)
	{
		return index switch
		{
			0u => 2u, 
			1u => 3u, 
			2u => 5u, 
			_ => 0u, 
		};
	}

	public static uint GetArenaTeamIndexFromSize(uint size)
	{
		return size switch
		{
			2u => 0u, 
			3u => 1u, 
			5u => 2u, 
			_ => 0u, 
		};
	}

	public static byte ConvertResponseCodesValue(byte legacyValue)
	{
		string value = Enum.ToObject(LegacyVersion.GetResponseCodesEnum(), legacyValue).ToString();
		return (byte)Enum.Parse(GetResponseCodesEnum(), value);
	}

	public static byte ConvertSocketColor(byte legacyValue)
	{
		Type typeFromHandle = typeof(SocketColorModern);
		SocketColorLegacy socketColorLegacy = (SocketColorLegacy)legacyValue;
		return (byte)Enum.Parse(typeFromHandle, socketColorLegacy.ToString());
	}
}
