using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class NewTaxiPath : ServerPacket
{
	public NewTaxiPath()
		: base(Opcode.SMSG_NEW_TAXI_PATH)
	{
	}

	public override void Write()
	{
	}
}
