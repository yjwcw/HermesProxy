namespace HermesProxy.World.Server.Packets;

public class GuildBankSetTabText : ClientPacket
{
	public int Tab;

	public string TabText;

	public GuildBankSetTabText(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		Tab = _worldPacket.ReadInt32();
		TabText = _worldPacket.ReadString(_worldPacket.ReadBits<uint>(14));
	}
}
