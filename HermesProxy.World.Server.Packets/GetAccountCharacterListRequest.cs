namespace HermesProxy.World.Server.Packets;

public class GetAccountCharacterListRequest : ClientPacket
{
	public uint Token;

	public GetAccountCharacterListRequest(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		Token = _worldPacket.ReadUInt32();
	}
}
