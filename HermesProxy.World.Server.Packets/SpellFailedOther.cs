using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class SpellFailedOther : ServerPacket
{
	public WowGuid128 CasterUnit;

	public WowGuid128 CastID;

	public uint SpellID;

	public uint SpellXSpellVisualID;

	public byte Reason;

	public SpellFailedOther()
		: base(Opcode.SMSG_SPELL_FAILED_OTHER, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(CasterUnit);
		_worldPacket.WritePackedGuid128(CastID);
		_worldPacket.WriteUInt32(SpellID);
		_worldPacket.WriteUInt32(SpellXSpellVisualID);
		_worldPacket.WriteUInt8(Reason);
	}
}
