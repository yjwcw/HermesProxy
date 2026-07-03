using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class SetFactionVisible : ServerPacket
{
	public uint FactionIndex;

	public SetFactionVisible(bool visible)
		: base(visible ? Opcode.SMSG_SET_FACTION_VISIBLE : Opcode.SMSG_SET_FACTION_NOT_VISIBLE, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt32(FactionIndex);
	}
}
