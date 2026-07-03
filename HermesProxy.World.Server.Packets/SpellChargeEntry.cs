namespace HermesProxy.World.Server.Packets;

public class SpellChargeEntry
{
	public uint Category;

	public uint NextRecoveryTime;

	public float ChargeModRate = 1f;

	public byte ConsumedCharges;

	public void Write(WorldPacket data)
	{
		data.WriteUInt32(Category);
		data.WriteUInt32(NextRecoveryTime);
		data.WriteFloat(ChargeModRate);
		data.WriteUInt8(ConsumedCharges);
	}
}
