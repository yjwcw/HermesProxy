using Framework.GameMath;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class CorpseLocation : ServerPacket
{
	public WowGuid128 Player;

	public WowGuid128 Transport;

	public Vector3 Position;

	public int ActualMapID;

	public int MapID;

	public bool Valid;

	public CorpseLocation()
		: base(Opcode.SMSG_CORPSE_LOCATION)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteBit(Valid);
		_worldPacket.FlushBits();
		_worldPacket.WritePackedGuid128(Player);
		_worldPacket.WriteInt32(ActualMapID);
		_worldPacket.WriteVector3(Position);
		_worldPacket.WriteInt32(MapID);
		_worldPacket.WritePackedGuid128(Transport);
	}
}
