using System.Collections.Generic;
using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class SupercededSpells : ServerPacket
{
	public List<uint> SpellID = new List<uint>();

	public List<uint> Superceded = new List<uint>();

	public List<int> FavoriteSpellID = new List<int>();

	public SupercededSpells()
		: base(Opcode.SMSG_SUPERCEDED_SPELLS, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteInt32(SpellID.Count);
		_worldPacket.WriteInt32(Superceded.Count);
		_worldPacket.WriteInt32(FavoriteSpellID.Count);
		foreach (uint item in SpellID)
		{
			_worldPacket.WriteUInt32(item);
		}
		foreach (uint item2 in Superceded)
		{
			_worldPacket.WriteUInt32(item2);
		}
		foreach (int item3 in FavoriteSpellID)
		{
			_worldPacket.WriteInt32(item3);
		}
	}
}
