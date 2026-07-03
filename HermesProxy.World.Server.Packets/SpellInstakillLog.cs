using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class SpellInstakillLog : ServerPacket
{
	public WowGuid128 TargetGUID;

	public WowGuid128 CasterGUID;

	public uint SpellID;

	public SpellInstakillLog()
		: base(Opcode.SMSG_SPELL_INSTAKILL_LOG, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(TargetGUID);
		_worldPacket.WritePackedGuid128(CasterGUID);
		_worldPacket.WriteUInt32(SpellID);
	}
}
