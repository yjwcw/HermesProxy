using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class LoadCUFProfiles : ServerPacket
{
	public byte[] Data;

	public LoadCUFProfiles()
		: base(Opcode.SMSG_LOAD_CUF_PROFILES, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteBytes(Data);
	}
}
