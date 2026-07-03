using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class ConnectionStatus : ServerPacket
{
	public byte State;

	public bool SuppressNotification;

	public ConnectionStatus()
		: base(Opcode.SMSG_BATTLE_NET_CONNECTION_STATUS)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteBits(State, 2);
		_worldPacket.WriteBit(SuppressNotification);
		_worldPacket.FlushBits();
	}
}
