namespace HermesProxy.World.Server.Packets;

internal class SetWatchedFaction : ClientPacket
{
	public uint FactionIndex;

	public SetWatchedFaction(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		FactionIndex = _worldPacket.ReadUInt32();
	}
}
