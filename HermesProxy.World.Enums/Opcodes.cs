using System;
using HermesProxy.Enums;
using HermesProxy.World.Enums.V1_12_1_5875;
using HermesProxy.World.Enums.V1_14_1_40688;
using HermesProxy.World.Enums.V2_4_3_8606;
using HermesProxy.World.Enums.V2_5_2_39570;
using HermesProxy.World.Enums.V2_5_3_41750;
using HermesProxy.World.Enums.V3_3_5_12340;

namespace HermesProxy.World.Enums;

public static class Opcodes
{
	public static ClientVersionBuild GetOpcodesDefiningBuild(ClientVersionBuild version)
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
		case ClientVersionBuild.V2_5_2_39570:
		case ClientVersionBuild.V2_5_2_39618:
		case ClientVersionBuild.V1_14_0_39802:
		case ClientVersionBuild.V2_5_2_39926:
		case ClientVersionBuild.V1_14_0_39958:
		case ClientVersionBuild.V2_5_2_40011:
		case ClientVersionBuild.V2_5_2_40045:
		case ClientVersionBuild.V1_14_0_40140:
		case ClientVersionBuild.V1_14_0_40179:
		case ClientVersionBuild.V2_5_2_40203:
		case ClientVersionBuild.V1_14_0_40237:
		case ClientVersionBuild.V2_5_2_40260:
		case ClientVersionBuild.V1_14_0_40347:
		case ClientVersionBuild.V2_5_2_40422:
		case ClientVersionBuild.V1_14_0_40441:
		case ClientVersionBuild.V2_5_2_40488:
		case ClientVersionBuild.V2_5_2_40617:
		case ClientVersionBuild.V1_14_0_40618:
		case ClientVersionBuild.V2_5_2_40892:
		case ClientVersionBuild.V2_5_2_41446:
		case ClientVersionBuild.V2_5_2_41510:
			return ClientVersionBuild.V2_5_2_39570;
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
		case ClientVersionBuild.V1_14_1_42032:
			return ClientVersionBuild.V1_14_1_40688;
		case ClientVersionBuild.V2_5_3_41402:
		case ClientVersionBuild.V2_5_3_41531:
		case ClientVersionBuild.V2_5_3_41750:
		case ClientVersionBuild.V2_5_3_41812:
		case ClientVersionBuild.V1_14_2_41858:
		case ClientVersionBuild.V1_14_2_41959:
		case ClientVersionBuild.V1_14_2_42065:
		case ClientVersionBuild.V1_14_2_42082:
		case ClientVersionBuild.V2_5_3_42083:
		case ClientVersionBuild.V1_14_2_42214:
		case ClientVersionBuild.V2_5_3_42328:
		case ClientVersionBuild.V1_14_2_42597:
		case ClientVersionBuild.V2_5_3_42598:
			return ClientVersionBuild.V2_5_3_41750;
		default:
			return ClientVersionBuild.Zero;
		}
	}

	public static Type GetOpcodesEnumForVersion(ClientVersionBuild version)
	{
		return GetOpcodesDefiningBuild(version) switch
		{
			ClientVersionBuild.V1_12_1_5875 => typeof(HermesProxy.World.Enums.V1_12_1_5875.Opcode), 
			ClientVersionBuild.V2_4_3_8606 => typeof(HermesProxy.World.Enums.V2_4_3_8606.Opcode), 
			ClientVersionBuild.V3_3_5a_12340 => typeof(HermesProxy.World.Enums.V3_3_5_12340.Opcode), 
			ClientVersionBuild.V2_5_2_39570 => typeof(HermesProxy.World.Enums.V2_5_2_39570.Opcode), 
			ClientVersionBuild.V2_5_3_41750 => typeof(HermesProxy.World.Enums.V2_5_3_41750.Opcode), 
			ClientVersionBuild.V1_14_1_40688 => typeof(HermesProxy.World.Enums.V1_14_1_40688.Opcode), 
			_ => null, 
		};
	}

	public static uint GetOpcodeValueForVersion(Opcode opcode, ClientVersionBuild version)
	{
		return GetOpcodeValueForVersion(opcode.ToString(), version);
	}

	public static uint GetOpcodeValueForVersion(string opcodeName, ClientVersionBuild version)
	{
		if (Enum.TryParse(GetOpcodesEnumForVersion(version), opcodeName, out object result))
		{
			return (uint)result;
		}
		return 0u;
	}

	public static string GetOpcodeNameForVersion(uint opcode, ClientVersionBuild version)
	{
		return Enum.ToObject(GetOpcodesEnumForVersion(version), opcode).ToString();
	}

	public static Opcode GetUniversalOpcode(uint opcode, ClientVersionBuild version)
	{
		return GetUniversalOpcode(GetOpcodeNameForVersion(opcode, version));
	}

	public static Opcode GetUniversalOpcode(string name)
	{
		if (Enum.TryParse(typeof(Opcode), name, out object result))
		{
			return (Opcode)result;
		}
		return Opcode.MSG_NULL_ACTION;
	}

	private static uint FindOpcodeValueInEnum<T>(string name) where T : Enum
	{
		foreach (object value in Enum.GetValues(typeof(T)))
		{
			if (Enum.GetName(typeof(T), value) == name)
			{
				return (uint)value;
			}
		}
		return 0u;
	}

	private static string FindOpcodeNameInEnum<T>(uint value) where T : Enum
	{
		foreach (object value2 in Enum.GetValues(typeof(T)))
		{
			if ((uint)value2 == value)
			{
				return Enum.GetName(typeof(T), value2);
			}
		}
		return "UNKNOWN";
	}
}
