namespace HermesProxy.World.Server.Packets;

public class PartyLFGInfo
{
	public byte MyFlags;

	public uint Slot;

	public byte BootCount;

	public uint MyRandomSlot;

	public bool Aborted;

	public byte MyPartialClear;

	public float MyGearDiff;

	public byte MyStrangerCount;

	public byte MyKickVoteCount;

	public bool MyFirstReward;

	public void Write(WorldPacket data)
	{
		data.WriteUInt8(MyFlags);
		data.WriteUInt32(Slot);
		data.WriteUInt32(MyRandomSlot);
		data.WriteUInt8(MyPartialClear);
		data.WriteFloat(MyGearDiff);
		data.WriteUInt8(MyStrangerCount);
		data.WriteUInt8(MyKickVoteCount);
		data.WriteUInt8(BootCount);
		data.WriteBit(Aborted);
		data.WriteBit(MyFirstReward);
		data.FlushBits();
	}
}
