using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class CastFailed : ServerPacket
{
	public WowGuid128 CastID;

	public uint SpellID;

	public uint Reason;

	public int FailedArg1 = -1;

	public int FailedArg2 = -1;

	public uint SpellXSpellVisualID;

	public CastFailed()
		: base(Opcode.SMSG_CAST_FAILED, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(CastID);
		_worldPacket.WriteUInt32(SpellID);
		_worldPacket.WriteUInt32(SpellXSpellVisualID);
		_worldPacket.WriteUInt32(Reason);
		_worldPacket.WriteInt32(FailedArg1);
		_worldPacket.WriteInt32(FailedArg2);
	}
}
