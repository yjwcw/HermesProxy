namespace HermesProxy.World.Server.Packets;

public class SpellHealPrediction
{
	public WowGuid128 BeaconGUID = WowGuid128.Empty;

	public uint Points;

	public byte Type;

	public void Write(WorldPacket data)
	{
		data.WriteUInt32(Points);
		data.WriteUInt8(Type);
		data.WritePackedGuid128(BeaconGUID);
	}
}
