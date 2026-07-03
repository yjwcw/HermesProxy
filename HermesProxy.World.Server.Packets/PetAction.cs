using Framework.GameMath;

namespace HermesProxy.World.Server.Packets;

internal class PetAction : ClientPacket
{
	public WowGuid128 PetGUID;

	public uint Action;

	public WowGuid128 TargetGUID;

	public Vector3 ActionPosition;

	public PetAction(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		PetGUID = _worldPacket.ReadPackedGuid128();
		Action = _worldPacket.ReadUInt32();
		TargetGUID = _worldPacket.ReadPackedGuid128();
		ActionPosition = _worldPacket.ReadVector3();
	}
}
