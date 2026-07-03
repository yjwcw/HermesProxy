using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class LogXPGain : ServerPacket
{
	public WowGuid128 Victim;

	public int Original;

	public PlayerLogXPReason Reason;

	public int Amount;

	public float GroupBonus = 1f;

	public byte RAFBonus;

	public LogXPGain()
		: base(Opcode.SMSG_LOG_XP_GAIN)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(Victim);
		_worldPacket.WriteInt32(Original);
		_worldPacket.WriteUInt8((byte)Reason);
		_worldPacket.WriteInt32(Amount);
		_worldPacket.WriteFloat(GroupBonus);
		_worldPacket.WriteUInt8(RAFBonus);
	}
}
