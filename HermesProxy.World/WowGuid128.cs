using Framework.Logging;
using HermesProxy.World.Enums;

namespace HermesProxy.World;

public class WowGuid128 : WowGuid
{
	private const ulong UNKNOWN_TMP_GUID_START = 10000000000uL;

	private static ulong _nextUnknownTmpGuid = 10000000000uL;

	public static WowGuid128 Empty = new WowGuid128();

	public WowGuid128()
	{
		base.Low = 0uL;
		base.High = 0uL;
		base.HighGuid = new HighGuid703((byte)((base.High >> 58) & 0x3F));
	}

	public WowGuid128(ulong high, ulong low)
	{
		base.Low = low;
		base.High = high;
		base.HighGuid = new HighGuid703((byte)((base.High >> 58) & 0x3F));
	}

	public static WowGuid128 Create(WowGuid64 guid, GameSessionData gamestate)
	{
		switch (guid.GetHighType())
		{
		case HighGuidType.Player:
			return Create(HighGuidType703.Player, guid.GetCounter());
		case HighGuidType.Item:
			return Create(HighGuidType703.Item, guid.GetCounter());
		case HighGuidType.Transport:
		case HighGuidType.MOTransport:
			return TransportCreate(guid.GetCounter(), guid.GetEntry());
		case HighGuidType.RaidGroup:
			return Create(HighGuidType703.RaidGroup, guid.GetCounter());
		case HighGuidType.GameObject:
			return Create(HighGuidType703.GameObject, gamestate.GetObjectSpawnCounter(guid), guid.GetEntry(), guid.GetCounter());
		case HighGuidType.Creature:
			return Create(HighGuidType703.Creature, gamestate.GetObjectSpawnCounter(guid), guid.GetEntry(), guid.GetCounter());
		case HighGuidType.Pet:
			return Create(HighGuidType703.Pet, 0u, guid.GetEntry(), guid.GetCounter());
		case HighGuidType.Vehicle:
			return Create(HighGuidType703.Vehicle, 0u, guid.GetEntry(), guid.GetCounter());
		case HighGuidType.DynamicObject:
			return Create(HighGuidType703.DynamicObject, 0u, guid.GetEntry(), guid.GetCounter());
		case HighGuidType.Corpse:
			return Create(HighGuidType703.Corpse, 0u, guid.GetEntry(), guid.GetCounter());
		default:
			return Empty;
		}
	}

	public static WowGuid128 Create(HighGuidType703 type, ulong counter)
	{
		switch (type)
		{
		case HighGuidType703.Uniq:
		case HighGuidType703.Party:
		case HighGuidType703.WowAccount:
		case HighGuidType703.BNetAccount:
		case HighGuidType703.GMTask:
		case HighGuidType703.RaidGroup:
		case HighGuidType703.Spell:
		case HighGuidType703.Mail:
		case HighGuidType703.UserRouter:
		case HighGuidType703.PVPQueueGroup:
		case HighGuidType703.UserClient:
		case HighGuidType703.UniqUserClient:
		case HighGuidType703.BattlePet:
		case HighGuidType703.CommerceObj:
		case HighGuidType703.ClientSession:
		case HighGuidType703.ArenaTeam:
			return GlobalCreate(type, counter);
		case HighGuidType703.Player:
		case HighGuidType703.Item:
		case HighGuidType703.Transport:
		case HighGuidType703.Guild:
			return RealmSpecificCreate(type, counter);
		default:
			Log.Print(LogType.Error, $"This guid type cannot be constructed using Create(HighGuid: {type} ulong counter).", "Create", "D:\\a\\HermesProxy\\HermesProxy\\World\\WowGuid.cs");
			return Empty;
		}
	}

	public static WowGuid128 Create(HighGuidType703 type, uint mapId, uint entry, ulong counter)
	{
		return MapSpecificCreate(type, 0, (ushort)mapId, 0u, entry, counter);
	}

	public static WowGuid128 Create(HighGuidType703 type, SpellCastSource subType, uint mapId, uint entry, ulong counter)
	{
		return MapSpecificCreate(type, (byte)subType, (ushort)mapId, 0u, entry, counter);
	}

	public static WowGuid128 CreateLootGuid(HighGuidTypeLegacy type, uint entry, ulong counter)
	{
		return MapSpecificCreate(HighGuidType703.LootObject, 0, 0, (uint)type, entry, counter);
	}

	public static WowGuid128 CreateUnknownPlayerGuid()
	{
		return Create(HighGuidType703.Player, _nextUnknownTmpGuid++);
	}

	public static bool IsUnknownPlayerGuid(WowGuid128 playerGuid)
	{
		if (playerGuid.IsPlayer())
		{
			return playerGuid.GetCounter() >= 10000000000L;
		}
		return false;
	}

	private static WowGuid128 GlobalCreate(HighGuidType703 type, ulong counter)
	{
		return new WowGuid128((ulong)((long)type << 58), counter);
	}

	private static WowGuid128 TransportCreate(ulong counter, uint entry)
	{
		return new WowGuid128(0x1800000000000000uL | (counter << 38) | entry, 0uL);
	}

	private static WowGuid128 RealmSpecificCreate(HighGuidType703 type, ulong counter)
	{
		if (type == HighGuidType703.Transport)
		{
			return new WowGuid128((ulong)((long)type << 58) | (counter << 38), 0uL);
		}
		return new WowGuid128((ulong)((long)type << 58) | 0x40000000000uL, counter);
	}

	private static WowGuid128 MapSpecificCreate(HighGuidType703 type, byte subType, ushort mapId, uint serverId, uint entry, ulong counter)
	{
		return new WowGuid128((ulong)((long)type << 58) | 0x40000000000uL | (ulong)((long)(mapId & 0x1FFF) << 29) | ((ulong)(entry & 0x7FFFFF) << 6) | ((ulong)subType & 0x3FuL), ((ulong)(serverId & 0xFFFFFF) << 40) | (counter & 0xFFFFFFFFFFuL));
	}

	public override bool HasEntry()
	{
		HighGuidType highType = GetHighType();
		if ((uint)(highType - 9) <= 3u || highType == HighGuidType.AreaTrigger)
		{
			return true;
		}
		return false;
	}

	public byte GetSubType()
	{
		return (byte)(base.High & 0x3F);
	}

	public ushort GetRealmId()
	{
		return (ushort)((base.High >> 42) & 0x1FFF);
	}

	public uint GetServerId()
	{
		return (uint)((base.Low >> 40) & 0xFFFFFF);
	}

	public ushort GetMapId()
	{
		return (ushort)((base.High >> 29) & 0x1FFF);
	}

	public override uint GetEntry()
	{
		if (GetHighType() == HighGuidType.Transport)
		{
			return (uint)(base.High & 0xFFFFFFFFu);
		}
		return (uint)((base.High >> 6) & 0x7FFFFF);
	}

	public override ulong GetCounter()
	{
		if (GetHighType() == HighGuidType.Transport)
		{
			return (base.High >> 38) & 0xFFFFF;
		}
		return base.Low & 0xFFFFFFFFFFuL;
	}

	public override string ToString()
	{
		if (base.Low == 0L && base.High == 0L)
		{
			return "Full: 0x0";
		}
		if (!HasEntry())
		{
			return $"Full: 0x{base.High:X16}{base.Low:X16} {GetHighType()}/{GetSubType()} R{GetRealmId()}/S{GetServerId()} Map: {GetMapId()} Low: {GetCounter()}";
		}
		return $"Full: 0x{base.High:X16}{base.Low:X16} {GetHighType()}/{GetSubType()} R{GetRealmId()}/S{GetServerId()} Map: {GetMapId()} Entry: {GetEntry()} Low: {GetCounter()}";
	}

	public override WowGuid64 To64()
	{
		return WowGuid64.Create(this);
	}

	public override WowGuid128 To128(GameSessionData gameState)
	{
		return this;
	}
}
