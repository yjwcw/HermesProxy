using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class PlayerTabardVendorActivate : ServerPacket
{
	public WowGuid128 DesignerGUID;

	public PlayerTabardVendorActivate()
		: base(Opcode.SMSG_PLAYER_TABARD_VENDOR_ACTIVATE)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(DesignerGUID);
	}
}
