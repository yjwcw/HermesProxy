using HermesProxy.Enums;

namespace HermesProxy;

/*
 * °æ±¾¼ì²éÆ÷
 */
public static class VersionChecker
{
	public static bool IsSupportedLegacyVersion(ClientVersionBuild legacyVersion)
	{
		switch (legacyVersion)
		{
		case ClientVersionBuild.V1_12_1_5875:
		case ClientVersionBuild.V1_12_2_6005:
		case ClientVersionBuild.V1_12_3_6141:
			return true;
		case ClientVersionBuild.V2_4_3_8606:
			return true;
		case ClientVersionBuild.V3_3_5a_12340:
			return false;
		default:
			return false;
		}
	}

	public static bool IsSupportedModernVersion(ClientVersionBuild modernVersion)
	{
		switch (modernVersion)
		{
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
			return true;
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
			return true;
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
			return true;
		default:
			return false;
		}
	}

	public static ClientVersionBuild GetBestLegacyVersion(ClientVersionBuild modernVersion)
	{
		return GetExpansionVersion(modernVersion) switch
		{
			1 => ClientVersionBuild.V1_12_1_5875, 
			2 => ClientVersionBuild.V2_4_3_8606, 
			_ => ClientVersionBuild.Zero, 
		};
	}

	private static byte GetExpansionVersion(ClientVersionBuild version)
	{
		string text = version.ToString();
		text = text.Replace("V", "");
		text = text.Substring(0, text.IndexOf("_"));
		return (byte)uint.Parse(text);
	}
}
