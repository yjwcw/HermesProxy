using Framework.GameMath;

namespace HermesProxy.World.Server.Packets;

internal class MinimapPingClient : ClientPacket
{
	public Vector2 Position;

	public sbyte PartyIndex;

	public MinimapPingClient(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		Position = _worldPacket.ReadVector2();
		PartyIndex = _worldPacket.ReadInt8();
	}
}
