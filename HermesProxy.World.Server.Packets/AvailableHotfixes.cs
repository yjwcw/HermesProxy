using System.Collections.Generic;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;

namespace HermesProxy.World.Server.Packets;

internal class AvailableHotfixes : ServerPacket
{
	public uint VirtualRealmAddress;

	public AvailableHotfixes()
		: base(Opcode.SMSG_AVAILABLE_HOTFIXES)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt32(VirtualRealmAddress);
		_worldPacket.WriteInt32(GameData.Hotfixes.Count);
		foreach (KeyValuePair<uint, HotfixRecord> hotfix in GameData.Hotfixes)
		{
			hotfix.Value.WriteAvailable(_worldPacket);
		}
	}
}
