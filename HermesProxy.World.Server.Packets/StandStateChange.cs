namespace HermesProxy.World.Server.Packets;

public class StandStateChange : ClientPacket
{
	public uint StandState;

	public StandStateChange(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		StandState = _worldPacket.ReadUInt32();
	}
}
