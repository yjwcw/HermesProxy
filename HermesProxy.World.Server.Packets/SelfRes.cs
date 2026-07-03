namespace HermesProxy.World.Server.Packets;

internal class SelfRes : ClientPacket
{
	public uint SpellId;

	public SelfRes(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		SpellId = _worldPacket.ReadUInt32();
	}
}
