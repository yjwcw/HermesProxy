using System.Collections.Generic;
using HermesProxy.World;
using HermesProxy.World.Enums;

namespace BNetServer.Networking;

public class AccountInfo
{
	public uint Id;

	public string Login;

	public uint LoginTicketExpiry;

	public bool IsBanned;

	public bool IsPermanenetlyBanned;

	public Dictionary<uint, GameAccountInfo> GameAccounts;

	public WowGuid128 BnetAccountGuid => WowGuid128.Create(HighGuidType703.BNetAccount, Id);

	public AccountInfo(string name)
	{
		Id = 1u;
		Login = name;
		LoginTicketExpiry = (uint)(Time.UnixTime + 10000);
		IsBanned = false;
		IsPermanenetlyBanned = false;
		GameAccounts = new Dictionary<uint, GameAccountInfo>();
		GameAccountInfo gameAccountInfo = new GameAccountInfo(name);
		GameAccounts[1u] = gameAccountInfo;
	}
}
