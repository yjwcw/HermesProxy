namespace HermesProxy.World.Server.Packets;

public class SetTitle : ClientPacket
{
	public int TitleID;

	public SetTitle(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		TitleID = _worldPacket.ReadInt32();
	}
}
