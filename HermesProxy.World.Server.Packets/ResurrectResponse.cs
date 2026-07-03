namespace HermesProxy.World.Server.Packets;

public class ResurrectResponse : ClientPacket
{
	public WowGuid128 CasterGUID;

	public uint Response;

	public ResurrectResponse(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		CasterGUID = _worldPacket.ReadPackedGuid128();
		Response = _worldPacket.ReadUInt32();
	}
}
