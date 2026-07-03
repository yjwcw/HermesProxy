namespace HermesProxy.World.Server.Packets;

public class GuildUpdateInfoText : ClientPacket
{
	public string InfoText;

	public GuildUpdateInfoText(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		uint length = _worldPacket.ReadBits<uint>(11);
		InfoText = _worldPacket.ReadString(length);
	}
}
