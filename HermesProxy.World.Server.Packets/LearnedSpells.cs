using System.Collections.Generic;
using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class LearnedSpells : ServerPacket
{
	public List<uint> Spells = new List<uint>();

	public List<int> FavoriteSpellID = new List<int>();

	public uint SpecializationID;

	public bool SuppressMessaging;

	public LearnedSpells()
		: base(Opcode.SMSG_LEARNED_SPELLS, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteInt32(Spells.Count);
		_worldPacket.WriteInt32(FavoriteSpellID.Count);
		_worldPacket.WriteUInt32(SpecializationID);
		foreach (uint spell in Spells)
		{
			_worldPacket.WriteUInt32(spell);
		}
		foreach (int item in FavoriteSpellID)
		{
			_worldPacket.WriteInt32(item);
		}
		_worldPacket.WriteBit(SuppressMessaging);
		_worldPacket.FlushBits();
	}
}
