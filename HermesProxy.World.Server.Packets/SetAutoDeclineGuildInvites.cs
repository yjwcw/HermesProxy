namespace HermesProxy.World.Server.Packets;

public class SetAutoDeclineGuildInvites : ClientPacket
{
	public bool GuildInvitesShouldGetBlocked;

	public SetAutoDeclineGuildInvites(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		GuildInvitesShouldGetBlocked = _worldPacket.ReadBool();
	}
}
