namespace HermesProxy.World.Server.Packets;

public class SetAmmo : ClientPacket
{
	public uint ItemId;

	public SetAmmo(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		ItemId = _worldPacket.ReadUInt32();
	}
}
