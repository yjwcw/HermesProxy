namespace HermesProxy.World.Server.Packets;

public class UserClientUpdateAccountData : ClientPacket
{
	public WowGuid128 PlayerGuid;

	public long Time;

	public uint Size;

	public uint DataType;

	public byte[] CompressedData;

	public UserClientUpdateAccountData(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		PlayerGuid = _worldPacket.ReadPackedGuid128();
		Time = _worldPacket.ReadInt64();
		Size = _worldPacket.ReadUInt32();
		if (ModernVersion.GetAccountDataCount() <= 8)
		{
			DataType = _worldPacket.ReadBits<uint>(3);
		}
		else
		{
			DataType = _worldPacket.ReadBits<uint>(4);
		}
		uint num = _worldPacket.ReadUInt32();
		if (num != 0)
		{
			CompressedData = _worldPacket.ReadBytes(num);
		}
	}
}
