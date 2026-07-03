using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class SpellDamageShield : ServerPacket
{
	public WowGuid128 VictimGUID;

	public WowGuid128 CasterGUID;

	public uint SpellID;

	public int Damage;

	public int OriginalDamage;

	public uint OverKill;

	public uint SchoolMask;

	public uint LogAbsorbed;

	public SpellCastLogData LogData;

	public SpellDamageShield()
		: base(Opcode.SMSG_SPELL_DAMAGE_SHIELD, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(VictimGUID);
		_worldPacket.WritePackedGuid128(CasterGUID);
		_worldPacket.WriteUInt32(SpellID);
		_worldPacket.WriteInt32(Damage);
		_worldPacket.WriteInt32(OriginalDamage);
		_worldPacket.WriteUInt32(OverKill);
		_worldPacket.WriteUInt32(SchoolMask);
		_worldPacket.WriteUInt32(LogAbsorbed);
		_worldPacket.WriteBit(LogData != null);
		_worldPacket.FlushBits();
		if (LogData != null)
		{
			LogData.Write(_worldPacket);
		}
	}
}
