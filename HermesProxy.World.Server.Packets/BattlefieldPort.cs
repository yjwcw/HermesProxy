namespace HermesProxy.World.Server.Packets;

internal class BattlefieldPort : ClientPacket
{
	public RideTicket Ticket = new RideTicket();

	public bool AcceptedInvite;

	public BattlefieldPort(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		Ticket.Read(_worldPacket);
		AcceptedInvite = _worldPacket.HasBit();
	}
}
