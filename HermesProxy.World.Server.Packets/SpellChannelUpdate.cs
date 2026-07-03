using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class SpellChannelUpdate : ServerPacket
{
	public WowGuid128 CasterGUID;

	public int TimeRemaining;

	public SpellChannelUpdate()
		: base(Opcode.SMSG_SPELL_CHANNEL_UPDATE, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(CasterGUID);
		_worldPacket.WriteInt32(TimeRemaining);
	}
}
