namespace HermesProxy.World.Server.Packets;

public class CharacterRenameRequest : ClientPacket
{
	public string NewName;

	public WowGuid128 Guid;

	public CharacterRenameRequest(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		Guid = _worldPacket.ReadPackedGuid128();
		NewName = _worldPacket.ReadString(_worldPacket.ReadBits<uint>(6));
	}
}
