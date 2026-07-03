using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class FishNotHooked : ServerPacket
{
	public FishNotHooked()
		: base(Opcode.SMSG_FISH_NOT_HOOKED)
	{
	}

	public override void Write()
	{
	}
}
