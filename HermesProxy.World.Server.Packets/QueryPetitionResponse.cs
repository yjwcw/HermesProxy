using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class QueryPetitionResponse : ServerPacket
{
	public uint PetitionID;

	public bool Allow;

	public PetitionInfo Info;

	public QueryPetitionResponse()
		: base(Opcode.SMSG_QUERY_PETITION_RESPONSE)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt32(PetitionID);
		_worldPacket.WriteBit(Allow);
		_worldPacket.FlushBits();
		if (Allow)
		{
			Info.Write(_worldPacket);
		}
	}
}
