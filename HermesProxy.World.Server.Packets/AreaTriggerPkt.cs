namespace HermesProxy.World.Server.Packets;

internal class AreaTriggerPkt : ClientPacket
{
	public uint AreaTriggerID;

	public bool Entered;

	public bool FromClient;

	public AreaTriggerPkt(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		AreaTriggerID = _worldPacket.ReadUInt32();
		Entered = _worldPacket.HasBit();
		FromClient = _worldPacket.HasBit();
	}
}
