namespace HermesProxy.World.Server.Packets;

public class ItemEnchantData
{
	public uint ID;

	public uint Expiration;

	public int Charges;

	public byte Slot;

	public void Write(WorldPacket data)
	{
		data.WriteUInt32(ID);
		data.WriteUInt32(Expiration);
		data.WriteInt32(Charges);
		data.WriteUInt8(Slot);
	}
}
