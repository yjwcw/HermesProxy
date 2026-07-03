using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class RespecWipeConfirm : ServerPacket
{
	public SpecResetType RespecType;

	public uint Cost;

	public WowGuid128 TrainerGUID;

	public RespecWipeConfirm()
		: base(Opcode.SMSG_RESPEC_WIPE_CONFIRM)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteInt8((sbyte)RespecType);
		_worldPacket.WriteUInt32(Cost);
		_worldPacket.WritePackedGuid128(TrainerGUID);
	}
}
