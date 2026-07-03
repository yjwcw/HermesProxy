using System;
using System.Collections.Generic;
using System.Threading;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Client;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server;
using HermesProxy.World.Server.Packets;

namespace HermesProxy;

public class GameSessionData
{
	public bool HasWsgHordeFlagCarrier;

	public bool HasWsgAllyFlagCarrier;

	public bool ChannelDisplayList;

	public bool ShowPlayedTime;

	public bool IsInFarSight;

	public bool IsInTaxiFlight;

	public bool IsWaitingForTaxiStart;

	public bool IsWaitingForNewWorld;

	public bool IsWaitingForWorldPortAck;

	public bool IsFirstEnterWorld;

	public bool IsConnectedToInstance;

	public Queue<ServerPacket> PendingUninstancedPackets = new Queue<ServerPacket>();

	public bool IsInWorld;

	public uint? CurrentMapId;

	public uint CurrentZoneId;

	public uint CurrentTaxiNode;

	public List<byte> UsableTaxiNodes = new List<byte>();

	public uint PendingTransferMapId;

	public uint LastEnteredAreaTrigger;

	public uint LastDispellSpellId;

	public string LeftChannelName = "";

	public bool IsPassingOnLoot;

	public int GroupUpdateCounter;

	public uint GroupReadyCheckResponses;

	public PartyUpdate[] CurrentGroups = new PartyUpdate[2];

	public bool WeWantToLeaveGroup;

	public List<OwnCharacterInfo> OwnCharacters = new List<OwnCharacterInfo>();

	public WowGuid128 CurrentPlayerGuid;

	public long CurrentPlayerCreateTime;

	public OwnCharacterInfo CurrentPlayerInfo;

	public CurrentPlayerStorage CurrentPlayerStorage;

	public uint CurrentGuildCreateTime;

	public uint CurrentGuildNumAccounts;

	public WowGuid128 CurrentInteractedWithNPC;

	public WowGuid128 CurrentInteractedWithGO;

	public uint LastWhoRequestId;

	public WowGuid128 CurrentPetGuid;

	public uint[] CurrentArenaTeamIds = new uint[3];

	public ClientCastRequest CurrentClientNormalCast;

	public ClientCastRequest CurrentClientSpecialCast;

	public ClientCastRequest CurrentClientPetCast;

	public List<ClientCastRequest> PendingClientCasts = new List<ClientCastRequest>();

	public List<ClientCastRequest> PendingClientPetCasts = new List<ClientCastRequest>();

	public WowGuid64 LastLootTargetGuid;

	public List<int> ActionButtons = new List<int>();

	public Dictionary<WowGuid128, Dictionary<byte, int>> UnitAuraDurationUpdateTime = new Dictionary<WowGuid128, Dictionary<byte, int>>();

	public Dictionary<WowGuid128, Dictionary<byte, int>> UnitAuraDurationLeft = new Dictionary<WowGuid128, Dictionary<byte, int>>();

	public Dictionary<WowGuid128, Dictionary<byte, int>> UnitAuraDurationFull = new Dictionary<WowGuid128, Dictionary<byte, int>>();

	public Dictionary<WowGuid128, Dictionary<byte, WowGuid128>> UnitAuraCaster = new Dictionary<WowGuid128, Dictionary<byte, WowGuid128>>();

	public Dictionary<WowGuid128, PlayerCache> CachedPlayers = new Dictionary<WowGuid128, PlayerCache>();

	public HashSet<WowGuid128> IgnoredPlayers = new HashSet<WowGuid128>();

	public Dictionary<WowGuid128, uint> PlayerGuildIds = new Dictionary<WowGuid128, uint>();

	public Mutex ObjectCacheMutex = new Mutex();

	public Dictionary<WowGuid128, Dictionary<int, UpdateField>> ObjectCacheLegacy = new Dictionary<WowGuid128, Dictionary<int, UpdateField>>();

	public Dictionary<WowGuid128, UpdateFieldsArray> ObjectCacheModern = new Dictionary<WowGuid128, UpdateFieldsArray>();

	public Dictionary<WowGuid128, ObjectType> OriginalObjectTypes = new Dictionary<WowGuid128, ObjectType>();

	public Dictionary<WowGuid128, uint[]> ItemGems = new Dictionary<WowGuid128, uint[]>();

	public Dictionary<uint, Class> CreatureClasses = new Dictionary<uint, Class>();

	public Dictionary<string, int> ChannelIds = new Dictionary<string, int>();

	public Dictionary<uint, uint> ItemBuyCount = new Dictionary<uint, uint>();

	public Dictionary<uint, uint> RealSpellToLearnSpell = new Dictionary<uint, uint>();

	public Dictionary<uint, ArenaTeamData> ArenaTeams = new Dictionary<uint, ArenaTeamData>();

	public MailListResult PendingMailListPacket;

	public HashSet<uint> RequestedItemTextIds = new HashSet<uint>();

	public Dictionary<uint, string> ItemTexts = new Dictionary<uint, string>();

	public Dictionary<uint, uint> BattleFieldQueueTypes = new Dictionary<uint, uint>();

	public Dictionary<uint, long> BattleFieldQueueTimes = new Dictionary<uint, long>();

	public Dictionary<uint, uint> DailyQuestsDone = new Dictionary<uint, uint>();

	public HashSet<WowGuid128> FlagCarrierGuids = new HashSet<WowGuid128>();

	public Dictionary<WowGuid64, ushort> ObjectSpawnCount = new Dictionary<WowGuid64, ushort>();

	public HashSet<WowGuid64> DespawnedGameObjects = new HashSet<WowGuid64>();

	public HashSet<WowGuid128> HunterPetGuids = new HashSet<WowGuid128>();

	public Dictionary<WowGuid128, Array<ArenaTeamInspectData>> PlayerArenaTeams = new Dictionary<WowGuid128, Array<ArenaTeamInspectData>>();

	public HashSet<string> AddonPrefixes = new HashSet<string>();

	public Dictionary<byte, Dictionary<byte, int>> FlatSpellMods = new Dictionary<byte, Dictionary<byte, int>>();

	public Dictionary<byte, Dictionary<byte, int>> PctSpellMods = new Dictionary<byte, Dictionary<byte, int>>();

	public Dictionary<WowGuid128, Dictionary<uint, WowGuid128>> LastAuraCasterOnTarget = new Dictionary<WowGuid128, Dictionary<uint, WowGuid128>>();

	public TradeSession? CurrentTrade;

	public HashSet<uint> RequestedItemHotfixes = new HashSet<uint>();

	public HashSet<uint> RequestedItemSparseHotfixes = new HashSet<uint>();

	private GameSessionData()
	{
	}

	public static GameSessionData CreateNewGameSessionData(GlobalSessionData globalSession)
	{
		return new GameSessionData
		{
			CurrentPlayerStorage = new CurrentPlayerStorage(globalSession)
		};
	}

	public uint GetCurrentGroupSize()
	{
		PartyUpdate currentGroup = GetCurrentGroup();
		if (currentGroup == null)
		{
			return 0u;
		}
		if (currentGroup.PlayerList.Count <= 1)
		{
			return 0u;
		}
		return (uint)(currentGroup.PlayerList.Count - 1);
	}

	public WowGuid128 GetCurrentGroupLeader()
	{
		PartyUpdate currentGroup = GetCurrentGroup();
		if (currentGroup == null)
		{
			return WowGuid128.Empty;
		}
		return currentGroup.LeaderGUID;
	}

	public LootMethod GetCurrentLootMethod()
	{
		return GetCurrentGroup()?.LootSettings.Method ?? LootMethod.FreeForAll;
	}

	public WowGuid128 GetCurrentGroupGuid()
	{
		PartyUpdate currentGroup = GetCurrentGroup();
		if (currentGroup == null)
		{
			return WowGuid128.Empty;
		}
		return currentGroup.PartyGUID;
	}

	public PartyUpdate GetCurrentGroup()
	{
		return CurrentGroups[GetCurrentPartyIndex()];
	}

	public sbyte GetCurrentPartyIndex()
	{
		return (sbyte)(IsInBattleground() ? 1 : 0);
	}

	public byte GetItemSpellSlot(WowGuid128 guid, uint spellId)
	{
		int updateField = LegacyVersion.GetUpdateField(ObjectField.OBJECT_FIELD_ENTRY);
		if (updateField < 0)
		{
			return 0;
		}
		Dictionary<int, UpdateField> cachedObjectFieldsLegacy = GetCachedObjectFieldsLegacy(guid);
		if (cachedObjectFieldsLegacy == null)
		{
			return 0;
		}
		return GameData.GetItemEffectSlot(cachedObjectFieldsLegacy[updateField].UInt32Value, spellId);
	}

	public uint GetItemId(WowGuid128 guid)
	{
		int updateField = LegacyVersion.GetUpdateField(ObjectField.OBJECT_FIELD_ENTRY);
		if (updateField < 0)
		{
			return 0u;
		}
		return GetCachedObjectFieldsLegacy(guid)?[updateField].UInt32Value ?? 0;
	}

	public void SetFlatSpellMod(byte spellMod, byte spellMask, int amount)
	{
		if (FlatSpellMods.ContainsKey(spellMod))
		{
			if (FlatSpellMods[spellMod].ContainsKey(spellMask))
			{
				FlatSpellMods[spellMod][spellMask] = amount;
			}
			else
			{
				FlatSpellMods[spellMod].Add(spellMask, amount);
			}
		}
		else
		{
			Dictionary<byte, int> dictionary = new Dictionary<byte, int>();
			dictionary.Add(spellMask, amount);
			FlatSpellMods.Add(spellMod, dictionary);
		}
	}

	public void SetPctSpellMod(byte spellMod, byte spellMask, int amount)
	{
		if (PctSpellMods.ContainsKey(spellMod))
		{
			if (PctSpellMods[spellMod].ContainsKey(spellMask))
			{
				PctSpellMods[spellMod][spellMask] = amount;
			}
			else
			{
				PctSpellMods[spellMod].Add(spellMask, amount);
			}
		}
		else
		{
			Dictionary<byte, int> dictionary = new Dictionary<byte, int>();
			dictionary.Add(spellMask, amount);
			PctSpellMods.Add(spellMod, dictionary);
		}
	}

	public ArenaTeamInspectData GetArenaTeamDataForPlayer(WowGuid128 guid, byte slot)
	{
		if (PlayerArenaTeams.ContainsKey(guid))
		{
			return PlayerArenaTeams[guid][slot];
		}
		return new ArenaTeamInspectData();
	}

	public void StoreArenaTeamDataForPlayer(WowGuid128 guid, byte slot, ArenaTeamInspectData team)
	{
		if (!PlayerArenaTeams.ContainsKey(guid))
		{
			PlayerArenaTeams.Add(guid, new Array<ArenaTeamInspectData>(3, new ArenaTeamInspectData()));
		}
		PlayerArenaTeams[guid][slot] = team;
	}

	public WowGuid64 GetInventorySlotItem(int slot)
	{
		int updateField = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_INV_SLOT_HEAD);
		if (updateField >= 0)
		{
			Dictionary<int, UpdateField> cachedObjectFieldsLegacy = GetCachedObjectFieldsLegacy(CurrentPlayerGuid);
			if (cachedObjectFieldsLegacy != null)
			{
				return cachedObjectFieldsLegacy.GetGuidValue(updateField + slot * 2).To64();
			}
		}
		return WowGuid64.Empty;
	}

	public ushort GetObjectSpawnCounter(WowGuid64 guid)
	{
		if (ObjectSpawnCount.TryGetValue(guid, out var value))
		{
			return value;
		}
		return 0;
	}

	public void IncrementObjectSpawnCounter(WowGuid64 guid)
	{
		if (ObjectSpawnCount.ContainsKey(guid))
		{
			ObjectSpawnCount[guid]++;
		}
		else
		{
			ObjectSpawnCount.Add(guid, 0);
		}
	}

	public void SetDailyQuestSlot(uint slot, uint questId)
	{
		if (DailyQuestsDone.ContainsKey(slot))
		{
			if (questId != 0)
			{
				DailyQuestsDone[slot] = questId;
			}
			else
			{
				DailyQuestsDone.Remove(slot);
			}
		}
		else if (questId != 0)
		{
			DailyQuestsDone.Add(slot, questId);
		}
	}

	public bool IsAlliancePlayer(WowGuid128 guid)
	{
		if (CachedPlayers.TryGetValue(guid, out var value))
		{
			return GameData.IsAllianceRace(value.RaceId);
		}
		return false;
	}

	public bool IsInBattleground()
	{
		if (!CurrentMapId.HasValue)
		{
			return false;
		}
		uint battlegroundIdFromMapId = GameData.GetBattlegroundIdFromMapId(CurrentMapId.Value);
		if (battlegroundIdFromMapId != 0)
		{
			foreach (KeyValuePair<uint, uint> battleFieldQueueType in BattleFieldQueueTypes)
			{
				if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
				{
					if (battleFieldQueueType.Value == CurrentMapId)
					{
						return true;
					}
				}
				else if (battleFieldQueueType.Value == battlegroundIdFromMapId)
				{
					return true;
				}
			}
		}
		return false;
	}

	public long GetBattleFieldQueueTime(uint queueSlot)
	{
		if (BattleFieldQueueTimes.ContainsKey(queueSlot))
		{
			return BattleFieldQueueTimes[queueSlot];
		}
		long unixTime = Time.UnixTime;
		BattleFieldQueueTimes.Add(queueSlot, unixTime);
		return unixTime;
	}

	public void StoreBattleFieldQueueType(uint queueSlot, uint mapOrBgId)
	{
		if (BattleFieldQueueTypes.ContainsKey(queueSlot))
		{
			BattleFieldQueueTypes[queueSlot] = mapOrBgId;
		}
		else
		{
			BattleFieldQueueTypes.Add(queueSlot, mapOrBgId);
		}
	}

	public uint GetBattleFieldQueueType(uint queueSlot)
	{
		if (BattleFieldQueueTypes.ContainsKey(queueSlot))
		{
			return BattleFieldQueueTypes[queueSlot];
		}
		return 0u;
	}

	public void StoreAuraDurationLeft(WowGuid128 guid, byte slot, int duration, int currentTime)
	{
		if (UnitAuraDurationLeft.ContainsKey(guid))
		{
			if (UnitAuraDurationLeft[guid].ContainsKey(slot))
			{
				UnitAuraDurationLeft[guid][slot] = duration;
			}
			else
			{
				UnitAuraDurationLeft[guid].Add(slot, duration);
			}
		}
		else
		{
			Dictionary<byte, int> dictionary = new Dictionary<byte, int>();
			dictionary.Add(slot, duration);
			UnitAuraDurationLeft.Add(guid, dictionary);
		}
		if (UnitAuraDurationUpdateTime.ContainsKey(guid))
		{
			if (UnitAuraDurationUpdateTime[guid].ContainsKey(slot))
			{
				UnitAuraDurationUpdateTime[guid][slot] = currentTime;
			}
			else
			{
				UnitAuraDurationUpdateTime[guid].Add(slot, currentTime);
			}
		}
		else
		{
			Dictionary<byte, int> dictionary2 = new Dictionary<byte, int>();
			dictionary2.Add(slot, currentTime);
			UnitAuraDurationUpdateTime.Add(guid, dictionary2);
		}
	}

	public void StoreAuraDurationFull(WowGuid128 guid, byte slot, int duration)
	{
		if (UnitAuraDurationFull.ContainsKey(guid))
		{
			if (UnitAuraDurationFull[guid].ContainsKey(slot))
			{
				UnitAuraDurationFull[guid][slot] = duration;
			}
			else
			{
				UnitAuraDurationFull[guid].Add(slot, duration);
			}
		}
		else
		{
			Dictionary<byte, int> dictionary = new Dictionary<byte, int>();
			dictionary.Add(slot, duration);
			UnitAuraDurationFull.Add(guid, dictionary);
		}
	}

	public void ClearAuraDuration(WowGuid128 guid, byte slot)
	{
		if (UnitAuraDurationUpdateTime.ContainsKey(guid) && UnitAuraDurationUpdateTime[guid].ContainsKey(slot))
		{
			UnitAuraDurationUpdateTime[guid].Remove(slot);
		}
		if (UnitAuraDurationLeft.ContainsKey(guid) && UnitAuraDurationLeft[guid].ContainsKey(slot))
		{
			UnitAuraDurationLeft[guid].Remove(slot);
		}
		if (UnitAuraDurationFull.ContainsKey(guid) && UnitAuraDurationFull[guid].ContainsKey(slot))
		{
			UnitAuraDurationFull[guid].Remove(slot);
		}
	}

	public void GetAuraDuration(WowGuid128 guid, byte slot, out int left, out int full)
	{
		if (UnitAuraDurationLeft.ContainsKey(guid) && UnitAuraDurationLeft[guid].ContainsKey(slot))
		{
			left = UnitAuraDurationLeft[guid][slot];
		}
		else
		{
			left = -1;
		}
		if (UnitAuraDurationFull.ContainsKey(guid) && UnitAuraDurationFull[guid].ContainsKey(slot))
		{
			full = UnitAuraDurationFull[guid][slot];
		}
		else
		{
			full = left;
		}
		if (left > 0 && UnitAuraDurationUpdateTime.ContainsKey(guid) && UnitAuraDurationUpdateTime[guid].ContainsKey(slot))
		{
			left -= Environment.TickCount - UnitAuraDurationUpdateTime[guid][slot];
		}
	}

	public void StoreAuraCaster(WowGuid128 target, byte slot, WowGuid128 caster)
	{
		if (UnitAuraCaster.ContainsKey(target))
		{
			if (UnitAuraCaster[target].ContainsKey(slot))
			{
				UnitAuraCaster[target][slot] = caster;
			}
			else
			{
				UnitAuraCaster[target].Add(slot, caster);
			}
		}
		else
		{
			Dictionary<byte, WowGuid128> dictionary = new Dictionary<byte, WowGuid128>();
			dictionary.Add(slot, caster);
			UnitAuraCaster.Add(target, dictionary);
		}
	}

	public void ClearAuraCaster(WowGuid128 guid, byte slot)
	{
		if (UnitAuraCaster.ContainsKey(guid) && UnitAuraCaster[guid].ContainsKey(slot))
		{
			UnitAuraCaster[guid].Remove(slot);
		}
	}

	public WowGuid128 GetAuraCaster(WowGuid128 target, byte slot)
	{
		if (UnitAuraCaster.ContainsKey(target) && UnitAuraCaster[target].ContainsKey(slot))
		{
			return UnitAuraCaster[target][slot];
		}
		return null;
	}

	public WowGuid128 GetAuraCaster(WowGuid128 target, byte slot, uint spellId)
	{
		WowGuid128 wowGuid = GetAuraCaster(target, slot);
		if (wowGuid == null)
		{
			wowGuid = GetLastAuraCasterOnTarget(target, spellId);
			if (wowGuid != null)
			{
				StoreAuraCaster(target, slot, wowGuid);
			}
		}
		return wowGuid;
	}

	public void StoreLastAuraCasterOnTarget(WowGuid128 target, uint spellId, WowGuid128 caster)
	{
		if (LastAuraCasterOnTarget.ContainsKey(target))
		{
			if (LastAuraCasterOnTarget[target].ContainsKey(spellId))
			{
				LastAuraCasterOnTarget[target][spellId] = caster;
			}
			else
			{
				LastAuraCasterOnTarget[target].Add(spellId, caster);
			}
		}
		else
		{
			Dictionary<uint, WowGuid128> dictionary = new Dictionary<uint, WowGuid128>();
			dictionary.Add(spellId, caster);
			LastAuraCasterOnTarget.Add(target, dictionary);
		}
	}

	public WowGuid128 GetLastAuraCasterOnTarget(WowGuid128 target, uint spellId)
	{
		if (LastAuraCasterOnTarget.ContainsKey(target) && LastAuraCasterOnTarget[target].TryGetValue(spellId, out var value))
		{
			LastAuraCasterOnTarget[target].Remove(spellId);
			return value;
		}
		return null;
	}

	public void StorePlayerGuildId(WowGuid128 guid, uint guildId)
	{
		if (PlayerGuildIds.ContainsKey(guid))
		{
			PlayerGuildIds[guid] = guildId;
		}
		else
		{
			PlayerGuildIds.Add(guid, guildId);
		}
	}

	public uint GetPlayerGuildId(WowGuid128 guid)
	{
		if (PlayerGuildIds.ContainsKey(guid))
		{
			return PlayerGuildIds[guid];
		}
		return 0u;
	}

	public uint[] GetGemsForItem(WowGuid128 guid)
	{
		if (ItemGems.ContainsKey(guid))
		{
			return ItemGems[guid];
		}
		return null;
	}

	public void SaveGemsForItem(WowGuid128 guid, uint?[] gems)
	{
		uint[] array;
		if (ItemGems.ContainsKey(guid))
		{
			array = ItemGems[guid];
		}
		else
		{
			array = new uint[3];
			ItemGems.Add(guid, array);
		}
		for (int i = 0; i < 3; i++)
		{
			if (gems[i].HasValue)
			{
				array[i] = gems[i].Value;
			}
		}
	}

	public WowGuid128 GetPetGuidByNumber(uint petNumber)
	{
		ObjectCacheMutex.WaitOne();
		foreach (KeyValuePair<WowGuid128, UpdateFieldsArray> item in ObjectCacheModern)
		{
			if (item.Key.GetHighType() == HighGuidType.Pet && item.Key.GetEntry() == petNumber)
			{
				ObjectCacheMutex.ReleaseMutex();
				return item.Key;
			}
		}
		ObjectCacheMutex.ReleaseMutex();
		return null;
	}

	public void StoreOriginalObjectType(WowGuid128 guid, ObjectType type)
	{
		if (OriginalObjectTypes.ContainsKey(guid))
		{
			OriginalObjectTypes[guid] = type;
		}
		else
		{
			OriginalObjectTypes.Add(guid, type);
		}
	}

	public ObjectType GetOriginalObjectType(WowGuid128 guid)
	{
		if (OriginalObjectTypes.ContainsKey(guid))
		{
			return OriginalObjectTypes[guid];
		}
		return guid.GetObjectType();
	}

	public void StoreRealSpell(uint realSpellId, uint learnSpellId)
	{
		if (RealSpellToLearnSpell.ContainsKey(realSpellId))
		{
			RealSpellToLearnSpell[realSpellId] = learnSpellId;
		}
		else
		{
			RealSpellToLearnSpell.Add(realSpellId, learnSpellId);
		}
	}

	public uint GetLearnSpellFromRealSpell(uint spellId)
	{
		if (RealSpellToLearnSpell.ContainsKey(spellId))
		{
			return RealSpellToLearnSpell[spellId];
		}
		return spellId;
	}

	public void StoreCreatureClass(uint entry, Class classId)
	{
		if (CreatureClasses.ContainsKey(entry))
		{
			CreatureClasses[entry] = classId;
		}
		else
		{
			CreatureClasses.Add(entry, classId);
		}
	}

	public void SetItemBuyCount(uint itemId, uint buyCount)
	{
		if (ItemBuyCount.ContainsKey(itemId))
		{
			ItemBuyCount[itemId] = buyCount;
		}
		else
		{
			ItemBuyCount.Add(itemId, buyCount);
		}
	}

	public uint GetItemBuyCount(uint itemId)
	{
		if (ItemBuyCount.ContainsKey(itemId))
		{
			return ItemBuyCount[itemId];
		}
		return 1u;
	}

	public void SetChannelId(string name, int id)
	{
		if (ChannelIds.ContainsKey(name))
		{
			ChannelIds[name] = id;
		}
		else
		{
			ChannelIds.Add(name, id);
		}
	}

	public string GetChannelName(int id)
	{
		foreach (KeyValuePair<string, int> channelId in ChannelIds)
		{
			if (channelId.Value == id)
			{
				return channelId.Key;
			}
		}
		return "";
	}

	public string GetPlayerName(WowGuid128 guid)
	{
		if (CachedPlayers.ContainsKey(guid) && CachedPlayers[guid].Name != null)
		{
			return CachedPlayers[guid].Name;
		}
		return "";
	}

	public WowGuid128? GetPlayerGuidByName(string name)
	{
		name = name.Trim().Replace("\0", "");
		foreach (KeyValuePair<WowGuid128, PlayerCache> cachedPlayer in CachedPlayers)
		{
			if (cachedPlayer.Value.Name == name && !WowGuid128.IsUnknownPlayerGuid(cachedPlayer.Key))
			{
				return cachedPlayer.Key;
			}
		}
		return null;
	}

	public void UpdatePlayerCache(WowGuid128 guid, PlayerCache data)
	{
		if (data.Name != null)
		{
			data.Name = data.Name.Trim().Replace("\0", "");
		}
		if (CachedPlayers.ContainsKey(guid))
		{
			if (!string.IsNullOrEmpty(data.Name))
			{
				CachedPlayers[guid].Name = data.Name;
			}
			if (data.RaceId != 0)
			{
				CachedPlayers[guid].RaceId = data.RaceId;
			}
			if (data.ClassId != 0)
			{
				CachedPlayers[guid].ClassId = data.ClassId;
			}
			if (data.SexId != Gender.None)
			{
				CachedPlayers[guid].SexId = data.SexId;
			}
			if (data.Level != 0)
			{
				CachedPlayers[guid].Level = data.Level;
			}
		}
		else
		{
			CachedPlayers.Add(guid, data);
		}
	}

	public Class GetUnitClass(WowGuid128 guid)
	{
		if (CachedPlayers.ContainsKey(guid))
		{
			return CachedPlayers[guid].ClassId;
		}
		if (CreatureClasses.ContainsKey(guid.GetEntry()))
		{
			return CreatureClasses[guid.GetEntry()];
		}
		return Class.Warrior;
	}

	public int GetLegacyFieldValueInt32<T>(WowGuid128 guid, T field)
	{
		int updateField = LegacyVersion.GetUpdateField(field);
		if (updateField < 0)
		{
			return 0;
		}
		Dictionary<int, UpdateField> cachedObjectFieldsLegacy = GetCachedObjectFieldsLegacy(guid);
		if (cachedObjectFieldsLegacy == null)
		{
			return 0;
		}
		if (!cachedObjectFieldsLegacy.ContainsKey(updateField))
		{
			return 0;
		}
		return cachedObjectFieldsLegacy[updateField].Int32Value;
	}

	public uint GetLegacyFieldValueUInt32<T>(WowGuid128 guid, T field)
	{
		int updateField = LegacyVersion.GetUpdateField(field);
		if (updateField < 0)
		{
			return 0u;
		}
		Dictionary<int, UpdateField> cachedObjectFieldsLegacy = GetCachedObjectFieldsLegacy(guid);
		if (cachedObjectFieldsLegacy == null)
		{
			return 0u;
		}
		if (!cachedObjectFieldsLegacy.ContainsKey(updateField))
		{
			return 0u;
		}
		return cachedObjectFieldsLegacy[updateField].UInt32Value;
	}

	public float GetLegacyFieldValueFloat<T>(WowGuid128 guid, T field)
	{
		int updateField = LegacyVersion.GetUpdateField(field);
		if (updateField < 0)
		{
			return 0f;
		}
		Dictionary<int, UpdateField> cachedObjectFieldsLegacy = GetCachedObjectFieldsLegacy(guid);
		if (cachedObjectFieldsLegacy == null)
		{
			return 0f;
		}
		if (!cachedObjectFieldsLegacy.ContainsKey(updateField))
		{
			return 0f;
		}
		return cachedObjectFieldsLegacy[updateField].FloatValue;
	}

	public Dictionary<int, UpdateField> GetCachedObjectFieldsLegacy(WowGuid128 guid)
	{
		ObjectCacheMutex.WaitOne();
		if (ObjectCacheLegacy.TryGetValue(guid, out var value))
		{
			ObjectCacheMutex.ReleaseMutex();
			return value;
		}
		ObjectCacheMutex.ReleaseMutex();
		return null;
	}

	public UpdateFieldsArray GetCachedObjectFieldsModern(WowGuid128 guid)
	{
		ObjectCacheMutex.WaitOne();
		if (ObjectCacheModern.TryGetValue(guid, out var value))
		{
			ObjectCacheMutex.ReleaseMutex();
			return value;
		}
		ObjectCacheMutex.ReleaseMutex();
		return null;
	}
}
