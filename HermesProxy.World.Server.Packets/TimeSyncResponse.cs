namespace HermesProxy.World.Server.Packets;

public class TimeSyncResponse : ClientPacket
{
	public uint ClientTime;

	public uint SequenceIndex;

	public TimeSyncResponse(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		SequenceIndex = _worldPacket.ReadUInt32();
		ClientTime = _worldPacket.ReadUInt32();
	}
}
