namespace HermesProxy.World.Server.Packets;

internal class SummonResponse : ClientPacket
{
	public WowGuid128 SummonerGUID;

	public bool Accept;

	public SummonResponse(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		SummonerGUID = _worldPacket.ReadPackedGuid128();
		Accept = _worldPacket.HasBit();
	}
}
