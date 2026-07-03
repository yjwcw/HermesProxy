using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class PetCastFailed : ServerPacket
{
	public WowGuid128 CastID;

	public uint SpellID;

	public uint Reason;

	public int FailedArg1 = -1;

	public int FailedArg2 = -1;

	public PetCastFailed()
		: base(Opcode.SMSG_PET_CAST_FAILED, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(CastID);
		_worldPacket.WriteUInt32(SpellID);
		_worldPacket.WriteUInt32(Reason);
		_worldPacket.WriteInt32(FailedArg1);
		_worldPacket.WriteInt32(FailedArg2);
	}
}
