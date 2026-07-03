using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class UpdateAccountData : ServerPacket
{
	public WowGuid128 Player;

	public long Time;

	public uint Size;

	public uint DataType;

	public byte[] CompressedData;

	public UpdateAccountData(AccountData data)
		: base(Opcode.SMSG_UPDATE_ACCOUNT_DATA)
	{
		Player = data.Guid;
		Time = data.Timestamp;
		Size = data.UncompressedSize;
		DataType = data.Type;
		CompressedData = data.CompressedData;
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(Player);
		_worldPacket.WriteInt64(Time);
		_worldPacket.WriteUInt32(Size);
		if (ModernVersion.GetAccountDataCount() <= 8)
		{
			_worldPacket.WriteBits(DataType, 3);
		}
		else
		{
			_worldPacket.WriteBits(DataType, 4);
		}
		if (CompressedData == null)
		{
			_worldPacket.WriteUInt32(0u);
			return;
		}
		_worldPacket.WriteInt32(CompressedData.Length);
		_worldPacket.WriteBytes(CompressedData);
	}
}
