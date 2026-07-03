namespace HermesProxy.World.Server.Packets;

public struct MissileTrajectoryRequest
{
	public float Pitch;

	public float Speed;

	public void Read(WorldPacket data)
	{
		Pitch = data.ReadFloat();
		Speed = data.ReadFloat();
	}
}
