using HermesProxy.World.Enums;

namespace HermesProxy.World;

public class WowGuid64 : WowGuid
{
	public static WowGuid64 Empty = new WowGuid64(0uL);

	public WowGuid64(ulong id)
	{
		base.Low = id;
		base.HighGuid = new HighGuidLegacy(GetHighGuidTypeLegacy());
	}

	public WowGuid64(HighGuidTypeLegacy hi, uint counter)
	{
		base.Low = (ulong)((counter != 0) ? (counter | ((long)hi << 48)) : 0);
		base.HighGuid = new HighGuidLegacy(GetHighGuidTypeLegacy());
	}

	public WowGuid64(HighGuidTypeLegacy hi, uint entry, uint counter)
	{
		base.Low = ((counter != 0) ? (counter | ((ulong)entry << 24) | (ulong)((long)hi << 48)) : 0);
		base.HighGuid = new HighGuidLegacy(GetHighGuidTypeLegacy());
	}

	public static WowGuid64 Create(WowGuid128 guid)
	{
		switch (guid.GetHighType())
		{
		case HighGuidType.Uniq:
			return WowGuid.ConvertUniqGuid(guid);
		case HighGuidType.Player:
			return new WowGuid64(HighGuidTypeLegacy.Player, (uint)guid.GetCounter());
		case HighGuidType.Item:
			return new WowGuid64(HighGuidTypeLegacy.Item, (uint)guid.GetCounter());
		case HighGuidType.Transport:
			if (guid.GetEntry() != 0)
			{
				return new WowGuid64(HighGuidTypeLegacy.Transport, guid.GetEntry(), (uint)guid.GetCounter());
			}
			return new WowGuid64(HighGuidTypeLegacy.MOTransport, (uint)guid.GetCounter());
		case HighGuidType.RaidGroup:
			return new WowGuid64(HighGuidTypeLegacy.Group, (uint)guid.GetCounter());
		case HighGuidType.GameObject:
			return new WowGuid64(HighGuidTypeLegacy.GameObject, guid.GetEntry(), (uint)guid.GetCounter());
		case HighGuidType.Creature:
			return new WowGuid64(HighGuidTypeLegacy.Creature, guid.GetEntry(), (uint)guid.GetCounter());
		case HighGuidType.Pet:
			return new WowGuid64(HighGuidTypeLegacy.Pet, guid.GetEntry(), (uint)guid.GetCounter());
		case HighGuidType.Vehicle:
			return new WowGuid64(HighGuidTypeLegacy.Vehicle, guid.GetEntry(), (uint)guid.GetCounter());
		case HighGuidType.DynamicObject:
			return new WowGuid64(HighGuidTypeLegacy.DynamicObject, guid.GetEntry(), (uint)guid.GetCounter());
		case HighGuidType.Corpse:
			return new WowGuid64(HighGuidTypeLegacy.Corpse, guid.GetEntry(), (uint)guid.GetCounter());
		case HighGuidType.LootObject:
			return new WowGuid64((HighGuidTypeLegacy)guid.GetServerId(), guid.GetEntry(), (uint)guid.GetCounter());
		default:
			return Empty;
		}
	}

	public override bool HasEntry()
	{
		switch (GetHighType())
		{
		case HighGuidType.Player:
		case HighGuidType.Item:
		case HighGuidType.MOTransport:
		case HighGuidType.DynamicObject:
		case HighGuidType.Corpse:
		case HighGuidType.RaidGroup:
			return false;
		default:
			return true;
		}
	}

	public override ulong GetCounter()
	{
		return (uint)(HasEntry() ? (base.Low & 0xFFFFFF) : (base.Low & 0xFFFFFFFFu));
	}

	public override uint GetEntry()
	{
		if (!HasEntry())
		{
			return 0u;
		}
		return (uint)((base.Low >> 24) & 0xFFFFFF);
	}

	public HighGuidTypeLegacy GetHighGuidTypeLegacy()
	{
		if (base.Low == 0L)
		{
			return HighGuidTypeLegacy.None;
		}
		return (HighGuidTypeLegacy)((base.Low >> 48) & 0xFFFF);
	}

	public override string ToString()
	{
		if (base.Low == 0L)
		{
			return "0x0";
		}
		if (HasEntry())
		{
			return "Full: 0x" + base.Low.ToString("X8") + " Type: " + GetHighType().ToString() + " Entry: " + GetEntry() + " Low: " + GetCounter();
		}
		return "Full: 0x" + base.Low.ToString("X8") + " Type: " + GetHighType().ToString() + " Low: " + GetCounter();
	}

	public override WowGuid64 To64()
	{
		return this;
	}

	public override WowGuid128 To128(GameSessionData gameState)
	{
		return WowGuid128.Create(this, gameState);
	}

	public WowGuid128 ToLootGuid()
	{
		return WowGuid128.CreateLootGuid(GetHighGuidTypeLegacy(), GetEntry(), GetCounter());
	}
}
