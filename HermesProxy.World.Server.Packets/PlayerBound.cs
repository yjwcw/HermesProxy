using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class PlayerBound : ServerPacket
{
	public WowGuid128 BinderGUID;

	public uint AreaID;

	public PlayerBound()
		: base(Opcode.SMSG_PLAYER_BOUND)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(BinderGUID);
		_worldPacket.WriteUInt32(AreaID);
	}
}
