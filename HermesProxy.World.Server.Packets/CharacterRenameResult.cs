using System;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class CharacterRenameResult : ServerPacket
{
	public string Name = "";

	public byte Result;

	public WowGuid128 Guid;

	public CharacterRenameResult()
		: base(Opcode.SMSG_CHARACTER_RENAME_RESULT)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt8(Result);
		_worldPacket.WriteBit(Guid != null);
		_worldPacket.WriteBits(Name.GetByteCount(), 6);
		_worldPacket.FlushBits();
		if (Guid != null)
		{
			_worldPacket.WritePackedGuid128(Guid);
		}
		_worldPacket.WriteString(Name);
	}
}
