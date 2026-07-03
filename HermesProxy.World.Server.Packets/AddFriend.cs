namespace HermesProxy.World.Server.Packets;

public class AddFriend : ClientPacket
{
	public string Note;

	public string Name;

	public AddFriend(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		uint length = _worldPacket.ReadBits<uint>(9);
		uint length2 = _worldPacket.ReadBits<uint>(10);
		Name = _worldPacket.ReadString(length);
		Note = _worldPacket.ReadString(length2);
	}
}
