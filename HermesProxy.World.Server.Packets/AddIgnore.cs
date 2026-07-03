namespace HermesProxy.World.Server.Packets;

public class AddIgnore : ClientPacket
{
	private WowGuid128 AccountGuid;

	public string Name;

	public AddIgnore(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		uint length = _worldPacket.ReadBits<uint>(9);
		if (ModernVersion.AddedInVersion(9, 1, 5, 1, 14, 1, 2, 5, 3))
		{
			AccountGuid = _worldPacket.ReadPackedGuid128();
		}
		Name = _worldPacket.ReadString(length);
	}
}
