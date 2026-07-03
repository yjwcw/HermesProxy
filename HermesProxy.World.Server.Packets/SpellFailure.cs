using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class SpellFailure : ServerPacket
{
	public WowGuid128 CasterUnit;

	public WowGuid128 CastID;

	public uint SpellID;

	public uint SpellXSpellVisualID;

	public ushort Reason;

	public SpellFailure()
		: base(Opcode.SMSG_SPELL_FAILURE, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(CasterUnit);
		_worldPacket.WritePackedGuid128(CastID);
		_worldPacket.WriteUInt32(SpellID);
		_worldPacket.WriteUInt32(SpellXSpellVisualID);
		_worldPacket.WriteUInt16(Reason);
	}
}
