namespace HermesProxy.World.Server.Packets;

internal class SetFactionAtWar : ClientPacket
{
	public byte FactionIndex;

	public SetFactionAtWar(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		FactionIndex = _worldPacket.ReadUInt8();
	}
}
