using HermesProxy.World.Enums;

namespace HermesProxy.World;

public abstract class WowGuid
{
	public ulong Low { get; protected set; }

	public HighGuid HighGuid { get; protected set; }

	public ulong High { get; protected set; }

	public abstract bool HasEntry();

	public abstract ulong GetCounter();

	public ulong GetLowValue()
	{
		return Low;
	}

	public abstract uint GetEntry();

	public HighGuidType GetHighType()
	{
		return HighGuid.GetHighGuidType();
	}

	public ulong GetHighValue()
	{
		return High;
	}

	public ObjectType GetObjectType()
	{
		switch (GetHighType())
		{
		case HighGuidType.Player:
			return ObjectType.Player;
		case HighGuidType.DynamicObject:
			return ObjectType.DynamicObject;
		case HighGuidType.Corpse:
			return ObjectType.Corpse;
		case HighGuidType.Item:
			return ObjectType.Item;
		case HighGuidType.Transport:
		case HighGuidType.MOTransport:
		case HighGuidType.GameObject:
			return ObjectType.GameObject;
		case HighGuidType.Creature:
		case HighGuidType.Vehicle:
		case HighGuidType.Pet:
			return ObjectType.Unit;
		case HighGuidType.AreaTrigger:
			return ObjectType.AreaTrigger;
		default:
			return ObjectType.Object;
		}
	}

	public bool IsWorldObject()
	{
		switch (GetHighType())
		{
		case HighGuidType.Player:
		case HighGuidType.Transport:
		case HighGuidType.MOTransport:
		case HighGuidType.Creature:
		case HighGuidType.Vehicle:
		case HighGuidType.Pet:
		case HighGuidType.GameObject:
		case HighGuidType.DynamicObject:
		case HighGuidType.Corpse:
			return true;
		default:
			return false;
		}
	}

	public bool IsTransport()
	{
		HighGuidType highType = GetHighType();
		if ((uint)(highType - 6) <= 1u)
		{
			return true;
		}
		return false;
	}

	public bool IsPlayer()
	{
		ObjectType objectType = GetObjectType();
		if ((uint)(objectType - 6) <= 1u)
		{
			return true;
		}
		return false;
	}

	public bool IsCreature()
	{
		return GetObjectType() == ObjectType.Unit;
	}

	public bool IsItem()
	{
		ObjectType objectType = GetObjectType();
		if ((uint)(objectType - 1) <= 1u)
		{
			return true;
		}
		return false;
	}

	public static WowGuid64 ConvertUniqGuid(WowGuid128 guid)
	{
		if ((int)guid.GetLowValue() == 10)
		{
			return new WowGuid64(6uL);
		}
		return WowGuid64.Empty;
	}

	public static bool operator ==(WowGuid first, WowGuid other)
	{
		if ((object)first == other)
		{
			return true;
		}
		if ((object)first == null || (object)other == null)
		{
			return false;
		}
		return first.Equals(other);
	}

	public static bool operator !=(WowGuid first, WowGuid other)
	{
		return !(first == other);
	}

	public override bool Equals(object obj)
	{
		if (obj is WowGuid)
		{
			return Equals((WowGuid)obj);
		}
		return false;
	}

	public bool Equals(WowGuid other)
	{
		if (other.Low == Low)
		{
			return other.High == High;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return new { Low, High }.GetHashCode();
	}

	public bool IsEmpty()
	{
		if (High == 0L)
		{
			return Low == 0;
		}
		return false;
	}

	public abstract WowGuid64 To64();

	public abstract WowGuid128 To128(GameSessionData gameState);
}
