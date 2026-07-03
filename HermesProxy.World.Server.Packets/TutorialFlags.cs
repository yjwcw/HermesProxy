using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class TutorialFlags : ServerPacket
{
	public uint[] TutorialData = new uint[8];

	public TutorialFlags()
		: base(Opcode.SMSG_TUTORIAL_FLAGS)
	{
	}

	public override void Write()
	{
		for (byte b = 0; b < 8; b++)
		{
			_worldPacket.WriteUInt32(TutorialData[b]);
		}
	}
}
