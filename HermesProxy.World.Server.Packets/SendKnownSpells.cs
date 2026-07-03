using System.Collections.Generic;
using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class SendKnownSpells : ServerPacket
{
	public bool InitialLogin;

	public List<uint> KnownSpells = new List<uint>();

	public List<uint> FavoriteSpells = new List<uint>();

	public SendKnownSpells()
		: base(Opcode.SMSG_SEND_KNOWN_SPELLS, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteBit(InitialLogin);
		_worldPacket.WriteInt32(KnownSpells.Count);
		_worldPacket.WriteInt32(FavoriteSpells.Count);
		foreach (uint knownSpell in KnownSpells)
		{
			_worldPacket.WriteUInt32(knownSpell);
		}
		foreach (uint favoriteSpell in FavoriteSpells)
		{
			_worldPacket.WriteUInt32(favoriteSpell);
		}
	}
}
