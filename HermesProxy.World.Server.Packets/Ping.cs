namespace HermesProxy.World.Server.Packets;

internal class Ping : ClientPacket
{
	public uint Serial;

	public uint Latency;

	public Ping(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		Serial = _worldPacket.ReadUInt32();
		Latency = _worldPacket.ReadUInt32();
	}
}
