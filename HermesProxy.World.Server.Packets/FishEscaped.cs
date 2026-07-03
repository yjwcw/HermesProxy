using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class FishEscaped : ServerPacket
{
	public FishEscaped()
		: base(Opcode.SMSG_FISH_ESCAPED)
	{
	}

	public override void Write()
	{
	}
}
