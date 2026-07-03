using System.Collections.Generic;
using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class EmoteMessage : ServerPacket
{
	public WowGuid128 Guid;

	public uint EmoteID;

	public int SequenceVariation;

	public List<uint> SpellVisualKitIDs = new List<uint>();

	public EmoteMessage()
		: base(Opcode.SMSG_EMOTE, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(Guid);
		_worldPacket.WriteUInt32(EmoteID);
		if (!ModernVersion.AddedInVersion(9, 0, 5, 1, 14, 0, 2, 5, 1))
		{
			return;
		}
		_worldPacket.WriteInt32(SpellVisualKitIDs.Count);
		if (ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 2, 2, 5, 3))
		{
			_worldPacket.WriteInt32(SequenceVariation);
		}
		foreach (uint spellVisualKitID in SpellVisualKitIDs)
		{
			_worldPacket.WriteUInt32(spellVisualKitID);
		}
	}
}
