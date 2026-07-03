using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class DuelRequested : ServerPacket
{
	public WowGuid128 ArbiterGUID;

	public WowGuid128 RequestedByGUID;

	public WowGuid128 RequestedByWowAccount;

	public DuelRequested()
		: base(Opcode.SMSG_DUEL_REQUESTED, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(ArbiterGUID);
		_worldPacket.WritePackedGuid128(RequestedByGUID);
		_worldPacket.WritePackedGuid128(RequestedByWowAccount);
	}
}
