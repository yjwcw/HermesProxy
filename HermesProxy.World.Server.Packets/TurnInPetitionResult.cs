using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class TurnInPetitionResult : ServerPacket
{
	public PetitionTurnResult Result;

	public TurnInPetitionResult()
		: base(Opcode.SMSG_TURN_IN_PETITION_RESULT)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteBits(Result, 4);
		_worldPacket.FlushBits();
	}
}
