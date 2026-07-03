namespace HermesProxy.World.Server.Packets;

public class ChatMessageWhisper : ClientPacket
{
	public uint Language;

	public string Text;

	public string Target;

	public ChatMessageWhisper(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		Language = _worldPacket.ReadUInt32();
		uint length = _worldPacket.ReadBits<uint>(9);
		uint length2 = _worldPacket.ReadBits<uint>(9);
		Target = _worldPacket.ReadString(length);
		Text = _worldPacket.ReadString(length2);
	}
}
