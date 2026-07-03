using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class SpellEnergizeLog : ServerPacket
{
	public WowGuid128 TargetGUID;

	public WowGuid128 CasterGUID;

	public uint SpellID;

	public PowerType Type;

	public int Amount;

	public int OverEnergize;

	public SpellCastLogData LogData;

	public SpellEnergizeLog()
		: base(Opcode.SMSG_SPELL_ENERGIZE_LOG, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(TargetGUID);
		_worldPacket.WritePackedGuid128(CasterGUID);
		_worldPacket.WriteUInt32(SpellID);
		_worldPacket.WriteUInt32((uint)Type);
		_worldPacket.WriteInt32(Amount);
		_worldPacket.WriteInt32(OverEnergize);
		_worldPacket.WriteBit(LogData != null);
		_worldPacket.FlushBits();
		if (LogData != null)
		{
			LogData.Write(_worldPacket);
		}
	}
}
