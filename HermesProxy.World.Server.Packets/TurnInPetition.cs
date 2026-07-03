namespace HermesProxy.World.Server.Packets;

public class TurnInPetition : ClientPacket
{
	public WowGuid128 Item;

	public uint BackgroundColor;

	public uint EmblemStyle;

	public uint EmblemColor;

	public uint BorderStyle;

	public uint BorderColor;

	public TurnInPetition(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		Item = _worldPacket.ReadPackedGuid128();
		if (_worldPacket.CanRead())
		{
			BackgroundColor = _worldPacket.ReadUInt32();
			EmblemStyle = _worldPacket.ReadUInt32();
			EmblemColor = _worldPacket.ReadUInt32();
			BorderStyle = _worldPacket.ReadUInt32();
			BorderColor = _worldPacket.ReadUInt32();
		}
	}
}
