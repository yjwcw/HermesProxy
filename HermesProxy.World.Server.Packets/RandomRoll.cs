using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class RandomRoll : ServerPacket
{
	public WowGuid128 Roller;

	public WowGuid128 RollerWowAccount;

	public int Min;

	public int Max;

	public int Result;

	public RandomRoll()
		: base(Opcode.SMSG_RANDOM_ROLL)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(Roller);
		_worldPacket.WritePackedGuid128(RollerWowAccount);
		_worldPacket.WriteInt32(Min);
		_worldPacket.WriteInt32(Max);
		_worldPacket.WriteInt32(Result);
	}
}
