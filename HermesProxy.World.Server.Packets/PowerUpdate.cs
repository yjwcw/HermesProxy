using System.Collections.Generic;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class PowerUpdate : ServerPacket
{
	public WowGuid128 Guid;

	public List<PowerUpdatePower> Powers;

	public PowerUpdate(WowGuid128 guid)
		: base(Opcode.SMSG_POWER_UPDATE)
	{
		Guid = guid;
		Powers = new List<PowerUpdatePower>();
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(Guid);
		_worldPacket.WriteInt32(Powers.Count);
		foreach (PowerUpdatePower power in Powers)
		{
			_worldPacket.WriteInt32(power.Power);
			_worldPacket.WriteUInt8(power.PowerType);
		}
	}
}
