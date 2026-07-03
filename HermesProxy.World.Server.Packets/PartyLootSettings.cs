using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class PartyLootSettings
{
	public LootMethod Method;

	public WowGuid128 LootMaster;

	public byte Threshold;

	public void Write(WorldPacket data)
	{
		data.WriteUInt8((byte)Method);
		data.WritePackedGuid128(LootMaster);
		data.WriteUInt8(Threshold);
	}
}
