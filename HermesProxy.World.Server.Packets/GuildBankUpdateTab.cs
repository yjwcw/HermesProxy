namespace HermesProxy.World.Server.Packets;

public class GuildBankUpdateTab : ClientPacket
{
	public WowGuid128 BankGuid;

	public byte BankTab;

	public string Name;

	public string Icon;

	public GuildBankUpdateTab(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		BankGuid = _worldPacket.ReadPackedGuid128();
		BankTab = _worldPacket.ReadUInt8();
		_worldPacket.ResetBitPos();
		uint length = _worldPacket.ReadBits<uint>(7);
		uint length2 = _worldPacket.ReadBits<uint>(9);
		Name = _worldPacket.ReadString(length);
		Icon = _worldPacket.ReadString(length2);
	}
}
