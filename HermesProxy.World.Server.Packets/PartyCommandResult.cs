using System;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class PartyCommandResult : ServerPacket
{
	public string Name;

	public byte Command;

	public byte Result;

	public uint ResultData;

	public WowGuid128 ResultGUID = WowGuid128.Empty;

	public PartyCommandResult()
		: base(Opcode.SMSG_PARTY_COMMAND_RESULT)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteBits(Name.GetByteCount(), 9);
		_worldPacket.WriteBits(Command, 4);
		_worldPacket.WriteBits(Result, 6);
		_worldPacket.WriteUInt32(ResultData);
		_worldPacket.WritePackedGuid128(ResultGUID);
		_worldPacket.WriteString(Name);
	}
}
