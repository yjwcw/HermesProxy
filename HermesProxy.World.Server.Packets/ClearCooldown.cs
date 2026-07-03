using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class ClearCooldown : ServerPacket
{
	public bool IsPet;

	public uint SpellID;

	public bool ClearOnHold;

	public ClearCooldown()
		: base(Opcode.SMSG_CLEAR_COOLDOWN, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt32(SpellID);
		_worldPacket.WriteBit(ClearOnHold);
		_worldPacket.WriteBit(IsPet);
		_worldPacket.FlushBits();
	}
}
