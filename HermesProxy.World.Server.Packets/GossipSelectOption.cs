namespace HermesProxy.World.Server.Packets;

public class GossipSelectOption : ClientPacket
{
	public WowGuid128 GossipUnit;

	public uint GossipIndex;

	public uint GossipID;

	public string PromotionCode;

	public GossipSelectOption(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		GossipUnit = _worldPacket.ReadPackedGuid128();
		GossipID = _worldPacket.ReadUInt32();
		GossipIndex = _worldPacket.ReadUInt32();
		uint length = _worldPacket.ReadBits<uint>(8);
		PromotionCode = _worldPacket.ReadString(length);
	}
}
