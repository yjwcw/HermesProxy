namespace HermesProxy.World.Server.Packets;

internal class SwapSubGroups : ClientPacket
{
	public WowGuid128 FirstTarget;

	public WowGuid128 SecondTarget;

	public sbyte PartyIndex;

	public SwapSubGroups(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		PartyIndex = _worldPacket.ReadInt8();
		FirstTarget = _worldPacket.ReadPackedGuid128();
		SecondTarget = _worldPacket.ReadPackedGuid128();
	}
}
