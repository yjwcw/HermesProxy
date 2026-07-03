using System;

namespace HermesProxy.World.Server.Packets;

public class WhoEntry
{
	public PlayerGuidLookupData PlayerData = new PlayerGuidLookupData();

	public WowGuid128 GuildGUID = WowGuid128.Empty;

	public uint GuildVirtualRealmAddress;

	public string GuildName = "";

	public int AreaID;

	public bool IsGM;

	public void Write(WorldPacket data)
	{
		PlayerData.Write(data);
		data.WritePackedGuid128(GuildGUID);
		data.WriteUInt32(GuildVirtualRealmAddress);
		data.WriteInt32(AreaID);
		data.WriteBits(GuildName.GetByteCount(), 7);
		data.WriteBit(IsGM);
		data.WriteString(GuildName);
		data.FlushBits();
	}
}
