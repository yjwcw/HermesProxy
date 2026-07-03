using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class LevelUpInfo : ServerPacket
{
	public int Level;

	public int HealthDelta;

	public int[] PowerDelta = new int[7];

	public int[] StatDelta = new int[5];

	public int NumNewTalents;

	public int NumNewPvpTalentSlots;

	public LevelUpInfo()
		: base(Opcode.SMSG_LEVEL_UP_INFO)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteInt32(Level);
		_worldPacket.WriteInt32(HealthDelta);
		for (int i = 0; i < ModernVersion.GetPowerCountForClientVersion(); i++)
		{
			_worldPacket.WriteInt32(PowerDelta[i]);
		}
		int[] statDelta = StatDelta;
		foreach (int data in statDelta)
		{
			_worldPacket.WriteInt32(data);
		}
		_worldPacket.WriteInt32(NumNewTalents);
		_worldPacket.WriteInt32(NumNewPvpTalentSlots);
	}
}
