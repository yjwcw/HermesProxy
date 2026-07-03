using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class CooldownEvent : ServerPacket
{
	public bool IsPet;

	public uint SpellID;

	public CooldownEvent()
		: base(Opcode.SMSG_COOLDOWN_EVENT, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt32(SpellID);
		_worldPacket.WriteBit(IsPet);
		_worldPacket.FlushBits();
	}
}
