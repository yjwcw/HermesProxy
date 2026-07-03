namespace HermesProxy.World.Server.Packets;

public class AddOnInfo
{
	public string Name;

	public string Version;

	public bool Loaded;

	public bool Disabled;

	public void Read(WorldPacket data)
	{
		data.ResetBitPos();
		uint num = data.ReadBits<uint>(10);
		uint num2 = data.ReadBits<uint>(10);
		Loaded = data.HasBit();
		Disabled = data.HasBit();
		if (num > 1)
		{
			Name = data.ReadString(num - 1);
			data.ReadUInt8();
		}
		if (num2 > 1)
		{
			Version = data.ReadString(num2 - 1);
			data.ReadUInt8();
		}
	}
}
