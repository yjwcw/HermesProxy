namespace HermesProxy.World.Server.Packets;

internal class ReadyCheckResponseClient : ClientPacket
{
	public byte PartyIndex;

	public bool IsReady;

	public ReadyCheckResponseClient(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		PartyIndex = _worldPacket.ReadUInt8();
		IsReady = _worldPacket.HasBit();
	}
}
