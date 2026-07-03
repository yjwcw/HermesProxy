using System.Collections.Generic;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class WhoResponsePkt : ServerPacket
{
	public uint RequestID;

	public List<WhoEntry> Players = new List<WhoEntry>();

	public WhoResponsePkt()
		: base(Opcode.SMSG_WHO)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt32(RequestID);
		_worldPacket.WriteBits(Players.Count, 6);
		_worldPacket.FlushBits();
		Players.ForEach(delegate(WhoEntry p)
		{
			p.Write(_worldPacket);
		});
	}
}
