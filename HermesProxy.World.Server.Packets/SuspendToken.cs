using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class SuspendToken : ServerPacket
{
	public uint SequenceIndex = 1u;

	public uint Reason = 1u;

	public SuspendToken()
		: base(Opcode.SMSG_SUSPEND_TOKEN, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt32(SequenceIndex);
		_worldPacket.WriteBits(Reason, 2);
		_worldPacket.FlushBits();
	}
}
