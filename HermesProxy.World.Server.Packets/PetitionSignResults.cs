using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class PetitionSignResults : ServerPacket
{
	public WowGuid128 Item;

	public WowGuid128 Player;

	public PetitionSignResult Error;

	public PetitionSignResults()
		: base(Opcode.SMSG_PETITION_SIGN_RESULTS)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(Item);
		_worldPacket.WritePackedGuid128(Player);
		_worldPacket.WriteBits(Error, 4);
		_worldPacket.FlushBits();
	}
}
