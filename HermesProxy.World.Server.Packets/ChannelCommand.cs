namespace HermesProxy.World.Server.Packets;

internal class ChannelCommand : ClientPacket
{
	public string ChannelName;

	public ChannelCommand(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		ChannelName = _worldPacket.ReadString(_worldPacket.ReadBits<uint>(7));
	}
}
