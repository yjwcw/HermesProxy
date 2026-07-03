namespace HermesProxy.World.Server.Packets;

internal class SetFactionNotAtWar : ClientPacket
{
	public byte FactionIndex;

	public SetFactionNotAtWar(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		FactionIndex = _worldPacket.ReadUInt8();
	}
}
