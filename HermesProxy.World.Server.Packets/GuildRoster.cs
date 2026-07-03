using System;
using System.Collections.Generic;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class GuildRoster : ServerPacket
{
	public List<GuildRosterMemberData> MemberData = new List<GuildRosterMemberData>();

	public string WelcomeText;

	public string InfoText;

	public uint CreateDate;

	public uint NumAccounts;

	public int GuildFlags = 2;

	public GuildRoster()
		: base(Opcode.SMSG_GUILD_ROSTER)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt32(NumAccounts);
		_worldPacket.WritePackedTime(CreateDate);
		_worldPacket.WriteInt32(GuildFlags);
		_worldPacket.WriteInt32(MemberData.Count);
		_worldPacket.WriteBits(WelcomeText.GetByteCount(), 11);
		_worldPacket.WriteBits(InfoText.GetByteCount(), 11);
		_worldPacket.FlushBits();
		MemberData.ForEach(delegate(GuildRosterMemberData p)
		{
			p.Write(_worldPacket);
		});
		_worldPacket.WriteString(WelcomeText);
		_worldPacket.WriteString(InfoText);
	}
}
