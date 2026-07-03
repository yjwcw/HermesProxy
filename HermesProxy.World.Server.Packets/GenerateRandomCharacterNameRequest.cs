using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class GenerateRandomCharacterNameRequest : ClientPacket
{
	public Race Race;

	public Gender Sex;

	public GenerateRandomCharacterNameRequest(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		Race = (Race)_worldPacket.ReadUInt8();
		Sex = (Gender)_worldPacket.ReadUInt8();
	}
}
