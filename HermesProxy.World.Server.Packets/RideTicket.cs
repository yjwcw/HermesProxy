namespace HermesProxy.World.Server.Packets;

public class RideTicket
{
	public WowGuid128 RequesterGuid = WowGuid128.Empty;

	public uint Id;

	public RideType Type;

	public long Time;

	public void Read(WorldPacket data)
	{
		RequesterGuid = data.ReadPackedGuid128();
		Id = data.ReadUInt32();
		Type = (RideType)data.ReadUInt32();
		Time = data.ReadInt64();
	}

	public void Write(WorldPacket data)
	{
		data.WritePackedGuid128(RequesterGuid);
		data.WriteUInt32(Id);
		data.WriteUInt32((uint)Type);
		data.WriteInt64(Time);
	}
}
