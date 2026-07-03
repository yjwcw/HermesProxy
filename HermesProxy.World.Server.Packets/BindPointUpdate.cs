using Framework.Constants;
using Framework.GameMath;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class BindPointUpdate : ServerPacket
{
	public uint BindMapID = uint.MaxValue;

	public Vector3 BindPosition;

	public uint BindAreaID;

	public BindPointUpdate()
		: base(Opcode.SMSG_BIND_POINT_UPDATE, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteVector3(BindPosition);
		_worldPacket.WriteUInt32(BindMapID);
		_worldPacket.WriteUInt32(BindAreaID);
	}
}
