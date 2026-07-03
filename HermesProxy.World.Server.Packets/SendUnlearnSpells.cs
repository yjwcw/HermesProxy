using System.Collections.Generic;
using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class SendUnlearnSpells : ServerPacket
{
	public List<uint> Spells = new List<uint>();

	public SendUnlearnSpells()
		: base(Opcode.SMSG_SEND_UNLEARN_SPELLS, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteInt32(Spells.Count);
		foreach (uint spell in Spells)
		{
			_worldPacket.WriteUInt32(spell);
		}
	}
}
