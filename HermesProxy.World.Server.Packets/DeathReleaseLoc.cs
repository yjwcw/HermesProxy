using Framework.GameMath;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class DeathReleaseLoc : ServerPacket
{
	public int MapID;

	public Vector3 Location;

	public DeathReleaseLoc()
		: base(Opcode.SMSG_DEATH_RELEASE_LOC)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteInt32(MapID);
		_worldPacket.WriteVector3(Location);
	}
}
