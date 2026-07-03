using HermesProxy.World;
using HermesProxy.World.Enums;

namespace BNetServer.Networking;

public class GameAccountInfo
{
	public uint Id;

	public string Name;

	public string DisplayName;

	public uint UnbanDate;

	public bool IsBanned;

	public bool IsPermanenetlyBanned;

	public WowGuid128 WoWAccountGuid => WowGuid128.Create(HighGuidType703.WowAccount, Id);

	public GameAccountInfo(string name)
	{
		Id = 1u;
		Name = name;
		UnbanDate = 0u;
		IsPermanenetlyBanned = false;
		IsBanned = IsPermanenetlyBanned || UnbanDate > Time.UnixTime;
		int num = Name.IndexOf('#');
		if (num != -1)
		{
			string name2 = Name;
			int num2 = num + 1;
			DisplayName = "WoW" + name2.Substring(num2, name2.Length - num2);
		}
		else
		{
			DisplayName = Name;
		}
	}
}
