namespace HermesProxy.World.Server.Packets;

public class SaveGuildEmblem : ClientPacket
{
	public WowGuid128 DesignerGUID;

	public uint EmblemStyle;

	public uint EmblemColor;

	public uint BorderStyle;

	public uint BorderColor;

	public uint BackgroundColor;

	public SaveGuildEmblem(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		DesignerGUID = _worldPacket.ReadPackedGuid128();
		EmblemStyle = _worldPacket.ReadUInt32();
		EmblemColor = _worldPacket.ReadUInt32();
		BorderStyle = _worldPacket.ReadUInt32();
		BorderColor = _worldPacket.ReadUInt32();
		BackgroundColor = _worldPacket.ReadUInt32();
	}
}
