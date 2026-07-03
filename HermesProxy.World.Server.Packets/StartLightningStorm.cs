using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class StartLightningStorm : ServerPacket
{
	public uint LightningStormId;

	public StartLightningStorm()
		: base(Opcode.SMSG_START_LIGHTNING_STORM, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt32(LightningStormId);
	}
}
