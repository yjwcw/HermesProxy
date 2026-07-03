using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class InitialSetup : ServerPacket
{
	public byte ServerExpansionLevel;

	public byte ServerExpansionTier;

	public InitialSetup()
		: base(Opcode.SMSG_INITIAL_SETUP, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt8(ServerExpansionLevel);
		_worldPacket.WriteUInt8(ServerExpansionTier);
	}
}
