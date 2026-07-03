namespace HermesProxy.World.Server.Packets;

public struct PowerUpdatePower
{
	public int Power;

	public byte PowerType;

	public PowerUpdatePower(int power, byte powerType)
	{
		Power = power;
		PowerType = powerType;
	}
}
