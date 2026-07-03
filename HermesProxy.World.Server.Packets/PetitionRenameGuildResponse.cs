using System;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class PetitionRenameGuildResponse : ServerPacket
{
	public WowGuid128 PetitionGuid;

	public string NewGuildName;

	public PetitionRenameGuildResponse()
		: base(Opcode.SMSG_PETITION_RENAME_GUILD_RESPONSE)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(PetitionGuid);
		_worldPacket.WriteBits(NewGuildName.GetByteCount(), 7);
		_worldPacket.FlushBits();
		_worldPacket.WriteString(NewGuildName);
	}
}
