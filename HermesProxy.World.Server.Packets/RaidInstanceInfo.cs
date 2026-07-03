using System.Collections.Generic;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class RaidInstanceInfo : ServerPacket
{
	public List<InstanceLock> LockList = new List<InstanceLock>();

	public RaidInstanceInfo()
		: base(Opcode.SMSG_RAID_INSTANCE_INFO)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteInt32(LockList.Count);
		foreach (InstanceLock @lock in LockList)
		{
			@lock.Write(_worldPacket);
		}
	}
}
