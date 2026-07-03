using System.Collections.Generic;
using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class UnlearnedSpells : ServerPacket
{
	public List<uint> Spells = new List<uint>();

	public bool SuppressMessaging;

	public UnlearnedSpells()
		: base(Opcode.SMSG_UNLEARNED_SPELLS, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteInt32(Spells.Count);
		foreach (uint spell in Spells)
		{
			_worldPacket.WriteUInt32(spell);
		}
		_worldPacket.WriteBit(SuppressMessaging);
		_worldPacket.FlushBits();
	}
}
