namespace HermesProxy.World.Server.Packets;

public struct CTROptions
{
	public uint ContentTuningConditionMask;

	public int Unused901;

	public uint ExpansionLevelMask;

	public void Write(WorldPacket data)
	{
		data.WriteUInt32(ContentTuningConditionMask);
		data.WriteInt32(Unused901);
		data.WriteUInt32(ExpansionLevelMask);
	}
}
