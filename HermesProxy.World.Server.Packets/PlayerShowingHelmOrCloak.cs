namespace HermesProxy.World.Server.Packets;

internal class PlayerShowingHelmOrCloak : ClientPacket
{
	public bool Showing;

	public PlayerShowingHelmOrCloak(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		_worldPacket.ResetBitPos();
		Showing = _worldPacket.HasBit();
	}
}
