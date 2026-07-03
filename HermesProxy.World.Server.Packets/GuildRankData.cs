using System;

namespace HermesProxy.World.Server.Packets;

public class GuildRankData
{
	public byte RankID;

	public uint RankOrder;

	public uint Flags;

	public int WithdrawGoldLimit;

	public string RankName;

	public uint[] TabFlags = new uint[6];

	public uint[] TabWithdrawItemLimit = new uint[6];

	public void Write(WorldPacket data)
	{
		data.WriteUInt8(RankID);
		data.WriteUInt32(RankOrder);
		data.WriteUInt32(Flags);
		data.WriteInt32(WithdrawGoldLimit);
		for (byte b = 0; b < 6; b++)
		{
			data.WriteUInt32(TabFlags[b]);
			data.WriteUInt32(TabWithdrawItemLimit[b]);
		}
		data.WriteBits(RankName.GetByteCount(), 7);
		data.WriteString(RankName);
	}
}
