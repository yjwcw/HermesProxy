namespace HermesProxy.World.Server.Packets;

public class ItemGemData
{
	public byte Slot;

	public ItemInstance Item = new ItemInstance();

	public void Write(WorldPacket data)
	{
		data.WriteUInt8(Slot);
		Item.Write(data);
	}

	public void Read(WorldPacket data)
	{
		Slot = data.ReadUInt8();
		Item.Read(data);
	}
}
