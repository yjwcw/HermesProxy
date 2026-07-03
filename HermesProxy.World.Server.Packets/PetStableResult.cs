using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class PetStableResult : ServerPacket
{
	public byte Result;

	public PetStableResult()
		: base(Opcode.SMSG_PET_STABLE_RESULT, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt8(Result);
	}
}
