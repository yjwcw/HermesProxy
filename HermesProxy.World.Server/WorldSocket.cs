using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using BNetServer;
using BNetServer.Services;
using Framework;
using Framework.Constants;
using Framework.Cryptography;
using Framework.IO;
using Framework.Logging;
using Framework.Networking;
using Framework.Realm;
using Google.Protobuf;
using HermesProxy.Enums;
using HermesProxy.World.Client;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server;

public class WorldSocket : SocketBase, BnetServices.INetwork
{
	public struct ConnectToKey
	{
		public uint AccountId;

		public ConnectionType connectionType;

		public ulong Key;

		public ulong Raw
		{
			get
			{
				return (ulong)(AccountId | ((long)connectionType << 32)) | (Key << 33);
			}
			set
			{
				AccountId = (uint)(value & 0xFFFFFFFFu);
				connectionType = (ConnectionType)((value >> 32) & 1);
				Key = value >> 33;
			}
		}
	}

	public class CharacterLoginFailed : ServerPacket
	{
		private LoginFailureReason Code;

		public CharacterLoginFailed(LoginFailureReason code)
			: base(Opcode.SMSG_CHARACTER_LOGIN_FAILED)
		{
			Code = code;
		}

		public override void Write()
		{
			_worldPacket.WriteUInt8((byte)Code);
		}
	}

	public class PacketHandler
	{
		private Action<WorldSocket, ClientPacket> methodCaller;

		private Type packetType;

		public PacketHandler(MethodInfo info, Type type)
		{
			methodCaller = (Action<WorldSocket, ClientPacket>)GetType().GetMethod("CreateDelegate", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(type).Invoke(null, new object[1] { info });
			packetType = type;
		}

		public void Invoke(WorldSocket session, WorldPacket packet)
		{
			if (packetType == null)
			{
				return;
			}
			using ClientPacket clientPacket = (ClientPacket)Activator.CreateInstance(packetType, packet);
			clientPacket.LogPacket(ref session.GetSession().ModernSniff);
			clientPacket.Read();
			methodCaller(session, clientPacket);
		}

		private static Action<WorldSocket, ClientPacket> CreateDelegate<P1>(MethodInfo method) where P1 : ClientPacket
		{
			Action<WorldSocket, P1> d = (Action<WorldSocket, P1>)method.CreateDelegate(typeof(Action<WorldSocket, P1>));
			return delegate(WorldSocket target, ClientPacket p)
			{
				d(target, (P1)p);
			};
		}
	}

	private static readonly string ClientConnectionInitialize = "WORLD OF WARCRAFT CONNECTION - CLIENT TO SERVER - V2";

	private static readonly string ServerConnectionInitialize = "WORLD OF WARCRAFT CONNECTION - SERVER TO CLIENT - V2";

	private static readonly byte[] AuthCheckSeed = new byte[16]
	{
		197, 198, 152, 149, 118, 63, 29, 205, 182, 161,
		55, 40, 179, 18, 255, 138
	};

	private static readonly byte[] SessionKeySeed = new byte[16]
	{
		88, 203, 207, 64, 254, 46, 206, 166, 90, 144,
		184, 1, 104, 108, 40, 11
	};

	private static readonly byte[] ContinuedSessionSeed = new byte[16]
	{
		22, 173, 12, 212, 70, 249, 79, 178, 239, 125,
		234, 42, 23, 102, 77, 47
	};

	private static readonly byte[] EncryptionKeySeed = new byte[16]
	{
		233, 117, 60, 80, 144, 147, 97, 218, 59, 7,
		238, 250, 255, 157, 65, 184
	};

	private static readonly int HeaderSize = 16;

	private SocketBuffer _headerBuffer;

	private SocketBuffer _packetBuffer;

	private ConnectionType _connectType;

	private ulong _key;

	private byte[] _serverChallenge;

	private WorldCrypt _worldCrypt;

	private byte[] _sessionKey;

	private byte[] _encryptKey;

	private ConnectToKey _instanceConnectKey;

	private RealmId _realmId;

	private ZLib.z_stream _compressionStream;

	private ConcurrentDictionary<Opcode, PacketHandler> _clientPacketTable = new ConcurrentDictionary<Opcode, PacketHandler>();

	private GlobalSessionData _globalSession;

	private Mutex _sendMutex = new Mutex();

	private BnetServices.ServiceManager _bnetRpc;

	public GlobalSessionData Session => _globalSession;

	[PacketHandler(Opcode.CMSG_ARENA_TEAM_ROSTER)]
	private void HandleArenaTeamRoster(ArenaTeamRosterRequest arena)
	{
		if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180) || GetSession().GameState.CurrentArenaTeamIds[arena.TeamIndex] == 0)
		{
			ArenaTeamRosterResponse arenaTeamRosterResponse = new ArenaTeamRosterResponse();
			arenaTeamRosterResponse.TeamSize = ModernVersion.GetArenaTeamSizeFromIndex(arena.TeamIndex);
			SendPacket(arenaTeamRosterResponse);
			return;
		}
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_ARENA_TEAM_QUERY);
		worldPacket.WriteUInt32(GetSession().GameState.CurrentArenaTeamIds[arena.TeamIndex]);
		SendPacketToServer(worldPacket);
		WorldPacket worldPacket2 = new WorldPacket(Opcode.CMSG_ARENA_TEAM_ROSTER);
		worldPacket2.WriteUInt32(GetSession().GameState.CurrentArenaTeamIds[arena.TeamIndex]);
		SendPacketToServer(worldPacket2);
	}

	[PacketHandler(Opcode.CMSG_ARENA_TEAM_QUERY)]
	private void HandleArenaTeamQuery(ArenaTeamQuery arena)
	{
		if (GetSession().GameState.ArenaTeams.TryGetValue(arena.TeamId, out var value))
		{
			ArenaTeamQueryResponse arenaTeamQueryResponse = new ArenaTeamQueryResponse();
			arenaTeamQueryResponse.TeamId = arena.TeamId;
			arenaTeamQueryResponse.Emblem = new ArenaTeamEmblem();
			arenaTeamQueryResponse.Emblem.TeamId = arena.TeamId;
			arenaTeamQueryResponse.Emblem.TeamSize = value.TeamSize;
			arenaTeamQueryResponse.Emblem.BackgroundColor = value.BackgroundColor;
			arenaTeamQueryResponse.Emblem.EmblemStyle = value.EmblemStyle;
			arenaTeamQueryResponse.Emblem.EmblemColor = value.EmblemColor;
			arenaTeamQueryResponse.Emblem.BorderStyle = value.BorderStyle;
			arenaTeamQueryResponse.Emblem.BorderColor = value.BorderColor;
			arenaTeamQueryResponse.Emblem.TeamName = value.Name;
			SendPacket(arenaTeamQueryResponse);
		}
	}

	[PacketHandler(Opcode.CMSG_BATTLEMASTER_JOIN_ARENA)]
	private void HandleBattlematerJoinArena(BattlemasterJoinArena join)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_BATTLEMASTER_JOIN_ARENA);
		worldPacket.WriteGuid(join.Guid.To64());
		worldPacket.WriteUInt8(join.TeamIndex);
		worldPacket.WriteBool(data: true);
		worldPacket.WriteBool(data: true);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_BATTLEMASTER_JOIN_SKIRMISH)]
	private void HandleBattlematerJoinSkirmish(BattlemasterJoinSkirmish join)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_BATTLEMASTER_JOIN_ARENA);
		worldPacket.WriteGuid(join.Guid.To64());
		worldPacket.WriteUInt8(join.TeamSize);
		worldPacket.WriteBool(join.AsGroup);
		worldPacket.WriteBool(data: false);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_ARENA_TEAM_REMOVE)]
	[PacketHandler(Opcode.CMSG_ARENA_TEAM_LEADER)]
	private void HandleArenaUnimplemented(ArenaTeamRemove arena)
	{
		WorldPacket worldPacket = new WorldPacket(arena.GetUniversalOpcode());
		worldPacket.WriteUInt32(arena.TeamId);
		worldPacket.WriteCString(GetSession().GameState.GetPlayerName(arena.PlayerGuid));
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_ARENA_TEAM_DISBAND)]
	[PacketHandler(Opcode.CMSG_ARENA_TEAM_LEAVE)]
	private void HandleArenaTeamLeave(ArenaTeamLeave arena)
	{
		WorldPacket worldPacket = new WorldPacket(arena.GetUniversalOpcode());
		worldPacket.WriteUInt32(arena.TeamId);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_ARENA_TEAM_ACCEPT)]
	[PacketHandler(Opcode.CMSG_ARENA_TEAM_DECLINE)]
	private void HandleArenaTeamInviteResponse(ArenaTeamAccept arena)
	{
		WorldPacket packet = new WorldPacket(arena.GetUniversalOpcode());
		SendPacketToServer(packet);
	}

	[PacketHandler(Opcode.CMSG_AUCTION_HELLO_REQUEST)]
	private void HandleAuctionHelloRequest(InteractWithNPC interact)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.MSG_AUCTION_HELLO);
		worldPacket.WriteGuid(interact.CreatureGUID.To64());
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_AUCTION_LIST_BIDDED_ITEMS)]
	private void HandleAuctionListBidderItems(AuctionListBidderItems auction)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_AUCTION_LIST_BIDDED_ITEMS);
		worldPacket.WriteGuid(auction.Auctioneer.To64());
		worldPacket.WriteUInt32(auction.Offset);
		worldPacket.WriteInt32(auction.AuctionItemIDs.Count);
		foreach (uint auctionItemID in auction.AuctionItemIDs)
		{
			worldPacket.WriteUInt32(auctionItemID);
		}
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_AUCTION_LIST_OWNED_ITEMS)]
	private void HandleAuctionListOwnerItems(AuctionListOwnerItems auction)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_AUCTION_LIST_OWNED_ITEMS);
		worldPacket.WriteGuid(auction.Auctioneer.To64());
		worldPacket.WriteUInt32(auction.Offset);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_AUCTION_LIST_ITEMS)]
	private void HandleAuctionListItems(AuctionListItems auction)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_AUCTION_LIST_ITEMS);
		worldPacket.WriteGuid(auction.Auctioneer.To64());
		worldPacket.WriteUInt32(auction.Offset);
		worldPacket.WriteCString(auction.Name);
		worldPacket.WriteUInt8(auction.MinLevel);
		worldPacket.WriteUInt8(auction.MaxLevel);
		if (auction.ClassFilters.Count > 0)
		{
			if (auction.ClassFilters[0].SubClassFilters.Count == 1)
			{
				worldPacket.WriteInt32(ModernToLegacyInventorySlotType(auction.ClassFilters[0].SubClassFilters[0].InvTypeMask));
				worldPacket.WriteInt32(auction.ClassFilters[0].ItemClass);
				worldPacket.WriteInt32(auction.ClassFilters[0].SubClassFilters[0].ItemSubclass);
			}
			else
			{
				worldPacket.WriteInt32(-1);
				worldPacket.WriteInt32(auction.ClassFilters[0].ItemClass);
				worldPacket.WriteInt32(-1);
			}
		}
		else
		{
			worldPacket.WriteInt32(-1);
			worldPacket.WriteInt32(-1);
			worldPacket.WriteInt32(-1);
		}
		worldPacket.WriteInt32(auction.Quality);
		worldPacket.WriteBool(auction.OnlyUsable);
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			worldPacket.WriteBool(auction.ExactMatch);
			worldPacket.WriteUInt8((byte)auction.Sorts.Count);
			foreach (AuctionSort sort in auction.Sorts)
			{
				worldPacket.WriteUInt8(sort.Type);
				worldPacket.WriteUInt8(sort.Direction);
			}
		}
		SendPacketToServer(worldPacket);
		static int ModernToLegacyInventorySlotType(uint modernInventoryFlag)
		{
			if (modernInventoryFlag == uint.MaxValue)
			{
				return -1;
			}
			for (int i = 0; i < 32; i++)
			{
				if ((modernInventoryFlag & (1 << i)) > 0)
				{
					return i;
				}
			}
			return -1;
		}
	}

	private int ModernToLegacyInventorySlotType(uint modernInventoryFlag)
	{
		if (modernInventoryFlag == uint.MaxValue)
		{
			return -1;
		}
		for (byte b = 0; b < 32; b++)
		{
			if ((modernInventoryFlag & (uint)(1 << (int)b)) != 0)
			{
				return b;
			}
		}
		return -1;
	}

	[PacketHandler(Opcode.CMSG_AUCTION_SELL_ITEM)]
	private void HandleAuctionSellItem(AuctionSellItem auction)
	{
		uint num = auction.ExpireTime;
		if (LegacyVersion.ExpansionVersion <= 1 && ModernVersion.ExpansionVersion > 1)
		{
			switch (num)
			{
			case 720u:
				num = 120u;
				break;
			case 1440u:
				num = 480u;
				break;
			case 2880u:
				num = 1440u;
				break;
			}
		}
		else if (LegacyVersion.ExpansionVersion > 1 && ModernVersion.ExpansionVersion <= 1)
		{
			switch (num)
			{
			case 120u:
				num = 720u;
				break;
			case 480u:
				num = 1440u;
				break;
			case 1440u:
				num = 2880u;
				break;
			}
		}
		if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_2_2a_10505))
		{
			foreach (AuctionItemForSale item in auction.Items)
			{
				WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_AUCTION_SELL_ITEM);
				worldPacket.WriteGuid(auction.Auctioneer.To64());
				worldPacket.WriteGuid(item.Guid.To64());
				worldPacket.WriteUInt32((uint)auction.MinBid);
				worldPacket.WriteUInt32((uint)auction.BuyoutPrice);
				worldPacket.WriteUInt32(num);
				SendPacketToServer(worldPacket);
			}
			return;
		}
		WorldPacket worldPacket2 = new WorldPacket(Opcode.CMSG_AUCTION_SELL_ITEM);
		worldPacket2.WriteGuid(auction.Auctioneer.To64());
		worldPacket2.WriteInt32(auction.Items.Count);
		foreach (AuctionItemForSale item2 in auction.Items)
		{
			worldPacket2.WriteGuid(item2.Guid.To64());
			worldPacket2.WriteUInt32(item2.UseCount);
		}
		worldPacket2.WriteUInt32((uint)auction.MinBid);
		worldPacket2.WriteUInt32((uint)auction.BuyoutPrice);
		worldPacket2.WriteUInt32(num);
		SendPacketToServer(worldPacket2);
	}

	[PacketHandler(Opcode.CMSG_AUCTION_REMOVE_ITEM)]
	private void HandleAuctionRemoveItem(AuctionRemoveItem auction)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_AUCTION_REMOVE_ITEM);
		worldPacket.WriteGuid(auction.Auctioneer.To64());
		worldPacket.WriteUInt32(auction.AuctionID);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_AUCTION_PLACE_BID)]
	private void HandleAuctionPlaceBId(AuctionPlaceBid auction)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_AUCTION_PLACE_BID);
		worldPacket.WriteGuid(auction.Auctioneer.To64());
		worldPacket.WriteUInt32(auction.AuctionID);
		worldPacket.WriteUInt32((uint)auction.BidAmount);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_BATTLEMASTER_JOIN)]
	private void HandleBattlefieldJoin(BattlemasterJoin join)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_BATTLEMASTER_JOIN);
		worldPacket.WriteGuid(join.BattlemasterGuid.To64());
		if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			worldPacket.WriteUInt32(GameData.GetMapIdFromBattlegroundId(join.BattlefieldListId));
		}
		else
		{
			worldPacket.WriteUInt32(join.BattlefieldListId);
		}
		worldPacket.WriteInt32(join.BattlefieldInstanceID);
		worldPacket.WriteBool(join.JoinAsGroup);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_BATTLEFIELD_PORT)]
	private void HandleBattlefieldPort(BattlefieldPort port)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_BATTLEFIELD_PORT);
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			worldPacket.WriteUInt8(2);
			worldPacket.WriteUInt8(0);
			worldPacket.WriteUInt32(GetSession().GameState.GetBattleFieldQueueType(port.Ticket.Id));
			worldPacket.WriteUInt16(8080);
			worldPacket.WriteBool(port.AcceptedInvite);
		}
		else
		{
			worldPacket.WriteUInt32(GetSession().GameState.GetBattleFieldQueueType(port.Ticket.Id));
			worldPacket.WriteBool(port.AcceptedInvite);
		}
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_REQUEST_BATTLEFIELD_STATUS)]
	private void HandleRequestBattlefieldStatus(RequestBattlefieldStatus log)
	{
		WorldPacket packet = new WorldPacket(Opcode.CMSG_BATTLEFIELD_STATUS);
		SendPacketToServer(packet);
	}

	[PacketHandler(Opcode.CMSG_PVP_LOG_DATA)]
	private void HandlePvPLogData(PVPLogDataRequest log)
	{
		WorldPacket packet = new WorldPacket(Opcode.MSG_PVP_LOG_DATA);
		SendPacketToServer(packet);
	}

	[PacketHandler(Opcode.CMSG_BATTLEFIELD_LEAVE)]
	private void HandleBattlefieldLeave(BattlefieldLeave leave)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_BATTLEFIELD_LEAVE);
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			worldPacket.WriteUInt8(2);
			worldPacket.WriteUInt8(0);
			worldPacket.WriteUInt32(GetSession().GameState.GetBattleFieldQueueType(1u));
			worldPacket.WriteUInt16(8080);
		}
		else
		{
			worldPacket.WriteUInt32(GetSession().GameState.CurrentMapId.Value);
		}
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_ENUM_CHARACTERS)]
	private void HandleEnumCharacters(EnumCharacters charEnum)
	{
		WorldPacket packet = new WorldPacket(Opcode.CMSG_ENUM_CHARACTERS);
		SendPacketToServer(packet);
	}

	[PacketHandler(Opcode.CMSG_GET_ACCOUNT_CHARACTER_LIST)]
	private void HandleGetAccountCharacterList(GetAccountCharacterListRequest request)
	{
		GetAccountCharacterListResult getAccountCharacterListResult = new GetAccountCharacterListResult();
		getAccountCharacterListResult.Token = request.Token;
		foreach (OwnCharacterInfo ownCharacter in GetSession().GameState.OwnCharacters)
		{
			getAccountCharacterListResult.CharacterList.Add(new AccountCharacterListEntry
			{
				// 账户id
				AccountId = WowGuid128.Create(HighGuidType703.WowAccount, GetSession().GameAccountInfo.Id),
                // 角色指南
                CharacterGuid = ownCharacter.CharacterGuid,
                // 服务器虚拟地址
                RealmVirtualAddress = GetSession().RealmId.GetAddress(),
				// 服务器名
				RealmName = "",
                // 上次登录 Unix 秒
                LastLoginUnixSec = ownCharacter.LastLoginUnixSec,
				// 名字
				Name = ownCharacter.Name,
				// 种族
				Race = ownCharacter.RaceId,
				// 职业
				Class = ownCharacter.ClassId,
				// 性别
				Sex = ownCharacter.SexId,
				// 等级
				Level = ownCharacter.Level

			});
		}
        //Console.WriteLine("姓名: " + getAccountCharacterListResult.CharacterList[2].Name);
        //Console.WriteLine("姓名: " + getAccountCharacterListResult.CharacterList[2].Race);
		//getAccountCharacterListResult.CharacterList[2].Race = Race.Human;
        SendPacket(getAccountCharacterListResult);
	}

	[PacketHandler(Opcode.CMSG_GENERATE_RANDOM_CHARACTER_NAME)]
	private void HandleGenerateRandomCharacterNameRequest(GenerateRandomCharacterNameRequest randomCharacterName)
	{
		GenerateRandomCharacterNameResult generateRandomCharacterNameResult = new GenerateRandomCharacterNameResult();
		generateRandomCharacterNameResult.Success = false;
		SendPacket(generateRandomCharacterNameResult);
	}

	[PacketHandler(Opcode.CMSG_CREATE_CHARACTER)]
	private void HandleCreateCharacter(CreateCharacter charCreate)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_CREATE_CHARACTER);
		worldPacket.WriteCString(charCreate.CreateInfo.Name);
		worldPacket.WriteUInt8((byte)charCreate.CreateInfo.RaceId);
		worldPacket.WriteUInt8((byte)charCreate.CreateInfo.ClassId);
		worldPacket.WriteUInt8((byte)charCreate.CreateInfo.Sex);
		CharacterCustomizations.ConvertModernCustomizationsToLegacy(charCreate.CreateInfo.Customizations, out var skin, out var face, out var hairStyle, out var hairColor, out var facialHair);
		worldPacket.WriteUInt8(skin);
		worldPacket.WriteUInt8(face);
		worldPacket.WriteUInt8(hairStyle);
		worldPacket.WriteUInt8(hairColor);
		worldPacket.WriteUInt8(facialHair);
		worldPacket.WriteUInt8(0);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_CHAR_DELETE)]
	private void HandleCharDelete(CharDelete charDelete)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_CHAR_DELETE);
		worldPacket.WriteGuid(charDelete.Guid.To64());
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_LOADING_SCREEN_NOTIFY)]
	private void HandleLoadScreen(LoadingScreenNotify loadingScreenNotify)
	{
		if (loadingScreenNotify.MapID >= 0)
		{
			GetSession().GameState.CurrentMapId = loadingScreenNotify.MapID;
		}
	}

	[PacketHandler(Opcode.CMSG_QUERY_PLAYER_NAME)]
	private void HandleNameQueryRequest(QueryPlayerName queryPlayerName)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_NAME_QUERY);
		worldPacket.WriteGuid(queryPlayerName.Player.To64());
		SendPacketToServer(worldPacket, (!GetSession().GameState.IsInWorld) ? Opcode.SMSG_LOGIN_VERIFY_WORLD : Opcode.MSG_NULL_ACTION);
	}

	[PacketHandler(Opcode.CMSG_QUERY_PLAYER_NAMES)]
	private void HandleNamesQueryRequest(QueryPlayerNames queryPlayerNames)
	{
		foreach (WowGuid128 player in queryPlayerNames.Players)
		{
			WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_NAME_QUERY);
			worldPacket.WriteGuid(player.To64());
			SendPacketToServer(worldPacket, (!GetSession().GameState.IsInWorld) ? Opcode.SMSG_LOGIN_VERIFY_WORLD : Opcode.MSG_NULL_ACTION);
		}
	}

	[PacketHandler(Opcode.CMSG_PLAYER_LOGIN)]
	private void HandlePlayerLogin(PlayerLogin playerLogin)
	{
		if (!GetSession().GameState.CachedPlayers.TryGetValue(playerLogin.Guid, out var value))
		{
			Log.Print(LogType.Error, $"Player tried to log in with unknown char id: {playerLogin.Guid}", "HandlePlayerLogin", "D:\\a\\HermesProxy\\HermesProxy\\World\\Server\\PacketHandlers\\CharacterHandler.cs");
			return;
		}
		Realm realm = GetSession().RealmManager.GetRealm(GetSession().RealmId);
		if (realm == null)
		{
			Log.Print(LogType.Error, $"Player tried to log in to unknown realm id: {GetSession().RealmId}", "HandlePlayerLogin", "D:\\a\\HermesProxy\\HermesProxy\\World\\Server\\PacketHandlers\\CharacterHandler.cs");
			return;
		}
		GetSession().AccountMetaDataMgr.SaveLastSelectedCharacter(realm.Name, value.Name, playerLogin.Guid.Low, Time.UnixTime);
		if (GetSession().AuthClient != null)
		{
			GetSession().AuthClient.Disconnect();
		}
		SendConnectToInstance(ConnectToSerial.WorldAttempt1);
		GetSession().GameState.IsConnectedToInstance = true;
		GetSession().GameState.IsFirstEnterWorld = true;
		GetSession().GameState.CurrentPlayerGuid = playerLogin.Guid;
		GetSession().GameState.CurrentPlayerInfo = GetSession().GameState.OwnCharacters.Single((OwnCharacterInfo x) => x.CharacterGuid == playerLogin.Guid);
		GetSession().GameState.CurrentPlayerStorage.LoadCurrentPlayer();
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_PLAYER_LOGIN);
		worldPacket.WriteGuid(playerLogin.Guid.To64());
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_LOGOUT_REQUEST)]
	private void HandleLogoutRequest(LogoutRequest logoutRequest)
	{
		WorldPacket packet = new WorldPacket(Opcode.CMSG_LOGOUT_REQUEST);
		SendPacketToServer(packet);
	}

	[PacketHandler(Opcode.CMSG_LOGOUT_CANCEL)]
	private void HandleLogoutCancel(LogoutCancel logoutCancel)
	{
		WorldPacket packet = new WorldPacket(Opcode.CMSG_LOGOUT_CANCEL);
		SendPacketToServer(packet);
	}

	[PacketHandler(Opcode.CMSG_REQUEST_PLAYED_TIME)]
	private void HandleRequestPlayedTime(RequestPlayedTime played)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_REQUEST_PLAYED_TIME);
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			worldPacket.WriteBool(played.TriggerScriptEvent);
		}
		SendPacketToServer(worldPacket);
		GetSession().GameState.ShowPlayedTime = played.TriggerScriptEvent;
	}

	[PacketHandler(Opcode.CMSG_SET_TITLE)]
	private void HandleTogglePvP(SetTitle title)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_SET_TITLE);
		worldPacket.WriteInt32(title.TitleID);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_TOGGLE_PVP)]
	private void HandleTogglePvP(TogglePvP pvp)
	{
		WorldPacket packet = new WorldPacket(Opcode.CMSG_TOGGLE_PVP);
		SendPacketToServer(packet);
	}

	[PacketHandler(Opcode.CMSG_SET_PVP)]
	private void HandleTogglePvP(SetPvP pvp)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_TOGGLE_PVP);
		worldPacket.WriteBool(pvp.Enable);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_SET_ACTION_BUTTON)]
	private void HandleSetActionButton(SetActionButton button)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_SET_ACTION_BUTTON);
		worldPacket.WriteUInt8(button.Index);
		worldPacket.WriteUInt16(button.Action);
		worldPacket.WriteUInt16(button.Type);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_SET_ACTION_BAR_TOGGLES)]
	private void HandleSetActionBarToggles(SetActionBarToggles bars)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_SET_ACTION_BAR_TOGGLES);
		worldPacket.WriteUInt8(bars.Mask);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_UNLEARN_SKILL)]
	private void HandleUnlearnSkill(UnlearnSkill skill)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_UNLEARN_SKILL);
		worldPacket.WriteUInt32(skill.SkillLine);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_PLAYER_SHOWING_CLOAK)]
	[PacketHandler(Opcode.CMSG_PLAYER_SHOWING_HELM)]
	private void HandleShowHelmOrCloak(PlayerShowingHelmOrCloak show)
	{
		WorldPacket worldPacket = new WorldPacket(show.GetUniversalOpcode());
		worldPacket.WriteBool(show.Showing);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_INSPECT)]
	private void HandleInspect(Inspect inspect)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_INSPECT);
		worldPacket.WriteGuid(inspect.Target.To64());
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_INSPECT_HONOR_STATS)]
	private void HandleInspectHonorStats(Inspect inspect)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.MSG_INSPECT_HONOR_STATS);
		worldPacket.WriteGuid(inspect.Target.To64());
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_INSPECT_PVP)]
	private void HandleInspectArenaTeams(Inspect inspect)
	{
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			WorldPacket worldPacket = new WorldPacket(Opcode.MSG_INSPECT_ARENA_TEAMS);
			worldPacket.WriteGuid(inspect.Target.To64());
			SendPacketToServer(worldPacket);
			return;
		}
		InspectPvP inspectPvP = new InspectPvP();
		inspectPvP.PlayerGUID = inspect.Target;
		inspectPvP.ArenaTeams.Add(new ArenaTeamInspectData());
		inspectPvP.ArenaTeams.Add(new ArenaTeamInspectData());
		inspectPvP.ArenaTeams.Add(new ArenaTeamInspectData());
		SendPacket(inspectPvP);
	}

	[PacketHandler(Opcode.CMSG_CHARACTER_RENAME_REQUEST)]
	private void HandleCharacterRenameRequest(CharacterRenameRequest rename)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_CHARACTER_RENAME_REQUEST);
		worldPacket.WriteGuid(rename.Guid.To64());
		worldPacket.WriteCString(rename.NewName);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_CHAT_JOIN_CHANNEL)]
	private void HandleChatJoinChannel(JoinChannel join)
	{
		if (GetSession().WorldClient != null)
		{
			GetSession().WorldClient.SendChatJoinChannel(join.ChatChannelId, join.ChannelName, join.Password);
		}
	}

	[PacketHandler(Opcode.CMSG_CHAT_LEAVE_CHANNEL)]
	private void HandleChatLeaveChannel(LeaveChannel leave)
	{
		if (GetSession().WorldClient != null)
		{
			GetSession().GameState.LeftChannelName = leave.ChannelName;
			GetSession().WorldClient.SendChatLeaveChannel(leave.ZoneChannelID, leave.ChannelName);
		}
	}

	[PacketHandler(Opcode.CMSG_CHAT_CHANNEL_OWNER)]
	[PacketHandler(Opcode.CMSG_CHAT_CHANNEL_ANNOUNCEMENTS)]
	private void HandleChatChannelCommand(ChannelCommand command)
	{
		WorldPacket worldPacket = new WorldPacket(command.GetUniversalOpcode());
		worldPacket.WriteCString(command.ChannelName);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_CHAT_CHANNEL_LIST)]
	private void HandleChatChannelList(ChannelCommand command)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_CHAT_CHANNEL_LIST);
		worldPacket.WriteCString(command.ChannelName);
		SendPacketToServer(worldPacket);
		GetSession().GameState.ChannelDisplayList = false;
	}

	[PacketHandler(Opcode.CMSG_CHAT_CHANNEL_DISPLAY_LIST)]
	private void HandleChatChannelDisplayList(ChannelCommand command)
	{
		if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_CHAT_CHANNEL_LIST);
			worldPacket.WriteCString(command.ChannelName);
			SendPacketToServer(worldPacket);
		}
		else
		{
			WorldPacket worldPacket2 = new WorldPacket(Opcode.CMSG_CHAT_CHANNEL_DISPLAY_LIST);
			worldPacket2.WriteCString(command.ChannelName);
			SendPacketToServer(worldPacket2);
		}
		GetSession().GameState.ChannelDisplayList = true;
	}

	[PacketHandler(Opcode.CMSG_CHAT_CHANNEL_DECLINE_INVITE)]
	private void HandleChatChannelDeclineInvite(ChannelCommand command)
	{
		if (!LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_CHAT_CHANNEL_DECLINE_INVITE);
			worldPacket.WriteCString(command.ChannelName);
			SendPacketToServer(worldPacket);
		}
	}

	[PacketHandler(Opcode.CMSG_CHAT_MESSAGE_AFK)]
	private void HandleChatMessageAFK(ChatMessageAFK afk)
	{
		List<string> list = ConvertTextMessageIntoMaxLengthParts(afk.Text);
		if (list.Count >= 1)
		{
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
			{
				GetSession().WorldClient.SendMessageChatWotLK(ChatMessageTypeWotLK.Afk, 0u, list[0], "", "");
			}
			else
			{
				GetSession().WorldClient.SendMessageChatVanilla(ChatMessageTypeVanilla.Afk, 0u, list[0], "", "");
			}
		}
	}

	[PacketHandler(Opcode.CMSG_CHAT_MESSAGE_DND)]
	private void HandleChatMessageDND(ChatMessageDND dnd)
	{
		List<string> list = ConvertTextMessageIntoMaxLengthParts(dnd.Text);
		if (list.Count >= 1)
		{
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
			{
				GetSession().WorldClient.SendMessageChatWotLK(ChatMessageTypeWotLK.Dnd, 0u, list[0], "", "");
			}
			else
			{
				GetSession().WorldClient.SendMessageChatVanilla(ChatMessageTypeVanilla.Dnd, 0u, list[0], "", "");
			}
		}
	}

	[PacketHandler(Opcode.CMSG_CHAT_MESSAGE_CHANNEL)]
	private void HandleChatMessageChannel(ChatMessageChannel channel)
	{
		foreach (string item in ConvertTextMessageIntoMaxLengthParts(channel.Text))
		{
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
			{
				GetSession().WorldClient.SendMessageChatWotLK(ChatMessageTypeWotLK.Channel, channel.Language, item, channel.Target, "");
			}
			else
			{
				GetSession().WorldClient.SendMessageChatVanilla(ChatMessageTypeVanilla.Channel, channel.Language, item, channel.Target, "");
			}
		}
	}

	[PacketHandler(Opcode.CMSG_CHAT_MESSAGE_WHISPER)]
	private void HandleChatMessageWhisper(ChatMessageWhisper whisper)
	{
		foreach (string item in ConvertTextMessageIntoMaxLengthParts(whisper.Text))
		{
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
			{
				GetSession().WorldClient.SendMessageChatWotLK(ChatMessageTypeWotLK.Whisper, whisper.Language, item, "", whisper.Target);
			}
			else
			{
				GetSession().WorldClient.SendMessageChatVanilla(ChatMessageTypeVanilla.Whisper, whisper.Language, item, "", whisper.Target);
			}
		}
	}

	[PacketHandler(Opcode.CMSG_CHAT_MESSAGE_EMOTE)]
	private void HandleChatMessageEmote(ChatMessageEmote emote)
	{
		List<string> list = ConvertTextMessageIntoMaxLengthParts(emote.Text);
		if (list.Count >= 1)
		{
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
			{
				GetSession().WorldClient.SendMessageChatWotLK(ChatMessageTypeWotLK.Emote, 0u, list[0], "", "");
			}
			else
			{
				GetSession().WorldClient.SendMessageChatVanilla(ChatMessageTypeVanilla.Emote, 0u, list[0], "", "");
			}
		}
	}

	[PacketHandler(Opcode.CMSG_CHAT_MESSAGE_GUILD)]
	[PacketHandler(Opcode.CMSG_CHAT_MESSAGE_OFFICER)]
	[PacketHandler(Opcode.CMSG_CHAT_MESSAGE_PARTY)]
	[PacketHandler(Opcode.CMSG_CHAT_MESSAGE_RAID)]
	[PacketHandler(Opcode.CMSG_CHAT_MESSAGE_RAID_WARNING)]
	[PacketHandler(Opcode.CMSG_CHAT_MESSAGE_SAY)]
	[PacketHandler(Opcode.CMSG_CHAT_MESSAGE_YELL)]
	[PacketHandler(Opcode.CMSG_CHAT_MESSAGE_INSTANCE_CHAT)]
	private void HandleChatMessage(ChatMessage packet)
	{
		ChatMessageTypeModern chatMessageTypeModern;
		switch (packet.GetUniversalOpcode())
		{
		case Opcode.CMSG_CHAT_MESSAGE_SAY:
			chatMessageTypeModern = ChatMessageTypeModern.Say;
			break;
		case Opcode.CMSG_CHAT_MESSAGE_YELL:
			chatMessageTypeModern = ChatMessageTypeModern.Yell;
			break;
		case Opcode.CMSG_CHAT_MESSAGE_GUILD:
			chatMessageTypeModern = ChatMessageTypeModern.Guild;
			break;
		case Opcode.CMSG_CHAT_MESSAGE_OFFICER:
			chatMessageTypeModern = ChatMessageTypeModern.Officer;
			break;
		case Opcode.CMSG_CHAT_MESSAGE_PARTY:
			chatMessageTypeModern = ChatMessageTypeModern.Party;
			break;
		case Opcode.CMSG_CHAT_MESSAGE_RAID:
			chatMessageTypeModern = ChatMessageTypeModern.Raid;
			break;
		case Opcode.CMSG_CHAT_MESSAGE_RAID_WARNING:
			chatMessageTypeModern = ChatMessageTypeModern.RaidWarning;
			break;
		case Opcode.CMSG_CHAT_MESSAGE_INSTANCE_CHAT:
			chatMessageTypeModern = ((!GetSession().GameState.IsInBattleground()) ? ChatMessageTypeModern.Party : ChatMessageTypeModern.Battleground);
			break;
		default:
			Log.Print(LogType.Error, $"HandleMessagechatOpcode : Unknown chat opcode ({packet.GetOpcode()})", "HandleChatMessage", "D:\\a\\HermesProxy\\HermesProxy\\World\\Server\\PacketHandlers\\ChatHandler.cs");
			return;
		}
		foreach (string item in ConvertTextMessageIntoMaxLengthParts(packet.Text))
		{
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
			{
				ChatMessageTypeWotLK type = (ChatMessageTypeWotLK)Enum.Parse(typeof(ChatMessageTypeWotLK), chatMessageTypeModern.ToString());
				GetSession().WorldClient.SendMessageChatWotLK(type, packet.Language, item, "", "");
			}
			else
			{
				ChatMessageTypeVanilla type2 = (ChatMessageTypeVanilla)Enum.Parse(typeof(ChatMessageTypeVanilla), chatMessageTypeModern.ToString());
				GetSession().WorldClient.SendMessageChatVanilla(type2, packet.Language, item, "", "");
			}
		}
	}

	[PacketHandler(Opcode.CMSG_CHAT_ADDON_MESSAGE)]
	private void HandleAddonMessage(ChatAddonMessage packet)
	{
		uint lang = uint.MaxValue;
		string msg = packet.Params.Prefix + "\t" + packet.Params.Text;
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			ChatMessageTypeWotLK type = (ChatMessageTypeWotLK)Enum.Parse(typeof(ChatMessageTypeWotLK), packet.Params.Type.ToString());
			GetSession().WorldClient.SendMessageChatWotLK(type, lang, msg, "", "");
		}
		else
		{
			ChatMessageTypeVanilla type2 = (ChatMessageTypeVanilla)Enum.Parse(typeof(ChatMessageTypeVanilla), packet.Params.Type.ToString());
			GetSession().WorldClient.SendMessageChatVanilla(type2, lang, msg, "", "");
		}
	}

	[PacketHandler(Opcode.CMSG_CHAT_ADDON_MESSAGE_TARGETED)]
	private void HandleAddonMessageTargeted(ChatAddonMessageTargeted packet)
	{
		uint lang = uint.MaxValue;
		string msg = packet.Params.Prefix + "\t" + packet.Params.Text;
		string channel = (packet.ChannelGuid.IsEmpty() ? "" : GetSession().GameState.GetChannelName((int)packet.ChannelGuid.GetCounter()));
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			ChatMessageTypeWotLK type = (ChatMessageTypeWotLK)Enum.Parse(typeof(ChatMessageTypeWotLK), packet.Params.Type.ToString());
			GetSession().WorldClient.SendMessageChatWotLK(type, lang, msg, channel, packet.Target);
		}
		else
		{
			ChatMessageTypeVanilla type2 = (ChatMessageTypeVanilla)Enum.Parse(typeof(ChatMessageTypeVanilla), packet.Params.Type.ToString());
			GetSession().WorldClient.SendMessageChatVanilla(type2, lang, msg, channel, packet.Target);
		}
	}

	[PacketHandler(Opcode.CMSG_SEND_TEXT_EMOTE)]
	private void HandleSendTextEmote(CTextEmote emote)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_SEND_TEXT_EMOTE);
		worldPacket.WriteInt32(emote.EmoteID);
		worldPacket.WriteInt32(emote.SoundIndex);
		worldPacket.WriteGuid(emote.Target.To64());
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_CHAT_REGISTER_ADDON_PREFIXES)]
	private void HandleChatRegisterAddonPrefixes(ChatRegisterAddonPrefixes addons)
	{
		foreach (string prefix in addons.Prefixes)
		{
			GetSession().GameState.AddonPrefixes.Add(prefix);
		}
	}

	[PacketHandler(Opcode.CMSG_CHAT_UNREGISTER_ALL_ADDON_PREFIXES)]
	private void HandleChatUnregisterAllAddonPrefixes(EmptyClientPacket addons)
	{
		GetSession().GameState.AddonPrefixes.Clear();
	}

	private static List<string> ConvertTextMessageIntoMaxLengthParts(string originalTextMessage)
	{
		List<string> list = new List<string>();
		if (originalTextMessage.Length <= 255)
		{
			list.Add(originalTextMessage);
		}
		else
		{
			string text = "(?=\\|c[a-f0-9]{8}\\|H)";
			string text2 = "(?<=\\|h\\|r)";
			IEnumerable<char[]> enumerable = Regex.Split(originalTextMessage, text + "|" + text2).SelectMany((string x) => x.Chunk(255));
			StringBuilder stringBuilder = new StringBuilder();
			foreach (char[] item in enumerable)
			{
				if (stringBuilder.Length + item.Length > 255)
				{
					list.Add(stringBuilder.ToString());
					stringBuilder.Clear();
				}
				stringBuilder.Append(item);
			}
			list.Add(stringBuilder.ToString());
		}
		return list;
	}

	[PacketHandler(Opcode.CMSG_UPDATE_ACCOUNT_DATA)]
	private void HandleUpdateAccountData(UserClientUpdateAccountData data)
	{
		GetSession().AccountDataMgr.SaveData(data.PlayerGuid, data.Time, data.DataType, data.Size, data.CompressedData);
	}

	[PacketHandler(Opcode.CMSG_REQUEST_ACCOUNT_DATA)]
	private void HandleRequestAccountData(RequestAccountData data)
	{
		if (GetSession().AccountDataMgr.Data[data.DataType] == null)
		{
			Log.Print(LogType.Error, $"Client requested missing account data {data.DataType}.", "HandleRequestAccountData", "D:\\a\\HermesProxy\\HermesProxy\\World\\Server\\PacketHandlers\\ClientConfigHandler.cs");
			GetSession().AccountDataMgr.Data[data.DataType] = new AccountData();
			GetSession().AccountDataMgr.Data[data.DataType].Type = data.DataType;
			GetSession().AccountDataMgr.Data[data.DataType].Timestamp = Time.UnixTime;
			GetSession().AccountDataMgr.Data[data.DataType].UncompressedSize = 0u;
			GetSession().AccountDataMgr.Data[data.DataType].CompressedData = new byte[0];
		}
		GetSession().AccountDataMgr.Data[data.DataType].Guid = data.PlayerGuid;
		UpdateAccountData packet = new UpdateAccountData(GetSession().AccountDataMgr.Data[data.DataType]);
		SendPacket(packet);
	}

	[PacketHandler(Opcode.CMSG_SAVE_CUF_PROFILES)]
	private void HandleUpdateAccountData(SaveCUFProfiles cuf)
	{
		GetSession().AccountDataMgr.SaveCUFProfiles(cuf.Data);
	}

	[PacketHandler(Opcode.CMSG_ATTACK_SWING)]
	private void HandleAttackSwing(AttackSwing attack)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_ATTACK_SWING);
		worldPacket.WriteGuid(attack.Victim.To64());
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_ATTACK_STOP)]
	private void HandleAttackSwing(AttackStop attack)
	{
		WorldPacket packet = new WorldPacket(Opcode.CMSG_ATTACK_STOP);
		SendPacketToServer(packet);
	}

	[PacketHandler(Opcode.CMSG_SET_SHEATHED)]
	private void HandleSetSheathed(SetSheathed sheath)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_SET_SHEATHED);
		worldPacket.WriteInt32(sheath.SheathState);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_CAN_DUEL)]
	private void HandleCanDuel(CanDuel request)
	{
		CanDuelResult canDuelResult = new CanDuelResult();
		canDuelResult.TargetGUID = request.TargetGUID;
		canDuelResult.Result = true;
		SendPacket(canDuelResult);
	}

	[PacketHandler(Opcode.CMSG_DUEL_RESPONSE)]
	private void HandleDuelResponse(DuelResponse response)
	{
		if (response.Accepted)
		{
			WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_DUEL_ACCEPTED);
			worldPacket.WriteGuid(response.ArbiterGUID.To64());
			SendPacketToServer(worldPacket);
		}
		else
		{
			WorldPacket worldPacket2 = new WorldPacket(Opcode.CMSG_DUEL_CANCELLED);
			worldPacket2.WriteGuid(response.ArbiterGUID.To64());
			SendPacketToServer(worldPacket2);
		}
	}

	[PacketHandler(Opcode.CMSG_GAME_OBJ_USE)]
	private void HandleGameObjUse(GameObjUse use)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_GAME_OBJ_USE);
		worldPacket.WriteGuid(use.Guid.To64());
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_GAME_OBJ_REPORT_USE)]
	private void HandleGameObjUse(GameObjReportUse use)
	{
		GetSession().GameState.CurrentInteractedWithGO = use.Guid;
	}

	[PacketHandler(Opcode.CMSG_PARTY_INVITE)]
	private void HandleUpdateRaidTarget(PartyInviteClient invite)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_PARTY_INVITE);
		worldPacket.WriteCString(invite.TargetName);
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			worldPacket.WriteUInt32(0u);
		}
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_PARTY_INVITE_RESPONSE)]
	private void HandlePartyInviteResponse(PartyInviteResponse invite)
	{
		if (invite.Accept)
		{
			WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_GROUP_ACCEPT);
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
			{
				worldPacket.WriteUInt32(0u);
			}
			SendPacketToServer(worldPacket);
		}
		else
		{
			WorldPacket packet = new WorldPacket(Opcode.CMSG_GROUP_DECLINE);
			SendPacketToServer(packet);
		}
	}

	[PacketHandler(Opcode.CMSG_LEAVE_GROUP)]
	private void HandleLeaveGroup(LeaveGroup leave)
	{
		GetSession().GameState.WeWantToLeaveGroup = true;
		WorldPacket packet = new WorldPacket(Opcode.CMSG_GROUP_DISBAND);
		SendPacketToServer(packet);
	}

	[PacketHandler(Opcode.CMSG_PARTY_UNINVITE)]
	private void HandlePartyUninvite(PartyUninvite kick)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_GROUP_UNINVITE_GUID);
		worldPacket.WriteGuid(kick.TargetGUID.To64());
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			worldPacket.WriteCString(kick.Reason);
		}
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_SET_ASSISTANT_LEADER)]
	private void HandleSetAssistantLeader(SetAssistantLeader assist)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_SET_ASSISTANT_LEADER);
		worldPacket.WriteGuid(assist.TargetGUID.To64());
		worldPacket.WriteBool(assist.Apply);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_SET_EVERYONE_IS_ASSISTANT)]
	private void HandleSetAssistantLeader(SetEveryoneIsAssistant assist)
	{
		foreach (PartyPlayerInfo player in GetSession().GameState.GetCurrentGroup().PlayerList)
		{
			if (!(player.GUID == GetSession().GameState.CurrentPlayerGuid))
			{
				WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_SET_ASSISTANT_LEADER);
				worldPacket.WriteGuid(player.GUID.To64());
				worldPacket.WriteBool(assist.Apply);
				SendPacketToServer(worldPacket);
			}
		}
	}

	[PacketHandler(Opcode.CMSG_SET_PARTY_LEADER)]
	private void HandleSetPartyLeader(SetPartyLeader leader)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_SET_PARTY_LEADER);
		worldPacket.WriteGuid(leader.TargetGUID.To64());
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_CONVERT_RAID)]
	private void HandleConvertRaid(ConvertRaid raid)
	{
		WorldPacket packet = new WorldPacket(Opcode.CMSG_CONVERT_RAID);
		SendPacketToServer(packet);
	}

	[PacketHandler(Opcode.CMSG_DO_READY_CHECK)]
	private void HandlReadyCheck(DoReadyCheck raid)
	{
		WorldPacket packet = new WorldPacket(Opcode.MSG_RAID_READY_CHECK);
		SendPacketToServer(packet);
	}

	[PacketHandler(Opcode.CMSG_READY_CHECK_RESPONSE)]
	private void HandlReadyCheckResponse(ReadyCheckResponseClient raid)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.MSG_RAID_READY_CHECK);
		worldPacket.WriteBool(raid.IsReady);
		SendPacketToServer(worldPacket);
		ReadyCheckResponse readyCheckResponse = new ReadyCheckResponse();
		readyCheckResponse.Player = GetSession().GameState.CurrentPlayerGuid;
		readyCheckResponse.IsReady = raid.IsReady;
		readyCheckResponse.PartyGUID = WowGuid128.Create(HighGuidType703.Party, 1000uL);
		SendPacket(readyCheckResponse);
	}

	[PacketHandler(Opcode.CMSG_UPDATE_RAID_TARGET)]
	private void HandleUpdateRaidTarget(UpdateRaidTarget update)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.MSG_RAID_TARGET_UPDATE);
		worldPacket.WriteInt8(update.Symbol);
		worldPacket.WriteGuid(update.Target.To64());
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_SUMMON_RESPONSE)]
	private void HandleSummonResponse(SummonResponse update)
	{
		if (update.Accept || LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_SUMMON_RESPONSE);
			worldPacket.WriteGuid(update.SummonerGUID.To64());
			worldPacket.WriteBool(update.Accept);
			SendPacketToServer(worldPacket);
		}
	}

	[PacketHandler(Opcode.CMSG_MINIMAP_PING)]
	private void HandleMinimapPing(MinimapPingClient ping)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.MSG_MINIMAP_PING);
		worldPacket.WriteVector2(ping.Position);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_RANDOM_ROLL)]
	private void HandleMinimapPing(RandomRollClient roll)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.MSG_RANDOM_ROLL);
		worldPacket.WriteInt32(roll.Min);
		worldPacket.WriteInt32(roll.Max);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_REQUEST_PARTY_MEMBER_STATS)]
	private void HandleRequestPartyMemberStats(RequestPartyMemberStats request)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_REQUEST_PARTY_MEMBER_STATS);
		worldPacket.WriteGuid(request.TargetGUID.To64());
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_GROUP_CHANGE_SUB_GROUP)]
	private void HandleGroupChangeSubGroup(ChangeSubGroup group)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_GROUP_CHANGE_SUB_GROUP);
		worldPacket.WriteCString(GetSession().GameState.GetPlayerName(group.TargetGUID));
		worldPacket.WriteUInt8(group.NewSubGroup);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_GROUP_SWAP_SUB_GROUP)]
	private void HandleGroupSwapSubGroup(SwapSubGroups group)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_GROUP_SWAP_SUB_GROUP);
		worldPacket.WriteCString(GetSession().GameState.GetPlayerName(group.FirstTarget));
		worldPacket.WriteCString(GetSession().GameState.GetPlayerName(group.SecondTarget));
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_QUERY_GUILD_INFO)]
	private void HandleQueryGuildInfo(QueryGuildInfo query)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_QUERY_GUILD_INFO);
		worldPacket.WriteUInt32((uint)query.GuildGuid.GetCounter());
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_GUILD_PERMISSIONS_QUERY)]
	private void HandleGuildPermissionsQuery(GuildPermissionsQuery query)
	{
		if (!LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			WorldPacket packet = new WorldPacket(Opcode.MSG_GUILD_PERMISSIONS);
			SendPacketToServer(packet);
		}
	}

	[PacketHandler(Opcode.CMSG_GUILD_BANK_REMAINING_WITHDRAW_MONEY_QUERY)]
	private void HandleGuildBankRemainingWithdrawnMoneyQuery(GuildBankRemainingWithdrawMoneyQuery query)
	{
		if (!LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			WorldPacket packet = new WorldPacket(Opcode.MSG_GUILD_BANK_MONEY_WITHDRAWN);
			SendPacketToServer(packet);
		}
	}

	[PacketHandler(Opcode.CMSG_GUILD_GET_ROSTER)]
	private void HandleGuildGetRoster(GuildGetRoster query)
	{
		WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_INFO);
		SendPacketToServer(packet);
		WorldPacket packet2 = new WorldPacket(Opcode.CMSG_GUILD_GET_ROSTER);
		SendPacketToServer(packet2);
	}

	[PacketHandler(Opcode.CMSG_GUILD_UPDATE_MOTD_TEXT)]
	private void HandleGuildUpdateMotdText(GuildUpdateMotdText text)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_GUILD_UPDATE_MOTD_TEXT);
		worldPacket.WriteCString(text.MotdText);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_GUILD_UPDATE_INFO_TEXT)]
	private void HandleGuildUpdateInfoText(GuildUpdateInfoText text)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_GUILD_UPDATE_INFO_TEXT);
		worldPacket.WriteCString(text.InfoText);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_GUILD_SET_MEMBER_NOTE)]
	private void HandleGuildSetMemberNote(GuildSetMemberNote note)
	{
		WorldPacket worldPacket = new WorldPacket(note.IsPublic ? Opcode.CMSG_GUILD_SET_PUBLIC_NOTE : Opcode.CMSG_GUILD_SET_OFFICER_NOTE);
		worldPacket.WriteCString(GetSession().GameState.GetPlayerName(note.NoteeGUID));
		worldPacket.WriteCString(note.Note);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_GUILD_PROMOTE_MEMBER)]
	private void HandleGuildPromoteMember(GuildPromoteMember promote)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_GUILD_PROMOTE_MEMBER);
		worldPacket.WriteCString(GetSession().GameState.GetPlayerName(promote.Promotee));
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_GUILD_DEMOTE_MEMBER)]
	private void HandleGuildDemoteMember(GuildDemoteMember demote)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_GUILD_DEMOTE_MEMBER);
		worldPacket.WriteCString(GetSession().GameState.GetPlayerName(demote.Demotee));
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_GUILD_OFFICER_REMOVE_MEMBER)]
	private void HandleGuildOfficerRemoveMember(GuildOfficerRemoveMember remove)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_GUILD_OFFICER_REMOVE_MEMBER);
		worldPacket.WriteCString(GetSession().GameState.GetPlayerName(remove.Removee));
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_GUILD_INVITE_BY_NAME)]
	private void HandleGuildInviteByName(GuildInviteByName invite)
	{
		if (invite.ArenaTeamId == 0)
		{
			WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_GUILD_INVITE_BY_NAME);
			worldPacket.WriteCString(invite.Name);
			SendPacketToServer(worldPacket);
		}
		else
		{
			WorldPacket worldPacket2 = new WorldPacket(Opcode.CMSG_ARENA_TEAM_INVITE);
			worldPacket2.WriteUInt32(invite.ArenaTeamId);
			worldPacket2.WriteCString(invite.Name);
			SendPacketToServer(worldPacket2);
		}
	}

	[PacketHandler(Opcode.CMSG_GUILD_SET_RANK_PERMISSIONS)]
	private void HandleGuildSetRankPermissions(GuildSetRankPermissions rank)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_GUILD_SET_RANK_PERMISSIONS);
		worldPacket.WriteUInt32(rank.RankID);
		worldPacket.WriteUInt32(rank.Flags);
		worldPacket.WriteCString(rank.RankName);
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			worldPacket.WriteInt32(rank.WithdrawGoldLimit);
			for (int i = 0; i < 6; i++)
			{
				worldPacket.WriteUInt32(rank.TabFlags[i]);
				worldPacket.WriteUInt32(rank.TabWithdrawItemLimit[i]);
			}
		}
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_GUILD_ADD_RANK)]
	private void HandleGuildAddRank(GuildAddRank rank)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_GUILD_ADD_RANK);
		worldPacket.WriteCString(rank.Name);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_GUILD_DELETE_RANK)]
	private void HandleGuildDeleteRank(GuildDeleteRank rank)
	{
		WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_DELETE_RANK);
		SendPacketToServer(packet);
	}

	[PacketHandler(Opcode.CMSG_GUILD_SET_GUILD_MASTER)]
	private void HandleGuildSetGuildMaster(GuildSetGuildMaster master)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_GUILD_SET_GUILD_MASTER);
		worldPacket.WriteCString(master.NewMasterName);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_GUILD_LEAVE)]
	private void HandleGuildLeave(GuildLeave leave)
	{
		WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_LEAVE);
		SendPacketToServer(packet);
	}

	[PacketHandler(Opcode.CMSG_ACCEPT_GUILD_INVITE)]
	private void HandleGuildAccept(AcceptGuildInvite accept)
	{
		WorldPacket packet = new WorldPacket(Opcode.CMSG_ACCEPT_GUILD_INVITE);
		SendPacketToServer(packet);
	}

	[PacketHandler(Opcode.CMSG_GUILD_DECLINE_INVITATION)]
	private void HandleGuildDecline(DeclineGuildInvite decline)
	{
		WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_DECLINE_INVITATION);
		SendPacketToServer(packet);
	}

	[PacketHandler(Opcode.CMSG_GUILD_DELETE)]
	private void HandleGuildDelete(GuildDelete delete)
	{
		WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_DELETE);
		SendPacketToServer(packet);
	}

	[PacketHandler(Opcode.CMSG_SAVE_GUILD_EMBLEM)]
	private void HandleSaveGuildEmblem(SaveGuildEmblem emblem)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.MSG_SAVE_GUILD_EMBLEM);
		worldPacket.WriteGuid(emblem.DesignerGUID.To64());
		worldPacket.WriteUInt32(emblem.EmblemStyle);
		worldPacket.WriteUInt32(emblem.EmblemColor);
		worldPacket.WriteUInt32(emblem.BorderStyle);
		worldPacket.WriteUInt32(emblem.BorderColor);
		worldPacket.WriteUInt32(emblem.BackgroundColor);
		SendPacketToServer(worldPacket);
	}

    //处理拒绝公会邀请
    [PacketHandler(Opcode.CMSG_DECLINE_GUILD_INVITES)]
	private void HandleDeclineGuildInvites(SetAutoDeclineGuildInvites packet)
	{
		GetSession().GameState.CurrentPlayerStorage.Settings.SetAutoBlockGuildInvites(packet.GuildInvitesShouldGetBlocked);
		ObjectUpdate objectUpdate = new ObjectUpdate(GetSession().GameState.CurrentPlayerGuid, UpdateTypeModern.Values, GetSession());
		PlayerFlags value = GetSession().GameState.CurrentPlayerStorage.Settings.CreateNewFlags();
		objectUpdate.PlayerData.PlayerFlags = (uint)value;
		UpdateObject updateObject = new UpdateObject(GetSession().GameState);
		updateObject.ObjectUpdates.Add(objectUpdate);
		GetSession().WorldClient.SendPacketToClient(updateObject);
	}

	[PacketHandler(Opcode.CMSG_GUILD_AUTO_DECLINE_INVITATION)]
	private void HandleGuildAutoDeclineInvitation(AutoDeclineGuildInvite autoDecline)
	{
		WorldPacket packet = new WorldPacket(Opcode.CMSG_GUILD_DECLINE_INVITATION);
		SendPacketToServer(packet);
	}

	[PacketHandler(Opcode.CMSG_GUILD_BANK_ACTIVATE)]
	private void HandleGuildBankActivate(GuildBankAtivate activate)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_GUILD_BANK_ACTIVATE);
		worldPacket.WriteGuid(activate.BankGuid.To64());
		worldPacket.WriteBool(activate.FullUpdate);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_GUILD_BANK_QUERY_TAB)]
	private void HandleGuildBankQueryTab(GuildBankQueryTab query)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_GUILD_BANK_QUERY_TAB);
		worldPacket.WriteGuid(query.BankGuid.To64());
		worldPacket.WriteUInt8(query.Tab);
		worldPacket.WriteBool(query.FullUpdate);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_GUILD_BANK_DEPOSIT_MONEY)]
	private void HandleGuildBankDepositMoney(GuildBankDepositMoney deposit)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_GUILD_BANK_DEPOSIT_MONEY);
		worldPacket.WriteGuid(deposit.BankGuid.To64());
		worldPacket.WriteUInt32((uint)deposit.Money);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_GUILD_BANK_TEXT_QUERY)]
	private void HandleGuildBankTextQuery(GuildBankTextQuery query)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.MSG_QUERY_GUILD_BANK_TEXT);
		worldPacket.WriteUInt8((byte)query.Tab);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_GUILD_BANK_UPDATE_TAB)]
	private void HandleGuildBankUpdateTab(GuildBankUpdateTab update)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_GUILD_BANK_UPDATE_TAB);
		worldPacket.WriteGuid(update.BankGuid.To64());
		worldPacket.WriteUInt8(update.BankTab);
		worldPacket.WriteCString(update.Name);
		worldPacket.WriteCString(update.Icon);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_GUILD_BANK_LOG_QUERY)]
	private void HandleGuildBankLogQuery(GuildBankLogQuery query)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.MSG_GUILD_BANK_LOG_QUERY);
		worldPacket.WriteUInt8((byte)query.Tab);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_GUILD_BANK_SET_TAB_TEXT)]
	private void HandleGuildBankSetTabText(GuildBankSetTabText query)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_GUILD_BANK_SET_TAB_TEXT);
		worldPacket.WriteUInt8((byte)query.Tab);
		worldPacket.WriteCString(query.TabText);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_GUILD_BANK_BUY_TAB)]
	private void HandleGuildBankBuyTab(GuildBankBuyTab buy)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_GUILD_BANK_BUY_TAB);
		worldPacket.WriteGuid(buy.BankGuid.To64());
		worldPacket.WriteUInt8(buy.BankTab);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_GUILD_BANK_WITHDRAW_MONEY)]
	private void HandleGuildBankBuyTab(GuildBankWithdrawMoney withdraw)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_GUILD_BANK_WITHDRAW_MONEY);
		worldPacket.WriteGuid(withdraw.BankGuid.To64());
		worldPacket.WriteUInt32((uint)withdraw.Money);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_AUTO_GUILD_BANK_ITEM)]
	private void HandleGuildBankItem(AutoGuildBankItem item)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_GUILD_BANK_SWAP_ITEMS);
		worldPacket.WriteGuid(item.BankGuid.To64());
		worldPacket.WriteBool(data: false);
		worldPacket.WriteUInt8(item.BankTab);
		worldPacket.WriteUInt8(item.BankSlot);
		worldPacket.WriteUInt32(0u);
		worldPacket.WriteBool(data: false);
		if (item.ContainerSlot.HasValue)
		{
			worldPacket.WriteUInt8(ModernVersion.AdjustInventorySlot(item.ContainerSlot.Value));
			worldPacket.WriteUInt8(item.ContainerItemSlot);
		}
		else
		{
			worldPacket.WriteUInt8(byte.MaxValue);
			worldPacket.WriteUInt8(ModernVersion.AdjustInventorySlot(item.ContainerItemSlot));
		}
		worldPacket.WriteBool(data: false);
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			worldPacket.WriteUInt32(0u);
		}
		else
		{
			worldPacket.WriteUInt8(0);
		}
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_SPLIT_ITEM_TO_GUILD_BANK)]
	[PacketHandler(Opcode.CMSG_MERGE_ITEM_WITH_GUILD_BANK_ITEM)]
	private void HandleSplitItemToGuildBank(SplitItemToGuildBank item)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_GUILD_BANK_SWAP_ITEMS);
		worldPacket.WriteGuid(item.BankGuid.To64());
		worldPacket.WriteBool(data: false);
		worldPacket.WriteUInt8(item.BankTab);
		worldPacket.WriteUInt8(item.BankSlot);
		worldPacket.WriteUInt32(0u);
		worldPacket.WriteBool(data: false);
		if (item.ContainerSlot.HasValue)
		{
			worldPacket.WriteUInt8(ModernVersion.AdjustInventorySlot(item.ContainerSlot.Value));
			worldPacket.WriteUInt8(item.ContainerItemSlot);
		}
		else
		{
			worldPacket.WriteUInt8(byte.MaxValue);
			worldPacket.WriteUInt8(ModernVersion.AdjustInventorySlot(item.ContainerItemSlot));
		}
		worldPacket.WriteBool(data: false);
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			worldPacket.WriteUInt32(item.StackCount);
		}
		else
		{
			worldPacket.WriteUInt8((byte)item.StackCount);
		}
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_AUTO_STORE_GUILD_BANK_ITEM)]
	private void HandleAutoStoreGuildBankItem(AutoStoreGuildBankItem item)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_GUILD_BANK_SWAP_ITEMS);
		worldPacket.WriteGuid(item.BankGuid.To64());
		worldPacket.WriteBool(data: false);
		worldPacket.WriteUInt8(item.BankTab);
		worldPacket.WriteUInt8(item.BankSlot);
		worldPacket.WriteUInt32(0u);
		worldPacket.WriteBool(data: true);
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			worldPacket.WriteUInt32(0u);
		}
		else
		{
			worldPacket.WriteUInt8(0);
		}
		worldPacket.WriteBool(data: true);
		worldPacket.WriteUInt8(0);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_STORE_GUILD_BANK_ITEM)]
	private void HandleStoreGuildBankItem(AutoGuildBankItem item)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_GUILD_BANK_SWAP_ITEMS);
		worldPacket.WriteGuid(item.BankGuid.To64());
		worldPacket.WriteBool(data: false);
		worldPacket.WriteUInt8(item.BankTab);
		worldPacket.WriteUInt8(item.BankSlot);
		worldPacket.WriteUInt32(0u);
		worldPacket.WriteBool(data: false);
		if (item.ContainerSlot.HasValue)
		{
			worldPacket.WriteUInt8(ModernVersion.AdjustInventorySlot(item.ContainerSlot.Value));
			worldPacket.WriteUInt8(item.ContainerItemSlot);
		}
		else
		{
			worldPacket.WriteUInt8(byte.MaxValue);
			worldPacket.WriteUInt8(ModernVersion.AdjustInventorySlot(item.ContainerItemSlot));
		}
		worldPacket.WriteBool(data: true);
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			worldPacket.WriteUInt32(0u);
		}
		else
		{
			worldPacket.WriteUInt8(0);
		}
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_MERGE_GUILD_BANK_ITEM_WITH_ITEM)]
	[PacketHandler(Opcode.CMSG_SPLIT_GUILD_BANK_ITEM_TO_INVENTORY)]
	private void HandleMergeGuildBankItemWithItem(SplitItemToGuildBank item)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_GUILD_BANK_SWAP_ITEMS);
		worldPacket.WriteGuid(item.BankGuid.To64());
		worldPacket.WriteBool(data: false);
		worldPacket.WriteUInt8(item.BankTab);
		worldPacket.WriteUInt8(item.BankSlot);
		worldPacket.WriteUInt32(0u);
		worldPacket.WriteBool(data: false);
		if (item.ContainerSlot.HasValue)
		{
			worldPacket.WriteUInt8(ModernVersion.AdjustInventorySlot(item.ContainerSlot.Value));
			worldPacket.WriteUInt8(item.ContainerItemSlot);
		}
		else
		{
			worldPacket.WriteUInt8(byte.MaxValue);
			worldPacket.WriteUInt8(ModernVersion.AdjustInventorySlot(item.ContainerItemSlot));
		}
		worldPacket.WriteBool(data: true);
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			worldPacket.WriteUInt32(item.StackCount);
		}
		else
		{
			worldPacket.WriteUInt8((byte)item.StackCount);
		}
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_MOVE_GUILD_BANK_ITEM)]
	private void HandleMoveGuildBankItem(MoveGuildBankItem item)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_GUILD_BANK_SWAP_ITEMS);
		worldPacket.WriteGuid(item.BankGuid.To64());
		worldPacket.WriteBool(data: true);
		worldPacket.WriteUInt8(item.BankTab2);
		worldPacket.WriteUInt8(item.BankSlot2);
		worldPacket.WriteUInt32(0u);
		worldPacket.WriteUInt8(item.BankTab1);
		worldPacket.WriteUInt8(item.BankSlot1);
		worldPacket.WriteUInt32(0u);
		worldPacket.WriteBool(data: false);
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			worldPacket.WriteUInt32(0u);
		}
		else
		{
			worldPacket.WriteUInt8(0);
		}
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_SPLIT_GUILD_BANK_ITEM)]
	[PacketHandler(Opcode.CMSG_MERGE_GUILD_BANK_ITEM_WITH_GUILD_BANK_ITEM)]
	private void HandleMoveGuildBankItem(SplitGuildBankItem item)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_GUILD_BANK_SWAP_ITEMS);
		worldPacket.WriteGuid(item.BankGuid.To64());
		worldPacket.WriteBool(data: true);
		worldPacket.WriteUInt8(item.BankTab2);
		worldPacket.WriteUInt8(item.BankSlot2);
		worldPacket.WriteUInt32(0u);
		worldPacket.WriteUInt8(item.BankTab1);
		worldPacket.WriteUInt8(item.BankSlot1);
		worldPacket.WriteUInt32(0u);
		worldPacket.WriteBool(data: false);
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			worldPacket.WriteUInt32(item.StackCount);
		}
		else
		{
			worldPacket.WriteUInt8((byte)item.StackCount);
		}
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_DB_QUERY_BULK)]
	private void HandleDbQueryBulk(DBQueryBulk query)
	{
		foreach (uint query2 in query.Queries)
		{
			DBReply dBReply = new DBReply();
			dBReply.RecordID = query2;
			dBReply.TableHash = query.TableHash;
			dBReply.Status = HotfixStatus.Invalid;
			dBReply.Timestamp = (uint)Time.UnixTime;
			Log.PrintNet(LogType.Debug, LogNetDir.C2P, $"DB_QUERY_BULK requested ({query.TableHash}) #{query2}", "HandleDbQueryBulk", "D:\\a\\HermesProxy\\HermesProxy\\World\\Server\\PacketHandlers\\HotfixHandler.cs");
			if (query.TableHash == DB2Hash.BroadcastText)
			{
				BroadcastText broadcastText = GameData.GetBroadcastText(query2);
				if (broadcastText == null)
				{
					broadcastText = new BroadcastText();
					broadcastText.Entry = query2;
					broadcastText.MaleText = "Clear your cache!";
					broadcastText.FemaleText = "Clear your cache!";
				}
				dBReply.Status = HotfixStatus.Valid;
				dBReply.Data.WriteCString(broadcastText.MaleText);
				dBReply.Data.WriteCString(broadcastText.FemaleText);
				dBReply.Data.WriteUInt32(broadcastText.Entry);
				dBReply.Data.WriteUInt32(broadcastText.Language);
				dBReply.Data.WriteUInt32(0u);
				dBReply.Data.WriteUInt16(0);
				dBReply.Data.WriteUInt8(0);
				dBReply.Data.WriteUInt32(0u);
				if (ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 1, 2, 5, 3))
				{
					dBReply.Data.WriteUInt32(0u);
				}
				for (int i = 0; i < 2; i++)
				{
					dBReply.Data.WriteUInt32(0u);
				}
				for (int j = 0; j < 3; j++)
				{
					dBReply.Data.WriteUInt16(broadcastText.Emotes[j]);
				}
				for (int k = 0; k < 3; k++)
				{
					dBReply.Data.WriteUInt16(broadcastText.EmoteDelays[k]);
				}
			}
			else if (query.TableHash == DB2Hash.Item)
			{
				ItemTemplate itemTemplate = GameData.GetItemTemplate(query2);
				if (itemTemplate != null)
				{
					dBReply.Status = HotfixStatus.Valid;
					GameData.WriteItemHotfix(itemTemplate, dBReply.Data);
				}
				else if (!GetSession().GameState.RequestedItemHotfixes.Contains(query2) && GetSession().WorldClient != null && GetSession().WorldClient.IsConnected())
				{
					GetSession().GameState.RequestedItemHotfixes.Add(query2);
					WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_ITEM_QUERY_SINGLE);
					worldPacket.WriteUInt32(query2);
					if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
					{
						worldPacket.WriteGuid(WowGuid64.Empty);
					}
					SendPacketToServer(worldPacket);
					continue;
				}
			}
			else if (query.TableHash == DB2Hash.ItemSparse)
			{
				ItemTemplate itemTemplate2 = GameData.GetItemTemplate(query2);
				if (itemTemplate2 != null)
				{
					dBReply.Status = HotfixStatus.Valid;
					GameData.WriteItemSparseHotfix(itemTemplate2, dBReply.Data);
				}
				else if (!GetSession().GameState.RequestedItemSparseHotfixes.Contains(query2) && GetSession().WorldClient != null && GetSession().WorldClient.IsConnected())
				{
					GetSession().GameState.RequestedItemSparseHotfixes.Add(query2);
					WorldPacket worldPacket2 = new WorldPacket(Opcode.CMSG_ITEM_QUERY_SINGLE);
					worldPacket2.WriteUInt32(query2);
					if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
					{
						worldPacket2.WriteGuid(WowGuid64.Empty);
					}
					SendPacketToServer(worldPacket2);
					continue;
				}
			}
			SendPacket(dBReply);
		}
	}

	[PacketHandler(Opcode.CMSG_HOTFIX_REQUEST)]
	private void HandleHotfixRequest(HotfixRequest request)
	{
		HotfixConnect hotfixConnect = new HotfixConnect();
		foreach (uint hotfix in request.Hotfixes)
		{
			if (GameData.Hotfixes.TryGetValue(hotfix, out var value))
			{
				Log.Print(LogType.Debug, $"Hotfix record {value.RecordId} from {value.TableHash}.", "HandleHotfixRequest", "D:\\a\\HermesProxy\\HermesProxy\\World\\Server\\PacketHandlers\\HotfixHandler.cs");
				hotfixConnect.Hotfixes.Add(value);
			}
		}
		SendPacket(hotfixConnect);
	}

	[PacketHandler(Opcode.CMSG_RESET_INSTANCES)]
	private void HandleResetInstances(EmptyClientPacket reset)
	{
		WorldPacket packet = new WorldPacket(Opcode.CMSG_RESET_INSTANCES);
		SendPacketToServer(packet);
	}

	[PacketHandler(Opcode.CMSG_REQUEST_RAID_INFO)]
	private void HandleRequestRaidInfo(EmptyClientPacket reset)
	{
		WorldPacket packet = new WorldPacket(Opcode.CMSG_REQUEST_RAID_INFO);
		SendPacketToServer(packet);
	}

	[PacketHandler(Opcode.CMSG_BUY_ITEM)]
	private void HandleBuyItem(BuyItem item)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_BUY_ITEM);
		worldPacket.WriteGuid(item.VendorGUID.To64());
		worldPacket.WriteUInt32(item.Item.ItemID);
		uint num = item.Quantity / GetSession().GameState.GetItemBuyCount(item.Item.ItemID);
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
		{
			worldPacket.WriteUInt32(item.Slot);
			worldPacket.WriteUInt32(num);
		}
		else
		{
			worldPacket.WriteUInt8((byte)num);
		}
		worldPacket.WriteUInt8((byte)item.BagSlot);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_SELL_ITEM)]
	private void HandleSellItem(SellItem item)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_SELL_ITEM);
		worldPacket.WriteGuid(item.VendorGUID.To64());
		worldPacket.WriteGuid(item.ItemGUID.To64());
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
		{
			worldPacket.WriteUInt32(item.Amount);
		}
		else
		{
			worldPacket.WriteUInt8((byte)item.Amount);
		}
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_SPLIT_ITEM)]
	private void HandleSplitItem(SplitItem item)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_SPLIT_ITEM);
		byte data = ((item.FromPackSlot != byte.MaxValue) ? ModernVersion.AdjustInventorySlot(item.FromPackSlot) : item.FromPackSlot);
		byte data2 = ((item.FromPackSlot == byte.MaxValue) ? ModernVersion.AdjustInventorySlot(item.FromSlot) : item.FromSlot);
		byte data3 = ((item.ToPackSlot != byte.MaxValue) ? ModernVersion.AdjustInventorySlot(item.ToPackSlot) : item.ToPackSlot);
		byte data4 = ((item.ToPackSlot == byte.MaxValue) ? ModernVersion.AdjustInventorySlot(item.ToSlot) : item.ToSlot);
		worldPacket.WriteUInt8(data);
		worldPacket.WriteUInt8(data2);
		worldPacket.WriteUInt8(data3);
		worldPacket.WriteUInt8(data4);
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
		{
			worldPacket.WriteInt32(item.Quantity);
		}
		else
		{
			worldPacket.WriteUInt8((byte)item.Quantity);
		}
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_SWAP_INV_ITEM)]
	private void HandleSwapInvItem(SwapInvItem item)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_SWAP_INV_ITEM);
		byte data = ModernVersion.AdjustInventorySlot(item.Slot1);
		byte data2 = ModernVersion.AdjustInventorySlot(item.Slot2);
		worldPacket.WriteUInt8(data);
		worldPacket.WriteUInt8(data2);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_SWAP_ITEM)]
	private void HandleSwapItem(SwapItem item)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_SWAP_ITEM);
		byte data = ((item.ContainerSlotB != byte.MaxValue) ? ModernVersion.AdjustInventorySlot(item.ContainerSlotB) : item.ContainerSlotB);
		byte data2 = ((item.ContainerSlotB == byte.MaxValue) ? ModernVersion.AdjustInventorySlot(item.SlotB) : item.SlotB);
		byte data3 = ((item.ContainerSlotA != byte.MaxValue) ? ModernVersion.AdjustInventorySlot(item.ContainerSlotA) : item.ContainerSlotA);
		byte data4 = ((item.ContainerSlotA == byte.MaxValue) ? ModernVersion.AdjustInventorySlot(item.SlotA) : item.SlotA);
		worldPacket.WriteUInt8(data);
		worldPacket.WriteUInt8(data2);
		worldPacket.WriteUInt8(data3);
		worldPacket.WriteUInt8(data4);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_DESTROY_ITEM)]
	private void HandleDestroyItem(DestroyItem item)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_DESTROY_ITEM);
		byte data = ((item.ContainerId != byte.MaxValue) ? ModernVersion.AdjustInventorySlot(item.ContainerId) : item.ContainerId);
		byte data2 = ((item.ContainerId == byte.MaxValue) ? ModernVersion.AdjustInventorySlot(item.SlotNum) : item.SlotNum);
		worldPacket.WriteUInt8(data);
		worldPacket.WriteUInt8(data2);
		worldPacket.WriteUInt32(item.Count);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_AUTO_EQUIP_ITEM)]
	[PacketHandler(Opcode.CMSG_AUTOSTORE_BANK_ITEM)]
	[PacketHandler(Opcode.CMSG_AUTOBANK_ITEM)]
	private void HandleAutoEquipItem(AutoEquipItem item)
	{
		WorldPacket worldPacket = new WorldPacket(item.GetUniversalOpcode());
		byte data = ((item.PackSlot != byte.MaxValue) ? ModernVersion.AdjustInventorySlot(item.PackSlot) : item.PackSlot);
		byte data2 = ((item.PackSlot == byte.MaxValue) ? ModernVersion.AdjustInventorySlot(item.Slot) : item.Slot);
		worldPacket.WriteUInt8(data);
		worldPacket.WriteUInt8(data2);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_AUTO_EQUIP_ITEM_SLOT)]
	private void HandleAutoEquipItemSlot(AutoEquipItemSlot item)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_AUTO_EQUIP_ITEM_SLOT);
		worldPacket.WriteGuid(item.Item.To64());
		byte data = ModernVersion.AdjustInventorySlot(item.ItemDstSlot);
		worldPacket.WriteUInt8(data);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_READ_ITEM)]
	private void HandleReadItem(ReadItem item)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_READ_ITEM);
		byte data = ((item.PackSlot != byte.MaxValue) ? ModernVersion.AdjustInventorySlot(item.PackSlot) : item.PackSlot);
		byte data2 = ((item.PackSlot == byte.MaxValue) ? ModernVersion.AdjustInventorySlot(item.Slot) : item.Slot);
		worldPacket.WriteUInt8(data);
		worldPacket.WriteUInt8(data2);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_BUY_BACK_ITEM)]
	private void HandleBuyBackItem(BuyBackItem item)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_BUY_BACK_ITEM);
		worldPacket.WriteGuid(item.VendorGUID.To64());
		byte data = ModernVersion.AdjustInventorySlot((byte)item.Slot);
		worldPacket.WriteUInt32(data);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_REPAIR_ITEM)]
	private void HandleRepairItem(RepairItem item)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_REPAIR_ITEM);
		worldPacket.WriteGuid(item.VendorGUID.To64());
		worldPacket.WriteGuid(item.ItemGUID.To64());
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			worldPacket.WriteBool(item.UseGuildBank);
		}
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_SOCKET_GEMS)]
	private void HandleSocketGems(SocketGems gems)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_SOCKET_GEMS);
		worldPacket.WriteGuid(gems.ItemGuid.To64());
		for (int i = 0; i < 3; i++)
		{
			worldPacket.WriteGuid(gems.Gems[i].To64());
		}
		SendPacketToServer(worldPacket);
		SocketGemsSuccess socketGemsSuccess = new SocketGemsSuccess();
		socketGemsSuccess.ItemGuid = gems.ItemGuid;
		SendPacket(socketGemsSuccess);
	}

	[PacketHandler(Opcode.CMSG_OPEN_ITEM)]
	private void HandleOpenItem(OpenItem item)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_OPEN_ITEM);
		byte data = ((item.PackSlot != byte.MaxValue) ? ModernVersion.AdjustInventorySlot(item.PackSlot) : item.PackSlot);
		byte data2 = ((item.PackSlot == byte.MaxValue) ? ModernVersion.AdjustInventorySlot(item.Slot) : item.Slot);
		worldPacket.WriteUInt8(data);
		worldPacket.WriteUInt8(data2);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_SET_AMMO)]
	private void HandleSetAmmo(SetAmmo ammo)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_SET_AMMO);
		worldPacket.WriteUInt32(ammo.ItemId);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_CANCEL_TEMP_ENCHANTMENT)]
	private void HandleCancelTempEnchantment(CancelTempEnchantment cancel)
	{
		if (!LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_CANCEL_TEMP_ENCHANTMENT);
			worldPacket.WriteUInt32(cancel.EnchantmentSlot);
			SendPacketToServer(worldPacket);
		}
	}

	[PacketHandler(Opcode.CMSG_WRAP_ITEM)]
	private void HandleWrapItem(WrapItem item)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_WRAP_ITEM);
		byte data = ((item.GiftBag != byte.MaxValue) ? ModernVersion.AdjustInventorySlot(item.GiftBag) : item.GiftBag);
		byte data2 = ((item.GiftBag == byte.MaxValue) ? ModernVersion.AdjustInventorySlot(item.GiftSlot) : item.GiftSlot);
		byte data3 = ((item.ItemBag != byte.MaxValue) ? ModernVersion.AdjustInventorySlot(item.ItemBag) : item.ItemBag);
		byte data4 = ((item.ItemBag == byte.MaxValue) ? ModernVersion.AdjustInventorySlot(item.ItemSlot) : item.ItemSlot);
		worldPacket.WriteUInt8(data);
		worldPacket.WriteUInt8(data2);
		worldPacket.WriteUInt8(data3);
		worldPacket.WriteUInt8(data4);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_LOOT_RELEASE)]
	private void HandleLootRelease(LootRelease loot)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_LOOT_RELEASE);
		worldPacket.WriteGuid(loot.Owner.To64());
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_LOOT_ITEM)]
	private void HandleLootItem(LootItemPkt loot)
	{
		foreach (LootRequest item in loot.Loot)
		{
			WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_AUTOSTORE_LOOT_ITEM);
			worldPacket.WriteUInt8(item.LootListID);
			SendPacketToServer(worldPacket);
		}
	}

	[PacketHandler(Opcode.CMSG_LOOT_UNIT)]
	private void HandleLootUnit(LootUnit loot)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_LOOT_UNIT);
		worldPacket.WriteGuid(loot.Unit.To64());
		SendPacketToServer(worldPacket);
		GetSession().GameState.LastLootTargetGuid = loot.Unit.To64();
	}

	[PacketHandler(Opcode.CMSG_LOOT_MONEY)]
	private void HandleLootMoney(LootMoney loot)
	{
		WorldPacket packet = new WorldPacket(Opcode.CMSG_LOOT_MONEY);
		SendPacketToServer(packet);
	}

	[PacketHandler(Opcode.CMSG_SET_LOOT_METHOD)]
	private void HandleSetLootMethod(SetLootMethod loot)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_SET_LOOT_METHOD);
		worldPacket.WriteUInt32((uint)loot.LootMethod);
		worldPacket.WriteGuid(loot.LootMasterGUID.To64());
		worldPacket.WriteUInt32(loot.LootThreshold);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_OPT_OUT_OF_LOOT)]
	private void HandleOptOutOfLoot(OptOutOfLoot loot)
	{
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_OPT_OUT_OF_LOOT);
			worldPacket.WriteInt32(loot.PassOnLoot ? 1 : 0);
			SendPacketToServer(worldPacket);
		}
		else
		{
			GetSession().GameState.IsPassingOnLoot = loot.PassOnLoot;
		}
	}

	[PacketHandler(Opcode.CMSG_LOOT_ROLL)]
	private void HandleLootRoll(LootRoll loot)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_LOOT_ROLL);
		worldPacket.WriteGuid(loot.LootObj.To64());
		worldPacket.WriteUInt32(loot.LootListID);
		worldPacket.WriteUInt8((byte)loot.RollType);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_LOOT_MASTER_GIVE)]
	private void HandleLootMasterGive(LootMasterGive loot)
	{
		foreach (LootRequest item in loot.Loot)
		{
			WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_LOOT_MASTER_GIVE);
			worldPacket.WriteGuid(item.LootObj.To64());
			worldPacket.WriteUInt8(item.LootListID);
			worldPacket.WriteGuid(loot.TargetGUID.To64());
			SendPacketToServer(worldPacket);
		}
	}

	[PacketHandler(Opcode.CMSG_QUERY_NEXT_MAIL_TIME)]
	private void HandleMailGetList(EmptyClientPacket mail)
	{
		WorldPacket packet = new WorldPacket(Opcode.MSG_QUERY_NEXT_MAIL_TIME);
		SendPacketToServer(packet);
	}

	[PacketHandler(Opcode.CMSG_MAIL_GET_LIST)]
	private void HandleMailGetList(MailGetList mail)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_MAIL_GET_LIST);
		worldPacket.WriteGuid(mail.Mailbox.To64());
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_MAIL_CREATE_TEXT_ITEM)]
	private void HandleMailCreateTextItem(MailCreateTextItem mail)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_MAIL_CREATE_TEXT_ITEM);
		worldPacket.WriteGuid(mail.Mailbox.To64());
		worldPacket.WriteUInt32(mail.MailID);
		if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			worldPacket.WriteUInt32(0u);
		}
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_MAIL_DELETE)]
	private void HandleMailDelete(MailDelete mail)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_MAIL_DELETE);
		worldPacket.WriteGuid(GetSession().GameState.CurrentInteractedWithGO.To64());
		worldPacket.WriteUInt32(mail.MailID);
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			worldPacket.WriteUInt32(0u);
		}
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_MAIL_MARK_AS_READ)]
	private void HandleMailMarkAsRead(MailMarkAsRead mail)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_MAIL_MARK_AS_READ);
		worldPacket.WriteGuid(mail.Mailbox.To64());
		worldPacket.WriteUInt32(mail.MailID);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_MAIL_RETURN_TO_SENDER)]
	private void HandleMailReturnToSender(MailReturnToSender mail)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_MAIL_RETURN_TO_SENDER);
		worldPacket.WriteGuid(GetSession().GameState.CurrentInteractedWithGO.To64());
		worldPacket.WriteUInt32(mail.MailID);
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			worldPacket.WriteGuid(mail.SenderGUID.To64());
		}
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_MAIL_TAKE_ITEM)]
	private void HandleMailTakeItem(MailTakeItem mail)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_MAIL_TAKE_ITEM);
		worldPacket.WriteGuid(mail.Mailbox.To64());
		worldPacket.WriteUInt32(mail.MailID);
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			worldPacket.WriteUInt32(mail.AttachID);
		}
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_MAIL_TAKE_MONEY)]
	private void HandleMailTakeMoney(MailTakeMoney mail)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_MAIL_TAKE_MONEY);
		worldPacket.WriteGuid(mail.Mailbox.To64());
		worldPacket.WriteUInt32(mail.MailID);
		SendPacketToServer(worldPacket);
	}

	private void BuildSendMail(SendMail mail, List<SendMail.MailAttachment> attachments)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_SEND_MAIL);
		worldPacket.WriteGuid(mail.Mailbox.To64());
		worldPacket.WriteCString(mail.Target);
		worldPacket.WriteCString(mail.Subject);
		worldPacket.WriteCString(mail.Body);
		worldPacket.WriteInt32(mail.StationeryID);
		worldPacket.WriteUInt32(0u);
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			worldPacket.WriteUInt8((byte)attachments.Count);
			foreach (SendMail.MailAttachment attachment in attachments)
			{
				worldPacket.WriteUInt8(attachment.AttachPosition);
				worldPacket.WriteGuid(attachment.ItemGUID.To64());
			}
		}
		else if (attachments.Count > 0)
		{
			worldPacket.WriteGuid(attachments[0].ItemGUID.To64());
		}
		else
		{
			worldPacket.WriteGuid(WowGuid64.Empty);
		}
		worldPacket.WriteUInt32((uint)mail.SendMoney);
		worldPacket.WriteUInt32((uint)mail.Cod);
		worldPacket.WriteUInt64(0uL);
		worldPacket.WriteUInt8(0);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_SEND_MAIL)]
	private void HandleSendMail(SendMail mail)
	{
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180) || mail.Attachments.Count <= 1)
		{
			BuildSendMail(mail, mail.Attachments);
			return;
		}
		mail.SendMoney /= mail.Attachments.Count;
		mail.Cod /= mail.Attachments.Count;
		foreach (SendMail.MailAttachment attachment in mail.Attachments)
		{
			List<SendMail.MailAttachment> list = new List<SendMail.MailAttachment>();
			list.Add(attachment);
			BuildSendMail(mail, list);
			Thread.Sleep(500);
		}
	}

	[PacketHandler(Opcode.CMSG_TIME_SYNC_RESPONSE)]
	private void HandleTimeSyncResponse(TimeSyncResponse response)
	{
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_TIME_SYNC_RESPONSE);
			worldPacket.WriteUInt32(response.SequenceIndex);
			worldPacket.WriteUInt32(response.ClientTime);
			SendPacketToServer(worldPacket);
		}
	}

	[PacketHandler(Opcode.CMSG_AREA_TRIGGER)]
	private void HandleAreaTrigger(AreaTriggerPkt at)
	{
		if (at.Entered)
		{
			GetSession().GameState.LastEnteredAreaTrigger = at.AreaTriggerID;
			WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_AREA_TRIGGER);
			worldPacket.WriteUInt32(at.AreaTriggerID);
			SendPacketToServer(worldPacket);
		}
	}

	[PacketHandler(Opcode.CMSG_SET_SELECTION)]
	private void HandleSetSelection(SetSelection selection)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_SET_SELECTION);
		worldPacket.WriteGuid(selection.TargetGUID.To64());
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_REPOP_REQUEST)]
	private void HandleRepopRequest(RepopRequest repop)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_REPOP_REQUEST);
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			worldPacket.WriteBool(repop.CheckInstance);
		}
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_QUERY_CORPSE_LOCATION_FROM_CLIENT)]
	private void HandleQueryCorpseLocationFromClient(QueryCorpseLocationFromClient query)
	{
		WorldPacket packet = new WorldPacket(Opcode.MSG_CORPSE_QUERY);
		SendPacketToServer(packet);
	}

	[PacketHandler(Opcode.CMSG_RECLAIM_CORPSE)]
	private void HandleReclaimCorpse(ReclaimCorpse corpse)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_RECLAIM_CORPSE);
		worldPacket.WriteGuid(corpse.CorpseGUID.To64());
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_STAND_STATE_CHANGE)]
	private void HandleStandStateChange(StandStateChange state)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_STAND_STATE_CHANGE);
		worldPacket.WriteUInt32(state.StandState);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_OPENING_CINEMATIC)]
	[PacketHandler(Opcode.CMSG_NEXT_CINEMATIC_CAMERA)]
	[PacketHandler(Opcode.CMSG_COMPLETE_CINEMATIC)]
	private void HandleCinematicPacket(ClientCinematicPkt cinematic)
	{
		WorldPacket packet = new WorldPacket(cinematic.GetUniversalOpcode());
		SendPacketToServer(packet);
	}

	[PacketHandler(Opcode.CMSG_FAR_SIGHT)]
	private void HandleFarSight(FarSight sight)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_FAR_SIGHT);
		worldPacket.WriteBool(sight.Enable);
		SendPacketToServer(worldPacket);
		GetSession().GameState.IsInFarSight = sight.Enable;
	}

	[PacketHandler(Opcode.CMSG_MOUNT_SPECIAL_ANIM)]
	private void HandleMountSpecialAnim(MountSpecial mount)
	{
		WorldPacket packet = new WorldPacket(Opcode.CMSG_MOUNT_SPECIAL_ANIM);
		SendPacketToServer(packet);
	}

	[PacketHandler(Opcode.CMSG_TUTORIAL_FLAG)]
	private void HandleTutorialFlag(TutorialSetFlag tutorial)
	{
		switch (tutorial.Action)
		{
		case TutorialAction.Clear:
		{
			WorldPacket packet2 = new WorldPacket(Opcode.CMSG_TUTORIAL_CLEAR);
			SendPacketToServer(packet2);
			break;
		}
		case TutorialAction.Reset:
		{
			WorldPacket packet = new WorldPacket(Opcode.CMSG_TUTORIAL_RESET);
			SendPacketToServer(packet);
			break;
		}
		case TutorialAction.Update:
		{
			WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_TUTORIAL_FLAG);
			worldPacket.WriteUInt32(tutorial.TutorialBit);
			SendPacketToServer(worldPacket);
			break;
		}
		}
	}

	[PacketHandler(Opcode.CMSG_REQUEST_LFG_LIST_BLACKLIST)]
	private void HandleRequestLFGListBlacklist(EmptyClientPacket request)
	{
		LFGListUpdateBlacklist lFGListUpdateBlacklist = new LFGListUpdateBlacklist();
		if (ModernVersion.ExpansionVersion > 1)
		{
			lFGListUpdateBlacklist.AddBlacklist(796, 3);
			lFGListUpdateBlacklist.AddBlacklist(797, 3);
			lFGListUpdateBlacklist.AddBlacklist(798, 3);
			lFGListUpdateBlacklist.AddBlacklist(799, 3);
			lFGListUpdateBlacklist.AddBlacklist(800, 3);
			lFGListUpdateBlacklist.AddBlacklist(801, 3);
			lFGListUpdateBlacklist.AddBlacklist(802, 3);
			lFGListUpdateBlacklist.AddBlacklist(803, 3);
			lFGListUpdateBlacklist.AddBlacklist(804, 3);
			lFGListUpdateBlacklist.AddBlacklist(805, 3);
			lFGListUpdateBlacklist.AddBlacklist(806, 3);
			lFGListUpdateBlacklist.AddBlacklist(807, 3);
			lFGListUpdateBlacklist.AddBlacklist(808, 3);
			lFGListUpdateBlacklist.AddBlacklist(809, 3);
			lFGListUpdateBlacklist.AddBlacklist(810, 3);
			lFGListUpdateBlacklist.AddBlacklist(811, 3);
			lFGListUpdateBlacklist.AddBlacklist(812, 3);
			lFGListUpdateBlacklist.AddBlacklist(813, 3);
			lFGListUpdateBlacklist.AddBlacklist(814, 3);
			lFGListUpdateBlacklist.AddBlacklist(815, 3);
			lFGListUpdateBlacklist.AddBlacklist(816, 3);
			lFGListUpdateBlacklist.AddBlacklist(817, 3);
			lFGListUpdateBlacklist.AddBlacklist(818, 3);
			lFGListUpdateBlacklist.AddBlacklist(820, 3);
			lFGListUpdateBlacklist.AddBlacklist(827, 3);
			lFGListUpdateBlacklist.AddBlacklist(828, 3);
			lFGListUpdateBlacklist.AddBlacklist(829, 3);
			lFGListUpdateBlacklist.AddBlacklist(835, 1031);
			lFGListUpdateBlacklist.AddBlacklist(837, 3);
			lFGListUpdateBlacklist.AddBlacklist(849, 1031);
			lFGListUpdateBlacklist.AddBlacklist(850, 1031);
			lFGListUpdateBlacklist.AddBlacklist(851, 1031);
			lFGListUpdateBlacklist.AddBlacklist(852, 1031);
			lFGListUpdateBlacklist.AddBlacklist(853, 3);
			lFGListUpdateBlacklist.AddBlacklist(854, 3);
			lFGListUpdateBlacklist.AddBlacklist(855, 3);
			lFGListUpdateBlacklist.AddBlacklist(856, 3);
			lFGListUpdateBlacklist.AddBlacklist(857, 3);
			lFGListUpdateBlacklist.AddBlacklist(858, 3);
			lFGListUpdateBlacklist.AddBlacklist(859, 3);
			lFGListUpdateBlacklist.AddBlacklist(860, 3);
			lFGListUpdateBlacklist.AddBlacklist(861, 3);
			lFGListUpdateBlacklist.AddBlacklist(862, 3);
			lFGListUpdateBlacklist.AddBlacklist(863, 3);
			lFGListUpdateBlacklist.AddBlacklist(864, 3);
			lFGListUpdateBlacklist.AddBlacklist(865, 3);
			lFGListUpdateBlacklist.AddBlacklist(866, 3);
			lFGListUpdateBlacklist.AddBlacklist(867, 3);
			lFGListUpdateBlacklist.AddBlacklist(868, 3);
			lFGListUpdateBlacklist.AddBlacklist(869, 3);
			lFGListUpdateBlacklist.AddBlacklist(870, 3);
			lFGListUpdateBlacklist.AddBlacklist(871, 3);
			lFGListUpdateBlacklist.AddBlacklist(872, 3);
			lFGListUpdateBlacklist.AddBlacklist(873, 3);
			lFGListUpdateBlacklist.AddBlacklist(874, 3);
			lFGListUpdateBlacklist.AddBlacklist(875, 3);
			lFGListUpdateBlacklist.AddBlacklist(876, 3);
			lFGListUpdateBlacklist.AddBlacklist(877, 3);
			lFGListUpdateBlacklist.AddBlacklist(878, 3);
			lFGListUpdateBlacklist.AddBlacklist(879, 3);
			lFGListUpdateBlacklist.AddBlacklist(880, 3);
			lFGListUpdateBlacklist.AddBlacklist(881, 3);
			lFGListUpdateBlacklist.AddBlacklist(882, 3);
			lFGListUpdateBlacklist.AddBlacklist(883, 3);
			lFGListUpdateBlacklist.AddBlacklist(884, 3);
			lFGListUpdateBlacklist.AddBlacklist(885, 3);
			lFGListUpdateBlacklist.AddBlacklist(886, 3);
			lFGListUpdateBlacklist.AddBlacklist(887, 3);
			lFGListUpdateBlacklist.AddBlacklist(888, 3);
			lFGListUpdateBlacklist.AddBlacklist(889, 3);
			lFGListUpdateBlacklist.AddBlacklist(890, 3);
			lFGListUpdateBlacklist.AddBlacklist(891, 3);
			lFGListUpdateBlacklist.AddBlacklist(892, 3);
			lFGListUpdateBlacklist.AddBlacklist(893, 3);
			lFGListUpdateBlacklist.AddBlacklist(898, 3);
			lFGListUpdateBlacklist.AddBlacklist(899, 3);
			lFGListUpdateBlacklist.AddBlacklist(900, 3);
			lFGListUpdateBlacklist.AddBlacklist(901, 3);
			lFGListUpdateBlacklist.AddBlacklist(902, 1031);
			lFGListUpdateBlacklist.AddBlacklist(917, 1031);
			lFGListUpdateBlacklist.AddBlacklist(919, 3);
			lFGListUpdateBlacklist.AddBlacklist(920, 3);
			lFGListUpdateBlacklist.AddBlacklist(921, 3);
			lFGListUpdateBlacklist.AddBlacklist(922, 3);
			lFGListUpdateBlacklist.AddBlacklist(923, 3);
			lFGListUpdateBlacklist.AddBlacklist(924, 3);
			lFGListUpdateBlacklist.AddBlacklist(926, 3);
			lFGListUpdateBlacklist.AddBlacklist(927, 3);
			lFGListUpdateBlacklist.AddBlacklist(928, 3);
			lFGListUpdateBlacklist.AddBlacklist(929, 3);
			lFGListUpdateBlacklist.AddBlacklist(930, 3);
			lFGListUpdateBlacklist.AddBlacklist(932, 3);
			lFGListUpdateBlacklist.AddBlacklist(934, 3);
		}
		SendPacket(lFGListUpdateBlacklist);
	}

	[PacketHandler(Opcode.CMSG_REQUEST_CONQUEST_FORMULA_CONSTANTS)]
	private void HandleRequestConquestFormulaConstants(EmptyClientPacket request)
	{
		ConquestFormulaConstants conquestFormulaConstants = new ConquestFormulaConstants();
		conquestFormulaConstants.PvpMinCPPerWeek = 1500;
		conquestFormulaConstants.PvpMaxCPPerWeek = 3000;
		conquestFormulaConstants.PvpCPBaseCoefficient = 1511.26f;
		conquestFormulaConstants.PvpCPExpCoefficient = 1639.28f;
		conquestFormulaConstants.PvpCPNumerator = 0.00412f;
		SendPacket(conquestFormulaConstants);
	}

	[PacketHandler(Opcode.CMSG_OBJECT_UPDATE_FAILED)]
	private void HandleObjectUpdateFailed(ObjectUpdateFailed fail)
	{
		Log.Print(LogType.Error, $"Object update failed for {fail.ObjectGuid}.", "HandleObjectUpdateFailed", "D:\\a\\HermesProxy\\HermesProxy\\World\\Server\\PacketHandlers\\MiscHandler.cs");
	}

	[PacketHandler(Opcode.CMSG_SET_DUNGEON_DIFFICULTY)]
	private void HandleSetDungeonDifficulty(SetDungeonDifficulty difficulty)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.MSG_SET_DUNGEON_DIFFICULTY);
		uint data = (byte)Enum.Parse(typeof(DifficultyLegacy), ((DifficultyModern)difficulty.DifficultyID).ToString());
		worldPacket.WriteUInt32(data);
		SendPacketToServer(worldPacket);
		DungeonDifficultySet dungeonDifficultySet = new DungeonDifficultySet();
		dungeonDifficultySet.DifficultyID = (int)difficulty.DifficultyID;
		SendPacket(dungeonDifficultySet);
	}

	[PacketHandler(Opcode.CMSG_MOVE_CHANGE_TRANSPORT)]
	[PacketHandler(Opcode.CMSG_MOVE_DISMISS_VEHICLE)]
	[PacketHandler(Opcode.CMSG_MOVE_FALL_LAND)]
	[PacketHandler(Opcode.CMSG_MOVE_FALL_RESET)]
	[PacketHandler(Opcode.CMSG_MOVE_HEARTBEAT)]
	[PacketHandler(Opcode.CMSG_MOVE_JUMP)]
	[PacketHandler(Opcode.CMSG_MOVE_REMOVE_MOVEMENT_FORCES)]
	[PacketHandler(Opcode.CMSG_MOVE_SET_FACING)]
	[PacketHandler(Opcode.CMSG_MOVE_SET_FACING_HEARTBEAT)]
	[PacketHandler(Opcode.CMSG_MOVE_SET_FLY)]
	[PacketHandler(Opcode.CMSG_MOVE_SET_PITCH)]
	[PacketHandler(Opcode.CMSG_MOVE_SET_RUN_MODE)]
	[PacketHandler(Opcode.CMSG_MOVE_SET_WALK_MODE)]
	[PacketHandler(Opcode.CMSG_MOVE_START_ASCEND)]
	[PacketHandler(Opcode.CMSG_MOVE_START_BACKWARD)]
	[PacketHandler(Opcode.CMSG_MOVE_START_DESCEND)]
	[PacketHandler(Opcode.CMSG_MOVE_START_FORWARD)]
	[PacketHandler(Opcode.CMSG_MOVE_START_PITCH_DOWN)]
	[PacketHandler(Opcode.CMSG_MOVE_START_PITCH_UP)]
	[PacketHandler(Opcode.CMSG_MOVE_START_SWIM)]
	[PacketHandler(Opcode.CMSG_MOVE_START_TURN_LEFT)]
	[PacketHandler(Opcode.CMSG_MOVE_START_TURN_RIGHT)]
	[PacketHandler(Opcode.CMSG_MOVE_START_STRAFE_LEFT)]
	[PacketHandler(Opcode.CMSG_MOVE_START_STRAFE_RIGHT)]
	[PacketHandler(Opcode.CMSG_MOVE_STOP)]
	[PacketHandler(Opcode.CMSG_MOVE_STOP_ASCEND)]
	[PacketHandler(Opcode.CMSG_MOVE_STOP_PITCH)]
	[PacketHandler(Opcode.CMSG_MOVE_STOP_STRAFE)]
	[PacketHandler(Opcode.CMSG_MOVE_STOP_SWIM)]
	[PacketHandler(Opcode.CMSG_MOVE_STOP_TURN)]
	[PacketHandler(Opcode.CMSG_MOVE_DOUBLE_JUMP)]
	private void HandlePlayerMove(ClientPlayerMovement movement)
	{
		uint opcodeValueForVersion = Opcodes.GetOpcodeValueForVersion(movement.GetUniversalOpcode().ToString().Replace("CMSG", "MSG"), Settings.ServerBuild);
		if (opcodeValueForVersion == 0)
		{
			opcodeValueForVersion = Opcodes.GetOpcodeValueForVersion("MSG_MOVE_SET_FACING", Settings.ServerBuild);
		}
		WorldPacket worldPacket = new WorldPacket(opcodeValueForVersion);
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
		{
			worldPacket.WritePackedGuid(movement.Guid.To64());
		}
		movement.MoveInfo.WriteMovementInfoLegacy(worldPacket);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_MOVE_TELEPORT_ACK)]
	private void HandleMoveTeleportAck(MoveTeleportAck teleport)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.MSG_MOVE_TELEPORT_ACK);
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
		{
			worldPacket.WritePackedGuid(teleport.MoverGUID.To64());
		}
		else
		{
			worldPacket.WriteGuid(teleport.MoverGUID.To64());
		}
		worldPacket.WriteUInt32(teleport.MoveCounter);
		worldPacket.WriteUInt32(teleport.MoveTime);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_WORLD_PORT_RESPONSE)]
	private void HandleWorldPortResponse(WorldPortResponse teleport)
	{
		GetSession().GameState.IsWaitingForWorldPortAck = false;
		WorldPacket packet = new WorldPacket(Opcode.MSG_MOVE_WORLDPORT_ACK);
		SendPacketToServer(packet);
	}

	[PacketHandler(Opcode.CMSG_MOVE_FORCE_FLIGHT_BACK_SPEED_CHANGE_ACK)]
	[PacketHandler(Opcode.CMSG_MOVE_FORCE_FLIGHT_SPEED_CHANGE_ACK)]
	[PacketHandler(Opcode.CMSG_MOVE_FORCE_PITCH_RATE_CHANGE_ACK)]
	[PacketHandler(Opcode.CMSG_MOVE_FORCE_RUN_BACK_SPEED_CHANGE_ACK)]
	[PacketHandler(Opcode.CMSG_MOVE_FORCE_RUN_SPEED_CHANGE_ACK)]
	[PacketHandler(Opcode.CMSG_MOVE_FORCE_SWIM_BACK_SPEED_CHANGE_ACK)]
	[PacketHandler(Opcode.CMSG_MOVE_FORCE_SWIM_SPEED_CHANGE_ACK)]
	[PacketHandler(Opcode.CMSG_MOVE_FORCE_TURN_RATE_CHANGE_ACK)]
	[PacketHandler(Opcode.CMSG_MOVE_FORCE_WALK_SPEED_CHANGE_ACK)]
	private void HandleMoveForceSpeedChangeAck(MovementSpeedAck speed)
	{
		Opcode universalOpcode = speed.GetUniversalOpcode();
		if (!LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180) || (universalOpcode != Opcode.CMSG_MOVE_FORCE_FLIGHT_SPEED_CHANGE_ACK && universalOpcode != Opcode.CMSG_MOVE_FORCE_FLIGHT_BACK_SPEED_CHANGE_ACK))
		{
			WorldPacket worldPacket = new WorldPacket(universalOpcode);
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
			{
				worldPacket.WritePackedGuid(speed.MoverGUID.To64());
			}
			else
			{
				worldPacket.WriteGuid(speed.MoverGUID.To64());
			}
			worldPacket.WriteUInt32(speed.Ack.MoveCounter);
			speed.Ack.MoveInfo.WriteMovementInfoLegacy(worldPacket);
			worldPacket.WriteFloat(speed.Speed);
			SendPacketToServer(worldPacket);
		}
	}

	private MovementFlagModern GetFlagForAckOpcode(Opcode opcode)
	{
		return opcode switch
		{
			Opcode.CMSG_MOVE_FEATHER_FALL_ACK => MovementFlagModern.CanSafeFall, 
			Opcode.CMSG_MOVE_HOVER_ACK => MovementFlagModern.Hover, 
			Opcode.CMSG_MOVE_SET_CAN_FLY_ACK => MovementFlagModern.CanFly, 
			Opcode.CMSG_MOVE_WATER_WALK_ACK => MovementFlagModern.Waterwalking, 
			_ => MovementFlagModern.None, 
		};
	}

	[PacketHandler(Opcode.CMSG_MOVE_FEATHER_FALL_ACK)]
	[PacketHandler(Opcode.CMSG_MOVE_HOVER_ACK)]
	[PacketHandler(Opcode.CMSG_MOVE_SET_CAN_FLY_ACK)]
	[PacketHandler(Opcode.CMSG_MOVE_WATER_WALK_ACK)]
	private void HandleMoveForceAck1(MovementAckMessage movementAck)
	{
		WorldPacket worldPacket = new WorldPacket(movementAck.GetUniversalOpcode());
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
		{
			worldPacket.WritePackedGuid(movementAck.MoverGUID.To64());
		}
		else
		{
			worldPacket.WriteGuid(movementAck.MoverGUID.To64());
		}
		worldPacket.WriteUInt32(movementAck.Ack.MoveCounter);
		movementAck.Ack.MoveInfo.WriteMovementInfoLegacy(worldPacket);
		worldPacket.WriteInt32(movementAck.Ack.MoveInfo.Flags.HasAnyFlag(GetFlagForAckOpcode(movementAck.GetUniversalOpcode())) ? 1 : 0);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_MOVE_FORCE_ROOT_ACK)]
	[PacketHandler(Opcode.CMSG_MOVE_FORCE_UNROOT_ACK)]
	[PacketHandler(Opcode.CMSG_MOVE_KNOCK_BACK_ACK)]
	[PacketHandler(Opcode.CMSG_MOVE_GRAVITY_DISABLE_ACK)]
	[PacketHandler(Opcode.CMSG_MOVE_GRAVITY_ENABLE_ACK)]
	private void HandleMoveForceAck2(MovementAckMessage movementAck)
	{
		WorldPacket worldPacket = new WorldPacket(movementAck.GetUniversalOpcode());
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
		{
			worldPacket.WritePackedGuid(movementAck.MoverGUID.To64());
		}
		else
		{
			worldPacket.WriteGuid(movementAck.MoverGUID.To64());
		}
		worldPacket.WriteUInt32(movementAck.Ack.MoveCounter);
		movementAck.Ack.MoveInfo.WriteMovementInfoLegacy(worldPacket);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_SET_ACTIVE_MOVER)]
	private void HandleMoveSetActiveMover(SetActiveMover move)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_SET_ACTIVE_MOVER);
		worldPacket.WriteGuid(move.MoverGUID.To64());
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_MOVE_INIT_ACTIVE_MOVER_COMPLETE)]
	private void HandleMoveInitActiveMoverComplete(InitActiveMoverComplete move)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_SET_ACTIVE_MOVER);
		worldPacket.WriteGuid(GetSession().GameState.CurrentPlayerGuid.To64());
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_MOVE_SPLINE_DONE)]
	private void HandleMoveSplineDone(MoveSplineDone movement)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_MOVE_SPLINE_DONE);
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
		{
			worldPacket.WritePackedGuid(movement.Guid.To64());
		}
		movement.MoveInfo.WriteMovementInfoLegacy(worldPacket);
		worldPacket.WriteInt32(movement.SplineID);
		if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			worldPacket.WriteFloat(0f);
		}
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_MOVE_TIME_SKIPPED)]
	private void HandleMoveSplineDone(MoveTimeSkipped movement)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_MOVE_TIME_SKIPPED);
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
		{
			worldPacket.WritePackedGuid(movement.MoverGUID.To64());
		}
		else
		{
			worldPacket.WriteGuid(movement.MoverGUID.To64());
		}
		worldPacket.WriteUInt32(movement.TimeSkipped);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_BANKER_ACTIVATE)]
	[PacketHandler(Opcode.CMSG_BINDER_ACTIVATE)]
	[PacketHandler(Opcode.CMSG_LIST_INVENTORY)]
	[PacketHandler(Opcode.CMSG_SPIRIT_HEALER_ACTIVATE)]
	[PacketHandler(Opcode.CMSG_TALK_TO_GOSSIP)]
	[PacketHandler(Opcode.CMSG_TRAINER_LIST)]
	[PacketHandler(Opcode.CMSG_BATTLEMASTER_HELLO)]
	[PacketHandler(Opcode.CMSG_AREA_SPIRIT_HEALER_QUERY)]
	[PacketHandler(Opcode.CMSG_AREA_SPIRIT_HEALER_QUEUE)]
	private void HandleInteractWithNPC(InteractWithNPC interact)
	{
		WorldPacket worldPacket = new WorldPacket(interact.GetUniversalOpcode());
		worldPacket.WriteGuid(interact.CreatureGUID.To64());
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_GOSSIP_SELECT_OPTION)]
	private void HandleGossipSelectOption(GossipSelectOption gossip)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_GOSSIP_SELECT_OPTION);
		worldPacket.WriteGuid(gossip.GossipUnit.To64());
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			worldPacket.WriteUInt32(gossip.GossipID);
		}
		worldPacket.WriteUInt32(gossip.GossipIndex);
		if (!string.IsNullOrEmpty(gossip.PromotionCode))
		{
			worldPacket.WriteCString(gossip.PromotionCode);
		}
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_BUY_BANK_SLOT)]
	private void HandleBuyBankSlot(BuyBankSlot bank)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_BUY_BANK_SLOT);
		worldPacket.WriteGuid(bank.Guid.To64());
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_TRAINER_BUY_SPELL)]
	private void HandleTrainerBuySpell(TrainerBuySpell buy)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_TRAINER_BUY_SPELL);
		worldPacket.WriteGuid(buy.TrainerGUID.To64());
		if (ModernVersion.ExpansionVersion > 1 && LegacyVersion.ExpansionVersion <= 1)
		{
			buy.SpellID = GetSession().GameState.GetLearnSpellFromRealSpell(buy.SpellID);
		}
		worldPacket.WriteUInt32(buy.SpellID);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_CONFIRM_RESPEC_WIPE)]
	private void HandleConfirmRespecWipe(ConfirmRespecWipe respec)
	{
		switch (respec.RespecType)
		{
		case SpecResetType.Talents:
		{
			WorldPacket worldPacket2 = new WorldPacket(Opcode.MSG_TALENT_WIPE_CONFIRM);
			worldPacket2.WriteGuid(respec.TrainerGUID.To64());
			SendPacketToServer(worldPacket2);
			break;
		}
		case SpecResetType.PetTalents:
		{
			WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_PET_UNLEARN);
			worldPacket.WriteGuid(respec.TrainerGUID.To64());
			SendPacketToServer(worldPacket);
			break;
		}
		default:
			Log.Print(LogType.Error, $"Unhandled respec type {respec.RespecType}.", "HandleConfirmRespecWipe", "D:\\a\\HermesProxy\\HermesProxy\\World\\Server\\PacketHandlers\\NPCHandler.cs");
			break;
		}
	}

	[PacketHandler(Opcode.CMSG_PET_ACTION)]
	private void HandlePetAction(PetAction act)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_PET_ACTION);
		worldPacket.WriteGuid(act.PetGUID.To64());
		worldPacket.WriteUInt32(act.Action);
		worldPacket.WriteGuid(act.TargetGUID.To64());
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_PET_STOP_ATTACK)]
	private void HandlePetStopAttack(PetStopAttack stop)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_PET_STOP_ATTACK);
		worldPacket.WriteGuid(stop.PetGUID.To64());
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_PET_SET_ACTION)]
	private void HandlePetStopAttack(PetSetAction action)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_PET_SET_ACTION);
		worldPacket.WriteGuid(action.PetGUID.To64());
		worldPacket.WriteUInt32(action.Index);
		worldPacket.WriteUInt32(action.Action);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_PET_RENAME)]
	private void HandlePetRename(PetRename pet)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_PET_RENAME);
		worldPacket.WriteGuid(pet.RenameData.PetGUID.To64());
		worldPacket.WriteCString(pet.RenameData.NewName);
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			worldPacket.WriteBool(pet.RenameData.HasDeclinedNames);
			if (pet.RenameData.HasDeclinedNames)
			{
				for (int i = 0; i < 5; i++)
				{
					worldPacket.WriteCString(pet.RenameData.DeclinedNames.name[i]);
				}
			}
		}
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_REQUEST_STABLED_PETS)]
	private void HandleRequestStabledPets(RequestStabledPets stable)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.MSG_LIST_STABLED_PETS);
		worldPacket.WriteGuid(stable.StableMaster.To64());
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_BUY_STABLE_SLOT)]
	private void HandleBuyStableSlot(BuyStableSlot stable)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_BUY_STABLE_SLOT);
		worldPacket.WriteGuid(stable.StableMaster.To64());
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_PET_ABANDON)]
	private void HandlePetAbandon(PetAbandon pet)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_PET_ABANDON);
		worldPacket.WriteGuid(pet.PetGUID.To64());
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_STABLE_PET)]
	private void HandleStablePet(StablePet pet)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_STABLE_PET);
		worldPacket.WriteGuid(pet.StableMaster.To64());
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_UNSTABLE_PET)]
	private void HandleUnstablePet(UnstablePet pet)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_UNSTABLE_PET);
		worldPacket.WriteGuid(pet.StableMaster.To64());
		worldPacket.WriteUInt32(pet.PetNumber);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_STABLE_SWAP_PET)]
	private void HandleStableSwapPet(StableSwapPet pet)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_STABLE_SWAP_PET);
		worldPacket.WriteGuid(pet.StableMaster.To64());
		worldPacket.WriteUInt32(pet.PetNumber);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_PET_CANCEL_AURA)]
	private void HandlePetCancelAura(PetCancelAura cancel)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_PET_CANCEL_AURA);
		worldPacket.WriteGuid(cancel.PetGUID.To64());
		worldPacket.WriteUInt32(cancel.SpellID);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_REQUEST_PET_INFO)]
	private void HandleRequestPetInfo(PetInfoRequest r)
	{
		WorldPacket packet = new WorldPacket(Opcode.CMSG_REQUEST_PET_INFO);
		SendPacketToServer(packet);
	}

	[PacketHandler(Opcode.CMSG_PETITION_BUY)]
	private void HandlePetitionBuy(PetitionBuy petition)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_PETITION_BUY);
		worldPacket.WriteGuid(petition.Unit.To64());
		worldPacket.WriteUInt32(0u);
		worldPacket.WriteUInt64(0uL);
		worldPacket.WriteCString(petition.Title);
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			worldPacket.WriteCString("");
		}
		worldPacket.WriteUInt32(0u);
		worldPacket.WriteUInt32(0u);
		worldPacket.WriteUInt32(0u);
		worldPacket.WriteUInt32(0u);
		worldPacket.WriteUInt32(0u);
		worldPacket.WriteUInt32(0u);
		worldPacket.WriteUInt32(0u);
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			worldPacket.WriteUInt16(0);
		}
		worldPacket.WriteUInt32(0u);
		worldPacket.WriteUInt32(0u);
		worldPacket.WriteUInt32(0u);
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			for (int i = 0; i < 10; i++)
			{
				worldPacket.WriteCString("");
			}
		}
		else
		{
			worldPacket.WriteUInt16(0);
			worldPacket.WriteUInt8(0);
		}
		worldPacket.WriteUInt32(petition.Index);
		worldPacket.WriteUInt32(0u);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_PETITION_SHOW_SIGNATURES)]
	private void HandlePetitionShowSignatures(PetitionShowSignatures petition)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_PETITION_SHOW_SIGNATURES);
		worldPacket.WriteGuid(petition.Item.To64());
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_QUERY_PETITION)]
	private void HandleQueryPetition(QueryPetition petition)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_QUERY_PETITION);
		worldPacket.WriteUInt32(petition.PetitionID);
		worldPacket.WriteGuid(petition.ItemGUID.To64());
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_PETITION_RENAME_GUILD)]
	private void HandlePetitionRenameGuild(PetitionRenameGuild petition)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.MSG_PETITION_RENAME);
		worldPacket.WriteGuid(petition.PetitionGuid.To64());
		worldPacket.WriteCString(petition.NewGuildName);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_OFFER_PETITION)]
	private void HandleOfferPetition(OfferPetition petition)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_OFFER_PETITION);
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			worldPacket.WriteUInt32(petition.UnkInt);
		}
		worldPacket.WriteGuid(petition.ItemGUID.To64());
		worldPacket.WriteGuid(petition.TargetPlayer.To64());
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_DECLINE_PETITION)]
	private void HandleDeclinePetition(DeclinePetition petition)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.MSG_PETITION_DECLINE);
		worldPacket.WriteGuid(petition.PetitionGUID.To64());
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_SIGN_PETITION)]
	private void HandleSignPetition(SignPetition petition)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_SIGN_PETITION);
		worldPacket.WriteGuid(petition.PetitionGUID.To64());
		worldPacket.WriteUInt8(petition.Choice);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_TURN_IN_PETITION)]
	private void HandleTurnInPetition(TurnInPetition petition)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_TURN_IN_PETITION);
		worldPacket.WriteGuid(petition.Item.To64());
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			worldPacket.WriteUInt32(petition.BackgroundColor);
			worldPacket.WriteUInt32(petition.EmblemStyle);
			worldPacket.WriteUInt32(petition.EmblemColor);
			worldPacket.WriteUInt32(petition.BorderStyle);
			worldPacket.WriteUInt32(petition.BorderColor);
		}
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_QUERY_TIME)]
	private void HandleQueryTime(EmptyClientPacket queryTime)
	{
		WorldPacket packet = new WorldPacket(Opcode.CMSG_QUERY_TIME);
		SendPacketToServer(packet);
	}

	[PacketHandler(Opcode.CMSG_QUERY_QUEST_INFO)]
	private void HandleQueryQuestInfo(QueryQuestInfo queryQuest)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_QUERY_QUEST_INFO);
		worldPacket.WriteUInt32(queryQuest.QuestID);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_QUERY_CREATURE)]
	private void HandleQueryCreature(QueryCreature queryCreature)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_QUERY_CREATURE);
		worldPacket.WriteUInt32(queryCreature.CreatureID);
		worldPacket.WriteGuid(new WowGuid64(HighGuidTypeLegacy.Creature, queryCreature.CreatureID, 1u));
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_QUERY_GAME_OBJECT)]
	private void HandleQueryGameObject(QueryGameObject queryGo)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_QUERY_GAME_OBJECT);
		worldPacket.WriteUInt32(queryGo.GameObjectID);
		worldPacket.WriteGuid(queryGo.Guid.To64());
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_QUERY_PAGE_TEXT)]
	private void HandleQueryPageText(QueryPageText queryText)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_QUERY_PAGE_TEXT);
		worldPacket.WriteUInt32(queryText.PageTextID);
		worldPacket.WriteGuid(queryText.ItemGUID.To64());
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_QUERY_NPC_TEXT)]
	private void HandleQueryNpcText(QueryNPCText queryText)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_QUERY_NPC_TEXT);
		worldPacket.WriteUInt32(queryText.TextID);
		worldPacket.WriteGuid(queryText.Guid.To64());
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_QUERY_PET_NAME)]
	private void HandleQueryPetName(QueryPetName queryName)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_QUERY_PET_NAME);
		worldPacket.WriteUInt32(queryName.UnitGUID.GetEntry());
		worldPacket.WriteGuid(queryName.UnitGUID.To64());
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_WHO)]
	private void HandleWhoRequest(WhoRequestPkt who)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_WHO);
		worldPacket.WriteInt32(who.Request.MinLevel);
		worldPacket.WriteInt32(who.Request.MaxLevel);
		worldPacket.WriteCString(who.Request.Name);
		worldPacket.WriteCString(who.Request.Guild);
		worldPacket.WriteInt32((int)who.Request.RaceFilter);
		worldPacket.WriteInt32(who.Request.ClassFilter);
		worldPacket.WriteInt32(who.Areas.Count);
		foreach (int area in who.Areas)
		{
			worldPacket.WriteInt32(area);
		}
		worldPacket.WriteInt32(who.Request.Words.Count);
		foreach (string word in who.Request.Words)
		{
			worldPacket.WriteCString(word);
		}
		SendPacketToServer(worldPacket);
		GetSession().GameState.LastWhoRequestId = who.RequestID;
	}

	[PacketHandler(Opcode.CMSG_QUEST_GIVER_QUERY_QUEST)]
	private void HandleQuestGiverQueryQuest(QuestGiverQueryQuest quest)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_QUEST_GIVER_QUERY_QUEST);
		worldPacket.WriteGuid(quest.QuestGiverGUID.To64());
		worldPacket.WriteUInt32(quest.QuestID);
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			worldPacket.WriteBool(quest.RespondToGiver);
		}
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_QUEST_GIVER_ACCEPT_QUEST)]
	private void HandleQuestGiverAcceptQuest(QuestGiverAcceptQuest quest)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_QUEST_GIVER_ACCEPT_QUEST);
		worldPacket.WriteGuid(quest.QuestGiverGUID.To64());
		worldPacket.WriteUInt32(quest.QuestID);
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_2_9901))
		{
			worldPacket.WriteInt32(quest.StartCheat ? 1 : 0);
		}
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_QUEST_LOG_REMOVE_QUEST)]
	private void HandleQuestLogRemoveQuest(QuestLogRemoveQuest quest)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_QUEST_LOG_REMOVE_QUEST);
		worldPacket.WriteUInt8(quest.Slot);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_QUEST_GIVER_STATUS_QUERY)]
	private void HandleQuestGiverStatusQuery(QuestGiverStatusQuery query)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_QUEST_GIVER_STATUS_QUERY);
		worldPacket.WriteGuid(query.QuestGiverGUID.To64());
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_QUEST_GIVER_STATUS_MULTIPLE_QUERY)]
	private void HandleQuestGiverStatusMultipleQuery(QuestGiverStatusMultipleQuery query)
	{
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			WorldPacket packet = new WorldPacket(Opcode.CMSG_QUEST_GIVER_STATUS_MULTIPLE_QUERY);
			SendPacketToServer(packet);
			return;
		}
		int updateField = ModernVersion.GetUpdateField(UnitField.UNIT_NPC_FLAGS);
		if (updateField < 0)
		{
			return;
		}
		List<WowGuid128> list = new List<WowGuid128>();
		GetSession().GameState.ObjectCacheMutex.WaitOne();
		foreach (KeyValuePair<WowGuid128, UpdateFieldsArray> item in GetSession().GameState.ObjectCacheModern)
		{
			if (item.Key.GetObjectType() == ObjectType.Unit && item.Value.GetUpdateField<uint>(updateField, 0).HasAnyFlag(NPCFlags.QuestGiver))
			{
				list.Add(item.Key);
			}
		}
		GetSession().GameState.ObjectCacheMutex.ReleaseMutex();
		foreach (WowGuid128 item2 in list)
		{
			WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_QUEST_GIVER_STATUS_QUERY);
			worldPacket.WriteGuid(item2.To64());
			SendPacketToServer(worldPacket);
		}
	}

	[PacketHandler(Opcode.CMSG_QUEST_GIVER_HELLO)]
	private void HandleQuestGiverHello(QuestGiverHello hello)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_QUEST_GIVER_HELLO);
		worldPacket.WriteGuid(hello.QuestGiverGUID.To64());
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_QUEST_GIVER_REQUEST_REWARD)]
	private void HandleQuestGiverRequestReward(QuestGiverRequestReward quest)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_QUEST_GIVER_REQUEST_REWARD);
		worldPacket.WriteGuid(quest.QuestGiverGUID.To64());
		worldPacket.WriteUInt32(quest.QuestID);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_QUEST_GIVER_CHOOSE_REWARD)]
	private void HandleQuestGiverChooseReward(QuestGiverChooseReward quest)
	{
		int data = 0;
		if (quest.Choice.Item.ItemID != 0)
		{
			QuestTemplate questTemplate = GameData.GetQuestTemplate(quest.QuestID);
			if (questTemplate == null)
			{
				Log.Print(LogType.Error, "Unable to select quest reward because quest template is missing. Try again.", "HandleQuestGiverChooseReward", "D:\\a\\HermesProxy\\HermesProxy\\World\\Server\\PacketHandlers\\QuestHandler.cs");
				WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_QUERY_QUEST_INFO);
				worldPacket.WriteUInt32(quest.QuestID);
				SendPacketToServer(worldPacket);
				QuestGiverQuestFailed questGiverQuestFailed = new QuestGiverQuestFailed();
				questGiverQuestFailed.QuestID = quest.QuestID;
				questGiverQuestFailed.Reason = InventoryResult.ItemNotFound;
				SendPacket(questGiverQuestFailed);
				return;
			}
			for (int i = 0; i < questTemplate.UnfilteredChoiceItems.Length; i++)
			{
				if (questTemplate.UnfilteredChoiceItems[i].ItemID == quest.Choice.Item.ItemID)
				{
					data = i;
					break;
				}
			}
		}
		WorldPacket worldPacket2 = new WorldPacket(Opcode.CMSG_QUEST_GIVER_CHOOSE_REWARD);
		worldPacket2.WriteGuid(quest.QuestGiverGUID.To64());
		worldPacket2.WriteUInt32(quest.QuestID);
		worldPacket2.WriteInt32(data);
		SendPacketToServer(worldPacket2);
	}

	[PacketHandler(Opcode.CMSG_QUEST_GIVER_COMPLETE_QUEST)]
	private void HandleQuestGiverCompleteQuest(QuestGiverCompleteQuest quest)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_QUEST_GIVER_COMPLETE_QUEST);
		worldPacket.WriteGuid(quest.QuestGiverGUID.To64());
		worldPacket.WriteUInt32(quest.QuestID);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_QUEST_CONFIRM_ACCEPT)]
	private void HandleQuestConfirmAcceptResponse(QuestConfirmAcceptResponse quest)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_QUEST_CONFIRM_ACCEPT);
		worldPacket.WriteUInt32(quest.QuestID);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_PUSH_QUEST_TO_PARTY)]
	private void HandlePushQuestToParty(PushQuestToParty quest)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_PUSH_QUEST_TO_PARTY);
		worldPacket.WriteUInt32(quest.QuestID);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_QUEST_PUSH_RESULT)]
	private void HandleQuestPushResult(QuestPushResultResponse quest)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.MSG_QUEST_PUSH_RESULT);
		worldPacket.WriteGuid(quest.SenderGUID.To64());
		worldPacket.WriteUInt8((byte)quest.Result);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_SET_FACTION_AT_WAR)]
	private void HandleSetFactionAtWar(SetFactionAtWar faction)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_SET_FACTION_AT_WAR);
		worldPacket.WriteUInt32(faction.FactionIndex);
		worldPacket.WriteBool(data: true);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_SET_FACTION_NOT_AT_WAR)]
	private void HandleSetFactionNotAtWar(SetFactionNotAtWar faction)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_SET_FACTION_AT_WAR);
		worldPacket.WriteUInt32(faction.FactionIndex);
		worldPacket.WriteBool(data: false);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_SET_FACTION_INACTIVE)]
	private void HandleSetFactionInactive(SetFactionInactive faction)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_SET_FACTION_INACTIVE);
		worldPacket.WriteUInt32(faction.FactionIndex);
		worldPacket.WriteBool(faction.State);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_SET_WATCHED_FACTION)]
	private void HandleSetFactionInactive(SetWatchedFaction faction)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_SET_WATCHED_FACTION);
		worldPacket.WriteUInt32(faction.FactionIndex);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_CHANGE_REALM_TICKET)]
	private void HandleChangeRealmTicket(ChangeRealmTicket request)
	{
		ChangeRealmTicketResponse changeRealmTicketResponse = new ChangeRealmTicketResponse();
		changeRealmTicketResponse.Token = request.Token;
		if (!GetSession().AuthClient.IsConnected() && GetSession().AuthClient.Reconnect() != 0)
		{
			Log.Print(LogType.Error, "Failed to reconnect to auth server.", "HandleChangeRealmTicket", "D:\\a\\HermesProxy\\HermesProxy\\World\\Server\\PacketHandlers\\SessionHandler.cs");
			changeRealmTicketResponse.Allow = false;
			SendPacket(changeRealmTicketResponse);
		}
		else
		{
			_bnetRpc.SetClientSecret(request.Secret);
			changeRealmTicketResponse.Allow = true;
			changeRealmTicketResponse.Ticket = new ByteBuffer(new byte[1]);
			SendPacket(changeRealmTicketResponse);
		}
	}

	[PacketHandler(Opcode.CMSG_BATTLENET_REQUEST)]
	private void HandleBattlenetRequest(BattlenetRequest request)
	{
		if (_bnetRpc == null)
		{
			Log.Print(LogType.Error, $"Client tried {108} without authentication", "HandleBattlenetRequest", "D:\\a\\HermesProxy\\HermesProxy\\World\\Server\\PacketHandlers\\SessionHandler.cs");
		}
		else
		{
			_bnetRpc.Invoke(0u, (OriginalHash)request.Method.GetServiceHash(), request.Method.GetMethodId(), request.Method.Token, new CodedInputStream(request.Data));
		}
	}

	[PacketHandler(Opcode.CMSG_CONTACT_LIST)]
	private void HandleContactList(ContactListRequest contacts)
	{
		if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			WorldPacket packet = new WorldPacket(Opcode.CMSG_FRIEND_LIST);
			SendPacketToServer(packet);
		}
		else
		{
			WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_CONTACT_LIST);
			worldPacket.WriteUInt32((uint)contacts.Flags);
			SendPacketToServer(worldPacket);
		}
	}

	[PacketHandler(Opcode.CMSG_ADD_FRIEND)]
	private void HandleAddFriend(AddFriend friend)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_ADD_FRIEND);
		worldPacket.WriteCString(friend.Name);
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			worldPacket.WriteCString(friend.Note);
		}
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_ADD_IGNORE)]
	private void HandleAddIgnore(AddIgnore ignore)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_ADD_IGNORE);
		worldPacket.WriteCString(ignore.Name);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_DEL_FRIEND)]
	[PacketHandler(Opcode.CMSG_DEL_IGNORE)]
	private void HandleDelFriend(DelFriend friend)
	{
		WorldPacket worldPacket = new WorldPacket(friend.GetUniversalOpcode());
		worldPacket.WriteGuid(friend.Guid.To64());
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_SET_CONTACT_NOTES)]
	private void HandleSetContactNotes(SetContactNotes friend)
	{
		if (!LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_SET_CONTACT_NOTES);
			worldPacket.WriteGuid(friend.Guid.To64());
			worldPacket.WriteCString(friend.Notes);
			SendPacketToServer(worldPacket);
		}
	}

	private SpellCastTargetFlags ConvertSpellTargetFlags(SpellTargetData target)
	{
		SpellCastTargetFlags spellCastTargetFlags = SpellCastTargetFlags.None;
		if (target.Unit != null && !target.Unit.IsEmpty())
		{
			if (target.Flags.HasFlag(SpellCastTargetFlags.Unit))
			{
				spellCastTargetFlags |= SpellCastTargetFlags.Unit;
			}
			if (target.Flags.HasFlag(SpellCastTargetFlags.CorpseEnemy))
			{
				spellCastTargetFlags |= SpellCastTargetFlags.CorpseEnemy;
			}
			if (target.Flags.HasFlag(SpellCastTargetFlags.GameObject))
			{
				spellCastTargetFlags |= SpellCastTargetFlags.GameObject;
			}
			if (target.Flags.HasFlag(SpellCastTargetFlags.CorpseAlly))
			{
				spellCastTargetFlags |= SpellCastTargetFlags.CorpseAlly;
			}
			if (target.Flags.HasFlag(SpellCastTargetFlags.UnitMinipet))
			{
				spellCastTargetFlags |= SpellCastTargetFlags.UnitMinipet;
			}
		}
		if ((target.Item != null) & !target.Item.IsEmpty())
		{
			if (target.Flags.HasFlag(SpellCastTargetFlags.Item))
			{
				spellCastTargetFlags |= SpellCastTargetFlags.Item;
			}
			if (target.Flags.HasFlag(SpellCastTargetFlags.TradeItem))
			{
				spellCastTargetFlags |= SpellCastTargetFlags.TradeItem;
			}
		}
		if (target.SrcLocation != null)
		{
			spellCastTargetFlags |= SpellCastTargetFlags.SourceLocation;
		}
		if (target.DstLocation != null)
		{
			spellCastTargetFlags |= SpellCastTargetFlags.DestLocation;
		}
		if (!string.IsNullOrEmpty(target.Name))
		{
			spellCastTargetFlags |= SpellCastTargetFlags.String;
		}
		return spellCastTargetFlags;
	}

	private void WriteSpellTargets(SpellTargetData target, SpellCastTargetFlags targetFlags, WorldPacket packet)
	{
		if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			packet.WriteUInt16((ushort)targetFlags);
		}
		else
		{
			packet.WriteUInt32((uint)targetFlags);
		}
		if (targetFlags.HasAnyFlag(SpellCastTargetFlags.CorpseMask | SpellCastTargetFlags.Unit | SpellCastTargetFlags.GameObject | SpellCastTargetFlags.UnitMinipet))
		{
			packet.WritePackedGuid(target.Unit.To64());
		}
		if (targetFlags.HasFlag(SpellCastTargetFlags.TradeItem) && target.Item == WowGuid128.Create(HighGuidType703.Uniq, 10uL))
		{
			packet.WritePackedGuid(new WowGuid64(6uL));
		}
		else if (targetFlags.HasFlag(SpellCastTargetFlags.Item))
		{
			packet.WritePackedGuid(target.Item.To64());
		}
		if (targetFlags.HasAnyFlag(SpellCastTargetFlags.SourceLocation))
		{
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
			{
				packet.WritePackedGuid(target.SrcLocation.Transport.To64());
			}
			packet.WriteVector3(target.SrcLocation.Location);
		}
		if (targetFlags.HasAnyFlag(SpellCastTargetFlags.DestLocation))
		{
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_8_9464))
			{
				packet.WritePackedGuid(target.DstLocation.Transport.To64());
			}
			packet.WriteVector3(target.DstLocation.Location);
		}
		if (targetFlags.HasAnyFlag(SpellCastTargetFlags.String))
		{
			packet.WriteCString(target.Name);
		}
	}

	public void SendCastRequestFailed(ClientCastRequest castRequest, bool isPet)
	{
		if (!castRequest.HasStarted)
		{
			SpellPrepare spellPrepare = new SpellPrepare();
			spellPrepare.ClientCastID = castRequest.ClientGUID;
			spellPrepare.ServerCastID = castRequest.ServerGUID;
			SendPacket(spellPrepare);
		}
		if (isPet)
		{
			PetCastFailed petCastFailed = new PetCastFailed();
			petCastFailed.SpellID = castRequest.SpellId;
			petCastFailed.Reason = 123u;
			petCastFailed.CastID = castRequest.ServerGUID;
			SendPacket(petCastFailed);
		}
		else
		{
			CastFailed castFailed = new CastFailed();
			castFailed.SpellID = castRequest.SpellId;
			castFailed.SpellXSpellVisualID = castRequest.SpellXSpellVisualId;
			castFailed.Reason = 123u;
			castFailed.CastID = castRequest.ServerGUID;
			SendPacket(castFailed);
		}
	}

	[PacketHandler(Opcode.CMSG_CAST_SPELL)]
	private void HandleCastSpell(CastSpell cast)
	{
		if (Settings.ServerSpellDelay > 0)
		{
			Thread.Sleep(Settings.ServerSpellDelay);
		}
		if (GameData.NextMeleeSpells.Contains(cast.Cast.SpellID) || GameData.AutoRepeatSpells.Contains(cast.Cast.SpellID))
		{
			ClientCastRequest clientCastRequest = new ClientCastRequest();
			clientCastRequest.Timestamp = Environment.TickCount;
			clientCastRequest.SpellId = cast.Cast.SpellID;
			clientCastRequest.SpellXSpellVisualId = cast.Cast.SpellXSpellVisualID;
			clientCastRequest.ClientGUID = cast.Cast.CastID;
			if (GetSession().GameState.CurrentClientSpecialCast != null)
			{
				clientCastRequest.ServerGUID = WowGuid128.Create(HighGuidType703.Cast, SpellCastSource.Normal, GetSession().GameState.CurrentMapId.Value, cast.Cast.SpellID, 10000 + cast.Cast.CastID.GetCounter());
				SendCastRequestFailed(clientCastRequest, isPet: false);
				return;
			}
			clientCastRequest.ServerGUID = WowGuid128.Create(HighGuidType703.Cast, SpellCastSource.Normal, GetSession().GameState.CurrentMapId.Value, cast.Cast.SpellID, cast.Cast.SpellID + GetSession().GameState.CurrentPlayerGuid.GetCounter());
			SpellPrepare spellPrepare = new SpellPrepare();
			spellPrepare.ClientCastID = cast.Cast.CastID;
			spellPrepare.ServerCastID = clientCastRequest.ServerGUID;
			SendPacket(spellPrepare);
			GetSession().GameState.CurrentClientSpecialCast = clientCastRequest;
		}
		else
		{
			ClientCastRequest clientCastRequest2 = new ClientCastRequest();
			clientCastRequest2.Timestamp = Environment.TickCount;
			clientCastRequest2.SpellId = cast.Cast.SpellID;
			clientCastRequest2.SpellXSpellVisualId = cast.Cast.SpellXSpellVisualID;
			clientCastRequest2.ClientGUID = cast.Cast.CastID;
			clientCastRequest2.ServerGUID = WowGuid128.Create(HighGuidType703.Cast, SpellCastSource.Normal, GetSession().GameState.CurrentMapId.Value, cast.Cast.SpellID, 10000 + cast.Cast.CastID.GetCounter());
			if (GetSession().GameState.CurrentClientNormalCast != null)
			{
				if (GetSession().GameState.CurrentClientNormalCast.HasStarted)
				{
					SendCastRequestFailed(clientCastRequest2, isPet: false);
				}
				else if (GetSession().GameState.CurrentClientNormalCast.Timestamp + 10000 < clientCastRequest2.Timestamp)
				{
					Log.Print(LogType.Warn, $"Clearing CurrentClientNormalCast because of 10 sec timeout! (oldSpell:{GetSession().GameState.CurrentClientNormalCast.SpellId} newSpell:{clientCastRequest2.SpellId})", "HandleCastSpell", "D:\\a\\HermesProxy\\HermesProxy\\World\\Server\\PacketHandlers\\SpellHandler.cs");
					Log.Print(LogType.Warn, "Are you playing on a server with another patch?", "HandleCastSpell", "D:\\a\\HermesProxy\\HermesProxy\\World\\Server\\PacketHandlers\\SpellHandler.cs");
					SendCastRequestFailed(GetSession().GameState.CurrentClientNormalCast, isPet: false);
					GetSession().GameState.CurrentClientNormalCast = null;
					foreach (ClientCastRequest pendingClientCast in GetSession().GameState.PendingClientCasts)
					{
						SendCastRequestFailed(pendingClientCast, isPet: false);
					}
					GetSession().GameState.PendingClientCasts.Clear();
					SendCastRequestFailed(clientCastRequest2, isPet: false);
				}
				else
				{
					GetSession().GameState.PendingClientCasts.Add(clientCastRequest2);
				}
				return;
			}
			GetSession().GameState.CurrentClientNormalCast = clientCastRequest2;
		}
		SpellCastTargetFlags targetFlags = ConvertSpellTargetFlags(cast.Cast.Target);
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_CAST_SPELL);
		if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			worldPacket.WriteUInt32(cast.Cast.SpellID);
		}
		else if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			worldPacket.WriteUInt32(cast.Cast.SpellID);
			worldPacket.WriteUInt8(0);
		}
		else
		{
			worldPacket.WriteUInt8(0);
			worldPacket.WriteUInt32(cast.Cast.SpellID);
			worldPacket.WriteUInt8((byte)cast.Cast.SendCastFlags);
		}
		WriteSpellTargets(cast.Cast.Target, targetFlags, worldPacket);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_PET_CAST_SPELL)]
	private void HandlePetCastSpell(PetCastSpell cast)
	{
		if (Settings.ServerSpellDelay > 0)
		{
			Thread.Sleep(Settings.ServerSpellDelay);
		}
		ClientCastRequest clientCastRequest = new ClientCastRequest();
		clientCastRequest.Timestamp = Environment.TickCount;
		clientCastRequest.SpellId = cast.Cast.SpellID;
		clientCastRequest.SpellXSpellVisualId = cast.Cast.SpellXSpellVisualID;
		clientCastRequest.ClientGUID = cast.Cast.CastID;
		clientCastRequest.ServerGUID = WowGuid128.Create(HighGuidType703.Cast, SpellCastSource.Normal, GetSession().GameState.CurrentMapId.Value, cast.Cast.SpellID, 10000 + cast.Cast.CastID.GetCounter());
		if (GetSession().GameState.CurrentClientPetCast != null)
		{
			if (GetSession().GameState.CurrentClientPetCast.HasStarted)
			{
				SendCastRequestFailed(clientCastRequest, isPet: true);
			}
			else if (GetSession().GameState.CurrentClientPetCast.Timestamp + 10000 < clientCastRequest.Timestamp)
			{
				Log.Print(LogType.Warn, $"Clearing CurrentClientPetCast because of 10 sec timeout! (oldSpell:{GetSession().GameState.CurrentClientPetCast.SpellId} newSpell:{clientCastRequest.SpellId})", "HandlePetCastSpell", "D:\\a\\HermesProxy\\HermesProxy\\World\\Server\\PacketHandlers\\SpellHandler.cs");
				SendCastRequestFailed(GetSession().GameState.CurrentClientPetCast, isPet: true);
				GetSession().GameState.CurrentClientPetCast = null;
				foreach (ClientCastRequest pendingClientPetCast in GetSession().GameState.PendingClientPetCasts)
				{
					SendCastRequestFailed(pendingClientPetCast, isPet: true);
				}
				GetSession().GameState.PendingClientPetCasts.Clear();
				SendCastRequestFailed(clientCastRequest, isPet: true);
			}
			else
			{
				GetSession().GameState.PendingClientPetCasts.Add(clientCastRequest);
			}
		}
		else
		{
			GetSession().GameState.CurrentClientPetCast = clientCastRequest;
			SpellCastTargetFlags targetFlags = ConvertSpellTargetFlags(cast.Cast.Target);
			WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_PET_CAST_SPELL);
			worldPacket.WriteGuid(cast.PetGUID.To64());
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
			{
				worldPacket.WriteUInt8(0);
			}
			worldPacket.WriteUInt32(cast.Cast.SpellID);
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
			{
				worldPacket.WriteUInt8((byte)cast.Cast.SendCastFlags);
			}
			WriteSpellTargets(cast.Cast.Target, targetFlags, worldPacket);
			SendPacketToServer(worldPacket);
		}
	}

	[PacketHandler(Opcode.CMSG_USE_ITEM)]
	private void HandleUseItem(UseItem use)
	{
		if (Settings.ServerSpellDelay > 0)
		{
			Thread.Sleep(Settings.ServerSpellDelay);
		}
		ClientCastRequest clientCastRequest = new ClientCastRequest();
		clientCastRequest.Timestamp = Environment.TickCount;
		clientCastRequest.SpellId = use.Cast.SpellID;
		clientCastRequest.SpellXSpellVisualId = use.Cast.SpellXSpellVisualID;
		clientCastRequest.ClientGUID = use.Cast.CastID;
		clientCastRequest.ServerGUID = WowGuid128.Create(HighGuidType703.Cast, SpellCastSource.Normal, GetSession().GameState.CurrentMapId.Value, use.Cast.SpellID, 10000 + use.Cast.CastID.GetCounter());
		clientCastRequest.ItemGUID = use.CastItem;
		if (GetSession().GameState.CurrentClientNormalCast != null)
		{
			if (GetSession().GameState.CurrentClientNormalCast.HasStarted)
			{
				SendCastRequestFailed(clientCastRequest, isPet: false);
			}
			else if (GetSession().GameState.CurrentClientNormalCast.Timestamp + 10000 < clientCastRequest.Timestamp)
			{
				Log.Print(LogType.Warn, $"Clearing CurrentClientNormalCast because of 10 sec timeout! (oldSpell:{GetSession().GameState.CurrentClientNormalCast.SpellId} newSpell:{clientCastRequest.SpellId})", "HandleUseItem", "D:\\a\\HermesProxy\\HermesProxy\\World\\Server\\PacketHandlers\\SpellHandler.cs");
				SendCastRequestFailed(GetSession().GameState.CurrentClientNormalCast, isPet: false);
				GetSession().GameState.CurrentClientNormalCast = null;
				foreach (ClientCastRequest pendingClientCast in GetSession().GameState.PendingClientCasts)
				{
					SendCastRequestFailed(pendingClientCast, isPet: false);
				}
				GetSession().GameState.PendingClientCasts.Clear();
				SendCastRequestFailed(clientCastRequest, isPet: false);
			}
			else
			{
				GetSession().GameState.PendingClientCasts.Add(clientCastRequest);
			}
		}
		else
		{
			GetSession().GameState.CurrentClientNormalCast = clientCastRequest;
			WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_USE_ITEM);
			byte data = ((use.PackSlot != byte.MaxValue) ? ModernVersion.AdjustInventorySlot(use.PackSlot) : use.PackSlot);
			byte data2 = ((use.PackSlot == byte.MaxValue) ? ModernVersion.AdjustInventorySlot(use.Slot) : use.Slot);
			worldPacket.WriteUInt8(data);
			worldPacket.WriteUInt8(data2);
			worldPacket.WriteUInt8(GetSession().GameState.GetItemSpellSlot(use.CastItem, use.Cast.SpellID));
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
			{
				worldPacket.WriteUInt8(0);
				worldPacket.WriteGuid(use.CastItem.To64());
			}
			SpellCastTargetFlags targetFlags = ConvertSpellTargetFlags(use.Cast.Target);
			WriteSpellTargets(use.Cast.Target, targetFlags, worldPacket);
			SendPacketToServer(worldPacket);
		}
	}

	[PacketHandler(Opcode.CMSG_CANCEL_CAST)]
	private void HandleCancelCast(CancelCast cast)
	{
		if (Settings.ServerSpellDelay > 0)
		{
			Thread.Sleep(Settings.ServerSpellDelay);
		}
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_CANCEL_CAST);
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			worldPacket.WriteUInt8(0);
		}
		worldPacket.WriteUInt32(cast.SpellID);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_CANCEL_CHANNELLING)]
	private void HandleCancelChannelling(CancelChannelling cast)
	{
		if (Settings.ServerSpellDelay > 0)
		{
			Thread.Sleep(Settings.ServerSpellDelay);
		}
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_CANCEL_CHANNELLING);
		worldPacket.WriteInt32(cast.SpellID);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_CANCEL_AUTO_REPEAT_SPELL)]
	private void HandleCancelAutoRepeatSpell(CancelAutoRepeatSpell spell)
	{
		if (Settings.ServerSpellDelay > 0)
		{
			Thread.Sleep(Settings.ServerSpellDelay);
		}
		WorldPacket packet = new WorldPacket(Opcode.CMSG_CANCEL_AUTO_REPEAT_SPELL);
		SendPacketToServer(packet);
	}

	[PacketHandler(Opcode.CMSG_CANCEL_AURA)]
	private void HandleCancelAura(CancelAura aura)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_CANCEL_AURA);
		worldPacket.WriteUInt32(aura.SpellID);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_CANCEL_MOUNT_AURA)]
	private void HandleCancelMountAura(EmptyClientPacket cancel)
	{
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			WorldPacket packet = new WorldPacket(Opcode.CMSG_CANCEL_MOUNT_AURA);
			SendPacketToServer(packet);
			return;
		}
		WowGuid128 currentPlayerGuid = GetSession().GameState.CurrentPlayerGuid;
		Dictionary<int, UpdateField> cachedObjectFieldsLegacy = GetSession().GameState.GetCachedObjectFieldsLegacy(currentPlayerGuid);
		if (cachedObjectFieldsLegacy == null)
		{
			return;
		}
		for (byte b = 0; b < 32; b++)
		{
			AuraDataInfo auraDataInfo = GetSession().WorldClient.ReadAuraSlot(b, currentPlayerGuid, cachedObjectFieldsLegacy);
			if (auraDataInfo != null && GameData.MountAuras.Contains(auraDataInfo.SpellID))
			{
				WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_CANCEL_AURA);
				worldPacket.WriteUInt32(auraDataInfo.SpellID);
				SendPacketToServer(worldPacket);
			}
		}
	}

	[PacketHandler(Opcode.CMSG_LEARN_TALENT)]
	private void HandleLearnTalent(LearnTalent talent)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_LEARN_TALENT);
		worldPacket.WriteUInt32(talent.TalentID);
		worldPacket.WriteUInt32(talent.Rank);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_RESURRECT_RESPONSE)]
	private void HandleResurrectResponse(ResurrectResponse revive)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_RESURRECT_RESPONSE);
		worldPacket.WriteGuid(revive.CasterGUID.To64());
		worldPacket.WriteUInt8((revive.Response == 0) ? ((byte)1) : ((byte)0));
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_SELF_RES)]
	private void HandleSelfRes(SelfRes revive)
	{
		WorldPacket packet = new WorldPacket(Opcode.CMSG_SELF_RES);
		SendPacketToServer(packet);
	}

	[PacketHandler(Opcode.CMSG_TOTEM_DESTROYED)]
	private void HandleTotemDestroyed(TotemDestroyed totem)
	{
		if (!LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_TOTEM_DESTROYED);
			worldPacket.WriteUInt8(totem.Slot);
			SendPacketToServer(worldPacket);
		}
	}

	[PacketHandler(Opcode.CMSG_SUPPORT_TICKET_SUBMIT_COMPLAINT)]
	private void HandleSupportTicketSubmitComplaint(SupportTicketSubmitComplaint complaint)
	{
		string playerName = Session.GameState.GetPlayerName(complaint.TargetCharacterGuid);
		if (string.IsNullOrWhiteSpace(playerName))
		{
			Session.SendHermesTextMessage("Unable to report player because CharacterName was not resolved (can be fixed by restarting the client)", isError: true);
			return;
		}
		string text = "[REPORTED VIA QUICKMENU]\r\nI would like to report player '" + playerName + "'";
		if (!WowGuid128.IsUnknownPlayerGuid(complaint.TargetCharacterGuid))
		{
			text += $"  (id: {complaint.TargetCharacterGuid.GetCounter()})";
		}
		if (complaint.ComplaintType != 0)
		{
			text += $" for {complaint.ComplaintType}";
		}
		if (complaint.SelectedMailInfo != null)
		{
			text = text + "\r\n" + $"Mail in question (id: {complaint.SelectedMailInfo.MailId}) with subject '{complaint.SelectedMailInfo.MailSubject}'";
		}
		if (!complaint.TextNote.IsEmpty())
		{
			text += "\r\n-------------";
			text = text + "\r\n" + complaint.TextNote;
		}
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_GM_TICKET_CREATE);
		if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			worldPacket.WriteUInt8(2);
			worldPacket.WriteUInt32(complaint.Header.SelfPlayerMapId);
			worldPacket.WriteVector3(complaint.Header.SelfPlayerPos);
			worldPacket.WriteCString(text);
			worldPacket.WriteCString("");
		}
		else
		{
			worldPacket.WriteUInt32(complaint.Header.SelfPlayerMapId);
			worldPacket.WriteVector3(complaint.Header.SelfPlayerPos);
			worldPacket.WriteCString(text);
			worldPacket.WriteUInt32(0u);
			worldPacket.WriteUInt32(0u);
			worldPacket.WriteUInt32(0u);
			worldPacket.WriteBytes(Array.Empty<byte>());
		}
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_TAXI_NODE_STATUS_QUERY)]
	[PacketHandler(Opcode.CMSG_TAXI_QUERY_AVAILABLE_NODES)]
	private void HandleTaxiNodesQuery(InteractWithNPC interact)
	{
		WorldPacket worldPacket = new WorldPacket(interact.GetUniversalOpcode());
		worldPacket.WriteGuid(interact.CreatureGUID.To64());
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_ENABLE_TAXI_NODE)]
	private void HandleEnableTaxiNode(InteractWithNPC interact)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_TALK_TO_GOSSIP);
		worldPacket.WriteGuid(interact.CreatureGUID.To64());
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_ACTIVATE_TAXI)]
	private void HandleActivateTaxi(ActivateTaxi taxi)
	{
		if (TaxiPathExist(GetSession().GameState.CurrentTaxiNode, taxi.Node))
		{
			WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_ACTIVATE_TAXI);
			worldPacket.WriteGuid(taxi.FlightMaster.To64());
			worldPacket.WriteUInt32(GetSession().GameState.CurrentTaxiNode);
			worldPacket.WriteUInt32(taxi.Node);
			SendPacketToServer(worldPacket);
		}
		else
		{
			HashSet<uint> taxiPath = GetTaxiPath(GetSession().GameState.CurrentTaxiNode, taxi.Node, GetSession().GameState.UsableTaxiNodes);
			if (taxiPath.Count <= 1)
			{
				return;
			}
			WorldPacket worldPacket2 = new WorldPacket(Opcode.CMSG_ACTIVATE_TAXI_EXPRESS);
			worldPacket2.WriteGuid(taxi.FlightMaster.To64());
			worldPacket2.WriteUInt32(0u);
			worldPacket2.WriteUInt32((uint)taxiPath.Count);
			foreach (uint item in taxiPath)
			{
				worldPacket2.WriteUInt32(item);
			}
			SendPacketToServer(worldPacket2);
		}
		GetSession().GameState.IsWaitingForTaxiStart = true;
	}

	private bool TaxiPathExist(uint from, uint to)
	{
		foreach (KeyValuePair<uint, TaxiPath> taxiPath in GameData.TaxiPaths)
		{
			if ((taxiPath.Value.From == from && taxiPath.Value.To == to) || (taxiPath.Value.From == to && taxiPath.Value.To == from))
			{
				return true;
			}
		}
		return false;
	}

	private bool IsTaxiNodeKnown(uint node, List<byte> usableNodes)
	{
		byte b = (byte)((node - 1) / 8);
		uint num = (uint)(1 << (int)(byte)((node - 1) % 8));
		return (usableNodes[b] & num) == num;
	}

	private HashSet<uint> GetTaxiPath(uint from, uint to, List<byte> usableNodes)
	{
		HashSet<uint> hashSet = new HashSet<uint> { from };
		int[,] array = new int[GameData.TaxiNodesGraph.GetLength(0), GameData.TaxiNodesGraph.GetLength(1)];
		Buffer.BlockCopy(GameData.TaxiNodesGraph, 0, array, 0, GameData.TaxiNodesGraph.Length * 4);
		for (uint num = 1u; num < array.GetLength(0); num++)
		{
			if (!IsTaxiNodeKnown(num, usableNodes))
			{
				for (uint num2 = 0u; num2 < array.GetLength(1); num2++)
				{
					array[num, num2] = 0;
				}
				for (uint num3 = 0u; num3 < array.GetLength(0); num3++)
				{
					array[num3, num] = 0;
				}
			}
		}
		Dijkstra(array, (int)from, (int)to, array.GetLength(0), hashSet);
		return hashSet;
	}

	private int MinDistance(int[] dist, bool[] sptSet, int vCnt)
	{
		int num = int.MaxValue;
		int result = -1;
		for (int i = 0; i < vCnt; i++)
		{
			if (!sptSet[i] && dist[i] <= num)
			{
				num = dist[i];
				result = i;
			}
		}
		return result;
	}

	private void SavePath(int[] parent, int j, HashSet<uint> nodes)
	{
		if (parent[j] != -1)
		{
			SavePath(parent, parent[j], nodes);
			nodes.Add((uint)j);
		}
	}

	private int Dijkstra(int[,] graph, int src, int dest, int vCnt, HashSet<uint> nodes)
	{
		int[] array = new int[vCnt];
		int[] array2 = new int[vCnt];
		bool[] array3 = new bool[vCnt];
		for (int i = 0; i < vCnt; i++)
		{
			array[i] = int.MaxValue;
			array3[i] = false;
			array2[i] = -1;
		}
		array[src] = 0;
		for (int j = 0; j < vCnt - 1; j++)
		{
			int num = MinDistance(array, array3, vCnt);
			array3[num] = true;
			for (int k = 0; k < vCnt; k++)
			{
				if (!array3[k] && graph[num, k] != 0 && array[num] != int.MaxValue && array[num] + graph[num, k] < array[k])
				{
					array2[k] = num;
					array[k] = array[num] + graph[num, k];
				}
			}
		}
		SavePath(array2, dest, nodes);
		return array[dest];
	}

	[PacketHandler(Opcode.CMSG_INITIATE_TRADE)]
	private void HandleInitiateTrade(InitiateTrade trade)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_INITIATE_TRADE);
		worldPacket.WriteGuid(trade.Guid.To64());
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_SET_TRADE_GOLD)]
	private void HandleSetTradeGold(SetTradeGold trade)
	{
		TradeSession currentTrade = GetSession().GameState.CurrentTrade;
		if (currentTrade == null)
		{
			Log.Print(LogType.Error, $"Got {trade.GetUniversalOpcode()} without trade session", "HandleSetTradeGold", "D:\\a\\HermesProxy\\HermesProxy\\World\\Server\\PacketHandlers\\TradeHandler.cs");
		}
		else
		{
			currentTrade.ClientStateIndex++;
			WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_SET_TRADE_GOLD);
			worldPacket.WriteInt32((int)trade.Coinage);
			SendPacketToServer(worldPacket);
		}
	}

	[PacketHandler(Opcode.CMSG_ACCEPT_TRADE)]
	private void HandleAcceptTrade(AcceptTrade trade)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_ACCEPT_TRADE);
		worldPacket.WriteUInt32(trade.StateIndex);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.CMSG_BEGIN_TRADE)]
	[PacketHandler(Opcode.CMSG_BUSY_TRADE)]
	[PacketHandler(Opcode.CMSG_CANCEL_TRADE)]
	[PacketHandler(Opcode.CMSG_UNACCEPT_TRADE)]
	[PacketHandler(Opcode.CMSG_IGNORE_TRADE)]
	private void HandleEmptyTradePacket(EmptyClientPacket trade)
	{
		WorldPacket packet = new WorldPacket(trade.GetUniversalOpcode());
		SendPacketToServer(packet);
	}

	[PacketHandler(Opcode.CMSG_CLEAR_TRADE_ITEM)]
	private void HandleClearTradeItem(ClearTradeItem trade)
	{
		TradeSession currentTrade = GetSession().GameState.CurrentTrade;
		if (currentTrade == null)
		{
			Log.Print(LogType.Error, $"Got {trade.GetUniversalOpcode()} without trade session", "HandleClearTradeItem", "D:\\a\\HermesProxy\\HermesProxy\\World\\Server\\PacketHandlers\\TradeHandler.cs");
		}
		else
		{
			currentTrade.ClientStateIndex++;
			WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_CLEAR_TRADE_ITEM);
			worldPacket.WriteUInt8(trade.TradeSlot);
			SendPacketToServer(worldPacket);
		}
	}

	[PacketHandler(Opcode.CMSG_SET_TRADE_ITEM)]
	private void HandleSetTradeItem(SetTradeItem trade)
	{
		TradeSession currentTrade = GetSession().GameState.CurrentTrade;
		if (currentTrade == null)
		{
			Log.Print(LogType.Error, $"Got {trade.GetUniversalOpcode()} without trade session", "HandleSetTradeItem", "D:\\a\\HermesProxy\\HermesProxy\\World\\Server\\PacketHandlers\\TradeHandler.cs");
			return;
		}
		currentTrade.ClientStateIndex++;
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_SET_TRADE_ITEM);
		worldPacket.WriteUInt8(trade.TradeSlot);
		byte data = ((trade.PackSlot != byte.MaxValue) ? ModernVersion.AdjustInventorySlot(trade.PackSlot) : trade.PackSlot);
		byte data2 = ((trade.PackSlot == byte.MaxValue) ? ModernVersion.AdjustInventorySlot(trade.ItemSlotInPack) : trade.ItemSlotInPack);
		worldPacket.WriteUInt8(data);
		worldPacket.WriteUInt8(data2);
		SendPacketToServer(worldPacket);
	}

	public WorldSocket(Socket socket)
		: base(socket)
	{
		_connectType = ConnectionType.Realm;
		_serverChallenge = Array.Empty<byte>().GenerateRandomKey(16);
		_worldCrypt = new WorldCrypt();
		_encryptKey = new byte[16];
		_headerBuffer = new SocketBuffer(HeaderSize);
		_packetBuffer = new SocketBuffer();
		InitializePacketHandlers();
	}

	public override void Dispose()
	{
		_serverChallenge = null;
		_sessionKey = null;
		_compressionStream = null;
		base.Dispose();
	}

	public GlobalSessionData GetSession()
	{
		return _globalSession;
	}

	public override void Accept()
	{
		GetRemoteIpAddress().ToString();
		_packetBuffer.Resize(ClientConnectionInitialize.Length + 1);
		AsyncReadWithCallback(InitializeHandler);
		ByteBuffer byteBuffer = new ByteBuffer();
		byteBuffer.WriteString(ServerConnectionInitialize);
		byteBuffer.WriteString("\n");
		AsyncWrite(byteBuffer.GetData());
	}

	private void InitializeHandler(SocketAsyncEventArgs args)
	{
		if (args.SocketError != 0)
		{
			CloseSocket();
		}
		else
		{
			if (args.BytesTransferred <= 0 || _packetBuffer.GetRemainingSpace() <= 0)
			{
				return;
			}
			int size = Math.Min(args.BytesTransferred, _packetBuffer.GetRemainingSpace());
			_packetBuffer.Write(args.Buffer, 0, size);
			if (_packetBuffer.GetRemainingSpace() > 0)
			{
				AsyncReadWithCallback(InitializeHandler);
				return;
			}
			ByteBuffer byteBuffer = new ByteBuffer(_packetBuffer.GetData());
			if (byteBuffer.ReadString((uint)ClientConnectionInitialize.Length) != ClientConnectionInitialize)
			{
				CloseSocket();
				return;
			}
			if (byteBuffer.ReadUInt8() != 10)
			{
				CloseSocket();
				return;
			}
			_compressionStream = new ZLib.z_stream();
			int num = ZLib.deflateInit2(_compressionStream, 1, 8, -15, 8, 0);
			if (num != 0)
			{
				CloseSocket();
				Log.Print(LogType.Error, $"Can't initialize packet compression (zlib: deflateInit2_) Error code: {num}", "InitializeHandler", "D:\\a\\HermesProxy\\HermesProxy\\World\\Server\\WorldSocket.cs");
			}
			else
			{
				_packetBuffer.Resize(0);
				_packetBuffer.Reset();
				HandleSendAuthSession();
				AsyncRead();
			}
		}
	}

	public override void ReadHandler(SocketAsyncEventArgs args)
	{
		if (!IsOpen())
		{
			return;
		}
		int num = 0;
		while (num < args.BytesTransferred)
		{
			if (_headerBuffer.GetRemainingSpace() > 0)
			{
				int num2 = Math.Min(args.BytesTransferred - num, _headerBuffer.GetRemainingSpace());
				_headerBuffer.Write(args.Buffer, num, num2);
				num += num2;
				if (_headerBuffer.GetRemainingSpace() > 0)
				{
					break;
				}
				if (!ReadHeader())
				{
					CloseSocket();
					return;
				}
			}
			if (_packetBuffer.GetRemainingSpace() > 0)
			{
				int num3 = Math.Min(args.BytesTransferred - num, _packetBuffer.GetRemainingSpace());
				_packetBuffer.Write(args.Buffer, num, num3);
				num += num3;
				if (_packetBuffer.GetRemainingSpace() > 0)
				{
					break;
				}
			}
			ReadDataHandlerResult readDataHandlerResult = ReadData();
			_headerBuffer.Reset();
			switch (readDataHandlerResult)
			{
			case ReadDataHandlerResult.WaitingForQuery:
				return;
			case ReadDataHandlerResult.Ok:
				continue;
			}
			CloseSocket();
			return;
		}
		AsyncRead();
	}

	private bool ReadHeader()
	{
		PacketHeader packetHeader = new PacketHeader();
		packetHeader.Read(_headerBuffer.GetData());
		_packetBuffer.Resize(packetHeader.Size);
		return true;
	}

    //读取数据处理结果
    private ReadDataHandlerResult ReadData()
	{
		PacketHeader packetHeader = new PacketHeader();
		packetHeader.Read(_headerBuffer.GetData());
		if (!_worldCrypt.Decrypt(_packetBuffer.GetData(), packetHeader.Tag))
		{
			Log.Print(LogType.Error, $"WorldSocket.ReadData(): client {GetRemoteIpAddress()} failed to decrypt packet (size: {packetHeader.Size})", "ReadData", "D:\\a\\HermesProxy\\HermesProxy\\World\\Server\\WorldSocket.cs");
			return ReadDataHandlerResult.Error;
		}
		WorldPacket worldPacket = new WorldPacket(_packetBuffer.GetData());
		_packetBuffer.Reset();
		Opcode universalOpcode = worldPacket.GetUniversalOpcode(isModern: true);
		Log.PrintNet(LogType.Debug, LogNetDir.C2P, $"收到操作码 {universalOpcode.ToString()} ({worldPacket.GetOpcode()}).", "ReadData", "D:\\a\\HermesProxy\\HermesProxy\\World\\Server\\WorldSocket.cs");
		if (universalOpcode != Opcode.CMSG_HOTFIX_REQUEST && !packetHeader.IsValidSize())
		{
			Log.Print(LogType.Error, $"WorldSocket.ReadHeaderHandler(): client {GetRemoteIpAddress()} sent malformed packet (size: {packetHeader.Size})", "ReadData", "D:\\a\\HermesProxy\\HermesProxy\\World\\Server\\WorldSocket.cs");
			return ReadDataHandlerResult.Error;
		}
		switch (universalOpcode)
		{
		case Opcode.CMSG_PING:
		{
			Ping ping = new Ping(worldPacket);
			ping.Read();
			if (_connectType == ConnectionType.Realm && GetSession().WorldClient != null && GetSession().WorldClient.IsConnected() && GetSession().WorldClient.IsAuthenticated())
			{
				GetSession().WorldClient.SendPing(ping.Serial, ping.Latency);
			}
			else
			{
				HandlePing(ping);
			}
			break;
		}
		case Opcode.CMSG_AUTH_SESSION:
		{
			AuthSession authSession = new AuthSession(worldPacket);
			authSession.Read();
			HandleAuthSession(authSession);
			return ReadDataHandlerResult.WaitingForQuery;
		}
		case Opcode.CMSG_AUTH_CONTINUED_SESSION:
		{
			AuthContinuedSession authContinuedSession = new AuthContinuedSession(worldPacket);
			authContinuedSession.Read();
			HandleAuthContinuedSession(authContinuedSession);
			return ReadDataHandlerResult.WaitingForQuery;
		}
		case Opcode.CMSG_LOG_DISCONNECT:
		{
			uint num = worldPacket.ReadUInt32();
			Log.Print(LogType.Server, $"Client disconnected with reason {num}.", "ReadData", "D:\\a\\HermesProxy\\HermesProxy\\World\\Server\\WorldSocket.cs");
			if (_connectType == ConnectionType.Realm)
			{
				if (GetSession().AuthClient != null)
				{
					GetSession().AuthClient.Disconnect();
				}
				if (GetSession().WorldClient != null)
				{
					GetSession().WorldClient.Disconnect();
				}
			}
			if (GetSession().ModernSniff != null)
			{
				GetSession().ModernSniff.CloseFile();
				GetSession().ModernSniff = null;
			}
			break;
		}
		case Opcode.CMSG_ENABLE_NAGLE:
			SetNoDelay(enable: false);
			break;
		case Opcode.CMSG_CONNECT_TO_FAILED:
		{
			ConnectToFailed connectToFailed = new ConnectToFailed(worldPacket);
			connectToFailed.Read();
			HandleConnectToFailed(connectToFailed);
			break;
		}
		case Opcode.CMSG_ENTER_ENCRYPTED_MODE_ACK:
			HandleEnterEncryptedModeAck();
			break;
		case Opcode.CMSG_SERVER_TIME_OFFSET_REQUEST:
			SendServerTimeOffset();
			break;
		default:
			HandlePacket(worldPacket);
			break;
		case Opcode.CMSG_KEEP_ALIVE:
			break;
		}
		return ReadDataHandlerResult.Ok;
	}

	public void HandlePacket(WorldPacket packet)
	{
		Opcode universalOpcode = packet.GetUniversalOpcode(isModern: true);
		PacketHandler handler = GetHandler(universalOpcode);
		if (handler != null)
		{
			if (universalOpcode != Opcode.CMSG_DB_QUERY_BULK)
			{
                Console.WriteLine("操作码: " + universalOpcode);
			}
			handler.Invoke(this, packet);
			return;
		}
		Log.PrintNet(LogType.Warn, LogNetDir.C2P, $"没有操作码的处理程序 {universalOpcode} ({packet.GetOpcode()}) (Got unknown packet from ModernClient)", "HandlePacket", "D:\\a\\HermesProxy\\HermesProxy\\World\\Server\\WorldSocket.cs");
	}

	// 发送数据包到服务器
	private void SendPacketToServer(WorldPacket packet, Opcode delayUntilOpcode = Opcode.MSG_NULL_ACTION)
	{
		if (GetSession().WorldClient != null)
		{
			GetSession().WorldClient.SendPacketToServer(packet, delayUntilOpcode);
			return;
		}
		Log.Print(LogType.Error, $"尝试发送操作码 {packet.GetUniversalOpcode(isModern: false)} ({packet.GetOpcode()}) 当客户端断开连接时!", "SendPacketToServer", "D:\\a\\HermesProxy\\HermesProxy\\World\\Server\\WorldSocket.cs");
	}

	public PacketHandler GetHandler(Opcode opcode)
	{
		return _clientPacketTable.LookupByKey(opcode);
	}

	// 发送数据包
	public void SendPacket(ServerPacket packet)
	{
		if (!IsOpen())
		{
			Log.PrintNet(LogType.Error, LogNetDir.P2C, $"无法发送 {packet.GetUniversalOpcode()}, 套接字已关闭!", "SendPacket", "D:\\a\\HermesProxy\\HermesProxy\\World\\Server\\WorldSocket.cs");
			if (GetSession() != null)
			{
				if (GetSession().RealmSocket == this)
				{
					GetSession().RealmSocket = null;
				}
				else if (GetSession().InstanceSocket == this)
				{
					GetSession().InstanceSocket = null;
				}
				GetSession().OnDisconnect();
			}
			return;
		}
		packet.WritePacketData();
		if (GetSession() != null)
		{
			packet.LogPacket(ref GetSession().ModernSniff);
		}
		_sendMutex.WaitOne();
		byte[] data = packet.GetData();
		Opcode universalOpcode = packet.GetUniversalOpcode();
		ushort num = (ushort)packet.GetOpcode();
		Log.PrintNet(LogType.Debug, LogNetDir.P2C, $"发送操作码 {universalOpcode} ({num}).", "SendPacket", "D:\\a\\HermesProxy\\HermesProxy\\World\\Server\\WorldSocket.cs");
		ByteBuffer byteBuffer = new ByteBuffer();
		int num2 = data.Length;
		if (num2 > 1024 && _worldCrypt.IsInitialized)
		{
			byteBuffer.WriteInt32(num2 + 2);
			byteBuffer.WriteUInt32(ZLib.adler32(ZLib.adler32(2552748273u, BitConverter.GetBytes(num), 2u), data, (uint)num2));
			byte[] outData;
			uint num3 = CompressPacket(data, num, out outData);
			byteBuffer.WriteUInt32(ZLib.adler32(2552748273u, outData, num3));
			byteBuffer.WriteBytes(outData, num3);
			num2 = (int)(num3 + 12);
			num = (ushort)ModernVersion.GetCurrentOpcode(Opcode.SMSG_COMPRESSED_PACKET);
			data = byteBuffer.GetData();
		}
		byteBuffer = new ByteBuffer();
		byteBuffer.WriteUInt16(num);
		byteBuffer.WriteBytes(data);
		num2 += 2;
		data = byteBuffer.GetData();
		PacketHeader packetHeader = new PacketHeader();
		packetHeader.Size = num2;
		_worldCrypt.Encrypt(ref data, ref packetHeader.Tag);
		ByteBuffer byteBuffer2 = new ByteBuffer();
		packetHeader.Write(byteBuffer2);
		byteBuffer2.WriteBytes(data);
		AsyncWrite(byteBuffer2.GetData());
		_sendMutex.ReleaseMutex();
	}

	public uint CompressPacket(byte[] data, ushort opcode, out byte[] outData)
	{
		byte[] array = BitConverter.GetBytes(opcode).Combine(data);
		uint num = ZLib.deflateBound(_compressionStream, (uint)data.Length);
		outData = new byte[num];
		_compressionStream.next_out = 0;
		_compressionStream.avail_out = num;
		_compressionStream.out_buf = outData;
		_compressionStream.next_in = 0u;
		_compressionStream.avail_in = (uint)array.Length;
		_compressionStream.in_buf = array;
		int num2 = ZLib.deflate(_compressionStream, 2);
		if (num2 != 0)
		{
			Log.PrintNet(LogType.Error, LogNetDir.P2C, $"无法压缩数据包数据 (zlib: deflate) 错误代码: {num2} msg: {_compressionStream.msg}", "CompressPacket", "D:\\a\\HermesProxy\\HermesProxy\\World\\Server\\WorldSocket.cs");
			return 0u;
		}
		return num - _compressionStream.avail_out;
	}

	public override bool Update()
	{
		if (!base.Update())
		{
			return false;
		}
		return true;
	}

	public override void OnClose()
	{
		base.OnClose();
	}

	private void HandleSendAuthSession()
	{
		AuthChallenge authChallenge = new AuthChallenge();
		authChallenge.Challenge = _serverChallenge;
		authChallenge.DosChallenge = new byte[32].GenerateRandomKey(32);
		authChallenge.DosZeroBits = 1;
		SendPacket(authChallenge);
	}

	// 处理身份验证session
	private void HandleAuthSession(AuthSession authSession)
	{
		_globalSession = BnetSessionTicketStorage.SessionsByName[authSession.RealmJoinTicket];
		_bnetRpc = new BnetServices.ServiceManager("WorldSocket", this, _globalSession);
		HandleAuthSessionCallback(authSession);
	}

	// 处理身份验证回调函数
	private void HandleAuthSessionCallback(AuthSession authSession)
	{
		RealmBuildInfo buildInfo = GetSession().RealmManager.GetBuildInfo(GetSession().Build);
		if (buildInfo == null)
		{
			SendAuthResponseError(BattlenetRpcErrorCode.BadVersion);
			Log.Print(LogType.Error, $"WorldSocket.HandleAuthSessionCallback: Missing auth seed for realm build {GetSession().Build} ({GetRemoteIpAddress()}).", "HandleAuthSessionCallback", "D:\\a\\HermesProxy\\HermesProxy\\World\\Server\\WorldSocket.cs");
			CloseSocket();
			GetSession().OnDisconnect();
			return;
		}
		IPEndPoint remoteIpAddress = GetRemoteIpAddress();
		if (GetSession().OS != "Wn64" && GetSession().OS != "Mc64" && GetSession().OS != "MacA")
		{
			Log.Print(LogType.Error, $"WorldSocket.HandleAuthSession: Unknown OS for account: {GetSession().GameAccountInfo.Id} ('{authSession.RealmJoinTicket}') address: {remoteIpAddress}", "HandleAuthSessionCallback", "D:\\a\\HermesProxy\\HermesProxy\\World\\Server\\WorldSocket.cs");
			CloseSocket();
			GetSession().OnDisconnect();
			return;
		}
		byte[] valueOrDefault = buildInfo.BuildSeeds.GetValueOrDefault(GetSession().OS);
		if (valueOrDefault == null || !TrySeed(valueOrDefault))
		{
			Log.Print(LogType.Debug, "WorldSocket.HandleAuthSession: Fallback to static seed", "HandleAuthSessionCallback", "D:\\a\\HermesProxy\\HermesProxy\\World\\Server\\WorldSocket.cs");
			if (!TrySeed(buildInfo.FallbackStaticSeed))
			{
				Log.Print(LogType.Error, $"WorldSocket.HandleAuthSession: Authentication failed for account: {GetSession().GameAccountInfo.Id} ('{authSession.RealmJoinTicket}') address: {remoteIpAddress}", "HandleAuthSessionCallback", "D:\\a\\HermesProxy\\HermesProxy\\World\\Server\\WorldSocket.cs");
				CloseSocket();
				GetSession().OnDisconnect();
				return;
			}
		}
		Sha256 sha = new Sha256();
		sha.Finish(GetSession().SessionKey);
		HmacSha256 hmacSha = new HmacSha256(sha.Digest);
		hmacSha.Process(_serverChallenge, 16);
		hmacSha.Process(authSession.LocalChallenge, authSession.LocalChallenge.Count);
		hmacSha.Finish(SessionKeySeed, 16);
		_sessionKey = new byte[40];
		new SessionKeyGenerator(hmacSha.Digest, 32).Generate(_sessionKey, 40u);
		HmacSha256 hmacSha2 = new HmacSha256(_sessionKey);
		hmacSha2.Process(authSession.LocalChallenge, authSession.LocalChallenge.Count);
		hmacSha2.Process(_serverChallenge, 16);
		hmacSha2.Finish(EncryptionKeySeed, 16);
		Buffer.BlockCopy(hmacSha2.Digest, 0, _encryptKey, 0, 16);
		GetSession().SessionKey = _sessionKey;
		Log.Print(LogType.Server, $"WorldSocket:HandleAuthSession: Client '{authSession.RealmJoinTicket}' authenticated successfully from {remoteIpAddress}.", "HandleAuthSessionCallback", "D:\\a\\HermesProxy\\HermesProxy\\World\\Server\\WorldSocket.cs");
		_realmId = new RealmId((byte)authSession.RegionID, (byte)authSession.BattlegroupID, authSession.RealmID);
		GetSession().WorldClient = new WorldClient();
		if (!GetSession().WorldClient.ConnectToWorldServer(GetSession().RealmManager.GetRealm(_realmId), GetSession()))
		{
			SendAuthResponseError(BattlenetRpcErrorCode.BadServer);
			Log.Print(LogType.Error, "客户端不能连接到服务器!", "HandleAuthSessionCallback", "D:\\a\\HermesProxy\\HermesProxy\\World\\Server\\WorldSocket.cs");
			Session.AccountMetaDataMgr.InvalidateLastSelectedCharacter();
			CloseSocket();
			GetSession().OnDisconnect();
		}
		else
		{
			SendPacket(new EnterEncryptedMode(_encryptKey, enabled: true));
			AsyncRead();
		}
		bool TrySeed(byte[] seed)
		{
			Sha256 sha2 = new Sha256();
			sha2.Process(GetSession().SessionKey, GetSession().SessionKey.Length);
			sha2.Finish(seed);
			HmacSha256 hmacSha3 = new HmacSha256(sha2.Digest);
			hmacSha3.Process(authSession.LocalChallenge, authSession.LocalChallenge.Count);
			hmacSha3.Process(_serverChallenge, 16);
			hmacSha3.Finish(AuthCheckSeed, 16);
			return hmacSha3.Digest.Compare(authSession.Digest);
		}
	}

	private void HandleAuthContinuedSession(AuthContinuedSession authSession)
	{
		ConnectToKey connectToKey = default(ConnectToKey);
		ulong key2 = (connectToKey.Raw = authSession.Key);
		_key = key2;
		_connectType = connectToKey.connectionType;
		if (_connectType != ConnectionType.Instance)
		{
			SendAuthResponseError(BattlenetRpcErrorCode.Denied);
			CloseSocket();
		}
		else
		{
			HandleAuthContinuedSessionCallback(authSession);
		}
	}

	private void HandleAuthContinuedSessionCallback(AuthContinuedSession authSession)
	{
		ConnectToKey connectToKey = default(ConnectToKey);
		ulong key2 = (connectToKey.Raw = authSession.Key);
		_key = key2;
		_globalSession = BnetSessionTicketStorage.SessionsByKey[_key];
		uint accountId = connectToKey.AccountId;
		string login = GetSession().AccountInfo.Login;
		_sessionKey = GetSession().SessionKey;
		HmacSha256 hmacSha = new HmacSha256(_sessionKey);
		hmacSha.Process(BitConverter.GetBytes(authSession.Key), 8);
		hmacSha.Process(authSession.LocalChallenge, authSession.LocalChallenge.Length);
		hmacSha.Process(_serverChallenge, 16);
		hmacSha.Finish(ContinuedSessionSeed, 16);
		if (!hmacSha.Digest.Compare(authSession.Digest))
		{
			Log.Print(LogType.Error, $"WorldSocket.HandleAuthContinuedSession: Authentication failed for account: {accountId} ('{login}') address: {GetRemoteIpAddress()}", "HandleAuthContinuedSessionCallback", "D:\\a\\HermesProxy\\HermesProxy\\World\\Server\\WorldSocket.cs");
			CloseSocket();
		}
		else
		{
			HmacSha256 hmacSha2 = new HmacSha256(_sessionKey);
			hmacSha2.Process(authSession.LocalChallenge, authSession.LocalChallenge.Length);
			hmacSha2.Process(_serverChallenge, 16);
			hmacSha2.Finish(EncryptionKeySeed, 16);
			Buffer.BlockCopy(hmacSha2.Digest, 0, _encryptKey, 0, 16);
			SendPacket(new EnterEncryptedMode(_encryptKey, enabled: true));
			AsyncRead();
		}
	}

	public void SendConnectToInstance(ConnectToSerial serial)
	{
		IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse(Settings.ExternalAddress), Settings.InstancePort);
		_instanceConnectKey.AccountId = GetSession().AccountInfo.Id;
		_instanceConnectKey.connectionType = ConnectionType.Instance;
		_instanceConnectKey.Key = RandomHelper.URand(0, int.MaxValue);
		BnetSessionTicketStorage.AddNewSessionByKey(_instanceConnectKey.Raw, GetSession());
		ConnectTo connectTo = new ConnectTo();
		connectTo.Key = _instanceConnectKey.Raw;
		connectTo.Serial = serial;
		connectTo.Payload.Port = (ushort)Settings.InstancePort;
		connectTo.Con = 1;
		if (iPEndPoint.AddressFamily == AddressFamily.InterNetwork)
		{
			connectTo.Payload.Where.IPv4 = iPEndPoint.Address.GetAddressBytes();
			connectTo.Payload.Where.Type = ConnectTo.AddressType.IPv4;
		}
		else
		{
			connectTo.Payload.Where.IPv6 = iPEndPoint.Address.GetAddressBytes();
			connectTo.Payload.Where.Type = ConnectTo.AddressType.IPv6;
		}
		SendPacket(connectTo);
	}

	public void AbortLogin(LoginFailureReason reason)
	{
		SendPacket(new CharacterLoginFailed(reason));
	}

	private void HandleConnectToFailed(ConnectToFailed connectToFailed)
	{
		switch (connectToFailed.Serial)
		{
		case ConnectToSerial.WorldAttempt1:
			SendConnectToInstance(ConnectToSerial.WorldAttempt2);
			break;
		case ConnectToSerial.WorldAttempt2:
			SendConnectToInstance(ConnectToSerial.WorldAttempt3);
			break;
		case ConnectToSerial.WorldAttempt3:
			SendConnectToInstance(ConnectToSerial.WorldAttempt4);
			break;
		case ConnectToSerial.WorldAttempt4:
			SendConnectToInstance(ConnectToSerial.WorldAttempt5);
			break;
		case ConnectToSerial.WorldAttempt5:
			Log.Print(LogType.Error, "Failed to connect 5 times to world socket, aborting login", "HandleConnectToFailed", "D:\\a\\HermesProxy\\HermesProxy\\World\\Server\\WorldSocket.cs");
			AbortLogin(LoginFailureReason.NoWorld);
			break;
		}
	}

	private void HandleEnterEncryptedModeAck()
	{
		_worldCrypt.Initialize(_encryptKey);
		if (_connectType == ConnectionType.Realm)
		{
			SendAuthResponse(BattlenetRpcErrorCode.Ok, GetSession().WorldClient.GetQueuePosition());
			SendSetTimeZoneInformation();
			SendFeatureSystemStatusGlueScreen();
			SendClientCacheVersion(0u);
			SendAvailableHotfixes();
			SendBnetConnectionState(1);
			GetSession().AccountDataMgr = new AccountDataManager(GetSession().Username, GetSession().RealmManager.GetRealm(_realmId).Name);
			GetSession().RealmSocket = this;
		}
		else
		{
			Log.Print(LogType.Server, "Client has connected to the instance server.", "HandleEnterEncryptedModeAck", "D:\\a\\HermesProxy\\HermesProxy\\World\\Server\\WorldSocket.cs");
			SendPacket(new ResumeComms(ConnectionType.Instance));
			GetSession().InstanceSocket = this;
		}
	}

	public void SendAuthResponseError(BattlenetRpcErrorCode code)
	{
		AuthResponse authResponse = new AuthResponse();
		authResponse.SuccessInfo = null;
		authResponse.WaitInfo = null;
		authResponse.Result = code;
		SendPacket(authResponse);
	}

	public void SendAuthResponse(BattlenetRpcErrorCode code, uint queuePos = 0u)
	{
		AuthResponse authResponse = new AuthResponse();
		authResponse.Result = code;
		if (code == BattlenetRpcErrorCode.Ok)
		{
			authResponse.SuccessInfo = new AuthResponse.AuthSuccessInfo();
			authResponse.SuccessInfo.ActiveExpansionLevel = (byte)(LegacyVersion.ExpansionVersion - 1);
			authResponse.SuccessInfo.AccountExpansionLevel = 0;
			authResponse.SuccessInfo.VirtualRealmAddress = _realmId.GetAddress();
			authResponse.SuccessInfo.Time = (uint)Time.UnixTime;
			Realm realm = GetSession().RealmManager.GetRealm(_realmId);
			authResponse.SuccessInfo.VirtualRealms.Add(new VirtualRealmInfo(realm.Id.GetAddress(), isHomeRealm: true, isInternalRealm: false, realm.Name, realm.NormalizedName));
			List<AuthResponse.RaceClassAvailability> list = new List<AuthResponse.RaceClassAvailability>();
			AuthResponse.RaceClassAvailability raceClassAvailability = new AuthResponse.RaceClassAvailability();
			raceClassAvailability.RaceID = 1;
			raceClassAvailability.Classes.Add(new AuthResponse.ClassAvailability(1, 0, 0));
			raceClassAvailability.Classes.Add(new AuthResponse.ClassAvailability(2, 0, 0));
			raceClassAvailability.Classes.Add(new AuthResponse.ClassAvailability(4, 0, 0));
			raceClassAvailability.Classes.Add(new AuthResponse.ClassAvailability(5, 0, 0));
			raceClassAvailability.Classes.Add(new AuthResponse.ClassAvailability(8, 0, 0));
			raceClassAvailability.Classes.Add(new AuthResponse.ClassAvailability(9, 0, 0));
			list.Add(raceClassAvailability);
			raceClassAvailability = new AuthResponse.RaceClassAvailability();
			raceClassAvailability.RaceID = 2;
			raceClassAvailability.Classes.Add(new AuthResponse.ClassAvailability(1, 0, 0));
			raceClassAvailability.Classes.Add(new AuthResponse.ClassAvailability(3, 0, 0));
			raceClassAvailability.Classes.Add(new AuthResponse.ClassAvailability(4, 0, 0));
			raceClassAvailability.Classes.Add(new AuthResponse.ClassAvailability(7, 0, 0));
			raceClassAvailability.Classes.Add(new AuthResponse.ClassAvailability(9, 0, 0));
			list.Add(raceClassAvailability);
			raceClassAvailability = new AuthResponse.RaceClassAvailability();
			raceClassAvailability.RaceID = 3;
			raceClassAvailability.Classes.Add(new AuthResponse.ClassAvailability(1, 0, 0));
			raceClassAvailability.Classes.Add(new AuthResponse.ClassAvailability(2, 0, 0));
			raceClassAvailability.Classes.Add(new AuthResponse.ClassAvailability(3, 0, 0));
			raceClassAvailability.Classes.Add(new AuthResponse.ClassAvailability(5, 0, 0));
			raceClassAvailability.Classes.Add(new AuthResponse.ClassAvailability(4, 0, 0));
			list.Add(raceClassAvailability);
			raceClassAvailability = new AuthResponse.RaceClassAvailability();
			raceClassAvailability.RaceID = 4;
			raceClassAvailability.Classes.Add(new AuthResponse.ClassAvailability(1, 0, 0));
			raceClassAvailability.Classes.Add(new AuthResponse.ClassAvailability(3, 0, 0));
			raceClassAvailability.Classes.Add(new AuthResponse.ClassAvailability(4, 0, 0));
			raceClassAvailability.Classes.Add(new AuthResponse.ClassAvailability(5, 0, 0));
			raceClassAvailability.Classes.Add(new AuthResponse.ClassAvailability(11, 0, 0));
			list.Add(raceClassAvailability);
			raceClassAvailability = new AuthResponse.RaceClassAvailability();
			raceClassAvailability.RaceID = 5;
			raceClassAvailability.Classes.Add(new AuthResponse.ClassAvailability(1, 0, 0));
			raceClassAvailability.Classes.Add(new AuthResponse.ClassAvailability(4, 0, 0));
			raceClassAvailability.Classes.Add(new AuthResponse.ClassAvailability(5, 0, 0));
			raceClassAvailability.Classes.Add(new AuthResponse.ClassAvailability(8, 0, 0));
			raceClassAvailability.Classes.Add(new AuthResponse.ClassAvailability(9, 0, 0));
			list.Add(raceClassAvailability);
			raceClassAvailability = new AuthResponse.RaceClassAvailability();
			raceClassAvailability.RaceID = 6;
			raceClassAvailability.Classes.Add(new AuthResponse.ClassAvailability(1, 0, 0));
			raceClassAvailability.Classes.Add(new AuthResponse.ClassAvailability(3, 0, 0));
			raceClassAvailability.Classes.Add(new AuthResponse.ClassAvailability(7, 0, 0));
			raceClassAvailability.Classes.Add(new AuthResponse.ClassAvailability(11, 0, 0));
			list.Add(raceClassAvailability);
			raceClassAvailability = new AuthResponse.RaceClassAvailability();
			raceClassAvailability.RaceID = 7;
			raceClassAvailability.Classes.Add(new AuthResponse.ClassAvailability(1, 0, 0));
			raceClassAvailability.Classes.Add(new AuthResponse.ClassAvailability(4, 0, 0));
			raceClassAvailability.Classes.Add(new AuthResponse.ClassAvailability(8, 0, 0));
			raceClassAvailability.Classes.Add(new AuthResponse.ClassAvailability(9, 0, 0));
			list.Add(raceClassAvailability);
			raceClassAvailability = new AuthResponse.RaceClassAvailability();
			raceClassAvailability.RaceID = 8;
			raceClassAvailability.Classes.Add(new AuthResponse.ClassAvailability(1, 0, 0));
			raceClassAvailability.Classes.Add(new AuthResponse.ClassAvailability(4, 0, 0));
			raceClassAvailability.Classes.Add(new AuthResponse.ClassAvailability(3, 0, 0));
			raceClassAvailability.Classes.Add(new AuthResponse.ClassAvailability(5, 0, 0));
			raceClassAvailability.Classes.Add(new AuthResponse.ClassAvailability(7, 0, 0));
			raceClassAvailability.Classes.Add(new AuthResponse.ClassAvailability(8, 0, 0));
			list.Add(raceClassAvailability);
			if (ModernVersion.ExpansionVersion >= 2 && LegacyVersion.ExpansionVersion >= 2)
			{
				raceClassAvailability = new AuthResponse.RaceClassAvailability();
				raceClassAvailability.RaceID = 10;
				raceClassAvailability.Classes.Add(new AuthResponse.ClassAvailability(3, 0, 0));
				raceClassAvailability.Classes.Add(new AuthResponse.ClassAvailability(4, 0, 0));
				raceClassAvailability.Classes.Add(new AuthResponse.ClassAvailability(5, 0, 0));
				raceClassAvailability.Classes.Add(new AuthResponse.ClassAvailability(8, 0, 0));
				raceClassAvailability.Classes.Add(new AuthResponse.ClassAvailability(9, 0, 0));
				raceClassAvailability.Classes.Add(new AuthResponse.ClassAvailability(2, 0, 0));
				list.Add(raceClassAvailability);
				raceClassAvailability = new AuthResponse.RaceClassAvailability();
				raceClassAvailability.RaceID = 11;
				raceClassAvailability.Classes.Add(new AuthResponse.ClassAvailability(1, 0, 0));
				raceClassAvailability.Classes.Add(new AuthResponse.ClassAvailability(2, 0, 0));
				raceClassAvailability.Classes.Add(new AuthResponse.ClassAvailability(3, 0, 0));
				raceClassAvailability.Classes.Add(new AuthResponse.ClassAvailability(5, 0, 0));
				raceClassAvailability.Classes.Add(new AuthResponse.ClassAvailability(8, 0, 0));
				raceClassAvailability.Classes.Add(new AuthResponse.ClassAvailability(7, 0, 0));
				list.Add(raceClassAvailability);
			}
			authResponse.SuccessInfo.AvailableClasses = list;
		}
		if (queuePos != 0)
		{
			authResponse.WaitInfo = new AuthWaitInfo();
			authResponse.WaitInfo.WaitCount = queuePos;
		}
		SendPacket(authResponse);
	}

	public void SendAuthWaitQue(uint position)
	{
		if (position != 0)
		{
			WaitQueueUpdate waitQueueUpdate = new WaitQueueUpdate();
			waitQueueUpdate.WaitInfo.WaitCount = position;
			waitQueueUpdate.WaitInfo.WaitTime = 0u;
			waitQueueUpdate.WaitInfo.HasFCM = false;
			SendPacket(waitQueueUpdate);
		}
		else
		{
			SendPacket(new WaitQueueFinish());
		}
	}

	public void SendSetTimeZoneInformation()
	{
		SetTimeZoneInformation setTimeZoneInformation = new SetTimeZoneInformation();
		setTimeZoneInformation.ServerTimeTZ = "Europe/Paris";
		setTimeZoneInformation.GameTimeTZ = "Europe/Paris";
		SendPacket(setTimeZoneInformation);
	}

	public void SendFeatureSystemStatusGlueScreen()
	{
		FeatureSystemStatusGlueScreen featureSystemStatusGlueScreen = new FeatureSystemStatusGlueScreen();
		featureSystemStatusGlueScreen.BpayStoreAvailable = false;
		featureSystemStatusGlueScreen.BpayStoreDisabledByParentalControls = false;
		featureSystemStatusGlueScreen.CharUndeleteEnabled = false;
		featureSystemStatusGlueScreen.BpayStoreEnabled = false;
		featureSystemStatusGlueScreen.MaxCharactersPerRealm = 10;
		featureSystemStatusGlueScreen.MinimumExpansionLevel = 5;
		featureSystemStatusGlueScreen.MaximumExpansionLevel = 8;
		featureSystemStatusGlueScreen.Unk14 = true;
		EuropaTicketConfig europaTicketConfig = new EuropaTicketConfig();
		europaTicketConfig.ThrottleState.MaxTries = 10u;
		europaTicketConfig.ThrottleState.PerMilliseconds = 60000u;
		europaTicketConfig.ThrottleState.TryCount = 1u;
		europaTicketConfig.ThrottleState.LastResetTimeBeforeNow = 111111u;
		europaTicketConfig.TicketsEnabled = true;
		europaTicketConfig.BugsEnabled = true;
		europaTicketConfig.ComplaintsEnabled = true;
		europaTicketConfig.SuggestionsEnabled = true;
		featureSystemStatusGlueScreen.EuropaTicketSystemStatus = europaTicketConfig;
		SendPacket(featureSystemStatusGlueScreen);
	}

	public void SendFeatureSystemStatus()
	{
		FeatureSystemStatus featureSystemStatus = new FeatureSystemStatus();
		featureSystemStatus.ComplaintStatus = 2;
		featureSystemStatus.ScrollOfResurrectionRequestsRemaining = 1u;
		featureSystemStatus.ScrollOfResurrectionMaxRequestsPerDay = 1u;
		featureSystemStatus.CfgRealmID = 1u;
		featureSystemStatus.CfgRealmRecID = 1;
		featureSystemStatus.TwitterPostThrottleLimit = 60u;
		featureSystemStatus.TwitterPostThrottleCooldown = 20u;
		featureSystemStatus.TokenPollTimeSeconds = 300u;
		featureSystemStatus.KioskSessionMinutes = 30u;
		featureSystemStatus.BpayStoreProductDeliveryDelay = 180u;
		featureSystemStatus.HiddenUIClubsPresenceUpdateTimer = 60000u;
		featureSystemStatus.VoiceEnabled = false;
		featureSystemStatus.BrowserEnabled = false;
		featureSystemStatus.EuropaTicketSystemStatus = new EuropaTicketConfig();
		featureSystemStatus.EuropaTicketSystemStatus.ThrottleState.MaxTries = 10u;
		featureSystemStatus.EuropaTicketSystemStatus.ThrottleState.PerMilliseconds = 60000u;
		featureSystemStatus.EuropaTicketSystemStatus.ThrottleState.TryCount = 1u;
		featureSystemStatus.EuropaTicketSystemStatus.ThrottleState.LastResetTimeBeforeNow = 111111u;
		featureSystemStatus.TutorialsEnabled = true;
		featureSystemStatus.Unk67 = true;
		featureSystemStatus.QuestSessionEnabled = true;
		featureSystemStatus.BattlegroundsEnabled = true;
		featureSystemStatus.QuickJoinConfig.ToastDuration = 7f;
		featureSystemStatus.QuickJoinConfig.DelayDuration = 10f;
		featureSystemStatus.QuickJoinConfig.QueueMultiplier = 1f;
		featureSystemStatus.QuickJoinConfig.PlayerMultiplier = 1f;
		featureSystemStatus.QuickJoinConfig.PlayerFriendValue = 5f;
		featureSystemStatus.QuickJoinConfig.PlayerGuildValue = 1f;
		featureSystemStatus.QuickJoinConfig.ThrottleDecayTime = 60f;
		featureSystemStatus.QuickJoinConfig.ThrottlePrioritySpike = 20f;
		featureSystemStatus.QuickJoinConfig.ThrottlePvPPriorityNormal = 50f;
		featureSystemStatus.QuickJoinConfig.ThrottlePvPPriorityLow = 1f;
		featureSystemStatus.QuickJoinConfig.ThrottlePvPHonorThreshold = 10f;
		featureSystemStatus.QuickJoinConfig.ThrottleLfgListPriorityDefault = 50f;
		featureSystemStatus.QuickJoinConfig.ThrottleLfgListPriorityAbove = 100f;
		featureSystemStatus.QuickJoinConfig.ThrottleLfgListPriorityBelow = 50f;
		featureSystemStatus.QuickJoinConfig.ThrottleLfgListIlvlScalingAbove = 1f;
		featureSystemStatus.QuickJoinConfig.ThrottleLfgListIlvlScalingBelow = 1f;
		featureSystemStatus.QuickJoinConfig.ThrottleRfPriorityAbove = 100f;
		featureSystemStatus.QuickJoinConfig.ThrottleRfIlvlScalingAbove = 1f;
		featureSystemStatus.QuickJoinConfig.ThrottleDfMaxItemLevel = 850f;
		featureSystemStatus.QuickJoinConfig.ThrottleDfBestPriority = 80f;
		featureSystemStatus.Squelch.IsSquelched = false;
		featureSystemStatus.Squelch.BnetAccountGuid = WowGuid128.Create(HighGuidType703.BNetAccount, GetSession().AccountInfo.Id);
		featureSystemStatus.Squelch.GuildGuid = WowGuid128.Empty;
		featureSystemStatus.EuropaTicketSystemStatus.TicketsEnabled = true;
		featureSystemStatus.EuropaTicketSystemStatus.BugsEnabled = true;
		featureSystemStatus.EuropaTicketSystemStatus.ComplaintsEnabled = true;
		featureSystemStatus.EuropaTicketSystemStatus.SuggestionsEnabled = true;
		featureSystemStatus.EuropaTicketSystemStatus.ThrottleState.MaxTries = 10u;
		featureSystemStatus.EuropaTicketSystemStatus.ThrottleState.PerMilliseconds = 60000u;
		featureSystemStatus.EuropaTicketSystemStatus.ThrottleState.TryCount = 1u;
		featureSystemStatus.EuropaTicketSystemStatus.ThrottleState.LastResetTimeBeforeNow = 10627480u;
		SendPacket(featureSystemStatus);
	}

	public void SendSeasonInfo()
	{
		SeasonInfo seasonInfo = new SeasonInfo();
		if (LegacyVersion.ExpansionVersion > 1 && ModernVersion.ExpansionVersion > 1)
		{
			seasonInfo.CurrentSeason = 2;
			seasonInfo.PreviousSeason = 1;
		}
		SendPacket(seasonInfo);
	}

	public void SendMotd()
	{
		MOTD packet = new MOTD();
		SendPacket(packet);
	}

	public void SendClientCacheVersion(uint version)
	{
		ClientCacheVersion clientCacheVersion = new ClientCacheVersion();
		clientCacheVersion.CacheVersion = version;
		SendPacket(clientCacheVersion);
	}

	public void SendAvailableHotfixes()
	{
		AvailableHotfixes availableHotfixes = new AvailableHotfixes();
		availableHotfixes.VirtualRealmAddress = GetSession().RealmId.GetAddress();
		SendPacket(availableHotfixes);
	}

	public void SendBnetConnectionState(byte state)
	{
		ConnectionStatus connectionStatus = new ConnectionStatus();
		connectionStatus.State = state;
		SendPacket(connectionStatus);
	}

	public void SendServerTimeOffset()
	{
		ServerTimeOffset serverTimeOffset = new ServerTimeOffset();
		serverTimeOffset.Time = Time.UnixTime;
		SendPacket(serverTimeOffset);
	}

	private void HandlePing(Ping ping)
	{
		SendPacket(new Pong(ping.Serial));
	}

	public void SendAccountDataTimes()
	{
		WowGuid128 currentPlayerGuid = GetSession().GameState.CurrentPlayerGuid;
		GetSession().AccountDataMgr.LoadAllData(currentPlayerGuid);
		AccountDataTimes accountDataTimes = new AccountDataTimes();
		accountDataTimes.PlayerGuid = currentPlayerGuid;
		accountDataTimes.ServerTime = Time.UnixTime;
		int accountDataCount = ModernVersion.GetAccountDataCount();
		accountDataTimes.AccountTimes = new long[accountDataCount];
		for (int i = 0; i < accountDataCount; i++)
		{
			accountDataTimes.AccountTimes[i] = ((GetSession().AccountDataMgr.Data[i] != null) ? GetSession().AccountDataMgr.Data[i].Timestamp : 0);
		}
		SendPacket(accountDataTimes);
	}

	public void SendRpcMessage(uint serviceId, OriginalHash service, uint methodId, uint token, BattlenetRpcErrorCode status, IMessage? message)
	{
		MethodCall method = default(MethodCall);
		method.SetServiceHash((uint)service);
		method.SetMethodId(methodId);
		method.Token = token;
		method.ObjectId = serviceId;
		byte[] data = ((message == null) ? Array.Empty<byte>() : message.ToByteArray());
		BattlenetResponse packet = new BattlenetResponse
		{
			Method = method,
			Status = status,
			Data = new ByteBuffer(data)
		};
		SendPacket(packet);
	}

	public IPEndPoint GetRemoteIpEndPoint()
	{
		return GetRemoteIpAddress();
	}

	public void InitializePacketHandlers()
	{
		MethodInfo[] methods = typeof(WorldSocket).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic);
		foreach (MethodInfo methodInfo in methods)
		{
			foreach (PacketHandlerAttribute customAttribute in methodInfo.GetCustomAttributes<PacketHandlerAttribute>())
			{
				if (customAttribute == null || customAttribute.Opcode == Opcode.MSG_NULL_ACTION)
				{
					continue;
				}
				if (_clientPacketTable.ContainsKey(customAttribute.Opcode))
				{
					Log.Print(LogType.Error, $"Tried to override OpcodeHandler of {_clientPacketTable[customAttribute.Opcode].ToString()} with {methodInfo.Name} (Opcode {customAttribute.Opcode})", "InitializePacketHandlers", "D:\\a\\HermesProxy\\HermesProxy\\World\\Server\\WorldSocket.cs");
				}
				else
				{
					ParameterInfo[] parameters = methodInfo.GetParameters();
					if (parameters.Length == 0)
					{
						Log.Print(LogType.Error, "Method: " + methodInfo.Name + " Has no paramters", "InitializePacketHandlers", "D:\\a\\HermesProxy\\HermesProxy\\World\\Server\\WorldSocket.cs");
					}
					else if (parameters[0].ParameterType.BaseType != typeof(ClientPacket))
					{
						Log.Print(LogType.Error, "Method: " + methodInfo.Name + " has wrong BaseType", "InitializePacketHandlers", "D:\\a\\HermesProxy\\HermesProxy\\World\\Server\\WorldSocket.cs");
					}
					else
					{
						_clientPacketTable[customAttribute.Opcode] = new PacketHandler(methodInfo, parameters[0].ParameterType);
					}
				}
			}
		}
	}
}
