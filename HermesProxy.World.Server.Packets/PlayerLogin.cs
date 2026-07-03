namespace HermesProxy.World.Server.Packets;

public class PlayerLogin : ClientPacket
{
	public WowGuid128 Guid;

	public float FarClip;

	public bool UnkBit;

	public PlayerLogin(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		Guid = _worldPacket.ReadPackedGuid128();
		FarClip = _worldPacket.ReadFloat();
		UnkBit = _worldPacket.HasBit();
	}
}
