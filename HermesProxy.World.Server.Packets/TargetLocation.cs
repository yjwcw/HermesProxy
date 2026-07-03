using Framework.GameMath;

namespace HermesProxy.World.Server.Packets;

public class TargetLocation
{
	public WowGuid128 Transport = WowGuid128.Empty;

	public Vector3 Location;

	public void Read(WorldPacket data)
	{
		Transport = data.ReadPackedGuid128();
		Location = data.ReadVector3();
	}

	public void Write(WorldPacket data)
	{
		data.WritePackedGuid128(Transport);
		data.WriteVector3(Location);
	}
}
