namespace HermesProxy.World.Server.Packets;

public class SpellTargetedHealPrediction
{
	public WowGuid128 TargetGUID;

	public SpellHealPrediction Predict;

	public void Write(WorldPacket data)
	{
		data.WritePackedGuid128(TargetGUID);
		Predict.Write(data);
	}
}
