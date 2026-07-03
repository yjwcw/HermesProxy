namespace HermesProxy.World.Server.Packets;

public class GuildSetGuildMaster : ClientPacket
{
	public string NewMasterName;

	public GuildSetGuildMaster(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		uint length = _worldPacket.ReadBits<uint>(9);
		NewMasterName = _worldPacket.ReadString(length);
	}
}
