namespace HermesProxy.World.Server.Packets;

internal class SaveCUFProfiles : ClientPacket
{
	public byte[] Data;

	public SaveCUFProfiles(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		Data = _worldPacket.ReadToEnd();
	}
}
