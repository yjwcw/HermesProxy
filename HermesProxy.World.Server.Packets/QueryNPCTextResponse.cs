using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class QueryNPCTextResponse : ServerPacket
{
	public uint TextID;

	public bool Allow;

	public float[] Probabilities = new float[8];

	public uint[] BroadcastTextID = new uint[8];

	public QueryNPCTextResponse()
		: base(Opcode.SMSG_QUERY_NPC_TEXT_RESPONSE, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt32(TextID);
		_worldPacket.WriteBit(Allow);
		_worldPacket.WriteInt32(Allow ? 64 : 0);
		if (Allow)
		{
			for (uint num = 0u; num < 8; num++)
			{
				_worldPacket.WriteFloat(Probabilities[num]);
			}
			for (uint num2 = 0u; num2 < 8; num2++)
			{
				_worldPacket.WriteUInt32(BroadcastTextID[num2]);
			}
		}
	}
}
