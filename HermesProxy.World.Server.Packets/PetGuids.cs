using System.Collections.Generic;
using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class PetGuids : ServerPacket
{
	public List<WowGuid128> Guids = new List<WowGuid128>();

	public PetGuids()
		: base(Opcode.SMSG_PET_GUIDS, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteInt32(Guids.Count);
		foreach (WowGuid128 guid in Guids)
		{
			_worldPacket.WritePackedGuid128(guid);
		}
	}
}
