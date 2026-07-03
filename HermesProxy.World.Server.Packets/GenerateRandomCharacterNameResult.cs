using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class GenerateRandomCharacterNameResult : ServerPacket
{
	public bool Success;

	public string Name = "";

	public GenerateRandomCharacterNameResult()
		: base(Opcode.SMSG_GENERATE_RANDOM_CHARACTER_NAME_RESULT)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteBool(Success);
		_worldPacket.WriteBits(Name.Length, 6);
		_worldPacket.WriteString(Name);
	}
}
