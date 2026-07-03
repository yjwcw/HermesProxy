using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class QueryPlayerNameResponse : ServerPacket
{
	public WowGuid128 Player;

	public byte Result;

	public PlayerGuidLookupData Data;

	public QueryPlayerNameResponse()
		: base(Opcode.SMSG_QUERY_PLAYER_NAME_RESPONSE)
	{
		Data = new PlayerGuidLookupData();
	}

	public override void Write()
	{
		_worldPacket.WriteInt8((sbyte)Result);
		_worldPacket.WritePackedGuid128(Player);
		if (Result == 0)
		{
			Data.Write(_worldPacket);
		}
	}
}
