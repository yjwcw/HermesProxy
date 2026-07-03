using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class TaxiNodeStatusPkt : ServerPacket
{
	public WowGuid128 FlightMaster;

	public TaxiNodeStatus Status;

	public TaxiNodeStatusPkt()
		: base(Opcode.SMSG_TAXI_NODE_STATUS)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(FlightMaster);
		_worldPacket.WriteBits(Status, 2);
		_worldPacket.FlushBits();
	}
}
