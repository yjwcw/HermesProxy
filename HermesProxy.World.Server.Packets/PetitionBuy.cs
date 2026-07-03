namespace HermesProxy.World.Server.Packets;

public class PetitionBuy : ClientPacket
{
	public WowGuid128 Unit;

	public uint Index;

	public string Title;

	public PetitionBuy(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		uint length = _worldPacket.ReadBits<uint>(7);
		Unit = _worldPacket.ReadPackedGuid128();
		Index = _worldPacket.ReadUInt32();
		Title = _worldPacket.ReadString(length);
	}
}
