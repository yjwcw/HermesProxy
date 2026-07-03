namespace HermesProxy.World.Server.Packets;

public class JoinChannel : ClientPacket
{
	public string Password;

	public string ChannelName;

	public int ChatChannelId;

	public JoinChannel(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		ChatChannelId = _worldPacket.ReadInt32();
		uint length = _worldPacket.ReadBits<uint>(7);
		uint length2 = _worldPacket.ReadBits<uint>(7);
		_worldPacket.ResetBitPos();
		ChannelName = _worldPacket.ReadString(length);
		Password = _worldPacket.ReadString(length2);
	}
}
