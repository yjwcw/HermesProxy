using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class TimeSyncRequest : ServerPacket
{
	public uint SequenceIndex;

	public TimeSyncRequest()
		: base(Opcode.SMSG_TIME_SYNC_REQUEST, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt32(SequenceIndex);
	}
}
