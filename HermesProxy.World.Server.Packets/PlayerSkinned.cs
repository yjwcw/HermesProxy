using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class PlayerSkinned : ServerPacket
{
	public bool FreeRepop;

	public PlayerSkinned()
		: base(Opcode.SMSG_PLAYER_SKINNED, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteBit(FreeRepop);
		_worldPacket.FlushBits();
	}
}
