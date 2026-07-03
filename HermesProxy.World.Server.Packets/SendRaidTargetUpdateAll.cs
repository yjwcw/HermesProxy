using System;
using System.Collections.Generic;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class SendRaidTargetUpdateAll : ServerPacket
{
	public sbyte PartyIndex;

	public List<Tuple<sbyte, WowGuid128>> TargetIcons = new List<Tuple<sbyte, WowGuid128>>();

	public SendRaidTargetUpdateAll()
		: base(Opcode.SMSG_SEND_RAID_TARGET_UPDATE_ALL)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteInt8(PartyIndex);
		_worldPacket.WriteInt32(TargetIcons.Count);
		foreach (Tuple<sbyte, WowGuid128> targetIcon in TargetIcons)
		{
			_worldPacket.WritePackedGuid128(targetIcon.Item2);
			_worldPacket.WriteInt8(targetIcon.Item1);
		}
	}
}
