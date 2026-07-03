using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class EnvironmentalDamageLog : ServerPacket
{
	public WowGuid128 Victim;

	public EnvironmentalDamage Type;

	public int Amount;

	public int Resisted;

	public int Absorbed;

	public SpellCastLogData LogData;

	public EnvironmentalDamageLog()
		: base(Opcode.SMSG_ENVIRONMENTAL_DAMAGE_LOG)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(Victim);
		_worldPacket.WriteUInt8((byte)Type);
		_worldPacket.WriteInt32(Amount);
		_worldPacket.WriteInt32(Resisted);
		_worldPacket.WriteInt32(Absorbed);
		_worldPacket.WriteBit(LogData != null);
		_worldPacket.FlushBits();
		if (LogData != null)
		{
			LogData.Write(_worldPacket);
		}
	}
}
