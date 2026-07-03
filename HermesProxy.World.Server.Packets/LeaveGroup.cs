namespace HermesProxy.World.Server.Packets;

internal class LeaveGroup : ClientPacket
{
	public sbyte PartyIndex;

	public LeaveGroup(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		PartyIndex = _worldPacket.ReadInt8();
	}
}
