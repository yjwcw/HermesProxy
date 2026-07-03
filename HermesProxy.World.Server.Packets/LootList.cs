using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class LootList : ServerPacket
{
	public WowGuid128 Owner;

	public WowGuid128 LootObj;

	public WowGuid128 Master;

	public WowGuid128 RoundRobinWinner;

	public LootList()
		: base(Opcode.SMSG_LOOT_LIST, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(Owner);
		_worldPacket.WritePackedGuid128(LootObj);
		_worldPacket.WriteBit(Master != null);
		_worldPacket.WriteBit(RoundRobinWinner != null);
		_worldPacket.FlushBits();
		if (Master != null)
		{
			_worldPacket.WritePackedGuid128(Master);
		}
		if (RoundRobinWinner != null)
		{
			_worldPacket.WritePackedGuid128(RoundRobinWinner);
		}
	}
}
