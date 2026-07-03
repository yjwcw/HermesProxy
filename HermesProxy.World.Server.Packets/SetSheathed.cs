namespace HermesProxy.World.Server.Packets;

public class SetSheathed : ClientPacket
{
	public int SheathState;

	public bool Animate = true;

	public SetSheathed(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		SheathState = _worldPacket.ReadInt32();
		Animate = _worldPacket.HasBit();
	}
}
