namespace HermesProxy.World.Server.Packets;

public struct AuraInfo
{
	public byte Slot;

	public AuraDataInfo AuraData;

	public void Write(WorldPacket data)
	{
		data.WriteUInt8(Slot);
		data.WriteBit(AuraData != null);
		data.FlushBits();
		if (AuraData != null)
		{
			AuraData.Write(data);
		}
	}
}
