using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class SpellPrepare : ServerPacket
{
	public WowGuid128 ClientCastID;

	public WowGuid128 ServerCastID;

	public SpellPrepare()
		: base(Opcode.SMSG_SPELL_PREPARE)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(ClientCastID);
		_worldPacket.WritePackedGuid128(ServerCastID);
	}
}
