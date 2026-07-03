namespace HermesProxy.World.Server.Packets;

public struct InspectEnchantData
{
	public uint Id;

	public byte Index;

	public InspectEnchantData(uint id, byte index)
	{
		Id = id;
		Index = index;
	}

	public void Write(WorldPacket data)
	{
		data.WriteUInt32(Id);
		data.WriteUInt8(Index);
	}
}
