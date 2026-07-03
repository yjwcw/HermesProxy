namespace HermesProxy.World.Server.Packets;

internal class TotemDestroyed : ClientPacket
{
	public byte Slot;

	public WowGuid Guid;

	public TotemDestroyed(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		Slot = _worldPacket.ReadUInt8();
		Guid = _worldPacket.ReadPackedGuid128();
	}
}
