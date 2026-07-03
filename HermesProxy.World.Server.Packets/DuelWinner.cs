using System;
using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class DuelWinner : ServerPacket
{
	public string BeatenName;

	public string WinnerName;

	public uint BeatenVirtualRealmAddress;

	public uint WinnerVirtualRealmAddress;

	public bool Fled;

	public DuelWinner()
		: base(Opcode.SMSG_DUEL_WINNER, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteBits(BeatenName.GetByteCount(), 6);
		_worldPacket.WriteBits(WinnerName.GetByteCount(), 6);
		_worldPacket.WriteBit(Fled);
		_worldPacket.WriteUInt32(BeatenVirtualRealmAddress);
		_worldPacket.WriteUInt32(WinnerVirtualRealmAddress);
		_worldPacket.WriteString(BeatenName);
		_worldPacket.WriteString(WinnerName);
	}
}
