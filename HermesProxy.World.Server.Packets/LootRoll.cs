using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class LootRoll : ClientPacket
{
	public WowGuid128 LootObj;

	public byte LootListID;

	public RollType RollType;

	public LootRoll(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		LootObj = _worldPacket.ReadPackedGuid128();
		LootListID = _worldPacket.ReadUInt8();
		RollType = (RollType)_worldPacket.ReadUInt8();
	}
}
