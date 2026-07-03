using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Bgs.Protocol;
using Bgs.Protocol.GameUtilities.V1;
using Framework;
using Framework.Constants;
using Framework.Logging;
using Framework.Realm;
using Framework.Serialization;
using Framework.Util;
using Framework.Web;
using Google.Protobuf;
using HermesProxy;
using HermesProxy.Auth;
using HermesProxy.World;

public class RealmManager
{
	private const int placeholderRegion = 1;

	private const int placeholderBattlegroup = 1;

	private List<RealmBuildInfo> _builds = new List<RealmBuildInfo>();

	private ConcurrentDictionary<RealmId, Realm> _realms = new ConcurrentDictionary<RealmId, Realm>();

	private List<string> _subRegions = new List<string>();

	public RealmManager()
	{
		LoadBuildInfo();
		string addressString = new RealmId(1, 1, 0u).GetAddressString();
		if (!_subRegions.Contains(addressString))
		{
			_subRegions.Add(addressString);
		}
	}

	private void LoadBuildInfo()
	{
		RealmBuildInfo realmBuildInfo = new RealmBuildInfo();
		realmBuildInfo.MajorVersion = ModernVersion.ExpansionVersion;
		realmBuildInfo.MinorVersion = ModernVersion.MajorVersion;
		realmBuildInfo.BugfixVersion = ModernVersion.MinorVersion;
		string text = "";
		if (!text.IsEmpty() && text.Length < realmBuildInfo.HotfixVersion.Length)
		{
			realmBuildInfo.HotfixVersion = text.ToCharArray();
		}
		realmBuildInfo.Build = (uint)Settings.ClientBuild;
		realmBuildInfo.FallbackStaticSeed = Settings.ClientSeed;
		realmBuildInfo.BuildSeeds = GameData.BuildAuthSeeds.GetValueOrDefault(realmBuildInfo.Build, new Dictionary<string, byte[]>());
		_builds.Add(realmBuildInfo);
	}

	private void UpdateRealm(Realm realm)
	{
		Realm realm2 = _realms.LookupByKey(realm.Id);
		if (realm2 == null || realm2 != realm)
		{
			_realms[realm.Id] = realm;
		}
	}

	public void AddRealm(uint id, string name, string externalAddress, ushort port, RealmType type, RealmFlags flags, byte characterCount, byte timezone, float populationLevel)
	{
		Dictionary<RealmId, string> dictionary = new Dictionary<RealmId, string>();
		foreach (KeyValuePair<RealmId, Realm> realm2 in _realms)
		{
			dictionary[realm2.Key] = realm2.Value.Name;
		}
		Realm realm = new Realm();
		realm.Name = name;
		realm.ExternalAddress = externalAddress;
		realm.Port = port;
		RealmType realmType = type;
		if (realmType == RealmType.FFAPVP)
		{
			realmType = RealmType.PVP;
		}
		if (realmType >= RealmType.MaxType)
		{
			realmType = RealmType.Normal;
		}
		realm.Type = (byte)realmType;
		realm.Flags = flags;
		realm.CharacterCount = characterCount;
		realm.Timezone = timezone;
		realm.PopulationLevel = populationLevel;
		realm.Build = (uint)Settings.ClientBuild;
		realm.Id = new RealmId(1, 1, id);
		UpdateRealm(realm);
		if (!dictionary.ContainsKey(realm.Id))
		{
			Log.Print(LogType.Server, $"Added realm \"{realm.Name}\" at {realm.ExternalAddress}:{realm.Port}", "AddRealm", "D:\\a\\HermesProxy\\HermesProxy\\Realm\\RealmManager.cs");
		}
		else
		{
			Log.Print(LogType.Server, $"Updating realm \"{realm.Name}\" at {realm.ExternalAddress}:{realm.Port}", "AddRealm", "D:\\a\\HermesProxy\\HermesProxy\\Realm\\RealmManager.cs");
		}
		dictionary.Remove(realm.Id);
	}

	public void UpdateRealms(List<RealmInfo> authRealmList)
	{
		_realms.Clear();
		if (authRealmList == null)
		{
			return;
		}
		foreach (RealmInfo authRealm in authRealmList)
		{
			AddRealm(authRealm.ID, authRealm.Name, authRealm.Address, authRealm.Port, authRealm.Type, authRealm.Flags, authRealm.CharacterCount, authRealm.Timezone, authRealm.Population);
		}
	}

	public void PrintRealmList()
	{
		if (_realms.Count == 0)
		{
			return;
		}
		Log.Print(LogType.Debug, "", "PrintRealmList", "D:\\a\\HermesProxy\\HermesProxy\\Realm\\RealmManager.cs");
		Log.Print(LogType.Debug, $"{"Type",-5} {"Type",-5} {"Locked",-8} {"Flags",-10} {"Name",-15} {"Address",-15} {"Port",-10} {"Build",-10}", "PrintRealmList", "D:\\a\\HermesProxy\\HermesProxy\\Realm\\RealmManager.cs");
		foreach (KeyValuePair<RealmId, Realm> realm in _realms)
		{
			Log.Print(LogType.Debug, realm.ToString(), "PrintRealmList", "D:\\a\\HermesProxy\\HermesProxy\\Realm\\RealmManager.cs");
		}
		Log.Print(LogType.Debug, "", "PrintRealmList", "D:\\a\\HermesProxy\\HermesProxy\\Realm\\RealmManager.cs");
	}

	public Realm? GetRealm(RealmId id)
	{
		return _realms.LookupByKey(id);
	}

	public RealmBuildInfo GetBuildInfo(uint build)
	{
		foreach (RealmBuildInfo build2 in _builds)
		{
			if (build2.Build == build)
			{
				return build2;
			}
		}
		return null;
	}

	public uint GetMinorMajorBugfixVersionForBuild(uint build)
	{
		RealmBuildInfo realmBuildInfo = _builds.FirstOrDefault((RealmBuildInfo p) => p.Build < build);
		if (realmBuildInfo == null)
		{
			return 0u;
		}
		return realmBuildInfo.MajorVersion * 10000 + realmBuildInfo.MinorVersion * 100 + realmBuildInfo.BugfixVersion;
	}

	public void WriteSubRegions(GetAllValuesForAttributeResponse response)
	{
		foreach (string subRegion in GetSubRegions())
		{
			Variant variant = new Variant();
			variant.StringValue = subRegion;
			response.AttributeValue.Add(variant);
		}
	}

	public byte[] GetCompressdRealmEntryJSON(Realm realm, uint build)
	{
		byte[] result = new byte[0];
		if (realm != null && !realm.Flags.HasAnyFlag(RealmFlags.Offline) && realm.Build == build)
		{
			RealmEntry realmEntry = new RealmEntry();
			realmEntry.WowRealmAddress = (int)realm.Id.GetAddress();
			realmEntry.CfgTimezonesID = 1;
			realmEntry.PopulationState = Math.Max((int)realm.PopulationLevel, 1);
			realmEntry.CfgCategoriesID = realm.Timezone;
			ClientVersion clientVersion = new ClientVersion();
			RealmBuildInfo buildInfo = GetBuildInfo(realm.Build);
			if (buildInfo != null)
			{
				clientVersion.Major = (int)buildInfo.MajorVersion;
				clientVersion.Minor = (int)buildInfo.MinorVersion;
				clientVersion.Revision = (int)buildInfo.BugfixVersion;
				clientVersion.Build = (int)buildInfo.Build;
			}
			else
			{
				clientVersion.Major = 6;
				clientVersion.Minor = 2;
				clientVersion.Revision = 4;
				clientVersion.Build = (int)realm.Build;
			}
			realmEntry.Version = clientVersion;
			realmEntry.CfgRealmsID = (int)realm.Id.Index;
			realmEntry.Flags = (int)realm.Flags;
			realmEntry.Name = realm.Name;
			realmEntry.CfgConfigsID = (int)realm.GetConfigId();
			realmEntry.CfgLanguagesID = 1;
			result = Json.Deflate("JamJSONRealmEntry", realmEntry);
		}
		return result;
	}

	public byte[] GetRealmList(uint build, string subRegion)
	{
		RealmListUpdates realmListUpdates = new RealmListUpdates();
		foreach (KeyValuePair<RealmId, Realm> realm in _realms)
		{
			if (!(realm.Value.Id.GetSubRegionAddress() != subRegion))
			{
				RealmFlags realmFlags = realm.Value.Flags;
				if (realm.Value.Build != build)
				{
					realmFlags |= RealmFlags.VersionMismatch;
				}
				RealmListUpdate realmListUpdate = new RealmListUpdate();
				realmListUpdate.Update.WowRealmAddress = (int)realm.Value.Id.GetAddress();
				realmListUpdate.Update.CfgTimezonesID = 1;
				realmListUpdate.Update.PopulationState = ((!realm.Value.Flags.HasAnyFlag(RealmFlags.Offline)) ? Math.Max((int)realm.Value.PopulationLevel, 1) : 0);
				realmListUpdate.Update.CfgCategoriesID = realm.Value.Timezone;
				RealmBuildInfo buildInfo = GetBuildInfo(realm.Value.Build);
				if (buildInfo != null)
				{
					realmListUpdate.Update.Version.Major = (int)buildInfo.MajorVersion;
					realmListUpdate.Update.Version.Minor = (int)buildInfo.MinorVersion;
					realmListUpdate.Update.Version.Revision = (int)buildInfo.BugfixVersion;
					realmListUpdate.Update.Version.Build = (int)buildInfo.Build;
				}
				else
				{
					realmListUpdate.Update.Version.Major = 7;
					realmListUpdate.Update.Version.Minor = 1;
					realmListUpdate.Update.Version.Revision = 0;
					realmListUpdate.Update.Version.Build = (int)realm.Value.Build;
				}
				realmListUpdate.Update.CfgRealmsID = (int)realm.Value.Id.Index;
				realmListUpdate.Update.Flags = (int)realmFlags;
				realmListUpdate.Update.Name = realm.Value.Name;
				realmListUpdate.Update.CfgConfigsID = (int)realm.Value.GetConfigId();
				realmListUpdate.Update.CfgLanguagesID = 1;
				realmListUpdate.Deleting = false;
				realmListUpdates.Updates.Add(realmListUpdate);
			}
		}
		return Json.Deflate("JSONRealmListUpdates", realmListUpdates);
	}

	public BattlenetRpcErrorCode JoinRealm(GlobalSessionData globalSession, uint realmAddress, uint build, IPAddress clientAddress, byte[] clientSecret, string accountName, ClientResponse response)
	{
		globalSession.RealmId = new RealmId(realmAddress);
		Realm realm = GetRealm(globalSession.RealmId);
		if (realm != null)
		{
			if (realm.Flags.HasAnyFlag(RealmFlags.Offline) || realm.Build != build)
			{
				return BattlenetRpcErrorCode.UserServerNotPermittedOnRealm;
			}
			RealmListServerIPAddresses realmListServerIPAddresses = new RealmListServerIPAddresses();
			AddressFamily addressFamily = new AddressFamily();
			addressFamily.Id = 1;
			Framework.Web.Address address = new Framework.Web.Address();
			address.Ip = realm.GetAddressForClient(clientAddress).Address.ToString();
			address.Port = Settings.RealmPort;
			addressFamily.Addresses.Add(address);
			realmListServerIPAddresses.Families.Add(addressFamily);
			byte[] bytes = Json.Deflate("JSONRealmListServerIPAddresses", realmListServerIPAddresses);
			byte[] array = new byte[0].GenerateRandomKey(32);
			byte[] sessionKey = clientSecret.ToArray().Combine(array);
			globalSession.SessionKey = sessionKey;
			response.Attribute.AddBlob("Param_RealmJoinTicket", ByteString.CopyFrom(accountName, Encoding.UTF8));
			response.Attribute.AddBlob("Param_ServerAddresses", ByteString.CopyFrom(bytes));
			response.Attribute.AddBlob("Param_JoinSecret", ByteString.CopyFrom(array));
			return BattlenetRpcErrorCode.Ok;
		}
		return BattlenetRpcErrorCode.UtilServerUnknownRealm;
	}

	public ICollection<Realm> GetRealms()
	{
		return _realms.Values;
	}

	private List<string> GetSubRegions()
	{
		return _subRegions;
	}
}
