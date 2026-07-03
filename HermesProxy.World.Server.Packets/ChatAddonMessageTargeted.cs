namespace HermesProxy.World.Server.Packets;

internal class ChatAddonMessageTargeted : ClientPacket
{
	public ChatAddonMessageParams Params = new ChatAddonMessageParams();

	public WowGuid128 ChannelGuid;

	public string Target;

	public ChatAddonMessageTargeted(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		uint length = _worldPacket.ReadBits<uint>(9);
		Params.Read(_worldPacket);
		ChannelGuid = _worldPacket.ReadPackedGuid128();
		Target = _worldPacket.ReadString(length);
	}
}
