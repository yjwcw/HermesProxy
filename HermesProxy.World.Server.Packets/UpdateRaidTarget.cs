namespace HermesProxy.World.Server.Packets;

internal class UpdateRaidTarget : ClientPacket
{
	public sbyte PartyIndex;

	public WowGuid128 Target;

	public sbyte Symbol;

	public UpdateRaidTarget(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		PartyIndex = _worldPacket.ReadInt8();
		Target = _worldPacket.ReadPackedGuid128();
		Symbol = _worldPacket.ReadInt8();
	}
}
