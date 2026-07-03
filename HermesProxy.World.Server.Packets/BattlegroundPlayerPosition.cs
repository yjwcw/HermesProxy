using Framework.GameMath;

namespace HermesProxy.World.Server.Packets;

public struct BattlegroundPlayerPosition
{
	public WowGuid128 Guid;

	public Vector2 Pos;

	public sbyte IconID;

	public sbyte ArenaSlot;

	public void Write(WorldPacket data)
	{
		data.WritePackedGuid128(Guid);
		data.WriteVector2(Pos);
		data.WriteInt8(IconID);
		data.WriteInt8(ArenaSlot);
	}
}
