using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class TutorialSetFlag : ClientPacket
{
	public TutorialAction Action;

	public uint TutorialBit;

	public TutorialSetFlag(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		Action = (TutorialAction)_worldPacket.ReadBits<byte>(2);
		if (Action == TutorialAction.Update)
		{
			TutorialBit = _worldPacket.ReadUInt32();
		}
	}
}
