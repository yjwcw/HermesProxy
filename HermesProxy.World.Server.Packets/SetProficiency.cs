using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class SetProficiency : ServerPacket
{
	public uint ProficiencyMask;

	public byte ProficiencyClass;

	public SetProficiency()
		: base(Opcode.SMSG_SET_PROFICIENCY, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt32(ProficiencyMask);
		_worldPacket.WriteUInt8(ProficiencyClass);
	}
}
