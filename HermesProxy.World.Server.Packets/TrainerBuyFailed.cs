using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class TrainerBuyFailed : ServerPacket
{
	public WowGuid128 TrainerGUID;

	public uint SpellID;

	public uint TrainerFailedReason;

	public TrainerBuyFailed()
		: base(Opcode.SMSG_TRAINER_BUY_FAILED)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(TrainerGUID);
		_worldPacket.WriteUInt32(SpellID);
		_worldPacket.WriteUInt32(TrainerFailedReason);
	}
}
