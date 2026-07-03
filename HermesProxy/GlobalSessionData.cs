using System.Collections.Generic;
using System.Linq;
using System.Text;
using BNetServer.Networking;
using Framework.Realm;
using HermesProxy.Auth;
using HermesProxy.World;
using HermesProxy.World.Client;
using HermesProxy.World.Enums;
using HermesProxy.World.Server;
using HermesProxy.World.Server.Packets;

namespace HermesProxy;

public class GlobalSessionData
{
	public AccountInfo AccountInfo;

	public GameAccountInfo GameAccountInfo;

	public string Username;

	public string LoginTicket;

	public byte[] SessionKey;

	public string Locale;

	public string OS;

	public uint Build;

	public GameSessionData GameState;

	public RealmId RealmId;

	public RealmManager RealmManager = new RealmManager();

	public AccountMetaDataManager AccountMetaDataMgr;

	public AccountDataManager AccountDataMgr;

	public WorldSocket RealmSocket;

	public WorldSocket InstanceSocket;

	public AuthClient AuthClient;

	public WorldClient WorldClient;

	public SniffFile ModernSniff;

	public Dictionary<string, WowGuid128> GuildsByName = new Dictionary<string, WowGuid128>();

	public Dictionary<uint, List<string>> GuildRanks = new Dictionary<uint, List<string>>();

	public Realm? Realm => RealmManager.GetRealm(RealmId);

	public GlobalSessionData()
	{
		GameState = GameSessionData.CreateNewGameSessionData(this);
	}

	public void StoreGuildRankNames(uint guildId, List<string> ranks)
	{
		if (GuildRanks.ContainsKey(guildId))
		{
			GuildRanks[guildId] = ranks;
		}
		else
		{
			GuildRanks.Add(guildId, ranks);
		}
	}

	public uint GetGuildRankIdByName(uint guildId, string name)
	{
		if (GuildRanks.ContainsKey(guildId))
		{
			for (int i = 0; i < GuildRanks[guildId].Count; i++)
			{
				if (GuildRanks[guildId][i] == name)
				{
					return (uint)i;
				}
			}
		}
		return 0u;
	}

	public string GetGuildRankNameById(uint guildId, byte rankId)
	{
		if (GuildRanks.ContainsKey(guildId))
		{
			return GuildRanks[guildId][rankId];
		}
		return $"Rank {rankId}";
	}

	public void StoreGuildGuidAndName(WowGuid128 guid, string name)
	{
		if (GuildsByName.ContainsKey(name))
		{
			GuildsByName[name] = guid;
		}
		else
		{
			GuildsByName.Add(name, guid);
		}
	}

	public WowGuid128 GetGuildGuid(string name)
	{
		if (GuildsByName.ContainsKey(name))
		{
			return GuildsByName[name];
		}
		WowGuid128 wowGuid = WowGuid128.Create(HighGuidType703.Guild, (ulong)(GuildsByName.Count + 1));
		GuildsByName.Add(name, wowGuid);
		return wowGuid;
	}

	public WowGuid128 GetGameAccountGuidForPlayer(WowGuid128 playerGuid)
	{
		if (GameState.OwnCharacters.Any((OwnCharacterInfo own) => own.CharacterGuid == playerGuid))
		{
			return WowGuid128.Create(HighGuidType703.WowAccount, GameAccountInfo.Id);
		}
		return WowGuid128.Create(HighGuidType703.WowAccount, playerGuid.GetCounter());
	}

	public WowGuid128 GetBnetAccountGuidForPlayer(WowGuid128 playerGuid)
	{
		if (GameState.OwnCharacters.Any((OwnCharacterInfo own) => own.CharacterGuid == playerGuid))
		{
			return WowGuid128.Create(HighGuidType703.BNetAccount, AccountInfo.Id);
		}
		return WowGuid128.Create(HighGuidType703.BNetAccount, playerGuid.GetCounter());
	}

	public void OnDisconnect()
	{
		if (ModernSniff != null)
		{
			ModernSniff.CloseFile();
			ModernSniff = null;
		}
		if (AuthClient != null)
		{
			AuthClient.Disconnect();
			AuthClient = null;
		}
		if (WorldClient != null)
		{
			WorldClient.Disconnect();
			WorldClient = null;
		}
		if (RealmSocket != null)
		{
			RealmSocket.CloseSocket();
			RealmSocket = null;
		}
		if (InstanceSocket != null)
		{
			InstanceSocket.CloseSocket();
			InstanceSocket = null;
		}
		GameState = GameSessionData.CreateNewGameSessionData(this);
	}

	public void SendHermesTextMessage(string message, bool isError = false)
	{
		WorldSocket instanceSocket = InstanceSocket;
		if (instanceSocket != null)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("|cFF111111[|r|cFF33DD22HermesProxy|r|cFF111111]|r ");
			if (isError)
			{
				stringBuilder.Append("|cFFFF0000");
			}
			stringBuilder.Append(message);
			ChatPkt packet = new ChatPkt(this, ChatMessageTypeModern.System, stringBuilder.ToString());
			instanceSocket.SendPacket(packet);
		}
	}
}
