using System.Collections.Generic;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;

namespace HermesProxy.World.Server.Packets;

public class HotFixMessage : ServerPacket
{
	public List<HotfixRecord> Hotfixes = new List<HotfixRecord>();

	public HotFixMessage()
		: base(Opcode.SMSG_HOTFIX_MESSAGE)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteInt32(Hotfixes.Count);
		uint num = 0u;
		foreach (HotfixRecord hotfix in Hotfixes)
		{
			num += hotfix.HotfixContent.GetSize();
			hotfix.WriteHotFixMessageContent(_worldPacket);
		}
		_worldPacket.WriteUInt32(num);
		foreach (HotfixRecord hotfix2 in Hotfixes)
		{
			_worldPacket.WriteBytes(hotfix2.HotfixContent);
		}
	}
}
