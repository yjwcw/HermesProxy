namespace HermesProxy.World.Server.Packets;

public class PetitionRenameGuild : ClientPacket
{
	public WowGuid128 PetitionGuid;

	public string NewGuildName;

	public PetitionRenameGuild(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		PetitionGuid = _worldPacket.ReadPackedGuid128();
		_worldPacket.ResetBitPos();
		uint length = _worldPacket.ReadBits<uint>(7);
		NewGuildName = _worldPacket.ReadString(length);
	}
}
