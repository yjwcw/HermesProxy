namespace HermesProxy.World.Server.Packets;

internal class SetFactionInactive : ClientPacket
{
	public uint FactionIndex;

	public bool State;

	public SetFactionInactive(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		FactionIndex = _worldPacket.ReadUInt32();
		State = _worldPacket.HasBit();
	}
}
