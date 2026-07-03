using Framework.GameMath;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class MinimapPing : ServerPacket
{
	public WowGuid128 SenderGUID;

	public Vector2 Position;

	public MinimapPing()
		: base(Opcode.SMSG_MINIMAP_PING)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(SenderGUID);
		_worldPacket.WriteVector2(Position);
	}
}
