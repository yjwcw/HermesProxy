namespace HermesProxy.World.Server.Packets;

internal class SetEveryoneIsAssistant : ClientPacket
{
	public byte PartyIndex;

	public bool Apply;

	public SetEveryoneIsAssistant(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		PartyIndex = _worldPacket.ReadUInt8();
		Apply = _worldPacket.HasBit();
	}
}
