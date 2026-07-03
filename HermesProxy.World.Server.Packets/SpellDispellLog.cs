using System.Collections.Generic;
using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class SpellDispellLog : ServerPacket
{
	public bool IsSteal;

	public bool IsBreak;

	public WowGuid128 TargetGUID;

	public WowGuid128 CasterGUID;

	public uint DispelledBySpellID;

	public List<SpellDispellData> DispellData = new List<SpellDispellData>();

	public SpellDispellLog()
		: base(Opcode.SMSG_SPELL_DISPELL_LOG, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteBit(IsSteal);
		_worldPacket.WriteBit(IsBreak);
		_worldPacket.WritePackedGuid128(TargetGUID);
		_worldPacket.WritePackedGuid128(CasterGUID);
		_worldPacket.WriteUInt32(DispelledBySpellID);
		_worldPacket.WriteInt32(DispellData.Count);
		foreach (SpellDispellData dispellDatum in DispellData)
		{
			_worldPacket.WriteUInt32(dispellDatum.SpellID);
			_worldPacket.WriteBit(dispellDatum.Harmful);
			_worldPacket.WriteBit(dispellDatum.Rolled.HasValue);
			_worldPacket.WriteBit(dispellDatum.Needed.HasValue);
			if (dispellDatum.Rolled.HasValue)
			{
				_worldPacket.WriteInt32(dispellDatum.Rolled.Value);
			}
			if (dispellDatum.Needed.HasValue)
			{
				_worldPacket.WriteInt32(dispellDatum.Needed.Value);
			}
			_worldPacket.FlushBits();
		}
	}
}
