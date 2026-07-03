using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class CreateCharacter : ClientPacket
{
	public CharacterCreateInfo CreateInfo;

	public CreateCharacter(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		CreateInfo = new CharacterCreateInfo();
		uint length = _worldPacket.ReadBits<uint>(6);
		bool num = _worldPacket.HasBit();
		CreateInfo.IsTrialBoost = _worldPacket.HasBit();
		CreateInfo.UseNPE = _worldPacket.HasBit();
		CreateInfo.RaceId = (Race)_worldPacket.ReadUInt8();
		CreateInfo.ClassId = (Class)_worldPacket.ReadUInt8();
		CreateInfo.Sex = (Gender)_worldPacket.ReadUInt8();
		uint num2 = _worldPacket.ReadUInt32();
		CreateInfo.Name = _worldPacket.ReadString(length);
		if (num)
		{
			CreateInfo.TemplateSet = _worldPacket.ReadUInt32();
		}
		for (int i = 0; i < num2; i++)
		{
			CreateInfo.Customizations[i] = new ChrCustomizationChoice(_worldPacket.ReadUInt32(), _worldPacket.ReadUInt32());
		}
		CreateInfo.Customizations.Sort();
	}
}
