namespace HermesProxy.World.Server.Packets;

public class ChatMessageEmote : ClientPacket
{
	public string Text;

	public ChatMessageEmote(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		uint length = _worldPacket.ReadBits<uint>(9);
		Text = _worldPacket.ReadString(length);
	}
}
