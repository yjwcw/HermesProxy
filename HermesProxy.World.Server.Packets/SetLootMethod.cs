using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class SetLootMethod : ClientPacket
{
	public sbyte PartyIndex;

	public LootMethod LootMethod;

	public WowGuid128 LootMasterGUID;

	public uint LootThreshold;

	public SetLootMethod(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		PartyIndex = _worldPacket.ReadInt8();
		LootMethod = (LootMethod)_worldPacket.ReadUInt8();
		LootMasterGUID = _worldPacket.ReadPackedGuid128();
		LootThreshold = _worldPacket.ReadUInt32();
	}
}
