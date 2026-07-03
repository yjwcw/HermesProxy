using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets;

public class GameObjectStats
{
	public string[] Name = new string[4];

	public string IconName = "";

	public string CastBarCaption = "";

	public string UnkString = "";

	public uint Type;

	public uint DisplayID;

	public int[] Data = new int[35];

	public float Size = 1f;

	public List<uint> QuestItems = new List<uint>();

	public uint ContentTuningId;
}
