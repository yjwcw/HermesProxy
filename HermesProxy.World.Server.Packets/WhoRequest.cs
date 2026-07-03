using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets;

public class WhoRequest
{
	public int MinLevel;

	public int MaxLevel;

	public string Name;

	public string VirtualRealmName;

	public string Guild;

	public string GuildVirtualRealmName;

	public long RaceFilter;

	public int ClassFilter = -1;

	public List<string> Words = new List<string>();

	public bool ShowEnemies;

	public bool ShowArenaPlayers;

	public bool ExactName;

	public WhoRequestServerInfo ServerInfo;

	public void Read(WorldPacket data)
	{
		MinLevel = data.ReadInt32();
		MaxLevel = data.ReadInt32();
		RaceFilter = data.ReadInt64();
		ClassFilter = data.ReadInt32();
		uint length = data.ReadBits<uint>(6);
		uint length2 = data.ReadBits<uint>(9);
		uint length3 = data.ReadBits<uint>(7);
		uint length4 = data.ReadBits<uint>(9);
		uint num = data.ReadBits<uint>(3);
		ShowEnemies = data.HasBit();
		ShowArenaPlayers = data.HasBit();
		ExactName = data.HasBit();
		if (data.HasBit())
		{
			ServerInfo = new WhoRequestServerInfo();
		}
		data.ResetBitPos();
		for (int i = 0; i < num; i++)
		{
			Words.Add(data.ReadString(data.ReadBits<uint>(7)));
			data.ResetBitPos();
		}
		Name = data.ReadString(length);
		VirtualRealmName = data.ReadString(length2);
		Guild = data.ReadString(length3);
		GuildVirtualRealmName = data.ReadString(length4);
		if (ServerInfo != null)
		{
			ServerInfo.Read(data);
		}
	}
}
