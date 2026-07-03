namespace HermesProxy.World.Server.Packets;

public class SetSelection : ClientPacket
{
	public WowGuid128 TargetGUID;

	public SetSelection(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		TargetGUID = _worldPacket.ReadPackedGuid128();
	}
}
