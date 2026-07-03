namespace HermesProxy.World.Server.Packets;

public class GuildSetRankPermissions : ClientPacket
{
	public uint RankID;

	public uint RankOrder;

	public int WithdrawGoldLimit;

	public uint Flags;

	public uint OldFlags;

	public uint[] TabFlags = new uint[6];

	public uint[] TabWithdrawItemLimit = new uint[6];

	public string RankName;

	public GuildSetRankPermissions(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		RankID = _worldPacket.ReadUInt32();
		RankOrder = _worldPacket.ReadUInt32();
		Flags = _worldPacket.ReadUInt32();
		WithdrawGoldLimit = _worldPacket.ReadInt32();
		for (byte b = 0; b < 6; b++)
		{
			TabFlags[b] = _worldPacket.ReadUInt32();
			TabWithdrawItemLimit[b] = _worldPacket.ReadUInt32();
		}
		OldFlags = _worldPacket.ReadUInt32();
		_worldPacket.ResetBitPos();
		uint length = _worldPacket.ReadBits<uint>(7);
		RankName = _worldPacket.ReadString(length);
	}
}
