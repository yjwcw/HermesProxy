namespace HermesProxy.World.Server.Packets;

public class ChatAddonMessage : ClientPacket
{
	public ChatAddonMessageParams Params = new ChatAddonMessageParams();

	public ChatAddonMessage(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		Params.Read(_worldPacket);
	}
}
