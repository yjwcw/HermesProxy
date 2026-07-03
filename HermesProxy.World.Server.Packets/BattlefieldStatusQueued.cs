using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class BattlefieldStatusQueued : ServerPacket
{
	public BattlefieldStatusHeader Hdr = new BattlefieldStatusHeader();

	public uint AverageWaitTime;

	public uint WaitTime;

	public int Unk254;

	public bool AsGroup;

	public bool EligibleForMatchmaking = true;

	public bool SuspendedQueue;

	public BattlefieldStatusQueued()
		: base(Opcode.SMSG_BATTLEFIELD_STATUS_QUEUED)
	{
	}

	public override void Write()
	{
		Hdr.Write(_worldPacket);
		_worldPacket.WriteUInt32(AverageWaitTime);
		_worldPacket.WriteUInt32(WaitTime);
		if (ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 3, 2, 5, 4))
		{
			_worldPacket.WriteInt32(Unk254);
		}
		_worldPacket.WriteBit(AsGroup);
		_worldPacket.WriteBit(EligibleForMatchmaking);
		_worldPacket.WriteBit(SuspendedQueue);
		_worldPacket.FlushBits();
	}
}
