using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class DurabilityDamageDeath : ServerPacket
{
	public uint Percent;

	public DurabilityDamageDeath()
		: base(Opcode.SMSG_DURABILITY_DAMAGE_DEATH)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt32(Percent);
	}
}
