using System;
using System.Collections.Generic;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class QueryPageTextResponse : ServerPacket
{
	public struct PageTextInfo
	{
		public uint Id;

		public uint NextPageID;

		public int PlayerConditionID;

		public byte Flags;

		public string Text;

		public void Write(WorldPacket data)
		{
			data.WriteUInt32(Id);
			data.WriteUInt32(NextPageID);
			data.WriteInt32(PlayerConditionID);
			data.WriteUInt8(Flags);
			data.WriteBits(Text.GetByteCount(), 12);
			data.FlushBits();
			data.WriteString(Text);
		}
	}

	public uint PageTextID;

	public bool Allow;

	public List<PageTextInfo> Pages = new List<PageTextInfo>();

	public QueryPageTextResponse()
		: base(Opcode.SMSG_QUERY_PAGE_TEXT_RESPONSE)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt32(PageTextID);
		_worldPacket.WriteBit(Allow);
		_worldPacket.FlushBits();
		if (!Allow)
		{
			return;
		}
		_worldPacket.WriteInt32(Pages.Count);
		foreach (PageTextInfo page in Pages)
		{
			page.Write(_worldPacket);
		}
	}
}
