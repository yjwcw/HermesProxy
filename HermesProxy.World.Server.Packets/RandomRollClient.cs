namespace HermesProxy.World.Server.Packets;

public class RandomRollClient : ClientPacket
{
	public int Min;

	public int Max;

	public byte PartyIndex;

	public RandomRollClient(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		Min = _worldPacket.ReadInt32();
		Max = _worldPacket.ReadInt32();
		PartyIndex = _worldPacket.ReadUInt8();
	}
}
