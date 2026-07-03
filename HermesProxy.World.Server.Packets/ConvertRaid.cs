namespace HermesProxy.World.Server.Packets;

internal class ConvertRaid : ClientPacket
{
	public bool Raid;

	public ConvertRaid(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		Raid = _worldPacket.HasBit();
	}
}
