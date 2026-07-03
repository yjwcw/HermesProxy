using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class RaidInstanceMessage : ServerPacket
{
	public InstanceResetWarningType Type;

	public uint MapID;

	public DifficultyModern DifficultyID;

	public bool Locked;

	public bool Extended;

	public RaidInstanceMessage()
		: base(Opcode.SMSG_RAID_INSTANCE_MESSAGE)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt8((byte)Type);
		_worldPacket.WriteUInt32(MapID);
		_worldPacket.WriteUInt32((uint)DifficultyID);
		_worldPacket.WriteBit(Locked);
		_worldPacket.WriteBit(Extended);
		_worldPacket.FlushBits();
	}
}
