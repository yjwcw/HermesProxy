using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class ItemMod
{
	public uint Value;

	public ItemModifier Type;

	public ItemMod()
	{
		Type = ItemModifier.Max;
	}

	public ItemMod(uint value, ItemModifier type)
	{
		Value = value;
		Type = type;
	}

	public void Read(WorldPacket data)
	{
		Value = data.ReadUInt32();
		Type = (ItemModifier)data.ReadUInt8();
	}

	public void Write(WorldPacket data)
	{
		data.WriteUInt32(Value);
		data.WriteUInt8((byte)Type);
	}
}
