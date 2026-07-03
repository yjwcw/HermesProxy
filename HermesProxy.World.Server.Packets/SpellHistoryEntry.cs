namespace HermesProxy.World.Server.Packets;

public class SpellHistoryEntry
{
	public uint SpellID;

	public uint ItemID;

	public uint Category;

	public int RecoveryTime;

	public int CategoryRecoveryTime;

	public float ModRate = 1f;

	public bool OnHold;

	private uint? unused622_1;

	private uint? unused622_2;

	public void Write(WorldPacket data)
	{
		data.WriteUInt32(SpellID);
		data.WriteUInt32(ItemID);
		data.WriteUInt32(Category);
		data.WriteInt32(RecoveryTime);
		data.WriteInt32(CategoryRecoveryTime);
		data.WriteFloat(ModRate);
		data.WriteBit(unused622_1.HasValue);
		data.WriteBit(unused622_2.HasValue);
		data.WriteBit(OnHold);
		data.FlushBits();
		if (unused622_1.HasValue)
		{
			data.WriteUInt32(unused622_1.Value);
		}
		if (unused622_2.HasValue)
		{
			data.WriteUInt32(unused622_2.Value);
		}
	}
}
