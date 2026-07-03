namespace HermesProxy.World.Server.Packets;

internal class CancelChannelling : ClientPacket
{
	public int SpellID;

	public int Reason;

	public CancelChannelling(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		SpellID = _worldPacket.ReadInt32();
		Reason = _worldPacket.ReadInt32();
	}
}
