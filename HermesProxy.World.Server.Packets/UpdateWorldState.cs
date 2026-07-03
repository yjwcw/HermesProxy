using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class UpdateWorldState : ServerPacket
{
	public uint VariableID;

	public int Value;

	public bool Hidden;

	public UpdateWorldState()
		: base(Opcode.SMSG_UPDATE_WORLD_STATE, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt32(VariableID);
		_worldPacket.WriteInt32(Value);
		_worldPacket.WriteBit(Hidden);
		_worldPacket.FlushBits();
	}
}
