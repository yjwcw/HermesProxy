using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class QueryTimeResponse : ServerPacket
{
	public long CurrentTime;

	public QueryTimeResponse()
		: base(Opcode.SMSG_QUERY_TIME_RESPONSE, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteInt64(CurrentTime);
	}
}
