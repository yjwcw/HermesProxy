namespace HermesProxy.World.Server.Packets;

public class LeaveChannel : ClientPacket
{
	public int ZoneChannelID;

	public string ChannelName;

	public LeaveChannel(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		ZoneChannelID = _worldPacket.ReadInt32();
		ChannelName = _worldPacket.ReadString(_worldPacket.ReadBits<uint>(7));
	}
}
