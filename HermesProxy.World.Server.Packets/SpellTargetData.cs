using System;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class SpellTargetData
{
	public SpellCastTargetFlags Flags;

	public WowGuid128 Unit;

	public WowGuid128 Item;

	public TargetLocation SrcLocation;

	public TargetLocation DstLocation;

	public float? Orientation;

	public int? MapID;

	public string Name = "";

	public void Read(WorldPacket data)
	{
		Flags = (SpellCastTargetFlags)data.ReadBits<uint>(26);
		if (data.HasBit())
		{
			SrcLocation = new TargetLocation();
		}
		if (data.HasBit())
		{
			DstLocation = new TargetLocation();
		}
		if (data.HasBit())
		{
			Orientation = 0f;
		}
		if (data.HasBit())
		{
			MapID = 0;
		}
		uint length = data.ReadBits<uint>(7);
		Unit = data.ReadPackedGuid128();
		Item = data.ReadPackedGuid128();
		if (SrcLocation != null)
		{
			SrcLocation.Read(data);
		}
		if (DstLocation != null)
		{
			DstLocation.Read(data);
		}
		if (Orientation.HasValue)
		{
			Orientation = data.ReadFloat();
		}
		if (MapID.HasValue)
		{
			MapID = data.ReadInt32();
		}
		Name = data.ReadString(length);
	}

	public void Write(WorldPacket data)
	{
		data.WriteBits((uint)Flags, 26);
		data.WriteBit(SrcLocation != null);
		data.WriteBit(DstLocation != null);
		data.WriteBit(Orientation.HasValue);
		data.WriteBit(MapID.HasValue);
		data.WriteBits(Name.GetByteCount(), 7);
		data.FlushBits();
		data.WritePackedGuid128(Unit);
		data.WritePackedGuid128(Item);
		if (SrcLocation != null)
		{
			SrcLocation.Write(data);
		}
		if (DstLocation != null)
		{
			DstLocation.Write(data);
		}
		if (Orientation.HasValue)
		{
			data.WriteFloat(Orientation.Value);
		}
		if (MapID.HasValue)
		{
			data.WriteInt32(MapID.Value);
		}
		data.WriteString(Name);
	}
}
