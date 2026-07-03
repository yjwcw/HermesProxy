namespace HermesProxy.World.Server.Packets;

public class GuildUpdateMotdText : ClientPacket
{
	public string MotdText;

	public GuildUpdateMotdText(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		uint length = _worldPacket.ReadBits<uint>(11);
		MotdText = _worldPacket.ReadString(length);
	}
}
