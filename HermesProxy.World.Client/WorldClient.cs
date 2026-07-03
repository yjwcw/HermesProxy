using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Framework;
using Framework.Constants;
using Framework.Cryptography;
using Framework.GameMath;
using Framework.IO;
using Framework.Logging;
using Framework.Networking;
using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Enums.Classic;
using HermesProxy.World.Enums.TBC;
using HermesProxy.World.Enums.Vanilla;
using HermesProxy.World.Enums.WotLK;
using HermesProxy.World.Objects;
using HermesProxy.World.Server;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Client;

/*
 * 魔兽世界客户端
 */
public class WorldClient
{
	private struct Sha1Helper
	{
		private readonly byte[] _part1;

		private byte[] _part2;

		private readonly byte[] _part3;

		private int _alreadyTaken;

		public Sha1Helper(byte[] sk)
		{
			_alreadyTaken = 0;
			byte[] array = sk.Take(20).ToArray();
			_part1 = Framework.Cryptography.HashAlgorithm.SHA1.Hash(array);
			_part2 = new byte[20];
			byte[] array2 = sk.Skip(20).ToArray();
			_part3 = Framework.Cryptography.HashAlgorithm.SHA1.Hash(array2);
			CalcHash();
		}

		public byte[] GetHash()
		{
			byte[] array = new byte[16];
			for (int i = 0; i < 16; i++)
			{
				array[i] = GetOneByte();
			}
			return array;
		}

		private byte GetOneByte()
		{
			if (_alreadyTaken >= _part2.Length)
			{
				CalcHash();
			}
			byte result = _part2[_alreadyTaken];
			_alreadyTaken++;
			return result;
		}

		private void CalcHash()
		{
			byte[] part = Framework.Cryptography.HashAlgorithm.SHA1.Hash(_part1, _part2, _part3);
			_part2 = part;
			_alreadyTaken = 0;
		}
	}

	private Sha1 _hashOut;

	private Sha1 _hashIn;

	private byte[] _wmh;

	private bool _allAddonsAllowed;

	private uint _requestBgPlayerPosCounter;

	private Socket _clientSocket;

	private bool? _isSuccessful;

	private uint _queuePosition;

	private string _username;

	private Realm _realm;

	private LegacyWorldCrypt _worldCrypt;

	private Dictionary<Opcode, Action<WorldPacket>> _packetHandlers;

	private GlobalSessionData _globalSession;

	private Mutex _sendMutex = new Mutex();

	private Dictionary<Opcode, List<WorldPacket>> _delayedPacketsToServer;

	private Dictionary<Opcode, List<ServerPacket>> _delayedPacketsToClient;

	public GlobalSessionData Session => _globalSession;

	private void HandleGenericVersion(ByteBuffer packet)
	{
		byte[] wmh = packet.ReadBytes(16u);
		_wmh = wmh;
		packet.ReadBytes(16u);
		packet.ReadUInt32();
		ByteBuffer byteBuffer = new ByteBuffer();
		byteBuffer.WriteUInt8(1);
		SendPacketToServer(byteBuffer);
	}

	private void HandleTbcVersion(ByteBuffer packet)
	{
		byte count = packet.ReadUInt8();
		byte[] array = packet.ReadBytes(count);
		if (_wmh != null && _allAddonsAllowed)
		{
			byte[] array2 = new byte[4] { 206, 250, 237, 254 };
			byte[] data = Framework.Cryptography.HashAlgorithm.SHA1.Hash(array, array2);
			byte[] data2 = MD5.Create().ComputeHash(array);
			ByteBuffer byteBuffer = new ByteBuffer();
			byteBuffer.WriteUInt8(2);
			byteBuffer.WriteBytes(data);
			byteBuffer.WriteBytes(data2);
			SendPacketToServer(byteBuffer);
		}
	}

	private void HandleFutureVersion(ByteBuffer packet)
	{
		CSV.BinaryFix binaryFix = CSV.KnownFixes.FirstOrDefault((CSV.BinaryFix x) => x.Expected.SequenceEqual(_wmh));
		if (binaryFix.Fixes != null)
		{
			byte[] fixValue = packet.ReadBytes(16u);
			CSV.BinaryFix.Fix fix = binaryFix.Fixes.FirstOrDefault((CSV.BinaryFix.Fix x) => x.Actual.SequenceEqual(fixValue));
			if (fix.Buffer != null)
			{
				ByteBuffer byteBuffer = new ByteBuffer();
				byteBuffer.WriteUInt8(4);
				byteBuffer.WriteBytes(fix.Accept);
				SendPacketToServer(byteBuffer);
				_hashIn = new Sha1();
				_hashIn.SetBase(fix.Buffer);
				_hashOut = new Sha1();
				_hashOut.SetBase(fix.Checksum);
				_allAddonsAllowed = true;
			}
		}
	}

	[PacketHandler(3184u)]
	private void HandleLegacyAddonVerification(WorldPacket packet)
	{
		if (_hashIn == null)
		{
			Sha1Helper sha1Helper = new Sha1Helper(GetSession().AuthClient.GetSessionKey());
			_hashIn = new Sha1();
			byte[] hash = sha1Helper.GetHash();
			_hashIn.SetBase(hash);
			_hashOut = new Sha1();
			byte[] hash2 = sha1Helper.GetHash();
			_hashOut.SetBase(hash2);
		}
		byte[] array = packet.ReadToEnd();
		_hashOut.ProcessBuffer(array, array.Length);
		Handle(new ByteBuffer(array));
	}

	private void Handle(ByteBuffer payload)
	{
		switch (payload.ReadUInt8())
		{
		case 0:
			HandleGenericVersion(payload);
			break;
		case 2:
			HandleTbcVersion(payload);
			break;
		case 5:
			HandleFutureVersion(payload);
			break;
		}
	}

	private void SendPacketToServer(ByteBuffer payload)
	{
		byte[] data = payload.GetData();
		_hashIn.ProcessBuffer(data, data.Length);
		WorldPacket worldPacket = new WorldPacket(743u);
		worldPacket.WriteBytes(data);
		SendPacketToServer(worldPacket);
	}

	[PacketHandler(Opcode.SMSG_ARENA_TEAM_QUERY_RESPONSE)]
	private void HandleArenaTeamQueryResponse(WorldPacket packet)
	{
		uint key = packet.ReadUInt32();
		if (!GetSession().GameState.ArenaTeams.TryGetValue(key, out var value))
		{
			value = new ArenaTeamData();
			GetSession().GameState.ArenaTeams.Add(key, value);
		}
		value.Name = packet.ReadCString();
		value.TeamSize = packet.ReadUInt32();
		value.BackgroundColor = packet.ReadUInt32();
		value.EmblemStyle = packet.ReadUInt32();
		value.EmblemColor = packet.ReadUInt32();
		value.BorderStyle = packet.ReadUInt32();
		value.BorderColor = packet.ReadUInt32();
	}

	[PacketHandler(Opcode.SMSG_ARENA_TEAM_STATS)]
	private void HandleArenaTeamStats(WorldPacket packet)
	{
		uint key = packet.ReadUInt32();
		if (!GetSession().GameState.ArenaTeams.TryGetValue(key, out var value))
		{
			value = new ArenaTeamData();
			GetSession().GameState.ArenaTeams.Add(key, value);
		}
		value.Rating = packet.ReadUInt32();
		value.WeekPlayed = packet.ReadUInt32();
		value.WeekWins = packet.ReadUInt32();
		value.SeasonPlayed = packet.ReadUInt32();
		value.SeasonWins = packet.ReadUInt32();
		value.Rank = packet.ReadUInt32();
	}

	[PacketHandler(Opcode.SMSG_ARENA_TEAM_ROSTER)]
	private void HandleArenaTeamRoster(WorldPacket packet)
	{
		ArenaTeamRosterResponse arenaTeamRosterResponse = new ArenaTeamRosterResponse();
		arenaTeamRosterResponse.TeamId = packet.ReadUInt32();
		bool flag = false;
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_8_9464))
		{
			packet.ReadBool();
		}
		uint num = packet.ReadUInt32();
		arenaTeamRosterResponse.TeamSize = packet.ReadUInt32();
		for (int i = 0; i < num; i++)
		{
			ArenaTeamMember arenaTeamMember = default(ArenaTeamMember);
			PlayerCache playerCache = new PlayerCache();
			arenaTeamMember.MemberGUID = packet.ReadGuid().To128(GetSession().GameState);
			arenaTeamMember.Online = packet.ReadBool();
			arenaTeamMember.Name = (playerCache.Name = packet.ReadCString());
			arenaTeamMember.Captain = packet.ReadInt32();
			arenaTeamMember.Level = (playerCache.Level = packet.ReadUInt8());
			arenaTeamMember.ClassId = (playerCache.ClassId = (Class)packet.ReadUInt8());
			GetSession().GameState.UpdatePlayerCache(arenaTeamMember.MemberGUID, playerCache);
			arenaTeamMember.WeekGamesPlayed = packet.ReadUInt32();
			arenaTeamMember.WeekGamesWon = packet.ReadUInt32();
			arenaTeamMember.SeasonGamesPlayed = packet.ReadUInt32();
			arenaTeamMember.SeasonGamesWon = packet.ReadUInt32();
			arenaTeamMember.PersonalRating = packet.ReadUInt32();
			if (flag)
			{
				arenaTeamMember.dword60 = packet.ReadFloat();
				arenaTeamMember.dword68 = packet.ReadFloat();
			}
			arenaTeamRosterResponse.Members.Add(arenaTeamMember);
		}
		if (GetSession().GameState.ArenaTeams.TryGetValue(arenaTeamRosterResponse.TeamId, out var value))
		{
			arenaTeamRosterResponse.TeamPlayed = value.WeekPlayed;
			arenaTeamRosterResponse.TeamWins = value.WeekWins;
			arenaTeamRosterResponse.SeasonPlayed = value.SeasonPlayed;
			arenaTeamRosterResponse.SeasonWins = value.SeasonWins;
			arenaTeamRosterResponse.TeamRating = value.Rating;
			arenaTeamRosterResponse.PlayerRating = value.Rank;
		}
		SendPacketToClient(arenaTeamRosterResponse);
	}

	[PacketHandler(Opcode.SMSG_ARENA_TEAM_EVENT)]
	private void HandleArenaTeamEvent(WorldPacket packet)
	{
		ArenaTeamEvent arenaTeamEvent = new ArenaTeamEvent();
		ArenaTeamEventLegacy arenaTeamEventLegacy = (ArenaTeamEventLegacy)packet.ReadUInt8();
		arenaTeamEvent.Event = (ArenaTeamEventModern)Enum.Parse(typeof(ArenaTeamEventModern), arenaTeamEventLegacy.ToString());
		byte b = packet.ReadUInt8();
		for (byte b2 = 0; b2 < b; b2++)
		{
			string text = packet.ReadCString();
			switch (b2)
			{
			case 0:
				arenaTeamEvent.Param1 = text;
				break;
			case 1:
				arenaTeamEvent.Param2 = text;
				break;
			case 2:
				arenaTeamEvent.Param3 = text;
				break;
			}
		}
		if (packet.CanRead())
		{
			packet.ReadGuid();
		}
		SendPacketToClient(arenaTeamEvent);
	}

	[PacketHandler(Opcode.SMSG_ARENA_TEAM_COMMAND_RESULT)]
	private void HandleArenaTeamCommandResult(WorldPacket packet)
	{
		ArenaTeamCommandResult arenaTeamCommandResult = new ArenaTeamCommandResult();
		arenaTeamCommandResult.Action = (ArenaTeamCommandType)packet.ReadUInt32();
		arenaTeamCommandResult.TeamName = packet.ReadCString();
		arenaTeamCommandResult.PlayerName = packet.ReadCString();
		ArenaTeamCommandErrorLegacy arenaTeamCommandErrorLegacy = (ArenaTeamCommandErrorLegacy)packet.ReadUInt32();
		arenaTeamCommandResult.Error = (ArenaTeamCommandErrorModern)Enum.Parse(typeof(ArenaTeamCommandErrorModern), arenaTeamCommandErrorLegacy.ToString());
		SendPacketToClient(arenaTeamCommandResult);
	}

	[PacketHandler(Opcode.SMSG_ARENA_TEAM_INVITE)]
	private void HandleArenaTeamInvite(WorldPacket packet)
	{
		ArenaTeamInvite arenaTeamInvite = new ArenaTeamInvite();
		arenaTeamInvite.PlayerName = packet.ReadCString();
		arenaTeamInvite.TeamName = packet.ReadCString();
		arenaTeamInvite.PlayerGuid = GetSession().GameState.GetPlayerGuidByName(arenaTeamInvite.PlayerName);
		if (arenaTeamInvite.PlayerGuid == null)
		{
			arenaTeamInvite.PlayerGuid = WowGuid128.Empty;
		}
		arenaTeamInvite.PlayerVirtualAddress = GetSession().RealmId.GetAddress();
		arenaTeamInvite.TeamGuid = WowGuid128.Create(HighGuidType703.ArenaTeam, 1uL);
		SendPacketToClient(arenaTeamInvite);
	}

	[PacketHandler(Opcode.MSG_AUCTION_HELLO)]
	private void HandleAuctionHello(WorldPacket packet)
	{
		AuctionHelloResponse auctionHelloResponse = new AuctionHelloResponse();
		auctionHelloResponse.Guid = packet.ReadGuid().To128(GetSession().GameState);
		GetSession().GameState.CurrentInteractedWithNPC = auctionHelloResponse.Guid;
		auctionHelloResponse.AuctionHouseID = packet.ReadUInt32();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
		{
			auctionHelloResponse.OpenForBusiness = packet.ReadBool();
		}
		SendPacketToClient(auctionHelloResponse);
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_AUCTION_LIST_OWNED_ITEMS);
		worldPacket.WriteGuid(auctionHelloResponse.Guid.To64());
		worldPacket.WriteUInt32(0u);
		SendPacketToServer(worldPacket);
	}

	// 读取拍卖品
	private AuctionItem ReadAuctionItem(WorldPacket packet)
	{
		AuctionItem auctionItem = new AuctionItem();
		auctionItem.AuctionID = packet.ReadUInt32();
		auctionItem.Item = new ItemInstance();
		auctionItem.Item.ItemID = packet.ReadUInt32();
		byte b = (byte)(LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056) ? 7 : ((!LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180)) ? 1 : 6));
		for (byte b2 = 0; b2 < b; b2++)
		{
			ItemEnchantData itemEnchantData = new ItemEnchantData();
			itemEnchantData.Slot = b2;
			itemEnchantData.ID = packet.ReadUInt32();
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
			{
				itemEnchantData.Expiration = packet.ReadUInt32();
				itemEnchantData.Charges = packet.ReadInt32();
			}
			if (itemEnchantData.ID != 0)
			{
				auctionItem.Enchantments.Add(itemEnchantData);
			}
		}
		auctionItem.Item.RandomPropertiesID = packet.ReadUInt32();
		auctionItem.Item.RandomPropertiesSeed = packet.ReadUInt32();
		auctionItem.Count = packet.ReadInt32();
		auctionItem.Charges = packet.ReadInt32();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			auctionItem.Flags = packet.ReadUInt32();
		}
		auctionItem.Owner = packet.ReadGuid().To128(GetSession().GameState);
		auctionItem.OwnerAccountID = GetSession().GetGameAccountGuidForPlayer(auctionItem.Owner);
		auctionItem.MinBid = packet.ReadUInt32();
		auctionItem.MinIncrement = packet.ReadUInt32();
		auctionItem.BuyoutPrice = packet.ReadUInt32();
		auctionItem.DurationLeft = packet.ReadInt32();
		auctionItem.Bidder = packet.ReadGuid().To128(GetSession().GameState);
		auctionItem.BidAmount = packet.ReadUInt32();
		if (auctionItem.Item.ItemID == 0)
		{
			auctionItem.Item = null;
		}
		return auctionItem;
	}

	[PacketHandler(Opcode.SMSG_AUCTION_LIST_BIDDED_ITEMS_RESULT)]
	[PacketHandler(Opcode.SMSG_AUCTION_LIST_OWNED_ITEMS_RESULT)]
	private void HandleAuctionListMyItemsResult(WorldPacket packet)
	{
		AuctionListMyItemsResult auctionListMyItemsResult = new AuctionListMyItemsResult(packet.GetUniversalOpcode(isModern: false));
		uint num = packet.ReadUInt32();
		for (uint num2 = 0u; num2 < num; num2++)
		{
			AuctionItem auctionItem = ReadAuctionItem(packet);
			auctionListMyItemsResult.Items.Add(auctionItem);
		}
		auctionListMyItemsResult.TotalItemsCount = packet.ReadInt32();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_3_0_7561))
		{
			auctionListMyItemsResult.DesiredDelay = packet.ReadUInt32();
		}
		SendPacketToClient(auctionListMyItemsResult);
	}

    //处理拍卖列表物品结果
    [PacketHandler(Opcode.SMSG_AUCTION_LIST_ITEMS_RESULT)]
	private void HandleAuctionListItemsResult(WorldPacket packet)
	{
		AuctionListItemsResult auctionListItemsResult = new AuctionListItemsResult();
		uint num = packet.ReadUInt32();
		for (uint num2 = 0u; num2 < num; num2++)
		{
			AuctionItem auctionItem = ReadAuctionItem(packet);
			auctionItem.CensorServerSideInfo = true;
			auctionListItemsResult.Items.Add(auctionItem);
		}
		auctionListItemsResult.TotalItemsCount = packet.ReadInt32();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_3_0_7561))
		{
			auctionListItemsResult.DesiredDelay = packet.ReadUInt32();
		}
		SendPacketToClient(auctionListItemsResult);
	}

    // 处理拍卖命令结果
    [PacketHandler(Opcode.SMSG_AUCTION_COMMAND_RESULT)]
	private void HandleAuctionCommandResult(WorldPacket packet)
	{
		AuctionCommandResult auctionCommandResult = new AuctionCommandResult();
		auctionCommandResult.AuctionID = packet.ReadUInt32();
		auctionCommandResult.Command = (AuctionHouseAction)packet.ReadUInt32();
		auctionCommandResult.ErrorCode = (AuctionHouseError)packet.ReadUInt32();
		switch (auctionCommandResult.ErrorCode)
		{
		case AuctionHouseError.Ok:
			if (auctionCommandResult.Command == AuctionHouseAction.Bid)
			{
				auctionCommandResult.MinIncrement = packet.ReadUInt32();
			}
			break;
		case AuctionHouseError.Inventory:
			auctionCommandResult.BagResult = LegacyVersion.ConvertInventoryResult(packet.ReadUInt32());
			break;
		case AuctionHouseError.HigherBid:
			auctionCommandResult.Guid = packet.ReadGuid().To128(GetSession().GameState);
			auctionCommandResult.Money = packet.ReadUInt32();
			auctionCommandResult.MinIncrement = packet.ReadUInt32();
			break;
		}
		SendPacketToClient(auctionCommandResult);
	}

    //处理拍卖所有者通知
    [PacketHandler(Opcode.SMSG_AUCTION_OWNER_NOTIFICATION)]
	private void HandleAuctionOwnerNotification(WorldPacket packet)
	{
		AuctionOwnerNotification auctionOwnerNotification = new AuctionOwnerNotification();
		auctionOwnerNotification.AuctionID = packet.ReadUInt32();
		auctionOwnerNotification.BidAmount = packet.ReadUInt32();
		uint num = packet.ReadUInt32();
		WowGuid wowGuid = packet.ReadGuid();
		auctionOwnerNotification.Item.ItemID = packet.ReadUInt32();
		auctionOwnerNotification.Item.RandomPropertiesID = packet.ReadUInt32();
		float proceedsMailDelay = ((!LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056)) ? 3600f : packet.ReadFloat());
		if (wowGuid.IsEmpty())
		{
			AuctionClosedNotification auctionClosedNotification = new AuctionClosedNotification();
			auctionClosedNotification.Info = auctionOwnerNotification;
			auctionClosedNotification.Sold = auctionOwnerNotification.BidAmount != 0;
			auctionClosedNotification.ProceedsMailDelay = proceedsMailDelay;
			SendPacketToClient(auctionClosedNotification);
		}
		else
		{
			AuctionOwnerBidNotification auctionOwnerBidNotification = new AuctionOwnerBidNotification();
			auctionOwnerBidNotification.Info = auctionOwnerNotification;
			auctionOwnerBidNotification.MinIncrement = num;
			auctionOwnerBidNotification.Bidder = wowGuid.To128(GetSession().GameState);
			SendPacketToClient(auctionOwnerBidNotification);
		}
	}

    //处理拍卖投标人通知
    [PacketHandler(Opcode.SMSG_AUCTION_BIDDER_NOTIFICATION)]
	private void HandleAuctionBidderNotification(WorldPacket packet)
	{
		AuctionBidderNotification auctionBidderNotification = new AuctionBidderNotification();
		packet.ReadUInt32();
		auctionBidderNotification.AuctionID = packet.ReadUInt32();
		auctionBidderNotification.Bidder = packet.ReadGuid().To128(GetSession().GameState);
		uint num = packet.ReadUInt32();
		uint num2 = packet.ReadUInt32();
		auctionBidderNotification.Item.ItemID = packet.ReadUInt32();
		auctionBidderNotification.Item.RandomPropertiesID = packet.ReadUInt32();
		if (num == 0)
		{
			AuctionWonNotification auctionWonNotification = new AuctionWonNotification();
			auctionWonNotification.Info = auctionBidderNotification;
			SendPacketToClient(auctionWonNotification);
		}
		else
		{
			AuctionOutbidNotification auctionOutbidNotification = new AuctionOutbidNotification();
			auctionOutbidNotification.Info = auctionBidderNotification;
			auctionOutbidNotification.BidAmount = num;
			auctionOutbidNotification.MinIncrement = num2;
			SendPacketToClient(auctionOutbidNotification);
		}
	}

	// 处理香草战场列表
	[PacketHandler(Opcode.SMSG_BATTLEFIELD_LIST, ClientVersionBuild.Zero, ClientVersionBuild.V2_0_1_6180)]
	private void HandleBattlefieldListVanilla(WorldPacket packet)
	{
		BattlefieldList battlefieldList = new BattlefieldList();
		battlefieldList.BattlemasterGuid = packet.ReadGuid().To128(GetSession().GameState);
		GetSession().GameState.CurrentInteractedWithNPC = battlefieldList.BattlemasterGuid;
		battlefieldList.BattlemasterListID = GameData.GetBattlegroundIdFromMapId(packet.ReadUInt32());
		packet.ReadUInt8();
		uint num = packet.ReadUInt32();
		for (int i = 0; i < num; i++)
		{
			int num2 = packet.ReadInt32();
			battlefieldList.BattlefieldInstances.Add(num2);
		}
		SendPacketToClient(battlefieldList);
	}

	// TBC 战场
	[PacketHandler(Opcode.SMSG_BATTLEFIELD_LIST, ClientVersionBuild.V2_0_1_6180, ClientVersionBuild.V3_0_2_9056)]
	private void HandleBattlefieldListTBC(WorldPacket packet)
	{
		BattlefieldList battlefieldList = new BattlefieldList();
		battlefieldList.BattlemasterGuid = packet.ReadGuid().To128(GetSession().GameState);
		GetSession().GameState.CurrentInteractedWithNPC = battlefieldList.BattlemasterGuid;
		battlefieldList.BattlemasterListID = packet.ReadUInt32();
		packet.ReadUInt8();
		uint num = packet.ReadUInt32();
		for (int i = 0; i < num; i++)
		{
			int num2 = packet.ReadInt32();
			battlefieldList.BattlefieldInstances.Add(num2);
		}
		SendPacketToClient(battlefieldList);
	}

    // WotLK 战场
    [PacketHandler(Opcode.SMSG_BATTLEFIELD_LIST, ClientVersionBuild.V3_0_2_9056)]
	private void HandleBattlefieldListWotLK(WorldPacket packet)
	{
		BattlefieldList battlefieldList = new BattlefieldList();
		battlefieldList.BattlemasterGuid = packet.ReadGuid().To128(GetSession().GameState);
		GetSession().GameState.CurrentInteractedWithNPC = battlefieldList.BattlemasterGuid;
		battlefieldList.PvpAnywhere = packet.ReadBool();
		battlefieldList.BattlemasterListID = packet.ReadUInt32();
		battlefieldList.MinLevel = packet.ReadUInt8();
		battlefieldList.MaxLevel = packet.ReadUInt8();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_3_11685))
		{
			packet.ReadBool();
			packet.ReadInt32();
			packet.ReadInt32();
			packet.ReadInt32();
			if (packet.ReadBool())
			{
				battlefieldList.HasRandomWinToday = packet.ReadBool();
				packet.ReadInt32();
				packet.ReadInt32();
				packet.ReadInt32();
			}
		}
		uint num = packet.ReadUInt32();
		for (int i = 0; i < num; i++)
		{
			int num2 = packet.ReadInt32();
			battlefieldList.BattlefieldInstances.Add(num2);
		}
		SendPacketToClient(battlefieldList);
	}

    // 处理战场状态香草
    [PacketHandler(Opcode.SMSG_BATTLEFIELD_STATUS, ClientVersionBuild.Zero, ClientVersionBuild.V2_0_1_6180)]
	private void HandleBattlefieldStatusVanilla(WorldPacket packet)
	{
		BattlefieldStatusHeader battlefieldStatusHeader = new BattlefieldStatusHeader();
		battlefieldStatusHeader.Ticket.Id = 1 + packet.ReadUInt32();
		battlefieldStatusHeader.Ticket.RequesterGuid = GetSession().GameState.CurrentPlayerGuid;
		battlefieldStatusHeader.Ticket.Time = GetSession().GameState.GetBattleFieldQueueTime(battlefieldStatusHeader.Ticket.Id);
		battlefieldStatusHeader.Ticket.Type = RideType.Battlegrounds;
		uint num = packet.ReadUInt32();
		if (num != 0)
		{
			uint battlegroundIdFromMapId = GameData.GetBattlegroundIdFromMapId(num);
			battlefieldStatusHeader.BattlefieldListIDs.Add(battlegroundIdFromMapId);
			packet.ReadUInt8();
			battlefieldStatusHeader.InstanceID = packet.ReadUInt32();
			BattleGroundStatus battleGroundStatus = (BattleGroundStatus)packet.ReadUInt32();
			switch (battleGroundStatus)
			{
			case BattleGroundStatus.WaitQueue:
			{
				BattlefieldStatusQueued battlefieldStatusQueued = new BattlefieldStatusQueued();
				battlefieldStatusQueued.Hdr = battlefieldStatusHeader;
				battlefieldStatusQueued.AverageWaitTime = packet.ReadUInt32();
				battlefieldStatusQueued.WaitTime = packet.ReadUInt32();
				SendPacketToClient(battlefieldStatusQueued);
				break;
			}
			case BattleGroundStatus.WaitJoin:
			{
				BattlefieldStatusNeedConfirmation battlefieldStatusNeedConfirmation = new BattlefieldStatusNeedConfirmation();
				battlefieldStatusNeedConfirmation.Hdr = battlefieldStatusHeader;
				battlefieldStatusNeedConfirmation.Mapid = num;
				battlefieldStatusNeedConfirmation.Timeout = packet.ReadUInt32();
				SendPacketToClient(battlefieldStatusNeedConfirmation);
				break;
			}
			case BattleGroundStatus.InProgress:
			{
				BattlefieldStatusActive battlefieldStatusActive = new BattlefieldStatusActive();
				battlefieldStatusActive.Hdr = battlefieldStatusHeader;
				battlefieldStatusActive.Mapid = num;
				battlefieldStatusActive.ShutdownTimer = packet.ReadUInt32();
				battlefieldStatusActive.StartTimer = packet.ReadUInt32();
				if (battlefieldStatusActive.ShutdownTimer == 0)
				{
					BattlegroundInit battlegroundInit = new BattlegroundInit();
					battlegroundInit.Milliseconds = 1154756799u;
					SendPacketToClient(battlegroundInit);
				}
				SendPacketToClient(battlefieldStatusActive);
				break;
			}
			default:
				Log.Print(LogType.Error, $"Unexpected BG status {battleGroundStatus}.", "HandleBattlefieldStatusVanilla", "D:\\a\\HermesProxy\\HermesProxy\\World\\Client\\PacketHandlers\\BattleGroundHandler.cs");
				break;
			}
		}
		else
		{
			uint battleFieldQueueType = GetSession().GameState.GetBattleFieldQueueType(battlefieldStatusHeader.Ticket.Id);
			if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180) && battleFieldQueueType == GetSession().GameState.CurrentMapId)
			{
				PartyUpdate partyUpdate = GetSession().GameState.CurrentGroups[1];
				if (partyUpdate != null)
				{
					PartyUpdate partyUpdate2 = new PartyUpdate();
					partyUpdate2.SequenceNum = GetSession().GameState.GroupUpdateCounter++;
					partyUpdate2.PartyFlags = GroupFlags.FakeRaid | GroupFlags.Destroyed;
					partyUpdate2.PartyIndex = 1;
					partyUpdate2.PartyGUID = partyUpdate.PartyGUID;
					partyUpdate2.LeaderGUID = WowGuid128.Empty;
					partyUpdate2.MyIndex = -1;
					GetSession().GameState.CurrentGroups[1] = null;
					SendPacketToClient(partyUpdate2);
				}
			}
			BattlefieldStatusFailed battlefieldStatusFailed = new BattlefieldStatusFailed();
			battlefieldStatusFailed.Ticket = battlefieldStatusHeader.Ticket;
			battlefieldStatusFailed.Reason = 30;
			battlefieldStatusFailed.BattlefieldListId = GameData.GetBattlegroundIdFromMapId(battleFieldQueueType);
			SendPacketToClient(battlefieldStatusFailed);
			GetSession().GameState.BattleFieldQueueTimes.Remove(battlefieldStatusHeader.Ticket.Id);
		}
		GetSession().GameState.StoreBattleFieldQueueType(battlefieldStatusHeader.Ticket.Id, num);
	}

	[PacketHandler(Opcode.SMSG_BATTLEFIELD_STATUS, ClientVersionBuild.V2_0_1_6180)]
	private void HandleBattlefieldStatusTBC(WorldPacket packet)
	{
		BattlefieldStatusHeader battlefieldStatusHeader = new BattlefieldStatusHeader();
		battlefieldStatusHeader.Ticket.Id = 1 + packet.ReadUInt32();
		battlefieldStatusHeader.Ticket.RequesterGuid = GetSession().GameState.CurrentPlayerGuid;
		battlefieldStatusHeader.Ticket.Time = GetSession().GameState.GetBattleFieldQueueTime(battlefieldStatusHeader.Ticket.Id);
		battlefieldStatusHeader.Ticket.Type = RideType.Battlegrounds;
		battlefieldStatusHeader.ArenaTeamSize = packet.ReadUInt8();
		packet.ReadUInt8();
		uint num = packet.ReadUInt32();
		packet.ReadUInt16();
		if (num != 0)
		{
			battlefieldStatusHeader.BattlefieldListIDs.Add(num);
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_3_11685))
			{
				battlefieldStatusHeader.RangeMin = packet.ReadUInt8();
				battlefieldStatusHeader.RangeMax = packet.ReadUInt8();
			}
			battlefieldStatusHeader.InstanceID = packet.ReadUInt32();
			battlefieldStatusHeader.IsArena = packet.ReadBool();
			BattleGroundStatus battleGroundStatus = (BattleGroundStatus)packet.ReadUInt32();
			switch (battleGroundStatus)
			{
			case BattleGroundStatus.WaitQueue:
			{
				BattlefieldStatusQueued battlefieldStatusQueued = new BattlefieldStatusQueued();
				battlefieldStatusQueued.Hdr = battlefieldStatusHeader;
				battlefieldStatusQueued.AverageWaitTime = packet.ReadUInt32();
				battlefieldStatusQueued.WaitTime = packet.ReadUInt32();
				SendPacketToClient(battlefieldStatusQueued);
				break;
			}
			case BattleGroundStatus.WaitJoin:
			{
				BattlefieldStatusNeedConfirmation battlefieldStatusNeedConfirmation = new BattlefieldStatusNeedConfirmation();
				battlefieldStatusNeedConfirmation.Hdr = battlefieldStatusHeader;
				battlefieldStatusNeedConfirmation.Mapid = packet.ReadUInt32();
				if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_5_12213))
				{
					packet.ReadUInt64();
				}
				battlefieldStatusNeedConfirmation.Timeout = packet.ReadUInt32();
				SendPacketToClient(battlefieldStatusNeedConfirmation);
				break;
			}
			case BattleGroundStatus.InProgress:
			{
				BattlefieldStatusActive battlefieldStatusActive = new BattlefieldStatusActive();
				battlefieldStatusActive.Hdr = battlefieldStatusHeader;
				battlefieldStatusActive.Mapid = packet.ReadUInt32();
				if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_5_12213))
				{
					packet.ReadUInt64();
				}
				battlefieldStatusActive.ShutdownTimer = packet.ReadUInt32();
				battlefieldStatusActive.StartTimer = packet.ReadUInt32();
				battlefieldStatusActive.ArenaFaction = packet.ReadUInt8();
				if (battlefieldStatusActive.ShutdownTimer == 0)
				{
					BattlegroundInit battlegroundInit = new BattlegroundInit();
					battlegroundInit.Milliseconds = 1154756799u;
					SendPacketToClient(battlegroundInit);
				}
				SendPacketToClient(battlefieldStatusActive);
				break;
			}
			default:
				Log.Print(LogType.Error, $"Unexpected BG status {battleGroundStatus}.", "HandleBattlefieldStatusTBC", "D:\\a\\HermesProxy\\HermesProxy\\World\\Client\\PacketHandlers\\BattleGroundHandler.cs");
				break;
			}
		}
		else
		{
			BattlefieldStatusFailed battlefieldStatusFailed = new BattlefieldStatusFailed();
			battlefieldStatusFailed.Ticket = battlefieldStatusHeader.Ticket;
			battlefieldStatusFailed.Reason = 30;
			battlefieldStatusFailed.BattlefieldListId = GetSession().GameState.GetBattleFieldQueueType(battlefieldStatusHeader.Ticket.Id);
			SendPacketToClient(battlefieldStatusFailed);
			GetSession().GameState.BattleFieldQueueTimes.Remove(battlefieldStatusHeader.Ticket.Id);
		}
		GetSession().GameState.StoreBattleFieldQueueType(battlefieldStatusHeader.Ticket.Id, num);
	}

    // 处理普通 PvP 日志数据 香草
    [PacketHandler(Opcode.MSG_PVP_LOG_DATA, ClientVersionBuild.Zero, ClientVersionBuild.V2_0_1_6180)]
	private void HandlePvPLogDataVanilla(WorldPacket packet)
	{
		PVPMatchStatisticsMessage pVPMatchStatisticsMessage = new PVPMatchStatisticsMessage();
		if (packet.ReadBool())
		{
			pVPMatchStatisticsMessage.Winner = packet.ReadUInt8();
		}
		int num = packet.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			PVPMatchStatisticsMessage.PVPMatchPlayerStatistics pVPMatchPlayerStatistics = new PVPMatchStatisticsMessage.PVPMatchPlayerStatistics();
			pVPMatchPlayerStatistics.PlayerGUID = packet.ReadGuid().To128(GetSession().GameState);
			pVPMatchPlayerStatistics.Rank = packet.ReadInt32();
			pVPMatchPlayerStatistics.Kills = packet.ReadUInt32();
			pVPMatchPlayerStatistics.Honor = new PVPMatchStatisticsMessage.HonorData();
			pVPMatchPlayerStatistics.Honor.HonorKills = packet.ReadUInt32();
			pVPMatchPlayerStatistics.Honor.Deaths = packet.ReadUInt32();
			pVPMatchPlayerStatistics.Honor.ContributionPoints = packet.ReadUInt32();
			int num2 = packet.ReadInt32();
			for (int j = 0; j < num2; j++)
			{
				pVPMatchPlayerStatistics.Stats.Add(packet.ReadUInt32());
			}
			if (GetSession().GameState.CachedPlayers.TryGetValue(pVPMatchPlayerStatistics.PlayerGUID, out var value))
			{
				pVPMatchPlayerStatistics.Sex = value.SexId;
				pVPMatchPlayerStatistics.PlayerRace = value.RaceId;
				pVPMatchPlayerStatistics.PlayerClass = value.ClassId;
				pVPMatchPlayerStatistics.Faction = GameData.IsAllianceRace(value.RaceId);
			}
			else
			{
				pVPMatchPlayerStatistics.Sex = Gender.Male;
				pVPMatchPlayerStatistics.PlayerRace = Race.Human;
				pVPMatchPlayerStatistics.PlayerClass = Class.Warrior;
			}
			pVPMatchStatisticsMessage.Statistics.Add(pVPMatchPlayerStatistics);
		}
		SendPacketToClient(pVPMatchStatisticsMessage);
	}

	[PacketHandler(Opcode.MSG_PVP_LOG_DATA, ClientVersionBuild.V2_0_1_6180)]
	private void HandlePvPLogDataTBC(WorldPacket packet)
	{
		PVPMatchStatisticsMessage pVPMatchStatisticsMessage = new PVPMatchStatisticsMessage();
		if (packet.ReadBool())
		{
			pVPMatchStatisticsMessage.ArenaTeams = new PVPMatchStatisticsMessage.ArenaTeamsInfo();
			pVPMatchStatisticsMessage.ArenaTeams.Guids[0] = WowGuid128.Empty;
			pVPMatchStatisticsMessage.ArenaTeams.Guids[1] = WowGuid128.Empty;
			for (int i = 0; i < 2; i++)
			{
				packet.ReadUInt32();
				packet.ReadUInt32();
				if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
				{
					packet.ReadUInt32();
				}
			}
			for (int j = 0; j < 2; j++)
			{
				pVPMatchStatisticsMessage.ArenaTeams.Names[j] = packet.ReadCString();
			}
		}
		if (packet.ReadBool())
		{
			pVPMatchStatisticsMessage.Winner = packet.ReadUInt8();
		}
		int num = packet.ReadInt32();
		for (int k = 0; k < num; k++)
		{
			PVPMatchStatisticsMessage.PVPMatchPlayerStatistics pVPMatchPlayerStatistics = new PVPMatchStatisticsMessage.PVPMatchPlayerStatistics();
			pVPMatchPlayerStatistics.PlayerGUID = packet.ReadGuid().To128(GetSession().GameState);
			pVPMatchPlayerStatistics.Kills = packet.ReadUInt32();
			if (pVPMatchStatisticsMessage.ArenaTeams == null)
			{
				pVPMatchPlayerStatistics.Honor = new PVPMatchStatisticsMessage.HonorData();
				pVPMatchPlayerStatistics.Honor.HonorKills = packet.ReadUInt32();
				pVPMatchPlayerStatistics.Honor.Deaths = packet.ReadUInt32();
				pVPMatchPlayerStatistics.Honor.ContributionPoints = packet.ReadUInt32();
			}
			else
			{
				pVPMatchPlayerStatistics.Faction = packet.ReadBool();
				pVPMatchStatisticsMessage.PlayerCount[pVPMatchPlayerStatistics.Faction ? 1 : 0]++;
			}
			pVPMatchPlayerStatistics.DamageDone = packet.ReadUInt32();
			pVPMatchPlayerStatistics.HealingDone = packet.ReadUInt32();
			int num2 = packet.ReadInt32();
			for (int l = 0; l < num2; l++)
			{
				pVPMatchPlayerStatistics.Stats.Add(packet.ReadUInt32());
			}
			if (GetSession().GameState.CachedPlayers.TryGetValue(pVPMatchPlayerStatistics.PlayerGUID, out var value))
			{
				pVPMatchPlayerStatistics.Sex = value.SexId;
				pVPMatchPlayerStatistics.PlayerRace = value.RaceId;
				pVPMatchPlayerStatistics.PlayerClass = value.ClassId;
				if (pVPMatchStatisticsMessage.ArenaTeams == null)
				{
					pVPMatchPlayerStatistics.Faction = GameData.IsAllianceRace(value.RaceId);
				}
			}
			else
			{
				pVPMatchPlayerStatistics.Sex = Gender.Male;
				pVPMatchPlayerStatistics.PlayerRace = Race.Human;
				pVPMatchPlayerStatistics.PlayerClass = Class.Warrior;
			}
			pVPMatchStatisticsMessage.Statistics.Add(pVPMatchPlayerStatistics);
		}
		SendPacketToClient(pVPMatchStatisticsMessage);
	}

	//战场玩家坐标
	private BattlegroundPlayerPosition ReadBattlegroundPlayerPosition(WorldPacket packet)
	{
		BattlegroundPlayerPosition result = default(BattlegroundPlayerPosition);
		result.Guid = packet.ReadGuid().To128(GetSession().GameState);
		result.Pos = packet.ReadVector2();
		return result;
	}

    // 处理战场玩家位置香草
    [PacketHandler(Opcode.MSG_BATTLEGROUND_PLAYER_POSITIONS, ClientVersionBuild.Zero, ClientVersionBuild.V2_0_1_6180)]
	private void HandleBattlegroundPlayerPositionsVanilla(WorldPacket packet)
	{
		GetSession().GameState.FlagCarrierGuids.Clear();
		BattlegroundPlayerPositions battlegroundPlayerPositions = new BattlegroundPlayerPositions();
		uint num = packet.ReadUInt32();
		for (uint num2 = 0u; num2 < num; num2++)
		{
			ReadBattlegroundPlayerPosition(packet);
		}
		if (packet.ReadBool())
		{
			BattlegroundPlayerPosition battlegroundPlayerPosition = ReadBattlegroundPlayerPosition(packet);
			if (GetSession().GameState.IsAlliancePlayer(battlegroundPlayerPosition.Guid))
			{
				battlegroundPlayerPosition.IconID = 1;
				battlegroundPlayerPosition.ArenaSlot = 3;
			}
			else
			{
				battlegroundPlayerPosition.IconID = 2;
				battlegroundPlayerPosition.ArenaSlot = 2;
			}
			battlegroundPlayerPositions.FlagCarriers.Add(battlegroundPlayerPosition);
			GetSession().GameState.FlagCarrierGuids.Add(battlegroundPlayerPosition.Guid);
		}
		SendPacketToClient(battlegroundPlayerPositions);
	}

	[PacketHandler(Opcode.MSG_BATTLEGROUND_PLAYER_POSITIONS, ClientVersionBuild.V2_0_1_6180)]
	private void HandleBattlegroundPlayerPositionsTBC(WorldPacket packet)
	{
		BattlegroundPlayerPositions battlegroundPlayerPositions = new BattlegroundPlayerPositions();
		uint num = packet.ReadUInt32();
		uint num2 = packet.ReadUInt32();
		for (uint num3 = 0u; num3 < num; num3++)
		{
			ReadBattlegroundPlayerPosition(packet);
		}
		GetSession().GameState.FlagCarrierGuids.Clear();
		for (uint num4 = 0u; num4 < num2; num4++)
		{
			BattlegroundPlayerPosition battlegroundPlayerPosition = ReadBattlegroundPlayerPosition(packet);
			if (GetSession().GameState.IsAlliancePlayer(battlegroundPlayerPosition.Guid))
			{
				battlegroundPlayerPosition.IconID = 1;
				battlegroundPlayerPosition.ArenaSlot = 3;
			}
			else
			{
				battlegroundPlayerPosition.IconID = 2;
				battlegroundPlayerPosition.ArenaSlot = 2;
			}
			battlegroundPlayerPositions.FlagCarriers.Add(battlegroundPlayerPosition);
			GetSession().GameState.FlagCarrierGuids.Add(battlegroundPlayerPosition.Guid);
		}
		SendPacketToClient(battlegroundPlayerPositions);
	}

	[PacketHandler(Opcode.SMSG_BATTLEGROUND_PLAYER_JOINED)]
	[PacketHandler(Opcode.SMSG_BATTLEGROUND_PLAYER_LEFT)]
	private void HandleBattlegroundPlayerLeftOrJoined(WorldPacket packet)
	{
		BattlegroundPlayerLeftOrJoined battlegroundPlayerLeftOrJoined = new BattlegroundPlayerLeftOrJoined(packet.GetUniversalOpcode(isModern: false));
		battlegroundPlayerLeftOrJoined.Guid = packet.ReadGuid().To128(GetSession().GameState);
		SendPacketToClient(battlegroundPlayerLeftOrJoined);
	}

	[PacketHandler(Opcode.SMSG_AREA_SPIRIT_HEALER_TIME)]
	private void HandleAreaSpiritHealerTime(WorldPacket packet)
	{
		AreaSpiritHealerTime areaSpiritHealerTime = new AreaSpiritHealerTime();
		areaSpiritHealerTime.HealerGuid = packet.ReadGuid().To128(GetSession().GameState);
		areaSpiritHealerTime.TimeLeft = packet.ReadUInt32();
		SendPacketToClient(areaSpiritHealerTime);
	}

	[PacketHandler(Opcode.SMSG_PVP_CREDIT)]
	private void HandlePvPCredit(WorldPacket packet)
	{
		PvPCredit pvPCredit = new PvPCredit();
		pvPCredit.OriginalHonor = packet.ReadInt32();
		pvPCredit.Target = packet.ReadGuid().To128(GetSession().GameState);
		pvPCredit.Rank = packet.ReadUInt32();
		SendPacketToClient(pvPCredit);
	}

	// 处理玩家皮肤
	[PacketHandler(Opcode.SMSG_PLAYER_SKINNED)]
	private void HandlePlayerSkinned(WorldPacket packet)
	{
		PlayerSkinned playerSkinned = new PlayerSkinned();
		if (packet.CanRead())
		{
			playerSkinned.FreeRepop = packet.ReadBool();
		}
		SendPacketToClient(playerSkinned);
	}

	// 角色选择面板
	[PacketHandler(Opcode.SMSG_ENUM_CHARACTERS_RESULT)]
	private void HandleEnumCharactersResult(WorldPacket packet)
	{
		EnumCharactersResult enumCharactersResult = new EnumCharactersResult();
		enumCharactersResult.Success = true;
		enumCharactersResult.IsDeletedCharacters = false;
		enumCharactersResult.IsNewPlayerRestrictionSkipped = false;
		enumCharactersResult.IsNewPlayerRestricted = false;
		enumCharactersResult.IsNewPlayer = true;
		enumCharactersResult.IsAlliedRacesCreationAllowed = false;
		GetSession().GameState.OwnCharacters.Clear();
		byte b = packet.ReadUInt8();
		for (byte b2 = 0; b2 < b; b2++)
		{
			EnumCharactersResult.CharacterInfo characterInfo = new EnumCharactersResult.CharacterInfo();
			PlayerCache playerCache = new PlayerCache();
			characterInfo.Guid = packet.ReadGuid().To128(GetSession().GameState);
			characterInfo.Name = (playerCache.Name = packet.ReadCString());
			characterInfo.RaceId = (playerCache.RaceId = (Race)packet.ReadUInt8());
			characterInfo.ClassId = (playerCache.ClassId = (Class)packet.ReadUInt8());
			characterInfo.SexId = (playerCache.SexId = (Gender)packet.ReadUInt8());
			byte skin = packet.ReadUInt8();
			byte face = packet.ReadUInt8();
			byte hairStyle = packet.ReadUInt8();
			byte hairColor = packet.ReadUInt8();
			byte facialHair = packet.ReadUInt8();
			characterInfo.Customizations = CharacterCustomizations.ConvertLegacyCustomizationsToModern(characterInfo.RaceId, characterInfo.SexId, skin, face, hairStyle, hairColor, facialHair);
			characterInfo.ExperienceLevel = (playerCache.Level = packet.ReadUInt8());
			if (characterInfo.ExperienceLevel > enumCharactersResult.MaxCharacterLevel)
			{
				enumCharactersResult.MaxCharacterLevel = characterInfo.ExperienceLevel;
			}
			GetSession().GameState.UpdatePlayerCache(characterInfo.Guid, playerCache);
			characterInfo.ZoneId = packet.ReadUInt32();
			characterInfo.MapId = packet.ReadUInt32();
			characterInfo.PreloadPos = packet.ReadVector3();
			uint num = packet.ReadUInt32();
			GetSession().GameState.StorePlayerGuildId(characterInfo.Guid, num);
			characterInfo.GuildGuid = WowGuid128.Create(HighGuidType703.Guild, num);
			characterInfo.Flags = (CharacterFlags)packet.ReadUInt32();
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
			{
				characterInfo.Flags2 = packet.ReadUInt32();
			}
			characterInfo.FirstLogin = packet.ReadUInt8() != 0;
			characterInfo.PetCreatureDisplayId = packet.ReadUInt32();
			characterInfo.PetExperienceLevel = packet.ReadUInt32();
			characterInfo.PetCreatureFamilyId = packet.ReadUInt32();
			for (int i = 0; i < 19; i++)
			{
				characterInfo.VisualItems[i].DisplayId = packet.ReadUInt32();
				characterInfo.VisualItems[i].InvType = packet.ReadUInt8();
				if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
				{
					characterInfo.VisualItems[i].DisplayEnchantId = packet.ReadUInt32();
				}
			}
			int num2 = ((!LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_3_11685)) ? 1 : 4);
			for (int j = 0; j < num2; j++)
			{
				characterInfo.VisualItems[19 + j].DisplayId = packet.ReadUInt32();
				characterInfo.VisualItems[19 + j].InvType = packet.ReadUInt8();
				if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
				{
					characterInfo.VisualItems[19 + j].DisplayEnchantId = packet.ReadUInt32();
				}
			}
			characterInfo.Flags2 = 402685956u;
			characterInfo.Flags3 = 855688192u;
			characterInfo.Flags4 = 0u;
			characterInfo.ProfessionIds[0] = 0u;
			characterInfo.ProfessionIds[1] = 0u;
			characterInfo.LastPlayedTime = (ulong)Time.UnixTime;
			characterInfo.SpecID = 0;
			characterInfo.Unknown703 = 55u;
			characterInfo.LastLoginVersion = 11400u;
			characterInfo.OverrideSelectScreenFileDataID = 0u;
			characterInfo.BoostInProgress = false;
			characterInfo.unkWod61x = 0;
			characterInfo.ExpansionChosen = true;
			enumCharactersResult.Characters.Add(characterInfo);
			GetSession().GameState.OwnCharacters.Add(new OwnCharacterInfo
			{
				AccountId = GetSession().GameAccountInfo.WoWAccountGuid,
				CharacterGuid = characterInfo.Guid,
				Realm = GetSession().Realm,
				LastLoginUnixSec = characterInfo.LastPlayedTime,
				Name = characterInfo.Name,
				RaceId = characterInfo.RaceId,
				ClassId = characterInfo.ClassId,
				SexId = characterInfo.SexId,
				Level = characterInfo.ExperienceLevel
			});
		}
		enumCharactersResult.RaceUnlockData.Add(new EnumCharactersResult.RaceUnlock(1, hasExpansion: true, hasAchievement: false, hasHeritageArmor: false));
		enumCharactersResult.RaceUnlockData.Add(new EnumCharactersResult.RaceUnlock(2, hasExpansion: true, hasAchievement: false, hasHeritageArmor: false));
		enumCharactersResult.RaceUnlockData.Add(new EnumCharactersResult.RaceUnlock(3, hasExpansion: true, hasAchievement: false, hasHeritageArmor: false));
		enumCharactersResult.RaceUnlockData.Add(new EnumCharactersResult.RaceUnlock(4, hasExpansion: true, hasAchievement: false, hasHeritageArmor: false));
		enumCharactersResult.RaceUnlockData.Add(new EnumCharactersResult.RaceUnlock(5, hasExpansion: true, hasAchievement: false, hasHeritageArmor: false));
		enumCharactersResult.RaceUnlockData.Add(new EnumCharactersResult.RaceUnlock(6, hasExpansion: true, hasAchievement: false, hasHeritageArmor: false));
		enumCharactersResult.RaceUnlockData.Add(new EnumCharactersResult.RaceUnlock(7, hasExpansion: true, hasAchievement: false, hasHeritageArmor: false));
		enumCharactersResult.RaceUnlockData.Add(new EnumCharactersResult.RaceUnlock(8, hasExpansion: true, hasAchievement: false, hasHeritageArmor: false));
		if (ModernVersion.ExpansionVersion >= 2 && LegacyVersion.ExpansionVersion >= 2)
		{
			enumCharactersResult.RaceUnlockData.Add(new EnumCharactersResult.RaceUnlock(10, hasExpansion: true, hasAchievement: false, hasHeritageArmor: false));
			enumCharactersResult.RaceUnlockData.Add(new EnumCharactersResult.RaceUnlock(11, hasExpansion: true, hasAchievement: false, hasHeritageArmor: false));
		}
		SendPacketToClient(enumCharactersResult);
	}

	[PacketHandler(Opcode.SMSG_CREATE_CHAR)]
	private void HandleCreateChar(WorldPacket packet)
	{
		byte legacyValue = packet.ReadUInt8();
		CreateChar createChar = new CreateChar();
		createChar.Guid = new WowGuid128();
		createChar.Code = ModernVersion.ConvertResponseCodesValue(legacyValue);
		SendPacketToClient(createChar);
	}

	[PacketHandler(Opcode.SMSG_DELETE_CHAR)]
	private void HandleDeleteChar(WorldPacket packet)
	{
		byte legacyValue = packet.ReadUInt8();
		DeleteChar deleteChar = new DeleteChar();
		deleteChar.Code = ModernVersion.ConvertResponseCodesValue(legacyValue);
		SendPacketToClient(deleteChar);
	}

    //处理查询玩家名称响应
    [PacketHandler(Opcode.SMSG_QUERY_PLAYER_NAME_RESPONSE)]
	private void HandleQueryPlayerNameResponse(WorldPacket packet)
	{
		QueryPlayerNameResponse queryPlayerNameResponse = new QueryPlayerNameResponse();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
		{
			queryPlayerNameResponse.Player = (queryPlayerNameResponse.Data.GuidActual = packet.ReadPackedGuid().To128(GetSession().GameState));
			if (packet.ReadBool())
			{
				queryPlayerNameResponse.Result = 1;
				SendPacketToClient(queryPlayerNameResponse);
				return;
			}
		}
		else
		{
			queryPlayerNameResponse.Player = (queryPlayerNameResponse.Data.GuidActual = packet.ReadGuid().To128(GetSession().GameState));
		}
		PlayerCache playerCache = new PlayerCache();
		queryPlayerNameResponse.Data.Name = (playerCache.Name = packet.ReadCString());
		packet.ReadCString();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
		{
			queryPlayerNameResponse.Data.RaceID = (playerCache.RaceId = (Race)packet.ReadUInt8());
			queryPlayerNameResponse.Data.Sex = (playerCache.SexId = (Gender)packet.ReadUInt8());
			queryPlayerNameResponse.Data.ClassID = (playerCache.ClassId = (Class)packet.ReadUInt8());
		}
		else
		{
			// 香草 数据
			queryPlayerNameResponse.Data.RaceID = (playerCache.RaceId = (Race)packet.ReadUInt32());
			queryPlayerNameResponse.Data.Sex = (playerCache.SexId = (Gender)packet.ReadUInt32());
			queryPlayerNameResponse.Data.ClassID = (playerCache.ClassId = (Class)packet.ReadInt32());
		}
		if (GetSession().GameState.CachedPlayers.ContainsKey(queryPlayerNameResponse.Player))
		{
			queryPlayerNameResponse.Data.Level = GetSession().GameState.CachedPlayers[queryPlayerNameResponse.Player].Level;
		}
		if (queryPlayerNameResponse.Data.Level == 0)
		{
			queryPlayerNameResponse.Data.Level = 1;
		}
		GetSession().GameState.UpdatePlayerCache(queryPlayerNameResponse.Player, playerCache);
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180) && packet.ReadBool())
		{
			for (int i = 0; i < 5; i++)
			{
				queryPlayerNameResponse.Data.DeclinedNames.name[i] = packet.ReadCString();
			}
		}
		queryPlayerNameResponse.Data.IsDeleted = false;
		queryPlayerNameResponse.Data.AccountID = GetSession().GetGameAccountGuidForPlayer(queryPlayerNameResponse.Player);
		queryPlayerNameResponse.Data.BnetAccountID = GetSession().GetBnetAccountGuidForPlayer(queryPlayerNameResponse.Player);
		queryPlayerNameResponse.Data.VirtualRealmAddress = GetSession().RealmId.GetAddress();
		SendPacketToClient(queryPlayerNameResponse);
	}

    //处理登录验证世界
    [PacketHandler(Opcode.SMSG_LOGIN_VERIFY_WORLD)]
	private void HandleLoginVerifyWorld(WorldPacket packet)
	{
		LoginVerifyWorld loginVerifyWorld = new LoginVerifyWorld();
		loginVerifyWorld.MapID = packet.ReadUInt32();
		GetSession().GameState.CurrentMapId = loginVerifyWorld.MapID;
		loginVerifyWorld.Pos.X = packet.ReadFloat();
		loginVerifyWorld.Pos.Y = packet.ReadFloat();
		loginVerifyWorld.Pos.Z = packet.ReadFloat();
		loginVerifyWorld.Pos.Orientation = packet.ReadFloat();
		SendPacketToClient(loginVerifyWorld);
		GetSession().GameState.IsInWorld = true;
		WorldServerInfo worldServerInfo = new WorldServerInfo();
		if (loginVerifyWorld.MapID > 1)
		{
			worldServerInfo.DifficultyID = 1u;
			worldServerInfo.InstanceGroupSize = 5u;
		}
		SendPacketToClient(worldServerInfo);
		SetAllTaskProgress packet2 = new SetAllTaskProgress();
		SendPacketToClient(packet2);
		InitialSetup initialSetup = new InitialSetup();
		initialSetup.ServerExpansionLevel = (byte)(LegacyVersion.ExpansionVersion - 1);
		SendPacketToClient(initialSetup);
		LoadCUFProfiles loadCUFProfiles = new LoadCUFProfiles();
		loadCUFProfiles.Data = GetSession().AccountDataMgr.LoadCUFProfiles();
		SendPacketToClient(loadCUFProfiles);
	}

	[PacketHandler(Opcode.SMSG_CHARACTER_LOGIN_FAILED)]
	private void HandleCharacterLoginFailed(WorldPacket packet)
	{
		CharacterLoginFailed characterLoginFailed = new CharacterLoginFailed();
		characterLoginFailed.Code = (LoginFailureReason)packet.ReadUInt8();
		SendPacketToClient(characterLoginFailed);
		GetSession().GameState.IsInWorld = false;
	}

    // 处理更新操作按钮
    [PacketHandler(Opcode.SMSG_UPDATE_ACTION_BUTTONS)]
	private void HandleUpdateActionButtons(WorldPacket packet)
	{
		if (!LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767) || packet.ReadUInt8() != 2)
		{
			List<int> list = new List<int>();
			int num = 120;
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
			{
				num = 144;
			}
			else if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
			{
				num = 132;
			}
			for (int i = 0; i < num; i++)
			{
				int num2 = packet.ReadInt32();
				list.Add(num2);
			}
			while (list.Count < 132)
			{
				list.Add(0);
			}
			GetSession().GameState.ActionButtons = list;
		}
	}

	[PacketHandler(Opcode.SMSG_LOGOUT_RESPONSE)]
	private void HandleLogoutResponse(WorldPacket packet)
	{
		LogoutResponse logoutResponse = new LogoutResponse();
		logoutResponse.LogoutResult = packet.ReadInt32();
		logoutResponse.Instant = packet.ReadBool();
		SendPacketToClient(logoutResponse);
	}

	[PacketHandler(Opcode.SMSG_LOGOUT_COMPLETE)]
	private void HandleLogoutComplete(WorldPacket packet)
	{
		LogoutComplete packet2 = new LogoutComplete();
		SendPacketToClient(packet2);
		GetSession().GameState = GameSessionData.CreateNewGameSessionData(GetSession());
		GetSession().InstanceSocket.CloseSocket();
		GetSession().InstanceSocket = null;
	}

	[PacketHandler(Opcode.SMSG_LOGOUT_CANCEL_ACK)]
	private void HandleLogoutCancelAck(WorldPacket packet)
	{
		LogoutCancelAck packet2 = new LogoutCancelAck();
		SendPacketToClient(packet2);
	}

	[PacketHandler(Opcode.SMSG_LOG_XP_GAIN)]
	private void HandleLogXPGain(WorldPacket packet)
	{
		LogXPGain logXPGain = new LogXPGain();
		logXPGain.Victim = packet.ReadGuid().To128(GetSession().GameState);
		logXPGain.Original = packet.ReadInt32();
		logXPGain.Reason = (PlayerLogXPReason)packet.ReadUInt8();
		if (logXPGain.Reason == PlayerLogXPReason.Kill)
		{
			logXPGain.Amount = packet.ReadInt32();
			logXPGain.GroupBonus = packet.ReadFloat();
		}
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_4_0_8089) && packet.CanRead())
		{
			logXPGain.RAFBonus = packet.ReadUInt8();
		}
		SendPacketToClient(logXPGain);
	}

	[PacketHandler(Opcode.SMSG_PLAYED_TIME)]
	private void HandlePlayedTime(WorldPacket packet)
	{
		PlayedTime playedTime = new PlayedTime();
		playedTime.TotalTime = packet.ReadUInt32();
		playedTime.LevelTime = packet.ReadUInt32();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			playedTime.TriggerEvent = packet.ReadBool();
		}
		else
		{
			playedTime.TriggerEvent = GetSession().GameState.ShowPlayedTime;
		}
		SendPacketToClient(playedTime);
	}

    // 处理升级信息
    [PacketHandler(Opcode.SMSG_LEVEL_UP_INFO)]
	private void HandleLevelUpInfo(WorldPacket packet)
	{
		LevelUpInfo levelUpInfo = new LevelUpInfo();
		levelUpInfo.Level = packet.ReadInt32();
		levelUpInfo.HealthDelta = packet.ReadInt32();
		for (int i = 0; i < LegacyVersion.GetPowersCount(); i++)
		{
			levelUpInfo.PowerDelta[i] = packet.ReadInt32();
		}
		for (int j = 0; j < 5; j++)
		{
			levelUpInfo.StatDelta[j] = packet.ReadInt32();
		}
		SendPacketToClient(levelUpInfo);
	}

    //处理更新连击点
    [PacketHandler(Opcode.SMSG_UPDATE_COMBO_POINTS)]
	private void HandleUpdateComboPoints(WorldPacket packet)
	{
		ObjectUpdate objectUpdate = new ObjectUpdate(GetSession().GameState.CurrentPlayerGuid, UpdateTypeModern.Values, GetSession());
		objectUpdate.ActivePlayerData.ComboTarget = packet.ReadPackedGuid().To128(GetSession().GameState);
		byte value = packet.ReadUInt8();
		sbyte powerSlotForClass = ClassPowerTypes.GetPowerSlotForClass(GetSession().GameState.GetUnitClass(GetSession().GameState.CurrentPlayerGuid), PowerType.ComboPoints);
		if (powerSlotForClass >= 0)
		{
			objectUpdate.UnitData.Power[powerSlotForClass] = value;
		}
		UpdateObject updateObject = new UpdateObject(GetSession().GameState);
		updateObject.ObjectUpdates.Add(objectUpdate);
		SendPacketToClient(updateObject);
	}

    //处理检查结果
    [PacketHandler(Opcode.SMSG_INSPECT_RESULT)]
	[PacketHandler(Opcode.SMSG_INSPECT_TALENT)]
	private void HandleInspectResult(WorldPacket packet)
	{
		InspectResult inspectResult = new InspectResult();
		if (packet.GetUniversalOpcode(isModern: false) == Opcode.SMSG_INSPECT_RESULT)
		{
			inspectResult.DisplayInfo.GUID = packet.ReadGuid().To128(GetSession().GameState);
		}
		else
		{
			inspectResult.DisplayInfo.GUID = packet.ReadPackedGuid().To128(GetSession().GameState);
		}
		if (!GetSession().GameState.CachedPlayers.TryGetValue(inspectResult.DisplayInfo.GUID, out var value))
		{
			return;
		}
		inspectResult.DisplayInfo.Name = value.Name;
		inspectResult.DisplayInfo.ClassId = value.ClassId;
        inspectResult.DisplayInfo.RaceId = value.RaceId;
        inspectResult.DisplayInfo.SexId = value.SexId;
        Dictionary<int, UpdateField> cachedObjectFieldsLegacy = GetSession().GameState.GetCachedObjectFieldsLegacy(inspectResult.DisplayInfo.GUID);
		if (cachedObjectFieldsLegacy != null)
		{
			int updateField = LegacyVersion.GetUpdateField(PlayerField.PLAYER_VISIBLE_ITEM_1_0);
			if (updateField >= 0)
			{
				byte b = (byte)(LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180) ? 16u : 12u);
				for (byte b2 = 0; b2 < 19; b2++)
				{
					if (cachedObjectFieldsLegacy.ContainsKey(updateField + b2 * b))
					{
						uint uInt32Value = cachedObjectFieldsLegacy[updateField + b2 * b].UInt32Value;
						if (uInt32Value != 0)
						{
							InspectItemData inspectItemData = new InspectItemData();
							inspectItemData.Index = b2;
							inspectItemData.Item.ItemID = uInt32Value;
							inspectResult.DisplayInfo.Items.Add(inspectItemData);
						}
					}
				}
			}
			int updateField2 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_VISIBLE_ITEM_1_ENTRYID);
			if (updateField2 >= 0)
			{
				int num = 2;
				for (byte b3 = 0; b3 < 19; b3++)
				{
					if (cachedObjectFieldsLegacy.ContainsKey(updateField2 + b3 * num))
					{
						uint uInt32Value2 = cachedObjectFieldsLegacy[updateField2 + b3 * num].UInt32Value;
						if (uInt32Value2 != 0)
						{
							InspectItemData inspectItemData2 = new InspectItemData();
							inspectItemData2.Index = b3;
							inspectItemData2.Item.ItemID = uInt32Value2;
							inspectResult.DisplayInfo.Items.Add(inspectItemData2);
						}
					}
				}
			}
			int updateField3 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_GUILDID);
			if (updateField3 >= 0 && cachedObjectFieldsLegacy.ContainsKey(updateField3))
			{
				inspectResult.GuildData = new InspectGuildData();
				inspectResult.GuildData.GuildGUID = WowGuid128.Create(HighGuidType703.Guild, cachedObjectFieldsLegacy[updateField3].UInt32Value);
			}
			int updateField4 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_BYTES);
			if (updateField4 >= 0 && cachedObjectFieldsLegacy.ContainsKey(updateField4))
			{
				inspectResult.LifetimeMaxRank = (byte)((cachedObjectFieldsLegacy[updateField4].UInt32Value >> 24) & 0xFFu);
			}
		}
		if (packet.GetUniversalOpcode(isModern: false) == Opcode.SMSG_INSPECT_TALENT)
		{
			uint num2 = packet.ReadUInt32();
			for (uint num3 = 0u; num3 < num2; num3++)
			{
				byte b4 = packet.ReadUInt8();
				if (num3 < 25)
				{
					inspectResult.Talents.Add(b4);
				}
			}
		}
        SendPacketToClient(inspectResult);
	}

    // 处理检查荣誉统计香草
    [PacketHandler(Opcode.MSG_INSPECT_HONOR_STATS, ClientVersionBuild.Zero, ClientVersionBuild.V2_0_1_6180)]
	private void HandleInspectHonorStatsVanilla(WorldPacket packet)
	{
		WowGuid128 playerGUID = packet.ReadGuid().To128(GetSession().GameState);
		byte lifetimeHighestRank = packet.ReadUInt8();
		ushort todayHonorableKills = packet.ReadUInt16();
		ushort todayDishonorableKills = packet.ReadUInt16();
		ushort yesterdayHonorableKills = packet.ReadUInt16();
		ushort yesterdayDishonorableKills = packet.ReadUInt16();
		ushort lastWeekHonorableKills = packet.ReadUInt16();
		ushort lastWeekDishonorableKills = packet.ReadUInt16();
		ushort thisWeekHonorableKills = packet.ReadUInt16();
		ushort thisWeekDishonorableKills = packet.ReadUInt16();
		uint num = packet.ReadUInt32();
		uint lifetimeDishonorableKills = packet.ReadUInt32();
		uint yesterdayHonor = packet.ReadUInt32();
		uint lastWeekHonor = packet.ReadUInt32();
		uint thisWeekHonor = packet.ReadUInt32();
		uint standing = packet.ReadUInt32();
		byte rankProgress = packet.ReadUInt8();
		if (ModernVersion.ExpansionVersion == 1)
		{
			InspectHonorStatsResultClassic inspectHonorStatsResultClassic = new InspectHonorStatsResultClassic();
			inspectHonorStatsResultClassic.PlayerGUID = playerGUID;
			inspectHonorStatsResultClassic.LifetimeHighestRank = lifetimeHighestRank;
			inspectHonorStatsResultClassic.TodayHonorableKills = todayHonorableKills;
			inspectHonorStatsResultClassic.TodayDishonorableKills = todayDishonorableKills;
			inspectHonorStatsResultClassic.YesterdayHonorableKills = yesterdayHonorableKills;
			inspectHonorStatsResultClassic.YesterdayDishonorableKills = yesterdayDishonorableKills;
			inspectHonorStatsResultClassic.LastWeekHonorableKills = lastWeekHonorableKills;
			inspectHonorStatsResultClassic.LastWeekDishonorableKills = lastWeekDishonorableKills;
			inspectHonorStatsResultClassic.ThisWeekHonorableKills = thisWeekHonorableKills;
			inspectHonorStatsResultClassic.ThisWeekDishonorableKills = thisWeekDishonorableKills;
			inspectHonorStatsResultClassic.LifetimeHonorableKills = num;
			inspectHonorStatsResultClassic.LifetimeDishonorableKills = lifetimeDishonorableKills;
			inspectHonorStatsResultClassic.YesterdayHonor = yesterdayHonor;
			inspectHonorStatsResultClassic.LastWeekHonor = lastWeekHonor;
			inspectHonorStatsResultClassic.ThisWeekHonor = thisWeekHonor;
			inspectHonorStatsResultClassic.Standing = standing;
			inspectHonorStatsResultClassic.RankProgress = rankProgress;
			SendPacketToClient(inspectHonorStatsResultClassic);
		}
		else
		{
			InspectHonorStatsResultTBC inspectHonorStatsResultTBC = new InspectHonorStatsResultTBC();
			inspectHonorStatsResultTBC.PlayerGUID = playerGUID;
			inspectHonorStatsResultTBC.LifetimeHighestRank = lifetimeHighestRank;
			inspectHonorStatsResultTBC.YesterdayHonorableKills = yesterdayHonorableKills;
			inspectHonorStatsResultTBC.LifetimeHonorableKills = (ushort)num;
			SendPacketToClient(inspectHonorStatsResultTBC);
		}
	}

	[PacketHandler(Opcode.MSG_INSPECT_HONOR_STATS, ClientVersionBuild.V2_0_1_6180)]
	private void HandleInspectHonorStatsTBC(WorldPacket packet)
	{
		WowGuid128 playerGUID = packet.ReadGuid().To128(GetSession().GameState);
		byte lifetimeHighestRank = packet.ReadUInt8();
		ushort todayHonorableKills = packet.ReadUInt16();
		ushort yesterdayHonorableKills = packet.ReadUInt16();
		uint lastWeekHonor = packet.ReadUInt32();
		uint yesterdayHonor = packet.ReadUInt32();
		uint num = packet.ReadUInt32();
		if (ModernVersion.ExpansionVersion == 1)
		{
			InspectHonorStatsResultClassic inspectHonorStatsResultClassic = new InspectHonorStatsResultClassic();
			inspectHonorStatsResultClassic.PlayerGUID = playerGUID;
			inspectHonorStatsResultClassic.LifetimeHighestRank = lifetimeHighestRank;
			inspectHonorStatsResultClassic.TodayHonorableKills = todayHonorableKills;
			inspectHonorStatsResultClassic.YesterdayHonorableKills = yesterdayHonorableKills;
			inspectHonorStatsResultClassic.LifetimeHonorableKills = num;
			inspectHonorStatsResultClassic.YesterdayHonor = yesterdayHonor;
			inspectHonorStatsResultClassic.LastWeekHonor = lastWeekHonor;
			SendPacketToClient(inspectHonorStatsResultClassic);
		}
		else
		{
			InspectHonorStatsResultTBC inspectHonorStatsResultTBC = new InspectHonorStatsResultTBC();
			inspectHonorStatsResultTBC.PlayerGUID = playerGUID;
			inspectHonorStatsResultTBC.LifetimeHighestRank = lifetimeHighestRank;
			inspectHonorStatsResultTBC.YesterdayHonorableKills = yesterdayHonorableKills;
			inspectHonorStatsResultTBC.LifetimeHonorableKills = (ushort)num;
			SendPacketToClient(inspectHonorStatsResultTBC);
		}
	}

	[PacketHandler(Opcode.MSG_INSPECT_ARENA_TEAMS)]
	private void HandleInspectArenaTeams(WorldPacket packet)
	{
		InspectPvP inspectPvP = new InspectPvP();
		inspectPvP.PlayerGUID = packet.ReadGuid().To128(GetSession().GameState);
		ArenaTeamInspectData arenaTeamInspectData = new ArenaTeamInspectData();
		byte slot = packet.ReadUInt8();
		uint num = packet.ReadUInt32();
		arenaTeamInspectData.TeamGuid = WowGuid128.Create(HighGuidType703.ArenaTeam, num);
		arenaTeamInspectData.TeamRating = packet.ReadInt32();
		arenaTeamInspectData.TeamGamesPlayed = packet.ReadInt32();
		arenaTeamInspectData.TeamGamesWon = packet.ReadInt32();
		arenaTeamInspectData.PersonalGamesPlayed = packet.ReadInt32();
		arenaTeamInspectData.PersonalRating = packet.ReadInt32();
		GetSession().GameState.StoreArenaTeamDataForPlayer(inspectPvP.PlayerGUID, slot, arenaTeamInspectData);
		for (byte b = 0; b < 3; b++)
		{
			inspectPvP.ArenaTeams.Add(GetSession().GameState.GetArenaTeamDataForPlayer(inspectPvP.PlayerGUID, slot));
		}
		SendPacketToClient(inspectPvP);
	}

    //处理角色重命名结果
    [PacketHandler(Opcode.SMSG_CHARACTER_RENAME_RESULT)]
	private void HandleCharacterRenameResult(WorldPacket packet)
	{
		byte legacyValue = packet.ReadUInt8();
		CharacterRenameResult characterRenameResult = new CharacterRenameResult();
		characterRenameResult.Result = ModernVersion.ConvertResponseCodesValue(legacyValue);
		if (characterRenameResult.Result == 0)
		{
			characterRenameResult.Guid = packet.ReadGuid().To128(GetSession().GameState);
			characterRenameResult.Name = packet.ReadCString();
		}
		SendPacketToClient(characterRenameResult);
	}

    // 处理通道通知
    [PacketHandler(Opcode.SMSG_CHANNEL_NOTIFY)]
	private void HandleChannelNotify(WorldPacket packet)
	{
		ChatNotify chatNotify = (ChatNotify)packet.ReadUInt8();
		if (chatNotify == ChatNotify.InvalidName)
		{
			packet.ReadBytes(3u);
		}
		string text = packet.ReadCString();
		switch (chatNotify)
		{
		case ChatNotify.Joined:
		case ChatNotify.Left:
		case ChatNotify.PasswordChanged:
		case ChatNotify.OwnerChanged:
		case ChatNotify.AnnouncementsOn:
		case ChatNotify.AnnouncementsOff:
		case ChatNotify.ModerationOn:
		case ChatNotify.ModerationOff:
		case ChatNotify.PlayerAlreadyMember:
		case ChatNotify.Invite:
		case ChatNotify.VoiceOn:
		case ChatNotify.VoiceOff:
			packet.ReadGuid();
			break;
		case ChatNotify.YouJoined:
		{
			ChannelFlags channelFlags = (ChannelFlags)((!LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180)) ? packet.ReadUInt32() : packet.ReadUInt8());
			int num = packet.ReadInt32();
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
			{
				packet.ReadInt32();
			}
			if (num == 0)
			{
				num = (int)GameData.GetChatChannelIdFromName(text);
			}
			GetSession().GameState.SetChannelId(text, num);
			ChannelNotifyJoined channelNotifyJoined = new ChannelNotifyJoined();
			channelNotifyJoined.Channel = text;
			channelNotifyJoined.ChannelFlags = channelFlags;
			channelNotifyJoined.ChatChannelID = num;
			channelNotifyJoined.ChannelGUID = WowGuid128.Create(HighGuidType703.ChatChannel, GetSession().GameState.CurrentMapId.Value, GetSession().GameState.CurrentZoneId, (ulong)num);
			SendPacketToClient(channelNotifyJoined);
			break;
		}
		case ChatNotify.YouLeft:
		{
			ChannelNotifyLeft channelNotifyLeft = new ChannelNotifyLeft();
			channelNotifyLeft.Channel = text;
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
			{
				channelNotifyLeft.ChatChannelID = packet.ReadInt32();
				channelNotifyLeft.Suspended = packet.ReadBool();
			}
			else
			{
				channelNotifyLeft.ChatChannelID = GetSession().GameState.ChannelIds[text];
				channelNotifyLeft.Suspended = false;
			}
			if (string.Equals(GetSession().GameState.LeftChannelName, text) || GameData.GetChatChannelIdFromName(text) == 0)
			{
				SendPacketToClient(channelNotifyLeft);
			}
			break;
		}
		case ChatNotify.PlayerNotFound:
		case ChatNotify.ChannelOwner:
		case ChatNotify.PlayerNotBanned:
		case ChatNotify.PlayerInvited:
		case ChatNotify.PlayerInviteBanned:
			packet.ReadCString();
			break;
		case ChatNotify.ModeChange:
			packet.ReadGuid();
			packet.ReadUInt8();
			packet.ReadUInt8();
			break;
		case ChatNotify.PlayerKicked:
		case ChatNotify.PlayerBanned:
		case ChatNotify.PlayerUnbanned:
			packet.ReadGuid();
			packet.ReadGuid();
			break;
		case ChatNotify.TrialRestricted:
			packet.ReadGuid();
			break;
		case ChatNotify.WrongPassword:
		case ChatNotify.NotMember:
		case ChatNotify.NotModerator:
		case ChatNotify.NotOwner:
		case ChatNotify.Muted:
		case ChatNotify.Banned:
		case ChatNotify.InviteWrongFaction:
		case ChatNotify.WrongFaction:
		case ChatNotify.InvalidName:
		case ChatNotify.NotModerated:
		case ChatNotify.Throttled:
		case ChatNotify.NotInArea:
		case ChatNotify.NotInLfg:
			break;
		}
	}

    // 处理频道列表
    [PacketHandler(Opcode.SMSG_CHANNEL_LIST)]
	private void HandleChannelList(WorldPacket packet)
	{
		ChannelListResponse channelListResponse = new ChannelListResponse();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			channelListResponse.Display = packet.ReadBool();
		}
		else
		{
			channelListResponse.Display = GetSession().GameState.ChannelDisplayList;
		}
		channelListResponse.ChannelName = packet.ReadCString();
		channelListResponse.ChannelFlags = (ChannelFlags)packet.ReadUInt8();
		int num = packet.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			ChannelListResponse.ChannelPlayer channelPlayer = default(ChannelListResponse.ChannelPlayer);
			channelPlayer.Guid = packet.ReadGuid().To128(GetSession().GameState);
			channelPlayer.VirtualRealmAddress = GetSession().RealmId.GetAddress();
			channelPlayer.Flags = packet.ReadUInt8();
			channelListResponse.Members.Add(channelPlayer);
		}
		SendPacketToClient(channelListResponse);
	}

    // 处理服务器聊天消息 香草
    [PacketHandler(Opcode.SMSG_CHAT, ClientVersionBuild.Zero, ClientVersionBuild.V2_0_1_6180)]
	private void HandleServerChatMessageVanilla(WorldPacket packet)
	{
		ChatMessageTypeVanilla chatMessageTypeVanilla = (ChatMessageTypeVanilla)packet.ReadUInt8();
		uint language = packet.ReadUInt32();
		string senderName = "";
		WowGuid128 lhs = null;
		WowGuid128 rhs = null;
		string channelName = "";
		switch (chatMessageTypeVanilla)
		{
		case ChatMessageTypeVanilla.MonsterEmote:
		case ChatMessageTypeVanilla.MonsterWhisper:
		case ChatMessageTypeVanilla.RaidBossEmote:
			packet.ReadUInt32();
			senderName = packet.ReadCString();
			rhs = packet.ReadGuid().To128(GetSession().GameState);
			break;
		case ChatMessageTypeVanilla.Say:
		case ChatMessageTypeVanilla.Party:
		case ChatMessageTypeVanilla.Yell:
			lhs = packet.ReadGuid().To128(GetSession().GameState);
			packet.ReadGuid();
			break;
		case ChatMessageTypeVanilla.MonsterSay:
		case ChatMessageTypeVanilla.MonsterYell:
			lhs = packet.ReadGuid().To128(GetSession().GameState);
			packet.ReadUInt32();
			senderName = packet.ReadCString();
			rhs = packet.ReadGuid().To128(GetSession().GameState);
			break;
		case ChatMessageTypeVanilla.Channel:
			channelName = packet.ReadCString();
			packet.ReadUInt32();
			lhs = packet.ReadGuid().To128(GetSession().GameState);
			break;
		default:
			lhs = packet.ReadGuid().To128(GetSession().GameState);
			break;
		}
		if (chatMessageTypeVanilla - 83 <= ChatMessageTypeVanilla.Party)
		{
			Utility.Swap(ref lhs, ref rhs);
		}
		uint length = packet.ReadUInt32();
		string text = packet.ReadString(length);
		ChatTag chatTag = (ChatTag)packet.ReadUInt8();
		ChatFlags chatFlags = (ChatFlags)Enum.Parse(typeof(ChatFlags), chatTag.ToString());
		if (Session.GameState.IgnoredPlayers.Contains(lhs) && !chatFlags.HasFlag(ChatFlags.GM))
		{
			switch (chatMessageTypeVanilla)
			{
			case ChatMessageTypeVanilla.Whisper:
			{
				WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_CHAT_REPORT_IGNORED);
				worldPacket.WriteGuid(lhs.To64());
				SendPacketToServer(worldPacket);
				return;
			}
			default:
				return;
			case ChatMessageTypeVanilla.Ignored:
				break;
			}
		}
		string addonPrefix = "";
		if (ChatPkt.CheckAddonPrefix(GetSession().GameState.AddonPrefixes, ref language, ref text, ref addonPrefix))
		{
			ChatMessageTypeModern chatType = (ChatMessageTypeModern)Enum.Parse(typeof(ChatMessageTypeModern), chatMessageTypeVanilla.ToString());
			ChatPkt packet2 = new ChatPkt(GetSession(), chatType, text, language, lhs, senderName, rhs, "", channelName, chatFlags, addonPrefix);
			SendPacketToClient(packet2);
		}
	}

	[PacketHandler(Opcode.SMSG_CHAT, ClientVersionBuild.V2_0_1_6180)]
	[PacketHandler(Opcode.SMSG_GM_MESSAGECHAT, ClientVersionBuild.V2_0_1_6180)]
	private void HandleServerChatMessageWotLK(WorldPacket packet)
	{
		ChatMessageTypeWotLK chatMessageTypeWotLK = (ChatMessageTypeWotLK)packet.ReadUInt8();
		uint language = packet.ReadUInt32();
		WowGuid128 lhs = packet.ReadGuid().To128(GetSession().GameState);
		string senderName = "";
		string receiverName = "";
		string channelName = "";
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_1_0_6692))
		{
			packet.ReadInt32();
		}
		WowGuid128 rhs;
		switch (chatMessageTypeWotLK)
		{
		case ChatMessageTypeWotLK.Achievement:
		case ChatMessageTypeWotLK.GuildAchievement:
			rhs = packet.ReadGuid().To128(GetSession().GameState);
			break;
		case ChatMessageTypeWotLK.WhisperForeign:
		{
			uint length5 = packet.ReadUInt32();
			senderName = packet.ReadString(length5);
			rhs = packet.ReadGuid().To128(GetSession().GameState);
			break;
		}
		case ChatMessageTypeWotLK.BattlegroundNeutral:
		case ChatMessageTypeWotLK.BattlegroundAlliance:
		case ChatMessageTypeWotLK.BattlegroundHorde:
		{
			rhs = packet.ReadGuid().To128(GetSession().GameState);
			HighGuidType highType = rhs.GetHighType();
			if (highType == HighGuidType.Transport || (uint)(highType - 9) <= 3u)
			{
				uint length4 = packet.ReadUInt32();
				senderName = packet.ReadString(length4);
			}
			break;
		}
		case ChatMessageTypeWotLK.MonsterSay:
		case ChatMessageTypeWotLK.MonsterParty:
		case ChatMessageTypeWotLK.MonsterYell:
		case ChatMessageTypeWotLK.MonsterWhisper:
		case ChatMessageTypeWotLK.MonsterEmote:
		case ChatMessageTypeWotLK.RaidBossEmote:
		case ChatMessageTypeWotLK.RaidBossWhisper:
		case ChatMessageTypeWotLK.BattleNet:
		{
			uint length2 = packet.ReadUInt32();
			senderName = packet.ReadString(length2);
			rhs = packet.ReadGuid().To128(GetSession().GameState);
			switch (rhs.GetHighType())
			{
			case HighGuidType.Transport:
			case HighGuidType.Creature:
			case HighGuidType.Vehicle:
			case HighGuidType.GameObject:
			{
				uint length3 = packet.ReadUInt32();
				receiverName = packet.ReadString(length3);
				break;
			}
			}
			break;
		}
		default:
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056) && packet.GetUniversalOpcode(isModern: false) == Opcode.SMSG_GM_MESSAGECHAT)
			{
				uint length = packet.ReadUInt32();
				packet.ReadString(length);
			}
			if (chatMessageTypeWotLK == ChatMessageTypeWotLK.Channel)
			{
				channelName = packet.ReadCString();
			}
			rhs = packet.ReadGuid().To128(GetSession().GameState);
			break;
		}
		if (chatMessageTypeWotLK - 37 <= ChatMessageTypeWotLK.Say)
		{
			Utility.Swap(ref lhs, ref rhs);
		}
		uint length6 = packet.ReadUInt32();
		string text = packet.ReadString(length6);
		ChatFlags chatFlags = (ChatFlags)packet.ReadUInt8();
		if (LegacyVersion.InVersion(ClientVersionBuild.V2_0_1_6180, ClientVersionBuild.V3_0_2_9056) && packet.GetUniversalOpcode(isModern: false) == Opcode.SMSG_GM_MESSAGECHAT)
		{
			uint length7 = packet.ReadUInt32();
			packet.ReadString(length7);
		}
		uint achievementId = 0u;
		if (chatMessageTypeWotLK == ChatMessageTypeWotLK.Achievement || chatMessageTypeWotLK == ChatMessageTypeWotLK.GuildAchievement)
		{
			achievementId = packet.ReadUInt32();
		}
		if (Session.GameState.IgnoredPlayers.Contains(lhs) && !chatFlags.HasFlag(ChatFlags.GM))
		{
			switch (chatMessageTypeWotLK)
			{
			case ChatMessageTypeWotLK.Whisper:
			{
				WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_CHAT_REPORT_IGNORED);
				worldPacket.WriteGuid(lhs.To64());
				worldPacket.WriteUInt8(0);
				SendPacketToServer(worldPacket);
				return;
			}
			default:
				return;
			case ChatMessageTypeWotLK.Ignored:
				break;
			}
		}
		string addonPrefix = "";
		if (ChatPkt.CheckAddonPrefix(GetSession().GameState.AddonPrefixes, ref language, ref text, ref addonPrefix))
		{
			ChatMessageTypeModern chatType = (ChatMessageTypeModern)Enum.Parse(typeof(ChatMessageTypeModern), chatMessageTypeWotLK.ToString());
			ChatPkt packet2 = new ChatPkt(GetSession(), chatType, text, language, lhs, senderName, rhs, receiverName, channelName, chatFlags, addonPrefix, achievementId);
			SendPacketToClient(packet2);
		}
	}

	// 发送字符消息 香草
	public void SendMessageChatVanilla(ChatMessageTypeVanilla type, uint lang, string msg, string channel, string to)
	{
		if (!HandleHermesInternalChatCommand(msg))
		{
			WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_MESSAGECHAT);
			worldPacket.WriteUInt32((uint)type);
			worldPacket.WriteUInt32(lang);
			switch (type)
			{
			case ChatMessageTypeVanilla.Channel:
				worldPacket.WriteCString(channel);
				worldPacket.WriteCString(msg);
				break;
			case ChatMessageTypeVanilla.Whisper:
				worldPacket.WriteCString(to);
				worldPacket.WriteCString(msg);
				break;
			case ChatMessageTypeVanilla.Say:
			case ChatMessageTypeVanilla.Party:
			case ChatMessageTypeVanilla.Raid:
			case ChatMessageTypeVanilla.Guild:
			case ChatMessageTypeVanilla.Officer:
			case ChatMessageTypeVanilla.Yell:
			case ChatMessageTypeVanilla.Emote:
			case ChatMessageTypeVanilla.Afk:
			case ChatMessageTypeVanilla.Dnd:
			case ChatMessageTypeVanilla.RaidLeader:
			case ChatMessageTypeVanilla.RaidWarning:
			case ChatMessageTypeVanilla.Battleground:
			case ChatMessageTypeVanilla.BattlegroundLeader:
				worldPacket.WriteCString(msg);
				break;
			}
			SendPacket(worldPacket);
		}
	}

	private bool HandleHermesInternalChatCommand(string msg)
	{
		if (msg.StartsWith("!qcomplete"))
		{
			string text = msg.Remove(0, "!qcomplete".Length);
			if (!uint.TryParse(text, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out var result))
			{
				GetSession().SendHermesTextMessage("Chat command invalid questId format '" + text + "'");
				return true;
			}
			GetSession().GameState.CurrentPlayerStorage.CompletedQuests.MarkQuestAsCompleted(result);
			return true;
		}
		if (msg.StartsWith("!quncomplete"))
		{
			string text2 = msg.Remove(0, "!quncomplete".Length);
			if (!uint.TryParse(text2, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out var result2))
			{
				GetSession().SendHermesTextMessage("Chat command invalid questId format '" + text2 + "'");
				return true;
			}
			GetSession().GameState.CurrentPlayerStorage.CompletedQuests.MarkQuestAsNotCompleted(result2);
			return true;
		}
		return false;
	}

	public void SendMessageChatWotLK(ChatMessageTypeWotLK type, uint lang, string msg, string channel, string to)
	{
		if (!HandleHermesInternalChatCommand(msg))
		{
			WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_MESSAGECHAT);
			worldPacket.WriteUInt32((uint)type);
			worldPacket.WriteUInt32(lang);
			switch (type)
			{
			case ChatMessageTypeWotLK.Channel:
				worldPacket.WriteCString(channel);
				worldPacket.WriteCString(msg);
				break;
			case ChatMessageTypeWotLK.Whisper:
				worldPacket.WriteCString(to);
				worldPacket.WriteCString(msg);
				break;
			case ChatMessageTypeWotLK.Say:
			case ChatMessageTypeWotLK.Party:
			case ChatMessageTypeWotLK.Raid:
			case ChatMessageTypeWotLK.Guild:
			case ChatMessageTypeWotLK.Officer:
			case ChatMessageTypeWotLK.Yell:
			case ChatMessageTypeWotLK.Emote:
			case ChatMessageTypeWotLK.Afk:
			case ChatMessageTypeWotLK.Dnd:
			case ChatMessageTypeWotLK.RaidLeader:
			case ChatMessageTypeWotLK.RaidWarning:
			case ChatMessageTypeWotLK.Battleground:
			case ChatMessageTypeWotLK.BattlegroundLeader:
			case ChatMessageTypeWotLK.PartyLeader:
				worldPacket.WriteCString(msg);
				break;
			}
			SendPacket(worldPacket);
		}
	}

    //处理表情
    [PacketHandler(Opcode.SMSG_EMOTE)]
	private void HandleEmote(WorldPacket packet)
	{
		EmoteMessage emoteMessage = new EmoteMessage();
		emoteMessage.EmoteID = packet.ReadUInt32();
		emoteMessage.Guid = packet.ReadGuid().To128(GetSession().GameState);
		SendPacketToClient(emoteMessage);
	}

	// 处理文字表情
	[PacketHandler(Opcode.SMSG_TEXT_EMOTE)]
	private void HandleTextEmote(WorldPacket packet)
	{
		STextEmote sTextEmote = new STextEmote();
		sTextEmote.SourceGUID = packet.ReadGuid().To128(GetSession().GameState);
		sTextEmote.SourceAccountGUID = GetSession().GetGameAccountGuidForPlayer(sTextEmote.SourceGUID);
		sTextEmote.EmoteID = packet.ReadInt32();
		sTextEmote.SoundIndex = packet.ReadInt32();
		uint length = packet.ReadUInt32();
		string name = packet.ReadString(length);
		WowGuid128 playerGuidByName = GetSession().GameState.GetPlayerGuidByName(name);
		sTextEmote.TargetGUID = ((playerGuidByName != null) ? playerGuidByName : WowGuid128.Empty);
		SendPacketToClient(sTextEmote);
	}

    //处理打印通知
    [PacketHandler(Opcode.SMSG_PRINT_NOTIFICATION)]
	private void HandlePrintNotification(WorldPacket packet)
	{
		PrintNotification printNotification = new PrintNotification();
		printNotification.NotifyText = packet.ReadCString();
		SendPacketToClient(printNotification);
	}

    //处理未找到的聊天玩家
    [PacketHandler(Opcode.SMSG_CHAT_PLAYER_NOTFOUND)]
	private void HandleChatPlayerNotFound(WorldPacket packet)
	{
		ChatPlayerNotfound chatPlayerNotfound = new ChatPlayerNotfound();
		chatPlayerNotfound.Name = packet.ReadCString();
		SendPacketToClient(chatPlayerNotfound);
	}

    // 处理防御消息
    [PacketHandler(Opcode.SMSG_DEFENSE_MESSAGE)]
	private void HandleDefenseMessage(WorldPacket packet)
	{
		DefenseMessage defenseMessage = new DefenseMessage();
		defenseMessage.ZoneID = packet.ReadUInt32();
		packet.ReadUInt32();
		defenseMessage.MessageText = packet.ReadCString();
		SendPacketToClient(defenseMessage);
	}

    // 处理聊天服务器消息
    [PacketHandler(Opcode.SMSG_CHAT_SERVER_MESSAGE)]
	private void HandleChatServerMessage(WorldPacket packet)
	{
		ChatServerMessage chatServerMessage = new ChatServerMessage();
		chatServerMessage.MessageID = packet.ReadInt32();
		chatServerMessage.StringParam = packet.ReadCString();
		SendPacketToClient(chatServerMessage);
	}

    // 发送聊天加入频道
    public void SendChatJoinChannel(int channelId, string channelName, string password)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_CHAT_JOIN_CHANNEL);
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			worldPacket.WriteInt32(channelId);
			worldPacket.WriteUInt8(0);
			worldPacket.WriteUInt8(0);
		}
		worldPacket.WriteCString(channelName);
		worldPacket.WriteCString(password);
		SendPacketToServer(worldPacket);
	}

    //发送聊天 离开频道

    public void SendChatLeaveChannel(int channelId, string channelName)
	{
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_CHAT_LEAVE_CHANNEL);
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			worldPacket.WriteInt32(channelId);
		}
		worldPacket.WriteCString(channelName);
		SendPacketToServer(worldPacket);
	}

    // 处理攻击开始
    [PacketHandler(Opcode.SMSG_ATTACK_START)]
	private void HandleAttackStart(WorldPacket packet)
	{
		SAttackStart sAttackStart = new SAttackStart();
		sAttackStart.Attacker = packet.ReadGuid().To128(GetSession().GameState);
		sAttackStart.Victim = packet.ReadGuid().To128(GetSession().GameState);
		SendPacketToClient(sAttackStart);
	}

	// 处理攻击结束
	[PacketHandler(Opcode.SMSG_ATTACK_STOP)]
	private void HandleAttackStop(WorldPacket packet)
	{
		SAttackStop sAttackStop = new SAttackStop();
		sAttackStop.Attacker = packet.ReadPackedGuid().To128(GetSession().GameState);
		sAttackStop.Victim = packet.ReadPackedGuid().To128(GetSession().GameState);
		sAttackStop.NowDead = packet.ReadUInt32() != 0;
		SendPacketToClient(sAttackStop);
	}

	// 处理攻击更新
	[PacketHandler(Opcode.SMSG_ATTACKER_STATE_UPDATE)]
	private void HandleAttackerStateUpdate(WorldPacket packet)
	{
		AttackerStateUpdate attackerStateUpdate = new AttackerStateUpdate();
		uint num = packet.ReadUInt32();
		attackerStateUpdate.HitInfo = LegacyVersion.ConvertHitInfoFlags(num);
		attackerStateUpdate.AttackerGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
		attackerStateUpdate.VictimGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
		attackerStateUpdate.Damage = packet.ReadInt32();
		attackerStateUpdate.OriginalDamage = attackerStateUpdate.Damage;
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_3_9183))
		{
			attackerStateUpdate.OverDamage = packet.ReadInt32();
		}
		else
		{
			attackerStateUpdate.OverDamage = -1;
		}
		byte b = packet.ReadUInt8();
		for (int i = 0; i < b; i++)
		{
			SubDamage subDamage = new SubDamage();
			uint num2 = packet.ReadUInt32();
			if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
			{
				num2 = (uint)(1 << (int)(byte)num2);
			}
			subDamage.SchoolMask = num2;
			subDamage.FloatDamage = packet.ReadFloat();
			subDamage.IntDamage = packet.ReadInt32();
			if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_3_9183) || num.HasAnyFlag(HitInfo.FullAbsorb | HitInfo.PartialAbsorb))
			{
				subDamage.Absorbed = packet.ReadInt32();
			}
			if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_3_9183) || num.HasAnyFlag(HitInfo.FullResist | HitInfo.PartialResist))
			{
				subDamage.Resisted = packet.ReadInt32();
			}
			attackerStateUpdate.SubDmg.Add(subDamage);
		}
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_3_9183))
		{
			attackerStateUpdate.VictimState = packet.ReadUInt8();
		}
		else
		{
			attackerStateUpdate.VictimState = (byte)packet.ReadUInt32();
		}
		attackerStateUpdate.AttackerState = packet.ReadInt32();
		attackerStateUpdate.MeleeSpellID = packet.ReadUInt32();
		if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_3_9183) || num.HasAnyFlag(HitInfo.Block))
		{
			attackerStateUpdate.BlockAmount = packet.ReadInt32();
		}
		if (num.HasAnyFlag(HitInfo.RageGain))
		{
			attackerStateUpdate.RageGained = packet.ReadInt32();
		}
		if (num.HasAnyFlag(HitInfo.Unk0))
		{
			attackerStateUpdate.UnkState = default(UnkAttackerState);
			attackerStateUpdate.UnkState.State1 = packet.ReadUInt32();
			attackerStateUpdate.UnkState.State2 = packet.ReadFloat();
			attackerStateUpdate.UnkState.State3 = packet.ReadFloat();
			attackerStateUpdate.UnkState.State4 = packet.ReadFloat();
			attackerStateUpdate.UnkState.State5 = packet.ReadFloat();
			attackerStateUpdate.UnkState.State6 = packet.ReadFloat();
			attackerStateUpdate.UnkState.State7 = packet.ReadFloat();
			attackerStateUpdate.UnkState.State8 = packet.ReadFloat();
			attackerStateUpdate.UnkState.State9 = packet.ReadFloat();
			attackerStateUpdate.UnkState.State10 = packet.ReadFloat();
			attackerStateUpdate.UnkState.State11 = packet.ReadFloat();
			attackerStateUpdate.UnkState.State12 = packet.ReadUInt32();
			packet.ReadUInt32();
			packet.ReadUInt32();
		}
		SendPacketToClient(attackerStateUpdate);
	}

    // 处理攻击挥杆不在范围内
    [PacketHandler(Opcode.SMSG_ATTACKSWING_NOTINRANGE)]
	private void HandleAttackSwingNotInRange(WorldPacket packet)
	{
		AttackSwingError attackSwingError = new AttackSwingError();
		attackSwingError.Reason = AttackSwingErr.NotInRange;
		SendPacketToClient(attackSwingError);
	}

	[PacketHandler(Opcode.SMSG_ATTACKSWING_BADFACING)]
	private void HandleAttackSwingBadFacing(WorldPacket packet)
	{
		AttackSwingError attackSwingError = new AttackSwingError();
		attackSwingError.Reason = AttackSwingErr.BadFacing;
		SendPacketToClient(attackSwingError);
	}


	[PacketHandler(Opcode.SMSG_ATTACKSWING_DEADTARGET)]
	private void HandleAttackSwingDeadTarget(WorldPacket packet)
	{
		AttackSwingError attackSwingError = new AttackSwingError();
		attackSwingError.Reason = AttackSwingErr.DeadTarget;
		SendPacketToClient(attackSwingError);
	}

	[PacketHandler(Opcode.SMSG_ATTACKSWING_CANT_ATTACK)]
	private void HandleAttackSwingCantAttack(WorldPacket packet)
	{
		AttackSwingError attackSwingError = new AttackSwingError();
		attackSwingError.Reason = AttackSwingErr.CantAttack;
		SendPacketToClient(attackSwingError);
	}

    //处理取消战斗
    [PacketHandler(Opcode.SMSG_CANCEL_COMBAT)]
	private void HandleCancelCombat(WorldPacket packet)
	{
		CancelCombat packet2 = new CancelCombat();
		SendPacketToClient(packet2);
	}

    // 处理人工智能反应
    [PacketHandler(Opcode.SMSG_AI_REACTION)]
	private void HandleAIReaction(WorldPacket packet)
	{
		AIReaction aIReaction = new AIReaction();
		aIReaction.UnitGUID = packet.ReadGuid().To128(GetSession().GameState);
		aIReaction.Reaction = packet.ReadUInt32();
		SendPacketToClient(aIReaction);
	}

    // 处理派对杀戮日志
    [PacketHandler(Opcode.SMSG_PARTY_KILL_LOG)]
	private void HandlePartyKillLog(WorldPacket packet)
	{
		PartyKillLog partyKillLog = new PartyKillLog();
		partyKillLog.Player = packet.ReadGuid().To128(GetSession().GameState);
		partyKillLog.Victim = packet.ReadGuid().To128(GetSession().GameState);
		SendPacketToClient(partyKillLog);
	}

	[PacketHandler(Opcode.SMSG_DUEL_REQUESTED)]
	private void HandleDuelRequested(WorldPacket packet)
	{
		DuelRequested duelRequested = new DuelRequested();
		duelRequested.ArbiterGUID = packet.ReadGuid().To128(GetSession().GameState);
		duelRequested.RequestedByGUID = packet.ReadGuid().To128(GetSession().GameState);
		duelRequested.RequestedByWowAccount = GetSession().GetGameAccountGuidForPlayer(duelRequested.RequestedByGUID);
		SendPacketToClient(duelRequested);
	}

	[PacketHandler(Opcode.SMSG_DUEL_COUNTDOWN)]
	private void HandleDuelCountdown(WorldPacket packet)
	{
		DuelCountdown duelCountdown = new DuelCountdown();
		duelCountdown.Countdown = packet.ReadUInt32();
		SendPacketToClient(duelCountdown);
	}

	[PacketHandler(Opcode.SMSG_DUEL_COMPLETE)]
	private void HandleDuelComplete(WorldPacket packet)
	{
		DuelComplete duelComplete = new DuelComplete();
		duelComplete.Started = packet.ReadBool();
		SendPacketToClient(duelComplete);
	}

	[PacketHandler(Opcode.SMSG_DUEL_WINNER)]
	private void HandleDuelWinner(WorldPacket packet)
	{
		DuelWinner duelWinner = new DuelWinner();
		duelWinner.Fled = packet.ReadBool();
		duelWinner.BeatenName = packet.ReadCString();
		duelWinner.WinnerName = packet.ReadCString();
		duelWinner.BeatenVirtualRealmAddress = GetSession().RealmId.GetAddress();
		duelWinner.WinnerVirtualRealmAddress = GetSession().RealmId.GetAddress();
		SendPacketToClient(duelWinner);
	}
	// 处理决斗越界
	[PacketHandler(Opcode.SMSG_DUEL_IN_BOUNDS)]
	private void HandleDuelInBounds(WorldPacket packet)
	{
		DuelInBounds packet2 = new DuelInBounds();
		SendPacketToClient(packet2);
	}

    // 处理决斗完成
    [PacketHandler(Opcode.SMSG_DUEL_OUT_OF_BOUNDS)]
	private void HandleDuelOutOfBounds(WorldPacket packet)
	{
		DuelOutOfBounds packet2 = new DuelOutOfBounds();
		SendPacketToClient(packet2);
	}

    // 处理游戏对象消失
    [PacketHandler(Opcode.SMSG_GAME_OBJECT_DESPAWN)]
	private void HandleGameObjectDespawn(WorldPacket packet)
	{
		WowGuid64 wowGuid = packet.ReadGuid();
		GameObjectDespawn gameObjectDespawn = new GameObjectDespawn();
		gameObjectDespawn.ObjectGUID = wowGuid.To128(GetSession().GameState);
		SendPacketToClient(gameObjectDespawn);
		GetSession().GameState.DespawnedGameObjects.Add(wowGuid);
	}

    //处理游戏对象重置状态
    [PacketHandler(Opcode.SMSG_GAME_OBJECT_RESET_STATE)]
	private void HandleGameObjectResetState(WorldPacket packet)
	{
		GameObjectResetState gameObjectResetState = new GameObjectResetState();
		gameObjectResetState.ObjectGUID = packet.ReadGuid().To128(GetSession().GameState);
		SendPacketToClient(gameObjectResetState);
	}

    //处理游戏对象自定义动画
    [PacketHandler(Opcode.SMSG_GAME_OBJECT_CUSTOM_ANIM)]
	private void HandleGameObjectCustomAnim(WorldPacket packet)
	{
		GameObjectCustomAnim gameObjectCustomAnim = new GameObjectCustomAnim();
		gameObjectCustomAnim.ObjectGUID = packet.ReadGuid().To128(GetSession().GameState);
		gameObjectCustomAnim.CustomAnim = packet.ReadUInt32();
		SendPacketToClient(gameObjectCustomAnim);
	}

    //处理未上钩的鱼
    [PacketHandler(Opcode.SMSG_FISH_NOT_HOOKED)]
	private void HandleFishNotHooked(WorldPacket packet)
	{
		FishNotHooked packet2 = new FishNotHooked();
		SendPacketToClient(packet2);
	}

	[PacketHandler(Opcode.SMSG_FISH_ESCAPED)]
	private void HandleFishEscaped(WorldPacket packet)
	{
		FishEscaped packet2 = new FishEscaped();
		SendPacketToClient(packet2);
	}


	[PacketHandler(Opcode.SMSG_PARTY_COMMAND_RESULT)]
	private void HandlePartyCommandResult(WorldPacket packet)
	{
		PartyCommandResult partyCommandResult = new PartyCommandResult();
		partyCommandResult.Command = (byte)packet.ReadUInt32();
		partyCommandResult.Name = packet.ReadCString();
		uint num = packet.ReadUInt32();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			partyCommandResult.Result = (byte)num;
		}
		else
		{
			Type typeFromHandle = typeof(PartyResultModern);
			PartyResultVanilla partyResultVanilla = (PartyResultVanilla)num;
			partyCommandResult.Result = (byte)Enum.Parse(typeFromHandle, partyResultVanilla.ToString());
		}
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			partyCommandResult.ResultData = packet.ReadUInt32();
		}
		SendPacketToClient(partyCommandResult);
	}

	[PacketHandler(Opcode.SMSG_GROUP_DECLINE)]
	private void HandleGroupDecline(WorldPacket packet)
	{
		GroupDecline groupDecline = new GroupDecline();
		groupDecline.Name = packet.ReadCString();
		SendPacketToClient(groupDecline);
	}

    //处理群组邀请
    [PacketHandler(Opcode.SMSG_PARTY_INVITE)]
	private void HandleGroupInvite(WorldPacket packet)
	{
		PartyInvite partyInvite = new PartyInvite();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			partyInvite.CanAccept = packet.ReadBool();
		}
		Realm realm = GetSession().RealmManager.GetRealm(GetSession().RealmId);
		partyInvite.InviterRealm = new VirtualRealmInfo(realm.Id.GetAddress(), isHomeRealm: true, isInternalRealm: false, realm.Name, realm.NormalizedName);
		partyInvite.InviterName = packet.ReadCString();
		partyInvite.InviterGUID = GetSession().GameState.GetPlayerGuidByName(partyInvite.InviterName);
		if (partyInvite.InviterGUID == null)
		{
			partyInvite.InviterGUID = WowGuid128.Empty;
			partyInvite.InviterBNetAccountId = WowGuid128.Empty;
		}
		else
		{
			partyInvite.InviterBNetAccountId = GetSession().GetBnetAccountGuidForPlayer(partyInvite.InviterGUID);
		}
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			partyInvite.ProposedRoles = packet.ReadUInt32();
			byte b = packet.ReadUInt8();
			for (int i = 0; i < b; i++)
			{
				partyInvite.LfgSlots.Add(packet.ReadInt32());
			}
			partyInvite.LfgCompletedMask = packet.ReadInt32();
		}
		SendPacketToClient(partyInvite);
	}

    //处理组列表香草
    [PacketHandler(Opcode.SMSG_GROUP_LIST, ClientVersionBuild.Zero, ClientVersionBuild.V2_0_1_6180)]
	private void HandleGroupListVanilla(WorldPacket packet)
	{
		PartyUpdate partyUpdate = new PartyUpdate();
		partyUpdate.SequenceNum = GetSession().GameState.GroupUpdateCounter++;
		bool flag = packet.ReadBool();
		byte b = packet.ReadUInt8();
		partyUpdate.PartyIndex = (byte)((flag && GetSession().GameState.IsInBattleground()) ? 1u : 0u);
		partyUpdate.PartyGUID = WowGuid128.Create(HighGuidType703.Party, (ulong)(1000 + partyUpdate.PartyIndex));
		if (partyUpdate.PartyIndex != 0)
		{
			partyUpdate.PartyFlags |= GroupFlags.FakeRaid;
		}
		HashSet<WowGuid128> hashSet = new HashSet<WowGuid128>();
		uint num = packet.ReadUInt32();
		if (num != 0)
		{
			if (flag)
			{
				partyUpdate.PartyFlags |= GroupFlags.Raid;
			}
			partyUpdate.DifficultySettings = new PartyDifficultySettings();
			partyUpdate.DifficultySettings.DungeonDifficultyID = DifficultyModern.Normal;
			if (ModernVersion.ExpansionVersion > 1)
			{
				partyUpdate.DifficultySettings.RaidDifficultyID = DifficultyModern.Raid25N;
			}
			else
			{
				partyUpdate.DifficultySettings.RaidDifficultyID = DifficultyModern.Raid40;
			}
			if (partyUpdate.PartyIndex != 0)
			{
				partyUpdate.PartyType = GroupType.PvP;
			}
			else
			{
				partyUpdate.PartyType = GroupType.Normal;
			}
			PartyPlayerInfo partyPlayerInfo = default(PartyPlayerInfo);
			partyPlayerInfo.GUID = GetSession().GameState.CurrentPlayerGuid;
			partyPlayerInfo.Name = GetSession().GameState.GetPlayerName(partyPlayerInfo.GUID);
			partyPlayerInfo.Subgroup = (byte)(b & 0xFu);
			partyPlayerInfo.Flags = (((b & 0x80u) != 0) ? GroupMemberFlags.Assistant : GroupMemberFlags.None);
			partyPlayerInfo.Status = GroupMemberOnlineStatus.Online;
			partyUpdate.PlayerList.Add(partyPlayerInfo);
			bool flag2 = true;
			for (uint num2 = 0u; num2 < num; num2++)
			{
				PartyPlayerInfo partyPlayerInfo2 = default(PartyPlayerInfo);
				partyPlayerInfo2.Name = packet.ReadCString();
				partyPlayerInfo2.GUID = packet.ReadGuid().To128(GetSession().GameState);
				partyPlayerInfo2.Status = (GroupMemberOnlineStatus)packet.ReadUInt8();
				byte b2 = packet.ReadUInt8();
				partyPlayerInfo2.Subgroup = (byte)(b2 & 0xFu);
				partyPlayerInfo2.Flags = (((b2 & 0x80u) != 0) ? GroupMemberFlags.Assistant : GroupMemberFlags.None);
				partyPlayerInfo2.ClassId = GetSession().GameState.GetUnitClass(partyPlayerInfo2.GUID);
				if (!partyPlayerInfo2.Flags.HasAnyFlag(GroupMemberFlags.Assistant))
				{
					flag2 = false;
				}
				if (!hashSet.Contains(partyPlayerInfo2.GUID))
				{
					partyUpdate.PlayerList.Add(partyPlayerInfo2);
					hashSet.Add(partyPlayerInfo2.GUID);
				}
				Session.GameState.UpdatePlayerCache(partyPlayerInfo2.GUID, new PlayerCache
				{
					Name = partyPlayerInfo2.Name,
					ClassId = partyPlayerInfo2.ClassId
				});
			}
			if (flag2)
			{
				partyUpdate.PartyFlags |= GroupFlags.EveryoneAssistant;
			}
			partyUpdate.LeaderGUID = packet.ReadGuid().To128(GetSession().GameState);
			partyUpdate.LootSettings = new PartyLootSettings();
			partyUpdate.LootSettings.Method = (LootMethod)packet.ReadUInt8();
			partyUpdate.LootSettings.LootMaster = packet.ReadGuid().To128(GetSession().GameState);
			partyUpdate.LootSettings.Threshold = packet.ReadUInt8();
			GetSession().GameState.WeWantToLeaveGroup = false;
			GetSession().GameState.CurrentGroups[partyUpdate.PartyIndex] = partyUpdate;
		}
		else
		{
			partyUpdate.PartyFlags |= GroupFlags.Destroyed;
			if (partyUpdate.PartyIndex == 0)
			{
				partyUpdate.PartyGUID = WowGuid128.Empty;
			}
			partyUpdate.LeaderGUID = WowGuid128.Empty;
			partyUpdate.MyIndex = -1;
			GetSession().GameState.CurrentGroups[partyUpdate.PartyIndex] = null;
			if (!GetSession().GameState.WeWantToLeaveGroup)
			{
				SendPacketToClient(new GroupUninvite());
			}
		}
		SendPacketToClient(partyUpdate);
	}

	[PacketHandler(Opcode.SMSG_GROUP_LIST, ClientVersionBuild.V2_0_1_6180)]
	private void HandleGroupListTBC(WorldPacket packet)
	{
		PartyUpdate partyUpdate = new PartyUpdate();
		partyUpdate.SequenceNum = GetSession().GameState.GroupUpdateCounter++;
		bool flag = packet.ReadBool();
		bool flag2 = packet.ReadBool();
		byte subgroup = packet.ReadUInt8();
		byte flags = packet.ReadUInt8();
		partyUpdate.PartyIndex = (byte)(flag2 ? 1u : 0u);
		partyUpdate.PartyGUID = packet.ReadGuid().To128(GetSession().GameState);
		if (partyUpdate.PartyIndex != 0)
		{
			partyUpdate.PartyFlags |= GroupFlags.FakeRaid;
		}
		HashSet<WowGuid128> hashSet = new HashSet<WowGuid128>();
		uint num = packet.ReadUInt32();
		if (num != 0)
		{
			if (flag)
			{
				partyUpdate.PartyFlags |= GroupFlags.Raid;
			}
			if (partyUpdate.PartyIndex != 0)
			{
				partyUpdate.PartyType = GroupType.PvP;
			}
			else
			{
				partyUpdate.PartyType = GroupType.Normal;
			}
			PartyPlayerInfo partyPlayerInfo = default(PartyPlayerInfo);
			partyPlayerInfo.GUID = GetSession().GameState.CurrentPlayerGuid;
			partyPlayerInfo.Name = GetSession().GameState.GetPlayerName(partyPlayerInfo.GUID);
			partyPlayerInfo.Subgroup = subgroup;
			partyPlayerInfo.Flags = (GroupMemberFlags)flags;
			partyPlayerInfo.Status = GroupMemberOnlineStatus.Online;
			partyUpdate.PlayerList.Add(partyPlayerInfo);
			bool flag3 = true;
			for (uint num2 = 0u; num2 < num; num2++)
			{
				PartyPlayerInfo partyPlayerInfo2 = default(PartyPlayerInfo);
				partyPlayerInfo2.Name = packet.ReadCString();
				partyPlayerInfo2.GUID = packet.ReadGuid().To128(GetSession().GameState);
				partyPlayerInfo2.Status = (GroupMemberOnlineStatus)packet.ReadUInt8();
				partyPlayerInfo2.Subgroup = packet.ReadUInt8();
				partyPlayerInfo2.Flags = (GroupMemberFlags)packet.ReadUInt8();
				partyPlayerInfo2.ClassId = GetSession().GameState.GetUnitClass(partyPlayerInfo2.GUID);
				if (!partyPlayerInfo2.Flags.HasAnyFlag(GroupMemberFlags.Assistant))
				{
					flag3 = false;
				}
				if (!hashSet.Contains(partyPlayerInfo2.GUID))
				{
					partyUpdate.PlayerList.Add(partyPlayerInfo2);
					hashSet.Add(partyPlayerInfo2.GUID);
				}
				Session.GameState.UpdatePlayerCache(partyPlayerInfo2.GUID, new PlayerCache
				{
					Name = partyPlayerInfo2.Name,
					ClassId = partyPlayerInfo2.ClassId
				});
			}
			if (flag3)
			{
				partyUpdate.PartyFlags |= GroupFlags.EveryoneAssistant;
			}
			partyUpdate.LeaderGUID = packet.ReadGuid().To128(GetSession().GameState);
			partyUpdate.LootSettings = new PartyLootSettings();
			partyUpdate.LootSettings.Method = (LootMethod)packet.ReadUInt8();
			partyUpdate.LootSettings.LootMaster = packet.ReadGuid().To128(GetSession().GameState);
			partyUpdate.LootSettings.Threshold = packet.ReadUInt8();
			partyUpdate.DifficultySettings = new PartyDifficultySettings();
			int num3 = packet.ReadUInt8();
			partyUpdate.DifficultySettings.DungeonDifficultyID = (DifficultyModern)Enum.Parse(typeof(DifficultyModern), ((DifficultyLegacy)num3).ToString());
			if (ModernVersion.ExpansionVersion > 1)
			{
				partyUpdate.DifficultySettings.RaidDifficultyID = DifficultyModern.Raid25N;
			}
			else
			{
				partyUpdate.DifficultySettings.RaidDifficultyID = DifficultyModern.Raid40;
			}
			GetSession().GameState.WeWantToLeaveGroup = false;
			GetSession().GameState.CurrentGroups[partyUpdate.PartyIndex] = partyUpdate;
		}
		else
		{
			partyUpdate.PartyFlags |= GroupFlags.Destroyed;
			if (partyUpdate.PartyIndex == 0)
			{
				partyUpdate.PartyGUID = WowGuid128.Empty;
			}
			partyUpdate.LeaderGUID = WowGuid128.Empty;
			partyUpdate.MyIndex = -1;
			GetSession().GameState.CurrentGroups[partyUpdate.PartyIndex] = null;
			if (!GetSession().GameState.WeWantToLeaveGroup)
			{
				SendPacketToClient(new GroupUninvite());
			}
		}
		SendPacketToClient(partyUpdate);
	}

    // 处理群组取消邀请
    [PacketHandler(Opcode.SMSG_GROUP_UNINVITE)]
	private void HandleGroupUninvite(WorldPacket packet)
	{
		GroupUninvite packet2 = new GroupUninvite();
		SendPacketToClient(packet2);
	}

    // 处理集团新任领导者
    [PacketHandler(Opcode.SMSG_GROUP_NEW_LEADER)]
	private void HandleGroupNewLeader(WorldPacket packet)
	{
		GroupNewLeader groupNewLeader = new GroupNewLeader();
		groupNewLeader.Name = packet.ReadCString();
		groupNewLeader.PartyIndex = GetSession().GameState.GetCurrentPartyIndex();
		SendPacketToClient(groupNewLeader);
	}

	// 处理团队检查 香草
	[PacketHandler(Opcode.MSG_RAID_READY_CHECK, ClientVersionBuild.Zero, ClientVersionBuild.V2_0_1_6180)]
	private void HandleRaidReadyCheckVanilla(WorldPacket packet)
	{
		if (!packet.CanRead())
		{
			ReadyCheckStarted readyCheckStarted = new ReadyCheckStarted();
			readyCheckStarted.InitiatorGUID = GetSession().GameState.GetCurrentGroupLeader();
			readyCheckStarted.PartyIndex = GetSession().GameState.GetCurrentPartyIndex();
			readyCheckStarted.PartyGUID = GetSession().GameState.GetCurrentGroupGuid();
			SendPacketToClient(readyCheckStarted);
			return;
		}
		ReadyCheckResponse readyCheckResponse = new ReadyCheckResponse();
		readyCheckResponse.Player = packet.ReadGuid().To128(GetSession().GameState);
		readyCheckResponse.IsReady = packet.ReadBool();
		readyCheckResponse.PartyGUID = GetSession().GameState.GetCurrentGroupGuid();
		SendPacketToClient(readyCheckResponse);
		GetSession().GameState.GroupReadyCheckResponses++;
		if (GetSession().GameState.GroupReadyCheckResponses >= GetSession().GameState.GetCurrentGroupSize())
		{
			GetSession().GameState.GroupReadyCheckResponses = 0u;
			ReadyCheckCompleted readyCheckCompleted = new ReadyCheckCompleted();
			readyCheckCompleted.PartyIndex = GetSession().GameState.GetCurrentPartyIndex();
			readyCheckCompleted.PartyGUID = GetSession().GameState.GetCurrentGroupGuid();
			SendPacketToClient(readyCheckCompleted);
		}
	}

	[PacketHandler(Opcode.MSG_RAID_READY_CHECK, ClientVersionBuild.V2_0_1_6180)]
	private void HandleRaidReadyCheck(WorldPacket packet)
	{
		ReadyCheckStarted readyCheckStarted = new ReadyCheckStarted();
		readyCheckStarted.InitiatorGUID = packet.ReadGuid().To128(GetSession().GameState);
		readyCheckStarted.PartyIndex = GetSession().GameState.GetCurrentPartyIndex();
		readyCheckStarted.PartyGUID = GetSession().GameState.GetCurrentGroupGuid();
		SendPacketToClient(readyCheckStarted);
	}

	[PacketHandler(Opcode.MSG_RAID_READY_CHECK_CONFIRM, ClientVersionBuild.V2_0_1_6180)]
	private void HandleRaidReadyCheckConfirm(WorldPacket packet)
	{
		ReadyCheckResponse readyCheckResponse = new ReadyCheckResponse();
		readyCheckResponse.Player = packet.ReadGuid().To128(GetSession().GameState);
		readyCheckResponse.IsReady = packet.ReadBool();
		readyCheckResponse.PartyGUID = GetSession().GameState.GetCurrentGroupGuid();
		SendPacketToClient(readyCheckResponse);
		GetSession().GameState.GroupReadyCheckResponses++;
		if (GetSession().GameState.GroupReadyCheckResponses >= GetSession().GameState.GetCurrentGroupSize())
		{
			GetSession().GameState.GroupReadyCheckResponses = 0u;
			ReadyCheckCompleted readyCheckCompleted = new ReadyCheckCompleted();
			readyCheckCompleted.PartyIndex = GetSession().GameState.GetCurrentPartyIndex();
			readyCheckCompleted.PartyGUID = GetSession().GameState.GetCurrentGroupGuid();
			SendPacketToClient(readyCheckCompleted);
		}
	}

	[PacketHandler(Opcode.MSG_RAID_READY_CHECK_FINISHED, ClientVersionBuild.V2_0_1_6180)]
	private void HandleRaidReadyCheckFinished(WorldPacket packet)
	{
		ReadyCheckCompleted readyCheckCompleted = new ReadyCheckCompleted();
		readyCheckCompleted.PartyIndex = GetSession().GameState.GetCurrentPartyIndex();
		readyCheckCompleted.PartyGUID = GetSession().GameState.GetCurrentGroupGuid();
		SendPacketToClient(readyCheckCompleted);
	}

	// 处理团队目标更新
	[PacketHandler(Opcode.MSG_RAID_TARGET_UPDATE)]
	private void HandleRaidTargetUpdate(WorldPacket packet)
	{
		if (packet.ReadBool())
		{
			SendRaidTargetUpdateAll sendRaidTargetUpdateAll = new SendRaidTargetUpdateAll();
			sendRaidTargetUpdateAll.PartyIndex = GetSession().GameState.GetCurrentPartyIndex();
			while (packet.CanRead())
			{
				sbyte b = packet.ReadInt8();
				WowGuid128 wowGuid = packet.ReadGuid().To128(GetSession().GameState);
				sendRaidTargetUpdateAll.TargetIcons.Add(new Tuple<sbyte, WowGuid128>(b, wowGuid));
			}
			SendPacketToClient(sendRaidTargetUpdateAll);
			return;
		}
		SendRaidTargetUpdateSingle sendRaidTargetUpdateSingle = new SendRaidTargetUpdateSingle();
		sendRaidTargetUpdateSingle.PartyIndex = GetSession().GameState.GetCurrentPartyIndex();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			sendRaidTargetUpdateSingle.ChangedBy = packet.ReadGuid().To128(GetSession().GameState);
		}
		else
		{
			sendRaidTargetUpdateSingle.ChangedBy = GetSession().GameState.CurrentPlayerGuid;
		}
		sendRaidTargetUpdateSingle.Symbol = packet.ReadInt8();
		sendRaidTargetUpdateSingle.Target = packet.ReadGuid().To128(GetSession().GameState);
		SendPacketToClient(sendRaidTargetUpdateSingle);
	}

    //处理召唤请求
    [PacketHandler(Opcode.SMSG_SUMMON_REQUEST)]
	private void HandleSummonRequest(WorldPacket packet)
	{
		SummonRequest summonRequest = new SummonRequest();
		summonRequest.SummonerGUID = packet.ReadGuid().To128(GetSession().GameState);
		summonRequest.SummonerVirtualRealmAddress = GetSession().RealmId.GetAddress();
		summonRequest.AreaID = packet.ReadInt32();
		packet.ReadUInt32();
		SendPacketToClient(summonRequest);
	}

    // 处理党员统计数据
    [PacketHandler(Opcode.SMSG_PARTY_MEMBER_PARTIAL_STATE, ClientVersionBuild.Zero, ClientVersionBuild.V2_0_1_6180)]
	private void HandlePartyMemberStats(WorldPacket packet)
	{
		if (GetSession().GameState.CurrentMapId == 489 && (GetSession().GameState.HasWsgAllyFlagCarrier || GetSession().GameState.HasWsgHordeFlagCarrier) && _requestBgPlayerPosCounter++ > 10)
		{
			WorldPacket packet2 = new WorldPacket(Opcode.MSG_BATTLEGROUND_PLAYER_POSITIONS);
			SendPacket(packet2);
			_requestBgPlayerPosCounter = 0u;
		}
		PartyMemberPartialState partyMemberPartialState = new PartyMemberPartialState();
		partyMemberPartialState.AffectedGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
		GroupUpdateFlagVanilla groupUpdateFlagVanilla = (GroupUpdateFlagVanilla)packet.ReadUInt32();
		if (groupUpdateFlagVanilla.HasFlag(GroupUpdateFlagVanilla.Status))
		{
			partyMemberPartialState.StatusFlags = packet.ReadUInt8();
		}
		if (groupUpdateFlagVanilla.HasFlag(GroupUpdateFlagVanilla.CurrentHealth))
		{
			partyMemberPartialState.CurrentHealth = packet.ReadUInt16();
		}
		if (groupUpdateFlagVanilla.HasFlag(GroupUpdateFlagVanilla.MaxHealth))
		{
			partyMemberPartialState.MaxHealth = packet.ReadUInt16();
		}
		if (groupUpdateFlagVanilla.HasFlag(GroupUpdateFlagVanilla.PowerType))
		{
			partyMemberPartialState.PowerType = packet.ReadUInt8();
		}
		if (groupUpdateFlagVanilla.HasFlag(GroupUpdateFlagVanilla.CurrentPower))
		{
			partyMemberPartialState.CurrentPower = packet.ReadUInt16();
		}
		if (groupUpdateFlagVanilla.HasFlag(GroupUpdateFlagVanilla.MaxPower))
		{
			partyMemberPartialState.MaxPower = packet.ReadUInt16();
		}
		if (groupUpdateFlagVanilla.HasFlag(GroupUpdateFlagVanilla.Level))
		{
			partyMemberPartialState.Level = packet.ReadUInt16();
		}
		if (groupUpdateFlagVanilla.HasFlag(GroupUpdateFlagVanilla.Zone))
		{
			partyMemberPartialState.ZoneID = packet.ReadUInt16();
		}
		if (groupUpdateFlagVanilla.HasFlag(GroupUpdateFlagVanilla.Position))
		{
			partyMemberPartialState.Position = new PartyMemberPartialState.Vector3_UInt16();
			partyMemberPartialState.Position.X = packet.ReadInt16();
			partyMemberPartialState.Position.Y = packet.ReadInt16();
		}
		if (groupUpdateFlagVanilla.HasFlag(GroupUpdateFlagVanilla.Auras))
		{
			if (partyMemberPartialState.Auras == null)
			{
				partyMemberPartialState.Auras = new List<PartyMemberAuraStates>();
			}
			uint num = packet.ReadUInt32();
			byte b = 32;
			for (byte b2 = 0; b2 < b; b2++)
			{
				if ((num & (1L << (int)b2)) != 0L)
				{
					PartyMemberAuraStates partyMemberAuraStates = new PartyMemberAuraStates();
					partyMemberAuraStates.SpellId = packet.ReadUInt16();
					if (partyMemberAuraStates.SpellId != 0)
					{
						partyMemberAuraStates.ActiveFlags = 1u;
						partyMemberAuraStates.AuraFlags = 256;
					}
					partyMemberPartialState.Auras.Add(partyMemberAuraStates);
				}
			}
		}
		if (groupUpdateFlagVanilla.HasFlag(GroupUpdateFlagVanilla.AurasNegative))
		{
			if (partyMemberPartialState.Auras == null)
			{
				partyMemberPartialState.Auras = new List<PartyMemberAuraStates>();
			}
			ushort num2 = packet.ReadUInt16();
			byte b3 = 48;
			for (byte b4 = 0; b4 < b3; b4++)
			{
				if ((num2 & (1L << (int)b4)) != 0L)
				{
					PartyMemberAuraStates partyMemberAuraStates2 = new PartyMemberAuraStates();
					partyMemberAuraStates2.SpellId = packet.ReadUInt16();
					if (partyMemberAuraStates2.SpellId != 0)
					{
						partyMemberAuraStates2.ActiveFlags = 1u;
						partyMemberAuraStates2.AuraFlags = 16;
					}
					partyMemberPartialState.Auras.Add(partyMemberAuraStates2);
				}
			}
		}
		if (groupUpdateFlagVanilla.HasFlag(GroupUpdateFlagVanilla.PetGuid))
		{
			if (partyMemberPartialState.Pet == null)
			{
				partyMemberPartialState.Pet = new PartyMemberPetStats();
			}
			partyMemberPartialState.Pet.NewPetGuid = packet.ReadGuid().To128(GetSession().GameState);
		}
		if (groupUpdateFlagVanilla.HasFlag(GroupUpdateFlagVanilla.PetName))
		{
			if (partyMemberPartialState.Pet == null)
			{
				partyMemberPartialState.Pet = new PartyMemberPetStats();
			}
			partyMemberPartialState.Pet.NewPetName = packet.ReadCString();
		}
		if (groupUpdateFlagVanilla.HasFlag(GroupUpdateFlagVanilla.PetModelId))
		{
			if (partyMemberPartialState.Pet == null)
			{
				partyMemberPartialState.Pet = new PartyMemberPetStats();
			}
			partyMemberPartialState.Pet.DisplayID = packet.ReadUInt16();
		}
		if (groupUpdateFlagVanilla.HasFlag(GroupUpdateFlagVanilla.PetCurrentHealth))
		{
			if (partyMemberPartialState.Pet == null)
			{
				partyMemberPartialState.Pet = new PartyMemberPetStats();
			}
			partyMemberPartialState.Pet.Health = packet.ReadUInt16();
		}
		if (groupUpdateFlagVanilla.HasFlag(GroupUpdateFlagVanilla.PetMaxHealth))
		{
			if (partyMemberPartialState.Pet == null)
			{
				partyMemberPartialState.Pet = new PartyMemberPetStats();
			}
			partyMemberPartialState.Pet.MaxHealth = packet.ReadUInt16();
		}
		if (groupUpdateFlagVanilla.HasFlag(GroupUpdateFlagVanilla.PetPowerType))
		{
			packet.ReadUInt8();
		}
		if (groupUpdateFlagVanilla.HasFlag(GroupUpdateFlagVanilla.PetCurrentPower))
		{
			packet.ReadInt16();
		}
		if (groupUpdateFlagVanilla.HasFlag(GroupUpdateFlagVanilla.PetMaxPower))
		{
			packet.ReadInt16();
		}
		if (groupUpdateFlagVanilla.HasFlag(GroupUpdateFlagVanilla.PetAuras))
		{
			if (partyMemberPartialState.Pet == null)
			{
				partyMemberPartialState.Pet = new PartyMemberPetStats();
			}
			if (partyMemberPartialState.Pet.Auras == null)
			{
				partyMemberPartialState.Pet.Auras = new List<PartyMemberAuraStates>();
			}
			uint num3 = packet.ReadUInt32();
			byte b5 = 32;
			for (byte b6 = 0; b6 < b5; b6++)
			{
				if ((num3 & (1L << (int)b6)) != 0L)
				{
					PartyMemberAuraStates partyMemberAuraStates3 = new PartyMemberAuraStates();
					partyMemberAuraStates3.SpellId = packet.ReadUInt16();
					if (partyMemberAuraStates3.SpellId != 0)
					{
						partyMemberAuraStates3.ActiveFlags = 1u;
						partyMemberAuraStates3.AuraFlags = 256;
					}
					partyMemberPartialState.Pet.Auras.Add(partyMemberAuraStates3);
				}
			}
		}
		if (groupUpdateFlagVanilla.HasFlag(GroupUpdateFlagVanilla.PetAurasNegative))
		{
			if (partyMemberPartialState.Pet == null)
			{
				partyMemberPartialState.Pet = new PartyMemberPetStats();
			}
			if (partyMemberPartialState.Pet.Auras == null)
			{
				partyMemberPartialState.Pet.Auras = new List<PartyMemberAuraStates>();
			}
			ushort num4 = packet.ReadUInt16();
			byte b7 = 48;
			for (byte b8 = 0; b8 < b7; b8++)
			{
				if ((num4 & (1L << (int)b8)) != 0L)
				{
					PartyMemberAuraStates partyMemberAuraStates4 = new PartyMemberAuraStates();
					partyMemberAuraStates4.SpellId = packet.ReadUInt16();
					if (partyMemberAuraStates4.SpellId != 0)
					{
						partyMemberAuraStates4.ActiveFlags = 1u;
						partyMemberAuraStates4.AuraFlags = 16;
					}
					partyMemberPartialState.Pet.Auras.Add(partyMemberAuraStates4);
				}
			}
		}
		SendPacketToClient(partyMemberPartialState);
	}

	[PacketHandler(Opcode.SMSG_PARTY_MEMBER_PARTIAL_STATE, ClientVersionBuild.V2_0_1_6180)]
	private void HandlePartyMemberStatsTbc(WorldPacket packet)
	{
		if (GetSession().GameState.CurrentMapId == 489 && (GetSession().GameState.HasWsgAllyFlagCarrier || GetSession().GameState.HasWsgHordeFlagCarrier) && _requestBgPlayerPosCounter++ > 10)
		{
			WorldPacket packet2 = new WorldPacket(Opcode.MSG_BATTLEGROUND_PLAYER_POSITIONS);
			SendPacket(packet2);
			_requestBgPlayerPosCounter = 0u;
		}
		PartyMemberPartialState partyMemberPartialState = new PartyMemberPartialState();
		partyMemberPartialState.AffectedGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
		GroupUpdateFlagTBC groupUpdateFlagTBC = (GroupUpdateFlagTBC)packet.ReadUInt32();
		if (groupUpdateFlagTBC.HasFlag(GroupUpdateFlagTBC.Status))
		{
			partyMemberPartialState.StatusFlags = packet.ReadUInt16();
		}
		if (groupUpdateFlagTBC.HasFlag(GroupUpdateFlagTBC.CurrentHealth))
		{
			partyMemberPartialState.CurrentHealth = packet.ReadUInt16();
		}
		if (groupUpdateFlagTBC.HasFlag(GroupUpdateFlagTBC.MaxHealth))
		{
			partyMemberPartialState.MaxHealth = packet.ReadUInt16();
		}
		if (groupUpdateFlagTBC.HasFlag(GroupUpdateFlagTBC.PowerType))
		{
			partyMemberPartialState.PowerType = packet.ReadUInt8();
		}
		if (groupUpdateFlagTBC.HasFlag(GroupUpdateFlagTBC.CurrentPower))
		{
			partyMemberPartialState.CurrentPower = packet.ReadUInt16();
		}
		if (groupUpdateFlagTBC.HasFlag(GroupUpdateFlagTBC.MaxPower))
		{
			partyMemberPartialState.MaxPower = packet.ReadUInt16();
		}
		if (groupUpdateFlagTBC.HasFlag(GroupUpdateFlagTBC.Level))
		{
			partyMemberPartialState.Level = packet.ReadUInt16();
		}
		if (groupUpdateFlagTBC.HasFlag(GroupUpdateFlagTBC.Zone))
		{
			partyMemberPartialState.ZoneID = packet.ReadUInt16();
		}
		if (groupUpdateFlagTBC.HasFlag(GroupUpdateFlagTBC.Position))
		{
			partyMemberPartialState.Position = new PartyMemberPartialState.Vector3_UInt16();
			partyMemberPartialState.Position.X = packet.ReadInt16();
			partyMemberPartialState.Position.Y = packet.ReadInt16();
		}
		if (groupUpdateFlagTBC.HasFlag(GroupUpdateFlagTBC.Auras))
		{
			if (partyMemberPartialState.Auras == null)
			{
				partyMemberPartialState.Auras = new List<PartyMemberAuraStates>();
			}
			ulong num = packet.ReadUInt64();
			for (byte b = 0; b < LegacyVersion.GetAuraSlotsCount(); b++)
			{
				if ((num & (ulong)(1L << (int)b)) != 0L)
				{
					PartyMemberAuraStates partyMemberAuraStates = new PartyMemberAuraStates();
					partyMemberAuraStates.SpellId = packet.ReadUInt16();
					packet.ReadUInt8();
					if (partyMemberAuraStates.SpellId != 0)
					{
						partyMemberAuraStates.ActiveFlags = 1u;
						partyMemberAuraStates.AuraFlags = 256;
					}
					partyMemberPartialState.Auras.Add(partyMemberAuraStates);
				}
			}
		}
		if (groupUpdateFlagTBC.HasFlag(GroupUpdateFlagTBC.PetGuid))
		{
			if (partyMemberPartialState.Pet == null)
			{
				partyMemberPartialState.Pet = new PartyMemberPetStats();
			}
			partyMemberPartialState.Pet.NewPetGuid = packet.ReadGuid().To128(GetSession().GameState);
		}
		if (groupUpdateFlagTBC.HasFlag(GroupUpdateFlagTBC.PetName))
		{
			if (partyMemberPartialState.Pet == null)
			{
				partyMemberPartialState.Pet = new PartyMemberPetStats();
			}
			partyMemberPartialState.Pet.NewPetName = packet.ReadCString();
		}
		if (groupUpdateFlagTBC.HasFlag(GroupUpdateFlagTBC.PetModelId))
		{
			if (partyMemberPartialState.Pet == null)
			{
				partyMemberPartialState.Pet = new PartyMemberPetStats();
			}
			partyMemberPartialState.Pet.DisplayID = packet.ReadUInt16();
		}
		if (groupUpdateFlagTBC.HasFlag(GroupUpdateFlagTBC.PetCurrentHealth))
		{
			if (partyMemberPartialState.Pet == null)
			{
				partyMemberPartialState.Pet = new PartyMemberPetStats();
			}
			partyMemberPartialState.Pet.Health = packet.ReadUInt16();
		}
		if (groupUpdateFlagTBC.HasFlag(GroupUpdateFlagTBC.PetMaxHealth))
		{
			if (partyMemberPartialState.Pet == null)
			{
				partyMemberPartialState.Pet = new PartyMemberPetStats();
			}
			partyMemberPartialState.Pet.MaxHealth = packet.ReadUInt16();
		}
		if (groupUpdateFlagTBC.HasFlag(GroupUpdateFlagTBC.PetPowerType))
		{
			packet.ReadUInt8();
		}
		if (groupUpdateFlagTBC.HasFlag(GroupUpdateFlagTBC.PetCurrentPower))
		{
			packet.ReadInt16();
		}
		if (groupUpdateFlagTBC.HasFlag(GroupUpdateFlagTBC.PetMaxPower))
		{
			packet.ReadInt16();
		}
		if (groupUpdateFlagTBC.HasFlag(GroupUpdateFlagTBC.PetAuras))
		{
			if (partyMemberPartialState.Pet == null)
			{
				partyMemberPartialState.Pet = new PartyMemberPetStats();
			}
			if (partyMemberPartialState.Pet.Auras == null)
			{
				partyMemberPartialState.Pet.Auras = new List<PartyMemberAuraStates>();
			}
			ulong num2 = packet.ReadUInt64();
			for (byte b2 = 0; b2 < LegacyVersion.GetAuraSlotsCount(); b2++)
			{
				if ((num2 & (ulong)(1L << (int)b2)) != 0L)
				{
					PartyMemberAuraStates partyMemberAuraStates2 = new PartyMemberAuraStates();
					partyMemberAuraStates2.SpellId = packet.ReadUInt16();
					packet.ReadUInt8();
					if (partyMemberAuraStates2.SpellId != 0)
					{
						partyMemberAuraStates2.ActiveFlags = 1u;
						partyMemberAuraStates2.AuraFlags = 256;
					}
					partyMemberPartialState.Pet.Auras.Add(partyMemberAuraStates2);
				}
			}
		}
		SendPacketToClient(partyMemberPartialState);
	}

	[PacketHandler(Opcode.SMSG_PARTY_MEMBER_FULL_STATE, ClientVersionBuild.Zero, ClientVersionBuild.V2_0_1_6180)]
	private void HandlePartyMemberStatsFull(WorldPacket packet)
	{
		if (GetSession().GameState.CurrentMapId == 489 && (GetSession().GameState.HasWsgAllyFlagCarrier || GetSession().GameState.HasWsgHordeFlagCarrier) && _requestBgPlayerPosCounter++ > 10)
		{
			WorldPacket packet2 = new WorldPacket(Opcode.MSG_BATTLEGROUND_PLAYER_POSITIONS);
			SendPacket(packet2);
			_requestBgPlayerPosCounter = 0u;
		}
		PartyMemberFullState partyMemberFullState = new PartyMemberFullState();
		if (GetSession().GameState.IsInBattleground())
		{
			partyMemberFullState.PartyType[0] = 0;
			partyMemberFullState.PartyType[1] = 2;
		}
		else
		{
			partyMemberFullState.PartyType[0] = 1;
			partyMemberFullState.PartyType[1] = 0;
		}
		partyMemberFullState.MemberGuid = packet.ReadPackedGuid().To128(GetSession().GameState);
		GroupUpdateFlagVanilla groupUpdateFlagVanilla = (GroupUpdateFlagVanilla)packet.ReadUInt32();
		if (groupUpdateFlagVanilla.HasFlag(GroupUpdateFlagVanilla.Status))
		{
			partyMemberFullState.StatusFlags = (GroupMemberOnlineStatus)packet.ReadUInt8();
		}
		if (groupUpdateFlagVanilla.HasFlag(GroupUpdateFlagVanilla.CurrentHealth))
		{
			partyMemberFullState.CurrentHealth = packet.ReadUInt16();
		}
		if (groupUpdateFlagVanilla.HasFlag(GroupUpdateFlagVanilla.MaxHealth))
		{
			partyMemberFullState.MaxHealth = packet.ReadUInt16();
		}
		if (groupUpdateFlagVanilla.HasFlag(GroupUpdateFlagVanilla.PowerType))
		{
			partyMemberFullState.PowerType = packet.ReadUInt8();
		}
		if (groupUpdateFlagVanilla.HasFlag(GroupUpdateFlagVanilla.CurrentPower))
		{
			partyMemberFullState.CurrentPower = packet.ReadUInt16();
		}
		if (groupUpdateFlagVanilla.HasFlag(GroupUpdateFlagVanilla.MaxPower))
		{
			partyMemberFullState.MaxPower = packet.ReadUInt16();
		}
		if (groupUpdateFlagVanilla.HasFlag(GroupUpdateFlagVanilla.Level))
		{
			partyMemberFullState.Level = packet.ReadUInt16();
		}
		if (groupUpdateFlagVanilla.HasFlag(GroupUpdateFlagVanilla.Zone))
		{
			partyMemberFullState.ZoneID = packet.ReadUInt16();
		}
		if (groupUpdateFlagVanilla.HasFlag(GroupUpdateFlagVanilla.Position))
		{
			partyMemberFullState.PositionX = packet.ReadInt16();
			partyMemberFullState.PositionY = packet.ReadInt16();
		}
		if (groupUpdateFlagVanilla.HasFlag(GroupUpdateFlagVanilla.Auras))
		{
			if (partyMemberFullState.Auras == null)
			{
				partyMemberFullState.Auras = new List<PartyMemberAuraStates>();
			}
			uint num = packet.ReadUInt32();
			byte b = 32;
			for (byte b2 = 0; b2 < b; b2++)
			{
				if ((num & (1L << (int)b2)) != 0L)
				{
					PartyMemberAuraStates partyMemberAuraStates = new PartyMemberAuraStates();
					partyMemberAuraStates.SpellId = packet.ReadUInt16();
					if (partyMemberAuraStates.SpellId != 0)
					{
						partyMemberAuraStates.ActiveFlags = 1u;
						partyMemberAuraStates.AuraFlags = 256;
					}
					partyMemberFullState.Auras.Add(partyMemberAuraStates);
				}
			}
		}
		if (groupUpdateFlagVanilla.HasFlag(GroupUpdateFlagVanilla.AurasNegative))
		{
			if (partyMemberFullState.Auras == null)
			{
				partyMemberFullState.Auras = new List<PartyMemberAuraStates>();
			}
			ushort num2 = packet.ReadUInt16();
			byte b3 = 48;
			for (byte b4 = 0; b4 < b3; b4++)
			{
				if ((num2 & (1L << (int)b4)) != 0L)
				{
					PartyMemberAuraStates partyMemberAuraStates2 = new PartyMemberAuraStates();
					partyMemberAuraStates2.SpellId = packet.ReadUInt16();
					if (partyMemberAuraStates2.SpellId != 0)
					{
						partyMemberAuraStates2.ActiveFlags = 1u;
						partyMemberAuraStates2.AuraFlags = 16;
					}
					partyMemberFullState.Auras.Add(partyMemberAuraStates2);
				}
			}
		}
		if (groupUpdateFlagVanilla.HasFlag(GroupUpdateFlagVanilla.PetGuid))
		{
			if (partyMemberFullState.Pet == null)
			{
				partyMemberFullState.Pet = new PartyMemberPetStats();
			}
			partyMemberFullState.Pet.NewPetGuid = packet.ReadGuid().To128(GetSession().GameState);
		}
		if (groupUpdateFlagVanilla.HasFlag(GroupUpdateFlagVanilla.PetName))
		{
			if (partyMemberFullState.Pet == null)
			{
				partyMemberFullState.Pet = new PartyMemberPetStats();
			}
			partyMemberFullState.Pet.NewPetName = packet.ReadCString();
		}
		if (groupUpdateFlagVanilla.HasFlag(GroupUpdateFlagVanilla.PetModelId))
		{
			if (partyMemberFullState.Pet == null)
			{
				partyMemberFullState.Pet = new PartyMemberPetStats();
			}
			partyMemberFullState.Pet.DisplayID = packet.ReadUInt16();
		}
		if (groupUpdateFlagVanilla.HasFlag(GroupUpdateFlagVanilla.PetCurrentHealth))
		{
			if (partyMemberFullState.Pet == null)
			{
				partyMemberFullState.Pet = new PartyMemberPetStats();
			}
			partyMemberFullState.Pet.Health = packet.ReadUInt16();
		}
		if (groupUpdateFlagVanilla.HasFlag(GroupUpdateFlagVanilla.PetMaxHealth))
		{
			if (partyMemberFullState.Pet == null)
			{
				partyMemberFullState.Pet = new PartyMemberPetStats();
			}
			partyMemberFullState.Pet.MaxHealth = packet.ReadUInt16();
		}
		if (groupUpdateFlagVanilla.HasFlag(GroupUpdateFlagVanilla.PetPowerType))
		{
			packet.ReadUInt8();
		}
		if (groupUpdateFlagVanilla.HasFlag(GroupUpdateFlagVanilla.PetCurrentPower))
		{
			packet.ReadInt16();
		}
		if (groupUpdateFlagVanilla.HasFlag(GroupUpdateFlagVanilla.PetMaxPower))
		{
			packet.ReadInt16();
		}
		if (groupUpdateFlagVanilla.HasFlag(GroupUpdateFlagVanilla.PetAuras))
		{
			if (partyMemberFullState.Pet == null)
			{
				partyMemberFullState.Pet = new PartyMemberPetStats();
			}
			if (partyMemberFullState.Pet.Auras == null)
			{
				partyMemberFullState.Pet.Auras = new List<PartyMemberAuraStates>();
			}
			uint num3 = packet.ReadUInt32();
			byte b5 = 32;
			for (byte b6 = 0; b6 < b5; b6++)
			{
				if ((num3 & (1L << (int)b6)) != 0L)
				{
					PartyMemberAuraStates partyMemberAuraStates3 = new PartyMemberAuraStates();
					partyMemberAuraStates3.SpellId = packet.ReadUInt16();
					if (partyMemberAuraStates3.SpellId != 0)
					{
						partyMemberAuraStates3.ActiveFlags = 1u;
						partyMemberAuraStates3.AuraFlags = 256;
					}
					partyMemberFullState.Pet.Auras.Add(partyMemberAuraStates3);
				}
			}
		}
		if (groupUpdateFlagVanilla.HasFlag(GroupUpdateFlagVanilla.PetAurasNegative))
		{
			if (partyMemberFullState.Pet == null)
			{
				partyMemberFullState.Pet = new PartyMemberPetStats();
			}
			if (partyMemberFullState.Pet.Auras == null)
			{
				partyMemberFullState.Pet.Auras = new List<PartyMemberAuraStates>();
			}
			ushort num4 = packet.ReadUInt16();
			byte b7 = 48;
			for (byte b8 = 0; b8 < b7; b8++)
			{
				if ((num4 & (1L << (int)b8)) != 0L)
				{
					PartyMemberAuraStates partyMemberAuraStates4 = new PartyMemberAuraStates();
					partyMemberAuraStates4.SpellId = packet.ReadUInt16();
					if (partyMemberAuraStates4.SpellId != 0)
					{
						partyMemberAuraStates4.ActiveFlags = 1u;
						partyMemberAuraStates4.AuraFlags = 16;
					}
					partyMemberFullState.Pet.Auras.Add(partyMemberAuraStates4);
				}
			}
		}
		SendPacketToClient(partyMemberFullState);
	}

	[PacketHandler(Opcode.SMSG_PARTY_MEMBER_FULL_STATE, ClientVersionBuild.V2_0_1_6180)]
	private void HandlePartyMemberStatsFullTBC(WorldPacket packet)
	{
		if (GetSession().GameState.CurrentMapId == 489 && (GetSession().GameState.HasWsgAllyFlagCarrier || GetSession().GameState.HasWsgHordeFlagCarrier) && _requestBgPlayerPosCounter++ > 10)
		{
			WorldPacket packet2 = new WorldPacket(Opcode.MSG_BATTLEGROUND_PLAYER_POSITIONS);
			SendPacket(packet2);
			_requestBgPlayerPosCounter = 0u;
		}
		PartyMemberFullState partyMemberFullState = new PartyMemberFullState();
		if (GetSession().GameState.IsInBattleground())
		{
			partyMemberFullState.PartyType[0] = 0;
			partyMemberFullState.PartyType[1] = 2;
		}
		else
		{
			partyMemberFullState.PartyType[0] = 1;
			partyMemberFullState.PartyType[1] = 0;
		}
		partyMemberFullState.MemberGuid = packet.ReadPackedGuid().To128(GetSession().GameState);
		GroupUpdateFlagTBC groupUpdateFlagTBC = (GroupUpdateFlagTBC)packet.ReadUInt32();
		if (groupUpdateFlagTBC.HasFlag(GroupUpdateFlagTBC.Status))
		{
			partyMemberFullState.StatusFlags = (GroupMemberOnlineStatus)packet.ReadUInt16();
		}
		if (groupUpdateFlagTBC.HasFlag(GroupUpdateFlagTBC.CurrentHealth))
		{
			partyMemberFullState.CurrentHealth = packet.ReadUInt16();
		}
		if (groupUpdateFlagTBC.HasFlag(GroupUpdateFlagTBC.MaxHealth))
		{
			partyMemberFullState.MaxHealth = packet.ReadUInt16();
		}
		if (groupUpdateFlagTBC.HasFlag(GroupUpdateFlagTBC.PowerType))
		{
			partyMemberFullState.PowerType = packet.ReadUInt8();
		}
		if (groupUpdateFlagTBC.HasFlag(GroupUpdateFlagTBC.CurrentPower))
		{
			partyMemberFullState.CurrentPower = packet.ReadUInt16();
		}
		if (groupUpdateFlagTBC.HasFlag(GroupUpdateFlagTBC.MaxPower))
		{
			partyMemberFullState.MaxPower = packet.ReadUInt16();
		}
		if (groupUpdateFlagTBC.HasFlag(GroupUpdateFlagTBC.Level))
		{
			partyMemberFullState.Level = packet.ReadUInt16();
		}
		if (groupUpdateFlagTBC.HasFlag(GroupUpdateFlagTBC.Zone))
		{
			partyMemberFullState.ZoneID = packet.ReadUInt16();
		}
		if (groupUpdateFlagTBC.HasFlag(GroupUpdateFlagTBC.Position))
		{
			partyMemberFullState.PositionX = packet.ReadInt16();
			partyMemberFullState.PositionY = packet.ReadInt16();
		}
		if (groupUpdateFlagTBC.HasFlag(GroupUpdateFlagTBC.Auras))
		{
			if (partyMemberFullState.Auras == null)
			{
				partyMemberFullState.Auras = new List<PartyMemberAuraStates>();
			}
			ulong num = packet.ReadUInt64();
			for (byte b = 0; b < LegacyVersion.GetAuraSlotsCount(); b++)
			{
				if ((num & (ulong)(1L << (int)b)) != 0L)
				{
					PartyMemberAuraStates partyMemberAuraStates = new PartyMemberAuraStates();
					partyMemberAuraStates.SpellId = packet.ReadUInt16();
					packet.ReadUInt8();
					if (partyMemberAuraStates.SpellId != 0)
					{
						partyMemberAuraStates.ActiveFlags = 1u;
						partyMemberAuraStates.AuraFlags = 256;
					}
					partyMemberFullState.Auras.Add(partyMemberAuraStates);
				}
			}
		}
		if (groupUpdateFlagTBC.HasFlag(GroupUpdateFlagTBC.PetGuid))
		{
			if (partyMemberFullState.Pet == null)
			{
				partyMemberFullState.Pet = new PartyMemberPetStats();
			}
			partyMemberFullState.Pet.NewPetGuid = packet.ReadGuid().To128(GetSession().GameState);
		}
		if (groupUpdateFlagTBC.HasFlag(GroupUpdateFlagTBC.PetName))
		{
			if (partyMemberFullState.Pet == null)
			{
				partyMemberFullState.Pet = new PartyMemberPetStats();
			}
			partyMemberFullState.Pet.NewPetName = packet.ReadCString();
		}
		if (groupUpdateFlagTBC.HasFlag(GroupUpdateFlagTBC.PetModelId))
		{
			if (partyMemberFullState.Pet == null)
			{
				partyMemberFullState.Pet = new PartyMemberPetStats();
			}
			partyMemberFullState.Pet.DisplayID = packet.ReadUInt16();
		}
		if (groupUpdateFlagTBC.HasFlag(GroupUpdateFlagTBC.PetCurrentHealth))
		{
			if (partyMemberFullState.Pet == null)
			{
				partyMemberFullState.Pet = new PartyMemberPetStats();
			}
			partyMemberFullState.Pet.Health = packet.ReadUInt16();
		}
		if (groupUpdateFlagTBC.HasFlag(GroupUpdateFlagTBC.PetMaxHealth))
		{
			if (partyMemberFullState.Pet == null)
			{
				partyMemberFullState.Pet = new PartyMemberPetStats();
			}
			partyMemberFullState.Pet.MaxHealth = packet.ReadUInt16();
		}
		if (groupUpdateFlagTBC.HasFlag(GroupUpdateFlagTBC.PetPowerType))
		{
			packet.ReadUInt8();
		}
		if (groupUpdateFlagTBC.HasFlag(GroupUpdateFlagTBC.PetCurrentPower))
		{
			packet.ReadInt16();
		}
		if (groupUpdateFlagTBC.HasFlag(GroupUpdateFlagTBC.PetMaxPower))
		{
			packet.ReadInt16();
		}
		if (groupUpdateFlagTBC.HasFlag(GroupUpdateFlagTBC.PetAuras))
		{
			if (partyMemberFullState.Pet == null)
			{
				partyMemberFullState.Pet = new PartyMemberPetStats();
			}
			if (partyMemberFullState.Pet.Auras == null)
			{
				partyMemberFullState.Pet.Auras = new List<PartyMemberAuraStates>();
			}
			ulong num2 = packet.ReadUInt64();
			for (byte b2 = 0; b2 < LegacyVersion.GetAuraSlotsCount(); b2++)
			{
				if ((num2 & (ulong)(1L << (int)b2)) != 0L)
				{
					PartyMemberAuraStates partyMemberAuraStates2 = new PartyMemberAuraStates();
					partyMemberAuraStates2.SpellId = packet.ReadUInt16();
					packet.ReadUInt8();
					if (partyMemberAuraStates2.SpellId != 0)
					{
						partyMemberAuraStates2.ActiveFlags = 1u;
						partyMemberAuraStates2.AuraFlags = 256;
					}
					partyMemberFullState.Pet.Auras.Add(partyMemberAuraStates2);
				}
			}
		}
		SendPacketToClient(partyMemberFullState);
	}

    // 处理小地图 Ping
    [PacketHandler(Opcode.MSG_MINIMAP_PING)]
	private void HandleMinimapPing(WorldPacket packet)
	{
		MinimapPing minimapPing = new MinimapPing();
		minimapPing.SenderGUID = packet.ReadGuid().To128(GetSession().GameState);
		minimapPing.Position = packet.ReadVector2();
		SendPacketToClient(minimapPing);
	}

	// 处理随机roll
	[PacketHandler(Opcode.MSG_RANDOM_ROLL)]
	private void HandleRandomRoll(WorldPacket packet)
	{
		RandomRoll randomRoll = new RandomRoll();
		randomRoll.Min = packet.ReadInt32();
		randomRoll.Max = packet.ReadInt32();
		randomRoll.Result = packet.ReadInt32();
		randomRoll.Roller = packet.ReadGuid().To128(GetSession().GameState);
		randomRoll.RollerWowAccount = GetSession().GetGameAccountGuidForPlayer(randomRoll.Roller);
		SendPacketToClient(randomRoll);
	}

	// 
	[PacketHandler(Opcode.SMSG_GUILD_COMMAND_RESULT)]
	private void HandleGuildCommandResult(WorldPacket packet)
	{
		GuildCommandResult guildCommandResult = new GuildCommandResult();
		guildCommandResult.Command = (GuildCommandType)packet.ReadUInt32();
		guildCommandResult.Name = packet.ReadCString();
		guildCommandResult.Result = (GuildCommandError)packet.ReadUInt32();
		SendPacketToClient(guildCommandResult);
	}

    // HandleGuildEvent
    [PacketHandler(Opcode.SMSG_GUILD_EVENT)]
	private void HandleGuildEvent(WorldPacket packet)
	{
		GuildEventType guildEventType = (GuildEventType)packet.ReadUInt8();
		byte b = packet.ReadUInt8();
		string[] array = new string[b];
		for (int i = 0; i < b; i++)
		{
			array[i] = packet.ReadCString();
		}
		WowGuid128 wowGuid = WowGuid128.Empty;
		if (packet.CanRead())
		{
			wowGuid = packet.ReadGuid().To128(GetSession().GameState);
		}
		switch (guildEventType)
		{
		case GuildEventType.Promotion:
		case GuildEventType.Demotion:
		{
			WowGuid128 playerGuidByName3 = GetSession().GameState.GetPlayerGuidByName(array[0]);
			WowGuid128 playerGuidByName4 = GetSession().GameState.GetPlayerGuidByName(array[1]);
			uint guildRankIdByName = GetSession().GetGuildRankIdByName(GetSession().GameState.GetPlayerGuildId(GetSession().GameState.CurrentPlayerGuid), array[2]);
			if (playerGuidByName3 != null && playerGuidByName4 != null)
			{
				GuildSendRankChange guildSendRankChange = new GuildSendRankChange();
				guildSendRankChange.Officer = playerGuidByName3;
				guildSendRankChange.Other = playerGuidByName4;
				guildSendRankChange.Promote = guildEventType == GuildEventType.Promotion;
				guildSendRankChange.RankID = guildRankIdByName;
				SendPacketToClient(guildSendRankChange);
			}
			break;
		}
		case GuildEventType.MOTD:
		{
			GuildEventMotd guildEventMotd = new GuildEventMotd();
			guildEventMotd.MotdText = array[0];
			SendPacketToClient(guildEventMotd);
			break;
		}
		case GuildEventType.PlayerJoined:
		{
			GuildEventPlayerJoined guildEventPlayerJoined = new GuildEventPlayerJoined();
			guildEventPlayerJoined.Guid = wowGuid;
			guildEventPlayerJoined.VirtualRealmAddress = GetSession().RealmId.GetAddress();
			guildEventPlayerJoined.Name = array[0];
			SendPacketToClient(guildEventPlayerJoined);
			break;
		}
		case GuildEventType.PlayerLeft:
		{
			GuildEventPlayerLeft guildEventPlayerLeft2 = new GuildEventPlayerLeft();
			guildEventPlayerLeft2.Removed = false;
			guildEventPlayerLeft2.LeaverGUID = wowGuid;
			guildEventPlayerLeft2.LeaverVirtualRealmAddress = GetSession().RealmId.GetAddress();
			guildEventPlayerLeft2.LeaverName = array[0];
			SendPacketToClient(guildEventPlayerLeft2);
			break;
		}
		case GuildEventType.PlayerRemoved:
		{
			GuildEventPlayerLeft guildEventPlayerLeft = new GuildEventPlayerLeft();
			guildEventPlayerLeft.Removed = true;
			guildEventPlayerLeft.LeaverGUID = wowGuid;
			guildEventPlayerLeft.LeaverVirtualRealmAddress = GetSession().RealmId.GetAddress();
			guildEventPlayerLeft.LeaverName = array[0];
			guildEventPlayerLeft.RemoverGUID = GetSession().GameState.GetPlayerGuidByName(array[1]);
			guildEventPlayerLeft.RemoverVirtualRealmAddress = GetSession().RealmId.GetAddress();
			guildEventPlayerLeft.RemoverName = array[1];
			SendPacketToClient(guildEventPlayerLeft);
			break;
		}
		case GuildEventType.LeaderChanged:
		{
			WowGuid128 playerGuidByName = GetSession().GameState.GetPlayerGuidByName(array[0]);
			WowGuid128 playerGuidByName2 = GetSession().GameState.GetPlayerGuidByName(array[1]);
			if (playerGuidByName != null && playerGuidByName2 != null)
			{
				GuildEventNewLeader guildEventNewLeader = new GuildEventNewLeader();
				guildEventNewLeader.OldLeaderGUID = playerGuidByName;
				guildEventNewLeader.OldLeaderVirtualRealmAddress = GetSession().RealmId.GetAddress();
				guildEventNewLeader.OldLeaderName = array[0];
				guildEventNewLeader.NewLeaderGUID = playerGuidByName2;
				guildEventNewLeader.NewLeaderVirtualRealmAddress = GetSession().RealmId.GetAddress();
				guildEventNewLeader.NewLeaderName = array[1];
				SendPacketToClient(guildEventNewLeader);
			}
			break;
		}
		case GuildEventType.Disbanded:
		{
			GuildEventDisbanded packet5 = new GuildEventDisbanded();
			SendPacketToClient(packet5);
			break;
		}
		case GuildEventType.RankUpdated:
		{
			GuildEventRanksUpdated packet4 = new GuildEventRanksUpdated();
			SendPacketToClient(packet4);
			break;
		}
		case GuildEventType.PlayerSignedOn:
		case GuildEventType.PlayerSignedOff:
		{
			GuildEventPresenceChange guildEventPresenceChange = new GuildEventPresenceChange();
			guildEventPresenceChange.Guid = wowGuid;
			guildEventPresenceChange.VirtualRealmAddress = GetSession().RealmId.GetAddress();
			guildEventPresenceChange.LoggedOn = guildEventType == GuildEventType.PlayerSignedOn;
			guildEventPresenceChange.Name = array[0];
			SendPacketToClient(guildEventPresenceChange);
			break;
		}
		case GuildEventType.BankTabPurchased:
		{
			GuildEventTabAdded packet3 = new GuildEventTabAdded();
			SendPacketToClient(packet3);
			break;
		}
		case GuildEventType.BankTabUpdated:
		{
			GuildEventTabModified guildEventTabModified = new GuildEventTabModified();
			guildEventTabModified.Name = array[0];
			guildEventTabModified.Icon = array[1];
			SendPacketToClient(guildEventTabModified);
			break;
		}
		case GuildEventType.BankMoneyUpdate:
		{
			GuildEventBankMoneyChanged guildEventBankMoneyChanged = new GuildEventBankMoneyChanged();
			guildEventBankMoneyChanged.Money = (ulong)int.Parse(array[0], NumberStyles.HexNumber);
			SendPacketToClient(guildEventBankMoneyChanged);
			break;
		}
		case GuildEventType.BankTextChanged:
		{
			GuildEventTabTextChanged packet2 = new GuildEventTabTextChanged();
			SendPacketToClient(packet2);
			break;
		}
		case GuildEventType.LeaderIs:
		case GuildEventType.TabardChange:
		case GuildEventType.Unk11:
		case GuildEventType.BankBagSlotsChanged:
		case GuildEventType.BankMoneyWithdraw:
			break;
		}
	}

	[PacketHandler(Opcode.SMSG_QUERY_GUILD_INFO_RESPONSE)]
	private void HandleQueryGuildInfoResponse(WorldPacket packet)
	{
		QueryGuildInfoResponse queryGuildInfoResponse = new QueryGuildInfoResponse();
		uint num = packet.ReadUInt32();
		queryGuildInfoResponse.GuildGUID = WowGuid128.Create(HighGuidType703.Guild, num);
		queryGuildInfoResponse.PlayerGuid = GetSession().GameState.CurrentPlayerGuid;
		queryGuildInfoResponse.HasGuildInfo = true;
		queryGuildInfoResponse.Info = new QueryGuildInfoResponse.GuildInfo();
		queryGuildInfoResponse.Info.GuildGuid = queryGuildInfoResponse.GuildGUID;
		queryGuildInfoResponse.Info.VirtualRealmAddress = GetSession().RealmId.GetAddress();
		queryGuildInfoResponse.Info.GuildName = packet.ReadCString();
		GetSession().StoreGuildGuidAndName(queryGuildInfoResponse.GuildGUID, queryGuildInfoResponse.Info.GuildName);
		List<string> list = new List<string>();
		for (uint num2 = 0u; num2 < 10; num2++)
		{
			string text = packet.ReadCString();
			if (!string.IsNullOrEmpty(text))
			{
				QueryGuildInfoResponse.GuildInfo.RankInfo rankInfo = default(QueryGuildInfoResponse.GuildInfo.RankInfo);
				rankInfo.RankID = num2;
				rankInfo.RankOrder = num2;
				rankInfo.RankName = text;
				list.Add(text);
				queryGuildInfoResponse.Info.Ranks.Add(rankInfo);
			}
		}
		GetSession().StoreGuildRankNames(num, list);
		queryGuildInfoResponse.Info.EmblemStyle = packet.ReadUInt32();
		queryGuildInfoResponse.Info.EmblemColor = packet.ReadUInt32();
		queryGuildInfoResponse.Info.BorderStyle = packet.ReadUInt32();
		queryGuildInfoResponse.Info.BorderColor = packet.ReadUInt32();
		queryGuildInfoResponse.Info.BackgroundColor = packet.ReadUInt32();
		SendPacketToClient(queryGuildInfoResponse);
	}

	// 处理公会信息
	[PacketHandler(Opcode.SMSG_GUILD_INFO)]
	private void HandleGuildInfo(WorldPacket packet)
	{
		packet.ReadCString();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			GetSession().GameState.CurrentGuildCreateTime = packet.ReadPackedTime();
		}
		else
		{
			int num = packet.ReadInt32();
			int num2 = packet.ReadInt32();
			int num3 = packet.ReadInt32();
			try
			{
				DateTime dateTime = new DateTime(num3, num2, num);
				GetSession().GameState.CurrentGuildCreateTime = (uint)Time.DateTimeToUnixTime(dateTime);
			}
			catch
			{
				Log.Print(LogType.Error, $"Invalid guild create date: {num}-{num2}-{num3}", "HandleGuildInfo", "D:\\a\\HermesProxy\\HermesProxy\\World\\Client\\PacketHandlers\\GuildHandler.cs");
			}
		}
		packet.ReadUInt32();
		GetSession().GameState.CurrentGuildNumAccounts = packet.ReadUInt32();
	}

	[PacketHandler(Opcode.SMSG_GUILD_ROSTER)]
	private void HandleGuildRoster(WorldPacket packet)
	{
		GuildRoster guildRoster = new GuildRoster();
		uint num = packet.ReadUInt32();
		if (GetSession().GameState.CurrentGuildNumAccounts != 0)
		{
			guildRoster.NumAccounts = GetSession().GameState.CurrentGuildNumAccounts;
		}
		else
		{
			guildRoster.NumAccounts = num;
		}
		guildRoster.WelcomeText = packet.ReadCString();
		guildRoster.InfoText = packet.ReadCString();
		if (GetSession().GameState.CurrentGuildCreateTime != 0)
		{
			guildRoster.CreateDate = GetSession().GameState.CurrentGuildCreateTime;
		}
		else
		{
			guildRoster.CreateDate = (uint)Time.UnixTime;
		}
		int num2 = packet.ReadInt32();
		if (num2 > 0)
		{
			GuildRanks guildRanks = new GuildRanks();
			for (byte b = 0; b < num2; b++)
			{
				GuildRankData guildRankData = new GuildRankData();
				guildRankData.RankID = b;
				guildRankData.RankOrder = b;
				guildRankData.RankName = GetSession().GetGuildRankNameById(GetSession().GameState.GetPlayerGuildId(GetSession().GameState.CurrentPlayerGuid), b);
				guildRankData.Flags = packet.ReadUInt32();
				if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
				{
					guildRankData.WithdrawGoldLimit = packet.ReadInt32();
					for (int i = 0; i < 6; i++)
					{
						guildRankData.TabFlags[i] = packet.ReadUInt32();
						guildRankData.TabWithdrawItemLimit[i] = packet.ReadUInt32();
					}
				}
				guildRanks.Ranks.Add(guildRankData);
			}
			SendPacketToClient(guildRanks);
		}
		for (int j = 0; j < num; j++)
		{
			GuildRosterMemberData guildRosterMemberData = new GuildRosterMemberData();
			PlayerCache playerCache = new PlayerCache();
			guildRosterMemberData.Guid = packet.ReadGuid().To128(GetSession().GameState);
			guildRosterMemberData.VirtualRealmAddress = GetSession().RealmId.GetAddress();
			guildRosterMemberData.Status = packet.ReadUInt8();
			guildRosterMemberData.Name = (playerCache.Name = packet.ReadCString());
			guildRosterMemberData.RankID = packet.ReadInt32();
			guildRosterMemberData.Level = (playerCache.Level = packet.ReadUInt8());
			guildRosterMemberData.ClassID = (playerCache.ClassId = (Class)packet.ReadUInt8());
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_4_0_8089))
			{
				guildRosterMemberData.SexID = (playerCache.SexId = (Gender)packet.ReadUInt8());
			}
			GetSession().GameState.UpdatePlayerCache(guildRosterMemberData.Guid, playerCache);
			guildRosterMemberData.AreaID = packet.ReadInt32();
			if (guildRosterMemberData.Status == 0)
			{
				guildRosterMemberData.LastSave = packet.ReadFloat();
			}
			else
			{
				guildRosterMemberData.Authenticated = true;
			}
			guildRosterMemberData.Note = packet.ReadCString();
			guildRosterMemberData.OfficerNote = packet.ReadCString();
			guildRoster.MemberData.Add(guildRosterMemberData);
		}
		SendPacketToClient(guildRoster);
	}

	[PacketHandler(Opcode.SMSG_GUILD_INVITE)]
	private void HandleGuildInvite(WorldPacket packet)
	{
		GuildInvite guildInvite = new GuildInvite();
		guildInvite.InviterName = packet.ReadCString();
		guildInvite.InviterVirtualRealmAddress = GetSession().RealmId.GetAddress();
		guildInvite.GuildName = packet.ReadCString();
		guildInvite.GuildVirtualRealmAddress = GetSession().RealmId.GetAddress();
		guildInvite.GuildGUID = GetSession().GetGuildGuid(guildInvite.GuildName);
		SendPacketToClient(guildInvite);
	}

	[PacketHandler(Opcode.MSG_TABARDVENDOR_ACTIVATE)]
	private void HandleTabardVendorActivate(WorldPacket packet)
	{
		PlayerTabardVendorActivate playerTabardVendorActivate = new PlayerTabardVendorActivate();
		playerTabardVendorActivate.DesignerGUID = packet.ReadGuid().To128(GetSession().GameState);
		SendPacketToClient(playerTabardVendorActivate);
	}

	[PacketHandler(Opcode.MSG_SAVE_GUILD_EMBLEM)]
	private void HandleSaveGuildEmblem(WorldPacket packet)
	{
		PlayerSaveGuildEmblem playerSaveGuildEmblem = new PlayerSaveGuildEmblem();
		playerSaveGuildEmblem.Error = (GuildEmblemError)packet.ReadUInt32();
		SendPacketToClient(playerSaveGuildEmblem);
	}

	[PacketHandler(Opcode.SMSG_GUILD_INVITE_DECLINED)]
	private void HandleGuildInviteDeclined(WorldPacket packet)
	{
		GuildInviteDeclined guildInviteDeclined = new GuildInviteDeclined();
		guildInviteDeclined.InviterName = packet.ReadCString();
		guildInviteDeclined.InviterVirtualRealmAddress = GetSession().RealmId.GetAddress();
		SendPacketToClient(guildInviteDeclined);
	}

	[PacketHandler(Opcode.SMSG_GUILD_BANK_QUERY_RESULTS)]
	private void HandleGuildBankQueryResults(WorldPacket packet)
	{
		GuildBankQueryResults guildBankQueryResults = new GuildBankQueryResults();
		guildBankQueryResults.Money = packet.ReadUInt64();
		guildBankQueryResults.Tab = packet.ReadUInt8();
		guildBankQueryResults.WithdrawalsRemaining = packet.ReadInt32();
		bool flag = false;
		if (packet.ReadBool() && guildBankQueryResults.Tab == 0)
		{
			flag = true;
			byte b = packet.ReadUInt8();
			for (int i = 0; i < b; i++)
			{
				GuildBankTabInfo guildBankTabInfo = default(GuildBankTabInfo);
				guildBankTabInfo.TabIndex = i;
				guildBankTabInfo.Name = packet.ReadCString();
				guildBankTabInfo.Icon = packet.ReadCString();
				guildBankQueryResults.TabInfo.Add(guildBankTabInfo);
			}
		}
		byte b2 = packet.ReadUInt8();
		for (int j = 0; j < b2; j++)
		{
			GuildBankItemInfo guildBankItemInfo = new GuildBankItemInfo();
			guildBankItemInfo.Slot = packet.ReadUInt8();
			int num = packet.ReadInt32();
			if (num > 0)
			{
				guildBankItemInfo.Item.ItemID = (uint)num;
				if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
				{
					guildBankItemInfo.Flags = packet.ReadUInt32();
				}
				guildBankItemInfo.Item.RandomPropertiesID = packet.ReadUInt32();
				if (guildBankItemInfo.Item.RandomPropertiesID != 0)
				{
					guildBankItemInfo.Item.RandomPropertiesSeed = packet.ReadUInt32();
				}
				if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
				{
					guildBankItemInfo.Count = packet.ReadInt32();
				}
				else
				{
					guildBankItemInfo.Count = packet.ReadUInt8();
				}
				guildBankItemInfo.EnchantmentID = packet.ReadInt32();
				guildBankItemInfo.Charges = packet.ReadUInt8();
				byte b3 = packet.ReadUInt8();
				for (int k = 0; k < b3; k++)
				{
					byte slot = packet.ReadUInt8();
					uint num2 = packet.ReadUInt32();
					if (num2 != 0)
					{
						uint gemFromEnchantId = GameData.GetGemFromEnchantId(num2);
						if (gemFromEnchantId != 0)
						{
							ItemGemData itemGemData = new ItemGemData();
							itemGemData.Slot = slot;
							itemGemData.Item.ItemID = gemFromEnchantId;
							guildBankItemInfo.SocketEnchant.Add(itemGemData);
						}
					}
				}
			}
			guildBankQueryResults.ItemInfo.Add(guildBankItemInfo);
		}
		guildBankQueryResults.FullUpdate = flag && b2 > 0;
		SendPacketToClient(guildBankQueryResults);
	}

	[PacketHandler(Opcode.MSG_QUERY_GUILD_BANK_TEXT)]
	private void HandleQueryGuildBankText(WorldPacket packet)
	{
		GuildBankTextQueryResult guildBankTextQueryResult = new GuildBankTextQueryResult();
		guildBankTextQueryResult.Tab = packet.ReadUInt8();
		guildBankTextQueryResult.Text = packet.ReadCString();
		SendPacketToClient(guildBankTextQueryResult);
	}

	[PacketHandler(Opcode.MSG_GUILD_BANK_LOG_QUERY)]
	private void HandleGuildBankLongQuery(WorldPacket packet)
	{
		GuildBankLogQueryResults guildBankLogQueryResults = new GuildBankLogQueryResults();
		guildBankLogQueryResults.Tab = packet.ReadUInt8();
		byte b = packet.ReadUInt8();
		for (byte b2 = 0; b2 < b; b2++)
		{
			GuildBankLogEntry guildBankLogEntry = new GuildBankLogEntry();
			guildBankLogEntry.EntryType = packet.ReadInt8();
			guildBankLogEntry.PlayerGUID = packet.ReadGuid().To128(GetSession().GameState);
			if (guildBankLogQueryResults.Tab != 6)
			{
				guildBankLogEntry.ItemID = packet.ReadInt32();
				guildBankLogEntry.Count = packet.ReadUInt8();
				if (guildBankLogEntry.EntryType == 3 || guildBankLogEntry.EntryType == 7)
				{
					guildBankLogEntry.OtherTab = packet.ReadInt8();
				}
			}
			else
			{
				guildBankLogEntry.Money = packet.ReadUInt32();
			}
			guildBankLogEntry.TimeOffset = packet.ReadUInt32();
			guildBankLogQueryResults.Entry.Add(guildBankLogEntry);
		}
		SendPacketToClient(guildBankLogQueryResults);
	}

	[PacketHandler(Opcode.MSG_GUILD_BANK_MONEY_WITHDRAWN)]
	private void HandleGuildBankMoneyWithdrawn(WorldPacket packet)
	{
		GuildBankRemainingWithdrawMoney guildBankRemainingWithdrawMoney = new GuildBankRemainingWithdrawMoney();
		guildBankRemainingWithdrawMoney.RemainingWithdrawMoney = packet.ReadUInt32();
		SendPacketToClient(guildBankRemainingWithdrawMoney);
	}

	[PacketHandler(Opcode.SMSG_UPDATE_INSTANCE_OWNERSHIP)]
	private void HandleUpdateInstanceOwnership(WorldPacket packet)
	{
		UpdateInstanceOwnership updateInstanceOwnership = new UpdateInstanceOwnership();
		updateInstanceOwnership.IOwnInstance = packet.ReadUInt32();
		SendPacketToClient(updateInstanceOwnership);
	}

	[PacketHandler(Opcode.SMSG_INSTANCE_RESET)]
	private void HandleInstanceReset(WorldPacket packet)
	{
		InstanceReset instanceReset = new InstanceReset();
		instanceReset.MapID = packet.ReadUInt32();
		SendPacketToClient(instanceReset);
	}

	[PacketHandler(Opcode.SMSG_INSTANCE_RESET_FAILED)]
	private void HandleInstanceResetFailed(WorldPacket packet)
	{
		InstanceResetFailed instanceResetFailed = new InstanceResetFailed();
		instanceResetFailed.ResetFailedReason = (ResetFailedReason)packet.ReadUInt32();
		instanceResetFailed.MapID = packet.ReadUInt32();
		SendPacketToClient(instanceResetFailed);
	}

	[PacketHandler(Opcode.SMSG_RESET_FAILED_NOTIFY)]
	private void HandleResetFailedNotify(WorldPacket packet)
	{
		ResetFailedNotify packet2 = new ResetFailedNotify();
		packet.ReadUInt32();
		SendPacketToClient(packet2);
	}

	[PacketHandler(Opcode.SMSG_RAID_INSTANCE_INFO)]
	private void HandleRaidInstanceInfo(WorldPacket packet)
	{
		RaidInstanceInfo raidInstanceInfo = new RaidInstanceInfo();
		int num = packet.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			InstanceLock instanceLock = new InstanceLock();
			instanceLock.MapID = packet.ReadUInt32();
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
			{
				instanceLock.DifficultyID = (DifficultyModern)packet.ReadUInt32();
			}
			else if (ModernVersion.ExpansionVersion == 1)
			{
				instanceLock.DifficultyID = DifficultyModern.Raid40;
			}
			else
			{
				instanceLock.DifficultyID = DifficultyModern.Raid25N;
			}
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
			{
				instanceLock.InstanceID = packet.ReadUInt64();
				instanceLock.Locked = packet.ReadBool();
				instanceLock.Extended = packet.ReadBool();
				instanceLock.TimeRemaining = packet.ReadInt32();
			}
			else
			{
				instanceLock.TimeRemaining = packet.ReadInt32();
				instanceLock.InstanceID = packet.ReadUInt32();
				if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
				{
					packet.ReadUInt32();
				}
			}
			raidInstanceInfo.LockList.Add(instanceLock);
		}
		SendPacketToClient(raidInstanceInfo);
	}

    // 处理实例保存已创建
    [PacketHandler(Opcode.SMSG_INSTANCE_SAVE_CREATED)]
	private void HandleInstanceSaveCreated(WorldPacket packet)
	{
		InstanceSaveCreated instanceSaveCreated = new InstanceSaveCreated();
		instanceSaveCreated.Gm = packet.ReadUInt32() != 0;
		SendPacketToClient(instanceSaveCreated);
	}

	[PacketHandler(Opcode.SMSG_RAID_GROUP_ONLY)]
	private void HandleRaidGroupOnly(WorldPacket packet)
	{
		RaidGroupOnly raidGroupOnly = new RaidGroupOnly();
		raidGroupOnly.Delay = packet.ReadInt32();
		raidGroupOnly.Reason = (RaidGroupReason)packet.ReadUInt32();
		SendPacketToClient(raidGroupOnly);
	}

	[PacketHandler(Opcode.SMSG_RAID_INSTANCE_MESSAGE)]
	private void HandleRaidInstanceMessage(WorldPacket packet)
	{
		RaidInstanceMessage raidInstanceMessage = new RaidInstanceMessage();
		raidInstanceMessage.Type = (InstanceResetWarningType)packet.ReadUInt32();
		raidInstanceMessage.MapID = packet.ReadUInt32();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			raidInstanceMessage.DifficultyID = (DifficultyModern)packet.ReadUInt32();
		}
		else if (ModernVersion.ExpansionVersion == 1)
		{
			raidInstanceMessage.DifficultyID = DifficultyModern.Raid40;
		}
		else
		{
			raidInstanceMessage.DifficultyID = DifficultyModern.Raid25N;
		}
		packet.ReadUInt32();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056) && raidInstanceMessage.Type == InstanceResetWarningType.Welcome)
		{
			raidInstanceMessage.Locked = packet.ReadBool();
			raidInstanceMessage.Extended = packet.ReadBool();
		}
		SendPacketToClient(raidInstanceMessage);
	}

	[PacketHandler(Opcode.SMSG_SET_PROFICIENCY)]
	private void HandleSetProficiency(WorldPacket packet)
	{
		SetProficiency setProficiency = new SetProficiency();
		setProficiency.ProficiencyClass = packet.ReadUInt8();
		setProficiency.ProficiencyMask = packet.ReadUInt32();
		SendPacketToClient(setProficiency);
	}

	// 处理购买成功
	[PacketHandler(Opcode.SMSG_BUY_SUCCEEDED)]
	private void HandleBuySucceeded(WorldPacket packet)
	{
		BuySucceeded buySucceeded = new BuySucceeded();
		buySucceeded.VendorGUID = packet.ReadGuid().To128(GetSession().GameState);
		buySucceeded.Slot = packet.ReadUInt32();
		buySucceeded.NewQuantity = packet.ReadInt32();
		buySucceeded.QuantityBought = packet.ReadUInt32();
		SendPacketToClient(buySucceeded);
	}

    // 处理物品推送结果
    [PacketHandler(Opcode.SMSG_ITEM_PUSH_RESULT)]
	private void HandleItemPushResult(WorldPacket packet)
	{
		ItemPushResult itemPushResult = new ItemPushResult();
		itemPushResult.PlayerGUID = packet.ReadGuid().To128(GetSession().GameState);
		bool num = packet.ReadUInt32() == 1;
		itemPushResult.Created = packet.ReadUInt32() == 1;
		bool flag = packet.ReadUInt32() == 1;
		if (num && !itemPushResult.Created)
		{
			itemPushResult.DisplayText = ItemPushResult.DisplayType.Received;
			itemPushResult.Pushed = true;
		}
		else if (!flag)
		{
			itemPushResult.DisplayText = ItemPushResult.DisplayType.Hidden;
		}
		else
		{
			itemPushResult.DisplayText = ItemPushResult.DisplayType.Loot;
		}
		itemPushResult.Slot = packet.ReadUInt8();
		itemPushResult.SlotInBag = packet.ReadInt32();
		itemPushResult.Item.ItemID = packet.ReadUInt32();
		itemPushResult.Item.RandomPropertiesSeed = packet.ReadUInt32();
		itemPushResult.Item.RandomPropertiesID = packet.ReadUInt32();
		itemPushResult.Quantity = packet.ReadUInt32();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			itemPushResult.QuantityInInventory = packet.ReadUInt32();
		}
		else
		{
			uint num2 = 0u;
			QuestObjective questObjectiveForItem = GameData.GetQuestObjectiveForItem(itemPushResult.Item.ItemID);
			if (questObjectiveForItem != null)
			{
				Dictionary<int, UpdateField> cachedObjectFieldsLegacy = GetSession().GameState.GetCachedObjectFieldsLegacy(GetSession().GameState.CurrentPlayerGuid);
				int questLogSize = LegacyVersion.GetQuestLogSize();
				for (int i = 0; i < questLogSize; i++)
				{
					QuestLog questLog = ReadQuestLogEntry(i, null, cachedObjectFieldsLegacy);
					if (questLog != null && questLog.QuestID.HasValue && questLog.QuestID == questObjectiveForItem.QuestID && questLog.ObjectiveProgress[questObjectiveForItem.StorageIndex].HasValue)
					{
						num2 = (uint)questLog.ObjectiveProgress[questObjectiveForItem.StorageIndex].Value;
						break;
					}
				}
			}
			itemPushResult.QuantityInInventory = itemPushResult.Quantity + num2;
		}
		if (itemPushResult.Slot == byte.MaxValue && itemPushResult.SlotInBag >= 0 && itemPushResult.PlayerGUID == GetSession().GameState.CurrentPlayerGuid)
		{
			itemPushResult.ItemGUID = GetSession().GameState.GetInventorySlotItem(itemPushResult.SlotInBag).To128(GetSession().GameState);
		}
		else
		{
			itemPushResult.ItemGUID = WowGuid128.Empty;
		}
		SendPacketToClient(itemPushResult);
	}

	[PacketHandler(Opcode.SMSG_READ_ITEM_RESULT_OK)]
	private void HandleReadItemResultOk(WorldPacket packet)
	{
		ReadItemResultOK readItemResultOK = new ReadItemResultOK();
		readItemResultOK.ItemGUID = packet.ReadGuid().To128(GetSession().GameState);
		SendPacketToClient(readItemResultOK);
	}

	[PacketHandler(Opcode.SMSG_READ_ITEM_RESULT_FAILED)]
	private void HandleReadItemResultFailed(WorldPacket packet)
	{
		ReadItemResultFailed readItemResultFailed = new ReadItemResultFailed();
		readItemResultFailed.ItemGUID = packet.ReadGuid().To128(GetSession().GameState);
		readItemResultFailed.Subcode = 2;
		SendPacketToClient(readItemResultFailed);
	}

    // 处理购买失败
    [PacketHandler(Opcode.SMSG_BUY_FAILED)]
	private void HandleBuyFailed(WorldPacket packet)
	{
		BuyFailed buyFailed = new BuyFailed();
		buyFailed.VendorGUID = packet.ReadGuid().To128(GetSession().GameState);
		buyFailed.Slot = packet.ReadUInt32();
		buyFailed.Reason = (BuyResult)packet.ReadUInt8();
		SendPacketToClient(buyFailed);
	}

    // 处理库存变更失败 香草
    [PacketHandler(Opcode.SMSG_INVENTORY_CHANGE_FAILURE, ClientVersionBuild.Zero, ClientVersionBuild.V2_0_1_6180)]
	private void HandleInventoryChangeFailureVanilla(WorldPacket packet)
	{
		InventoryChangeFailure inventoryChangeFailure = new InventoryChangeFailure();
		inventoryChangeFailure.BagResult = LegacyVersion.ConvertInventoryResult(packet.ReadUInt8());
		if (inventoryChangeFailure.BagResult != 0)
		{
			if (inventoryChangeFailure.BagResult == InventoryResult.CantEquipLevel)
			{
				inventoryChangeFailure.Level = packet.ReadInt32();
			}
			inventoryChangeFailure.Item[0] = packet.ReadGuid().To128(GetSession().GameState);
			inventoryChangeFailure.Item[1] = packet.ReadGuid().To128(GetSession().GameState);
			inventoryChangeFailure.ContainerBSlot = packet.ReadUInt8();
			SendPacketToClient(inventoryChangeFailure);
			if (GetSession().GameState.CurrentClientNormalCast != null && !GetSession().GameState.CurrentClientNormalCast.HasStarted && GetSession().GameState.CurrentClientNormalCast.ItemGUID == inventoryChangeFailure.Item[0])
			{
				GetSession().InstanceSocket.SendCastRequestFailed(GetSession().GameState.CurrentClientNormalCast, isPet: false);
				GetSession().GameState.CurrentClientNormalCast = null;
			}
		}
	}

	[PacketHandler(Opcode.SMSG_INVENTORY_CHANGE_FAILURE, ClientVersionBuild.V2_0_1_6180)]
	private void HandleInventoryChangeFailure(WorldPacket packet)
	{
		InventoryChangeFailure inventoryChangeFailure = new InventoryChangeFailure();
		inventoryChangeFailure.BagResult = LegacyVersion.ConvertInventoryResult(packet.ReadUInt8());
		if (inventoryChangeFailure.BagResult != 0)
		{
			inventoryChangeFailure.Item[0] = packet.ReadGuid().To128(GetSession().GameState);
			inventoryChangeFailure.Item[1] = packet.ReadGuid().To128(GetSession().GameState);
			inventoryChangeFailure.ContainerBSlot = packet.ReadUInt8();
			switch (inventoryChangeFailure.BagResult)
			{
			case InventoryResult.CantEquipLevel:
			case InventoryResult.PurchaseLevelTooLow:
				inventoryChangeFailure.Level = packet.ReadInt32();
				break;
			case InventoryResult.EventAutoEquipBindConfirm:
				inventoryChangeFailure.SrcContainer = packet.ReadGuid().To128(GetSession().GameState);
				inventoryChangeFailure.SrcSlot = packet.ReadInt32();
				inventoryChangeFailure.DstContainer = packet.ReadGuid().To128(GetSession().GameState);
				break;
			case InventoryResult.ItemMaxLimitCategoryCountExceeded:
			case InventoryResult.ItemMaxLimitCategorySocketedExceeded:
			case InventoryResult.ItemMaxLimitCategoryEquippedExceeded:
				inventoryChangeFailure.LimitCategory = packet.ReadInt32();
				break;
			}
			SendPacketToClient(inventoryChangeFailure);
			if (GetSession().GameState.CurrentClientNormalCast != null && !GetSession().GameState.CurrentClientNormalCast.HasStarted && GetSession().GameState.CurrentClientNormalCast.ItemGUID == inventoryChangeFailure.Item[0])
			{
				GetSession().InstanceSocket.SendCastRequestFailed(GetSession().GameState.CurrentClientNormalCast, isPet: false);
				GetSession().GameState.CurrentClientNormalCast = null;
			}
		}
	}

	[PacketHandler(Opcode.SMSG_DURABILITY_DAMAGE_DEATH)]
	private void HandleDurabilityDamageDeath(WorldPacket packet)
	{
		DurabilityDamageDeath durabilityDamageDeath = new DurabilityDamageDeath();
		durabilityDamageDeath.Percent = 10u;
		SendPacketToClient(durabilityDamageDeath);
	}

	// 处理物品冷却CD
	[PacketHandler(Opcode.SMSG_ITEM_COOLDOWN)]
	private void HandleItemCooldown(WorldPacket packet)
	{
		ItemCooldown itemCooldown = new ItemCooldown();
		itemCooldown.ItemGuid = packet.ReadGuid().To128(GetSession().GameState);
		itemCooldown.SpellID = packet.ReadUInt32();
		itemCooldown.Cooldown = 30000u;
		SendPacketToClient(itemCooldown);
	}

    // 处理卖出反应
    [PacketHandler(Opcode.SMSG_SELL_RESPONSE)]
	private void HandleSellResponse(WorldPacket packet)
	{
		SellResponse sellResponse = new SellResponse();
		sellResponse.VendorGUID = packet.ReadGuid().To128(GetSession().GameState);
		sellResponse.ItemGUID = packet.ReadGuid().To128(GetSession().GameState);
		sellResponse.Reason = packet.ReadUInt8();
		SendPacketToClient(sellResponse);
	}

    // 处理物品附魔时间更新
    [PacketHandler(Opcode.SMSG_ITEM_ENCHANT_TIME_UPDATE)]
	private void HandleItemEnchantTimeUpdate(WorldPacket packet)
	{
		ItemEnchantTimeUpdate itemEnchantTimeUpdate = new ItemEnchantTimeUpdate();
		itemEnchantTimeUpdate.ItemGuid = packet.ReadGuid().To128(GetSession().GameState);
		itemEnchantTimeUpdate.Slot = packet.ReadUInt32();
		itemEnchantTimeUpdate.DurationLeft = packet.ReadUInt32();
		itemEnchantTimeUpdate.OwnerGuid = packet.ReadGuid().To128(GetSession().GameState);
		SendPacketToClient(itemEnchantTimeUpdate);
	}

	[PacketHandler(Opcode.SMSG_ENCHANTMENT_LOG)]
	private void HandleEnchantmentLog(WorldPacket packet)
	{
		EnchantmentLog enchantmentLog = new EnchantmentLog();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			enchantmentLog.Owner = packet.ReadPackedGuid().To128(GetSession().GameState);
			enchantmentLog.Caster = packet.ReadPackedGuid().To128(GetSession().GameState);
		}
		else
		{
			enchantmentLog.Owner = packet.ReadGuid().To128(GetSession().GameState);
			enchantmentLog.Caster = packet.ReadGuid().To128(GetSession().GameState);
		}
		enchantmentLog.ItemID = packet.ReadInt32();
		GameSessionData gameState = GetSession().GameState;
		for (int i = 0; i < 23; i++)
		{
			if (gameState.GetItemId(gameState.GetInventorySlotItem(i).To128(gameState)).Equals((uint)enchantmentLog.ItemID))
			{
				enchantmentLog.ItemGUID = gameState.GetInventorySlotItem(i).To128(gameState);
				break;
			}
		}
		if (!(enchantmentLog.ItemGUID == null))
		{
			enchantmentLog.Enchantment = packet.ReadInt32();
			SendPacketToClient(enchantmentLog);
		}
	}

    // 处理战利品响应
    [PacketHandler(Opcode.SMSG_LOOT_RESPONSE)]
	private void HandleLootResponse(WorldPacket packet)
	{
		LootResponse lootResponse = new LootResponse();
		GetSession().GameState.LastLootTargetGuid = packet.ReadGuid();
		lootResponse.Owner = GetSession().GameState.LastLootTargetGuid.To128(GetSession().GameState);
		lootResponse.LootObj = GetSession().GameState.LastLootTargetGuid.ToLootGuid();
		lootResponse.AcquireReason = (LootType)packet.ReadUInt8();
		if (lootResponse.AcquireReason == LootType.None)
		{
			lootResponse.FailureReason = (LootError)packet.ReadUInt8();
			return;
		}
		lootResponse.LootMethod = GetSession().GameState.GetCurrentLootMethod();
		lootResponse.Coins = packet.ReadUInt32();
		byte b = packet.ReadUInt8();
		for (int i = 0; i < b; i++)
		{
			LootItemData lootItemData = new LootItemData();
			lootItemData.LootListID = packet.ReadUInt8();
			lootItemData.Loot.ItemID = packet.ReadUInt32();
			lootItemData.Quantity = packet.ReadUInt32();
			packet.ReadUInt32();
			lootItemData.Loot.RandomPropertiesSeed = packet.ReadUInt32();
			lootItemData.Loot.RandomPropertiesID = packet.ReadUInt32();
			LootSlotTypeLegacy lootSlotTypeLegacy = (LootSlotTypeLegacy)packet.ReadUInt8();
			lootItemData.UIType = (LootSlotTypeModern)Enum.Parse(typeof(LootSlotTypeModern), lootSlotTypeLegacy.ToString());
			lootResponse.Items.Add(lootItemData);
		}
		SendPacketToClient(lootResponse);
	}
    // 处理战利品释放
    [PacketHandler(Opcode.SMSG_LOOT_RELEASE)]
	private void HandleLootRelease(WorldPacket packet)
	{
		LootReleaseResponse lootReleaseResponse = new LootReleaseResponse();
		WowGuid64 wowGuid = packet.ReadGuid();
		lootReleaseResponse.Owner = wowGuid.To128(GetSession().GameState);
		lootReleaseResponse.LootObj = wowGuid.ToLootGuid();
		packet.ReadBool();
		SendPacketToClient(lootReleaseResponse);
	}

	// 处理战利品删除
	[PacketHandler(Opcode.SMSG_LOOT_REMOVED)]
	private void HandleLootRemoved(WorldPacket packet)
	{
		LootRemoved lootRemoved = new LootRemoved();
		lootRemoved.Owner = GetSession().GameState.LastLootTargetGuid.To128(GetSession().GameState);
		lootRemoved.LootObj = GetSession().GameState.LastLootTargetGuid.ToLootGuid();
		lootRemoved.LootListID = packet.ReadUInt8();
		SendPacketToClient(lootRemoved);
	}

	[PacketHandler(Opcode.SMSG_LOOT_MONEY_NOTIFY)]
	private void HandleLootMoneyNotify(WorldPacket packet)
	{
		LootMoneyNotify lootMoneyNotify = new LootMoneyNotify();
		lootMoneyNotify.Money = packet.ReadUInt32();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			lootMoneyNotify.SoleLooter = packet.ReadBool();
		}
		SendPacketToClient(lootMoneyNotify);
	}

	[PacketHandler(Opcode.SMSG_LOOT_CLEAR_MONEY)]
	private void HandleLootCelarMoney(WorldPacket packet)
	{
		CoinRemoved coinRemoved = new CoinRemoved();
		coinRemoved.LootObj = GetSession().GameState.LastLootTargetGuid.ToLootGuid();
		SendPacketToClient(coinRemoved);
	}

	// 处理战利品roll点
	[PacketHandler(Opcode.SMSG_LOOT_START_ROLL)]
	private void HandleLootStartRoll(WorldPacket packet)
	{
		StartLootRoll startLootRoll = new StartLootRoll();
		WowGuid64 wowGuid = packet.ReadGuid();
		startLootRoll.LootObj = wowGuid.ToLootGuid();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			startLootRoll.MapID = packet.ReadUInt32();
		}
		else
		{
			startLootRoll.MapID = GetSession().GameState.CurrentMapId.Value;
		}
		startLootRoll.Item.LootListID = (byte)packet.ReadUInt32();
		startLootRoll.Item.Loot.ItemID = packet.ReadUInt32();
		startLootRoll.Item.Loot.RandomPropertiesSeed = packet.ReadUInt32();
		startLootRoll.Item.Loot.RandomPropertiesID = packet.ReadUInt32();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			startLootRoll.Item.Quantity = packet.ReadUInt32();
		}
		else
		{
			startLootRoll.Item.Quantity = 1u;
		}
		startLootRoll.RollTime = packet.ReadUInt32();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			startLootRoll.ValidRolls = (RollMask)packet.ReadUInt8();
		}
		else
		{
			startLootRoll.ValidRolls = RollMask.AllNoDisenchant;
		}
		SendPacketToClient(startLootRoll);
		if (GetSession().GameState.IsPassingOnLoot)
		{
			WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_LOOT_ROLL);
			worldPacket.WriteGuid(wowGuid);
			worldPacket.WriteUInt32(startLootRoll.Item.LootListID);
			worldPacket.WriteUInt8(0);
			SendPacketToServer(worldPacket);
		}
	}

	[PacketHandler(Opcode.SMSG_LOOT_ROLL)]
	private void HandleLootRoll(WorldPacket packet)
	{
		LootRollBroadcast lootRollBroadcast = new LootRollBroadcast();
		WowGuid64 wowGuid = packet.ReadGuid();
		lootRollBroadcast.LootObj = wowGuid.ToLootGuid();
		lootRollBroadcast.Item.LootListID = (byte)packet.ReadUInt32();
		lootRollBroadcast.Player = packet.ReadGuid().To128(GetSession().GameState);
		lootRollBroadcast.Item.Loot.ItemID = packet.ReadUInt32();
		lootRollBroadcast.Item.Loot.RandomPropertiesSeed = packet.ReadUInt32();
		lootRollBroadcast.Item.Loot.RandomPropertiesID = packet.ReadUInt32();
		lootRollBroadcast.Item.Quantity = 1u;
		lootRollBroadcast.Roll = packet.ReadUInt8();
		byte b = packet.ReadUInt8();
		if (lootRollBroadcast.Roll == 128 && b == 128)
		{
			lootRollBroadcast.RollType = RollType.Pass;
		}
		else if (lootRollBroadcast.Roll == 0 && b == 0)
		{
			lootRollBroadcast.RollType = RollType.Need;
		}
		else
		{
			lootRollBroadcast.RollType = (RollType)b;
		}
		if (lootRollBroadcast.Roll == 128)
		{
			lootRollBroadcast.Roll = 0;
		}
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			lootRollBroadcast.Autopassed = packet.ReadBool();
		}
		SendPacketToClient(lootRollBroadcast);
	}

	[PacketHandler(Opcode.SMSG_LOOT_ROLL_WON)]
	private void HandleLootRollWon(WorldPacket packet)
	{
		LootRollWon lootRollWon = new LootRollWon();
		lootRollWon.LootObj = packet.ReadGuid().ToLootGuid();
		lootRollWon.Item.LootListID = (byte)packet.ReadUInt32();
		lootRollWon.Item.Loot.ItemID = packet.ReadUInt32();
		lootRollWon.Item.Loot.RandomPropertiesSeed = packet.ReadUInt32();
		lootRollWon.Item.Loot.RandomPropertiesID = packet.ReadUInt32();
		lootRollWon.Item.Quantity = 1u;
		lootRollWon.Winner = packet.ReadGuid().To128(GetSession().GameState);
		lootRollWon.Roll = packet.ReadUInt8();
		lootRollWon.RollType = (RollType)packet.ReadUInt8();
		if (lootRollWon.RollType == RollType.Need)
		{
			lootRollWon.MainSpec = 128;
		}
		SendPacketToClient(lootRollWon);
		LootRollsComplete lootRollsComplete = new LootRollsComplete();
		lootRollsComplete.LootObj = lootRollWon.LootObj;
		lootRollsComplete.LootListID = lootRollWon.Item.LootListID;
		SendPacketToClient(lootRollsComplete);
	}

	[PacketHandler(Opcode.SMSG_LOOT_ALL_PASSED)]
	private void HandleLootAllPassed(WorldPacket packet)
	{
		LootAllPassed lootAllPassed = new LootAllPassed();
		lootAllPassed.LootObj = packet.ReadGuid().ToLootGuid();
		lootAllPassed.Item.LootListID = (byte)packet.ReadUInt32();
		lootAllPassed.Item.Loot.ItemID = packet.ReadUInt32();
		lootAllPassed.Item.Loot.RandomPropertiesSeed = packet.ReadUInt32();
		lootAllPassed.Item.Loot.RandomPropertiesID = packet.ReadUInt32();
		lootAllPassed.Item.Quantity = 1u;
		SendPacketToClient(lootAllPassed);
		LootRollsComplete lootRollsComplete = new LootRollsComplete();
		lootRollsComplete.LootObj = lootAllPassed.LootObj;
		lootRollsComplete.LootListID = lootAllPassed.Item.LootListID;
		SendPacketToClient(lootRollsComplete);
	}

	[PacketHandler(Opcode.SMSG_LOOT_MASTER_LIST)]
	private void HandleLootMasterList(WorldPacket packet)
	{
		if (!(GetSession().GameState.LastLootTargetGuid == null))
		{
			LootList lootList = new LootList();
			lootList.Owner = GetSession().GameState.LastLootTargetGuid.To128(GetSession().GameState);
			lootList.LootObj = GetSession().GameState.LastLootTargetGuid.ToLootGuid();
			lootList.Master = GetSession().GameState.CurrentPlayerGuid;
			SendPacketToClient(lootList);
			MasterLootCandidateList masterLootCandidateList = new MasterLootCandidateList();
			masterLootCandidateList.LootObj = GetSession().GameState.LastLootTargetGuid.ToLootGuid();
			byte b = packet.ReadUInt8();
			for (byte b2 = 0; b2 < b; b2++)
			{
				WowGuid128 wowGuid = packet.ReadGuid().To128(GetSession().GameState);
				masterLootCandidateList.Players.Add(wowGuid);
			}
			SendPacketToClient(masterLootCandidateList);
		}
	}

	// 邮件通知
	[PacketHandler(Opcode.SMSG_NOTIFY_RECEIVED_MAIL)]
	private void HandleNotifyReceivedMail(WorldPacket packet)
	{
		NotifyReceivedMail notifyReceivedMail = new NotifyReceivedMail();
		notifyReceivedMail.Delay = packet.ReadFloat();
		SendPacketToClient(notifyReceivedMail);
	}

	[PacketHandler(Opcode.MSG_QUERY_NEXT_MAIL_TIME)]
	private void HandleQueryNextMailTime(WorldPacket packet)
	{
		MailQueryNextTimeResult mailQueryNextTimeResult = new MailQueryNextTimeResult();
		mailQueryNextTimeResult.NextMailTime = packet.ReadFloat();
		if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_3_0_7561))
		{
			if (mailQueryNextTimeResult.NextMailTime == 0f)
			{
				MailQueryNextTimeResult.MailNextTimeEntry mailNextTimeEntry = new MailQueryNextTimeResult.MailNextTimeEntry();
				mailNextTimeEntry.SenderGuid = GetSession().GameState.CurrentPlayerGuid;
				mailNextTimeEntry.AltSenderID = 0;
				mailNextTimeEntry.AltSenderType = 0;
				mailNextTimeEntry.StationeryID = 41;
				mailNextTimeEntry.TimeLeft = 3600f;
				mailQueryNextTimeResult.Mails.Add(mailNextTimeEntry);
			}
		}
		else
		{
			uint num = packet.ReadUInt32();
			for (int i = 0; i < num; i++)
			{
				MailQueryNextTimeResult.MailNextTimeEntry mailNextTimeEntry2 = new MailQueryNextTimeResult.MailNextTimeEntry();
				mailNextTimeEntry2.SenderGuid = packet.ReadGuid().To128(GetSession().GameState);
				mailNextTimeEntry2.AltSenderID = packet.ReadInt32();
				mailNextTimeEntry2.AltSenderType = (sbyte)packet.ReadInt32();
				mailNextTimeEntry2.StationeryID = packet.ReadInt32();
				mailNextTimeEntry2.TimeLeft = packet.ReadFloat();
				mailQueryNextTimeResult.Mails.Add(mailNextTimeEntry2);
			}
		}
		SendPacketToClient(mailQueryNextTimeResult);
	}

	[PacketHandler(Opcode.SMSG_MAIL_LIST_RESULT)]
	private void HandleMailListResult(WorldPacket packet)
	{
		MailListResult mailListResult = new MailListResult();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
		{
			mailListResult.TotalNumRecords = packet.ReadInt32();
		}
		byte b = packet.ReadUInt8();
		if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_2_0_10192))
		{
			mailListResult.TotalNumRecords = b;
		}
		for (int i = 0; i < b; i++)
		{
			MailListEntry mailListEntry = new MailListEntry();
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
			{
				packet.ReadUInt16();
			}
			mailListEntry.MailID = packet.ReadInt32();
			mailListEntry.SenderType = (MailType)packet.ReadUInt8();
			switch (mailListEntry.SenderType)
			{
			case MailType.Normal:
				mailListEntry.SenderCharacter = packet.ReadGuid().To128(GetSession().GameState);
				break;
			case MailType.Item:
				if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
				{
					mailListEntry.AltSenderID = packet.ReadUInt32();
				}
				break;
			default:
				mailListEntry.AltSenderID = packet.ReadUInt32();
				break;
			}
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
			{
				mailListEntry.Cod = packet.ReadUInt32();
			}
			else
			{
				mailListEntry.Subject = packet.ReadCString();
			}
			if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_3_0_10958))
			{
				mailListEntry.ItemTextId = packet.ReadUInt32();
				if (mailListEntry.ItemTextId != 0 && !GetSession().GameState.ItemTexts.ContainsKey(mailListEntry.ItemTextId))
				{
					GetSession().GameState.RequestedItemTextIds.Add(mailListEntry.ItemTextId);
					WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_ITEM_TEXT_QUERY);
					worldPacket.WriteUInt32(mailListEntry.ItemTextId);
					worldPacket.WriteInt32(mailListEntry.MailID);
					worldPacket.WriteUInt32(0u);
					SendPacket(worldPacket);
				}
			}
			packet.ReadUInt32();
			mailListEntry.StationeryID = packet.ReadInt32();
			if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
			{
				MailAttachedItem mailAttachedItem = ReadMailItem(packet);
				if (mailAttachedItem.Item.ItemID != 0)
				{
					mailAttachedItem.AttachID = 1;
					mailListEntry.Attachments.Add(mailAttachedItem);
				}
			}
			mailListEntry.SentMoney = packet.ReadUInt32();
			if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
			{
				mailListEntry.Cod = packet.ReadUInt32();
			}
			mailListEntry.Flags = packet.ReadUInt32();
			mailListEntry.DaysLeft = packet.ReadFloat();
			mailListEntry.MailTemplateID = packet.ReadInt32();
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
			{
				mailListEntry.Subject = packet.ReadCString();
			}
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
			{
				mailListEntry.Body = packet.ReadCString();
			}
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
			{
				byte b2 = packet.ReadUInt8();
				for (int j = 0; j < b2; j++)
				{
					MailAttachedItem mailAttachedItem2 = ReadMailItem(packet);
					mailListEntry.Attachments.Add(mailAttachedItem2);
				}
			}
			mailListResult.Mails.Add(mailListEntry);
		}
		if (GetSession().GameState.RequestedItemTextIds.Count == 0)
		{
			foreach (MailListEntry mail in mailListResult.Mails)
			{
				if (mail.ItemTextId != 0)
				{
					mail.Body = GetSession().GameState.ItemTexts[mail.ItemTextId];
				}
			}
			SendPacketToClient(mailListResult);
		}
		else
		{
			GetSession().GameState.PendingMailListPacket = mailListResult;
		}
	}

	[PacketHandler(Opcode.SMSG_QUERY_ITEM_TEXT_RESPONSE)]
	private void HandleQueryItemTextResponse(WorldPacket packet)
	{
		uint num = packet.ReadUInt32();
		string text = packet.ReadCString();
		if (GetSession().GameState.ItemTexts.ContainsKey(num))
		{
			GetSession().GameState.ItemTexts[num] = text;
		}
		else
		{
			GetSession().GameState.ItemTexts.Add(num, text);
		}
		if (GetSession().GameState.RequestedItemTextIds.Contains(num))
		{
			GetSession().GameState.RequestedItemTextIds.Remove(num);
		}
		if (GetSession().GameState.PendingMailListPacket == null || GetSession().GameState.RequestedItemTextIds.Count != 0)
		{
			return;
		}
		MailListResult pendingMailListPacket = GetSession().GameState.PendingMailListPacket;
		foreach (MailListEntry mail in pendingMailListPacket.Mails)
		{
			if (mail.ItemTextId != 0)
			{
				mail.Body = GetSession().GameState.ItemTexts[mail.ItemTextId];
			}
		}
		SendPacketToClient(pendingMailListPacket);
	}

	private MailAttachedItem ReadMailItem(WorldPacket packet)
	{
		MailAttachedItem mailAttachedItem = new MailAttachedItem();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			mailAttachedItem.Position = packet.ReadUInt8();
			mailAttachedItem.AttachID = packet.ReadInt32();
		}
		mailAttachedItem.Item.ItemID = packet.ReadUInt32();
		byte b = (byte)(LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056) ? 7 : ((!LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180)) ? 1 : 6));
		for (byte b2 = 0; b2 < b; b2++)
		{
			ItemEnchantData itemEnchantData = new ItemEnchantData();
			itemEnchantData.Slot = b2;
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
			{
				itemEnchantData.Charges = packet.ReadInt32();
				itemEnchantData.Expiration = packet.ReadUInt32();
			}
			itemEnchantData.ID = packet.ReadUInt32();
			if (itemEnchantData.ID != 0)
			{
				mailAttachedItem.Enchants.Add(itemEnchantData);
			}
		}
		mailAttachedItem.Item.RandomPropertiesID = packet.ReadUInt32();
		mailAttachedItem.Item.RandomPropertiesSeed = packet.ReadUInt32();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			mailAttachedItem.Count = (byte)packet.ReadUInt32();
		}
		else
		{
			mailAttachedItem.Count = packet.ReadUInt8();
		}
		mailAttachedItem.Charges = packet.ReadInt32();
		mailAttachedItem.MaxDurability = packet.ReadUInt32();
		mailAttachedItem.Durability = packet.ReadUInt32();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			mailAttachedItem.Unlocked = packet.ReadBool();
		}
		return mailAttachedItem;
	}

	[PacketHandler(Opcode.SMSG_MAIL_COMMAND_RESULT)]
	private void HandleMailCommandResult(WorldPacket packet)
	{
		MailCommandResult mailCommandResult = new MailCommandResult();
		mailCommandResult.MailID = packet.ReadUInt32();
		mailCommandResult.Command = (MailActionType)packet.ReadUInt32();
		mailCommandResult.ErrorCode = (MailErrorType)packet.ReadUInt32();
		if (mailCommandResult.ErrorCode == MailErrorType.Equip)
		{
			mailCommandResult.BagResult = LegacyVersion.ConvertInventoryResult(packet.ReadUInt32());
		}
		else if (mailCommandResult.Command == MailActionType.AttachmentExpired)
		{
			mailCommandResult.AttachID = packet.ReadUInt32();
			mailCommandResult.QtyInInventory = packet.ReadUInt32();
			if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
			{
				mailCommandResult.AttachID = 1u;
			}
		}
		SendPacketToClient(mailCommandResult);
	}

	[PacketHandler(Opcode.SMSG_PONG)]
	private void HandlePingResponse(WorldPacket packet)
	{
		uint serial = packet.ReadUInt32();
		SendPacketToClient(new Pong(serial));
	}

	[PacketHandler(Opcode.SMSG_TUTORIAL_FLAGS)]
	private void HandleTutorialFlags(WorldPacket packet)
	{
		TutorialFlags tutorialFlags = new TutorialFlags();
		for (byte b = 0; b < 8; b++)
		{
			tutorialFlags.TutorialData[b] = packet.ReadUInt32();
		}
		SendPacketToClient(tutorialFlags);
	}

	// 处理账户数据时间
	[PacketHandler(Opcode.SMSG_ACCOUNT_DATA_TIMES)]
	private void HandleAccountDataTimes(WorldPacket packet)
	{
		GetSession().RealmSocket.SendAccountDataTimes();
		if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			GetSession().RealmSocket.SendFeatureSystemStatus();
			GetSession().RealmSocket.SendMotd();
			GetSession().RealmSocket.SendSetTimeZoneInformation();
			GetSession().RealmSocket.SendSeasonInfo();
		}
	}

	[PacketHandler(Opcode.SMSG_BIND_POINT_UPDATE)]
	private void HandleBindPointUpdate(WorldPacket packet)
	{
		BindPointUpdate bindPointUpdate = new BindPointUpdate();
		bindPointUpdate.BindPosition = packet.ReadVector3();
		bindPointUpdate.BindMapID = packet.ReadUInt32();
		bindPointUpdate.BindAreaID = packet.ReadUInt32();
		SendPacketToClient(bindPointUpdate);
	}

    // 处理玩家绑定
    [PacketHandler(Opcode.SMSG_PLAYER_BOUND)]
	private void HandlePlayerBound(WorldPacket packet)
	{
		PlayerBound playerBound = new PlayerBound();
		playerBound.BinderGUID = packet.ReadGuid().To128(GetSession().GameState);
		playerBound.AreaID = packet.ReadUInt32();
		SendPacketToClient(playerBound);
	}

	[PacketHandler(Opcode.SMSG_DEATH_RELEASE_LOC)]
	private void HandleDeathReleaseLoc(WorldPacket packet)
	{
		DeathReleaseLoc deathReleaseLoc = new DeathReleaseLoc();
		deathReleaseLoc.MapID = packet.ReadInt32();
		deathReleaseLoc.Location = packet.ReadVector3();
		SendPacketToClient(deathReleaseLoc);
	}

    // 处理尸体回收延迟
    [PacketHandler(Opcode.SMSG_CORPSE_RECLAIM_DELAY)]
	private void HandleCorpseReclaimDelay(WorldPacket packet)
	{
		CorpseReclaimDelay corpseReclaimDelay = new CorpseReclaimDelay();
		corpseReclaimDelay.Remaining = packet.ReadUInt32();
		SendPacketToClient(corpseReclaimDelay);
	}

    // 处理时间同步请求
    [PacketHandler(Opcode.SMSG_TIME_SYNC_REQUEST)]
	private void HandleTimeSyncRequest(WorldPacket packet)
	{
		TimeSyncRequest timeSyncRequest = new TimeSyncRequest();
		timeSyncRequest.SequenceIndex = packet.ReadUInt32();
		SendPacketToClient(timeSyncRequest);
	}

	// 天气
	[PacketHandler(Opcode.SMSG_WEATHER)]
	private void HandleWeather(WorldPacket packet)
	{
		WeatherPkt weatherPkt = new WeatherPkt();
		if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			WeatherType type = (WeatherType)packet.ReadUInt32();
			weatherPkt.Intensity = packet.ReadFloat();
			weatherPkt.WeatherID = Weather.ConvertWeatherTypeToWeatherState(type, weatherPkt.Intensity);
			packet.ReadUInt32();
			if (packet.CanRead())
			{
				weatherPkt.Abrupt = packet.ReadBool();
			}
		}
		else
		{
			weatherPkt.WeatherID = (WeatherState)packet.ReadUInt32();
			weatherPkt.Intensity = packet.ReadFloat();
			weatherPkt.Abrupt = packet.ReadBool();
		}
		SendPacketToClient(weatherPkt);
		SendPacketToClient(new StartLightningStorm());
	}

	[PacketHandler(Opcode.SMSG_LOGIN_SET_TIME_SPEED)]
	private void HandleLoginSetTimeSpeed(WorldPacket packet)
	{
		if (GetSession().GameState.IsFirstEnterWorld)
		{
			LoginSetTimeSpeed loginSetTimeSpeed = new LoginSetTimeSpeed();
			loginSetTimeSpeed.ServerTime = packet.ReadUInt32();
			loginSetTimeSpeed.GameTime = loginSetTimeSpeed.ServerTime;
			loginSetTimeSpeed.NewSpeed = packet.ReadFloat();
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_2_9901))
			{
				loginSetTimeSpeed.ServerTimeHolidayOffset = packet.ReadInt32();
				loginSetTimeSpeed.GameTimeHolidayOffset = loginSetTimeSpeed.ServerTimeHolidayOffset;
			}
			SendPacketToClient(loginSetTimeSpeed);
		}
	}

    // 处理区域触发消息
    [PacketHandler(Opcode.SMSG_AREA_TRIGGER_MESSAGE)]
	private void HandleAreaTriggerMessage(WorldPacket packet)
	{
		uint length = packet.ReadUInt32();
		string message = packet.ReadString(length);
		if (GetSession().GameState.LastEnteredAreaTrigger != 0)
		{
			AreaTriggerMessage areaTriggerMessage = new AreaTriggerMessage();
			areaTriggerMessage.AreaTriggerID = GetSession().GameState.LastEnteredAreaTrigger;
			SendPacketToClient(areaTriggerMessage);
		}
		else
		{
			ChatPkt packet2 = new ChatPkt(GetSession(), ChatMessageTypeModern.System, message);
			SendPacketToClient(packet2);
		}
	}

    //处理尸体查询
    [PacketHandler(Opcode.MSG_CORPSE_QUERY)]
	private void HandleCorpseQuery(WorldPacket packet)
	{
		CorpseLocation corpseLocation = new CorpseLocation
		{
			Player = GetSession().GameState.CurrentPlayerGuid,
			Transport = WowGuid128.Empty
		};
		corpseLocation.Valid = packet.ReadBool();
		if (corpseLocation.Valid)
		{
			corpseLocation.ActualMapID = packet.ReadInt32();
			corpseLocation.Position = packet.ReadVector3();
			corpseLocation.MapID = packet.ReadInt32();
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_2_10482))
			{
				packet.ReadInt32();
			}
		}
		else
		{
			corpseLocation.MapID = (corpseLocation.ActualMapID = (int)GetSession().GameState.CurrentMapId.Value);
		}
		SendPacketToClient(corpseLocation);
	}

	[PacketHandler(Opcode.SMSG_STAND_STATE_UPDATE)]
	private void HandleStandStateUpdate(WorldPacket packet)
	{
		StandStateUpdate standStateUpdate = new StandStateUpdate();
		standStateUpdate.StandState = packet.ReadUInt8();
		SendPacketToClient(standStateUpdate);
	}

	[PacketHandler(Opcode.SMSG_EXPLORATION_EXPERIENCE)]
	private void HandleExplorationExperience(WorldPacket packet)
	{
		ExplorationExperience explorationExperience = new ExplorationExperience();
		explorationExperience.AreaID = packet.ReadUInt32();
		explorationExperience.Experience = packet.ReadUInt32();
		SendPacketToClient(explorationExperience);
	}

	// 处理玩家音乐
	[PacketHandler(Opcode.SMSG_PLAY_MUSIC)]
	private void HandlePlayMusic(WorldPacket packet)
	{
		PlayMusic playMusic = new PlayMusic();
		playMusic.SoundEntryID = packet.ReadUInt32();
		SendPacketToClient(playMusic);
	}

	// 处理玩家声音
	[PacketHandler(Opcode.SMSG_PLAY_SOUND)]
	private void HandlePlaySound(WorldPacket packet)
	{
		PlaySound playSound = new PlaySound();
		playSound.SoundEntryID = packet.ReadUInt32();
		playSound.SourceObjectGuid = GetSession().GameState.CurrentPlayerGuid;
		SendPacketToClient(playSound);
	}

	[PacketHandler(Opcode.SMSG_PLAY_OBJECT_SOUND)]
	private void HandlePlayObjectSound(WorldPacket packet)
	{
		PlayObjectSound playObjectSound = new PlayObjectSound();
		playObjectSound.SoundEntryID = packet.ReadUInt32();
		playObjectSound.SourceObjectGUID = packet.ReadGuid().To128(GetSession().GameState);
		playObjectSound.TargetObjectGUID = playObjectSound.SourceObjectGUID;
		SendPacketToClient(playObjectSound);
	}

    // HandleTriggerCinematic
    [PacketHandler(Opcode.SMSG_TRIGGER_CINEMATIC)]
	private void HandleTriggerCinematic(WorldPacket packet)
	{
		TriggerCinematic triggerCinematic = new TriggerCinematic();
		triggerCinematic.CinematicID = packet.ReadUInt32();
		SendPacketToClient(triggerCinematic);
	}

    //处理特殊安装动画
    [PacketHandler(Opcode.SMSG_SPECIAL_MOUNT_ANIM)]
	private void HandleSpecialMountAnim(WorldPacket packet)
	{
		SpecialMountAnim specialMountAnim = new SpecialMountAnim();
		specialMountAnim.UnitGUID = packet.ReadGuid().To128(GetSession().GameState);
		SendPacketToClient(specialMountAnim);
	}

	[PacketHandler(Opcode.SMSG_START_MIRROR_TIMER)]
	private void HandleStartMirrorTimer(WorldPacket packet)
	{
		StartMirrorTimer startMirrorTimer = new StartMirrorTimer();
		startMirrorTimer.Timer = (MirrorTimerType)packet.ReadUInt32();
		startMirrorTimer.Value = packet.ReadInt32();
		startMirrorTimer.MaxValue = packet.ReadInt32();
		startMirrorTimer.Scale = packet.ReadInt32();
		startMirrorTimer.Paused = packet.ReadBool();
		startMirrorTimer.SpellID = packet.ReadInt32();
		SendPacketToClient(startMirrorTimer);
	}

	[PacketHandler(Opcode.SMSG_PAUSE_MIRROR_TIMER)]
	private void HandlePauseMirrorTimer(WorldPacket packet)
	{
		PauseMirrorTimer pauseMirrorTimer = new PauseMirrorTimer();
		pauseMirrorTimer.Timer = (MirrorTimerType)packet.ReadUInt32();
		pauseMirrorTimer.Paused = packet.ReadBool();
		SendPacketToClient(pauseMirrorTimer);
	}

	[PacketHandler(Opcode.SMSG_STOP_MIRROR_TIMER)]
	private void HandleStopMirrorTimer(WorldPacket packet)
	{
		StopMirrorTimer stopMirrorTimer = new StopMirrorTimer();
		stopMirrorTimer.Timer = (MirrorTimerType)packet.ReadUInt32();
		SendPacketToClient(stopMirrorTimer);
	}

    // 处理无效玩家
    [PacketHandler(Opcode.SMSG_INVALIDATE_PLAYER)]
	private void HandleInvalidatePlayer(WorldPacket packet)
	{
		InvalidatePlayer invalidatePlayer = new InvalidatePlayer();
		invalidatePlayer.Guid = packet.ReadGuid().To128(GetSession().GameState);
		SendPacketToClient(invalidatePlayer);
		if (GetSession().GameState.CachedPlayers.ContainsKey(invalidatePlayer.Guid))
		{
			GetSession().GameState.CachedPlayers.Remove(invalidatePlayer.Guid);
		}
	}

    //处理受到攻击的区域
    [PacketHandler(Opcode.SMSG_ZONE_UNDER_ATTACK)]
	private void HandleZoneUnderAttack(WorldPacket packet)
	{
		ZoneUnderAttack zoneUnderAttack = new ZoneUnderAttack();
		zoneUnderAttack.AreaID = packet.ReadInt32();
		SendPacketToClient(zoneUnderAttack);
	}

    // 处理设置地下城难度
    [PacketHandler(Opcode.MSG_SET_DUNGEON_DIFFICULTY)]
	private void HandleSetDungeonDifficulty(WorldPacket packet)
	{
		DungeonDifficultySet dungeonDifficultySet = new DungeonDifficultySet();
		int num = packet.ReadInt32();
		dungeonDifficultySet.DifficultyID = (byte)Enum.Parse(typeof(DifficultyModern), ((DifficultyLegacy)num).ToString());
		packet.ReadInt32();
		packet.ReadInt32();
		SendPacketToClient(dungeonDifficultySet);
	}

    // 处理移动消息
    [PacketHandler(Opcode.MSG_MOVE_START_FORWARD)]
	[PacketHandler(Opcode.MSG_MOVE_START_BACKWARD)]
	[PacketHandler(Opcode.MSG_MOVE_STOP)]
	[PacketHandler(Opcode.MSG_MOVE_START_STRAFE_LEFT)]
	[PacketHandler(Opcode.MSG_MOVE_START_STRAFE_RIGHT)]
	[PacketHandler(Opcode.MSG_MOVE_STOP_STRAFE)]
	[PacketHandler(Opcode.MSG_MOVE_START_ASCEND)]
	[PacketHandler(Opcode.MSG_MOVE_START_DESCEND)]
	[PacketHandler(Opcode.MSG_MOVE_STOP_ASCEND)]
	[PacketHandler(Opcode.MSG_MOVE_JUMP)]
	[PacketHandler(Opcode.MSG_MOVE_START_TURN_LEFT)]
	[PacketHandler(Opcode.MSG_MOVE_START_TURN_RIGHT)]
	[PacketHandler(Opcode.MSG_MOVE_STOP_TURN)]
	[PacketHandler(Opcode.MSG_MOVE_START_PITCH_UP)]
	[PacketHandler(Opcode.MSG_MOVE_START_PITCH_DOWN)]
	[PacketHandler(Opcode.MSG_MOVE_STOP_PITCH)]
	[PacketHandler(Opcode.MSG_MOVE_SET_RUN_MODE)]
	[PacketHandler(Opcode.MSG_MOVE_SET_WALK_MODE)]
	[PacketHandler(Opcode.MSG_MOVE_TELEPORT)]
	[PacketHandler(Opcode.MSG_MOVE_SET_FACING)]
	[PacketHandler(Opcode.MSG_MOVE_SET_PITCH)]
	[PacketHandler(Opcode.MSG_MOVE_TOGGLE_COLLISION_CHEAT)]
	[PacketHandler(Opcode.MSG_MOVE_GRAVITY_CHNG)]
	[PacketHandler(Opcode.MSG_MOVE_ROOT)]
	[PacketHandler(Opcode.MSG_MOVE_UNROOT)]
	[PacketHandler(Opcode.MSG_MOVE_START_SWIM)]
	[PacketHandler(Opcode.MSG_MOVE_STOP_SWIM)]
	[PacketHandler(Opcode.MSG_MOVE_START_SWIM_CHEAT)]
	[PacketHandler(Opcode.MSG_MOVE_STOP_SWIM_CHEAT)]
	[PacketHandler(Opcode.MSG_MOVE_HEARTBEAT)]
	[PacketHandler(Opcode.MSG_MOVE_FALL_LAND)]
	[PacketHandler(Opcode.MSG_MOVE_UPDATE_CAN_FLY)]
	[PacketHandler(Opcode.MSG_MOVE_UPDATE_CAN_TRANSITION_BETWEEN_SWIM_AND_FLY)]
	[PacketHandler(Opcode.MSG_MOVE_HOVER)]
	[PacketHandler(Opcode.MSG_MOVE_FEATHER_FALL)]
	[PacketHandler(Opcode.MSG_MOVE_WATER_WALK)]
	private void HandleMovementMessages(WorldPacket packet)
	{
		MoveUpdate moveUpdate = new MoveUpdate();
		moveUpdate.MoverGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
		moveUpdate.MoveInfo = new MovementInfo();
		moveUpdate.MoveInfo.ReadMovementInfoLegacy(packet, GetSession().GameState);
		moveUpdate.MoveInfo.Flags = (uint)((MovementFlagWotLK)moveUpdate.MoveInfo.Flags).CastFlags<MovementFlagModern>();
		moveUpdate.MoveInfo.ValidateMovementInfo();
		SendPacketToClient(moveUpdate);
	}

	[PacketHandler(Opcode.MSG_MOVE_KNOCK_BACK)]
	private void HandleMoveKnockBack(WorldPacket packet)
	{
		MoveUpdateKnockBack moveUpdateKnockBack = new MoveUpdateKnockBack();
		moveUpdateKnockBack.MoverGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
		moveUpdateKnockBack.MoveInfo = new MovementInfo();
		moveUpdateKnockBack.MoveInfo.ReadMovementInfoLegacy(packet, GetSession().GameState);
		moveUpdateKnockBack.MoveInfo.Flags = (uint)((MovementFlagWotLK)moveUpdateKnockBack.MoveInfo.Flags).CastFlags<MovementFlagModern>();
		moveUpdateKnockBack.MoveInfo.JumpSinAngle = packet.ReadFloat();
		moveUpdateKnockBack.MoveInfo.JumpCosAngle = packet.ReadFloat();
		moveUpdateKnockBack.MoveInfo.JumpHorizontalSpeed = packet.ReadFloat();
		moveUpdateKnockBack.MoveInfo.JumpVerticalSpeed = packet.ReadFloat();
		moveUpdateKnockBack.MoveInfo.ValidateMovementInfo();
		SendPacketToClient(moveUpdateKnockBack);
	}

	[PacketHandler(Opcode.SMSG_MOVE_KNOCK_BACK)]
	private void HandleMoveForceKnockBack(WorldPacket packet)
	{
		MoveKnockBack moveKnockBack = new MoveKnockBack();
		moveKnockBack.MoverGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
		moveKnockBack.MoveCounter = packet.ReadUInt32();
		moveKnockBack.Direction = packet.ReadVector2();
		moveKnockBack.HorizontalSpeed = packet.ReadFloat();
		moveKnockBack.VerticalSpeed = packet.ReadFloat();
		SendPacketToClient(moveKnockBack);
	}

	[PacketHandler(Opcode.SMSG_CONTROL_UPDATE)]
	private void HandleControlUpdate(WorldPacket packet)
	{
		ControlUpdate controlUpdate = new ControlUpdate();
		controlUpdate.Guid = packet.ReadPackedGuid().To128(GetSession().GameState);
		controlUpdate.HasControl = packet.ReadBool();
		SendPacketToClient(controlUpdate);
	}

    // 处理移动传送确认
    [PacketHandler(Opcode.MSG_MOVE_TELEPORT_ACK)]
	private void HandleMoveTeleportAck(WorldPacket packet)
	{
		WowGuid128 wowGuid = packet.ReadPackedGuid().To128(GetSession().GameState);
		if (GetSession().GameState.IsInTaxiFlight && GetSession().GameState.CurrentPlayerGuid == wowGuid)
		{
			ControlUpdate controlUpdate = new ControlUpdate();
			controlUpdate.Guid = wowGuid;
			controlUpdate.HasControl = true;
			SendPacketToClient(controlUpdate);
			GetSession().GameState.IsInTaxiFlight = false;
		}
		MoveTeleport moveTeleport = new MoveTeleport();
		moveTeleport.MoverGUID = wowGuid;
		moveTeleport.MoveCounter = packet.ReadUInt32();
		MovementInfo movementInfo = new MovementInfo();
		movementInfo.ReadMovementInfoLegacy(packet, GetSession().GameState);
		movementInfo.Flags = (uint)((MovementFlagWotLK)movementInfo.Flags).CastFlags<MovementFlagModern>();
		movementInfo.ValidateMovementInfo();
		moveTeleport.Position = movementInfo.Position;
		moveTeleport.Orientation = movementInfo.Orientation;
		moveTeleport.TransportGUID = movementInfo.TransportGuid;
		if (movementInfo.TransportSeat > 0)
		{
			moveTeleport.Vehicle = new VehicleTeleport();
			moveTeleport.Vehicle.VehicleSeatIndex = movementInfo.TransportSeat;
		}
		SendPacketToClient(moveTeleport);
	}

	[PacketHandler(Opcode.SMSG_TRANSFER_PENDING)]
	private void HandleTransferPending(WorldPacket packet)
	{
		if (GetSession().GameState.IsWaitingForWorldPortAck)
		{
			Log.Print(LogType.Error, "Skipping SMSG_TRANSFER_PENDING, client is already being teleported.", "HandleTransferPending", "D:\\a\\HermesProxy\\HermesProxy\\World\\Client\\PacketHandlers\\MovementHandler.cs");
			return;
		}
		TransferPending transferPending = new TransferPending();
		transferPending.MapID = (GetSession().GameState.PendingTransferMapId = packet.ReadUInt32());
		transferPending.OldMapPosition = Vector3.Zero;
		SendPacketToClient(transferPending);
		GetSession().GameState.IsFirstEnterWorld = false;
		GetSession().GameState.IsWaitingForNewWorld = true;
		SuspendToken suspendToken = new SuspendToken();
		suspendToken.SequenceIndex = 3u;
		suspendToken.Reason = 1u;
		SendPacketToClient(suspendToken);
	}

	[PacketHandler(Opcode.SMSG_TRANSFER_ABORTED)]
	private void HandleTransferAborted(WorldPacket packet)
	{
		TransferAborted transferAborted = new TransferAborted();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			transferAborted.MapID = packet.ReadUInt32();
		}
		else
		{
			transferAborted.MapID = GetSession().GameState.PendingTransferMapId;
		}
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			transferAborted.Reason = (TransferAbortReasonModern)packet.ReadUInt8();
		}
		else
		{
			TransferAbortReasonLegacy transferAbortReasonLegacy = (TransferAbortReasonLegacy)packet.ReadUInt8();
			transferAborted.Reason = (TransferAbortReasonModern)Enum.Parse(typeof(TransferAbortReasonModern), transferAbortReasonLegacy.ToString());
		}
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			transferAborted.Arg = packet.ReadUInt8();
		}
		SendPacketToClient(transferAborted);
		GetSession().GameState.IsWaitingForNewWorld = false;
	}

    // 处理新世界
    [PacketHandler(Opcode.SMSG_NEW_WORLD)]
	private void HandleNewWorld(WorldPacket packet)
	{
		NewWorld newWorld = new NewWorld();
		GetSession().GameState.CurrentMapId = (newWorld.MapID = packet.ReadUInt32());
		newWorld.Position = packet.ReadVector3();
		newWorld.Orientation = packet.ReadFloat();
		newWorld.Reason = 4u;
		GetSession().GameState.IsFirstEnterWorld = false;
		if (!GetSession().GameState.IsWaitingForNewWorld)
		{
			return;
		}
		GetSession().GameState.IsWaitingForNewWorld = false;
		GetSession().GameState.IsWaitingForWorldPortAck = true;
		SendPacketToClient(newWorld);
		if (newWorld.MapID > 1)
		{
			UpdateLastInstance updateLastInstance = new UpdateLastInstance();
			updateLastInstance.MapID = newWorld.MapID;
			SendPacketToClient(updateLastInstance);
			if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
			{
				SendPacketToClient(new TimeSyncRequest());
			}
			ResumeToken resumeToken = new ResumeToken();
			resumeToken.SequenceIndex = 3u;
			resumeToken.Reason = 1u;
			SendPacketToClient(resumeToken);
		}
		WorldServerInfo worldServerInfo = new WorldServerInfo();
		if (newWorld.MapID > 1)
		{
			worldServerInfo.DifficultyID = 1u;
			worldServerInfo.InstanceGroupSize = 5u;
		}
		SendPacketToClient(worldServerInfo);
	}

	[PacketHandler(Opcode.SMSG_MOVE_SPLINE_SET_FLIGHT_BACK_SPEED)]
	[PacketHandler(Opcode.SMSG_MOVE_SPLINE_SET_FLIGHT_SPEED)]
	[PacketHandler(Opcode.SMSG_MOVE_SPLINE_SET_PITCH_RATE)]
	[PacketHandler(Opcode.SMSG_MOVE_SPLINE_SET_RUN_BACK_SPEED)]
	[PacketHandler(Opcode.SMSG_MOVE_SPLINE_SET_RUN_SPEED)]
	[PacketHandler(Opcode.SMSG_MOVE_SPLINE_SET_SWIM_BACK_SPEED)]
	[PacketHandler(Opcode.SMSG_MOVE_SPLINE_SET_SWIM_SPEED)]
	[PacketHandler(Opcode.SMSG_MOVE_SPLINE_SET_TURN_RATE)]
	[PacketHandler(Opcode.SMSG_MOVE_SPLINE_SET_WALK_BACK_SPEED)]
	[PacketHandler(Opcode.SMSG_MOVE_SPLINE_SET_WALK_SPEED)]
	private void HandleMoveSplineSetSpeed(WorldPacket packet)
	{
		MoveSplineSetSpeed moveSplineSetSpeed = new MoveSplineSetSpeed(packet.GetUniversalOpcode(isModern: false));
		moveSplineSetSpeed.MoverGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
		moveSplineSetSpeed.Speed = packet.ReadFloat();
		SendPacketToClient(moveSplineSetSpeed);
	}

	[PacketHandler(Opcode.SMSG_FORCE_WALK_SPEED_CHANGE)]
	[PacketHandler(Opcode.SMSG_FORCE_RUN_SPEED_CHANGE)]
	[PacketHandler(Opcode.SMSG_FORCE_RUN_BACK_SPEED_CHANGE)]
	[PacketHandler(Opcode.SMSG_FORCE_SWIM_SPEED_CHANGE)]
	[PacketHandler(Opcode.SMSG_FORCE_SWIM_BACK_SPEED_CHANGE)]
	[PacketHandler(Opcode.SMSG_FORCE_TURN_RATE_CHANGE)]
	[PacketHandler(Opcode.SMSG_FORCE_FLIGHT_SPEED_CHANGE)]
	[PacketHandler(Opcode.SMSG_FORCE_FLIGHT_BACK_SPEED_CHANGE)]
	[PacketHandler(Opcode.SMSG_FORCE_PITCH_RATE_CHANGE)]
	private void HandleMoveForceSpeedChange(WorldPacket packet)
	{
		Opcode universalOpcode = Opcodes.GetUniversalOpcode(packet.GetUniversalOpcode(isModern: false).ToString().Replace("SMSG_FORCE_", "SMSG_MOVE_SET_")
			.Replace("_CHANGE", ""));
		MoveSetSpeed moveSetSpeed = new MoveSetSpeed(universalOpcode);
		moveSetSpeed.MoverGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
		moveSetSpeed.MoveCounter = packet.ReadUInt32();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180) && packet.GetUniversalOpcode(isModern: false) == Opcode.SMSG_FORCE_RUN_SPEED_CHANGE)
		{
			packet.ReadUInt8();
		}
		moveSetSpeed.Speed = packet.ReadFloat();
		SendPacketToClient(moveSetSpeed);
		if ((universalOpcode == Opcode.SMSG_MOVE_SET_SWIM_SPEED || universalOpcode == Opcode.SMSG_MOVE_SET_SWIM_BACK_SPEED) && LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			MoveSetSpeed moveSetSpeed2 = new MoveSetSpeed((Opcode)Enum.Parse(typeof(Opcode), universalOpcode.ToString().Replace("SWIM", "FLIGHT")));
			moveSetSpeed2.MoverGUID = moveSetSpeed.MoverGUID;
			moveSetSpeed2.MoveCounter = moveSetSpeed.MoveCounter;
			moveSetSpeed2.Speed = moveSetSpeed.Speed;
			SendPacketToClient(moveSetSpeed2);
		}
	}

	[PacketHandler(Opcode.MSG_MOVE_SET_FLIGHT_BACK_SPEED)]
	[PacketHandler(Opcode.MSG_MOVE_SET_FLIGHT_SPEED)]
	[PacketHandler(Opcode.MSG_MOVE_SET_PITCH_RATE)]
	[PacketHandler(Opcode.MSG_MOVE_SET_RUN_BACK_SPEED)]
	[PacketHandler(Opcode.MSG_MOVE_SET_RUN_SPEED)]
	[PacketHandler(Opcode.MSG_MOVE_SET_SWIM_BACK_SPEED)]
	[PacketHandler(Opcode.MSG_MOVE_SET_SWIM_SPEED)]
	[PacketHandler(Opcode.MSG_MOVE_SET_TURN_RATE)]
	[PacketHandler(Opcode.MSG_MOVE_SET_WALK_SPEED)]
	private void HandleMoveUpdateSpeed(WorldPacket packet)
	{
		Opcode universalOpcode = Opcodes.GetUniversalOpcode(packet.GetUniversalOpcode(isModern: false).ToString().Replace("MSG_MOVE_SET", "SMSG_MOVE_UPDATE"));
		MoveUpdateSpeed moveUpdateSpeed = new MoveUpdateSpeed(universalOpcode);
		moveUpdateSpeed.MoverGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
		moveUpdateSpeed.MoveInfo = new MovementInfo();
		moveUpdateSpeed.MoveInfo.ReadMovementInfoLegacy(packet, GetSession().GameState);
		MovementFlagModern flags = ((MovementFlagWotLK)moveUpdateSpeed.MoveInfo.Flags).CastFlags<MovementFlagModern>();
		moveUpdateSpeed.MoveInfo.Flags = (uint)flags;
		moveUpdateSpeed.MoveInfo.ValidateMovementInfo();
		moveUpdateSpeed.Speed = packet.ReadFloat();
		SendPacketToClient(moveUpdateSpeed);
		if ((universalOpcode == Opcode.SMSG_MOVE_UPDATE_SWIM_SPEED || universalOpcode == Opcode.SMSG_MOVE_UPDATE_SWIM_BACK_SPEED) && LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			MoveUpdateSpeed moveUpdateSpeed2 = new MoveUpdateSpeed((Opcode)Enum.Parse(typeof(Opcode), universalOpcode.ToString().Replace("SWIM", "FLIGHT")));
			moveUpdateSpeed2.MoverGUID = moveUpdateSpeed.MoverGUID;
			moveUpdateSpeed2.MoveInfo = moveUpdateSpeed.MoveInfo;
			moveUpdateSpeed2.Speed = moveUpdateSpeed.Speed;
			SendPacketToClient(moveUpdateSpeed2);
		}
	}

	[PacketHandler(Opcode.SMSG_MOVE_SPLINE_ROOT)]
	[PacketHandler(Opcode.SMSG_MOVE_SPLINE_UNROOT)]
	[PacketHandler(Opcode.SMSG_MOVE_SPLINE_ENABLE_GRAVITY)]
	[PacketHandler(Opcode.SMSG_MOVE_SPLINE_DISABLE_GRAVITY)]
	[PacketHandler(Opcode.SMSG_MOVE_SPLINE_SET_FEATHER_FALL)]
	[PacketHandler(Opcode.SMSG_MOVE_SPLINE_SET_NORMAL_FALL)]
	[PacketHandler(Opcode.SMSG_MOVE_SPLINE_SET_HOVER)]
	[PacketHandler(Opcode.SMSG_MOVE_SPLINE_UNSET_HOVER)]
	[PacketHandler(Opcode.SMSG_MOVE_SPLINE_SET_WATER_WALK)]
	[PacketHandler(Opcode.SMSG_MOVE_SPLINE_SET_LAND_WALK)]
	[PacketHandler(Opcode.SMSG_MOVE_SPLINE_START_SWIM)]
	[PacketHandler(Opcode.SMSG_MOVE_SPLINE_STOP_SWIM)]
	[PacketHandler(Opcode.SMSG_MOVE_SPLINE_SET_RUN_MODE)]
	[PacketHandler(Opcode.SMSG_MOVE_SPLINE_SET_WALK_MODE)]
	[PacketHandler(Opcode.SMSG_MOVE_SPLINE_SET_FLYING)]
	[PacketHandler(Opcode.SMSG_MOVE_SPLINE_UNSET_FLYING)]
	private void HandleSplineMovementMessages(WorldPacket packet)
	{
		MoveSplineSetFlag moveSplineSetFlag = new MoveSplineSetFlag(packet.GetUniversalOpcode(isModern: false));
		moveSplineSetFlag.MoverGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
		SendPacketToClient(moveSplineSetFlag);
	}

	[PacketHandler(Opcode.SMSG_MOVE_ROOT)]
	[PacketHandler(Opcode.SMSG_MOVE_UNROOT)]
	[PacketHandler(Opcode.SMSG_MOVE_SET_WATER_WALK)]
	[PacketHandler(Opcode.SMSG_MOVE_SET_LAND_WALK)]
	[PacketHandler(Opcode.SMSG_MOVE_SET_HOVERING)]
	[PacketHandler(Opcode.SMSG_MOVE_UNSET_HOVERING)]
	[PacketHandler(Opcode.SMSG_MOVE_SET_CAN_FLY)]
	[PacketHandler(Opcode.SMSG_MOVE_UNSET_CAN_FLY)]
	[PacketHandler(Opcode.SMSG_MOVE_ENABLE_TRANSITION_BETWEEN_SWIM_AND_FLY)]
	[PacketHandler(Opcode.SMSG_MOVE_DISABLE_TRANSITION_BETWEEN_SWIM_AND_FLY)]
	[PacketHandler(Opcode.SMSG_MOVE_DISABLE_GRAVITY)]
	[PacketHandler(Opcode.SMSG_MOVE_ENABLE_GRAVITY)]
	[PacketHandler(Opcode.SMSG_MOVE_SET_FEATHER_FALL)]
	[PacketHandler(Opcode.SMSG_MOVE_SET_NORMAL_FALL)]
	private void HandleMoveForceFlagChange(WorldPacket packet)
	{
		MoveSetFlag moveSetFlag = new MoveSetFlag(packet.GetUniversalOpcode(isModern: false));
		moveSetFlag.MoverGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
		moveSetFlag.MoveCounter = packet.ReadUInt32();
		SendPacketToClient(moveSetFlag);
	}

	[PacketHandler(Opcode.SMSG_COMPRESSED_MOVES)]
	private void HandleCompressedMoves(WorldPacket packet)
	{
		int inflatedSize = packet.ReadInt32();
		WorldPacket worldPacket = packet.Inflate(inflatedSize);
		while (worldPacket.CanRead())
		{
			byte b = worldPacket.ReadUInt8();
			ushort opcode = worldPacket.ReadUInt16();
			byte[] data = worldPacket.ReadBytes((uint)(b - 2));
			WorldPacket worldPacket2 = new WorldPacket(opcode, data);
			worldPacket2.SetReceiveTime(worldPacket.GetReceivedTime());
			HandlePacket(worldPacket2);
		}
	}

	[PacketHandler(Opcode.SMSG_ON_MONSTER_MOVE)]
	[PacketHandler(Opcode.SMSG_MONSTER_MOVE_TRANSPORT)]
	private void HandleMonsterMove(WorldPacket packet)
	{
		WowGuid128 wowGuid = packet.ReadPackedGuid().To128(GetSession().GameState);
		ServerSideMovement serverSideMovement = new ServerSideMovement();
		if (packet.GetUniversalOpcode(isModern: false) == Opcode.SMSG_MONSTER_MOVE_TRANSPORT)
		{
			serverSideMovement.TransportGuid = packet.ReadPackedGuid().To128(GetSession().GameState);
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
			{
				serverSideMovement.TransportSeat = packet.ReadInt8();
			}
		}
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
		{
			packet.ReadBool();
		}
		serverSideMovement.StartPosition = packet.ReadVector3();
		serverSideMovement.SplineId = packet.ReadUInt32();
		SplineTypeLegacy splineTypeLegacy = (SplineTypeLegacy)packet.ReadUInt8();
		switch (splineTypeLegacy)
		{
		case SplineTypeLegacy.FacingSpot:
			serverSideMovement.SplineType = SplineTypeModern.FacingSpot;
			serverSideMovement.FinalFacingSpot = packet.ReadVector3();
			break;
		case SplineTypeLegacy.FacingTarget:
			serverSideMovement.SplineType = SplineTypeModern.FacingTarget;
			serverSideMovement.FinalFacingGuid = packet.ReadGuid().To128(GetSession().GameState);
			break;
		case SplineTypeLegacy.FacingAngle:
			serverSideMovement.SplineType = SplineTypeModern.FacingAngle;
			serverSideMovement.FinalOrientation = packet.ReadFloat();
			MovementInfo.ClampOrientation(ref serverSideMovement.FinalOrientation);
			break;
		case SplineTypeLegacy.Stop:
		{
			serverSideMovement.SplineType = SplineTypeModern.None;
			MonsterMove packet2 = new MonsterMove(wowGuid, serverSideMovement);
			SendPacketToClient(packet2);
			return;
		}
		}
		bool flag;
		bool flag2;
		bool flag3;
		bool flag4;
		if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			SplineFlagVanilla splineFlagVanilla = (SplineFlagVanilla)packet.ReadUInt32();
			flag = false;
			flag2 = false;
			flag3 = splineFlagVanilla.HasAnyFlag(SplineFlagVanilla.Flying);
			flag4 = splineFlagVanilla == (SplineFlagVanilla.Runmode | SplineFlagVanilla.Flying);
			if (splineFlagVanilla == SplineFlagVanilla.Runmode)
			{
				serverSideMovement.SplineFlags = SplineFlagModern.Unknown5;
				UnitFlagsVanilla legacyFieldValueUInt = (UnitFlagsVanilla)GetSession().GameState.GetLegacyFieldValueUInt32(wowGuid, UnitField.UNIT_FIELD_FLAGS);
				if (legacyFieldValueUInt.HasFlag(UnitFlagsVanilla.CanSwim))
				{
					serverSideMovement.SplineFlags |= SplineFlagModern.CanSwim;
				}
				if (splineTypeLegacy == SplineTypeLegacy.Normal && !legacyFieldValueUInt.HasFlag(UnitFlagsVanilla.InCombat))
				{
					serverSideMovement.SplineFlags |= SplineFlagModern.Steering | SplineFlagModern.Unknown10;
				}
			}
			else
			{
				serverSideMovement.SplineFlags = splineFlagVanilla.CastFlags<SplineFlagModern>();
			}
		}
		else if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			SplineFlagTBC splineFlagTBC = (SplineFlagTBC)packet.ReadUInt32();
			flag = false;
			flag2 = false;
			flag3 = splineFlagTBC.HasAnyFlag(SplineFlagTBC.Flying);
			flag4 = splineFlagTBC == (SplineFlagTBC.Runmode | SplineFlagTBC.Flying);
			if (splineFlagTBC == SplineFlagTBC.Runmode)
			{
				serverSideMovement.SplineFlags = SplineFlagModern.Unknown5;
				UnitFlags legacyFieldValueUInt2 = (UnitFlags)GetSession().GameState.GetLegacyFieldValueUInt32(wowGuid, UnitField.UNIT_FIELD_FLAGS);
				if (legacyFieldValueUInt2.HasFlag(UnitFlags.CanSwim))
				{
					serverSideMovement.SplineFlags |= SplineFlagModern.CanSwim;
				}
				if (splineTypeLegacy == SplineTypeLegacy.Normal && !legacyFieldValueUInt2.HasFlag(UnitFlags.InCombat))
				{
					serverSideMovement.SplineFlags |= SplineFlagModern.Steering | SplineFlagModern.Unknown10;
				}
			}
			else
			{
				serverSideMovement.SplineFlags = splineFlagTBC.CastFlags<SplineFlagModern>();
			}
		}
		else
		{
			SplineFlagWotLK splineFlagWotLK = (SplineFlagWotLK)packet.ReadUInt32();
			flag = splineFlagWotLK.HasAnyFlag(SplineFlagWotLK.AnimationTier);
			flag2 = splineFlagWotLK.HasAnyFlag(SplineFlagWotLK.Trajectory);
			flag3 = splineFlagWotLK.HasAnyFlag(SplineFlagWotLK.Flying | SplineFlagWotLK.CatmullRom);
			flag4 = splineFlagWotLK == (SplineFlagWotLK.WalkMode | SplineFlagWotLK.Flying);
			serverSideMovement.SplineFlags = splineFlagWotLK.CastFlags<SplineFlagModern>();
		}
		if (flag)
		{
			packet.ReadUInt8();
			packet.ReadInt32();
		}
		serverSideMovement.SplineTimeFull = packet.ReadUInt32();
		if (flag2)
		{
			packet.ReadFloat();
			packet.ReadInt32();
		}
		serverSideMovement.SplineCount = packet.ReadUInt32();
		if (flag3)
		{
			for (int i = 0; i < serverSideMovement.SplineCount; i++)
			{
				Vector3 vector = packet.ReadVector3();
				serverSideMovement?.SplinePoints.Add(vector);
			}
			serverSideMovement.SplineFlags |= SplineFlagModern.UncompressedPath;
		}
		else
		{
			serverSideMovement.EndPosition = packet.ReadVector3();
			Vector3 vector2 = (serverSideMovement.StartPosition + serverSideMovement.EndPosition) * 0.5f;
			for (int j = 1; j < serverSideMovement.SplineCount; j++)
			{
				Vector3 vector3 = packet.ReadPackedVector3();
				vector3 = ((!LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180)) ? (serverSideMovement.EndPosition - vector3) : (vector2 - vector3));
				serverSideMovement.SplinePoints.Add(vector3);
			}
		}
		int num;
		if (flag4 && (GetSession().GameState.IsWaitingForTaxiStart || Math.Abs(packet.GetReceivedTime() - GetSession().GameState.CurrentPlayerCreateTime) <= 1000))
		{
			num = ((GetSession().GameState.CurrentPlayerGuid == wowGuid) ? 1 : 0);
			if (num != 0)
			{
				ServerSideMovement serverSideMovement2 = new ServerSideMovement
				{
					StartPosition = serverSideMovement.StartPosition,
					SplineId = serverSideMovement.SplineId - 2
				};
				MonsterMove packet3 = new MonsterMove(wowGuid, serverSideMovement2);
				SendPacketToClient(packet3);
				SendPacketToClient(new ControlUpdate
				{
					Guid = wowGuid,
					HasControl = false
				});
				serverSideMovement2.SplineId = serverSideMovement.SplineId - 1;
				packet3 = new MonsterMove(wowGuid, serverSideMovement2);
				SendPacketToClient(packet3);
				SendPacketToClient(new ControlUpdate
				{
					Guid = wowGuid,
					HasControl = false
				});
				serverSideMovement.SplineFlags = SplineFlagModern.Flying | SplineFlagModern.CatmullRom | SplineFlagModern.CanSwim | SplineFlagModern.UncompressedPath | SplineFlagModern.Unknown5 | SplineFlagModern.Steering | SplineFlagModern.Unknown10;
				if (!flag3 && serverSideMovement.EndPosition != Vector3.Zero)
				{
					serverSideMovement.SplinePoints.Add(serverSideMovement.EndPosition);
				}
			}
		}
		else
		{
			num = 0;
		}
		MonsterMove packet4 = new MonsterMove(wowGuid, serverSideMovement);
		SendPacketToClient(packet4);
		if (num != 0)
		{
			if (GetSession().GameState.IsWaitingForTaxiStart)
			{
				ActivateTaxiReplyPkt activateTaxiReplyPkt = new ActivateTaxiReplyPkt();
				activateTaxiReplyPkt.Reply = ActivateTaxiReply.Ok;
				SendPacketToClient(activateTaxiReplyPkt);
				GetSession().GameState.IsWaitingForTaxiStart = false;
			}
			GetSession().GameState.IsInTaxiFlight = true;
		}
	}

	[PacketHandler(Opcode.SMSG_GOSSIP_MESSAGE)]
	private void HandleGossipmessage(WorldPacket packet)
	{
		GossipMessagePkt gossipMessagePkt = new GossipMessagePkt();
		gossipMessagePkt.GossipGUID = packet.ReadGuid().To128(GetSession().GameState);
		GetSession().GameState.CurrentInteractedWithNPC = gossipMessagePkt.GossipGUID;
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_4_0_8089))
		{
			gossipMessagePkt.GossipID = packet.ReadInt32();
		}
		else
		{
			gossipMessagePkt.GossipID = (int)gossipMessagePkt.GossipGUID.GetEntry();
		}
		gossipMessagePkt.TextID = packet.ReadInt32();
		uint num = packet.ReadUInt32();
		for (uint num2 = 0u; num2 < num; num2++)
		{
			ClientGossipOption clientGossipOption = new ClientGossipOption();
			clientGossipOption.OptionIndex = packet.ReadInt32();
			clientGossipOption.OptionIcon = packet.ReadUInt8();
			clientGossipOption.OptionFlags = (byte)(packet.ReadBool() ? 1u : 0u);
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
			{
				clientGossipOption.OptionCost = packet.ReadInt32();
			}
			clientGossipOption.Text = packet.ReadCString();
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
			{
				clientGossipOption.Confirm = packet.ReadCString();
			}
			gossipMessagePkt.GossipOptions.Add(clientGossipOption);
		}
		uint num3 = packet.ReadUInt32();
		for (uint num4 = 0u; num4 < num3; num4++)
		{
			ClientGossipQuest clientGossipQuest = ReadGossipQuestOption(packet);
			gossipMessagePkt.GossipQuests.Add(clientGossipQuest);
		}
		SendPacketToClient(gossipMessagePkt);
	}

	[PacketHandler(Opcode.SMSG_GOSSIP_COMPLETE)]
	private void HandleGossipComplete(WorldPacket packet)
	{
		GossipComplete packet2 = new GossipComplete();
		SendPacketToClient(packet2);
	}

	[PacketHandler(Opcode.SMSG_GOSSIP_POI)]
	private void HandleGossipPoi(WorldPacket packet)
	{
		GossipPOI gossipPOI = new GossipPOI();
		gossipPOI.Flags = packet.ReadUInt32();
		gossipPOI.Pos = new Vector3(packet.ReadVector2());
		gossipPOI.Icon = packet.ReadUInt32();
		gossipPOI.Importance = packet.ReadUInt32();
		gossipPOI.Name = packet.ReadCString();
		SendPacketToClient(gossipPOI);
	}

	[PacketHandler(Opcode.SMSG_BINDER_CONFIRM)]
	private void HandleBinderConfirm(WorldPacket packet)
	{
		BinderConfirm binderConfirm = new BinderConfirm();
		binderConfirm.Guid = packet.ReadGuid().To128(GetSession().GameState);
		GetSession().GameState.CurrentInteractedWithNPC = binderConfirm.Guid;
		SendPacketToClient(binderConfirm);
	}

	[PacketHandler(Opcode.SMSG_VENDOR_INVENTORY)]
	private void HandleVendorInventory(WorldPacket packet)
	{
		VendorInventory vendorInventory = new VendorInventory();
		vendorInventory.VendorGUID = packet.ReadGuid().To128(GetSession().GameState);
		GetSession().GameState.CurrentInteractedWithNPC = vendorInventory.VendorGUID;
		byte b = packet.ReadUInt8();
		if (b == 0)
		{
			vendorInventory.Reason = packet.ReadUInt8();
			SendPacketToClient(vendorInventory);
			return;
		}
		for (byte b2 = 0; b2 < b; b2++)
		{
			VendorItem vendorItem = new VendorItem();
			vendorItem.Slot = packet.ReadInt32();
			vendorItem.Item.ItemID = packet.ReadUInt32();
			packet.ReadUInt32();
			vendorItem.Quantity = packet.ReadInt32();
			vendorItem.Price = packet.ReadUInt32();
			vendorItem.Durability = packet.ReadInt32();
			vendorItem.StackCount = packet.ReadUInt32();
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
			{
				vendorItem.ExtendedCostID = packet.ReadInt32();
			}
			GetSession().GameState.SetItemBuyCount(vendorItem.Item.ItemID, vendorItem.StackCount);
			vendorInventory.Items.Add(vendorItem);
		}
		SendPacketToClient(vendorInventory);
	}

	[PacketHandler(Opcode.SMSG_SHOW_BANK)]
	private void HandleShowBank(WorldPacket packet)
	{
		ShowBank showBank = new ShowBank();
		showBank.Guid = packet.ReadGuid().To128(GetSession().GameState);
		GetSession().GameState.CurrentInteractedWithNPC = showBank.Guid;
		SendPacketToClient(showBank);
	}

	[PacketHandler(Opcode.SMSG_TRAINER_LIST)]
	private void HandleTrainerList(WorldPacket packet)
	{
		TrainerList trainerList = new TrainerList();
		trainerList.TrainerGUID = packet.ReadGuid().To128(GetSession().GameState);
		GetSession().GameState.CurrentInteractedWithNPC = trainerList.TrainerGUID;
		trainerList.TrainerID = trainerList.TrainerGUID.GetEntry();
		trainerList.TrainerType = packet.ReadInt32();
		int num = packet.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			TrainerListSpell trainerListSpell = new TrainerListSpell();
			uint num2 = packet.ReadUInt32();
			if (ModernVersion.ExpansionVersion > 1 && LegacyVersion.ExpansionVersion <= 1)
			{
				uint realSpell = GameData.GetRealSpell(num2);
				if (realSpell != num2)
				{
					GetSession().GameState.StoreRealSpell(realSpell, num2);
					num2 = realSpell;
				}
			}
			trainerListSpell.SpellID = num2;
			TrainerSpellStateLegacy trainerSpellStateLegacy = (TrainerSpellStateLegacy)packet.ReadUInt8();
			TrainerSpellStateModern usable = (TrainerSpellStateModern)Enum.Parse(typeof(TrainerSpellStateModern), trainerSpellStateLegacy.ToString());
			trainerListSpell.Usable = usable;
			trainerListSpell.MoneyCost = packet.ReadUInt32();
			packet.ReadInt32();
			packet.ReadInt32();
			trainerListSpell.ReqLevel = packet.ReadUInt8();
			trainerListSpell.ReqSkillLine = packet.ReadUInt32();
			trainerListSpell.ReqSkillRank = packet.ReadUInt32();
			trainerListSpell.ReqAbility[0] = packet.ReadUInt32();
			trainerListSpell.ReqAbility[1] = packet.ReadUInt32();
			trainerListSpell.ReqAbility[2] = packet.ReadUInt32();
			trainerList.Spells.Add(trainerListSpell);
		}
		trainerList.Greeting = packet.ReadCString();
		SendPacketToClient(trainerList);
	}

	[PacketHandler(Opcode.SMSG_TRAINER_BUY_FAILED)]
	private void HandleTrainerBuyFailed(WorldPacket packet)
	{
		TrainerBuyFailed trainerBuyFailed = new TrainerBuyFailed();
		trainerBuyFailed.TrainerGUID = packet.ReadGuid().To128(GetSession().GameState);
		trainerBuyFailed.SpellID = packet.ReadUInt32();
		trainerBuyFailed.TrainerFailedReason = packet.ReadUInt32();
		SendPacketToClient(trainerBuyFailed);
		ChatPkt packet2 = new ChatPkt(GetSession(), ChatMessageTypeModern.System, $"Failed to learn Spell {trainerBuyFailed.SpellID} (Reason {trainerBuyFailed.TrainerFailedReason}).");
		SendPacketToClient(packet2);
	}

	[PacketHandler(Opcode.MSG_TALENT_WIPE_CONFIRM)]
	private void HandleTalentWipeConfirm(WorldPacket packet)
	{
		RespecWipeConfirm respecWipeConfirm = new RespecWipeConfirm();
		respecWipeConfirm.TrainerGUID = packet.ReadGuid().To128(GetSession().GameState);
		respecWipeConfirm.Cost = packet.ReadUInt32();
		SendPacketToClient(respecWipeConfirm);
	}

	[PacketHandler(Opcode.SMSG_SPIRIT_HEALER_CONFIRM)]
	private void HandleSpiritHealerConfirm(WorldPacket packet)
	{
		SpiritHealerConfirm spiritHealerConfirm = new SpiritHealerConfirm();
		spiritHealerConfirm.Guid = packet.ReadGuid().To128(GetSession().GameState);
		SendPacketToClient(spiritHealerConfirm);
	}

	[PacketHandler(Opcode.SMSG_PET_SPELLS_MESSAGE)]
	private void HandlePetSpellsMessage(WorldPacket packet)
	{
		WowGuid wowGuid = packet.ReadGuid();
		GetSession().GameState.CurrentPetGuid = wowGuid.To128(GetSession().GameState);
		GetSession().GameState.CurrentClientPetCast = null;
		if (wowGuid.IsEmpty())
		{
			PetClearSpells packet2 = new PetClearSpells();
			SendPacketToClient(packet2);
			return;
		}
		PetSpells petSpells = new PetSpells();
		petSpells.PetGUID = wowGuid.To128(GetSession().GameState);
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
		{
			petSpells.CreatureFamily = packet.ReadUInt16();
		}
		petSpells.TimeLimit = packet.ReadUInt32();
		petSpells.ReactState = (ReactStates)packet.ReadUInt8();
		petSpells.CommandState = (CommandStates)packet.ReadUInt8();
		packet.ReadUInt8();
		petSpells.Flag = packet.ReadUInt8();
		for (int i = 0; i < 10; i++)
		{
			petSpells.ActionButtons[i] = packet.ReadUInt32();
		}
		byte b = packet.ReadUInt8();
		for (int j = 0; j < b; j++)
		{
			petSpells.Actions.Add(packet.ReadUInt32());
		}
		byte b2 = packet.ReadUInt8();
		for (int k = 0; k < b2; k++)
		{
			PetSpellCooldown petSpellCooldown = new PetSpellCooldown();
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
			{
				petSpellCooldown.SpellID = packet.ReadUInt32();
			}
			else
			{
				petSpellCooldown.SpellID = packet.ReadUInt16();
			}
			petSpellCooldown.Category = packet.ReadUInt16();
			petSpellCooldown.Duration = packet.ReadUInt32();
			petSpellCooldown.CategoryDuration = packet.ReadUInt32();
			petSpells.Cooldowns.Add(petSpellCooldown);
		}
		SendPacketToClient(petSpells);
	}

	[PacketHandler(Opcode.SMSG_PET_ACTION_SOUND)]
	private void HandlePetActionSound(WorldPacket packet)
	{
		PetActionSound petActionSound = new PetActionSound();
		petActionSound.UnitGUID = packet.ReadGuid().To128(GetSession().GameState);
		petActionSound.Action = packet.ReadUInt32();
		SendPacketToClient(petActionSound);
	}

	[PacketHandler(Opcode.SMSG_PET_BROKEN)]
	private void HandlePetBroken(WorldPacket packet)
	{
		PrintNotification printNotification = new PrintNotification();
		printNotification.NotifyText = "Your pet has run away";
		SendPacketToClient(printNotification);
	}

	[PacketHandler(Opcode.SMSG_PET_UNLEARN_CONFIRM)]
	private void HandlePetUnlearnConfirm(WorldPacket packet)
	{
		RespecWipeConfirm respecWipeConfirm = new RespecWipeConfirm();
		respecWipeConfirm.TrainerGUID = packet.ReadGuid().To128(GetSession().GameState);
		respecWipeConfirm.Cost = packet.ReadUInt32();
		respecWipeConfirm.RespecType = SpecResetType.PetTalents;
		SendPacketToClient(respecWipeConfirm);
	}

	[PacketHandler(Opcode.MSG_LIST_STABLED_PETS)]
	private void HandleListStabledPets(WorldPacket packet)
	{
		PetGuids petGuids = new PetGuids();
		Dictionary<int, UpdateField> cachedObjectFieldsLegacy = GetSession().GameState.GetCachedObjectFieldsLegacy(GetSession().GameState.CurrentPlayerGuid);
		int updateField = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_SUMMON);
		if (updateField >= 0 && cachedObjectFieldsLegacy.ContainsKey(updateField))
		{
			WowGuid128 wowGuid = GetGuidValue(cachedObjectFieldsLegacy, UnitField.UNIT_FIELD_SUMMON).To128(GetSession().GameState);
			if (!wowGuid.IsEmpty())
			{
				petGuids.Guids.Add(wowGuid);
			}
		}
		SendPacketToClient(petGuids);
		PetStableList petStableList = new PetStableList();
		petStableList.StableMaster = packet.ReadGuid().To128(GetSession().GameState);
		byte b = packet.ReadUInt8();
		petStableList.NumStableSlots = packet.ReadUInt8();
		for (byte b2 = 0; b2 < b; b2++)
		{
			PetStableInfo petStableInfo = new PetStableInfo();
			petStableInfo.PetNumber = packet.ReadUInt32();
			petStableInfo.CreatureID = packet.ReadUInt32();
			petStableInfo.ExperienceLevel = packet.ReadUInt32();
			petStableInfo.PetName = packet.ReadCString();
			if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
			{
				petStableInfo.LoyaltyLevel = (byte)packet.ReadUInt32();
			}
			petStableInfo.PetFlags = packet.ReadUInt8();
			if (petStableInfo.PetFlags != 1)
			{
				petStableInfo.PetFlags = 3;
			}
			CreatureTemplate creatureTemplate = GameData.GetCreatureTemplate(petStableInfo.CreatureID);
			if (creatureTemplate != null)
			{
				petStableInfo.DisplayID = creatureTemplate.Display.CreatureDisplay[0].CreatureDisplayID;
			}
			else
			{
				WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_QUERY_CREATURE);
				worldPacket.WriteUInt32(petStableInfo.CreatureID);
				worldPacket.WriteGuid(WowGuid64.Empty);
				SendPacket(worldPacket);
			}
			petStableList.Pets.Add(petStableInfo);
		}
		SendPacketToClient(petStableList);
	}

	[PacketHandler(Opcode.SMSG_PET_STABLE_RESULT)]
	private void HandlePetStableResult(WorldPacket packet)
	{
		PetStableResult petStableResult = new PetStableResult();
		petStableResult.Result = packet.ReadUInt8();
		SendPacketToClient(petStableResult);
	}

	[PacketHandler(Opcode.SMSG_PETITION_SHOW_LIST)]
	private void HandlePetitionShowList(WorldPacket packet)
	{
		ServerPetitionShowList serverPetitionShowList = new ServerPetitionShowList();
		serverPetitionShowList.Unit = packet.ReadGuid().To128(GetSession().GameState);
		GetSession().GameState.CurrentInteractedWithNPC = serverPetitionShowList.Unit;
		byte b = packet.ReadUInt8();
		for (int i = 0; i < b; i++)
		{
			PetitionEntry petitionEntry = default(PetitionEntry);
			petitionEntry.Index = packet.ReadUInt32();
			petitionEntry.CharterEntry = packet.ReadUInt32();
			petitionEntry.IsArena = ((petitionEntry.CharterEntry != 5863) ? 1u : 0u);
			packet.ReadUInt32();
			petitionEntry.CharterCost = packet.ReadUInt32();
			packet.ReadUInt32();
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
			{
				petitionEntry.RequiredSignatures = packet.ReadUInt32();
			}
			else
			{
				petitionEntry.RequiredSignatures = 9u;
			}
			serverPetitionShowList.Petitions.Add(petitionEntry);
		}
		SendPacketToClient(serverPetitionShowList);
	}

	[PacketHandler(Opcode.SMSG_PETITION_SHOW_SIGNATURES)]
	private void HandlePetitionShowSignatures(WorldPacket packet)
	{
		ServerPetitionShowSignatures serverPetitionShowSignatures = new ServerPetitionShowSignatures();
		serverPetitionShowSignatures.Item = packet.ReadGuid().To128(GetSession().GameState);
		serverPetitionShowSignatures.Owner = packet.ReadGuid().To128(GetSession().GameState);
		serverPetitionShowSignatures.OwnerAccountID = GetSession().GetGameAccountGuidForPlayer(serverPetitionShowSignatures.Owner);
		serverPetitionShowSignatures.PetitionID = packet.ReadInt32();
		byte b = packet.ReadUInt8();
		for (int i = 0; i < b; i++)
		{
			ServerPetitionShowSignatures.PetitionSignature petitionSignature = default(ServerPetitionShowSignatures.PetitionSignature);
			petitionSignature.Signer = packet.ReadGuid().To128(GetSession().GameState);
			petitionSignature.Choice = packet.ReadInt32();
			serverPetitionShowSignatures.Signatures.Add(petitionSignature);
		}
		SendPacketToClient(serverPetitionShowSignatures);
	}

	[PacketHandler(Opcode.SMSG_QUERY_PETITION_RESPONSE)]
	private void HandlePetitionQueryResponse(WorldPacket packet)
	{
		QueryPetitionResponse queryPetitionResponse = new QueryPetitionResponse();
		queryPetitionResponse.PetitionID = packet.ReadUInt32();
		queryPetitionResponse.Allow = true;
		queryPetitionResponse.Info = new PetitionInfo();
		queryPetitionResponse.Info.PetitionID = queryPetitionResponse.PetitionID;
		queryPetitionResponse.Info.Petitioner = packet.ReadGuid().To128(GetSession().GameState);
		queryPetitionResponse.Info.Title = packet.ReadCString();
		queryPetitionResponse.Info.BodyText = packet.ReadCString();
		if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			packet.ReadUInt32();
		}
		queryPetitionResponse.Info.MinSignatures = packet.ReadUInt32();
		queryPetitionResponse.Info.MaxSignatures = packet.ReadUInt32();
		queryPetitionResponse.Info.DeadLine = packet.ReadInt32();
		queryPetitionResponse.Info.IssueDate = packet.ReadInt32();
		queryPetitionResponse.Info.AllowedGuildID = packet.ReadInt32();
		queryPetitionResponse.Info.AllowedClasses = packet.ReadInt32();
		queryPetitionResponse.Info.AllowedRaces = packet.ReadInt32();
		queryPetitionResponse.Info.AllowedGender = packet.ReadInt16();
		queryPetitionResponse.Info.AllowedMinLevel = packet.ReadInt32();
		queryPetitionResponse.Info.AllowedMaxLevel = packet.ReadInt32();
		queryPetitionResponse.Info.NumChoices = packet.ReadInt32();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
		{
			for (int i = 0; i < 10; i++)
			{
				queryPetitionResponse.Info.Choicetext[i] = packet.ReadCString();
			}
		}
		queryPetitionResponse.Info.Muid = packet.ReadUInt32();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			queryPetitionResponse.Info.StaticType = packet.ReadInt32();
		}
		SendPacketToClient(queryPetitionResponse);
	}

	[PacketHandler(Opcode.MSG_PETITION_RENAME)]
	private void HandlePetitionRename(WorldPacket packet)
	{
		PetitionRenameGuildResponse petitionRenameGuildResponse = new PetitionRenameGuildResponse();
		petitionRenameGuildResponse.PetitionGuid = packet.ReadGuid().To128(GetSession().GameState);
		petitionRenameGuildResponse.NewGuildName = packet.ReadCString();
		SendPacketToClient(petitionRenameGuildResponse);
	}

	[PacketHandler(Opcode.MSG_PETITION_DECLINE)]
	private void HandlePetitionDecline(WorldPacket packet)
	{
		WowGuid128 guid = packet.ReadGuid().To128(GetSession().GameState);
		string playerName = GetSession().GameState.GetPlayerName(guid);
		if (!string.IsNullOrEmpty(playerName))
		{
			ChatPkt packet2 = new ChatPkt(GetSession(), ChatMessageTypeModern.System, playerName + " has declined your guild invitation.");
			SendPacketToClient(packet2);
		}
	}

	[PacketHandler(Opcode.SMSG_PETITION_SIGN_RESULTS)]
	private void HandlePetitionSignResults(WorldPacket packet)
	{
		PetitionSignResults petitionSignResults = new PetitionSignResults();
		petitionSignResults.Item = packet.ReadGuid().To128(GetSession().GameState);
		petitionSignResults.Player = packet.ReadGuid().To128(GetSession().GameState);
		petitionSignResults.Error = (PetitionSignResult)packet.ReadUInt32();
		SendPacketToClient(petitionSignResults);
	}

	[PacketHandler(Opcode.SMSG_TURN_IN_PETITION_RESULT)]
	private void HandleTurnInPetitionResult(WorldPacket packet)
	{
		TurnInPetitionResult turnInPetitionResult = new TurnInPetitionResult();
		turnInPetitionResult.Result = (PetitionTurnResult)packet.ReadUInt32();
		SendPacketToClient(turnInPetitionResult);
	}

	[PacketHandler(Opcode.SMSG_QUERY_TIME_RESPONSE)]
	private void HandleQueryTimeResponse(WorldPacket packet)
	{
		QueryTimeResponse queryTimeResponse = new QueryTimeResponse();
		queryTimeResponse.CurrentTime = packet.ReadInt32();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180) && packet.CanRead())
		{
			packet.ReadInt32();
		}
		SendPacketToClient(queryTimeResponse);
	}

	[PacketHandler(Opcode.SMSG_QUERY_QUEST_INFO_RESPONSE)]
	private void HandleQueryQuestInfoResponse(WorldPacket packet)
	{
		QueryQuestInfoResponse queryQuestInfoResponse = new QueryQuestInfoResponse();
		KeyValuePair<int, bool> keyValuePair = packet.ReadEntry();
		queryQuestInfoResponse.QuestID = (uint)keyValuePair.Key;
		if (keyValuePair.Value)
		{
			queryQuestInfoResponse.Allow = false;
			SendPacketToClient(queryQuestInfoResponse);
			return;
		}
		queryQuestInfoResponse.Allow = true;
		queryQuestInfoResponse.Info = new QuestTemplate();
		QuestTemplate info = queryQuestInfoResponse.Info;
		info.QuestID = queryQuestInfoResponse.QuestID;
		info.QuestType = packet.ReadInt32();
		info.QuestLevel = packet.ReadInt32();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
		{
			info.MinLevel = packet.ReadInt32();
		}
		else
		{
			info.MinLevel = 1;
		}
		info.QuestSortID = packet.ReadInt32();
		info.QuestInfoID = packet.ReadUInt32();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			info.SuggestedGroupNum = packet.ReadUInt32();
		}
		sbyte b = 0;
		for (int i = 0; i < 2; i++)
		{
			int num = packet.ReadInt32();
			int num2 = packet.ReadInt32();
			if (num != 0 && num2 != 0)
			{
				QuestObjective questObjective = new QuestObjective();
				questObjective.QuestID = queryQuestInfoResponse.QuestID;
				questObjective.Id = QuestObjective.QuestObjectiveCounter++;
				questObjective.StorageIndex = b++;
				questObjective.Type = QuestObjectiveType.MinReputation;
				questObjective.ObjectID = num;
				questObjective.Amount = num2;
				info.Objectives.Add(questObjective);
			}
		}
		info.RewardNextQuest = packet.ReadUInt32();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
		{
			info.RewardXPDifficulty = packet.ReadUInt32();
		}
		int num3 = packet.ReadInt32();
		if (num3 >= 0)
		{
			info.RewardMoney = num3;
		}
		else
		{
			QuestObjective questObjective2 = new QuestObjective();
			questObjective2.QuestID = queryQuestInfoResponse.QuestID;
			questObjective2.Id = QuestObjective.QuestObjectiveCounter++;
			questObjective2.StorageIndex = b++;
			questObjective2.Type = QuestObjectiveType.Money;
			questObjective2.ObjectID = 0;
			questObjective2.Amount = -num3;
			info.Objectives.Add(questObjective2);
		}
		info.RewardBonusMoney = packet.ReadUInt32();
		info.RewardDisplaySpell[0] = packet.ReadUInt32();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			info.RewardSpell = packet.ReadUInt32();
			info.RewardHonor = packet.ReadUInt32();
		}
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
		{
			info.RewardKillHonor = packet.ReadFloat();
		}
		info.StartItem = packet.ReadUInt32();
		info.Flags = packet.ReadUInt32();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_4_0_8089))
		{
			info.RewardTitle = packet.ReadUInt32();
		}
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			int num4 = packet.ReadInt32();
			if (num4 != 0)
			{
				QuestObjective questObjective3 = new QuestObjective();
				questObjective3.QuestID = queryQuestInfoResponse.QuestID;
				questObjective3.Id = QuestObjective.QuestObjectiveCounter++;
				questObjective3.StorageIndex = b++;
				questObjective3.Type = QuestObjectiveType.PlayerKills;
				questObjective3.ObjectID = 0;
				questObjective3.Amount = num4;
				info.Objectives.Add(questObjective3);
			}
			packet.ReadUInt32();
		}
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
		{
			info.RewardArenaPoints = packet.ReadInt32();
		}
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
		{
			packet.ReadInt32();
		}
		for (int j = 0; j < 4; j++)
		{
			info.RewardItems[j] = packet.ReadUInt32();
			info.RewardAmount[j] = packet.ReadUInt32();
		}
		for (int k = 0; k < 6; k++)
		{
			QuestInfoChoiceItem questInfoChoiceItem = default(QuestInfoChoiceItem);
			questInfoChoiceItem.ItemID = packet.ReadUInt32();
			questInfoChoiceItem.Quantity = packet.ReadUInt32();
			uint itemDisplayId = GameData.GetItemDisplayId(questInfoChoiceItem.ItemID);
			if (itemDisplayId != 0)
			{
				questInfoChoiceItem.DisplayID = itemDisplayId;
			}
			info.UnfilteredChoiceItems[k] = questInfoChoiceItem;
		}
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
		{
			for (int l = 0; l < 5; l++)
			{
				info.RewardFactionID[l] = packet.ReadUInt32();
			}
			for (int m = 0; m < 5; m++)
			{
				info.RewardFactionValue[m] = packet.ReadInt32();
			}
			for (int n = 0; n < 5; n++)
			{
				info.RewardFactionOverride[n] = (int)packet.ReadUInt32();
			}
		}
		info.POIContinent = packet.ReadUInt32();
		info.POIx = packet.ReadFloat();
		info.POIy = packet.ReadFloat();
		info.POIPriority = packet.ReadUInt32();
		info.LogTitle = packet.ReadCString();
		info.LogDescription = packet.ReadCString();
		info.QuestDescription = packet.ReadCString();
		info.AreaDescription = packet.ReadCString();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
		{
			info.QuestCompletionLog = packet.ReadCString();
		}
		KeyValuePair<int, bool>[] array = new KeyValuePair<int, bool>[4];
		int num5 = 4;
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_8_9464))
		{
			num5 = 5;
		}
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
		{
			num5 = 6;
		}
		int[] array2 = new int[num5];
		int[] array3 = new int[num5];
		for (int num6 = 0; num6 < 4; num6++)
		{
			array[num6] = packet.ReadEntry();
			bool value = array[num6].Value;
			int key = array[num6].Key;
			int num7 = packet.ReadInt32();
			if (key != 0 && num7 != 0)
			{
				QuestObjective questObjective4 = new QuestObjective();
				questObjective4.QuestID = queryQuestInfoResponse.QuestID;
				questObjective4.Id = QuestObjective.QuestObjectiveCounter++;
				questObjective4.StorageIndex = b++;
				questObjective4.Type = (value ? QuestObjectiveType.GameObject : QuestObjectiveType.Monster);
				questObjective4.ObjectID = key;
				questObjective4.Amount = num7;
				info.Objectives.Add(questObjective4);
			}
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
			{
				array2[num6] = packet.ReadInt32();
			}
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
			{
				array3[num6] = packet.ReadInt32();
			}
			if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_8_9464))
			{
				array2[num6] = packet.ReadInt32();
				array3[num6] = packet.ReadInt32();
			}
		}
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_8_9464))
		{
			for (int num8 = 0; num8 < num5; num8++)
			{
				array2[num8] = packet.ReadInt32();
				array3[num8] = packet.ReadInt32();
			}
		}
		for (int num9 = 0; num9 < num5; num9++)
		{
			if (array2[num9] != 0 && array3[num9] != 0)
			{
				QuestObjective questObjective5 = new QuestObjective();
				questObjective5.QuestID = queryQuestInfoResponse.QuestID;
				questObjective5.Id = QuestObjective.QuestObjectiveCounter++;
				questObjective5.StorageIndex = b++;
				questObjective5.Type = QuestObjectiveType.Item;
				questObjective5.ObjectID = array2[num9];
				questObjective5.Amount = array3[num9];
				info.Objectives.Add(questObjective5);
			}
		}
		for (int num10 = 0; num10 < 4; num10++)
		{
			string description = packet.ReadCString();
			if (info.Objectives.Count > num10)
			{
				info.Objectives[num10].Description = description;
			}
		}
		info.QuestMaxScalingLevel = 255;
		info.RewardXPMultiplier = 1f;
		info.RewardMoneyMultiplier = 1f;
		info.RewardArtifactXPMultiplier = 1f;
		for (int num11 = 0; num11 < 5; num11++)
		{
			info.RewardFactionCapIn[num11] = 7;
		}
		info.AllowableRaces = 511L;
		info.AcceptedSoundKitID = 890u;
		info.CompleteSoundKitID = 878u;
		GameData.StoreQuestTemplate(queryQuestInfoResponse.QuestID, info);
		SendPacketToClient(queryQuestInfoResponse);
	}

	[PacketHandler(Opcode.SMSG_QUERY_CREATURE_RESPONSE)]
	private void HandleQueryCreatureResponse(WorldPacket packet)
	{
		QueryCreatureResponse queryCreatureResponse = new QueryCreatureResponse();
		KeyValuePair<int, bool> keyValuePair = packet.ReadEntry();
		queryCreatureResponse.CreatureID = (uint)keyValuePair.Key;
		if (keyValuePair.Value)
		{
			queryCreatureResponse.Allow = false;
			SendPacketToClient(queryCreatureResponse);
			return;
		}
		queryCreatureResponse.Allow = true;
		queryCreatureResponse.Stats = new CreatureTemplate();
		CreatureTemplate stats = queryCreatureResponse.Stats;
		for (int i = 0; i < 4; i++)
		{
			stats.Name[i] = packet.ReadCString();
		}
		stats.Title = packet.ReadCString();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			stats.CursorName = packet.ReadCString();
		}
		stats.Flags[0] = packet.ReadUInt32();
		stats.Type = packet.ReadInt32();
		stats.Family = packet.ReadInt32();
		stats.Classification = packet.ReadInt32();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
		{
			for (int j = 0; j < 2; j++)
			{
				stats.ProxyCreatureID[j] = packet.ReadUInt32();
			}
		}
		else
		{
			if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
			{
				packet.ReadInt32();
			}
			stats.PetSpellDataId = packet.ReadUInt32();
		}
		int num = ((!LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180)) ? 1 : 4);
		for (int k = 0; k < num; k++)
		{
			uint num2 = packet.ReadUInt32();
			if (num2 != 0)
			{
				stats.Display.CreatureDisplay.Add(new CreatureXDisplay(num2, 1f, 0f));
			}
		}
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			stats.HpMulti = packet.ReadFloat();
			stats.EnergyMulti = packet.ReadFloat();
		}
		else
		{
			stats.HpMulti = 1f;
			stats.EnergyMulti = 1f;
		}
		if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			stats.Civilian = packet.ReadBool();
		}
		stats.Leader = packet.ReadBool();
		int num3 = (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192) ? 6 : 4);
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
		{
			for (uint num4 = 0u; num4 < num3; num4++)
			{
				uint num5 = packet.ReadUInt32();
				if (num5 != 0)
				{
					stats.QuestItems.Add(num5);
				}
			}
			packet.ReadUInt32();
		}
		stats.Flags[0] |= 134217728u;
		stats.MovementInfoID = 1693u;
		stats.Class = 1;
		GameData.StoreCreatureTemplate(queryCreatureResponse.CreatureID, stats);
		SendPacketToClient(queryCreatureResponse);
	}

	[PacketHandler(Opcode.SMSG_QUERY_GAME_OBJECT_RESPONSE)]
	private void HandleQueryGameObjectResposne(WorldPacket packet)
	{
		QueryGameObjectResponse queryGameObjectResponse = new QueryGameObjectResponse();
		KeyValuePair<int, bool> keyValuePair = packet.ReadEntry();
		queryGameObjectResponse.GameObjectID = (uint)keyValuePair.Key;
		queryGameObjectResponse.Guid = WowGuid128.Empty;
		if (keyValuePair.Value)
		{
			queryGameObjectResponse.Allow = false;
			SendPacketToClient(queryGameObjectResponse);
			return;
		}
		queryGameObjectResponse.Allow = true;
		queryGameObjectResponse.Stats = new GameObjectStats();
		GameObjectStats stats = queryGameObjectResponse.Stats;
		stats.Type = packet.ReadUInt32();
		stats.DisplayID = packet.ReadUInt32();
		for (int i = 0; i < 4; i++)
		{
			stats.Name[i] = packet.ReadCString();
		}
		stats.IconName = packet.ReadCString();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			stats.CastBarCaption = packet.ReadCString();
			stats.UnkString = packet.ReadCString();
		}
		for (int j = 0; j < 24; j++)
		{
			stats.Data[j] = packet.ReadInt32();
		}
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			stats.Size = packet.ReadFloat();
		}
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
		{
			uint num = (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192) ? 6u : 4u);
			for (uint num2 = 0u; num2 < num; num2++)
			{
				uint num3 = packet.ReadUInt32();
				if (num3 != 0)
				{
					stats.QuestItems.Add(num3);
				}
			}
		}
		SendPacketToClient(queryGameObjectResponse);
	}

	[PacketHandler(Opcode.SMSG_QUERY_PAGE_TEXT_RESPONSE)]
	private void HandleQueryPageTextResponse(WorldPacket packet)
	{
		QueryPageTextResponse queryPageTextResponse = new QueryPageTextResponse();
		queryPageTextResponse.PageTextID = packet.ReadUInt32();
		queryPageTextResponse.Allow = true;
		QueryPageTextResponse.PageTextInfo pageTextInfo = default(QueryPageTextResponse.PageTextInfo);
		pageTextInfo.Id = queryPageTextResponse.PageTextID;
		pageTextInfo.Text = packet.ReadCString();
		pageTextInfo.NextPageID = packet.ReadUInt32();
		queryPageTextResponse.Pages.Add(pageTextInfo);
		SendPacketToClient(queryPageTextResponse);
	}

	[PacketHandler(Opcode.SMSG_QUERY_NPC_TEXT_RESPONSE)]
	private void HandleQueryNpcTextResponse(WorldPacket packet)
	{
		QueryNPCTextResponse queryNPCTextResponse = new QueryNPCTextResponse();
		KeyValuePair<int, bool> keyValuePair = packet.ReadEntry();
		queryNPCTextResponse.TextID = (uint)keyValuePair.Key;
		if (keyValuePair.Value)
		{
			queryNPCTextResponse.Allow = false;
			SendPacketToClient(queryNPCTextResponse);
			return;
		}
		queryNPCTextResponse.Allow = true;
		for (int i = 0; i < 8; i++)
		{
			queryNPCTextResponse.Probabilities[i] = packet.ReadFloat();
			string text = packet.ReadCString().TrimEnd().Replace("\0", "");
			string text2 = packet.ReadCString().TrimEnd().Replace("\0", "");
			uint language = packet.ReadUInt32();
			ushort[] array = new ushort[3];
			ushort[] array2 = new ushort[3];
			for (int j = 0; j < 3; j++)
			{
				array[j] = (ushort)packet.ReadUInt32();
				array2[j] = (ushort)packet.ReadUInt32();
			}
			if ((string.IsNullOrEmpty(text) && string.IsNullOrEmpty(text2)) || (text.Equals("Greetings $N") && text2.Equals("Greetings $N") && i != 0))
			{
				queryNPCTextResponse.BroadcastTextID[i] = 0u;
			}
			else
			{
				queryNPCTextResponse.BroadcastTextID[i] = GameData.GetBroadcastTextId(text, text2, language, array, array2);
			}
		}
		SendPacketToClient(queryNPCTextResponse);
	}

    // 处理物品查询响应
    [PacketHandler(Opcode.SMSG_ITEM_QUERY_SINGLE_RESPONSE)]
	private void HandleItemQueryResponse(WorldPacket packet)
	{
		KeyValuePair<int, bool> keyValuePair = packet.ReadEntry();
		if (keyValuePair.Value)
		{
			if (GetSession().GameState.RequestedItemHotfixes.Contains((uint)keyValuePair.Key))
			{
				DBReply dBReply = new DBReply();
				dBReply.RecordID = (uint)keyValuePair.Key;
				dBReply.TableHash = DB2Hash.Item;
				dBReply.Status = HotfixStatus.Invalid;
				dBReply.Timestamp = (uint)Time.UnixTime;
				SendPacketToClient(dBReply);
			}
			if (GetSession().GameState.RequestedItemSparseHotfixes.Contains((uint)keyValuePair.Key))
			{
				DBReply dBReply2 = new DBReply();
				dBReply2.RecordID = (uint)keyValuePair.Key;
				dBReply2.TableHash = DB2Hash.ItemSparse;
				dBReply2.Status = HotfixStatus.Invalid;
				dBReply2.Timestamp = (uint)Time.UnixTime;
				SendPacketToClient(dBReply2);
			}
		}
		else
		{
			ItemTemplate itemTemplate = new ItemTemplate();
			itemTemplate.ReadFromLegacyPacket((uint)keyValuePair.Key, packet);
			SendItemUpdatesIfNeeded(itemTemplate);
			GameData.StoreItemTemplate((uint)keyValuePair.Key, itemTemplate);
		}
	}

    // 如果需要，发送装备更新
    private void SendItemUpdatesIfNeeded(ItemTemplate item)
	{
		HotFixMessage hotFixMessage = GameData.GenerateItemUpdateIfNeeded(item);
		if (hotFixMessage != null)
		{
			SendPacketToClient(hotFixMessage);
		}
		hotFixMessage = GameData.GenerateItemSparseUpdateIfNeeded(item);
		if (hotFixMessage != null)
		{
			SendPacketToClient(hotFixMessage);
			DBReply dBReply = new DBReply();
			dBReply.Status = HotfixStatus.Valid;
			dBReply.Timestamp = (uint)Time.UnixTime;
			dBReply.RecordID = hotFixMessage.Hotfixes[0].RecordId;
			dBReply.TableHash = hotFixMessage.Hotfixes[0].TableHash;
			dBReply.Data = hotFixMessage.Hotfixes[0].HotfixContent;
			SendPacketToClient(dBReply);
		}
		for (byte b = 0; b < 5; b++)
		{
            // 如果需要，生成物品效果更新
            hotFixMessage = GameData.GenerateItemEffectUpdateIfNeeded(item, b);
			if (hotFixMessage != null)
			{
				SendPacketToClient(hotFixMessage);
			}
		}
		if (GameData.ItemCanHaveModel(item))
		{
			hotFixMessage = GameData.GenerateItemAppearanceUpdateIfNeeded(item);
			if (hotFixMessage != null)
			{
				SendPacketToClient(hotFixMessage);
			}
			hotFixMessage = GameData.GenerateItemModifiedAppearanceUpdateIfNeeded(item);
			if (hotFixMessage != null)
			{
				SendPacketToClient(hotFixMessage);
			}
		}
	}

	[PacketHandler(Opcode.SMSG_QUERY_PET_NAME_RESPONSE)]
	private void HandleQueryPetNameResponse(WorldPacket packet)
	{
		uint num = packet.ReadUInt32();
		WowGuid128 petGuidByNumber = GetSession().GameState.GetPetGuidByNumber(num);
		if (petGuidByNumber == null)
		{
			Log.Print(LogType.Error, $"Pet name query response for unknown pet {num}!", "HandleQueryPetNameResponse", "D:\\a\\HermesProxy\\HermesProxy\\World\\Client\\PacketHandlers\\QueryHandler.cs");
			return;
		}
		QueryPetNameResponse queryPetNameResponse = new QueryPetNameResponse();
		queryPetNameResponse.UnitGUID = petGuidByNumber;
		queryPetNameResponse.Name = packet.ReadCString();
		if (queryPetNameResponse.Name.Length == 0)
		{
			queryPetNameResponse.Allow = false;
			packet.ReadBytes(7u);
			return;
		}
		queryPetNameResponse.Allow = true;
		queryPetNameResponse.Timestamp = packet.ReadUInt32();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180) && packet.ReadBool())
		{
			for (int i = 0; i < 5; i++)
			{
				queryPetNameResponse.DeclinedNames.name[i] = packet.ReadCString();
			}
		}
		SendPacketToClient(queryPetNameResponse);
	}

	[PacketHandler(Opcode.SMSG_ITEM_NAME_QUERY_RESPONSE)]
	private void HandleItemNameQueryResponse(WorldPacket packet)
	{
		uint entry = packet.ReadUInt32();
		string name = packet.ReadCString();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			packet.ReadUInt32();
		}
		GameData.StoreItemName(entry, name);
	}

	[PacketHandler(Opcode.SMSG_WHO)]
	private void HandleWhoResponse(WorldPacket packet)
	{
		WhoResponsePkt whoResponsePkt = new WhoResponsePkt();
		whoResponsePkt.RequestID = GetSession().GameState.LastWhoRequestId;
		uint num = packet.ReadUInt32();
		packet.ReadUInt32();
		for (int i = 0; i < num; i++)
		{
			WhoEntry whoEntry = new WhoEntry();
			whoEntry.PlayerData.Name = packet.ReadCString();
			whoEntry.GuildName = packet.ReadCString();
			whoEntry.PlayerData.Level = (byte)packet.ReadUInt32();
			whoEntry.PlayerData.ClassID = (Class)packet.ReadUInt32();
			whoEntry.PlayerData.RaceID = (Race)packet.ReadUInt32();
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
			{
				whoEntry.PlayerData.Sex = (Gender)packet.ReadUInt8();
			}
			whoEntry.AreaID = packet.ReadInt32();
			whoEntry.PlayerData.GuidActual = GetSession().GameState.GetPlayerGuidByName(whoEntry.PlayerData.Name);
			if (whoEntry.PlayerData.GuidActual == null)
			{
				whoEntry.PlayerData.GuidActual = WowGuid128.CreateUnknownPlayerGuid();
			}
			whoEntry.PlayerData.AccountID = GetSession().GetGameAccountGuidForPlayer(whoEntry.PlayerData.GuidActual);
			whoEntry.PlayerData.BnetAccountID = GetSession().GetBnetAccountGuidForPlayer(whoEntry.PlayerData.GuidActual);
			whoEntry.PlayerData.VirtualRealmAddress = GetSession().RealmId.GetAddress();
			if (!string.IsNullOrEmpty(whoEntry.GuildName))
			{
				whoEntry.GuildGUID = GetSession().GetGuildGuid(whoEntry.GuildName);
				whoEntry.GuildVirtualRealmAddress = whoEntry.PlayerData.VirtualRealmAddress;
			}
			whoResponsePkt.Players.Add(whoEntry);
			Session.GameState.UpdatePlayerCache(whoEntry.PlayerData.GuidActual, new PlayerCache
			{
				Name = whoEntry.PlayerData.Name,
				RaceId = whoEntry.PlayerData.RaceID,
				ClassId = whoEntry.PlayerData.ClassID,
				SexId = whoEntry.PlayerData.Sex,
				Level = whoEntry.PlayerData.Level
			});
		}
		SendPacketToClient(whoResponsePkt);
	}

	[PacketHandler(Opcode.SMSG_QUEST_GIVER_QUEST_DETAILS)]
	private void HandleQuestGiverQuestDetails(WorldPacket packet)
	{
		QuestGiverQuestDetails questGiverQuestDetails = new QuestGiverQuestDetails();
		questGiverQuestDetails.QuestGiverGUID = packet.ReadGuid().To128(GetSession().GameState);
		GetSession().GameState.CurrentInteractedWithNPC = questGiverQuestDetails.QuestGiverGUID;
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			questGiverQuestDetails.InformUnit = packet.ReadGuid().To128(GetSession().GameState);
		}
		else
		{
			questGiverQuestDetails.InformUnit = questGiverQuestDetails.QuestGiverGUID;
		}
		questGiverQuestDetails.QuestID = packet.ReadUInt32();
		questGiverQuestDetails.QuestTitle = packet.ReadCString();
		questGiverQuestDetails.DescriptionText = packet.ReadCString();
		questGiverQuestDetails.LogDescription = packet.ReadCString();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
		{
			questGiverQuestDetails.AutoLaunched = packet.ReadBool();
		}
		else
		{
			questGiverQuestDetails.AutoLaunched = packet.ReadUInt32() != 0;
		}
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_3_11685))
		{
			questGiverQuestDetails.QuestFlags[0] = packet.ReadUInt32();
		}
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			questGiverQuestDetails.SuggestedPartyMembers = packet.ReadUInt32();
		}
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			packet.ReadUInt8();
		}
		if (LegacyVersion.InVersion(ClientVersionBuild.V3_1_0_9767, ClientVersionBuild.V3_3_3a_11723))
		{
			questGiverQuestDetails.StartCheat = packet.ReadBool();
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_2_11403))
			{
				questGiverQuestDetails.DisplayPopup = packet.ReadBool();
			}
		}
		if (questGiverQuestDetails.QuestFlags[0].HasAnyFlag(QuestFlags.HiddenRewards) && LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_3_5a_12340))
		{
			packet.ReadUInt32();
			packet.ReadUInt32();
			questGiverQuestDetails.Rewards.Money = packet.ReadUInt32();
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_2_10482))
			{
				questGiverQuestDetails.Rewards.XP = packet.ReadUInt32();
			}
		}
		ReadExtraQuestInfo(packet, questGiverQuestDetails.Rewards, readFlags: false);
		uint num = packet.ReadUInt32();
		for (int i = 0; i < num; i++)
		{
			questGiverQuestDetails.DescEmotes[i].Type = packet.ReadUInt32();
			questGiverQuestDetails.DescEmotes[i].Delay = packet.ReadUInt32();
		}
		SendPacketToClient(questGiverQuestDetails);
	}

	private void ReadExtraQuestInfo(WorldPacket packet, QuestRewards rewards, bool readFlags)
	{
		rewards.ChoiceItemCount = packet.ReadUInt32();
		for (int i = 0; i < rewards.ChoiceItemCount; i++)
		{
			rewards.ChoiceItems[i].Item.ItemID = packet.ReadUInt32();
			rewards.ChoiceItems[i].Quantity = packet.ReadUInt32();
			packet.ReadUInt32();
		}
		uint num = packet.ReadUInt32();
		for (int j = 0; j < num; j++)
		{
			rewards.ItemID[j] = packet.ReadUInt32();
			rewards.ItemQty[j] = packet.ReadUInt32();
			packet.ReadUInt32();
		}
		rewards.Money = packet.ReadUInt32();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_2_10482))
		{
			rewards.XP = packet.ReadUInt32();
		}
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_3_0_7561))
		{
			rewards.Honor = packet.ReadUInt32();
		}
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
		{
			packet.ReadFloat();
		}
		if (readFlags)
		{
			packet.ReadUInt32();
		}
		rewards.SpellCompletionID = packet.ReadUInt32();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			packet.ReadUInt32();
		}
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_4_0_8089))
		{
			rewards.Title = packet.ReadUInt32();
		}
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			rewards.NumSkillUps = packet.ReadUInt32();
		}
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
		{
			packet.ReadUInt32();
			packet.ReadUInt32();
		}
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
		{
			for (int k = 0; k < 5; k++)
			{
				rewards.FactionID[k] = packet.ReadUInt32();
			}
			for (int l = 0; l < 5; l++)
			{
				rewards.FactionValue[l] = packet.ReadInt32();
			}
			for (int m = 0; m < 5; m++)
			{
				packet.ReadInt32();
			}
		}
	}

	[PacketHandler(Opcode.SMSG_QUEST_GIVER_STATUS)]
	private void HandleQuestGiverStatus(WorldPacket packet)
	{
		QuestGiverStatusPkt questGiverStatusPkt = new QuestGiverStatusPkt();
		questGiverStatusPkt.QuestGiver.Guid = packet.ReadGuid().To128(GetSession().GameState);
		questGiverStatusPkt.QuestGiver.Status = LegacyVersion.ConvertQuestGiverStatus(packet.ReadUInt8());
		SendPacketToClient(questGiverStatusPkt);
	}

	[PacketHandler(Opcode.SMSG_QUEST_GIVER_STATUS_MULTIPLE)]
	private void HandleQuestGiverStatusMultple(WorldPacket packet)
	{
		QuestGiverStatusMultiple questGiverStatusMultiple = new QuestGiverStatusMultiple();
		int num = packet.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			QuestGiverInfo questGiverInfo = new QuestGiverInfo();
			questGiverInfo.Guid = packet.ReadGuid().To128(GetSession().GameState);
			questGiverInfo.Status = LegacyVersion.ConvertQuestGiverStatus(packet.ReadUInt8());
			questGiverStatusMultiple.QuestGivers.Add(questGiverInfo);
		}
		SendPacketToClient(questGiverStatusMultiple);
	}

	[PacketHandler(Opcode.SMSG_QUEST_GIVER_QUEST_LIST_MESSAGE)]
	private void HandleQuestGiverQuestListMessage(WorldPacket packet)
	{
		QuestGiverQuestListMessage questGiverQuestListMessage = new QuestGiverQuestListMessage();
		questGiverQuestListMessage.QuestGiverGUID = packet.ReadGuid().To128(GetSession().GameState);
		GetSession().GameState.CurrentInteractedWithNPC = questGiverQuestListMessage.QuestGiverGUID;
		questGiverQuestListMessage.Greeting = packet.ReadCString();
		questGiverQuestListMessage.GreetEmoteDelay = packet.ReadUInt32();
		questGiverQuestListMessage.GreetEmoteType = packet.ReadUInt32();
		byte b = packet.ReadUInt8();
		for (int i = 0; i < b; i++)
		{
			ClientGossipQuest clientGossipQuest = ReadGossipQuestOption(packet);
			questGiverQuestListMessage.QuestOptions.Add(clientGossipQuest);
		}
		SendPacketToClient(questGiverQuestListMessage);
	}

	private ClientGossipQuest ReadGossipQuestOption(WorldPacket packet)
	{
		ClientGossipQuest clientGossipQuest = new ClientGossipQuest();
		clientGossipQuest.QuestID = packet.ReadUInt32();
		if (LegacyVersion.ConvertQuestGiverStatus((byte)packet.ReadInt32()).HasAnyFlag(QuestGiverStatusModern.LowLevelAvailable | QuestGiverStatusModern.LowLevelAvailableRep | QuestGiverStatusModern.AvailableRep | QuestGiverStatusModern.Available | QuestGiverStatusModern.AvailableLegendaryQuest | QuestGiverStatusModern.AvailableJourney | QuestGiverStatusModern.AvailableCovenantCalling))
		{
			clientGossipQuest.QuestType = 2;
		}
		else
		{
			clientGossipQuest.QuestType = 4;
		}
		clientGossipQuest.QuestLevel = packet.ReadInt32();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_3_11685))
		{
			clientGossipQuest.QuestFlags = packet.ReadUInt32();
		}
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_3_11685))
		{
			clientGossipQuest.Repeatable = packet.ReadBool();
		}
		clientGossipQuest.QuestTitle = packet.ReadCString();
		return clientGossipQuest;
	}

	[PacketHandler(Opcode.SMSG_QUEST_GIVER_REQUEST_ITEMS)]
	private void HandleQuestGiverRequestItems(WorldPacket packet)
	{
		QuestGiverRequestItems questGiverRequestItems = new QuestGiverRequestItems();
		questGiverRequestItems.QuestGiverGUID = packet.ReadGuid().To128(GetSession().GameState);
		GetSession().GameState.CurrentInteractedWithNPC = questGiverRequestItems.QuestGiverGUID;
		questGiverRequestItems.QuestGiverCreatureID = questGiverRequestItems.QuestGiverGUID.GetEntry();
		questGiverRequestItems.QuestID = packet.ReadUInt32();
		questGiverRequestItems.QuestTitle = packet.ReadCString();
		questGiverRequestItems.CompletionText = packet.ReadCString();
		questGiverRequestItems.CompEmoteDelay = packet.ReadUInt32();
		questGiverRequestItems.CompEmoteType = packet.ReadUInt32();
		questGiverRequestItems.AutoLaunched = packet.ReadUInt32() != 0;
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_3_11685))
		{
			questGiverRequestItems.QuestFlags[0] = packet.ReadUInt32();
		}
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			questGiverRequestItems.SuggestPartyMembers = packet.ReadUInt32();
		}
		questGiverRequestItems.MoneyToGet = packet.ReadInt32();
		uint num = packet.ReadUInt32();
		for (int i = 0; i < num; i++)
		{
			QuestObjectiveCollect questObjectiveCollect = default(QuestObjectiveCollect);
			questObjectiveCollect.ObjectID = packet.ReadUInt32();
			questObjectiveCollect.Amount = packet.ReadUInt32();
			packet.ReadUInt32();
			questGiverRequestItems.Collect.Add(questObjectiveCollect);
		}
		if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			packet.ReadUInt32();
		}
		if ((packet.ReadUInt32() & 3u) != 0)
		{
			questGiverRequestItems.StatusFlags = 223u;
		}
		else
		{
			questGiverRequestItems.StatusFlags = 219u;
		}
		packet.ReadUInt32();
		packet.ReadUInt32();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			packet.ReadUInt32();
		}
		SendPacketToClient(questGiverRequestItems);
	}

	[PacketHandler(Opcode.SMSG_QUEST_GIVER_OFFER_REWARD_MESSAGE)]
	private void HandleQuestGiverOfferRewardMessage(WorldPacket packet)
	{
		QuestGiverOfferRewardMessage questGiverOfferRewardMessage = new QuestGiverOfferRewardMessage();
		questGiverOfferRewardMessage.QuestData.QuestGiverGUID = packet.ReadGuid().To128(GetSession().GameState);
		GetSession().GameState.CurrentInteractedWithNPC = questGiverOfferRewardMessage.QuestData.QuestGiverGUID;
		questGiverOfferRewardMessage.QuestData.QuestGiverCreatureID = questGiverOfferRewardMessage.QuestData.QuestGiverGUID.GetEntry();
		questGiverOfferRewardMessage.QuestData.QuestID = packet.ReadUInt32();
		questGiverOfferRewardMessage.QuestTitle = packet.ReadCString();
		questGiverOfferRewardMessage.RewardText = packet.ReadCString();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
		{
			questGiverOfferRewardMessage.QuestData.AutoLaunched = packet.ReadBool();
		}
		else
		{
			questGiverOfferRewardMessage.QuestData.AutoLaunched = packet.ReadUInt32() != 0;
		}
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_3_11685))
		{
			questGiverOfferRewardMessage.QuestData.QuestFlags[0] = packet.ReadUInt32();
		}
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			questGiverOfferRewardMessage.QuestData.SuggestedPartyMembers = packet.ReadUInt32();
		}
		uint num = packet.ReadUInt32();
		for (int i = 0; i < num; i++)
		{
			QuestDescEmote questDescEmote = default(QuestDescEmote);
			questDescEmote.Delay = packet.ReadUInt32();
			questDescEmote.Type = packet.ReadUInt32();
		}
		ReadExtraQuestInfo(packet, questGiverOfferRewardMessage.QuestData.Rewards, readFlags: true);
		SendPacketToClient(questGiverOfferRewardMessage);
	}

	[PacketHandler(Opcode.SMSG_QUEST_GIVER_QUEST_COMPLETE)]
	private void HandleQuestGiverQuestComplete(WorldPacket packet)
	{
		QuestGiverQuestComplete questGiverQuestComplete = new QuestGiverQuestComplete();
		questGiverQuestComplete.QuestID = packet.ReadUInt32();
		GetSession().GameState.CurrentPlayerStorage.CompletedQuests.MarkQuestAsCompleted(questGiverQuestComplete.QuestID);
		if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			packet.ReadUInt32();
		}
		questGiverQuestComplete.XPReward = packet.ReadUInt32();
		questGiverQuestComplete.MoneyReward = packet.ReadInt32();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_3_0_7561))
		{
			packet.ReadInt32();
		}
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			packet.ReadInt32();
			packet.ReadInt32();
		}
		uint num = 0u;
		uint num2 = 0u;
		if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			uint num3 = packet.ReadUInt32();
			for (uint num4 = 0u; num4 < num3; num4++)
			{
				uint num5 = packet.ReadUInt32();
				uint num6 = packet.ReadUInt32();
				if (num5 != 0 && num6 != 0)
				{
					num = num5;
					num2 = num6;
				}
			}
		}
		questGiverQuestComplete.ItemReward.ItemID = num;
		QuestTemplate questTemplate = GameData.GetQuestTemplate(questGiverQuestComplete.QuestID);
		if (questTemplate != null && questTemplate.RewardNextQuest == 0)
		{
			questGiverQuestComplete.LaunchQuest = false;
			if (GetSession().GameState.CurrentInteractedWithNPC != null && GetSession().GameState.GetLegacyFieldValueUInt32(GetSession().GameState.CurrentInteractedWithNPC, UnitField.UNIT_NPC_FLAGS).HasAnyFlag(NPCFlags.Gossip))
			{
				questGiverQuestComplete.LaunchGossip = true;
			}
		}
		SendPacketToClient(questGiverQuestComplete);
		DisplayToast displayToast = new DisplayToast();
		displayToast.QuestID = questGiverQuestComplete.QuestID;
		if (num != 0 && num2 != 0)
		{
			displayToast.Quantity = 1uL;
			displayToast.Type = 0;
			displayToast.ItemReward.ItemID = num;
		}
		else
		{
			displayToast.Quantity = 60uL;
			displayToast.Type = 2;
		}
		SendPacketToClient(displayToast);
	}

	[PacketHandler(Opcode.SMSG_QUEST_GIVER_QUEST_FAILED)]
	private void HandleQuestGiverQuestFailed(WorldPacket packet)
	{
		QuestGiverQuestFailed questGiverQuestFailed = new QuestGiverQuestFailed();
		questGiverQuestFailed.QuestID = packet.ReadUInt32();
		questGiverQuestFailed.Reason = LegacyVersion.ConvertInventoryResult(packet.ReadUInt32());
		SendPacketToClient(questGiverQuestFailed);
	}

	[PacketHandler(Opcode.SMSG_QUEST_GIVER_INVALID_QUEST)]
	private void HandleQuestGiverInvalidQuest(WorldPacket packet)
	{
		QuestGiverInvalidQuest questGiverInvalidQuest = new QuestGiverInvalidQuest();
		questGiverInvalidQuest.Reason = (QuestFailedReasons)packet.ReadUInt32();
		SendPacketToClient(questGiverInvalidQuest);
	}

	[PacketHandler(Opcode.SMSG_QUEST_UPDATE_COMPLETE)]
	[PacketHandler(Opcode.SMSG_QUEST_UPDATE_FAILED)]
	[PacketHandler(Opcode.SMSG_QUEST_UPDATE_FAILED_TIMER)]
	private void HandleQuestUpdateStatus(WorldPacket packet)
	{
		QuestUpdateStatus questUpdateStatus = new QuestUpdateStatus(packet.GetUniversalOpcode(isModern: false));
		questUpdateStatus.QuestID = packet.ReadUInt32();
		SendPacketToClient(questUpdateStatus);
	}

	[PacketHandler(Opcode.SMSG_QUEST_UPDATE_ADD_ITEM)]
	private void HandleQuestUpdateAddItem(WorldPacket packet)
	{
		uint entry = packet.ReadUInt32();
		packet.ReadUInt32();
		if (GameData.GetQuestObjectiveForItem(entry) != null)
		{
			return;
		}
		Dictionary<int, UpdateField> cachedObjectFieldsLegacy = GetSession().GameState.GetCachedObjectFieldsLegacy(GetSession().GameState.CurrentPlayerGuid);
		int questLogSize = LegacyVersion.GetQuestLogSize();
		for (int i = 0; i < questLogSize; i++)
		{
			QuestLog questLog = ReadQuestLogEntry(i, null, cachedObjectFieldsLegacy);
			if (questLog != null && questLog.QuestID.HasValue && GameData.GetQuestTemplate((uint)questLog.QuestID.Value) == null)
			{
				WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_QUERY_QUEST_INFO);
				worldPacket.WriteUInt32((uint)questLog.QuestID.Value);
				SendPacketToServer(worldPacket);
			}
		}
	}

	[PacketHandler(Opcode.SMSG_QUEST_UPDATE_ADD_KILL)]
	private void HandleQuestUpdateAddKill(WorldPacket packet)
	{
		QuestUpdateAddCredit questUpdateAddCredit = new QuestUpdateAddCredit();
		questUpdateAddCredit.QuestID = packet.ReadUInt32();
		KeyValuePair<int, bool> keyValuePair = packet.ReadEntry();
		questUpdateAddCredit.ObjectID = keyValuePair.Key;
		questUpdateAddCredit.ObjectiveType = (keyValuePair.Value ? QuestObjectiveType.GameObject : QuestObjectiveType.Monster);
		questUpdateAddCredit.Count = (ushort)packet.ReadUInt32();
		questUpdateAddCredit.Required = (ushort)packet.ReadUInt32();
		questUpdateAddCredit.VictimGUID = packet.ReadGuid().To128(GetSession().GameState);
		SendPacketToClient(questUpdateAddCredit);
	}

	[PacketHandler(Opcode.SMSG_QUEST_CONFIRM_ACCEPT)]
	private void HandleQuestConfirmAccept(WorldPacket packet)
	{
		QuestConfirmAccept questConfirmAccept = new QuestConfirmAccept();
		questConfirmAccept.QuestID = packet.ReadUInt32();
		questConfirmAccept.QuestTitle = packet.ReadCString();
		questConfirmAccept.InitiatedBy = packet.ReadGuid().To128(GetSession().GameState);
		SendPacketToClient(questConfirmAccept);
	}

	[PacketHandler(Opcode.MSG_QUEST_PUSH_RESULT)]
	private void HandleQuestPushResult(WorldPacket packet)
	{
		QuestPushResult questPushResult = new QuestPushResult();
		questPushResult.SenderGUID = packet.ReadGuid().To128(GetSession().GameState);
		questPushResult.Result = (QuestPushReason)packet.ReadUInt8();
		SendPacketToClient(questPushResult);
	}

	[PacketHandler(Opcode.SMSG_INITIALIZE_FACTIONS)]
	private void HandleInitializeFactions(WorldPacket packet)
	{
		if (GetSession().GameState.IsFirstEnterWorld)
		{
			InitializeFactions initializeFactions = new InitializeFactions();
			uint num = packet.ReadUInt32();
			for (uint num2 = 0u; num2 < num; num2++)
			{
				initializeFactions.FactionFlags[num2] = (ReputationFlags)packet.ReadUInt8();
				initializeFactions.FactionStandings[num2] = packet.ReadInt32();
			}
			SendPacketToClient(initializeFactions);
			if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
			{
				SendPacketToClient(new TimeSyncRequest());
			}
		}
	}

	[PacketHandler(Opcode.SMSG_SET_FACTION_STANDING)]
	private void HandleSetFactionStanding(WorldPacket packet)
	{
		SetFactionStanding setFactionStanding = new SetFactionStanding();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_4_0_8089))
		{
			packet.ReadFloat();
		}
		bool showVisual = true;
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			showVisual = packet.ReadBool();
		}
		setFactionStanding.ShowVisual = showVisual;
		int num = packet.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			FactionStandingData factionStandingData = default(FactionStandingData);
			factionStandingData.Index = packet.ReadInt32();
			factionStandingData.Standing = packet.ReadInt32();
			setFactionStanding.Factions.Add(factionStandingData);
		}
		SendPacketToClient(setFactionStanding);
	}

	[PacketHandler(Opcode.SMSG_SET_FORCED_REACTIONS)]
	private void HandleSetForcedReaction(WorldPacket packet)
	{
		SetForcedReactions setForcedReactions = new SetForcedReactions();
		int num = packet.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			ForcedReaction forcedReaction = default(ForcedReaction);
			forcedReaction.Faction = packet.ReadInt32();
			forcedReaction.Reaction = packet.ReadInt32();
			setForcedReactions.Reactions.Add(forcedReaction);
		}
		SendPacketToClient(setForcedReactions);
	}

	[PacketHandler(Opcode.SMSG_SET_FACTION_VISIBLE)]
	private void HandleSetFactionVisible(WorldPacket packet)
	{
		SetFactionVisible setFactionVisible = new SetFactionVisible(visible: true);
		setFactionVisible.FactionIndex = packet.ReadUInt32();
		SendPacketToClient(setFactionVisible);
	}

	[PacketHandler(Opcode.SMSG_FRIEND_LIST)]
	private void HandleFriendList(WorldPacket packet)
	{
		ContactList contactList = new ContactList();
		contactList.Flags = SocialFlag.Friend;
		byte b = packet.ReadUInt8();
		for (int i = 0; i < b; i++)
		{
			ContactInfo contactInfo = new ContactInfo();
			contactInfo.TypeFlags = SocialFlag.Friend;
			contactInfo.Guid = packet.ReadGuid().To128(GetSession().GameState);
			contactInfo.WowAccountGuid = GetSession().GetGameAccountGuidForPlayer(contactInfo.Guid);
			contactInfo.NativeRealmAddr = GetSession().RealmId.GetAddress();
			contactInfo.VirtualRealmAddr = GetSession().RealmId.GetAddress();
			contactInfo.Status = (FriendStatus)packet.ReadUInt8();
			if (contactInfo.Status != 0)
			{
				contactInfo.AreaID = packet.ReadUInt32();
				contactInfo.Level = packet.ReadUInt32();
				contactInfo.ClassID = (Class)packet.ReadUInt32();
			}
			contactList.Contacts.Add(contactInfo);
		}
		SendPacketToClient(contactList);
	}

	[PacketHandler(Opcode.SMSG_IGNORE_LIST)]
	private void HandleIgnoreList(WorldPacket packet)
	{
		ContactList contactList = new ContactList();
		contactList.Flags = SocialFlag.Ignored;
		byte b = packet.ReadUInt8();
		HashSet<WowGuid128> hashSet = new HashSet<WowGuid128>();
		for (int i = 0; i < b; i++)
		{
			ContactInfo contactInfo = new ContactInfo();
			contactInfo.TypeFlags = SocialFlag.Ignored;
			contactInfo.Guid = packet.ReadGuid().To128(GetSession().GameState);
			contactInfo.WowAccountGuid = GetSession().GetGameAccountGuidForPlayer(contactInfo.Guid);
			contactInfo.NativeRealmAddr = GetSession().RealmId.GetAddress();
			contactInfo.VirtualRealmAddr = GetSession().RealmId.GetAddress();
			contactList.Contacts.Add(contactInfo);
			hashSet.Add(contactInfo.Guid);
		}
		Session.GameState.IgnoredPlayers = hashSet;
		SendPacketToClient(contactList);
	}

	[PacketHandler(Opcode.SMSG_CONTACT_LIST)]
	private void HandleContactList(WorldPacket packet)
	{
		ContactList contactList = new ContactList();
		contactList.Flags = (SocialFlag)packet.ReadUInt32();
		uint num = packet.ReadUInt32();
		for (int i = 0; i < num; i++)
		{
			ContactInfo contactInfo = new ContactInfo();
			contactInfo.Guid = packet.ReadGuid().To128(GetSession().GameState);
			contactInfo.WowAccountGuid = GetSession().GetGameAccountGuidForPlayer(contactInfo.Guid);
			contactInfo.NativeRealmAddr = GetSession().RealmId.GetAddress();
			contactInfo.VirtualRealmAddr = GetSession().RealmId.GetAddress();
			contactInfo.TypeFlags = (SocialFlag)packet.ReadUInt32();
			contactInfo.Note = packet.ReadCString();
			if (contactInfo.TypeFlags.HasAnyFlag(SocialFlag.Friend))
			{
				contactInfo.Status = (FriendStatus)packet.ReadUInt8();
				if (contactInfo.Status != 0)
				{
					contactInfo.AreaID = packet.ReadUInt32();
					contactInfo.Level = packet.ReadUInt32();
					contactInfo.ClassID = (Class)packet.ReadUInt32();
				}
			}
			contactList.Contacts.Add(contactInfo);
		}
		SendPacketToClient(contactList);
	}

	[PacketHandler(Opcode.SMSG_FRIEND_STATUS)]
	private void HandleFriendStatus(WorldPacket packet)
	{
		FriendStatusPkt friendStatusPkt = new FriendStatusPkt();
		friendStatusPkt.FriendResult = (FriendsResult)packet.ReadUInt8();
		friendStatusPkt.Guid = packet.ReadGuid().To128(GetSession().GameState);
		friendStatusPkt.WowAccountGuid = GetSession().GetGameAccountGuidForPlayer(friendStatusPkt.Guid);
		friendStatusPkt.VirtualRealmAddress = GetSession().RealmId.GetAddress();
		switch (friendStatusPkt.FriendResult)
		{
		case FriendsResult.AddedOffline:
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
			{
				friendStatusPkt.Notes = packet.ReadCString();
			}
			break;
		case FriendsResult.AddedOnline:
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
			{
				friendStatusPkt.Notes = packet.ReadCString();
			}
			friendStatusPkt.Status = (FriendStatus)packet.ReadUInt8();
			friendStatusPkt.AreaID = packet.ReadUInt32();
			friendStatusPkt.Level = packet.ReadUInt32();
			friendStatusPkt.ClassID = (Class)packet.ReadUInt32();
			break;
		case FriendsResult.Online:
			friendStatusPkt.Status = (FriendStatus)packet.ReadUInt8();
			friendStatusPkt.AreaID = packet.ReadUInt32();
			friendStatusPkt.Level = packet.ReadUInt32();
			friendStatusPkt.ClassID = (Class)packet.ReadUInt32();
			break;
		}
		SendPacketToClient(friendStatusPkt);
		if (friendStatusPkt.FriendResult == FriendsResult.IgnoreAdded)
		{
			Session.GameState.IgnoredPlayers.Add(friendStatusPkt.Guid);
		}
		else if (friendStatusPkt.FriendResult == FriendsResult.IgnoreRemoved)
		{
			Session.GameState.IgnoredPlayers.Remove(friendStatusPkt.Guid);
		}
	}

	[PacketHandler(Opcode.SMSG_SEND_KNOWN_SPELLS)]
	private void HandleSendKnownSpells(WorldPacket packet)
	{
		SendKnownSpells sendKnownSpells = new SendKnownSpells();
		sendKnownSpells.InitialLogin = packet.ReadBool();
		ushort num = packet.ReadUInt16();
		for (ushort num2 = 0; num2 < num; num2++)
		{
			uint num3 = ((!LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767)) ? packet.ReadUInt16() : packet.ReadUInt32());
			sendKnownSpells.KnownSpells.Add(num3);
			packet.ReadInt16();
		}
		SendPacketToClient(sendKnownSpells);
		ushort num4 = packet.ReadUInt16();
		if (num4 != 0)
		{
			SendSpellHistory sendSpellHistory = new SendSpellHistory();
			for (ushort num5 = 0; num5 < num4; num5++)
			{
				SpellHistoryEntry spellHistoryEntry = new SpellHistoryEntry();
				uint spellID = ((!LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767)) ? packet.ReadUInt16() : packet.ReadUInt32());
				spellHistoryEntry.SpellID = spellID;
				uint itemID = ((!LegacyVersion.AddedInVersion(ClientVersionBuild.V4_2_2_14545)) ? packet.ReadUInt16() : packet.ReadUInt32());
				spellHistoryEntry.ItemID = itemID;
				spellHistoryEntry.Category = packet.ReadUInt16();
				spellHistoryEntry.RecoveryTime = packet.ReadInt32();
				spellHistoryEntry.CategoryRecoveryTime = packet.ReadInt32();
				sendSpellHistory.Entries.Add(spellHistoryEntry);
			}
			SendPacketToClient(sendSpellHistory, Opcode.SMSG_SEND_UNLEARN_SPELLS);
		}
		if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			SendPacketToClient(new SendUnlearnSpells());
			SendPacketToClient(new SendSpellCharges());
		}
	}

	[PacketHandler(Opcode.SMSG_SUPERCEDED_SPELLS)]
	private void HandleSupercededSpells(WorldPacket packet)
	{
		SupercededSpells supercededSpells = new SupercededSpells();
		uint num;
		uint num2;
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			num = packet.ReadUInt32();
			num2 = packet.ReadUInt32();
		}
		else
		{
			num = packet.ReadUInt16();
			num2 = packet.ReadUInt16();
		}
		supercededSpells.SpellID.Add(num2);
		supercededSpells.Superceded.Add(num);
		SendPacketToClient(supercededSpells);
	}

	[PacketHandler(Opcode.SMSG_LEARNED_SPELL)]
	private void HandleLearnedSpell(WorldPacket packet)
	{
		LearnedSpells learnedSpells = new LearnedSpells();
		uint num = packet.ReadUInt32();
		learnedSpells.Spells.Add(num);
		SendPacketToClient(learnedSpells);
	}

	[PacketHandler(Opcode.SMSG_SEND_UNLEARN_SPELLS)]
	private void HandleSendUnlearnSpells(WorldPacket packet)
	{
		SendUnlearnSpells sendUnlearnSpells = new SendUnlearnSpells();
		uint num = packet.ReadUInt32();
		for (uint num2 = 0u; num2 < num; num2++)
		{
			uint num3 = packet.ReadUInt32();
			sendUnlearnSpells.Spells.Add(num3);
		}
		SendPacketToClient(sendUnlearnSpells);
	}

	[PacketHandler(Opcode.SMSG_UNLEARNED_SPELLS)]
	private void HandleUnlearnedSpells(WorldPacket packet)
	{
		UnlearnedSpells unlearnedSpells = new UnlearnedSpells();
		uint num = ((!LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767)) ? packet.ReadUInt16() : packet.ReadUInt32());
		unlearnedSpells.Spells.Add(num);
		SendPacketToClient(unlearnedSpells);
	}

	[PacketHandler(Opcode.SMSG_CAST_FAILED)]
	private void HandleCastFailed(WorldPacket packet)
	{
		if (Settings.ClientSpellDelay > 0)
		{
			Thread.Sleep(Settings.ClientSpellDelay);
		}
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			packet.ReadUInt8();
		}
		uint num = packet.ReadUInt32();
		if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180) && packet.ReadUInt8() != 2)
		{
			return;
		}
		uint result = packet.ReadUInt8();
		if (LegacyVersion.InVersion(ClientVersionBuild.V2_0_1_6180, ClientVersionBuild.V3_0_2_9056))
		{
			packet.ReadUInt8();
		}
		int failedArg = 0;
		int failedArg2 = 0;
		if (packet.CanRead())
		{
			failedArg = packet.ReadInt32();
		}
		if (packet.CanRead())
		{
			failedArg2 = packet.ReadInt32();
		}
		if (GetSession().GameState.CurrentClientSpecialCast != null && GetSession().GameState.CurrentClientSpecialCast.SpellId == num)
		{
			CastFailed castFailed = new CastFailed();
			castFailed.SpellID = GetSession().GameState.CurrentClientSpecialCast.SpellId;
			castFailed.SpellXSpellVisualID = GetSession().GameState.CurrentClientSpecialCast.SpellXSpellVisualId;
			castFailed.Reason = LegacyVersion.ConvertSpellCastResult(result);
			castFailed.CastID = GetSession().GameState.CurrentClientSpecialCast.ServerGUID;
			castFailed.FailedArg1 = failedArg;
			castFailed.FailedArg2 = failedArg2;
			SendPacketToClient(castFailed);
			GetSession().GameState.CurrentClientSpecialCast = null;
		}
		else
		{
			if (GetSession().GameState.CurrentClientNormalCast == null || GetSession().GameState.CurrentClientNormalCast.SpellId != num)
			{
				return;
			}
			if (!GetSession().GameState.CurrentClientNormalCast.HasStarted)
			{
				SpellPrepare spellPrepare = new SpellPrepare();
				spellPrepare.ClientCastID = GetSession().GameState.CurrentClientNormalCast.ClientGUID;
				spellPrepare.ServerCastID = GetSession().GameState.CurrentClientNormalCast.ServerGUID;
				SendPacketToClient(spellPrepare);
			}
			CastFailed castFailed2 = new CastFailed();
			castFailed2.SpellID = GetSession().GameState.CurrentClientNormalCast.SpellId;
			castFailed2.SpellXSpellVisualID = GetSession().GameState.CurrentClientNormalCast.SpellXSpellVisualId;
			castFailed2.Reason = LegacyVersion.ConvertSpellCastResult(result);
			castFailed2.CastID = GetSession().GameState.CurrentClientNormalCast.ServerGUID;
			castFailed2.FailedArg1 = failedArg;
			castFailed2.FailedArg2 = failedArg2;
			SendPacketToClient(castFailed2);
			GetSession().GameState.CurrentClientNormalCast = null;
			foreach (ClientCastRequest pendingClientCast in GetSession().GameState.PendingClientCasts)
			{
				GetSession().InstanceSocket.SendCastRequestFailed(pendingClientCast, isPet: false);
			}
			GetSession().GameState.PendingClientCasts.Clear();
		}
	}

	[PacketHandler(Opcode.SMSG_PET_CAST_FAILED, ClientVersionBuild.Zero, ClientVersionBuild.V2_0_1_6180)]
	private void HandlePetCastFailed(WorldPacket packet)
	{
		if (Settings.ClientSpellDelay > 0)
		{
			Thread.Sleep(Settings.ClientSpellDelay);
		}
		uint num = packet.ReadUInt32();
		if (packet.ReadUInt8() != 2 || GetSession().GameState.CurrentClientPetCast == null || GetSession().GameState.CurrentClientPetCast.SpellId != num)
		{
			return;
		}
		if (!GetSession().GameState.CurrentClientPetCast.HasStarted)
		{
			SpellPrepare spellPrepare = new SpellPrepare();
			spellPrepare.ClientCastID = GetSession().GameState.CurrentClientPetCast.ClientGUID;
			spellPrepare.ServerCastID = GetSession().GameState.CurrentClientPetCast.ServerGUID;
			SendPacketToClient(spellPrepare);
		}
		PetCastFailed petCastFailed = new PetCastFailed();
		petCastFailed.SpellID = num;
		uint result = packet.ReadUInt8();
		petCastFailed.Reason = LegacyVersion.ConvertSpellCastResult(result);
		petCastFailed.CastID = GetSession().GameState.CurrentClientPetCast.ServerGUID;
		SendPacketToClient(petCastFailed);
		foreach (ClientCastRequest pendingClientPetCast in GetSession().GameState.PendingClientPetCasts)
		{
			GetSession().InstanceSocket.SendCastRequestFailed(pendingClientPetCast, isPet: true);
		}
		GetSession().GameState.PendingClientPetCasts.Clear();
	}

	[PacketHandler(Opcode.SMSG_PET_CAST_FAILED, ClientVersionBuild.V2_0_1_6180)]
	private void HandlePetCastFailedTBC(WorldPacket packet)
	{
		if (Settings.ClientSpellDelay > 0)
		{
			Thread.Sleep(Settings.ClientSpellDelay);
		}
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			packet.ReadUInt8();
		}
		uint num = packet.ReadUInt32();
		if (GetSession().GameState.CurrentClientPetCast == null || GetSession().GameState.CurrentClientPetCast.SpellId != num)
		{
			return;
		}
		if (!GetSession().GameState.CurrentClientPetCast.HasStarted)
		{
			SpellPrepare spellPrepare = new SpellPrepare();
			spellPrepare.ClientCastID = GetSession().GameState.CurrentClientPetCast.ClientGUID;
			spellPrepare.ServerCastID = GetSession().GameState.CurrentClientPetCast.ServerGUID;
			SendPacketToClient(spellPrepare);
		}
		PetCastFailed petCastFailed = new PetCastFailed();
		petCastFailed.SpellID = num;
		uint result = packet.ReadUInt8();
		petCastFailed.Reason = LegacyVersion.ConvertSpellCastResult(result);
		petCastFailed.CastID = GetSession().GameState.CurrentClientPetCast.ServerGUID;
		if (packet.CanRead())
		{
			petCastFailed.FailedArg1 = packet.ReadInt32();
		}
		if (packet.CanRead())
		{
			petCastFailed.FailedArg2 = packet.ReadInt32();
		}
		SendPacketToClient(petCastFailed);
		foreach (ClientCastRequest pendingClientPetCast in GetSession().GameState.PendingClientPetCasts)
		{
			GetSession().InstanceSocket.SendCastRequestFailed(pendingClientPetCast, isPet: true);
		}
		GetSession().GameState.PendingClientPetCasts.Clear();
	}

	[PacketHandler(Opcode.SMSG_SPELL_FAILED_OTHER)]
	private void HandleSpellFailedOther(WorldPacket packet)
	{
		WowGuid128 wowGuid = ((!LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180)) ? packet.ReadGuid().To128(GetSession().GameState) : packet.ReadPackedGuid().To128(GetSession().GameState));
		if (wowGuid == GetSession().GameState.CurrentPlayerGuid && Settings.ClientSpellDelay > 0)
		{
			Thread.Sleep(Settings.ClientSpellDelay);
		}
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			packet.ReadUInt8();
		}
		uint num = packet.ReadUInt32();
		byte reason = 61;
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			reason = (byte)LegacyVersion.ConvertSpellCastResult(packet.ReadUInt8());
		}
		WowGuid128 castID;
		uint spellXSpellVisualID;
		if (GetSession().GameState.CurrentPlayerGuid == wowGuid && GetSession().GameState.CurrentClientNormalCast != null && GetSession().GameState.CurrentClientNormalCast.SpellId == num)
		{
			castID = GetSession().GameState.CurrentClientNormalCast.ServerGUID;
			spellXSpellVisualID = GetSession().GameState.CurrentClientNormalCast.SpellXSpellVisualId;
		}
		else if (GetSession().GameState.CurrentPetGuid == wowGuid && GetSession().GameState.CurrentClientPetCast != null && GetSession().GameState.CurrentClientPetCast.SpellId == num)
		{
			castID = GetSession().GameState.CurrentClientPetCast.ServerGUID;
			spellXSpellVisualID = GetSession().GameState.CurrentClientPetCast.SpellXSpellVisualId;
		}
		else
		{
			castID = WowGuid128.Create(HighGuidType703.Cast, SpellCastSource.Normal, GetSession().GameState.CurrentMapId.Value, num, num + wowGuid.GetCounter());
			spellXSpellVisualID = GameData.GetSpellVisual(num);
		}
		SpellFailure spellFailure = new SpellFailure();
		spellFailure.CasterUnit = wowGuid;
		spellFailure.CastID = castID;
		spellFailure.SpellID = num;
		spellFailure.SpellXSpellVisualID = spellXSpellVisualID;
		spellFailure.Reason = reason;
		SendPacketToClient(spellFailure);
		SpellFailedOther spellFailedOther = new SpellFailedOther();
		spellFailedOther.CasterUnit = wowGuid;
		spellFailedOther.CastID = castID;
		spellFailedOther.SpellID = num;
		spellFailedOther.SpellXSpellVisualID = spellXSpellVisualID;
		spellFailedOther.Reason = reason;
		SendPacketToClient(spellFailedOther);
	}

	[PacketHandler(Opcode.SMSG_SPELL_START)]
	private void HandleSpellStart(WorldPacket packet)
	{
		if (!GetSession().GameState.CurrentMapId.HasValue)
		{
			return;
		}
		SpellStart spellStart = new SpellStart();
		spellStart.Cast = HandleSpellStartOrGo(packet, isSpellGo: false);
		byte b = 0;
		if (GetSession().GameState.CurrentPlayerGuid == spellStart.Cast.CasterUnit && GetSession().GameState.CurrentClientNormalCast != null && GetSession().GameState.CurrentClientNormalCast.SpellId == spellStart.Cast.SpellID)
		{
			spellStart.Cast.CastID = GetSession().GameState.CurrentClientNormalCast.ServerGUID;
			spellStart.Cast.SpellXSpellVisualID = GetSession().GameState.CurrentClientNormalCast.SpellXSpellVisualId;
			GetSession().GameState.CurrentClientNormalCast.HasStarted = true;
			SpellPrepare spellPrepare = new SpellPrepare();
			spellPrepare.ClientCastID = GetSession().GameState.CurrentClientNormalCast.ClientGUID;
			spellPrepare.ServerCastID = spellStart.Cast.CastID;
			SendPacketToClient(spellPrepare);
			b = 1;
		}
		else if (GetSession().GameState.CurrentPetGuid == spellStart.Cast.CasterUnit && GetSession().GameState.CurrentClientPetCast != null && GetSession().GameState.CurrentClientPetCast.SpellId == spellStart.Cast.SpellID)
		{
			spellStart.Cast.CastID = GetSession().GameState.CurrentClientPetCast.ServerGUID;
			spellStart.Cast.SpellXSpellVisualID = GetSession().GameState.CurrentClientPetCast.SpellXSpellVisualId;
			GetSession().GameState.CurrentClientPetCast.HasStarted = true;
			SpellPrepare spellPrepare2 = new SpellPrepare();
			spellPrepare2.ClientCastID = GetSession().GameState.CurrentClientPetCast.ClientGUID;
			spellPrepare2.ServerCastID = spellStart.Cast.CastID;
			SendPacketToClient(spellPrepare2);
			b = 2;
		}
		if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180) && GameData.DispellSpells.Contains((uint)spellStart.Cast.SpellID))
		{
			GetSession().GameState.LastDispellSpellId = (uint)spellStart.Cast.SpellID;
		}
		SendPacketToClient(spellStart);
		switch (b)
		{
		case 1:
			foreach (ClientCastRequest pendingClientCast in GetSession().GameState.PendingClientCasts)
			{
				GetSession().InstanceSocket.SendCastRequestFailed(pendingClientCast, isPet: false);
			}
			GetSession().GameState.PendingClientCasts.Clear();
			break;
		case 2:
			foreach (ClientCastRequest pendingClientPetCast in GetSession().GameState.PendingClientPetCasts)
			{
				GetSession().InstanceSocket.SendCastRequestFailed(pendingClientPetCast, isPet: true);
			}
			GetSession().GameState.PendingClientPetCasts.Clear();
			break;
		}
	}

	[PacketHandler(Opcode.SMSG_SPELL_GO)]
	private void HandleSpellGo(WorldPacket packet)
	{
		if (!GetSession().GameState.CurrentMapId.HasValue)
		{
			return;
		}
		SpellGo spellGo = new SpellGo();
		spellGo.Cast = HandleSpellStartOrGo(packet, isSpellGo: true);
		if (GetSession().GameState.CurrentPlayerGuid == spellGo.Cast.CasterUnit && GetSession().GameState.CurrentClientNormalCast != null && GetSession().GameState.CurrentClientNormalCast.SpellId == spellGo.Cast.SpellID)
		{
			spellGo.Cast.CastID = GetSession().GameState.CurrentClientNormalCast.ServerGUID;
			spellGo.Cast.SpellXSpellVisualID = GetSession().GameState.CurrentClientNormalCast.SpellXSpellVisualId;
			GetSession().GameState.CurrentClientNormalCast = null;
		}
		else if (GetSession().GameState.CurrentPlayerGuid == spellGo.Cast.CasterUnit && GetSession().GameState.CurrentClientSpecialCast != null && GetSession().GameState.CurrentClientSpecialCast.SpellId == spellGo.Cast.SpellID)
		{
			spellGo.Cast.CastID = GetSession().GameState.CurrentClientSpecialCast.ServerGUID;
			spellGo.Cast.SpellXSpellVisualID = GetSession().GameState.CurrentClientSpecialCast.SpellXSpellVisualId;
			GetSession().GameState.CurrentClientSpecialCast = null;
		}
		else if (GetSession().GameState.CurrentPetGuid == spellGo.Cast.CasterUnit && GetSession().GameState.CurrentClientPetCast != null && GetSession().GameState.CurrentClientPetCast.SpellId == spellGo.Cast.SpellID)
		{
			spellGo.Cast.CastID = GetSession().GameState.CurrentClientPetCast.ServerGUID;
			spellGo.Cast.SpellXSpellVisualID = GetSession().GameState.CurrentClientPetCast.SpellXSpellVisualId;
			GetSession().GameState.CurrentClientPetCast = null;
		}
		if (!spellGo.Cast.CasterUnit.IsEmpty() && GameData.AuraSpells.Contains((uint)spellGo.Cast.SpellID))
		{
			foreach (WowGuid128 hitTarget in spellGo.Cast.HitTargets)
			{
				GetSession().GameState.StoreLastAuraCasterOnTarget(hitTarget, (uint)spellGo.Cast.SpellID, spellGo.Cast.CasterUnit);
			}
		}
		SendPacketToClient(spellGo);
	}

	private SpellCastData HandleSpellStartOrGo(WorldPacket packet, bool isSpellGo)
	{
		SpellCastData spellCastData = new SpellCastData();
		spellCastData.CasterGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
		spellCastData.CasterUnit = packet.ReadPackedGuid().To128(GetSession().GameState);
		if (spellCastData.CasterUnit == GetSession().GameState.CurrentPlayerGuid && Settings.ClientSpellDelay > 0)
		{
			Thread.Sleep(Settings.ClientSpellDelay);
		}
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			packet.ReadUInt8();
		}
		spellCastData.SpellID = packet.ReadInt32();
		spellCastData.SpellXSpellVisualID = GameData.GetSpellVisual((uint)spellCastData.SpellID);
		spellCastData.CastID = WowGuid128.Create(HighGuidType703.Cast, SpellCastSource.Normal, GetSession().GameState.CurrentMapId.Value, (uint)spellCastData.SpellID, (ulong)spellCastData.SpellID + spellCastData.CasterUnit.GetCounter());
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180) && LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056) && !isSpellGo)
		{
			packet.ReadUInt8();
		}
		uint num = (spellCastData.CastFlags = ((!LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056)) ? packet.ReadUInt16() : packet.ReadUInt32()));
		if (!isSpellGo || LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			spellCastData.CastTime = packet.ReadUInt32();
		}
		if (isSpellGo)
		{
			byte b = packet.ReadUInt8();
			for (int i = 0; i < b; i++)
			{
				WowGuid128 wowGuid = packet.ReadGuid().To128(GetSession().GameState);
				spellCastData.HitTargets.Add(wowGuid);
			}
			byte b2 = packet.ReadUInt8();
			for (int j = 0; j < b2; j++)
			{
				WowGuid128 wowGuid2 = packet.ReadGuid().To128(GetSession().GameState);
				SpellMissInfo spellMissInfo = (SpellMissInfo)packet.ReadUInt8();
				SpellMissInfo reflectStatus = SpellMissInfo.None;
				if (spellMissInfo == SpellMissInfo.Reflect)
				{
					reflectStatus = (SpellMissInfo)packet.ReadUInt8();
				}
				spellCastData.MissTargets.Add(wowGuid2);
				spellCastData.MissStatus.Add(new SpellMissStatus(spellMissInfo, reflectStatus));
			}
		}
		SpellCastTargetFlags spellCastTargetFlags = (SpellCastTargetFlags)(LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180) ? packet.ReadUInt32() : packet.ReadUInt16());
		spellCastData.Target.Flags = spellCastTargetFlags;
		WowGuid128 unit = WowGuid128.Empty;
		if (spellCastTargetFlags.HasAnyFlag(SpellCastTargetFlags.CorpseMask | SpellCastTargetFlags.Unit | SpellCastTargetFlags.GameObject | SpellCastTargetFlags.UnitMinipet))
		{
			unit = packet.ReadPackedGuid().To128(GetSession().GameState);
		}
		spellCastData.Target.Unit = unit;
		WowGuid128 item = WowGuid128.Empty;
		if (spellCastTargetFlags.HasAnyFlag(SpellCastTargetFlags.Item | SpellCastTargetFlags.TradeItem))
		{
			item = packet.ReadPackedGuid().To128(GetSession().GameState);
		}
		spellCastData.Target.Item = item;
		if (spellCastTargetFlags.HasAnyFlag(SpellCastTargetFlags.SourceLocation))
		{
			spellCastData.Target.SrcLocation = new TargetLocation();
			spellCastData.Target.SrcLocation.Transport = WowGuid128.Empty;
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
			{
				spellCastData.Target.SrcLocation.Transport = packet.ReadPackedGuid().To128(GetSession().GameState);
			}
			spellCastData.Target.SrcLocation.Location = packet.ReadVector3();
		}
		if (spellCastTargetFlags.HasAnyFlag(SpellCastTargetFlags.DestLocation))
		{
			spellCastData.Target.DstLocation = new TargetLocation();
			spellCastData.Target.DstLocation.Transport = WowGuid128.Empty;
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_8_9464))
			{
				spellCastData.Target.DstLocation.Transport = packet.ReadPackedGuid().To128(GetSession().GameState);
			}
			spellCastData.Target.DstLocation.Location = packet.ReadVector3();
		}
		if (spellCastTargetFlags.HasAnyFlag(SpellCastTargetFlags.String))
		{
			spellCastData.Target.Name = packet.ReadCString();
		}
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			if (num.HasAnyFlag(CastFlag.PredictedPower))
			{
				packet.ReadInt32();
			}
			if (num.HasAnyFlag(CastFlag.RuneInfo))
			{
				byte b3 = packet.ReadUInt8();
				byte b4 = packet.ReadUInt8();
				for (int k = 0; k < 6; k++)
				{
					int num2 = 1 << k;
					if ((num2 & b3) != 0 && (num2 & b4) == 0)
					{
						packet.ReadUInt8();
					}
				}
			}
			if (isSpellGo && num.HasAnyFlag(CastFlag.AdjustMissile))
			{
				spellCastData.MissileTrajectory.Pitch = packet.ReadFloat();
				spellCastData.MissileTrajectory.TravelTime = packet.ReadUInt32();
			}
		}
		if (num.HasAnyFlag(CastFlag.Projectile))
		{
			spellCastData.AmmoDisplayId = packet.ReadInt32();
			spellCastData.AmmoInventoryType = packet.ReadInt32();
		}
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			if (isSpellGo)
			{
				if (num.HasAnyFlag(CastFlag.VisualChain))
				{
					packet.ReadInt32();
					packet.ReadInt32();
				}
				if (spellCastTargetFlags.HasAnyFlag(SpellCastTargetFlags.DestLocation))
				{
					packet.ReadInt8();
				}
				if (spellCastTargetFlags.HasAnyFlag(SpellCastTargetFlags.ExtraTargets))
				{
					int num3 = packet.ReadInt32();
					if (num3 > 0)
					{
						TargetLocation targetLocation = new TargetLocation();
						for (int l = 0; l < num3; l++)
						{
							targetLocation.Location = packet.ReadVector3();
							targetLocation.Transport = packet.ReadGuid().To128(GetSession().GameState);
						}
						spellCastData.TargetPoints.Add(targetLocation);
					}
				}
			}
			else
			{
				if (num.HasAnyFlag(CastFlag.Immunity))
				{
					spellCastData.Immunities.School = packet.ReadUInt32();
					spellCastData.Immunities.Value = packet.ReadUInt32();
				}
				if (num.HasAnyFlag(CastFlag.HealPrediction))
				{
					packet.ReadInt32();
					if (packet.ReadUInt8() == 2)
					{
						packet.ReadPackedGuid();
					}
				}
			}
		}
		return spellCastData;
	}

	[PacketHandler(Opcode.SMSG_CANCEL_AUTO_REPEAT)]
	private void HandleCancelAutoRepeat(WorldPacket packet)
	{
		if (Settings.ClientSpellDelay > 0)
		{
			Thread.Sleep(Settings.ClientSpellDelay);
		}
		if (GetSession().GameState.CurrentClientSpecialCast != null && GameData.AutoRepeatSpells.Contains(GetSession().GameState.CurrentClientSpecialCast.SpellId))
		{
			GetSession().GameState.CurrentClientSpecialCast = null;
		}
		CancelAutoRepeat cancelAutoRepeat = new CancelAutoRepeat();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			cancelAutoRepeat.Guid = packet.ReadPackedGuid().To128(GetSession().GameState);
		}
		else
		{
			cancelAutoRepeat.Guid = GetSession().GameState.CurrentPlayerGuid;
		}
		SendPacketToClient(cancelAutoRepeat);
	}

	[PacketHandler(Opcode.SMSG_SPELL_COOLDOWN)]
	private void HandleSpellCooldown(WorldPacket packet)
	{
		SpellCooldownPkt spellCooldownPkt = new SpellCooldownPkt();
		try
		{
			spellCooldownPkt.Caster = packet.ReadGuid().To128(GetSession().GameState);
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
			{
				spellCooldownPkt.Flags = packet.ReadUInt8();
			}
			while (packet.CanRead())
			{
				SpellCooldownStruct spellCooldownStruct = new SpellCooldownStruct();
				spellCooldownStruct.SpellID = packet.ReadUInt32();
				spellCooldownStruct.ForcedCooldown = packet.ReadUInt32();
				spellCooldownPkt.SpellCooldowns.Add(spellCooldownStruct);
			}
		}
		catch (ArgumentOutOfRangeException)
		{
			packet.ResetReadPos();
			SpellCooldownStruct spellCooldownStruct2 = new SpellCooldownStruct();
			spellCooldownStruct2.SpellID = packet.ReadUInt32();
			spellCooldownPkt.Caster = packet.ReadPackedGuid().To128(GetSession().GameState);
			spellCooldownStruct2.ForcedCooldown = packet.ReadUInt32();
			spellCooldownPkt.SpellCooldowns.Add(spellCooldownStruct2);
		}
		SendPacketToClient(spellCooldownPkt);
	}

	[PacketHandler(Opcode.SMSG_COOLDOWN_EVENT)]
	private void HandleCooldownEvent(WorldPacket packet)
	{
		CooldownEvent cooldownEvent = new CooldownEvent();
		cooldownEvent.SpellID = packet.ReadUInt32();
		WowGuid wowGuid = packet.ReadGuid();
		cooldownEvent.IsPet = wowGuid.GetHighType() == HighGuidType.Pet;
		SendPacketToClient(cooldownEvent);
	}

	[PacketHandler(Opcode.SMSG_CLEAR_COOLDOWN)]
	private void HandleClearCooldown(WorldPacket packet)
	{
		ClearCooldown clearCooldown = new ClearCooldown();
		clearCooldown.SpellID = packet.ReadUInt32();
		WowGuid wowGuid = packet.ReadGuid();
		clearCooldown.IsPet = wowGuid.GetHighType() == HighGuidType.Pet;
		SendPacketToClient(clearCooldown);
	}

	[PacketHandler(Opcode.SMSG_COOLDOWN_CHEAT)]
	private void HandleCooldownCheat(WorldPacket packet)
	{
		CooldownCheat cooldownCheat = new CooldownCheat();
		cooldownCheat.Guid = packet.ReadGuid().To128(GetSession().GameState);
		SendPacketToClient(cooldownCheat);
	}

	[PacketHandler(Opcode.SMSG_SPELL_NON_MELEE_DAMAGE_LOG)]
	private void HandleSpellNonMeleeDamageLog(WorldPacket packet)
	{
		SpellNonMeleeDamageLog spellNonMeleeDamageLog = new SpellNonMeleeDamageLog();
		spellNonMeleeDamageLog.TargetGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
		spellNonMeleeDamageLog.CasterGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
		spellNonMeleeDamageLog.SpellID = packet.ReadUInt32();
		spellNonMeleeDamageLog.SpellXSpellVisualID = GameData.GetSpellVisual(spellNonMeleeDamageLog.SpellID);
		spellNonMeleeDamageLog.CastID = WowGuid128.Create(HighGuidType703.Cast, SpellCastSource.Normal, GetSession().GameState.CurrentMapId.Value, spellNonMeleeDamageLog.SpellID, spellNonMeleeDamageLog.SpellID + spellNonMeleeDamageLog.CasterGUID.GetCounter());
		spellNonMeleeDamageLog.Damage = packet.ReadInt32();
		spellNonMeleeDamageLog.OriginalDamage = spellNonMeleeDamageLog.Damage;
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_3_9183))
		{
			spellNonMeleeDamageLog.Overkill = packet.ReadInt32();
		}
		else
		{
			spellNonMeleeDamageLog.Overkill = -1;
		}
		byte b = packet.ReadUInt8();
		if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			b = (byte)(1 << (int)b);
		}
		spellNonMeleeDamageLog.SchoolMask = b;
		spellNonMeleeDamageLog.Absorbed = packet.ReadInt32();
		spellNonMeleeDamageLog.Resisted = packet.ReadInt32();
		spellNonMeleeDamageLog.Periodic = packet.ReadBool();
		packet.ReadUInt8();
		spellNonMeleeDamageLog.ShieldBlock = packet.ReadInt32();
		spellNonMeleeDamageLog.Flags = (SpellHitType)packet.ReadUInt32();
		if (packet.ReadBool() && !spellNonMeleeDamageLog.Flags.HasAnyFlag(SpellHitType.Split))
		{
			if (spellNonMeleeDamageLog.Flags.HasAnyFlag(SpellHitType.CritDebug))
			{
				packet.ReadFloat();
				packet.ReadFloat();
			}
			if (spellNonMeleeDamageLog.Flags.HasAnyFlag(SpellHitType.HitDebug))
			{
				packet.ReadFloat();
				packet.ReadFloat();
			}
			if (spellNonMeleeDamageLog.Flags.HasAnyFlag(SpellHitType.AttackTableDebug))
			{
				packet.ReadFloat();
				packet.ReadFloat();
				packet.ReadFloat();
				packet.ReadFloat();
				packet.ReadFloat();
				packet.ReadFloat();
			}
		}
		SendPacketToClient(spellNonMeleeDamageLog);
	}

	[PacketHandler(Opcode.SMSG_SPELL_HEAL_LOG)]
	private void HandleSpellHealLog(WorldPacket packet)
	{
		SpellHealLog spellHealLog = new SpellHealLog();
		spellHealLog.TargetGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
		spellHealLog.CasterGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
		spellHealLog.SpellID = packet.ReadUInt32();
		spellHealLog.HealAmount = packet.ReadInt32();
		spellHealLog.OriginalHealAmount = spellHealLog.HealAmount;
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_3_9183))
		{
			spellHealLog.OverHeal = packet.ReadUInt32();
		}
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
		{
			spellHealLog.Absorbed = packet.ReadUInt32();
		}
		spellHealLog.Crit = packet.ReadBool();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180) && packet.ReadBool())
		{
			spellHealLog.CritRollMade = packet.ReadFloat();
			spellHealLog.CritRollNeeded = packet.ReadFloat();
		}
		SendPacketToClient(spellHealLog);
	}

	[PacketHandler(Opcode.SMSG_SPELL_PERIODIC_AURA_LOG)]
	private void HandleSpellPeriodicAuraLog(WorldPacket packet)
	{
		SpellPeriodicAuraLog spellPeriodicAuraLog = new SpellPeriodicAuraLog();
		spellPeriodicAuraLog.TargetGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
		spellPeriodicAuraLog.CasterGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
		spellPeriodicAuraLog.SpellID = packet.ReadUInt32();
		int num = packet.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			AuraType auraType = (AuraType)packet.ReadUInt32();
			switch (auraType)
			{
			case AuraType.PeriodicDamage:
			case AuraType.PeriodicDamagePercent:
			{
				SpellPeriodicAuraLog.SpellLogEffect spellLogEffect4 = new SpellPeriodicAuraLog.SpellLogEffect();
				spellLogEffect4.Effect = (uint)auraType;
				spellLogEffect4.Amount = packet.ReadInt32();
				spellLogEffect4.OriginalDamage = spellLogEffect4.Amount;
				if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
				{
					spellLogEffect4.OverHealOrKill = packet.ReadUInt32();
				}
				uint num2 = packet.ReadUInt32();
				if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
				{
					num2 = (uint)(1 << (int)(byte)num2);
				}
				spellLogEffect4.SchoolMaskOrPower = num2;
				spellLogEffect4.AbsorbedOrAmplitude = packet.ReadUInt32();
				spellLogEffect4.Resisted = packet.ReadUInt32();
				if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_2_9901))
				{
					spellLogEffect4.Crit = packet.ReadBool();
				}
				spellPeriodicAuraLog.Effects.Add(spellLogEffect4);
				break;
			}
			case AuraType.PeriodicHeal:
			case AuraType.ObsModHealth:
			{
				SpellPeriodicAuraLog.SpellLogEffect spellLogEffect3 = new SpellPeriodicAuraLog.SpellLogEffect();
				spellLogEffect3.Effect = (uint)auraType;
				spellLogEffect3.Amount = packet.ReadInt32();
				spellLogEffect3.OriginalDamage = spellLogEffect3.Amount;
				if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
				{
					spellLogEffect3.OverHealOrKill = packet.ReadUInt32();
				}
				if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
				{
					spellLogEffect3.AbsorbedOrAmplitude = packet.ReadUInt32();
				}
				if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_2_9901))
				{
					spellLogEffect3.Crit = packet.ReadBool();
				}
				spellPeriodicAuraLog.Effects.Add(spellLogEffect3);
				break;
			}
			case AuraType.ObsModPower:
			case AuraType.PeriodicEnergize:
			{
				SpellPeriodicAuraLog.SpellLogEffect spellLogEffect2 = new SpellPeriodicAuraLog.SpellLogEffect();
				spellLogEffect2.Effect = (uint)auraType;
				spellLogEffect2.SchoolMaskOrPower = packet.ReadUInt32();
				spellLogEffect2.Amount = packet.ReadInt32();
				spellPeriodicAuraLog.Effects.Add(spellLogEffect2);
				break;
			}
			case AuraType.PeriodicManaLeech:
			{
				SpellPeriodicAuraLog.SpellLogEffect spellLogEffect = new SpellPeriodicAuraLog.SpellLogEffect();
				spellLogEffect.Effect = (uint)auraType;
				spellLogEffect.SchoolMaskOrPower = packet.ReadUInt32();
				spellLogEffect.Amount = packet.ReadInt32();
				packet.ReadFloat();
				spellPeriodicAuraLog.Effects.Add(spellLogEffect);
				break;
			}
			}
		}
		SendPacketToClient(spellPeriodicAuraLog);
	}

	[PacketHandler(Opcode.SMSG_SPELL_ENERGIZE_LOG)]
	private void HandleSpellEnergizeLog(WorldPacket packet)
	{
		SpellEnergizeLog spellEnergizeLog = new SpellEnergizeLog();
		spellEnergizeLog.TargetGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
		spellEnergizeLog.CasterGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
		spellEnergizeLog.SpellID = packet.ReadUInt32();
		spellEnergizeLog.Type = (PowerType)packet.ReadUInt32();
		spellEnergizeLog.Amount = packet.ReadInt32();
		SendPacketToClient(spellEnergizeLog);
	}

	[PacketHandler(Opcode.SMSG_SPELL_DELAYED)]
	private void HandleSpellDelayed(WorldPacket packet)
	{
		SpellDelayed spellDelayed = new SpellDelayed();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			spellDelayed.CasterGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
		}
		else
		{
			spellDelayed.CasterGUID = packet.ReadGuid().To128(GetSession().GameState);
		}
		spellDelayed.Delay = packet.ReadInt32();
		SendPacketToClient(spellDelayed);
	}

	[PacketHandler(Opcode.MSG_CHANNEL_START)]
	private void HandleSpellChannelStart(WorldPacket packet)
	{
		SpellChannelStart spellChannelStart = new SpellChannelStart();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			spellChannelStart.CasterGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
		}
		else
		{
			spellChannelStart.CasterGUID = GetSession().GameState.CurrentPlayerGuid;
		}
		spellChannelStart.SpellID = packet.ReadUInt32();
		spellChannelStart.SpellXSpellVisualID = GameData.GetSpellVisual(spellChannelStart.SpellID);
		spellChannelStart.Duration = packet.ReadUInt32();
		SendPacketToClient(spellChannelStart);
	}

	[PacketHandler(Opcode.MSG_CHANNEL_UPDATE)]
	private void HandleSpellChannelUpdate(WorldPacket packet)
	{
		SpellChannelUpdate spellChannelUpdate = new SpellChannelUpdate();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			spellChannelUpdate.CasterGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
		}
		else
		{
			spellChannelUpdate.CasterGUID = GetSession().GameState.CurrentPlayerGuid;
		}
		spellChannelUpdate.TimeRemaining = packet.ReadInt32();
		SendPacketToClient(spellChannelUpdate);
	}

	[PacketHandler(Opcode.SMSG_SPELL_DAMAGE_SHIELD)]
	private void HandleSpellDamageShield(WorldPacket packet)
	{
		SpellDamageShield spellDamageShield = new SpellDamageShield();
		spellDamageShield.VictimGUID = packet.ReadGuid().To128(GetSession().GameState);
		spellDamageShield.CasterGUID = packet.ReadGuid().To128(GetSession().GameState);
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			spellDamageShield.SpellID = packet.ReadUInt32();
		}
		else
		{
			spellDamageShield.SpellID = 7294u;
		}
		spellDamageShield.Damage = packet.ReadInt32();
		spellDamageShield.OriginalDamage = spellDamageShield.Damage;
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			spellDamageShield.OverKill = packet.ReadUInt32();
		}
		uint num = packet.ReadUInt32();
		if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			num = (uint)(1 << (int)(byte)num);
		}
		spellDamageShield.SchoolMask = num;
		SendPacketToClient(spellDamageShield);
	}

	[PacketHandler(Opcode.SMSG_ENVIRONMENTAL_DAMAGE_LOG)]
	private void HandleEnvironmentalDamageLog(WorldPacket packet)
	{
		EnvironmentalDamageLog environmentalDamageLog = new EnvironmentalDamageLog();
		environmentalDamageLog.Victim = packet.ReadGuid().To128(GetSession().GameState);
		environmentalDamageLog.Type = (EnvironmentalDamage)packet.ReadUInt8();
		environmentalDamageLog.Amount = packet.ReadInt32();
		environmentalDamageLog.Absorbed = packet.ReadInt32();
		environmentalDamageLog.Resisted = packet.ReadInt32();
		SendPacketToClient(environmentalDamageLog);
	}

	[PacketHandler(Opcode.SMSG_SPELL_INSTAKILL_LOG)]
	private void HandleSpellInstakillLog(WorldPacket packet)
	{
		SpellInstakillLog spellInstakillLog = new SpellInstakillLog();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			spellInstakillLog.CasterGUID = packet.ReadGuid().To128(GetSession().GameState);
			spellInstakillLog.TargetGUID = packet.ReadGuid().To128(GetSession().GameState);
		}
		else
		{
			spellInstakillLog.CasterGUID = (spellInstakillLog.TargetGUID = packet.ReadGuid().To128(GetSession().GameState));
		}
		spellInstakillLog.SpellID = packet.ReadUInt32();
		SendPacketToClient(spellInstakillLog);
	}

	[PacketHandler(Opcode.SMSG_SPELL_DISPELL_LOG)]
	private void HandleSpellDispellLog(WorldPacket packet)
	{
		SpellDispellLog spellDispellLog = new SpellDispellLog();
		spellDispellLog.TargetGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
		spellDispellLog.CasterGUID = packet.ReadPackedGuid().To128(GetSession().GameState);
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			spellDispellLog.DispelledBySpellID = packet.ReadUInt32();
		}
		else
		{
			spellDispellLog.DispelledBySpellID = GetSession().GameState.LastDispellSpellId;
		}
		bool flag = LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180) && packet.ReadBool();
		int num = packet.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			SpellDispellData spellDispellData = default(SpellDispellData);
			spellDispellData.SpellID = packet.ReadUInt32();
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
			{
				spellDispellData.Harmful = packet.ReadBool();
			}
			spellDispellLog.DispellData.Add(spellDispellData);
		}
		if (flag)
		{
			packet.ReadInt32();
			packet.ReadInt32();
		}
		SendPacketToClient(spellDispellLog);
	}

	[PacketHandler(Opcode.SMSG_PLAY_SPELL_VISUAL)]
	private void HandlePlaySpellVisualKit(WorldPacket packet)
	{
		PlaySpellVisualKit playSpellVisualKit = new PlaySpellVisualKit();
		playSpellVisualKit.Unit = packet.ReadGuid().To128(GetSession().GameState);
		playSpellVisualKit.KitRecID = packet.ReadUInt32();
		SendPacketToClient(playSpellVisualKit);
	}

	[PacketHandler(Opcode.SMSG_UPDATE_AURA_DURATION)]
	private void HandleUpdateAuraDuration(WorldPacket packet)
	{
		byte b = packet.ReadUInt8();
		int num = packet.ReadInt32();
		WowGuid128 currentPlayerGuid = GetSession().GameState.CurrentPlayerGuid;
		if (currentPlayerGuid == null)
		{
			return;
		}
		GetSession().GameState.StoreAuraDurationLeft(currentPlayerGuid, b, num, (int)packet.GetReceivedTime());
		if (num <= 0)
		{
			return;
		}
		Dictionary<int, UpdateField> cachedObjectFieldsLegacy = GetSession().GameState.GetCachedObjectFieldsLegacy(currentPlayerGuid);
		if (cachedObjectFieldsLegacy != null)
		{
			AuraInfo auraInfo = default(AuraInfo);
			auraInfo.Slot = b;
			auraInfo.AuraData = ReadAuraSlot(b, currentPlayerGuid, cachedObjectFieldsLegacy);
			if (auraInfo.AuraData != null)
			{
				auraInfo.AuraData.Flags |= AuraFlagsModern.Duration;
				auraInfo.AuraData.Duration = num;
				auraInfo.AuraData.Remaining = num;
				AuraUpdate auraUpdate = new AuraUpdate(currentPlayerGuid, all: false);
				auraUpdate.Auras.Add(auraInfo);
				SendPacketToClient(auraUpdate);
			}
		}
	}

	[PacketHandler(Opcode.SMSG_SET_EXTRA_AURA_INFO)]
	[PacketHandler(Opcode.SMSG_SET_EXTRA_AURA_INFO_NEED_UPDATE)]
	private void HandleSetExtraAuraInfo(WorldPacket packet)
	{
		WowGuid128 wowGuid = packet.ReadPackedGuid().To128(GetSession().GameState);
		if (!packet.CanRead())
		{
			return;
		}
		byte b = packet.ReadUInt8();
		uint num = packet.ReadUInt32();
		int num2 = packet.ReadInt32();
		int num3 = packet.ReadInt32();
		GetSession().GameState.StoreAuraDurationFull(wowGuid, b, num2);
		GetSession().GameState.StoreAuraDurationLeft(wowGuid, b, num3, (int)packet.GetReceivedTime());
		if (packet.GetUniversalOpcode(isModern: false) == Opcode.SMSG_SET_EXTRA_AURA_INFO_NEED_UPDATE)
		{
			GetSession().GameState.StoreAuraCaster(wowGuid, b, GetSession().GameState.CurrentPlayerGuid);
		}
		if (num2 <= 0 && num3 <= 0)
		{
			return;
		}
		Dictionary<int, UpdateField> cachedObjectFieldsLegacy = GetSession().GameState.GetCachedObjectFieldsLegacy(wowGuid);
		if (cachedObjectFieldsLegacy != null)
		{
			AuraInfo auraInfo = default(AuraInfo);
			auraInfo.Slot = b;
			auraInfo.AuraData = ReadAuraSlot(b, wowGuid, cachedObjectFieldsLegacy);
			if (auraInfo.AuraData != null && auraInfo.AuraData.SpellID == num)
			{
				auraInfo.AuraData.CastUnit = GetSession().GameState.GetAuraCaster(wowGuid, b, num);
				auraInfo.AuraData.Flags |= AuraFlagsModern.Duration;
				auraInfo.AuraData.Duration = num2;
				auraInfo.AuraData.Remaining = num3;
				AuraUpdate auraUpdate = new AuraUpdate(wowGuid, all: false);
				auraUpdate.Auras.Add(auraInfo);
				SendPacketToClient(auraUpdate);
			}
		}
	}

	[PacketHandler(Opcode.SMSG_RESURRECT_REQUEST)]
	private void HandleResurrectRequest(WorldPacket packet)
	{
		ResurrectRequest resurrectRequest = new ResurrectRequest();
		resurrectRequest.CasterGUID = packet.ReadGuid().To128(GetSession().GameState);
		resurrectRequest.CasterVirtualRealmAddress = GetSession().RealmId.GetAddress();
		packet.ReadUInt32();
		resurrectRequest.Name = packet.ReadCString();
		resurrectRequest.Sickness = packet.ReadBool();
		resurrectRequest.UseTimer = packet.ReadBool();
		SendPacketToClient(resurrectRequest);
	}

	[PacketHandler(Opcode.SMSG_TOTEM_CREATED)]
	private void HandleTotemCreated(WorldPacket packet)
	{
		TotemCreated totemCreated = new TotemCreated();
		totemCreated.Slot = packet.ReadUInt8();
		totemCreated.Totem = packet.ReadGuid().To128(GetSession().GameState);
		totemCreated.Duration = packet.ReadUInt32();
		totemCreated.SpellId = packet.ReadUInt32();
		SendPacketToClient(totemCreated);
	}

	[PacketHandler(Opcode.SMSG_SET_FLAT_SPELL_MODIFIER)]
	[PacketHandler(Opcode.SMSG_SET_PCT_SPELL_MODIFIER)]
	private void HandleSetSpellModifier(WorldPacket packet)
	{
		byte b = packet.ReadUInt8();
		byte b2 = packet.ReadUInt8();
		int num = packet.ReadInt32();
		if (GetSession().GameState.CurrentPlayerCreateTime != 0L)
		{
			SetSpellModifier setSpellModifier = new SetSpellModifier(packet.GetUniversalOpcode(isModern: false));
			SpellModifierInfo spellModifierInfo = new SpellModifierInfo();
			SpellModifierData spellModifierData = default(SpellModifierData);
			spellModifierData.ClassIndex = b;
			spellModifierInfo.ModIndex = b2;
			spellModifierData.ModifierValue = num;
			spellModifierInfo.ModifierData.Add(spellModifierData);
			setSpellModifier.Modifiers.Add(spellModifierInfo);
			SendPacketToClient(setSpellModifier);
		}
		if (packet.GetUniversalOpcode(isModern: false) == Opcode.SMSG_SET_FLAT_SPELL_MODIFIER)
		{
			GetSession().GameState.SetFlatSpellMod(b2, b, num);
		}
		else
		{
			GetSession().GameState.SetPctSpellMod(b2, b, num);
		}
	}

	[PacketHandler(Opcode.SMSG_GM_TICKET_CREATE)]
	private void HandleGmTicketCreate(WorldPacket packet)
	{
		LegacyGmTicketResponse legacyGmTicketResponse = (LegacyGmTicketResponse)packet.ReadUInt32();
		bool isError = legacyGmTicketResponse != LegacyGmTicketResponse.CreateSuccess && legacyGmTicketResponse != LegacyGmTicketResponse.UpdateSuccess;
		Session.SendHermesTextMessage($"GM Ticket Status: {legacyGmTicketResponse}", isError);
	}

	[PacketHandler(Opcode.SMSG_FEATURE_SYSTEM_STATUS)]
	private void HandleFeatureSystemStatus(WorldPacket packet)
	{
		GetSession().RealmSocket.SendFeatureSystemStatus();
	}

	[PacketHandler(Opcode.SMSG_MOTD)]
	private void HandleMotd(WorldPacket packet)
	{
		MOTD mOTD = new MOTD();
		uint num = packet.ReadUInt32();
		for (uint num2 = 0u; num2 < num; num2++)
		{
			mOTD.Text.Add(packet.ReadCString());
		}
		SendPacketToClient(mOTD);
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			GetSession().RealmSocket.SendSetTimeZoneInformation();
			GetSession().RealmSocket.SendSeasonInfo();
		}
	}

	[PacketHandler(Opcode.SMSG_TAXI_NODE_STATUS)]
	private void HandleTaxiNodeStatus(WorldPacket packet)
	{
		TaxiNodeStatusPkt taxiNodeStatusPkt = new TaxiNodeStatusPkt();
		taxiNodeStatusPkt.FlightMaster = packet.ReadGuid().To128(GetSession().GameState);
		bool flag = packet.ReadBool();
		taxiNodeStatusPkt.Status = (flag ? TaxiNodeStatus.Learned : TaxiNodeStatus.Unlearned);
		SendPacketToClient(taxiNodeStatusPkt);
	}

	[PacketHandler(Opcode.SMSG_SHOW_TAXI_NODES)]
	private void HandleShowTaxiNodes(WorldPacket packet)
	{
		if (GetSession().GameState.GetLegacyFieldValueUInt32(GetSession().GameState.CurrentPlayerGuid, PlayerField.PLAYER_FLAGS).HasAnyFlag(PlayerFlags.GM))
		{
			ChatPkt packet2 = new ChatPkt(GetSession(), ChatMessageTypeModern.System, "Disable GM mode before talking to taxi master or your game will freeze.");
			SendPacketToClient(packet2);
			return;
		}
		ShowTaxiNodes showTaxiNodes = new ShowTaxiNodes();
		if (packet.ReadUInt32() != 0)
		{
			showTaxiNodes.WindowInfo = new ShowTaxiNodesWindowInfo();
			showTaxiNodes.WindowInfo.UnitGUID = packet.ReadGuid().To128(GetSession().GameState);
			showTaxiNodes.WindowInfo.CurrentNode = (GetSession().GameState.CurrentTaxiNode = packet.ReadUInt32());
		}
		while (packet.CanRead())
		{
			byte b = packet.ReadUInt8();
			showTaxiNodes.CanLandNodes.Add(b);
			showTaxiNodes.CanUseNodes.Add(b);
		}
		GetSession().GameState.UsableTaxiNodes = showTaxiNodes.CanUseNodes;
		SendPacketToClient(showTaxiNodes);
	}

	[PacketHandler(Opcode.SMSG_NEW_TAXI_PATH)]
	private void HandleNewTaxiPath(WorldPacket packet)
	{
		NewTaxiPath packet2 = new NewTaxiPath();
		SendPacketToClient(packet2);
	}

	[PacketHandler(Opcode.SMSG_ACTIVATE_TAXI_REPLY)]
	private void HandleActivateTaxiReply(WorldPacket packet)
	{
		ActivateTaxiReply activateTaxiReply = (ActivateTaxiReply)packet.ReadUInt32();
		if (activateTaxiReply != 0)
		{
			ActivateTaxiReplyPkt activateTaxiReplyPkt = new ActivateTaxiReplyPkt();
			activateTaxiReplyPkt.Reply = activateTaxiReply;
			SendPacketToClient(activateTaxiReplyPkt);
			GetSession().GameState.IsWaitingForTaxiStart = false;
		}
	}

	[PacketHandler(Opcode.SMSG_TRADE_STATUS)]
	private void HandleTradeStatus(WorldPacket packet)
	{
		TradeStatusPkt tradeStatusPkt = new TradeStatusPkt();
		tradeStatusPkt.Status = (TradeStatus)packet.ReadUInt32();
		TradeSession tradeSession = GetSession().GameState.CurrentTrade;
		TradeStatus status;
		if (tradeSession == null)
		{
			status = tradeStatusPkt.Status;
			if ((uint)(status - 1) > 1u)
			{
				Log.Print(LogType.Error, $"Got SMSG_TRADE_STATUS without trade session (status: {tradeStatusPkt.Status})", "HandleTradeStatus", "D:\\a\\HermesProxy\\HermesProxy\\World\\Client\\PacketHandlers\\TradeHandler.cs");
				SendPacketToClient(new TradeStatusPkt
				{
					Status = TradeStatus.Cancelled
				});
				return;
			}
			tradeSession = new TradeSession();
			GetSession().GameState.CurrentTrade = tradeSession;
		}
		switch (tradeStatusPkt.Status)
		{
		case TradeStatus.Proposed:
			tradeStatusPkt.Partner = (tradeSession.Partner = packet.ReadGuid().To128(GetSession().GameState));
			tradeStatusPkt.PartnerAccount = (tradeSession.PartnerAccount = GetSession().GetGameAccountGuidForPlayer(tradeStatusPkt.Partner));
			break;
		case TradeStatus.Initiated:
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
			{
				tradeStatusPkt.Id = packet.ReadUInt32();
			}
			else
			{
				tradeStatusPkt.Id = TradeSession.GlobalTradeIdCounter++;
			}
			tradeSession.TradeId = tradeStatusPkt.Id;
			break;
		case TradeStatus.Failed:
			tradeStatusPkt.BagResult = LegacyVersion.ConvertInventoryResult(packet.ReadUInt32());
			tradeStatusPkt.FailureForYou = packet.ReadBool();
			tradeStatusPkt.ItemID = packet.ReadUInt32();
			break;
		case TradeStatus.WrongRealm:
		case TradeStatus.NotOnTaplist:
			tradeStatusPkt.TradeSlot = packet.ReadUInt8();
			break;
		}
		status = tradeStatusPkt.Status;
		if (status != TradeStatus.Proposed && status != TradeStatus.Initiated && status != TradeStatus.Accepted && status != TradeStatus.Unaccepted && status != TradeStatus.StateChanged && status != TradeStatus.WrongRealm)
		{
			GetSession().GameState.CurrentTrade = null;
		}
		SendPacketToClient(tradeStatusPkt);
	}

	[PacketHandler(Opcode.SMSG_TRADE_STATUS_EXTENDED)]
	private void HandleTradeStatusExtended(WorldPacket packet)
	{
		TradeSession currentTrade = GetSession().GameState.CurrentTrade;
		if (currentTrade == null)
		{
			Log.Print(LogType.Error, "Got SMSG_TRADE_STATUS_EXTENDED without trade session", "HandleTradeStatusExtended", "D:\\a\\HermesProxy\\HermesProxy\\World\\Client\\PacketHandlers\\TradeHandler.cs");
			return;
		}
		currentTrade.ServerStateIndex++;
		TradeUpdated tradeUpdated = new TradeUpdated();
		tradeUpdated.WhichPlayer = packet.ReadUInt8();
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			uint num = packet.ReadUInt32();
			if (num != tradeUpdated.Id)
			{
				Log.Print(LogType.Error, $"Got SMSG_TRADE_STATUS_EXTENDED with wrong tradeId (expected {tradeUpdated.Id} but got {num})", "HandleTradeStatusExtended", "D:\\a\\HermesProxy\\HermesProxy\\World\\Client\\PacketHandlers\\TradeHandler.cs");
				return;
			}
		}
		tradeUpdated.Id = currentTrade.TradeId;
		packet.ReadUInt32();
		packet.ReadUInt32();
		tradeUpdated.ClientStateIndex = currentTrade.ClientStateIndex;
		tradeUpdated.CurrentStateIndex = currentTrade.ServerStateIndex;
		tradeUpdated.Gold = packet.ReadUInt32();
		tradeUpdated.ProposedEnchantment = packet.ReadInt32();
		while (packet.CanRead())
		{
			TradeUpdated.TradeItem tradeItem = new TradeUpdated.TradeItem();
			tradeItem.Unwrapped = new TradeUpdated.UnwrappedTradeItem();
			tradeItem.Slot = packet.ReadUInt8();
			tradeItem.Item.ItemID = packet.ReadUInt32();
			packet.ReadUInt32();
			tradeItem.StackCount = packet.ReadInt32();
			packet.ReadUInt32();
			tradeItem.GiftCreator = packet.ReadGuid().To128(GetSession().GameState);
			tradeItem.Unwrapped.EnchantID = packet.ReadInt32();
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
			{
				for (int i = 0; i < 3; i++)
				{
					packet.ReadUInt32();
				}
			}
			tradeItem.Unwrapped.Creator = packet.ReadGuid().To128(GetSession().GameState);
			tradeItem.Unwrapped.Charges = packet.ReadInt32();
			tradeItem.Item.RandomPropertiesSeed = packet.ReadUInt32();
			tradeItem.Item.RandomPropertiesID = packet.ReadUInt32();
			tradeItem.Unwrapped.Lock = packet.ReadUInt32() != 0;
			tradeItem.Unwrapped.MaxDurability = packet.ReadUInt32();
			tradeItem.Unwrapped.Durability = packet.ReadUInt32();
			tradeUpdated.Items.Add(tradeItem);
		}
		SendPacketToClient(tradeUpdated);
	}

	[PacketHandler(Opcode.SMSG_DESTROY_OBJECT)]
	private void HandleDestroyObject(WorldPacket packet)
	{
		WowGuid128 wowGuid = packet.ReadGuid().To128(GetSession().GameState);
		GetSession().GameState.ObjectCacheMutex.WaitOne();
		GetSession().GameState.ObjectCacheLegacy.Remove(wowGuid);
		GetSession().GameState.ObjectCacheModern.Remove(wowGuid);
		GetSession().GameState.ObjectCacheMutex.ReleaseMutex();
		GetSession().GameState.LastAuraCasterOnTarget.Remove(wowGuid);
		UpdateObject updateObject = new UpdateObject(GetSession().GameState);
		updateObject.DestroyedGuids.Add(wowGuid);
		SendPacketToClient(updateObject);
	}

    //处理压缩更新对象
    [PacketHandler(Opcode.SMSG_COMPRESSED_UPDATE_OBJECT)]
	private void HandleCompressedUpdateObject(WorldPacket packet)
	{
		using WorldPacket packet2 = packet.Inflate(packet.ReadInt32());
		HandleUpdateObject(packet2);
	}

	// 处理更新对象
	[PacketHandler(Opcode.SMSG_UPDATE_OBJECT)]
	private void HandleUpdateObject(WorldPacket packet)
	{
		uint num = packet.ReadUInt32();
		PrintString($"Updates Count = {num}");
		if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
		{
			packet.ReadBool();
		}
		HashSet<uint> hashSet = new HashSet<uint>();
		List<AuraUpdate> list = new List<AuraUpdate>();
		UpdateObject updateObject = new UpdateObject(GetSession().GameState);
		for (int i = 0; i < num; i++)
		{
			UpdateTypeLegacy updateTypeLegacy = (UpdateTypeLegacy)packet.ReadUInt8();
			PrintString($"Update Type = {updateTypeLegacy}", i);
			switch (updateTypeLegacy)
			{
			case UpdateTypeLegacy.Values:
			{
				WowGuid128 wowGuid4 = packet.ReadPackedGuid().To128(GetSession().GameState);
				PrintString("Guid = " + wowGuid4.ToString(), i);
				ObjectUpdate objectUpdate2 = new ObjectUpdate(wowGuid4, UpdateTypeModern.Values, GetSession());
				AuraUpdate auraUpdate2 = new AuraUpdate(wowGuid4, all: false);
				PowerUpdate powerUpdate = new PowerUpdate(wowGuid4);
				ReadValuesUpdateBlock(packet, wowGuid4, objectUpdate2, auraUpdate2, powerUpdate, i);
				if (powerUpdate.Powers.Count != 0)
				{
					SendPacketToClient(powerUpdate);
				}
				updateObject.ObjectUpdates.Add(objectUpdate2);
				if (auraUpdate2.Auras.Count != 0)
				{
					list.Add(auraUpdate2);
				}
				break;
			}
			case UpdateTypeLegacy.Movement:
			{
				WowGuid64 wowGuid3 = (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_2_9901) ? packet.ReadPackedGuid() : packet.ReadGuid());
				PrintString("Guid = " + wowGuid3.ToString(), i);
				ReadMovementUpdateBlock(packet, wowGuid3, null, i);
				break;
			}
			case UpdateTypeLegacy.CreateObject1:
			{
				WowGuid64 wowGuid5 = packet.ReadPackedGuid();
				if (wowGuid5.GetHighType() == HighGuidType.Creature || wowGuid5.GetHighType() == HighGuidType.GameObject)
				{
					if (!GetSession().GameState.ObjectSpawnCount.ContainsKey(wowGuid5))
					{
						GetSession().GameState.ObjectSpawnCount.Add(wowGuid5, 0);
					}
					else if (wowGuid5.GetHighType() == HighGuidType.GameObject && GetSession().GameState.DespawnedGameObjects.Contains(wowGuid5))
					{
						GetSession().GameState.IncrementObjectSpawnCounter(wowGuid5);
					}
				}
				WowGuid128 wowGuid6 = wowGuid5.To128(GetSession().GameState);
				PrintString("Guid = " + wowGuid6.ToString(), i);
				if (wowGuid6 == GetSession().GameState.CurrentPlayerGuid && GetSession().GameState.IsInFarSight)
				{
					UpdateObject updateObject2 = new UpdateObject(GetSession().GameState);
					ObjectUpdate objectUpdate3 = new ObjectUpdate(wowGuid6, UpdateTypeModern.Values, GetSession());
					objectUpdate3.ActivePlayerData.FarsightObject = WowGuid128.Empty;
					updateObject2.ObjectUpdates.Add(objectUpdate3);
					SendPacketToClient(updateObject2);
				}
				ObjectUpdate objectUpdate4 = new ObjectUpdate(wowGuid6, UpdateTypeModern.CreateObject1, GetSession());
				AuraUpdate auraUpdate3 = new AuraUpdate(wowGuid6, all: true);
				ReadCreateObjectBlock(packet, wowGuid6, objectUpdate4, auraUpdate3, i);
				if (objectUpdate4.Guid == GetSession().GameState.CurrentPlayerGuid)
				{
					GetSession().GameState.CurrentPlayerStorage.CompletedQuests.WriteAllCompletedIntoArray(objectUpdate4.ActivePlayerData.QuestCompleted);
				}
				if (wowGuid6.IsItem() && objectUpdate4.ObjectData.EntryID.HasValue && !GameData.ItemTemplates.ContainsKey((uint)objectUpdate4.ObjectData.EntryID.Value))
				{
					hashSet.Add((uint)objectUpdate4.ObjectData.EntryID.Value);
				}
				if (objectUpdate4.CreateData.MoveInfo != null || !wowGuid6.IsWorldObject())
				{
					updateObject.ObjectUpdates.Add(objectUpdate4);
					if (auraUpdate3.Auras.Count != 0)
					{
						list.Add(auraUpdate3);
					}
				}
				else
				{
					Log.Print(LogType.Error, $"Broken create1 without position for {wowGuid6}", "HandleUpdateObject", "D:\\a\\HermesProxy\\HermesProxy\\World\\Client\\PacketHandlers\\UpdateHandler.cs");
				}
				break;
			}
			case UpdateTypeLegacy.CreateObject2:
			{
				WowGuid64 wowGuid = packet.ReadPackedGuid();
				if (wowGuid.GetHighType() == HighGuidType.Creature || wowGuid.GetHighType() == HighGuidType.GameObject)
				{
					GetSession().GameState.IncrementObjectSpawnCounter(wowGuid);
				}
				WowGuid128 wowGuid2 = wowGuid.To128(GetSession().GameState);
				PrintString("Guid = " + wowGuid2.ToString(), i);
				ObjectUpdate objectUpdate = new ObjectUpdate(wowGuid2, UpdateTypeModern.CreateObject2, GetSession());
				AuraUpdate auraUpdate = new AuraUpdate(wowGuid2, all: true);
				ReadCreateObjectBlock(packet, wowGuid2, objectUpdate, auraUpdate, i);
				if (wowGuid2.IsItem() && objectUpdate.ObjectData.EntryID.HasValue && !GameData.ItemTemplates.ContainsKey((uint)objectUpdate.ObjectData.EntryID.Value))
				{
					hashSet.Add((uint)objectUpdate.ObjectData.EntryID.Value);
				}
				if (objectUpdate.CreateData.MoveInfo != null || !wowGuid2.IsWorldObject())
				{
					updateObject.ObjectUpdates.Add(objectUpdate);
					if (auraUpdate.Auras.Count != 0)
					{
						list.Add(auraUpdate);
					}
				}
				else
				{
					Log.Print(LogType.Error, $"Broken create2 without position for {wowGuid2}", "HandleUpdateObject", "D:\\a\\HermesProxy\\HermesProxy\\World\\Client\\PacketHandlers\\UpdateHandler.cs");
				}
				break;
			}
			case UpdateTypeLegacy.NearObjects:
				ReadNearObjectsBlock(packet, i);
				break;
			case UpdateTypeLegacy.FarObjects:
				ReadFarObjectsBlock(packet, updateObject, i);
				break;
			}
		}
		if (updateObject.ObjectUpdates.Count == 0 && GetSession().GameState.IsWaitingForNewWorld)
		{
			return;
		}
		foreach (uint item in hashSet)
		{
			WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_ITEM_QUERY_SINGLE);
			worldPacket.WriteUInt32(item);
			if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
			{
				worldPacket.WriteGuid(WowGuid64.Empty);
			}
			SendPacketToServer(worldPacket);
		}
		int num2 = -1;
		for (int j = 0; j < updateObject.ObjectUpdates.Count; j++)
		{
			if (updateObject.ObjectUpdates[j].CreateData != null && updateObject.ObjectUpdates[j].CreateData.ThisIsYou)
			{
				num2 = j;
				break;
			}
		}
		if (num2 >= 0)
		{
			if (GetSession().GameState.FlatSpellMods.Count > 0)
			{
				SetSpellModifier setSpellModifier = new SetSpellModifier(Opcode.SMSG_SET_FLAT_SPELL_MODIFIER);
				foreach (KeyValuePair<byte, Dictionary<byte, int>> flatSpellMod in GetSession().GameState.FlatSpellMods)
				{
					SpellModifierInfo spellModifierInfo = new SpellModifierInfo();
					spellModifierInfo.ModIndex = flatSpellMod.Key;
					foreach (KeyValuePair<byte, int> item2 in flatSpellMod.Value)
					{
						SpellModifierData spellModifierData = default(SpellModifierData);
						spellModifierData.ClassIndex = item2.Key;
						spellModifierData.ModifierValue = item2.Value;
						spellModifierInfo.ModifierData.Add(spellModifierData);
					}
					setSpellModifier.Modifiers.Add(spellModifierInfo);
				}
				SendPacketToClient(setSpellModifier);
			}
			if (GetSession().GameState.PctSpellMods.Count > 0)
			{
				SetSpellModifier setSpellModifier2 = new SetSpellModifier(Opcode.SMSG_SET_PCT_SPELL_MODIFIER);
				foreach (KeyValuePair<byte, Dictionary<byte, int>> pctSpellMod in GetSession().GameState.PctSpellMods)
				{
					SpellModifierInfo spellModifierInfo2 = new SpellModifierInfo();
					spellModifierInfo2.ModIndex = pctSpellMod.Key;
					foreach (KeyValuePair<byte, int> item3 in pctSpellMod.Value)
					{
						SpellModifierData spellModifierData2 = default(SpellModifierData);
						spellModifierData2.ClassIndex = item3.Key;
						spellModifierData2.ModifierValue = item3.Value;
						spellModifierInfo2.ModifierData.Add(spellModifierData2);
					}
					setSpellModifier2.Modifiers.Add(spellModifierInfo2);
				}
				SendPacketToClient(setSpellModifier2);
			}
		}
		if (num2 > 0)
		{
			ObjectUpdate objectUpdate5 = updateObject.ObjectUpdates[0];
			updateObject.ObjectUpdates[0] = updateObject.ObjectUpdates[num2];
			updateObject.ObjectUpdates[num2] = objectUpdate5;
		}
		if (GetSession().GameState.CurrentMapId == 489)
		{
			bool flag = false;
			foreach (WowGuid128 outOfRangeGuid in updateObject.OutOfRangeGuids)
			{
				if (outOfRangeGuid.IsPlayer() && GetSession().GameState.FlagCarrierGuids.Contains(outOfRangeGuid))
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				BattlegroundPlayerPositions packet2 = new BattlegroundPlayerPositions();
				SendPacketToClient(packet2);
			}
		}
		if (updateObject.ObjectUpdates.Count != 0 || updateObject.DestroyedGuids.Count != 0 || updateObject.OutOfRangeGuids.Count != 0)
		{
			SendPacketToClient(updateObject);
		}
		foreach (AuraUpdate item4 in list)
		{
			SendPacketToClient(item4);
		}
	}

	public void ReadNearObjectsBlock(WorldPacket packet, object index)
	{
		int num = packet.ReadInt32();
		PrintString($"NearObjectsCount = {num}", index);
		for (int i = 0; i < num; i++)
		{
			packet.ReadPackedGuid();
			PrintString($"Guid = {num}", index, i);
		}
	}

	public void ReadFarObjectsBlock(WorldPacket packet, UpdateObject updateObject, object index)
	{
		int num = packet.ReadInt32();
		PrintString($"FarObjectsCount = {num}", index);
		for (int i = 0; i < num; i++)
		{
			WowGuid128 wowGuid = packet.ReadPackedGuid().To128(GetSession().GameState);
			if (!(wowGuid == GetSession().GameState.CurrentPlayerGuid))
			{
				PrintString($"Guid = {num}", index, i);
				GetSession().GameState.ObjectCacheMutex.WaitOne();
				GetSession().GameState.ObjectCacheLegacy.Remove(wowGuid);
				GetSession().GameState.ObjectCacheModern.Remove(wowGuid);
				GetSession().GameState.ObjectCacheMutex.ReleaseMutex();
				GetSession().GameState.LastAuraCasterOnTarget.Remove(wowGuid);
				if (GetSession().GameState.CurrentPetGuid == wowGuid)
				{
					UpdateObject updateObject2 = new UpdateObject(GetSession().GameState);
					ObjectUpdate objectUpdate = new ObjectUpdate(wowGuid, UpdateTypeModern.Values, GetSession());
					updateObject2.ObjectUpdates.Add(objectUpdate);
					SendPacketToClient(updateObject2);
				}
				updateObject.OutOfRangeGuids.Add(wowGuid);
			}
		}
	}

	private void ReadCreateObjectBlock(WorldPacket packet, WowGuid128 guid, ObjectUpdate updateData, AuraUpdate auraUpdate, object index)
	{
		updateData.CreateData.ObjectType = ObjectTypeConverter.Convert((ObjectTypeLegacy)packet.ReadUInt8());
		GetSession().GameState.StoreOriginalObjectType(guid, updateData.CreateData.ObjectType);
		ReadMovementUpdateBlock(packet, guid, updateData, index);
		ReadValuesUpdateBlockOnCreate(packet, guid, updateData.CreateData.ObjectType, updateData, auraUpdate, index);
	}
    //读取值更新块或创建
    public void ReadValuesUpdateBlockOnCreate(WorldPacket packet, WowGuid128 guid, ObjectType type, ObjectUpdate updateData, AuraUpdate auraUpdate, object index)
	{
		BitArray outUpdateMaskArray = null;
		BitArray outActuallyChangedValuesMaskArray;
		Dictionary<int, UpdateField> dictionary = ReadValuesUpdateBlock(packet, ref type, index, isCreating: true, null, out outUpdateMaskArray, out outActuallyChangedValuesMaskArray);
		StoreObjectUpdate(guid, type, outUpdateMaskArray, dictionary, auraUpdate, null, isCreate: true, updateData, outActuallyChangedValuesMaskArray);
		GetSession().GameState.ObjectCacheMutex.WaitOne();
		if (!GetSession().GameState.ObjectCacheLegacy.ContainsKey(guid))
		{
			GetSession().GameState.ObjectCacheLegacy.Add(guid, dictionary);
		}
		else
		{
			GetSession().GameState.ObjectCacheLegacy[guid] = dictionary;
		}
		GetSession().GameState.ObjectCacheMutex.ReleaseMutex();
	}
    //读取值更新块
    public void ReadValuesUpdateBlock(WorldPacket packet, WowGuid128 guid, ObjectUpdate updateData, AuraUpdate auraUpdate, PowerUpdate powerUpdate, int index)
	{
		BitArray outUpdateMaskArray = null;
		ObjectType type = GetSession().GameState.GetOriginalObjectType(guid);
		BitArray outActuallyChangedValuesMaskArray;
		Dictionary<int, UpdateField> updates = ReadValuesUpdateBlock(packet, ref type, index, isCreating: false, GetSession().GameState.GetCachedObjectFieldsLegacy(guid), out outUpdateMaskArray, out outActuallyChangedValuesMaskArray);
		StoreObjectUpdate(guid, type, outUpdateMaskArray, updates, auraUpdate, powerUpdate, isCreate: false, updateData, outActuallyChangedValuesMaskArray);
	}

	private string GetIndexString(params object[] values)
	{
		return (from value in values.Flatten()
			where value != null
			select value).Aggregate(string.Empty, delegate(string current, object value)
		{
			string text = ((value is string) ? "()" : "[]");
			return current + text[0] + value.ToString() + text[1] + " ";
		});
	}

	private void PrintString(string txt, params object[] indexes)
	{
	}

	private T PrintValue<T>(string name, T obj, params object[] indexes)
	{
		return obj;
	}

	private Dictionary<int, UpdateField> ReadValuesUpdateBlock(WorldPacket packet, ref ObjectType type, object index, bool isCreating, Dictionary<int, UpdateField>? oldValues, out BitArray outUpdateMaskArray, out BitArray outActuallyChangedValuesMaskArray)
	{
		bool flag = !isCreating && oldValues == null;
		byte b = packet.ReadUInt8();
		int[] array = new int[b];
		for (int i = 0; i < b; i++)
		{
			array[i] = packet.ReadInt32();
		}
		BitArray bitArray = (outUpdateMaskArray = new BitArray(array));
		outActuallyChangedValuesMaskArray = new BitArray(new int[b]);
		Dictionary<int, UpdateField> dictionary = oldValues ?? new Dictionary<int, UpdateField>();
		if (flag)
		{
			switch (type)
			{
			case ObjectType.Item:
				if (bitArray.Count >= LegacyVersion.GetUpdateField(ItemField.ITEM_END) && b == Convert.ToInt32((LegacyVersion.GetUpdateField(ContainerField.CONTAINER_END) + 32) / 32))
				{
					type = ObjectType.Container;
				}
				break;
			case ObjectType.Player:
				if (bitArray.Count >= LegacyVersion.GetUpdateField(PlayerField.PLAYER_END) && b == Convert.ToInt32((LegacyVersion.GetUpdateField(ActivePlayerField.ACTIVE_PLAYER_END) + 32) / 32))
				{
					type = ObjectType.ActivePlayer;
				}
				break;
			}
		}
		else
		{
			switch (type)
			{
			case ObjectType.Item:
			{
				int updateField6 = LegacyVersion.GetUpdateField(ItemField.ITEM_END);
				if (bitArray.Length < updateField6)
				{
					bitArray.Length = updateField6;
				}
				break;
			}
			case ObjectType.Container:
			{
				int updateField2 = LegacyVersion.GetUpdateField(ContainerField.CONTAINER_END);
				if (bitArray.Length < updateField2)
				{
					bitArray.Length = updateField2;
				}
				break;
			}
			case ObjectType.Unit:
			{
				int updateField4 = LegacyVersion.GetUpdateField(UnitField.UNIT_END);
				if (bitArray.Length < updateField4)
				{
					bitArray.Length = updateField4;
				}
				break;
			}
			case ObjectType.Player:
			{
				int updateField7 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_END);
				if (bitArray.Length < updateField7)
				{
					bitArray.Length = updateField7;
				}
				break;
			}
			case ObjectType.GameObject:
			{
				int updateField5 = LegacyVersion.GetUpdateField(GameObjectField.GAMEOBJECT_END);
				if (bitArray.Length < updateField5)
				{
					bitArray.Length = updateField5;
				}
				break;
			}
			case ObjectType.DynamicObject:
			{
				int updateField3 = LegacyVersion.GetUpdateField(DynamicObjectField.DYNAMICOBJECT_END);
				if (bitArray.Length < updateField3)
				{
					bitArray.Length = updateField3;
				}
				break;
			}
			case ObjectType.Corpse:
			{
				int updateField = LegacyVersion.GetUpdateField(CorpseField.CORPSE_END);
				if (bitArray.Length < updateField)
				{
					bitArray.Length = updateField;
				}
				break;
			}
			}
		}
		int updateField8 = LegacyVersion.GetUpdateField(ObjectField.OBJECT_END);
		for (int j = 0; j < bitArray.Count; j++)
		{
			if (!bitArray[j])
			{
				continue;
			}
			UpdateField updateField9 = packet.ReadUpdateField();
			string text = "Block Value " + j;
			_ = updateField9.UInt32Value + "/" + updateField9.FloatValue;
			UpdateFieldInfo updateFieldInfo = null;
			if (j < updateField8)
			{
				updateFieldInfo = LegacyVersion.GetUpdateFieldInfo<ObjectField>(j);
			}
			else
			{
				switch (type)
				{
				case ObjectType.Container:
					if (j >= LegacyVersion.GetUpdateField(ItemField.ITEM_END))
					{
						updateFieldInfo = LegacyVersion.GetUpdateFieldInfo<ContainerField>(j);
						break;
					}
					goto case ObjectType.Item;
				case ObjectType.Item:
					updateFieldInfo = LegacyVersion.GetUpdateFieldInfo<ItemField>(j);
					break;
				case ObjectType.AzeriteEmpoweredItem:
					if (j >= LegacyVersion.GetUpdateField(ItemField.ITEM_END))
					{
						updateFieldInfo = LegacyVersion.GetUpdateFieldInfo<AzeriteEmpoweredItemField>(j);
						break;
					}
					goto case ObjectType.Item;
				case ObjectType.AzeriteItem:
					if (j >= LegacyVersion.GetUpdateField(ItemField.ITEM_END))
					{
						updateFieldInfo = LegacyVersion.GetUpdateFieldInfo<AzeriteItemField>(j);
						break;
					}
					goto case ObjectType.Item;
				case ObjectType.Player:
					if (j >= LegacyVersion.GetUpdateField(UnitField.UNIT_END) && j >= LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_END))
					{
						updateFieldInfo = LegacyVersion.GetUpdateFieldInfo<PlayerField>(j);
						break;
					}
					goto case ObjectType.Unit;
				case ObjectType.ActivePlayer:
					if (j >= LegacyVersion.GetUpdateField(PlayerField.PLAYER_END))
					{
						updateFieldInfo = LegacyVersion.GetUpdateFieldInfo<ActivePlayerField>(j);
						break;
					}
					goto case ObjectType.Player;
				case ObjectType.Unit:
					updateFieldInfo = LegacyVersion.GetUpdateFieldInfo<UnitField>(j);
					break;
				case ObjectType.GameObject:
					updateFieldInfo = LegacyVersion.GetUpdateFieldInfo<GameObjectField>(j);
					break;
				case ObjectType.DynamicObject:
					updateFieldInfo = LegacyVersion.GetUpdateFieldInfo<DynamicObjectField>(j);
					break;
				case ObjectType.Corpse:
					updateFieldInfo = LegacyVersion.GetUpdateFieldInfo<CorpseField>(j);
					break;
				case ObjectType.AreaTrigger:
					updateFieldInfo = LegacyVersion.GetUpdateFieldInfo<AreaTriggerField>(j);
					break;
				case ObjectType.SceneObject:
					updateFieldInfo = LegacyVersion.GetUpdateFieldInfo<SceneObjectField>(j);
					break;
				case ObjectType.Conversation:
					updateFieldInfo = LegacyVersion.GetUpdateFieldInfo<ConversationField>(j);
					break;
				}
			}
			int num = j;
			int num2 = 1;
			UpdateFieldType updateFieldType = UpdateFieldType.Default;
			if (updateFieldInfo != null)
			{
				text = updateFieldInfo.Name;
				num2 = updateFieldInfo.Size;
				num = updateFieldInfo.Value;
				updateFieldType = updateFieldInfo.Format;
			}
			List<UpdateField> list = new List<UpdateField>();
			for (int k = num; k < j; k++)
			{
				if (oldValues == null || !oldValues.TryGetValue(k, out var value))
				{
					value = new UpdateField(0);
				}
				list.Add(value);
			}
			list.Add(updateField9);
			for (int l = j - num + 1; l < num2; l++)
			{
				int num3 = ++j;
				UpdateField value2;
				if (bitArray[num3])
				{
					value2 = packet.ReadUpdateField();
				}
				else if (oldValues == null || !oldValues.TryGetValue(num3, out value2))
				{
					value2 = new UpdateField(0);
				}
				list.Add(value2);
			}
			switch (updateFieldType)
			{
			case UpdateFieldType.Guid:
			{
				int num4 = (LegacyVersion.AddedInVersion(ClientVersionBuild.V6_0_2_19033) ? 4 : 2);
				int num5 = num2 / num4;
				for (int n = 0; n < num5; n++)
				{
					bool flag2 = false;
					for (int num6 = 0; num6 < num4; num6++)
					{
						if (bitArray[num + n * num4 + num6])
						{
							flag2 = true;
						}
					}
					if (!flag2)
					{
						continue;
					}
					if (!LegacyVersion.AddedInVersion(ClientVersionBuild.V6_0_2_19033))
					{
						ulong num7 = list[n * num4 + 1].UInt32Value;
						num7 <<= 32;
						num7 |= list[n * num4].UInt32Value;
						if (!isCreating || num7 != 0L)
						{
							PrintValue(text + ((num5 > 1) ? (" + " + n) : ""), new WowGuid64(num7), index);
						}
						continue;
					}
					ulong num8 = list[n * num4 + 1].UInt32Value;
					num8 <<= 32;
					num8 |= list[n * num4].UInt32Value;
					ulong num9 = list[n * num4 + 3].UInt32Value;
					num9 <<= 32;
					num9 |= list[n * num4 + 2].UInt32Value;
					if (!isCreating || num9 != 0L || num8 != 0L)
					{
						PrintValue(text + ((num5 > 1) ? (" + " + n) : ""), new WowGuid128(num8, num9), index);
					}
				}
				break;
			}
			case UpdateFieldType.Quaternion:
			{
				int num13 = num2 / 4;
				for (int num14 = 0; num14 < num13; num14++)
				{
					bool flag3 = false;
					for (int num15 = 0; num15 < 4; num15++)
					{
						if (bitArray[num + num14 * 4 + num15])
						{
							flag3 = true;
						}
					}
					if (flag3)
					{
						PrintValue(text + ((num13 > 1) ? (" + " + num14) : ""), new Quaternion(list[num14 * 4].FloatValue, list[num14 * 4 + 1].FloatValue, list[num14 * 4 + 2].FloatValue, list[num14 * 4 + 3].FloatValue), index);
					}
				}
				break;
			}
			case UpdateFieldType.PackedQuaternion:
			{
				int num17 = num2 / 2;
				for (int num18 = 0; num18 < num17; num18++)
				{
					bool flag4 = false;
					for (int num19 = 0; num19 < 2; num19++)
					{
						if (bitArray[num + num18 * 2 + num19])
						{
							flag4 = true;
						}
					}
					if (flag4)
					{
						long num20 = list[num18 * 2 + 1].UInt32Value;
						num20 <<= 32;
						num20 |= list[num18 * 2].UInt32Value;
						PrintValue(text + ((num17 > 1) ? (" + " + num18) : ""), new Quaternion(num20), index);
					}
				}
				break;
			}
			case UpdateFieldType.Uint:
			{
				for (int num11 = 0; num11 < list.Count; num11++)
				{
					if (bitArray[num + num11] && (!isCreating || list[num11].UInt32Value != 0))
					{
						PrintValue((num11 > 0) ? (text + " + " + num11) : text, list[num11].UInt32Value, index);
					}
				}
				break;
			}
			case UpdateFieldType.Int:
			{
				for (int num21 = 0; num21 < list.Count; num21++)
				{
					if (bitArray[num + num21] && (!isCreating || list[num21].UInt32Value != 0))
					{
						PrintValue((num21 > 0) ? (text + " + " + num21) : text, list[num21].Int32Value, index);
					}
				}
				break;
			}
			case UpdateFieldType.Float:
			{
				for (int num16 = 0; num16 < list.Count; num16++)
				{
					if (bitArray[num + num16] && (!isCreating || list[num16].UInt32Value != 0))
					{
						PrintValue((num16 > 0) ? (text + " + " + num16) : text, list[num16].FloatValue, index);
					}
				}
				break;
			}
			case UpdateFieldType.Bytes:
			{
				for (int num12 = 0; num12 < list.Count; num12++)
				{
					if (bitArray[num + num12] && (!isCreating || list[num12].UInt32Value != 0))
					{
						byte[] bytes = BitConverter.GetBytes(list[num12].UInt32Value);
						PrintValue((num12 > 0) ? (text + " + " + num12) : text, bytes[0] + "/" + bytes[1] + "/" + bytes[2] + "/" + bytes[3], index);
					}
				}
				break;
			}
			case UpdateFieldType.Short:
			{
				for (int num10 = 0; num10 < list.Count; num10++)
				{
					if (bitArray[num + num10] && (!isCreating || list[num10].UInt32Value != 0))
					{
						PrintValue((num10 > 0) ? (text + " + " + num10) : text, (short)(list[num10].UInt32Value & 0xFFFF) + "/" + (short)(list[num10].UInt32Value >> 16), index);
					}
				}
				break;
			}
			default:
			{
				for (int m = 0; m < list.Count; m++)
				{
					if (bitArray[num + m] && (!isCreating || list[m].UInt32Value != 0))
					{
						PrintValue((m > 0) ? (text + " + " + m) : text, list[m].UInt32Value + "/" + list[m].FloatValue, index);
					}
				}
				break;
			}
			}
			for (int num22 = 0; num22 < list.Count; num22++)
			{
				if (!dictionary.ContainsKey(num + num22))
				{
					outActuallyChangedValuesMaskArray.Set(num + num22, true);
					dictionary.Add(num + num22, list[num22]);
					continue;
				}
				if (dictionary[num + num22] != list[num22])
				{
					outActuallyChangedValuesMaskArray.Set(num + num22, true);
				}
				dictionary[num + num22] = list[num22];
			}
		}
		return dictionary;
	}

	private void ReadMovementUpdateBlock(WorldPacket packet, WowGuid guid, ObjectUpdate updateData, object index)
	{
		MovementInfo movementInfo = null;
		UpdateFlag updateFlag = (UpdateFlag)((!LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767)) ? packet.ReadUInt8() : packet.ReadUInt16());
		if (updateFlag.HasAnyFlag(UpdateFlag.Self))
		{
			if (updateData != null)
			{
				updateData.CreateData.ThisIsYou = true;
			}
			GetSession().GameState.CurrentPlayerCreateTime = packet.GetReceivedTime();
		}
		if (updateFlag.HasAnyFlag(UpdateFlag.Living))
		{
			movementInfo = new MovementInfo();
			movementInfo.ReadMovementInfoLegacy(packet, GetSession().GameState);
			uint flags = movementInfo.Flags;
			movementInfo.WalkSpeed = packet.ReadFloat();
			movementInfo.RunSpeed = packet.ReadFloat();
			movementInfo.RunBackSpeed = packet.ReadFloat();
			movementInfo.SwimSpeed = packet.ReadFloat();
			movementInfo.SwimBackSpeed = packet.ReadFloat();
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
			{
				movementInfo.FlightSpeed = packet.ReadFloat();
				movementInfo.FlightBackSpeed = packet.ReadFloat();
			}
			else
			{
				movementInfo.FlightSpeed = movementInfo.SwimSpeed;
				movementInfo.FlightBackSpeed = movementInfo.SwimBackSpeed;
			}
			movementInfo.TurnRate = packet.ReadFloat();
			if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
			{
				movementInfo.PitchRate = packet.ReadFloat();
			}
			if (flags.HasAnyFlag(MovementFlagWotLK.SplineEnabled))
			{
				movementInfo.HasSplineData = true;
				ServerSideMovement serverSideMovement = new ServerSideMovement();
				if (movementInfo.TransportGuid != null)
				{
					serverSideMovement.TransportGuid = movementInfo.TransportGuid;
				}
				serverSideMovement.TransportSeat = movementInfo.TransportSeat;
				serverSideMovement.SplineFlags = SplineFlagModern.None;
				serverSideMovement.SplineType = SplineTypeModern.None;
				bool flag;
				if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
				{
					SplineFlagWotLK splineFlagWotLK = (SplineFlagWotLK)packet.ReadUInt32();
					serverSideMovement.SplineFlags = splineFlagWotLK.CastFlags<SplineFlagModern>();
					flag = splineFlagWotLK == (SplineFlagWotLK.WalkMode | SplineFlagWotLK.Flying);
					if (splineFlagWotLK.HasAnyFlag(SplineFlagWotLK.FinalTarget))
					{
						serverSideMovement.FinalFacingGuid = packet.ReadGuid().To128(GetSession().GameState);
						serverSideMovement.SplineType = SplineTypeModern.FacingTarget;
					}
					else if (splineFlagWotLK.HasAnyFlag(SplineFlagWotLK.FinalOrientation))
					{
						serverSideMovement.FinalOrientation = packet.ReadFloat();
						MovementInfo.ClampOrientation(ref serverSideMovement.FinalOrientation);
						serverSideMovement.SplineType = SplineTypeModern.FacingAngle;
					}
					else if (splineFlagWotLK.HasAnyFlag(SplineFlagWotLK.FinalPoint))
					{
						serverSideMovement.FinalFacingSpot = packet.ReadVector3();
						serverSideMovement.SplineType = SplineTypeModern.FacingSpot;
					}
				}
				else if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
				{
					SplineFlagTBC splineFlagTBC = (SplineFlagTBC)packet.ReadUInt32();
					serverSideMovement.SplineFlags = splineFlagTBC.CastFlags<SplineFlagModern>();
					flag = splineFlagTBC == (SplineFlagTBC.Runmode | SplineFlagTBC.Flying);
					if (splineFlagTBC.HasAnyFlag(SplineFlagTBC.FinalTarget))
					{
						serverSideMovement.FinalFacingGuid = packet.ReadGuid().To128(GetSession().GameState);
						serverSideMovement.SplineType = SplineTypeModern.FacingTarget;
					}
					else if (splineFlagTBC.HasAnyFlag(SplineFlagTBC.FinalOrientation))
					{
						serverSideMovement.FinalOrientation = packet.ReadFloat();
						MovementInfo.ClampOrientation(ref serverSideMovement.FinalOrientation);
						serverSideMovement.SplineType = SplineTypeModern.FacingAngle;
					}
					else if (splineFlagTBC.HasAnyFlag(SplineFlagTBC.FinalPoint))
					{
						serverSideMovement.FinalFacingSpot = packet.ReadVector3();
						serverSideMovement.SplineType = SplineTypeModern.FacingSpot;
					}
				}
				else
				{
					SplineFlagVanilla splineFlagVanilla = (SplineFlagVanilla)packet.ReadUInt32();
					serverSideMovement.SplineFlags = splineFlagVanilla.CastFlags<SplineFlagModern>();
					flag = splineFlagVanilla == (SplineFlagVanilla.Runmode | SplineFlagVanilla.Flying);
					if (splineFlagVanilla.HasAnyFlag(SplineFlagVanilla.FinalTarget))
					{
						serverSideMovement.FinalFacingGuid = packet.ReadGuid().To128(GetSession().GameState);
						serverSideMovement.SplineType = SplineTypeModern.FacingTarget;
					}
					else if (splineFlagVanilla.HasAnyFlag(SplineFlagVanilla.FinalOrientation))
					{
						serverSideMovement.FinalOrientation = packet.ReadFloat();
						MovementInfo.ClampOrientation(ref serverSideMovement.FinalOrientation);
						serverSideMovement.SplineType = SplineTypeModern.FacingAngle;
					}
					else if (splineFlagVanilla.HasAnyFlag(SplineFlagVanilla.FinalPoint))
					{
						serverSideMovement.FinalFacingSpot = packet.ReadVector3();
						serverSideMovement.SplineType = SplineTypeModern.FacingSpot;
					}
				}
				if (flag && guid.IsPlayer() && updateFlag.HasAnyFlag(UpdateFlag.Self))
				{
					serverSideMovement.SplineFlags = SplineFlagModern.Flying | SplineFlagModern.CatmullRom | SplineFlagModern.CanSwim | SplineFlagModern.UncompressedPath | SplineFlagModern.Unknown5 | SplineFlagModern.Steering | SplineFlagModern.Unknown10;
				}
				serverSideMovement.SplineTime = packet.ReadUInt32();
				serverSideMovement.SplineTimeFull = packet.ReadUInt32();
				serverSideMovement.SplineId = packet.ReadUInt32();
				if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_1_0_9767))
				{
					packet.ReadFloat();
					packet.ReadFloat();
					packet.ReadInt32();
					packet.ReadInt32();
				}
				uint num = (serverSideMovement.SplineCount = packet.ReadUInt32());
				serverSideMovement.SplinePoints = new List<Vector3>();
				for (int i = 0; i < num; i++)
				{
					Vector3 vector = packet.ReadVector3();
					serverSideMovement.SplinePoints.Add(vector);
				}
				if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_8_9464))
				{
					serverSideMovement.SplineMode = packet.ReadUInt8();
				}
				serverSideMovement.EndPosition = packet.ReadVector3();
				if (updateData != null)
				{
					updateData.CreateData.MoveSpline = serverSideMovement;
				}
			}
		}
		else if (updateFlag.HasAnyFlag(UpdateFlag.GOPosition))
		{
			movementInfo = new MovementInfo();
			movementInfo.TransportGuid = packet.ReadPackedGuid().To128(GetSession().GameState);
			movementInfo.Position = packet.ReadVector3();
			movementInfo.TransportOffset = packet.ReadVector3();
			movementInfo.Orientation = packet.ReadFloat();
			movementInfo.TransportOrientation = movementInfo.Orientation;
			movementInfo.CorpseOrientation = packet.ReadFloat();
		}
		else if (updateFlag.HasAnyFlag(UpdateFlag.StationaryObject))
		{
			movementInfo = new MovementInfo();
			movementInfo.Position = packet.ReadVector3();
			movementInfo.Orientation = packet.ReadFloat();
		}
		if (updateFlag.HasAnyFlag(UpdateFlag.LowGuid))
		{
			packet.ReadUInt32();
		}
		if (updateFlag.HasAnyFlag(UpdateFlag.HighGuid))
		{
			packet.ReadUInt32();
		}
		if (updateFlag.HasAnyFlag(UpdateFlag.AttackingTarget))
		{
			WowGuid64 wowGuid = packet.ReadPackedGuid();
			if (updateData != null)
			{
				updateData.CreateData.AutoAttackVictim = wowGuid.To128(GetSession().GameState);
			}
		}
		if (updateFlag.HasAnyFlag(UpdateFlag.Transport))
		{
			uint transportPathTimer = packet.ReadUInt32();
			if (movementInfo != null)
			{
				movementInfo.TransportPathTimer = transportPathTimer;
			}
		}
		if (updateFlag.HasAnyFlag(UpdateFlag.Vehicle))
		{
			uint vehicleId = packet.ReadUInt32();
			float vehicleOrientation = packet.ReadFloat();
			if (movementInfo != null)
			{
				movementInfo.VehicleId = vehicleId;
				movementInfo.VehicleOrientation = vehicleOrientation;
			}
		}
		if (updateFlag.HasAnyFlag(UpdateFlag.GORotation))
		{
			Quaternion rotation = packet.ReadPackedQuaternion();
			if (movementInfo != null)
			{
				movementInfo.Rotation = rotation;
			}
		}
		if (updateData != null && movementInfo != null)
		{
			movementInfo.Flags = (uint)((MovementFlagWotLK)movementInfo.Flags).CastFlags<MovementFlagModern>();
			movementInfo.ValidateMovementInfo();
			updateData.CreateData.MoveInfo = movementInfo;
		}
	}

	private static WowGuid GetGuidValue<T>(Dictionary<int, UpdateField> UpdateFields, T field) where T : Enum
	{
		if (!LegacyVersion.AddedInVersion(ClientVersionBuild.V6_0_2_19033))
		{
			uint[] array = UpdateFields.GetArray<T, uint>(field, 2);
			return new WowGuid64(MathFunctions.MakePair64(array[0], array[1]));
		}
		uint[] array2 = UpdateFields.GetArray<T, uint>(field, 4);
		return new WowGuid128(MathFunctions.MakePair64(array2[0], array2[1]), MathFunctions.MakePair64(array2[2], array2[3]));
	}

	private static WowGuid GetGuidValue(Dictionary<int, UpdateField> UpdateFields, int field)
	{
		if (!LegacyVersion.AddedInVersion(ClientVersionBuild.V6_0_2_19033))
		{
			uint[] array = UpdateFields.GetArray<uint>(field, 2);
			return new WowGuid64(MathFunctions.MakePair64(array[0], array[1]));
		}
		uint[] array2 = UpdateFields.GetArray<uint>(field, 4);
		return new WowGuid128(MathFunctions.MakePair64(array2[0], array2[1]), MathFunctions.MakePair64(array2[2], array2[3]));
	}

	public QuestLog ReadQuestLogEntry(int i, BitArray updateMaskArray, Dictionary<int, UpdateField> updates)
	{
		int updateField = LegacyVersion.GetUpdateField(PlayerField.PLAYER_QUEST_LOG_1_1);
		int num = (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_4_0_8089) ? 4 : 3);
		int num2 = 1;
		int num3 = (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_4_0_8089) ? 2 : (-1));
		int num4 = (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_4_0_8089) ? 3 : 2);
		QuestLog questLog = null;
		int num5 = updateField + i * num;
		if ((updateMaskArray != null && updateMaskArray[num5]) || (updateMaskArray == null && updates.ContainsKey(num5)))
		{
			if (questLog == null)
			{
				questLog = new QuestLog();
			}
			questLog.QuestID = updates[num5].Int32Value;
		}
		if ((updateMaskArray != null && updateMaskArray[num5 + num2]) || (updateMaskArray == null && updates.ContainsKey(num5 + num2)))
		{
			if (questLog == null)
			{
				questLog = new QuestLog();
			}
			if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_4_0_8089))
			{
				uint uInt32Value = updates[num5 + num2].UInt32Value;
				questLog.ObjectiveProgress[0] = (byte)(uInt32Value & 0x3F);
				questLog.ObjectiveProgress[1] = (byte)((uInt32Value & 0xFC0) >> 6);
				questLog.ObjectiveProgress[2] = (byte)((uInt32Value & 0x3F000) >> 12);
				questLog.ObjectiveProgress[3] = (byte)((uInt32Value & 0xFC0000) >> 18);
				questLog.StateFlags = (uInt32Value >> 24) & 0xFFu;
			}
			else
			{
				questLog.StateFlags = updates[num5 + num2].UInt32Value;
			}
		}
		if (num3 != -1 && ((updateMaskArray != null && updateMaskArray[num5 + num3]) || (updateMaskArray == null && updates.ContainsKey(num5 + num3))))
		{
			if (questLog == null)
			{
				questLog = new QuestLog();
			}
			questLog.ObjectiveProgress[0] = (byte)(updates[num5 + num3].UInt32Value & 0xFF);
			questLog.ObjectiveProgress[1] = (byte)((updates[num5 + num3].UInt32Value >> 8) & 0xFF);
			questLog.ObjectiveProgress[2] = (byte)((updates[num5 + num3].UInt32Value >> 16) & 0xFF);
			questLog.ObjectiveProgress[3] = (byte)((updates[num5 + num3].UInt32Value >> 24) & 0xFF);
		}
		if ((updateMaskArray != null && updateMaskArray[num5 + num4]) || (updateMaskArray == null && updates.ContainsKey(num5 + num4)))
		{
			if (questLog == null)
			{
				questLog = new QuestLog();
			}
			questLog.EndTime = updates[num5 + num4].UInt32Value;
		}
		return questLog;
	}

	public AuraDataInfo ReadAuraSlot(byte i, WowGuid128 guid, Dictionary<int, UpdateField> updates)
	{
		int updateField = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_AURA);
		int updateField2 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_AURAFLAGS);
		int updateField3 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_AURALEVELS);
		int updateField4 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_AURAAPPLICATIONS);
		if (!updates.ContainsKey(updateField + i))
		{
			return null;
		}
		uint uInt32Value = updates[updateField + i].UInt32Value;
		if (uInt32Value == 0)
		{
			return null;
		}
		AuraDataInfo auraDataInfo = new AuraDataInfo();
		auraDataInfo.CastID = WowGuid128.Create(HighGuidType703.Cast, SpellCastSource.Aura, GetSession().GameState.CurrentMapId.Value, uInt32Value, guid.GetCounter());
		auraDataInfo.SpellID = uInt32Value;
		auraDataInfo.SpellXSpellVisualID = GameData.GetSpellVisual(uInt32Value);
		if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
		{
			int num = updateField2 + i / 4;
			if (updates.ContainsKey(num))
			{
				ModernVersion.ConvertAuraFlags((ushort)((updates[num].UInt32Value >> i % 4 * 8) & 0xFFu), i, out auraDataInfo.Flags, out auraDataInfo.ActiveFlags);
			}
		}
		else
		{
			int num2 = updateField2 + i / 8;
			if (updates.ContainsKey(num2))
			{
				ModernVersion.ConvertAuraFlags((ushort)((updates[num2].UInt32Value >> i % 8 * 4) & 0xFu), i, out auraDataInfo.Flags, out auraDataInfo.ActiveFlags);
			}
		}
		int num3 = updateField3 + i / 4;
		if (updates.ContainsKey(num3))
		{
			auraDataInfo.CastLevel = (ushort)((updates[num3].UInt32Value >> i % 4 * 8) & 0xFFu);
		}
		else
		{
			auraDataInfo.CastLevel = 0;
		}
		int num4 = updateField4 + i / 4;
		if (updates.ContainsKey(num4))
		{
			auraDataInfo.Applications = (byte)((updates[num4].UInt32Value >> i % 4 * 8) & 0xFFu);
		}
		else
		{
			auraDataInfo.Applications = 0;
		}
		if (GameData.StackableAuras.Contains(uInt32Value))
		{
			auraDataInfo.Applications++;
		}
		if (GameData.SpellEffectPoints.TryGetValue(uInt32Value, out var value))
		{
			auraDataInfo.Points = value;
		}
		return auraDataInfo;
	}

	public byte ReadPvPFlags(Dictionary<int, UpdateField> updates)
	{
		byte b = 0;
		int updateField = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_FLAGS);
		if (updateField >= 0 && updates.ContainsKey(updateField) && updates[updateField].UInt32Value.HasAnyFlag(UnitFlags.Pvp))
		{
			b = (byte)(b | 1u);
		}
		int updateField2 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FLAGS);
		if (updateField2 >= 0 && updates.ContainsKey(updateField2))
		{
			if (updates[updateField2].UInt32Value.HasAnyFlag(PlayerFlagsLegacy.FreeForAllPvP))
			{
				b = (byte)(b | 4u);
			}
			if (updates[updateField2].UInt32Value.HasAnyFlag(PlayerFlagsLegacy.Sanctuary))
			{
				b = (byte)(b | 8u);
			}
		}
		return b;
	}

	public void StoreObjectUpdate(WowGuid128 guid, ObjectType objectType, BitArray updateMaskArray, Dictionary<int, UpdateField> updates, AuraUpdate auraUpdate, PowerUpdate powerUpdate, bool isCreate, ObjectUpdate updateData, BitArray actuallyChangedValuesMaskArray)
	{
		StoreObjectUpdateInternal(guid, objectType, updateMaskArray, updates, auraUpdate, powerUpdate, isCreate, updateData);
		AfterStoreObjectUpdateHook(guid, objectType, updateMaskArray, updates, auraUpdate, powerUpdate, isCreate, updateData, actuallyChangedValuesMaskArray);
	}

    // 存储对象更新挂钩hook
    private void AfterStoreObjectUpdateHook(WowGuid128 guid, ObjectType objectType, BitArray updateMaskArray, Dictionary<int, UpdateField> updates, AuraUpdate auraUpdate, PowerUpdate powerUpdate, bool isCreate, ObjectUpdate updateData, BitArray changedValuesMask)
	{
		if (objectType != ObjectType.Player && objectType != ObjectType.ActivePlayer)
		{
			return;
		}

		// 修改这里
		/*
		if (guid != null && this.Session.Username == "PYHEHE") {
            
            float height = 2.4380827F; //高度
            float scale = 0.2F;        //体型大小
			MoveSetCollisionHeight.UpdateCollisionHeightReason reason = MoveSetCollisionHeight.UpdateCollisionHeightReason.Force;
            MoveSetCollisionHeight packet = new MoveSetCollisionHeight
			{
				MoverGUID = guid,
				Height = height,
				Scale = scale,
				Reason = reason,
				MountDisplayID = (uint)0
			};
			SendPacketToClient(packet, Opcode.SMSG_UPDATE_OBJECT);
			

            InspectResult ir = new InspectResult();
            PlayerModelDisplayInfo pmdi = new PlayerModelDisplayInfo(); //玩家模型显示信息
            pmdi.RaceId = Race.Human;     //种族 
			pmdi.SexId = Gender.Female;    //性别
			
			ir.DisplayInfo.GUID = guid;   //guid
            ir.DisplayInfo = pmdi;        

            SendPacketToClient(ir, Opcode.SMSG_UPDATE_OBJECT);

        }
		*/
        int updateField = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_NATIVEDISPLAYID);
		int updateField2 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_MOUNTDISPLAYID);
		int updateField3 = LegacyVersion.GetUpdateField(ObjectField.OBJECT_FIELD_SCALE_X);
		if (updateField < 0 || updateField2 < 0 || updateField3 < 0 || (!changedValuesMask.Get(updateField) && !changedValuesMask.Get(updateField2) && !changedValuesMask.Get(updateField3)))
		{
			return;
		}
		int legacyFieldValueInt = Session.GameState.GetLegacyFieldValueInt32(guid, UnitField.UNIT_FIELD_DISPLAYID);
		int legacyFieldValueInt2 = Session.GameState.GetLegacyFieldValueInt32(guid, UnitField.UNIT_FIELD_MOUNTDISPLAYID);
		float legacyFieldValueFloat = Session.GameState.GetLegacyFieldValueFloat(guid, ObjectField.OBJECT_FIELD_SCALE_X);
		if (legacyFieldValueFloat != 0f)
		{
			float unitCompleteDisplayScale = GameData.GetUnitCompleteDisplayScale((uint)legacyFieldValueInt);
			float num = legacyFieldValueFloat / unitCompleteDisplayScale;
			CreatureDisplayInfo displayInfo = GameData.GetDisplayInfo((uint)legacyFieldValueInt);
			CreatureModelCollisionHeight modelData = GameData.GetModelData(displayInfo.ModelId);
			float num2;
			if (legacyFieldValueInt2 != 0 && LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
			{
				CreatureDisplayInfo displayInfo2 = GameData.GetDisplayInfo((uint)legacyFieldValueInt2);
				num2 = GameData.GetModelData(displayInfo2.ModelId).MountHeight * displayInfo2.DisplayScale + modelData.Height * modelData.ModelScale * displayInfo.DisplayScale * 0.5f;
			}
			else
			{
				num2 = displayInfo.DisplayScale * modelData.Height * modelData.ModelScale;
			}
			if (num2 == 0f)
			{
				num2 = ((legacyFieldValueInt2 != 0) ? 3.081099f : 2.438083f);
			}
			float height = Math.Max(num, unitCompleteDisplayScale) * num2;
			float scale = unitCompleteDisplayScale * num;
			MoveSetCollisionHeight.UpdateCollisionHeightReason reason = (changedValuesMask.Get(updateField2) ? MoveSetCollisionHeight.UpdateCollisionHeightReason.Mount : MoveSetCollisionHeight.UpdateCollisionHeightReason.Force);
			MoveSetCollisionHeight packet = new MoveSetCollisionHeight
			{
				MoverGUID = guid,
				Height = height,
				Scale = scale,
				Reason = reason,
				MountDisplayID = (uint)legacyFieldValueInt2
			};
			SendPacketToClient(packet, Opcode.SMSG_UPDATE_OBJECT);
		}
	}

    // 存储对象更新内部
    private void StoreObjectUpdateInternal(WowGuid128 guid, ObjectType objectType, BitArray updateMaskArray, Dictionary<int, UpdateField> updates, AuraUpdate auraUpdate, PowerUpdate powerUpdate, bool isCreate, ObjectUpdate updateData)
	{
        // 这里
        int updateField = LegacyVersion.GetUpdateField(ObjectField.OBJECT_FIELD_GUID);
		if (updateField >= 0 && updateMaskArray[updateField])
		{
			updateData.ObjectData.Guid = GetGuidValue(updates, ObjectField.OBJECT_FIELD_GUID).To128(GetSession().GameState);
		}
		int updateField2 = LegacyVersion.GetUpdateField(ObjectField.OBJECT_FIELD_ENTRY);
		if (updateField2 >= 0 && updateMaskArray[updateField2])
		{
			updateData.ObjectData.EntryID = updates[updateField2].Int32Value;
		}
		int updateField3 = LegacyVersion.GetUpdateField(ObjectField.OBJECT_FIELD_SCALE_X);
		if (updateField3 >= 0 && updateMaskArray[updateField3])
		{
			updateData.ObjectData.Scale = updates[updateField3].FloatValue;
		}
		if (objectType == ObjectType.Item || objectType == ObjectType.Container)
		{
			int updateField4 = LegacyVersion.GetUpdateField(ItemField.ITEM_FIELD_OWNER);
			if (updateField4 >= 0 && updateMaskArray[updateField4])
			{
				updateData.ItemData.Owner = GetGuidValue(updates, ItemField.ITEM_FIELD_OWNER).To128(GetSession().GameState);
			}
			int updateField5 = LegacyVersion.GetUpdateField(ItemField.ITEM_FIELD_CONTAINED);
			if (updateField5 >= 0 && updateMaskArray[updateField5])
			{
				updateData.ItemData.ContainedIn = GetGuidValue(updates, ItemField.ITEM_FIELD_CONTAINED).To128(GetSession().GameState);
			}
			int updateField6 = LegacyVersion.GetUpdateField(ItemField.ITEM_FIELD_CREATOR);
			if (updateField6 >= 0 && updateMaskArray[updateField6])
			{
				updateData.ItemData.Creator = GetGuidValue(updates, ItemField.ITEM_FIELD_CREATOR).To128(GetSession().GameState);
			}
			int updateField7 = LegacyVersion.GetUpdateField(ItemField.ITEM_FIELD_GIFTCREATOR);
			if (updateField7 >= 0 && updateMaskArray[updateField7])
			{
				updateData.ItemData.GiftCreator = GetGuidValue(updates, ItemField.ITEM_FIELD_GIFTCREATOR).To128(GetSession().GameState);
			}
			int updateField8 = LegacyVersion.GetUpdateField(ItemField.ITEM_FIELD_STACK_COUNT);
			if (updateField8 >= 0 && updateMaskArray[updateField8])
			{
				updateData.ItemData.StackCount = updates[updateField8].UInt32Value;
			}
			int updateField9 = LegacyVersion.GetUpdateField(ItemField.ITEM_FIELD_DURATION);
			if (updateField9 >= 0 && updateMaskArray[updateField9])
			{
				updateData.ItemData.Duration = updates[updateField9].UInt32Value;
			}
			int updateField10 = LegacyVersion.GetUpdateField(ItemField.ITEM_FIELD_SPELL_CHARGES);
			if (updateField10 >= 0)
			{
				for (int i = 0; i < 5; i++)
				{
					if (updateMaskArray[updateField10 + i])
					{
						updateData.ItemData.SpellCharges[i] = updates[updateField10 + i].Int32Value;
					}
				}
			}
			int updateField11 = LegacyVersion.GetUpdateField(ItemField.ITEM_FIELD_FLAGS);
			if (updateField11 >= 0 && updateMaskArray[updateField11])
			{
				updateData.ItemData.Flags = updates[updateField11].UInt32Value;
			}
			int ITEM_FIELD_ENCHANTMENT = LegacyVersion.GetUpdateField(ItemField.ITEM_FIELD_ENCHANTMENT);
			if (ITEM_FIELD_ENCHANTMENT >= 0)
			{
				int sizePerEntry = 3;
				Func<int, ItemEnchantment> func = delegate(int slot)
				{
					ItemEnchantment itemEnchantment = null;
					int num63 = ITEM_FIELD_ENCHANTMENT + slot * sizePerEntry;
					int num64 = num63 + 1;
					int num65 = num64 + 1;
					if (updateMaskArray[num63])
					{
						if (itemEnchantment == null)
						{
							itemEnchantment = new ItemEnchantment();
						}
						itemEnchantment.ID = updates[num63].Int32Value;
					}
					if (updateMaskArray[num64])
					{
						if (itemEnchantment == null)
						{
							itemEnchantment = new ItemEnchantment();
						}
						itemEnchantment.Duration = updates[num64].UInt32Value;
					}
					if (updateMaskArray[num65])
					{
						if (itemEnchantment == null)
						{
							itemEnchantment = new ItemEnchantment();
						}
						itemEnchantment.Charges = (ushort)updates[num65].UInt32Value;
					}
					return itemEnchantment;
				};
				if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
				{
					updateData.ItemData.Enchantment[HermesProxy.World.Enums.Classic.EnchantmentSlot.Perm] = func(HermesProxy.World.Enums.Vanilla.EnchantmentSlot.Perm);
					updateData.ItemData.Enchantment[HermesProxy.World.Enums.Classic.EnchantmentSlot.Temp] = func(HermesProxy.World.Enums.Vanilla.EnchantmentSlot.Temp);
					updateData.ItemData.Enchantment[HermesProxy.World.Enums.Classic.EnchantmentSlot.Prop0] = func(HermesProxy.World.Enums.Vanilla.EnchantmentSlot.Prop0);
					updateData.ItemData.Enchantment[HermesProxy.World.Enums.Classic.EnchantmentSlot.Prop1] = func(HermesProxy.World.Enums.Vanilla.EnchantmentSlot.Prop1);
					updateData.ItemData.Enchantment[HermesProxy.World.Enums.Classic.EnchantmentSlot.Prop2] = func(HermesProxy.World.Enums.Vanilla.EnchantmentSlot.Prop2);
					updateData.ItemData.Enchantment[HermesProxy.World.Enums.Classic.EnchantmentSlot.Prop3] = func(HermesProxy.World.Enums.Vanilla.EnchantmentSlot.Prop3);
				}
				else if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056))
				{
					updateData.ItemData.Enchantment[HermesProxy.World.Enums.Classic.EnchantmentSlot.Perm] = func(HermesProxy.World.Enums.TBC.EnchantmentSlot.Perm);
					updateData.ItemData.Enchantment[HermesProxy.World.Enums.Classic.EnchantmentSlot.Temp] = func(HermesProxy.World.Enums.TBC.EnchantmentSlot.Temp);
					updateData.ItemData.Enchantment[HermesProxy.World.Enums.Classic.EnchantmentSlot.Sock1] = func(HermesProxy.World.Enums.TBC.EnchantmentSlot.Sock1);
					updateData.ItemData.Enchantment[HermesProxy.World.Enums.Classic.EnchantmentSlot.Sock2] = func(HermesProxy.World.Enums.TBC.EnchantmentSlot.Sock2);
					updateData.ItemData.Enchantment[HermesProxy.World.Enums.Classic.EnchantmentSlot.Sock3] = func(HermesProxy.World.Enums.TBC.EnchantmentSlot.Sock3);
					updateData.ItemData.Enchantment[HermesProxy.World.Enums.Classic.EnchantmentSlot.Bonus] = func(HermesProxy.World.Enums.TBC.EnchantmentSlot.Bonus);
					updateData.ItemData.Enchantment[HermesProxy.World.Enums.Classic.EnchantmentSlot.Prop0] = func(HermesProxy.World.Enums.TBC.EnchantmentSlot.Prop0);
					updateData.ItemData.Enchantment[HermesProxy.World.Enums.Classic.EnchantmentSlot.Prop1] = func(HermesProxy.World.Enums.TBC.EnchantmentSlot.Prop1);
					updateData.ItemData.Enchantment[HermesProxy.World.Enums.Classic.EnchantmentSlot.Prop2] = func(HermesProxy.World.Enums.TBC.EnchantmentSlot.Prop2);
					updateData.ItemData.Enchantment[HermesProxy.World.Enums.Classic.EnchantmentSlot.Prop3] = func(HermesProxy.World.Enums.TBC.EnchantmentSlot.Prop3);
					updateData.ItemData.Enchantment[HermesProxy.World.Enums.Classic.EnchantmentSlot.Prop4] = func(HermesProxy.World.Enums.TBC.EnchantmentSlot.Prop4);
				}
				else
				{
					updateData.ItemData.Enchantment[HermesProxy.World.Enums.Classic.EnchantmentSlot.Perm] = func(HermesProxy.World.Enums.WotLK.EnchantmentSlot.Perm);
					updateData.ItemData.Enchantment[HermesProxy.World.Enums.Classic.EnchantmentSlot.Temp] = func(HermesProxy.World.Enums.WotLK.EnchantmentSlot.Temp);
					updateData.ItemData.Enchantment[HermesProxy.World.Enums.Classic.EnchantmentSlot.Sock1] = func(HermesProxy.World.Enums.WotLK.EnchantmentSlot.Sock1);
					updateData.ItemData.Enchantment[HermesProxy.World.Enums.Classic.EnchantmentSlot.Sock2] = func(HermesProxy.World.Enums.WotLK.EnchantmentSlot.Sock2);
					updateData.ItemData.Enchantment[HermesProxy.World.Enums.Classic.EnchantmentSlot.Sock3] = func(HermesProxy.World.Enums.WotLK.EnchantmentSlot.Sock3);
					updateData.ItemData.Enchantment[HermesProxy.World.Enums.Classic.EnchantmentSlot.Bonus] = func(HermesProxy.World.Enums.WotLK.EnchantmentSlot.Bonus);
					updateData.ItemData.Enchantment[HermesProxy.World.Enums.Classic.EnchantmentSlot.Prismatic] = func(HermesProxy.World.Enums.WotLK.EnchantmentSlot.Prismatic);
					updateData.ItemData.Enchantment[HermesProxy.World.Enums.Classic.EnchantmentSlot.Prop0] = func(HermesProxy.World.Enums.WotLK.EnchantmentSlot.Prop0);
					updateData.ItemData.Enchantment[HermesProxy.World.Enums.Classic.EnchantmentSlot.Prop1] = func(HermesProxy.World.Enums.WotLK.EnchantmentSlot.Prop1);
					updateData.ItemData.Enchantment[HermesProxy.World.Enums.Classic.EnchantmentSlot.Prop2] = func(HermesProxy.World.Enums.WotLK.EnchantmentSlot.Prop2);
					updateData.ItemData.Enchantment[HermesProxy.World.Enums.Classic.EnchantmentSlot.Prop3] = func(HermesProxy.World.Enums.WotLK.EnchantmentSlot.Prop3);
					updateData.ItemData.Enchantment[HermesProxy.World.Enums.Classic.EnchantmentSlot.Prop4] = func(HermesProxy.World.Enums.WotLK.EnchantmentSlot.Prop4);
				}
				uint?[] array = new uint?[3];
				for (int j = 0; j < 3; j++)
				{
					int num = HermesProxy.World.Enums.Classic.EnchantmentSlot.Sock1 + j;
					if (updateData.ItemData.Enchantment[num] != null && updateData.ItemData.Enchantment[num].ID.HasValue)
					{
						uint gemFromEnchantId = GameData.GetGemFromEnchantId((uint)updateData.ItemData.Enchantment[num].ID.Value);
						if (gemFromEnchantId != 0 || updateData.ItemData.Enchantment[num].ID == 0)
						{
							array[j] = gemFromEnchantId;
							updateData.ItemData.HasGemsUpdate = true;
						}
					}
				}
				if (updateData.ItemData.HasGemsUpdate)
				{
					GetSession().GameState.SaveGemsForItem(guid, array);
				}
			}
			int updateField12 = LegacyVersion.GetUpdateField(ItemField.ITEM_FIELD_PROPERTY_SEED);
			if (updateField12 >= 0 && updateMaskArray[updateField12])
			{
				updateData.ItemData.PropertySeed = updates[updateField12].UInt32Value;
			}
			int updateField13 = LegacyVersion.GetUpdateField(ItemField.ITEM_FIELD_RANDOM_PROPERTIES_ID);
			if (updateField13 >= 0 && updateMaskArray[updateField13])
			{
				updateData.ItemData.RandomProperty = updates[updateField13].UInt32Value;
			}
			int updateField14 = LegacyVersion.GetUpdateField(ItemField.ITEM_FIELD_DURABILITY);
			if (updateField14 >= 0 && updateMaskArray[updateField14])
			{
				updateData.ItemData.Durability = updates[updateField14].UInt32Value;
			}
			int updateField15 = LegacyVersion.GetUpdateField(ItemField.ITEM_FIELD_MAXDURABILITY);
			if (updateField15 >= 0 && updateMaskArray[updateField15])
			{
				updateData.ItemData.MaxDurability = updates[updateField15].UInt32Value;
			}
		}
		if (objectType == ObjectType.Container)
		{
			int updateField16 = LegacyVersion.GetUpdateField(ContainerField.CONTAINER_FIELD_NUM_SLOTS);
			if (updateField16 >= 0 && updateMaskArray[updateField16])
			{
				updateData.ContainerData.NumSlots = updates[updateField16].UInt32Value;
			}
			int updateField17 = LegacyVersion.GetUpdateField(ContainerField.CONTAINER_FIELD_SLOT_1);
			if (updateField17 >= 0)
			{
				for (int k = 0; k < 36; k++)
				{
					if (updateMaskArray[updateField17 + k * 2])
					{
						updateData.ContainerData.Slots[k] = GetGuidValue(updates, updateField17 + k * 2).To128(GetSession().GameState);
					}
				}
			}
		}
		if (objectType == ObjectType.Unit || objectType == ObjectType.Player || objectType == ObjectType.ActivePlayer)
		{
			int updateField18 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_CHARM);
			if (updateField18 >= 0 && updateMaskArray[updateField18])
			{
				updateData.UnitData.Charm = GetGuidValue(updates, UnitField.UNIT_FIELD_CHARM).To128(GetSession().GameState);
			}
			int updateField19 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_SUMMON);
			if (updateField19 >= 0 && updateMaskArray[updateField19])
			{
				updateData.UnitData.Summon = GetGuidValue(updates, UnitField.UNIT_FIELD_SUMMON).To128(GetSession().GameState);
			}
			int updateField20 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_CHARMEDBY);
			if (updateField20 >= 0 && updateMaskArray[updateField20])
			{
				updateData.UnitData.CharmedBy = GetGuidValue(updates, UnitField.UNIT_FIELD_CHARMEDBY).To128(GetSession().GameState);
			}
			int updateField21 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_SUMMONEDBY);
			if (updateField21 >= 0 && updateMaskArray[updateField21])
			{
				updateData.UnitData.SummonedBy = GetGuidValue(updates, UnitField.UNIT_FIELD_SUMMONEDBY).To128(GetSession().GameState);
			}
			int updateField22 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_CREATEDBY);
			if (updateField22 >= 0 && updateMaskArray[updateField22])
			{
				updateData.UnitData.CreatedBy = GetGuidValue(updates, UnitField.UNIT_FIELD_CREATEDBY).To128(GetSession().GameState);
			}
			int updateField23 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_TARGET);
			if (updateField23 >= 0 && updateMaskArray[updateField23])
			{
				updateData.UnitData.Target = GetGuidValue(updates, UnitField.UNIT_FIELD_TARGET).To128(GetSession().GameState);
			}
			int updateField24 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_CHANNEL_OBJECT);
			if (updateField24 >= 0 && updateMaskArray[updateField24])
			{
				updateData.UnitData.ChannelObject = GetGuidValue(updates, UnitField.UNIT_FIELD_CHANNEL_OBJECT).To128(GetSession().GameState);
			}
			int updateField25 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_HEALTH);
			if (updateField25 >= 0 && updateMaskArray[updateField25])
			{
				updateData.UnitData.Health = updates[updateField25].Int32Value;
			}
			int updateField26 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_MAXHEALTH);
			if (updateField26 >= 0 && updateMaskArray[updateField26])
			{
				updateData.UnitData.MaxHealth = updates[updateField26].Int32Value;
			}
			int updateField27 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_LEVEL);
			if (updateField27 >= 0 && updateMaskArray[updateField27])
			{
				updateData.UnitData.Level = updates[updateField27].Int32Value;
			}
			int updateField28 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_FACTIONTEMPLATE);
			if (updateField28 >= 0 && updateMaskArray[updateField28])
			{
				updateData.UnitData.FactionTemplate = updates[updateField28].Int32Value;
			}
			int updateField29 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_BYTES_0);
            if (updateField29 >= 0 && updateMaskArray[updateField29])
			{
				// 这里修改有效
				updateData.UnitData.RaceId = (byte)(updates[updateField29].UInt32Value & 0xFFu);
                updateData.UnitData.ClassId = (byte)((updates[updateField29].UInt32Value >> 8) & 0xFFu);
				updateData.UnitData.SexId = (byte)((updates[updateField29].UInt32Value >> 16) & 0xFFu);
				updateData.UnitData.DisplayPower = (byte)((updates[updateField29].UInt32Value >> 24) & 0xFFu);
                if (guid.GetHighType() == HighGuidType.Pet && updateData.UnitData.DisplayPower == 2)
				{
					GetSession().GameState.HunterPetGuids.Add(guid);
				}
				if (objectType == ObjectType.Unit)
				{
					GetSession().GameState.StoreCreatureClass(guid.GetEntry(), (Class)updateData.UnitData.ClassId.Value);
				}
				else
				{
                    // 这里是竞技场派系
                    updateData.PlayerData.ArenaFaction = (byte)(GameData.IsAllianceRace((Race)updateData.UnitData.RaceId.Value) ? 1u : 0u);
				}
			}
			int updateField30 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_POWER1);
			if (updateField30 >= 0)
			{
				for (int l = 0; l < LegacyVersion.GetPowersCount(); l++)
				{
					if (updateMaskArray[updateField30 + l])
					{
						if (powerUpdate != null && (guid == GetSession().GameState.CurrentPlayerGuid || guid == GetSession().GameState.CurrentPetGuid))
						{
							powerUpdate.Powers.Add(new PowerUpdatePower(updates[updateField30 + l].Int32Value, (byte)l));
						}
						sbyte b;
						if (GetSession().GameState.HunterPetGuids.Contains(guid))
						{
							b = ClassPowerTypes.GetPowerSlotForPet((PowerType)l);
						}
						else
						{
							Class classId = ((!updateData.UnitData.ClassId.HasValue) ? GetSession().GameState.GetUnitClass(guid.To128(GetSession().GameState)) : ((Class)updateData.UnitData.ClassId.Value));
							b = ClassPowerTypes.GetPowerSlotForClass(classId, (PowerType)l);
						}
						if (b >= 0)
						{
							updateData.UnitData.Power[b] = updates[updateField30 + l].Int32Value;
						}
					}
				}
			}
			int updateField31 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_MAXPOWER1);
			if (updateField31 >= 0)
			{
				for (int m = 0; m < LegacyVersion.GetPowersCount(); m++)
				{
					if (!updateMaskArray[updateField31 + m])
					{
						continue;
					}
					Class classId2 = ((!updateData.UnitData.ClassId.HasValue) ? GetSession().GameState.GetUnitClass(guid.To128(GetSession().GameState)) : ((Class)updateData.UnitData.ClassId.Value));
					sbyte b2 = ((!GetSession().GameState.HunterPetGuids.Contains(guid)) ? ClassPowerTypes.GetPowerSlotForClass(classId2, (PowerType)m) : ClassPowerTypes.GetPowerSlotForPet((PowerType)m));
					if (b2 >= 0)
					{
						updateData.UnitData.MaxPower[b2] = updates[updateField31 + m].Int32Value;
					}
					if (m == 3)
					{
						b2 = ClassPowerTypes.GetPowerSlotForClass(classId2, PowerType.ComboPoints);
						if (b2 >= 0)
						{
							updateData.UnitData.MaxPower[b2] = 5;
						}
					}
				}
			}
			int updateField32 = LegacyVersion.GetUpdateField(UnitField.UNIT_VIRTUAL_ITEM_SLOT_DISPLAY);
			if (updateField32 >= 0)
			{
				for (int n = 0; n < 3; n++)
				{
					if (updateMaskArray[updateField32 + n])
					{
						uint itemIdWithDisplayId = GameData.GetItemIdWithDisplayId(updates[updateField32 + n].UInt32Value);
						if (itemIdWithDisplayId != 0)
						{
							VisibleItem visibleItem = new VisibleItem();
							visibleItem.ItemID = (int)itemIdWithDisplayId;
							updateData.UnitData.VirtualItems[n] = visibleItem;
						}
					}
				}
			}
			int updateField33 = LegacyVersion.GetUpdateField(UnitField.UNIT_VIRTUAL_ITEM_SLOT_ID);
			if (updateField33 >= 0)
			{
				for (int num2 = 0; num2 < 3; num2++)
				{
					if (updateMaskArray[updateField33 + num2])
					{
						VisibleItem visibleItem2 = new VisibleItem();
						visibleItem2.ItemID = updates[updateField33 + num2].Int32Value;
						updateData.UnitData.VirtualItems[num2] = visibleItem2;
					}
				}
			}
			int updateField34 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_FLAGS);
			if (updateField34 >= 0 && updateMaskArray[updateField34])
			{
				if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
				{
					UnitFlagsVanilla uInt32Value = (UnitFlagsVanilla)updates[updateField34].UInt32Value;
					updateData.UnitData.Flags = (uint)uInt32Value.CastFlags<UnitFlags>();
					if (uInt32Value.HasAnyFlag(UnitFlagsVanilla.PetRename))
					{
						if (!updateData.UnitData.PetFlags.HasValue)
						{
							updateData.UnitData.PetFlags = 1;
						}
						else
						{
							UnitData unitData = updateData.UnitData;
							unitData.PetFlags |= 1;
						}
					}
					if (uInt32Value.HasAnyFlag(UnitFlagsVanilla.PetAbandon))
					{
						if (!updateData.UnitData.PetFlags.HasValue)
						{
							updateData.UnitData.PetFlags = 2;
						}
						else
						{
							UnitData unitData2 = updateData.UnitData;
							unitData2.PetFlags |= 2;
						}
					}
				}
				else
				{
					updateData.UnitData.Flags = updates[updateField34].UInt32Value;
				}
				if (updateData.UnitData.Flags.HasAnyFlag(UnitFlags.ServerControlled) && isCreate && guid == GetSession().GameState.CurrentPlayerGuid && updateData.CreateData.MoveSpline == null)
				{
					updateData.UnitData.Flags &= 4294967294u;
				}
				if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056) && !updateData.UnitData.PvpFlags.HasValue)
				{
					updateData.UnitData.PvpFlags = ReadPvPFlags(updates);
				}
			}
			int updateField35 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_FLAGS_2);
			if (updateField35 >= 0 && updateMaskArray[updateField35])
			{
				updateData.UnitData.Flags2 = updates[updateField35].UInt32Value;
			}
			int updateField36 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_AURASTATE);
			if (updateField36 >= 0 && updateMaskArray[updateField36])
			{
				updateData.UnitData.AuraState = updates[updateField36].UInt32Value;
			}
			int updateField37 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_BASEATTACKTIME);
			if (updateField37 >= 0)
			{
				for (int num3 = 0; num3 < 2; num3++)
				{
					if (updateMaskArray[updateField37 + num3])
					{
						updateData.UnitData.AttackRoundBaseTime[num3] = updates[updateField37 + num3].UInt32Value;
					}
				}
			}
			int updateField38 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_RANGEDATTACKTIME);
			if (updateField38 >= 0 && updateMaskArray[updateField38])
			{
				updateData.UnitData.RangedAttackRoundBaseTime = updates[updateField38].UInt32Value;
			}
			int updateField39 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_BOUNDINGRADIUS);
			if (updateField39 >= 0 && updateMaskArray[updateField39])
			{
				updateData.UnitData.BoundingRadius = updates[updateField39].FloatValue;
			}
			int updateField40 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_COMBATREACH);
			if (updateField40 >= 0 && updateMaskArray[updateField40])
			{
				updateData.UnitData.CombatReach = updates[updateField40].FloatValue;
			}
			int updateField41 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_DISPLAYID);
			if (updateField41 >= 0 && updateMaskArray[updateField41])
			{
                // 生物id
                updateData.UnitData.DisplayID = updates[updateField41].Int32Value;
				if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
				{
					updateData.UnitData.DisplayScale = 1f / GameData.GetUnitCompleteDisplayScale((uint)updateData.UnitData.DisplayID.Value);
				}
			}
			int updateField42 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_NATIVEDISPLAYID); //132
			if (updateField42 >= 0 && updateMaskArray[updateField42])
			{
				// 原生显示ID 人类男49 人类女50 暗夜男55
				updateData.UnitData.NativeDisplayID = updates[updateField42].Int32Value;
            }
			int updateField43 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_MOUNTDISPLAYID); //133
			if (updateField43 >= 0 && updateMaskArray[updateField43])
			{
				// 挂载显示ID
				updateData.UnitData.MountDisplayID = updates[updateField43].Int32Value;
			}
			int updateField44 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_MINDAMAGE);
			if (updateField44 >= 0 && updateMaskArray[updateField44])
			{
				updateData.UnitData.MinDamage = updates[updateField44].FloatValue;
			}
			int updateField45 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_MAXDAMAGE);
			if (updateField45 >= 0 && updateMaskArray[updateField45])
			{
				updateData.UnitData.MaxDamage = updates[updateField45].FloatValue;
			}
			int updateField46 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_MINOFFHANDDAMAGE);
			if (updateField46 >= 0 && updateMaskArray[updateField46])
			{
				updateData.UnitData.MinOffHandDamage = updates[updateField46].FloatValue;
			}
			int updateField47 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_MAXOFFHANDDAMAGE);
			if (updateField47 >= 0 && updateMaskArray[updateField47])
			{
				updateData.UnitData.MaxOffHandDamage = updates[updateField47].FloatValue;
			}
			int updateField48 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_BYTES_1);
			if (updateField48 >= 0 && updateMaskArray[updateField48])
			{
				updateData.UnitData.StandState = (byte)(updates[updateField48].UInt32Value & 0xFFu);
				byte b3 = (byte)((updates[updateField48].UInt32Value >> 8) & 0xFFu);
				if (b3 != 238)
				{
					updateData.UnitData.PetLoyaltyIndex = b3;
				}
				if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_4_0_8089))
				{
					updateData.UnitData.VisFlags = (byte)((updates[updateField48].UInt32Value >> 16) & 0xFFu);
					updateData.UnitData.AnimTier = (byte)((updates[updateField48].UInt32Value >> 24) & 0xFFu);
				}
				else
				{
					updateData.UnitData.ShapeshiftForm = (byte)((updates[updateField48].UInt32Value >> 16) & 0xFFu);
					updateData.UnitData.VisFlags = (byte)((updates[updateField48].UInt32Value >> 24) & 0xFFu);
				}
			}
			int updateField49 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_PETNUMBER);
			if (updateField49 >= 0 && updateMaskArray[updateField49])
			{
				updateData.UnitData.PetNumber = updates[updateField49].UInt32Value;
			}
			int updateField50 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_PET_NAME_TIMESTAMP);
			if (updateField50 >= 0 && updateMaskArray[updateField50])
			{
				updateData.UnitData.PetNameTimestamp = updates[updateField50].UInt32Value;
			}
			int updateField51 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_PETEXPERIENCE);
			if (updateField51 >= 0 && updateMaskArray[updateField51])
			{
				updateData.UnitData.PetExperience = updates[updateField51].UInt32Value;
			}
			int updateField52 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_PETNEXTLEVELEXP);
			if (updateField52 >= 0 && updateMaskArray[updateField52])
			{
				updateData.UnitData.PetNextLevelExperience = updates[updateField52].UInt32Value;
			}
			int updateField53 = LegacyVersion.GetUpdateField(UnitField.UNIT_DYNAMIC_FLAGS);
			if (updateField53 >= 0 && updateMaskArray[updateField53])
			{
				UnitDynamicFlagsLegacy unitDynamicFlagsLegacy = (UnitDynamicFlagsLegacy)updates[updateField53].UInt32Value;
				if (unitDynamicFlagsLegacy.HasFlag(UnitDynamicFlagsLegacy.Tapped) && unitDynamicFlagsLegacy.HasFlag(UnitDynamicFlagsLegacy.TappedByPlayer))
				{
					unitDynamicFlagsLegacy &= ~(UnitDynamicFlagsLegacy.Tapped | UnitDynamicFlagsLegacy.TappedByPlayer);
				}
				updateData.ObjectData.DynamicFlags = (uint)unitDynamicFlagsLegacy.CastFlags<UnitDynamicFlagsModern>();
				if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
				{
					if (!updateData.UnitData.Flags2.HasValue)
					{
						updateData.UnitData.Flags2 = 2048u;
					}
					if (unitDynamicFlagsLegacy.HasAnyFlag(UnitDynamicFlagsLegacy.AppearDead))
					{
						updateData.UnitData.Flags2 |= 1u;
					}
				}
			}
			int updateField54 = LegacyVersion.GetUpdateField(UnitField.UNIT_CHANNEL_SPELL);
			if (updateField54 >= 0 && updateMaskArray[updateField54])
			{
				UnitChannel unitChannel = new UnitChannel();
				unitChannel.SpellID = updates[updateField54].Int32Value;
				unitChannel.SpellXSpellVisualID = (int)GameData.GetSpellVisual((uint)unitChannel.SpellID);
				updateData.UnitData.ChannelData = unitChannel;
			}
			int updateField55 = LegacyVersion.GetUpdateField(UnitField.UNIT_MOD_CAST_SPEED);
			if (updateField55 >= 0 && updateMaskArray[updateField55])
			{
				updateData.UnitData.ModCastSpeed = updates[updateField55].FloatValue;
			}
			int updateField56 = LegacyVersion.GetUpdateField(UnitField.UNIT_CREATED_BY_SPELL);
			if (updateField56 >= 0 && updateMaskArray[updateField56])
			{
				updateData.UnitData.CreatedBySpell = updates[updateField56].Int32Value;
				if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180) && isCreate && updateData.UnitData.CreatedBy == GetSession().GameState.CurrentPlayerGuid)
				{
					int totemSlotForSpell = GameData.GetTotemSlotForSpell((uint)updateData.UnitData.CreatedBySpell.Value);
					if (totemSlotForSpell >= 0)
					{
						TotemCreated totemCreated = new TotemCreated();
						totemCreated.Slot = (byte)totemSlotForSpell;
						totemCreated.Totem = guid;
						totemCreated.Duration = 120000u;
						totemCreated.SpellId = (uint)updateData.UnitData.CreatedBySpell.Value;
						totemCreated.CannotDismiss = true;
						SendPacketToClient(totemCreated);
					}
				}
			}
			int updateField57 = LegacyVersion.GetUpdateField(UnitField.UNIT_NPC_FLAGS);
			if (updateField57 >= 0 && updateMaskArray[updateField57])
			{
				if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
				{
					NPCFlagsVanilla uInt32Value2 = (NPCFlagsVanilla)updates[updateField57].UInt32Value;
					updateData.UnitData.NpcFlags[0] = (uint)uInt32Value2.CastFlags<NPCFlags>();
				}
				else
				{
					updateData.UnitData.NpcFlags[0] = updates[updateField57].UInt32Value;
				}
			}
			int updateField58 = LegacyVersion.GetUpdateField(UnitField.UNIT_NPC_EMOTESTATE);
			if (updateField58 >= 0 && updateMaskArray[updateField58])
			{
				updateData.UnitData.EmoteState = updates[updateField58].Int32Value;
			}
			int updateField59 = LegacyVersion.GetUpdateField(UnitField.UNIT_TRAINING_POINTS);
			if (updateField59 >= 0 && updateMaskArray[updateField59])
			{
				updateData.UnitData.TrainingPointsUsed = (ushort)(updates[updateField59].UInt32Value & 0xFFFFu);
				updateData.UnitData.TrainingPointsTotal = (ushort)((updates[updateField59].UInt32Value >> 16) & 0xFFFFu);
			}
			int updateField60 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_STAT0);
			if (updateField60 >= 0)
			{
				for (int num4 = 0; num4 < 5; num4++)
				{
					if (updateMaskArray[updateField60 + num4])
					{
						updateData.UnitData.Stats[num4] = updates[updateField60 + num4].Int32Value;
					}
				}
			}
			int updateField61 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_POSSTAT0);
			if (updateField61 >= 0)
			{
				for (int num5 = 0; num5 < 5; num5++)
				{
					if (updateMaskArray[updateField61 + num5])
					{
						updateData.UnitData.StatPosBuff[num5] = updates[updateField61 + num5].Int32Value;
					}
				}
			}
			int updateField62 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_NEGSTAT0);
			if (updateField62 >= 0)
			{
				for (int num6 = 0; num6 < 5; num6++)
				{
					if (updateMaskArray[updateField62 + num6])
					{
						updateData.UnitData.StatNegBuff[num6] = updates[updateField62 + num6].Int32Value;
					}
				}
			}
			int updateField63 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_RESISTANCES);
			if (updateField63 >= 0)
			{
				for (int num7 = 0; num7 < 7; num7++)
				{
					if (updateMaskArray[updateField63 + num7])
					{
						updateData.UnitData.Resistances[num7] = updates[updateField63 + num7].Int32Value;
					}
				}
			}
			int updateField64 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE);
			if (updateField64 >= 0)
			{
				for (int num8 = 0; num8 < 7; num8++)
				{
					if (updateMaskArray[updateField64 + num8])
					{
						updateData.UnitData.ResistanceBuffModsPositive[num8] = updates[updateField64 + num8].Int32Value;
					}
				}
			}
			int updateField65 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE);
			if (updateField65 >= 0)
			{
				for (int num9 = 0; num9 < 7; num9++)
				{
					if (updateMaskArray[updateField65 + num9])
					{
						updateData.UnitData.ResistanceBuffModsNegative[num9] = updates[updateField65 + num9].Int32Value;
					}
				}
			}
			int updateField66 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_BASE_MANA);
			if (updateField66 >= 0 && updateMaskArray[updateField66])
			{
				updateData.UnitData.BaseMana = updates[updateField66].Int32Value;
			}
			int updateField67 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_BASE_HEALTH);
			if (updateField67 >= 0 && updateMaskArray[updateField67])
			{
				updateData.UnitData.BaseHealth = updates[updateField67].Int32Value;
			}
			int updateField68 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_BYTES_2);
			if (updateField68 >= 0 && updateMaskArray[updateField68])
			{
				updateData.UnitData.SheatheState = (byte)(updates[updateField68].UInt32Value & 0xFFu);
				if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
				{
					updateData.UnitData.PvpFlags = (byte)((updates[updateField68].UInt32Value >> 8) & 0xFFu);
				}
				if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
				{
					updateData.UnitData.PetFlags = (byte)((updates[updateField68].UInt32Value >> 16) & 0xFFu);
				}
				if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_4_0_8089))
				{
					updateData.UnitData.ShapeshiftForm = (byte)((updates[updateField68].UInt32Value >> 24) & 0xFFu);
				}
			}
			int updateField69 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_ATTACK_POWER);
			if (updateField69 >= 0 && updateMaskArray[updateField69])
			{
				updateData.UnitData.AttackPower = updates[updateField69].Int32Value;
			}
			int updateField70 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_ATTACK_POWER_MODS);
			if (updateField70 >= 0 && updateMaskArray[updateField70])
			{
				updateData.UnitData.AttackPowerModNeg = updates[updateField70].Int32Value & 0xFFFF;
				updateData.UnitData.AttackPowerModPos = (updates[updateField70].Int32Value >> 16) & 0xFFFF;
			}
			int updateField71 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_RANGED_ATTACK_POWER_MULTIPLIER);
			if (updateField71 >= 0 && updateMaskArray[updateField71])
			{
				updateData.UnitData.AttackPowerMultiplier = updates[updateField71].FloatValue;
			}
			int updateField72 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_MINRANGEDDAMAGE);
			if (updateField72 >= 0 && updateMaskArray[updateField72])
			{
				updateData.UnitData.MinRangedDamage = updates[updateField72].FloatValue;
			}
			int updateField73 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_MAXRANGEDDAMAGE);
			if (updateField73 >= 0 && updateMaskArray[updateField73])
			{
				updateData.UnitData.MaxRangedDamage = updates[updateField73].FloatValue;
			}
			int updateField74 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_POWER_COST_MODIFIER);
			if (updateField74 >= 0)
			{
				for (int num10 = 0; num10 < 7; num10++)
				{
					if (updateMaskArray[updateField74 + num10])
					{
						updateData.UnitData.PowerCostModifier[num10] = updates[updateField74 + num10].Int32Value;
					}
				}
			}
			int updateField75 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_POWER_COST_MULTIPLIER);
			if (updateField75 >= 0)
			{
				for (int num11 = 0; num11 < 7; num11++)
				{
					if (updateMaskArray[updateField75 + num11])
					{
						updateData.UnitData.PowerCostMultiplier[num11] = updates[updateField75 + num11].FloatValue;
					}
				}
			}
			int updateField76 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_MAXHEALTHMODIFIER);
			if (updateField76 >= 0 && updateMaskArray[updateField76])
			{
				updateData.UnitData.MaxHealthModifier = updates[updateField76].FloatValue;
			}
			int updateField77 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_AURA);
			int updateField78 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_AURAFLAGS);
			int updateField79 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_AURALEVELS);
			int updateField80 = LegacyVersion.GetUpdateField(UnitField.UNIT_FIELD_AURAAPPLICATIONS);
			if (updateField77 > 0 && updateField78 > 0 && updateField79 > 0 && updateField80 > 0)
			{
				int auraSlotsCount = LegacyVersion.GetAuraSlotsCount();
				for (byte b4 = 0; b4 < auraSlotsCount; b4++)
				{
					if (updateMaskArray[updateField77 + b4] || updateMaskArray[updateField79 + b4 / 4] || updateMaskArray[updateField80 + b4 / 4])
					{
						AuraInfo auraInfo = default(AuraInfo);
						auraInfo.Slot = b4;
						auraInfo.AuraData = ReadAuraSlot(b4, guid, updates);
						if (auraInfo.AuraData != null)
						{
							GetSession().GameState.GetAuraDuration(guid, b4, out var left, out var full);
							if (left > 0 && full > 0)
							{
								auraInfo.AuraData.Flags |= AuraFlagsModern.Duration;
								auraInfo.AuraData.Duration = full;
								auraInfo.AuraData.Remaining = left;
							}
							auraInfo.AuraData.CastUnit = GetSession().GameState.GetAuraCaster(guid, b4, auraInfo.AuraData.SpellID);
						}
						else if (updateMaskArray[updateField77 + b4])
						{
							GetSession().GameState.ClearAuraDuration(guid, b4);
							GetSession().GameState.ClearAuraCaster(guid, b4);
						}
						if (auraInfo.AuraData != null || updateMaskArray[updateField77 + b4])
						{
							auraUpdate.Auras.Add(auraInfo);
						}
					}
				}
			}
		}
		if (objectType == ObjectType.Player || objectType == ObjectType.ActivePlayer)
		{
			int updateField81 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_DUEL_ARBITER);
			if (updateField81 >= 0 && updateMaskArray[updateField81])
			{
				updateData.PlayerData.DuelArbiter = GetGuidValue(updates, PlayerField.PLAYER_DUEL_ARBITER).To128(GetSession().GameState);
			}
			int updateField82 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FLAGS);
			if (updateField82 >= 0 && updateMaskArray[updateField82])
			{
				uint uInt32Value3 = updates[updateField82].UInt32Value;
				PlayerFlags flags = ((PlayerFlagsLegacy)uInt32Value3).CastFlags<PlayerFlags>();
				if (updateData.Guid == GetSession().GameState.CurrentPlayerGuid)
				{
					GetSession().GameState.CurrentPlayerStorage.Settings.PatchFlags(ref flags);
				}
				updateData.PlayerData.PlayerFlags = (uint)flags;
				if (!updateData.PlayerData.PlayerFlagsEx.HasValue)
				{
					updateData.PlayerData.PlayerFlagsEx = 0u;
				}
				if (((PlayerFlagsLegacy)uInt32Value3).HasAnyFlag(PlayerFlagsLegacy.HideHelm))
				{
					updateData.PlayerData.PlayerFlagsEx |= 128u;
				}
				if (((PlayerFlagsLegacy)uInt32Value3).HasAnyFlag(PlayerFlagsLegacy.HideCloak))
				{
					updateData.PlayerData.PlayerFlagsEx |= 256u;
				}
				if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_0_2_9056) && !updateData.UnitData.PvpFlags.HasValue)
				{
					updateData.UnitData.PvpFlags = ReadPvPFlags(updates);
				}
			}
			else if (updateData.Guid == GetSession().GameState.CurrentPlayerGuid && GetSession().GameState.CurrentPlayerStorage.Settings.NeedToForcePatchFlags)
			{
				PlayerFlags value = GetSession().GameState.CurrentPlayerStorage.Settings.CreateNewFlags();
				updateData.PlayerData.PlayerFlags = (uint)value;
			}
			int updateField83 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_GUILDID);
			if (updateField83 >= 0 && updateMaskArray[updateField83])
			{
				GetSession().GameState.StorePlayerGuildId(guid, updates[updateField83].UInt32Value);
				updateData.UnitData.GuildGUID = WowGuid128.Create(HighGuidType703.Guild, updates[updateField83].UInt32Value);
			}
			int updateField84 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_GUILDRANK);
			if (updateField84 >= 0 && updateMaskArray[updateField84])
			{
				updateData.PlayerData.GuildLevel = 25;
				updateData.PlayerData.GuildRankID = updates[updateField84].UInt32Value;
			}
			int updateField85 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_GUILD_TIMESTAMP);
			if (updateField85 >= 0 && updateMaskArray[updateField85])
			{
				updateData.PlayerData.GuildTimeStamp = updates[updateField85].Int32Value;
			}
			if (LegacyVersion.GetUpdateField(PlayerField.PLAYER_QUEST_LOG_1_1) >= 0)
			{
				int questLogSize = LegacyVersion.GetQuestLogSize();
				for (int num12 = 0; num12 < questLogSize; num12++)
				{
					updateData.PlayerData.QuestLog[num12] = ReadQuestLogEntry(num12, updateMaskArray, updates);
				}
			}
			int updateField86 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_CHOSEN_TITLE);
			if (updateField86 >= 0 && updateMaskArray[updateField86])
			{
				updateData.PlayerData.ChosenTitle = updates[updateField86].Int32Value;
			}
			int updateField87 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_VISIBLE_ITEM_1_0);
			if (updateField87 >= 0)
			{
				int num13 = (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180) ? 16 : 12);
				for (int num14 = 0; num14 < 19; num14++)
				{
					int num15 = updateField87 + num14 * num13;
					int num16 = updateField87 + 1 + num14 * num13;
					if (updateMaskArray[num15] || updateMaskArray[num16])
					{
						updateData.PlayerData.VisibleItems[num14] = new VisibleItem();
						if (updates.ContainsKey(num15))
						{
							updateData.PlayerData.VisibleItems[num14].ItemID = updates[num15].Int32Value;
						}
						if (updates.ContainsKey(num16))
						{
							updateData.PlayerData.VisibleItems[num14].ItemVisual = (ushort)GameData.GetItemEnchantVisual(updates[num16].UInt32Value);
						}
					}
				}
			}
			int updateField88 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_VISIBLE_ITEM_1_ENTRYID);
			if (updateField88 >= 0)
			{
				int num17 = 2;
				for (int num18 = 0; num18 < 19; num18++)
				{
					if (updateMaskArray[updateField88 + num18 * num17])
					{
						updateData.PlayerData.VisibleItems[num18] = new VisibleItem();
						updateData.PlayerData.VisibleItems[num18].ItemID = updates[updateField88 + num18 * num17].Int32Value;
					}
				}
			}
			int updateField89 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_INV_SLOT_HEAD);
			if (updateField89 >= 0)
			{
				for (int num19 = 0; num19 < 23; num19++)
				{
					if (updateMaskArray[updateField89 + num19 * 2])
					{
						updateData.ActivePlayerData.InvSlots[num19] = GetGuidValue(updates, updateField89 + num19 * 2).To128(GetSession().GameState);
					}
				}
			}
			int updateField90 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_PACK_SLOT_1);
			if (updateField90 >= 0)
			{
				for (int num20 = 0; num20 < 16; num20++)
				{
					if (updateMaskArray[updateField90 + num20 * 2])
					{
						updateData.ActivePlayerData.PackSlots[num20] = GetGuidValue(updates, updateField90 + num20 * 2).To128(GetSession().GameState);
					}
				}
			}
			int updateField91 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_BANK_SLOT_1);
			if (updateField91 >= 0)
			{
				int num21 = (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180) ? 28 : 24);
				for (int num22 = 0; num22 < num21; num22++)
				{
					if (updateMaskArray[updateField91 + num22 * 2])
					{
						updateData.ActivePlayerData.BankSlots[num22] = GetGuidValue(updates, updateField91 + num22 * 2).To128(GetSession().GameState);
					}
				}
			}
			int updateField92 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_BANKBAG_SLOT_1);
			if (updateField92 >= 0)
			{
				int num23 = (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180) ? 7 : 6);
				for (int num24 = 0; num24 < num23; num24++)
				{
					if (updateMaskArray[updateField92 + num24 * 2])
					{
						updateData.ActivePlayerData.BankBagSlots[num24] = GetGuidValue(updates, updateField92 + num24 * 2).To128(GetSession().GameState);
					}
				}
			}
			int updateField93 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_VENDORBUYBACK_SLOT_1);
			if (updateField93 >= 0)
			{
				for (int num25 = 0; num25 < 12; num25++)
				{
					if (updateMaskArray[updateField93 + num25 * 2])
					{
						updateData.ActivePlayerData.BuyBackSlots[num25] = GetGuidValue(updates, updateField93 + num25 * 2).To128(GetSession().GameState);
					}
				}
			}
			int updateField94 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_KEYRING_SLOT_1);
			if (updateField94 >= 0)
			{
				for (int num26 = 0; num26 < 32; num26++)
				{
					if (updateMaskArray[updateField94 + num26 * 2])
					{
						updateData.ActivePlayerData.KeyringSlots[num26] = GetGuidValue(updates, updateField94 + num26 * 2).To128(GetSession().GameState);
					}
				}
			}
			byte? b5 = null;
			byte? b6 = null;
			byte? b7 = null;
			byte? b8 = null;
			byte? b9 = null;
			int updateField95 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_BYTES);
			if (updateField95 >= 0 && updateMaskArray[updateField95])
			{
				b5 = (byte)(updates[updateField95].UInt32Value & 0xFFu);
				b6 = (byte)((updates[updateField95].UInt32Value >> 8) & 0xFFu);
				b7 = (byte)((updates[updateField95].UInt32Value >> 16) & 0xFFu);
				b8 = (byte)((updates[updateField95].UInt32Value >> 24) & 0xFFu);
			}
			RestInfo restInfo = ((isCreate && guid == GetSession().GameState.CurrentPlayerGuid) ? new RestInfo() : null);
			if (restInfo != null)
			{
				restInfo.StateID = 2u;
			}
			int updateField96 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_BYTES_2); //194
			if (updateField96 >= 0 && updateMaskArray[updateField96])
			{
				b9 = (byte)(updates[updateField96].UInt32Value & 0xFFu);
				updateData.PlayerData.NumBankSlots = (byte)((updates[updateField96].UInt32Value >> 16) & 0xFFu);
				if (restInfo == null && guid == GetSession().GameState.CurrentPlayerGuid)
				{
					restInfo = new RestInfo();
				}
				if (restInfo != null)
				{
					restInfo.StateID = (byte)((updates[updateField96].UInt32Value >> 24) & 0xFFu);
				}
			}
			if (b5.HasValue && b6.HasValue && b7.HasValue && b8.HasValue && b9.HasValue)
			{
				Race race = Race.None;
				Gender gender = Gender.None;
				if (updateData.UnitData.RaceId.HasValue)
				{
					race = (Race)updateData.UnitData.RaceId.Value;
                }
				if (updateData.UnitData.SexId.HasValue)
				{
					gender = (Gender)updateData.UnitData.SexId.Value;
                }
				if ((race == Race.None || gender == Gender.None) && GetSession().GameState.CachedPlayers.TryGetValue(guid.To128(GetSession().GameState), out var value2))
				{
					race = value2.RaceId;
					gender = value2.SexId;
				}
				if (race != 0 && gender != Gender.None)
				{
					Array<ChrCustomizationChoice> array2 = CharacterCustomizations.ConvertLegacyCustomizationsToModern(race, gender, b5.Value, b6.Value, b7.Value, b8.Value, b9.Value);
					for (int num27 = 0; num27 < 5; num27++)
					{
						updateData.PlayerData.Customizations[num27] = array2[num27];
					}
				}
			}
			int updateField97 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_REST_STATE_EXPERIENCE);
			if (updateField97 >= 0 && updateMaskArray[updateField97])
			{
				if (restInfo == null && guid == GetSession().GameState.CurrentPlayerGuid)
				{
					restInfo = new RestInfo();
				}
				if (restInfo != null)
				{
					restInfo.Threshold = updates[updateField97].UInt32Value;
				}
			}
			if (restInfo != null)
			{
				updateData.ActivePlayerData.RestInfo[0] = restInfo;
			}
			int updateField98 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_BYTES_3);
			if (updateField98 >= 0 && updateMaskArray[updateField98])
			{
				ushort num28 = (ushort)(updates[updateField98].UInt32Value & 0xFFFFu);
				updateData.PlayerData.NativeSex = (byte)(num28 & 1u);
				updateData.PlayerData.Inebriation = (byte)(num28 & 0xFFFEu);
				updateData.PlayerData.PvpTitle = (byte)((updates[updateField98].UInt32Value >> 16) & 0xFFu);
				updateData.PlayerData.PvPRank = (byte)((updates[updateField98].UInt32Value >> 24) & 0xFFu);
			}
			int updateField99 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_DUEL_TEAM);
			if (updateField99 >= 0 && updateMaskArray[updateField99])
			{
				updateData.PlayerData.DuelTeam = updates[updateField99].UInt32Value;
			}
			int updateField100 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FARSIGHT);
			if (updateField100 >= 0 && updateMaskArray[updateField100])
			{
				updateData.ActivePlayerData.FarsightObject = GetGuidValue(updates, PlayerField.PLAYER_FARSIGHT).To128(GetSession().GameState);
			}
			int updateField101 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_COMBO_TARGET);
			if (updateField101 >= 0 && updateMaskArray[updateField101])
			{
				updateData.ActivePlayerData.ComboTarget = GetGuidValue(updates, PlayerField.PLAYER_FIELD_COMBO_TARGET).To128(GetSession().GameState);
			}
			int updateField102 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_KNOWN_TITLES);
			if (updateField102 >= 0)
			{
				int num29 = (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056) ? 3 : 2);
				for (int num30 = 0; num30 < num29; num30++)
				{
					if (updateMaskArray[updateField102 + num30])
					{
						updateData.ActivePlayerData.KnownTitles[num30] = updates[updateField102 + num30].UInt32Value;
					}
				}
			}
			int updateField103 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_XP);
			if (updateField103 >= 0 && updateMaskArray[updateField103])
			{
				updateData.ActivePlayerData.XP = updates[updateField103].Int32Value;
			}
			int updateField104 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_NEXT_LEVEL_XP);
			if (updateField104 >= 0 && updateMaskArray[updateField104])
			{
				updateData.ActivePlayerData.NextLevelXP = updates[updateField104].Int32Value;
			}
			int updateField105 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_SKILL_INFO_1_1);
			if (updateField105 >= 0)
			{
				for (int num31 = 0; num31 < 128; num31++)
				{
					int num32 = updateField105 + num31 * 3;
					if (updateMaskArray[num32])
					{
						updateData.ActivePlayerData.Skill.SkillLineID[num31] = (ushort)(updates[num32].UInt32Value & 0xFFFFu);
						updateData.ActivePlayerData.Skill.SkillStep[num31] = (ushort)((updates[num32].UInt32Value >> 16) & 0xFFFFu);
					}
					int num33 = num32 + 1;
					if (updateMaskArray[num33])
					{
						updateData.ActivePlayerData.Skill.SkillRank[num31] = (ushort)(updates[num33].UInt32Value & 0xFFFFu);
						updateData.ActivePlayerData.Skill.SkillMaxRank[num31] = (ushort)((updates[num33].UInt32Value >> 16) & 0xFFFFu);
					}
					int num34 = num33 + 1;
					if (updateMaskArray[num34])
					{
						updateData.ActivePlayerData.Skill.SkillTempBonus[num31] = (short)(updates[num34].Int32Value & 0xFFFF);
						updateData.ActivePlayerData.Skill.SkillPermBonus[num31] = (ushort)((updates[num34].UInt32Value >> 16) & 0xFFFFu);
					}
				}
			}
			int updateField106 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_CHARACTER_POINTS1);
			if (updateField106 >= 0 && updateMaskArray[updateField106])
			{
				updateData.ActivePlayerData.CharacterPoints = updates[updateField106].Int32Value;
			}
			int updateField107 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_TRACK_CREATURES);
			if (updateField107 >= 0 && updateMaskArray[updateField107])
			{
				updateData.ActivePlayerData.TrackCreatureMask = updates[updateField107].UInt32Value;
			}
			int updateField108 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_TRACK_RESOURCES);
			if (updateField108 >= 0 && updateMaskArray[updateField108])
			{
				updateData.ActivePlayerData.TrackResourceMask[0] = updates[updateField108].UInt32Value;
			}
			int updateField109 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_BLOCK_PERCENTAGE);
			if (updateField109 >= 0 && updateMaskArray[updateField109])
			{
				updateData.ActivePlayerData.BlockPercentage = updates[updateField109].FloatValue;
			}
			int updateField110 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_DODGE_PERCENTAGE);
			if (updateField110 >= 0 && updateMaskArray[updateField110])
			{
				updateData.ActivePlayerData.DodgePercentage = updates[updateField110].FloatValue;
			}
			int updateField111 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_PARRY_PERCENTAGE);
			if (updateField111 >= 0 && updateMaskArray[updateField111])
			{
				updateData.ActivePlayerData.ParryPercentage = updates[updateField111].FloatValue;
			}
			int updateField112 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_EXPERTISE);
			if (updateField112 >= 0 && updateMaskArray[updateField112])
			{
				updateData.ActivePlayerData.MainhandExpertise = updates[updateField112].Int32Value;
			}
			int updateField113 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_OFFHAND_EXPERTISE);
			if (updateField113 >= 0 && updateMaskArray[updateField113])
			{
				updateData.ActivePlayerData.OffhandExpertise = updates[updateField113].Int32Value;
			}
			int updateField114 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_CRIT_PERCENTAGE);
			if (updateField114 >= 0 && updateMaskArray[updateField114])
			{
				updateData.ActivePlayerData.CritPercentage = updates[updateField114].FloatValue;
			}
			int updateField115 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_RANGED_CRIT_PERCENTAGE);
			if (updateField115 >= 0 && updateMaskArray[updateField115])
			{
				updateData.ActivePlayerData.RangedCritPercentage = updates[updateField115].FloatValue;
			}
			int updateField116 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_OFFHAND_CRIT_PERCENTAGE);
			if (updateField116 >= 0 && updateMaskArray[updateField116])
			{
				updateData.ActivePlayerData.OffhandCritPercentage = updates[updateField116].FloatValue;
			}
			int updateField117 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_SPELL_CRIT_PERCENTAGE1);
			if (updateField117 >= 0)
			{
				for (int num35 = 0; num35 < 7; num35++)
				{
					if (updateMaskArray[updateField117 + num35])
					{
						updateData.ActivePlayerData.SpellCritPercentage[num35] = updates[updateField117 + num35].FloatValue;
					}
				}
			}
			int updateField118 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_SHIELD_BLOCK);
			if (updateField118 >= 0 && updateMaskArray[updateField118])
			{
				updateData.ActivePlayerData.ShieldBlock = updates[updateField118].Int32Value;
			}
			int updateField119 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_EXPLORED_ZONES_1);
			if (updateField119 >= 0)
			{
				int num36 = (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180) ? 128 : 64);
				for (int num37 = 0; num37 < num36; num37++)
				{
					if (updateMaskArray[updateField119 + num37])
					{
						if (((uint)num37 & (true ? 1u : 0u)) != 0)
						{
							ulong num38 = (updateData.ActivePlayerData.ExploredZones[num37 / 2].HasValue ? updateData.ActivePlayerData.ExploredZones[num37 / 2].Value : 0);
							updateData.ActivePlayerData.ExploredZones[num37 / 2] = num38 | ((ulong)updates[updateField119 + num37].UInt32Value << 32);
						}
						else
						{
							updateData.ActivePlayerData.ExploredZones[num37 / 2] = updates[updateField119 + num37].UInt32Value;
						}
					}
				}
			}
			int updateField120 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_COINAGE);
			if (updateField120 >= 0 && updateMaskArray[updateField120])
			{
				updateData.ActivePlayerData.Coinage = updates[updateField120].UInt32Value;
			}
			int updateField121 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_POSSTAT0);
			if (updateField121 >= 0)
			{
				for (int num39 = 0; num39 < 5; num39++)
				{
					if (updateMaskArray[updateField121 + num39])
					{
						updateData.UnitData.StatPosBuff[num39] = updates[updateField121 + num39].Int32Value;
					}
				}
			}
			int updateField122 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_NEGSTAT0);
			if (updateField122 >= 0)
			{
				for (int num40 = 0; num40 < 5; num40++)
				{
					if (updateMaskArray[updateField122 + num40])
					{
						updateData.UnitData.StatNegBuff[num40] = updates[updateField122 + num40].Int32Value;
					}
				}
			}
			int updateField123 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_RESISTANCEBUFFMODSPOSITIVE);
			if (updateField123 >= 0)
			{
				for (int num41 = 0; num41 < 7; num41++)
				{
					if (updateMaskArray[updateField123 + num41])
					{
						updateData.UnitData.ResistanceBuffModsPositive[num41] = updates[updateField123 + num41].Int32Value;
					}
				}
			}
			int updateField124 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_RESISTANCEBUFFMODSNEGATIVE);
			if (updateField124 >= 0)
			{
				for (int num42 = 0; num42 < 7; num42++)
				{
					if (updateMaskArray[updateField124 + num42])
					{
						updateData.UnitData.ResistanceBuffModsNegative[num42] = updates[updateField124 + num42].Int32Value;
					}
				}
			}
			int updateField125 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_MOD_DAMAGE_DONE_POS);
			if (updateField125 >= 0)
			{
				for (int num43 = 0; num43 < 7; num43++)
				{
					if (updateMaskArray[updateField125 + num43])
					{
						updateData.ActivePlayerData.ModDamageDonePos[num43] = updates[updateField125 + num43].Int32Value;
					}
				}
			}
			int updateField126 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_MOD_DAMAGE_DONE_NEG);
			if (updateField126 >= 0)
			{
				for (int num44 = 0; num44 < 7; num44++)
				{
					if (updateMaskArray[updateField126 + num44])
					{
						updateData.ActivePlayerData.ModDamageDoneNeg[num44] = updates[updateField126 + num44].Int32Value;
					}
				}
			}
			int updateField127 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_MOD_DAMAGE_DONE_PCT);
			if (updateField127 >= 0)
			{
				for (int num45 = 0; num45 < 7; num45++)
				{
					if (updateMaskArray[updateField127 + num45])
					{
						updateData.ActivePlayerData.ModDamageDonePercent[num45] = updates[updateField127 + num45].FloatValue;
					}
				}
			}
			int updateField128 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_MOD_HEALING_DONE_POS);
			if (updateField128 >= 0 && updateMaskArray[updateField128])
			{
				updateData.ActivePlayerData.ModHealingDonePos = updates[updateField128].Int32Value;
			}
			int updateField129 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_MOD_TARGET_RESISTANCE);
			if (updateField129 >= 0 && updateMaskArray[updateField129])
			{
				updateData.ActivePlayerData.ModTargetResistance = updates[updateField129].Int32Value;
			}
			int updateField130 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_MOD_TARGET_PHYSICAL_RESISTANCE);
			if (updateField130 >= 0 && updateMaskArray[updateField130])
			{
				updateData.ActivePlayerData.ModTargetPhysicalResistance = updates[updateField130].Int32Value;
			}
			int updateField131 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_BYTES);
			if (updateField131 >= 0 && updateMaskArray[updateField131])
			{
				updateData.ActivePlayerData.LocalFlags = (byte)(updates[updateField131].UInt32Value & 0xFFu);
				if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
				{
					byte b10 = (byte)((updates[updateField131].UInt32Value >> 8) & 0xFFu);
					Class @class = Class.None;
					@class = ((!updateData.UnitData.ClassId.HasValue) ? GetSession().GameState.GetUnitClass(guid.To128(GetSession().GameState)) : ((Class)updateData.UnitData.ClassId.Value));
					sbyte powerSlotForClass = ClassPowerTypes.GetPowerSlotForClass(@class, PowerType.ComboPoints);
					if (powerSlotForClass >= 0)
					{
						if (powerUpdate != null && guid == GetSession().GameState.CurrentPlayerGuid)
						{
							powerUpdate.Powers.Add(new PowerUpdatePower(b10, 14));
						}
						updateData.UnitData.Power[powerSlotForClass] = b10;
					}
				}
				else
				{
					updateData.ActivePlayerData.GrantableLevels = (byte)((updates[updateField131].UInt32Value >> 8) & 0xFFu);
				}
				updateData.ActivePlayerData.MultiActionBars = (byte)((updates[updateField131].UInt32Value >> 16) & 0xFFu);
				updateData.ActivePlayerData.LifetimeMaxRank = (byte)((updates[updateField131].UInt32Value >> 24) & 0xFFu);
			}
			int updateField132 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_AMMO_ID);
			if (updateField132 >= 0 && updateMaskArray[updateField132])
			{
				updateData.ActivePlayerData.AmmoID = updates[updateField132].UInt32Value;
			}
			int updateField133 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_SELF_RES_SPELL);
			if (updateField133 >= 0 && updateMaskArray[updateField133])
			{
				uint uInt32Value4 = updates[updateField133].UInt32Value;
				updateData.ActivePlayerData.SelfResSpells = new List<uint>();
				updateData.ActivePlayerData.SelfResSpells.Add(uInt32Value4);
			}
			int updateField134 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_PVP_MEDALS);
			if (updateField134 >= 0 && updateMaskArray[updateField134])
			{
				updateData.ActivePlayerData.PvpMedals = updates[updateField134].UInt32Value;
			}
			int updateField135 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_BUYBACK_PRICE_1);
			if (updateField135 >= 0)
			{
				for (int num46 = 0; num46 < 12; num46++)
				{
					if (updateMaskArray[updateField135 + num46])
					{
						updateData.ActivePlayerData.BuybackPrice[num46] = updates[updateField135 + num46].UInt32Value;
					}
				}
			}
			int updateField136 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_BUYBACK_TIMESTAMP_1);
			if (updateField136 >= 0)
			{
				for (int num47 = 0; num47 < 12; num47++)
				{
					if (updateMaskArray[updateField136 + num47])
					{
						updateData.ActivePlayerData.BuybackTimestamp[num47] = updates[updateField136 + num47].UInt32Value;
					}
				}
			}
			int updateField137 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_SESSION_KILLS);
			if (updateField137 >= 0 && updateMaskArray[updateField137])
			{
				updateData.ActivePlayerData.TodayHonorableKills = (ushort)(updates[updateField137].UInt32Value & 0xFFFFu);
				updateData.ActivePlayerData.TodayDishonorableKills = (ushort)((updates[updateField137].UInt32Value >> 16) & 0xFFFFu);
			}
			int updateField138 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_KILLS);
			if (updateField138 >= 0 && updateMaskArray[updateField138])
			{
				updateData.ActivePlayerData.TodayHonorableKills = (ushort)(updates[updateField138].UInt32Value & 0xFFFFu);
				updateData.ActivePlayerData.YesterdayHonorableKills = (ushort)((updates[updateField138].UInt32Value >> 16) & 0xFFFFu);
			}
			int updateField139 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_YESTERDAY_KILLS);
			if (updateField139 >= 0 && updateMaskArray[updateField139])
			{
				updateData.ActivePlayerData.YesterdayHonorableKills = (ushort)(updates[updateField139].UInt32Value & 0xFFFFu);
				updateData.ActivePlayerData.YesterdayDishonorableKills = (ushort)((updates[updateField139].UInt32Value >> 16) & 0xFFFFu);
			}
			int updateField140 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_LAST_WEEK_KILLS);
			if (updateField140 >= 0 && updateMaskArray[updateField140])
			{
				updateData.ActivePlayerData.LastWeekHonorableKills = (ushort)(updates[updateField140].UInt32Value & 0xFFFFu);
				updateData.ActivePlayerData.LastWeekDishonorableKills = (ushort)((updates[updateField140].UInt32Value >> 16) & 0xFFFFu);
			}
			int updateField141 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_THIS_WEEK_KILLS);
			if (updateField141 >= 0 && updateMaskArray[updateField141])
			{
				updateData.ActivePlayerData.ThisWeekHonorableKills = (ushort)(updates[updateField141].UInt32Value & 0xFFFFu);
				updateData.ActivePlayerData.ThisWeekDishonorableKills = (ushort)((updates[updateField141].UInt32Value >> 16) & 0xFFFFu);
			}
			int updateField142 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_THIS_WEEK_CONTRIBUTION);
			if (updateField142 < 0)
			{
				updateField142 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_TODAY_CONTRIBUTION);
			}
			if (updateField142 >= 0 && updateMaskArray[updateField142])
			{
				updateData.ActivePlayerData.ThisWeekContribution = updates[updateField142].UInt32Value;
			}
			int updateField143 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_LIFETIME_HONORABLE_KILLS);
			if (updateField143 >= 0 && updateMaskArray[updateField143])
			{
				updateData.ActivePlayerData.LifetimeHonorableKills = updates[updateField143].UInt32Value;
			}
			int updateField144 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_LIFETIME_DISHONORABLE_KILLS);
			if (updateField144 >= 0 && updateMaskArray[updateField144])
			{
				updateData.ActivePlayerData.LifetimeDishonorableKills = updates[updateField144].UInt32Value;
			}
			int updateField145 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_YESTERDAY_CONTRIBUTION);
			if (updateField145 >= 0 && updateMaskArray[updateField145])
			{
				updateData.ActivePlayerData.YesterdayContribution = updates[updateField145].UInt32Value;
			}
			int updateField146 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_LAST_WEEK_CONTRIBUTION);
			if (updateField146 >= 0 && updateMaskArray[updateField146])
			{
				updateData.ActivePlayerData.LastWeekContribution = updates[updateField146].UInt32Value;
			}
			int updateField147 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_LAST_WEEK_RANK);
			if (updateField147 >= 0 && updateMaskArray[updateField147])
			{
				updateData.ActivePlayerData.LastWeekRank = updates[updateField147].UInt32Value;
			}
			int updateField148 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_BYTES2);
			if (updateField148 >= 0 && updateMaskArray[updateField148])
			{
				updateData.ActivePlayerData.PvPRankProgress = (byte)(updates[updateField148].UInt32Value & 0xFFu);
				updateData.ActivePlayerData.AuraVision = (byte)((updates[updateField148].UInt32Value >> 8) & 0xFFu);
			}
			int updateField149 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_WATCHED_FACTION_INDEX);
			if (updateField149 >= 0 && updateMaskArray[updateField149])
			{
				updateData.ActivePlayerData.WatchedFactionIndex = updates[updateField149].Int32Value;
			}
			int updateField150 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_COMBAT_RATING_1);
			if (updateField150 >= 0)
			{
				for (int num48 = 0; num48 < 20; num48++)
				{
					if (updateMaskArray[updateField150 + num48])
					{
						updateData.ActivePlayerData.CombatRatings[num48] = updates[updateField150 + num48].Int32Value;
					}
				}
			}
			int updateField151 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_ARENA_TEAM_INFO_1_1);
			if (updateField151 >= 0)
			{
				int num49 = 0;
				int num50 = 2;
				int num51 = 3;
				int num52 = 4;
				int num53 = 5;
				int num54 = 6;
				for (int num55 = 0; num55 < 3; num55++)
				{
					int num56 = updateField151 + num55 * num54;
					if (updateMaskArray[num56 + num49] && guid == GetSession().GameState.CurrentPlayerGuid)
					{
						uint num57 = (GetSession().GameState.CurrentArenaTeamIds[num55] = updates[num56 + num49].UInt32Value);
						if (num57 != 0)
						{
							WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_ARENA_TEAM_QUERY);
							worldPacket.WriteUInt32(num57);
							SendPacketToServer(worldPacket);
							WorldPacket worldPacket2 = new WorldPacket(Opcode.CMSG_ARENA_TEAM_ROSTER);
							worldPacket2.WriteUInt32(num57);
							SendPacketToServer(worldPacket2);
						}
						else
						{
							ArenaTeamRosterResponse arenaTeamRosterResponse = new ArenaTeamRosterResponse();
							arenaTeamRosterResponse.TeamSize = ModernVersion.GetArenaTeamSizeFromIndex((uint)num55);
							SendPacketToClient(arenaTeamRosterResponse);
						}
					}
					if (updateMaskArray[num56 + num50])
					{
						if (updateData.ActivePlayerData.PvpInfo[num55] == null)
						{
							updateData.ActivePlayerData.PvpInfo[num55] = new PVPInfo();
						}
						updateData.ActivePlayerData.PvpInfo[num55].WeeklyPlayed = updates[num56 + num50].UInt32Value;
					}
					if (updateMaskArray[num56 + num51])
					{
						if (updateData.ActivePlayerData.PvpInfo[num55] == null)
						{
							updateData.ActivePlayerData.PvpInfo[num55] = new PVPInfo();
						}
						updateData.ActivePlayerData.PvpInfo[num55].SeasonPlayed = updates[num56 + num51].UInt32Value;
					}
					if (updateMaskArray[num56 + num52])
					{
						if (updateData.ActivePlayerData.PvpInfo[num55] == null)
						{
							updateData.ActivePlayerData.PvpInfo[num55] = new PVPInfo();
						}
						updateData.ActivePlayerData.PvpInfo[num55].SeasonWon = updates[num56 + num52].UInt32Value;
					}
					if (updateMaskArray[num56 + num53])
					{
						if (updateData.ActivePlayerData.PvpInfo[num55] == null)
						{
							updateData.ActivePlayerData.PvpInfo[num55] = new PVPInfo();
						}
						updateData.ActivePlayerData.PvpInfo[num55].Rating = updates[num56 + num53].UInt32Value;
					}
				}
			}
			if (guid == GetSession().GameState.CurrentPlayerGuid && ModernVersion.ExpansionVersion > 1)
			{
				int updateField152 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_HONOR_CURRENCY);
				int updateField153 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_ARENA_CURRENCY);
				if (updateField152 >= 0 && updateField153 >= 0 && (updateMaskArray[updateField152] || updateMaskArray[updateField153]))
				{
					SetupCurrency setupCurrency = new SetupCurrency();
					if (updates.ContainsKey(updateField153))
					{
						SetupCurrency.Record record = default(SetupCurrency.Record);
						record.Type = 1900u;
						record.Quantity = updates[updateField153].UInt32Value;
						setupCurrency.Data.Add(record);
					}
					if (updates.ContainsKey(updateField152))
					{
						SetupCurrency.Record record2 = default(SetupCurrency.Record);
						record2.Type = 1901u;
						record2.Quantity = updates[updateField152].UInt32Value;
						setupCurrency.Data.Add(record2);
					}
					SendPacketToClient(setupCurrency);
				}
			}
			int updateField154 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_MOD_MANA_REGEN);
			if (updateField154 >= 0 && updateMaskArray[updateField154])
			{
				updateData.UnitData.ModPowerRegen[0] = updates[updateField154].FloatValue;
			}
			int updateField155 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_MAX_LEVEL);
			if (updateField155 >= 0 && updateMaskArray[updateField155])
			{
				updateData.ActivePlayerData.MaxLevel = updates[updateField155].Int32Value;
			}
			int updateField156 = LegacyVersion.GetUpdateField(PlayerField.PLAYER_FIELD_DAILY_QUESTS_1);
			if (updateField156 >= 0 && guid == GetSession().GameState.CurrentPlayerGuid)
			{
				for (int num58 = 0; num58 < 25; num58++)
				{
					if (updateMaskArray[updateField156 + num58])
					{
						GetSession().GameState.SetDailyQuestSlot((uint)num58, updates[updateField156 + num58].UInt32Value);
						updateData.ActivePlayerData.HasDailyQuestsUpdate = true;
					}
				}
			}
		}
		if (objectType == ObjectType.GameObject)
		{
			int updateField157 = LegacyVersion.GetUpdateField(GameObjectField.GAMEOBJECT_FIELD_CREATED_BY);
			if (updateField157 >= 0 && updateMaskArray[updateField157])
			{
				updateData.GameObjectData.CreatedBy = GetGuidValue(updates, GameObjectField.GAMEOBJECT_FIELD_CREATED_BY).To128(GetSession().GameState);
			}
			int updateField158 = LegacyVersion.GetUpdateField(GameObjectField.GAMEOBJECT_DISPLAYID);
			if (updateField158 >= 0 && updateMaskArray[updateField158])
			{
				updateData.GameObjectData.DisplayID = updates[updateField158].Int32Value;
			}
			int updateField159 = LegacyVersion.GetUpdateField(GameObjectField.GAMEOBJECT_FLAGS);
			if (updateField159 >= 0 && updateMaskArray[updateField159])
			{
				updateData.GameObjectData.Flags = updates[updateField159].UInt32Value;
			}
			int updateField160 = LegacyVersion.GetUpdateField(GameObjectField.GAMEOBJECT_ROTATION);
			if (updateField160 >= 0 && updateData.CreateData != null && updateData.CreateData.MoveInfo != null)
			{
				for (int num59 = 0; num59 < 4; num59++)
				{
					if (updateMaskArray[updateField160 + num59])
					{
						updateData.CreateData.MoveInfo.Rotation[num59] = updates[updateField160 + num59].FloatValue;
					}
				}
				switch (updateData.ObjectData.EntryID)
				{
				case 176080:
				case 176084:
				case 176085:
				{
					EulerAngles eulerAngles = updateData.CreateData.MoveInfo.Rotation.AsEulerAngles();
					eulerAngles.Yaw *= -1.0;
					updateData.CreateData.MoveInfo.Rotation = eulerAngles.AsQuaternion();
					break;
				}
				}
				switch (updateData.ObjectData.EntryID)
				{
				case 176081:
				case 176082:
				case 176083:
				case 176085:
					updateData.GameObjectData.ParentRotation = new float?[4] { -4.371139E-08f, 0f, 1f, 0f };
					break;
				case 183177:
					updateData.GameObjectData.ParentRotation = new float?[4] { 0f, 0f, -0.69465846f, 0.7193397f };
					break;
				}
			}
			int updateField161 = LegacyVersion.GetUpdateField(GameObjectField.GAMEOBJECT_STATE);
			if (updateField161 >= 0 && updateMaskArray[updateField161])
			{
				updateData.GameObjectData.State = (sbyte)updates[updateField161].Int32Value;
			}
			int updateField162 = LegacyVersion.GetUpdateField(GameObjectField.GAMEOBJECT_DYN_FLAGS);
			if (updateField162 >= 0 && updateMaskArray[updateField162])
			{
				uint num60 = 0u;
				if (updateData.ObjectData.DynamicFlags.HasValue)
				{
					num60 = updateData.ObjectData.DynamicFlags.Value;
				}
				else if (!guid.IsTransport())
				{
					num60 = 4294901760u;
				}
				GameObjectDynamicFlagsLegacy uInt32Value5 = (GameObjectDynamicFlagsLegacy)updates[updateField162].UInt32Value;
				updateData.ObjectData.DynamicFlags = num60 | (uint)uInt32Value5.CastFlags<GameObjectDynamicFlagsModern>();
			}
			int updateField163 = LegacyVersion.GetUpdateField(GameObjectField.GAMEOBJECT_FACTION);
			if (updateField163 >= 0 && updateMaskArray[updateField163])
			{
				updateData.GameObjectData.FactionTemplate = updates[updateField163].Int32Value;
			}
			int updateField164 = LegacyVersion.GetUpdateField(GameObjectField.GAMEOBJECT_TYPE_ID);
			if (updateField164 >= 0 && updateMaskArray[updateField164])
			{
				updateData.GameObjectData.TypeID = (sbyte)updates[updateField164].Int32Value;
			}
			int updateField165 = LegacyVersion.GetUpdateField(GameObjectField.GAMEOBJECT_LEVEL);
			if (updateField165 >= 0 && updateMaskArray[updateField165])
			{
				updateData.GameObjectData.Level = updates[updateField165].Int32Value;
			}
			int updateField166 = LegacyVersion.GetUpdateField(GameObjectField.GAMEOBJECT_ARTKIT);
			if (updateField166 >= 0 && updateMaskArray[updateField166])
			{
				updateData.GameObjectData.ArtKit = (byte)updates[updateField166].UInt32Value;
			}
		}
		if (objectType == ObjectType.DynamicObject)
		{
			int updateField167 = LegacyVersion.GetUpdateField(DynamicObjectField.DYNAMICOBJECT_CASTER);
			if (updateField167 >= 0 && updateMaskArray[updateField167])
			{
				updateData.DynamicObjectData.Caster = GetGuidValue(updates, DynamicObjectField.DYNAMICOBJECT_CASTER).To128(GetSession().GameState);
			}
			int updateField168 = LegacyVersion.GetUpdateField(DynamicObjectField.DYNAMICOBJECT_SPELLID);
			if (updateField168 >= 0 && updateMaskArray[updateField168])
			{
				updateData.DynamicObjectData.SpellID = updates[updateField168].Int32Value;
				updateData.DynamicObjectData.SpellXSpellVisualID = (int)GameData.GetSpellVisual((uint)updateData.DynamicObjectData.SpellID.Value);
			}
			int updateField169 = LegacyVersion.GetUpdateField(DynamicObjectField.DYNAMICOBJECT_RADIUS);
			if (updateField169 >= 0 && updateMaskArray[updateField169])
			{
				updateData.DynamicObjectData.Radius = updates[updateField169].FloatValue;
			}
		}
		if (objectType != ObjectType.Corpse)
		{
			return;
		}
		int updateField170 = LegacyVersion.GetUpdateField(CorpseField.CORPSE_FIELD_OWNER);
		if (updateField170 >= 0 && updateMaskArray[updateField170])
		{
			updateData.CorpseData.Owner = GetGuidValue(updates, CorpseField.CORPSE_FIELD_OWNER).To128(GetSession().GameState);
		}
		int updateField171 = LegacyVersion.GetUpdateField(CorpseField.CORPSE_FIELD_DISPLAY_ID);
		if (updateField171 >= 0 && updateMaskArray[updateField171])
		{
			updateData.CorpseData.DisplayID = updates[updateField171].UInt32Value;
		}
		int updateField172 = LegacyVersion.GetUpdateField(CorpseField.CORPSE_FIELD_ITEM);
		if (updateField172 >= 0)
		{
			for (int num61 = 0; num61 < 19; num61++)
			{
				if (updateMaskArray[updateField172 + num61])
				{
					updateData.CorpseData.Items[num61] = updates[updateField172 + num61].UInt32Value;
				}
			}
		}
		int updateField173 = LegacyVersion.GetUpdateField(CorpseField.CORPSE_FIELD_BYTES_1);
		if (updateField173 >= 0 && updateMaskArray[updateField173])
		{
			updateData.CorpseData.RaceId = (byte)((updates[updateField173].UInt32Value >> 8) & 0xFFu);
			updateData.CorpseData.SexId = (byte)((updates[updateField173].UInt32Value >> 16) & 0xFFu);
			byte skin = (byte)((updates[updateField173].UInt32Value >> 24) & 0xFFu);
			int updateField174 = LegacyVersion.GetUpdateField(CorpseField.CORPSE_FIELD_BYTES_2);
			if (updateField174 >= 0 && updateMaskArray[updateField174])
			{
				byte face = (byte)(updates[updateField174].UInt32Value & 0xFFu);
				byte hairStyle = (byte)((updates[updateField174].UInt32Value >> 8) & 0xFFu);
				byte hairColor = (byte)((updates[updateField174].UInt32Value >> 16) & 0xFFu);
				byte facialHair = (byte)((updates[updateField174].UInt32Value >> 24) & 0xFFu);
				Array<ChrCustomizationChoice> array3 = CharacterCustomizations.ConvertLegacyCustomizationsToModern((Race)updateData.CorpseData.RaceId.Value, (Gender)updateData.CorpseData.SexId.Value, skin, face, hairStyle, hairColor, facialHair);
				for (int num62 = 0; num62 < 5; num62++)
				{
					updateData.CorpseData.Customizations[num62] = array3[num62];
				}
			}
		}
		int updateField175 = LegacyVersion.GetUpdateField(CorpseField.CORPSE_FIELD_GUILD);
		if (updateField175 >= 0 && updateMaskArray[updateField175])
		{
			updateData.CorpseData.GuildGUID = WowGuid128.Create(HighGuidType703.Guild, updates[updateField175].UInt32Value);
		}
		int updateField176 = LegacyVersion.GetUpdateField(CorpseField.CORPSE_FIELD_FLAGS);
		if (updateField176 >= 0 && updateMaskArray[updateField176])
		{
			updateData.CorpseData.Flags = updates[updateField176].UInt32Value;
			if (updateData.CorpseData.Flags.HasAnyFlag(CorpseFlags.HideHelm))
			{
				updateData.CorpseData.Flags &= 4294967287u;
				updateData.CorpseData.Items[0] = null;
			}
			if (updateData.CorpseData.Flags.HasAnyFlag(CorpseFlags.HideCloak))
			{
				updateData.CorpseData.Flags &= 4294967279u;
				updateData.CorpseData.Items[14] = null;
			}
		}
		int updateField177 = LegacyVersion.GetUpdateField(CorpseField.CORPSE_FIELD_DYNAMIC_FLAGS);
		if (updateField177 >= 0 && updateMaskArray[updateField177])
		{
			updateData.CorpseData.DynamicFlags = updates[updateField177].UInt32Value;
		}
    }

	[PacketHandler(Opcode.SMSG_INIT_WORLD_STATES)]
	private void HandleInitWorldStates(WorldPacket packet)
	{
		InitWorldStates initWorldStates = new InitWorldStates();
		initWorldStates.MapID = packet.ReadUInt32();
		GetSession().GameState.CurrentMapId = initWorldStates.MapID;
		initWorldStates.ZoneID = packet.ReadUInt32();
		initWorldStates.AreaID = (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_1_0_6692) ? packet.ReadUInt32() : initWorldStates.ZoneID);
		GetSession().GameState.HasWsgAllyFlagCarrier = false;
		GetSession().GameState.HasWsgHordeFlagCarrier = false;
		ushort num = packet.ReadUInt16();
		for (ushort num2 = 0; num2 < num; num2++)
		{
			uint num3 = packet.ReadUInt32();
			int num4 = packet.ReadInt32();
			if (num3 != 0 || num4 != 0)
			{
				initWorldStates.AddState(num3, num4);
			}
			switch (num3)
			{
			case 2339u:
				GetSession().GameState.HasWsgAllyFlagCarrier = num4 == 2;
				break;
			case 2338u:
				GetSession().GameState.HasWsgHordeFlagCarrier = num4 == 2;
				break;
			}
		}
		initWorldStates.AddClassicStates();
		SendPacketToClient(initWorldStates);
		if (LegacyVersion.ExpansionVersion <= 1 || ModernVersion.ExpansionVersion <= 1)
		{
			SendPacketToClient(new SetupCurrency());
		}
		SendPacketToClient(new AllAccountCriteria());
		if (GetSession().GameState.HasWsgHordeFlagCarrier || GetSession().GameState.HasWsgAllyFlagCarrier)
		{
			WorldPacket packet2 = new WorldPacket(Opcode.MSG_BATTLEGROUND_PLAYER_POSITIONS);
			SendPacket(packet2);
		}
		if (GetSession().GameState.CurrentZoneId == initWorldStates.ZoneID)
		{
			return;
		}
		string areaName = GameData.GetAreaName(GetSession().GameState.CurrentZoneId);
		string areaName2 = GameData.GetAreaName(initWorldStates.ZoneID);
		GetSession().GameState.CurrentZoneId = initWorldStates.ZoneID;
		if (string.IsNullOrEmpty(areaName) || string.IsNullOrEmpty(areaName2))
		{
			return;
		}
		foreach (ChatChannel chatChannelsWithFlag in GameData.GetChatChannelsWithFlags(ChannelFlags.AutoJoin | ChannelFlags.ZoneBased))
		{
			SendChatLeaveChannel(1, chatChannelsWithFlag.Name + " - " + areaName);
			SendChatJoinChannel(1, chatChannelsWithFlag.Name + " - " + areaName2, "");
		}
	}

    // 处理更新魔兽世界状态
    [PacketHandler(Opcode.SMSG_UPDATE_WORLD_STATE)]
	private void HandleUpdateWorldState(WorldPacket packet)
	{
		UpdateWorldState updateWorldState = new UpdateWorldState();
		updateWorldState.VariableID = packet.ReadUInt32();
		updateWorldState.Value = packet.ReadInt32();
		SendPacketToClient(updateWorldState);
		if (updateWorldState.VariableID == 2339)
		{
			WorldPacket packet2 = new WorldPacket(Opcode.MSG_BATTLEGROUND_PLAYER_POSITIONS);
			SendPacket(packet2);
			GetSession().GameState.HasWsgAllyFlagCarrier = updateWorldState.Value == 2;
		}
		else if (updateWorldState.VariableID == 2338)
		{
			WorldPacket packet3 = new WorldPacket(Opcode.MSG_BATTLEGROUND_PLAYER_POSITIONS);
			SendPacket(packet3);
			GetSession().GameState.HasWsgHordeFlagCarrier = updateWorldState.Value == 2;
		}
	}

    // 初始化数据包处理程序
    public WorldClient()
	{
		InitializePacketHandlers();
	}

    // 魔兽世界会话数据
    public GlobalSessionData GetSession()
	{
		return _globalSession;
	}

	// 连接到魔兽世界服务器
	public bool ConnectToWorldServer(Realm realm, GlobalSessionData globalSession)
	{
		_worldCrypt = null;
		_realm = realm;
		_globalSession = globalSession;
		_username = globalSession.Username;
		_isSuccessful = null;
		_delayedPacketsToServer = new Dictionary<Opcode, List<WorldPacket>>();  //延迟数据包到服务器
        _delayedPacketsToClient = new Dictionary<Opcode, List<ServerPacket>>(); //延迟数据包到客户端
		Log.Print(LogType.Network, "正在连接世界服务器...", "ConnectToWorldServer", "D:\\a\\HermesProxy\\HermesProxy\\World\\Client\\WorldClient.cs");
		try
		{
			IPAddress iPAddress = NetworkUtils.ResolveOrDirectIPv4(realm.ExternalAddress);
			Log.Print(LogType.Network, $"世界服务器地址 {realm.ExternalAddress}:{realm.Port} resolved as {iPAddress}:{realm.Port}", "ConnectToWorldServer", "D:\\a\\HermesProxy\\HermesProxy\\World\\Client\\WorldClient.cs");
			_clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			IPEndPoint iPEndPoint = new IPEndPoint(iPAddress, realm.Port);
			_clientSocket.BeginConnect(iPEndPoint, ConnectCallback, null);
		}
		catch (Exception ex)
		{
			Log.Print(LogType.Error, "Socket Error: " + ex.Message, "ConnectToWorldServer", "D:\\a\\HermesProxy\\HermesProxy\\World\\Client\\WorldClient.cs");
			_isSuccessful = false;
		}
		while (!_isSuccessful.HasValue)
		{
		}
		return _isSuccessful.Value;
	}

	public bool IsAuthenticated()
	{
		return _isSuccessful == true;
	}

    // 初始化加密
    private void InitializeEncryption(byte[] sessionKey)
	{
		switch (Settings.ServerBuild)
		{
		case ClientVersionBuild.V1_12_1_5875:
		case ClientVersionBuild.V1_12_2_6005:
		case ClientVersionBuild.V1_12_3_6141:
			_worldCrypt = new VanillaWorldCrypt();
			break;
		case ClientVersionBuild.V2_4_3_8606:
			_worldCrypt = new TbcWorldCrypt();
			break;
		}
		if (_worldCrypt != null)
		{
			_worldCrypt.Initialize(sessionKey);
		}
	}
	// 断开
	public void Disconnect()
	{
		if (IsConnected())
		{
			_clientSocket.Shutdown(SocketShutdown.Both);
			_clientSocket.Disconnect(false);
			if (GetSession().WorldClient == this)
			{
				GetSession().WorldClient = null;
			}
		}
	}
	// 判断是否连接
	public bool IsConnected()
	{
		if (_clientSocket != null)
		{
			return _clientSocket.Connected;
		}
		return false;
	}

	public uint GetQueuePosition()
	{
		return _queuePosition;
	}

	private void ConnectCallback(IAsyncResult AR)
	{
		try
		{
			Log.Print(LogType.Network, "Connection established!", "ConnectCallback", "D:\\a\\HermesProxy\\HermesProxy\\World\\Client\\WorldClient.cs");
			_clientSocket.EndConnect(AR);
			_clientSocket.ReceiveBufferSize = 65535;
			Task.Run((Func<Task?>)ReceiveLoop);
		}
		catch (Exception ex)
		{
			Log.Print(LogType.Error, "Connect Error: " + ex.Message, "ConnectCallback", "D:\\a\\HermesProxy\\HermesProxy\\World\\Client\\WorldClient.cs");
			if (!_isSuccessful.HasValue)
			{
				_isSuccessful = false;
			}
		}
	}

	private async Task<bool> ReceiveBufferFully(ArraySegment<byte> bufferToFill)
	{
		int num;
		for (int alreadyReceived = 0; alreadyReceived < bufferToFill.Count; alreadyReceived += num)
		{
			ArraySegment<byte> arraySegment = new ArraySegment<byte>(bufferToFill.Array, alreadyReceived + bufferToFill.Offset, bufferToFill.Count - alreadyReceived);
			num = await _clientSocket.ReceiveAsync(arraySegment, SocketFlags.None);
			if (num == 0)
			{
				return false;
			}
		}
		return true;
	}

    // 接收循环
    private async Task ReceiveLoop()
	{
		_ = 1;
		try
		{
			while (true)
			{
				byte[] headerBuffer = new byte[4];
				if (!(await ReceiveBufferFully(headerBuffer)))
				{
					Log.PrintNet(LogType.Error, LogNetDir.S2P, "Socket Closed By GameWorldServer (header)", "ReceiveLoop", "D:\\a\\HermesProxy\\HermesProxy\\World\\Client\\WorldClient.cs");
					if (!_isSuccessful.HasValue)
					{
						_isSuccessful = false;
					}
					else if (GetSession().WorldClient == this)
					{
						GetSession().OnDisconnect();
					}
					return;
				}
				if (_worldCrypt != null)
				{
					_worldCrypt.Decrypt(headerBuffer, 4);
				}
				LegacyServerPacketHeader legacyServerPacketHeader = new LegacyServerPacketHeader();
				legacyServerPacketHeader.Read(headerBuffer);
				ushort size = legacyServerPacketHeader.Size;
				if (size != 0)
				{
					byte[] buffer = new byte[size];
					buffer[0] = headerBuffer[2];
					buffer[1] = headerBuffer[3];
					if (!(await ReceiveBufferFully(new ArraySegment<byte>(buffer, 2, buffer.Length - 2))))
					{
						break;
					}
					WorldPacket worldPacket = new WorldPacket(buffer);
					worldPacket.SetReceiveTime(Environment.TickCount);
					HandlePacket(worldPacket);
				}
			}
			Log.PrintNet(LogType.Error, LogNetDir.S2P, "Socket Closed By GameWorldServer (payload)", "ReceiveLoop", "D:\\a\\HermesProxy\\HermesProxy\\World\\Client\\WorldClient.cs");
			if (!_isSuccessful.HasValue)
			{
				_isSuccessful = false;
			}
			else if (GetSession().WorldClient == this)
			{
				GetSession().OnDisconnect();
			}
		}
		catch (Exception ex)
		{
			Log.PrintNet(LogType.Error, LogNetDir.S2P, "Packet Read Error: " + ex.Message + Environment.NewLine + ex.StackTrace, "ReceiveLoop", "D:\\a\\HermesProxy\\HermesProxy\\World\\Client\\WorldClient.cs");
			if (!_isSuccessful.HasValue)
			{
				_isSuccessful = false;
				return;
			}
			Disconnect();
			GetSession().OnDisconnect();
		}
	}

	private void SendPacket(WorldPacket packet)
	{
		_sendMutex.WaitOne();
		try
		{
			ByteBuffer byteBuffer = new ByteBuffer();
			LegacyClientPacketHeader legacyClientPacketHeader = new LegacyClientPacketHeader();
			legacyClientPacketHeader.Size = (ushort)(packet.GetSize() + 4);
			legacyClientPacketHeader.Opcode = packet.GetOpcode();
			legacyClientPacketHeader.Write(byteBuffer);
			Log.PrintNet(LogType.Debug, LogNetDir.P2S, $"Sending opcode {LegacyVersion.GetUniversalOpcode(legacyClientPacketHeader.Opcode)} ({legacyClientPacketHeader.Opcode}) with size {legacyClientPacketHeader.Size}.", "SendPacket", "D:\\a\\HermesProxy\\HermesProxy\\World\\Client\\WorldClient.cs");
			byte[] data = byteBuffer.GetData();
			if (_worldCrypt != null)
			{
				_worldCrypt.Encrypt(data, 6);
			}
			byteBuffer.Clear();
			byteBuffer.WriteBytes(data);
			byteBuffer.WriteBytes(packet.GetData(), packet.GetSize());
			_clientSocket.Send(byteBuffer.GetData(), SocketFlags.None);
		}
		catch (Exception ex)
		{
			Log.PrintNet(LogType.Error, LogNetDir.P2S, "Packet Write Error: " + ex.Message, "SendPacket", "D:\\a\\HermesProxy\\HermesProxy\\World\\Client\\WorldClient.cs");
			if (!_isSuccessful.HasValue)
			{
				_isSuccessful = false;
			}
		}
		_sendMutex.ReleaseMutex();
	}

	// 发送数据包到客户端
	public void SendPacketToClient(ServerPacket packet, Opcode delayUntilOpcode = Opcode.MSG_NULL_ACTION)
	{
        Opcode universalOpcode = packet.GetUniversalOpcode();
		if (delayUntilOpcode != 0)
		{
			if (_delayedPacketsToClient.ContainsKey(delayUntilOpcode))
			{
				_delayedPacketsToClient[delayUntilOpcode].Add(packet);
				return;
			}
			List<ServerPacket> list = new List<ServerPacket>();
			list.Add(packet);
			_delayedPacketsToClient.Add(delayUntilOpcode, list);
		}
		else
		{
			SendPacketToClientDirect(packet);
			SendDelayedPacketsToClientOnOpcode(universalOpcode);
		}
	}
    // 直接发送数据包到客户端
    private void SendPacketToClientDirect(ServerPacket packet)
	{
        Queue<ServerPacket> pendingUninstancedPackets = GetSession().GameState.PendingUninstancedPackets;
		if (packet.GetConnection() == ConnectionType.Realm)
		{
			GetSession().RealmSocket.SendPacket(packet);
			return;
		}
		if (GetSession().InstanceSocket == null && !GetSession().GameState.IsConnectedToInstance)
		{
			lock (pendingUninstancedPackets)
			{
				if (GetSession().InstanceSocket == null && !GetSession().GameState.IsConnectedToInstance)
				{
					pendingUninstancedPackets.Enqueue(packet);
					Log.PrintNet(LogType.Warn, LogNetDir.P2C, $"无法发送操作码 {packet.GetUniversalOpcode()} ({packet.GetOpcode()}) 在进入世界之前！队列", "SendPacketToClientDirect", "D:\\a\\HermesProxy\\HermesProxy\\World\\Client\\WorldClient.cs");
					return;
				}
			}
		}
		while (GetSession().InstanceSocket == null)
		{
			Log.PrintNet(LogType.Network, LogNetDir.P2C, $"等待发送 {packet.GetUniversalOpcode()} ({packet.GetOpcode()}).", "SendPacketToClientDirect", "D:\\a\\HermesProxy\\HermesProxy\\World\\Client\\WorldClient.cs");
			Thread.Sleep(200);
		}
		WorldSocket instanceSocket = GetSession().InstanceSocket;
		if (pendingUninstancedPackets.Count > 0)
		{
			lock (pendingUninstancedPackets)
			{
				ServerPacket packet2;
				while (pendingUninstancedPackets.TryDequeue(out packet2))
				{
					instanceSocket.SendPacket(packet2);
				}
			}
		}
        instanceSocket.SendPacket(packet);
	}

	//发送数据包到服务器
	public void SendPacketToServer(WorldPacket packet, Opcode delayUntilOpcode = Opcode.MSG_NULL_ACTION)
	{
		Opcode universalOpcode = packet.GetUniversalOpcode(isModern: false);
		if (delayUntilOpcode != 0)
		{
			if (_delayedPacketsToServer.ContainsKey(delayUntilOpcode))
			{
				_delayedPacketsToServer[delayUntilOpcode].Add(packet);
				return;
			}
			List<WorldPacket> list = new List<WorldPacket>();
			list.Add(packet);
			_delayedPacketsToServer.Add(delayUntilOpcode, list);
		}
		else
		{
			SendPacket(packet);
			SendDelayedPacketsToServerOnOpcode(universalOpcode);
		}
	}

    // 通过操作码将延迟数据包发送到服务器
    private void SendDelayedPacketsToServerOnOpcode(Opcode opcode)
	{
		if (_delayedPacketsToServer.ContainsKey(opcode))
		{
			List<WorldPacket> list = _delayedPacketsToServer[opcode];
			for (int num = list.Count - 1; num >= 0; num--)
			{
				SendPacket(list[num]);
				list.RemoveAt(num);
			}
		}
	}

    // 通过操作码向客户端发送延迟数据包
    private void SendDelayedPacketsToClientOnOpcode(Opcode opcode)
	{
		if (_delayedPacketsToClient.ContainsKey(opcode))
		{
			List<ServerPacket> list = _delayedPacketsToClient[opcode];
			for (int num = list.Count - 1; num >= 0; num--)
			{
				SendPacketToClientDirect(list[num]);
				list.RemoveAt(num);
			}
		}
	}

	// 处理数据包
	private void HandlePacket(WorldPacket packet)
	{
		Opcode universalOpcode = packet.GetUniversalOpcode(isModern: false);
		Log.PrintNet(LogType.Debug, LogNetDir.S2P, $"Received opcode {universalOpcode} ({packet.GetOpcode()}).", "HandlePacket", "D:\\a\\HermesProxy\\HermesProxy\\World\\Client\\WorldClient.cs");
		switch (universalOpcode)
		{
		case Opcode.SMSG_AUTH_CHALLENGE:
			HandleAuthChallenge(packet);
			break;
		case Opcode.SMSG_AUTH_RESPONSE:
			HandleAuthResponse(packet);
			break;
		default:
			if (_packetHandlers.ContainsKey(universalOpcode))
			{
				_packetHandlers[universalOpcode](packet);
				break;
			}
			Log.PrintNet(LogType.Warn, LogNetDir.S2P, $"No handler for opcode {universalOpcode} ({packet.GetOpcode()}) (Got unknown packet from WorldServer)", "HandlePacket", "D:\\a\\HermesProxy\\HermesProxy\\World\\Client\\WorldClient.cs");
			if (!_isSuccessful.HasValue)
			{
				_isSuccessful = false;
			}
			break;
		case Opcode.SMSG_ADDON_INFO:
			break;
		}
		SendDelayedPacketsToServerOnOpcode(universalOpcode);
	}

	private void HandleAuthChallenge(WorldPacket packet)
	{
		if (Settings.ServerBuild >= ClientVersionBuild.V3_3_5a_12340)
		{
			packet.ReadUInt32();
		}
		uint serverSeed = packet.ReadUInt32();
		if (Settings.ServerBuild >= ClientVersionBuild.V3_3_5a_12340)
		{
			packet.ReadBytes(16u).ToBigInteger();
			packet.ReadBytes(16u).ToBigInteger();
		}
		RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create();
		byte[] array = new byte[4];
		randomNumberGenerator.GetBytes(array);
        System.Numerics.BigInteger bigInteger = array.ToBigInteger();
		SendAuthResponse((uint)bigInteger, serverSeed);
	}

	public void SendAuthResponse(uint clientSeed, uint serverSeed)
	{
		uint num = 0u;
		byte[] data = Framework.Cryptography.HashAlgorithm.SHA1.Hash(Encoding.ASCII.GetBytes(_username.ToUpper()), BitConverter.GetBytes(num), BitConverter.GetBytes(clientSeed), BitConverter.GetBytes(serverSeed), GetSession().AuthClient.GetSessionKey());
		WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_AUTH_SESSION);
		worldPacket.WriteUInt32((uint)Settings.ServerBuild);
		worldPacket.WriteUInt32(_realm.Id.Index);
		worldPacket.WriteBytes(_username.ToUpper().ToCString());
		if (Settings.ServerBuild >= ClientVersionBuild.V3_0_2_9056)
		{
			worldPacket.WriteUInt32(num);
		}
		worldPacket.WriteUInt32(clientSeed);
		if (Settings.ServerBuild >= ClientVersionBuild.V3_3_5a_12340)
		{
			worldPacket.WriteUInt32(_realm.Id.Region);
			worldPacket.WriteUInt32(_realm.Id.Site);
			worldPacket.WriteUInt32(_realm.Id.Index);
		}
		if (Settings.ServerBuild >= ClientVersionBuild.V3_2_0_10192)
		{
			worldPacket.WriteUInt64(num);
		}
		worldPacket.WriteBytes(data);
		byte[] data2 = new byte[178]
		{
			208, 1, 0, 0, 120, 156, 117, 207, 61, 14,
			194, 48, 12, 5, 224, 114, 14, 184, 12, 97,
			64, 149, 154, 133, 150, 25, 153, 196, 173, 172,
			38, 78, 21, 82, 126, 58, 113, 66, 206, 68,
			81, 133, 24, 98, 188, 126, 126, 79, 182, 114,
			52, 77, 16, 237, 105, 59, 154, 68, 129, 143,
			101, 177, 242, 183, 77, 85, 204, 163, 190, 166,
			32, 37, 135, 45, 161, 179, 154, 152, 60, 12,
			210, 18, 177, 37, 238, 230, 130, 87, 102, 187,
			224, 207, 144, 170, 208, 9, 185, 197, 26, 188,
			39, 9, 35, 180, 73, 188, 105, 175, 235, 49,
			94, 241, 33, 227, 72, 206, 42, 224, 94, 212,
			146, 47, 3, 154, 79, 237, 58, 183, 132, 190,
			14, 166, 199, 180, 252, 146, 167, 53, 152, 24,
			102, 121, 102, 114, 0, 178, 51, 196, 12, 26,
			112, 200, 242, 27, 77, 4, 139, 117, 79, 206,
			253, 99, 98, 140, 178, 145, 71, 13, 12, 29,
			198, 159, 190, 1, 43, 0, 141, 195
		};
		worldPacket.WriteBytes(data2);
		SendPacket(worldPacket);
		InitializeEncryption(GetSession().AuthClient.GetSessionKey());
	}

	private void HandleAuthResponse(WorldPacket packet)
	{
		AuthResult authResult = (AuthResult)packet.ReadUInt8();
		if (!_isSuccessful.HasValue)
		{
			packet.ReadUInt32();
			packet.ReadUInt8();
			packet.ReadUInt32();
			if (Settings.ServerBuild >= ClientVersionBuild.V2_0_1_6180)
			{
				packet.ReadUInt8();
			}
		}
		switch (authResult)
		{
		case AuthResult.AUTH_OK:
			Log.Print(LogType.Network, "Authentication succeeded!", "HandleAuthResponse", "D:\\a\\HermesProxy\\HermesProxy\\World\\Client\\WorldClient.cs");
			if (_queuePosition != 0 && GetSession().RealmSocket != null)
			{
				_queuePosition = 0u;
				GetSession().RealmSocket.SendAuthWaitQue(_queuePosition);
			}
			_isSuccessful = true;
			break;
		case AuthResult.AUTH_WAIT_QUEUE:
			_queuePosition = packet.ReadUInt32();
			Log.Print(LogType.Network, $"Position in queue is {_queuePosition}.", "HandleAuthResponse", "D:\\a\\HermesProxy\\HermesProxy\\World\\Client\\WorldClient.cs");
			if (_isSuccessful.HasValue && GetSession().RealmSocket != null)
			{
				GetSession().RealmSocket.SendAuthWaitQue(_queuePosition);
			}
			_isSuccessful = true;
			break;
		default:
			Log.Print(LogType.Network, "Authentication failed!", "HandleAuthResponse", "D:\\a\\HermesProxy\\HermesProxy\\World\\Client\\WorldClient.cs");
			_isSuccessful = false;
			break;
		}
	}

	public void SendPing(uint ping, uint latency)
	{
		if (IsConnected() && _isSuccessful != false)
		{
			WorldPacket worldPacket = new WorldPacket(Opcode.CMSG_PING);
			worldPacket.WriteUInt32(ping);
			worldPacket.WriteUInt32(latency);
			SendPacket(worldPacket);
		}
	}

	public void InitializePacketHandlers()
	{
		_packetHandlers = new Dictionary<Opcode, Action<WorldPacket>>();
		MethodInfo[] methods = typeof(WorldClient).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic);
		foreach (MethodInfo methodInfo in methods)
		{
			foreach (PacketHandlerAttribute customAttribute in methodInfo.GetCustomAttributes<PacketHandlerAttribute>())
			{
				if (customAttribute == null || customAttribute.Opcode == Opcode.MSG_NULL_ACTION)
				{
					continue;
				}
				if (_packetHandlers.ContainsKey(customAttribute.Opcode))
				{
					Log.Print(LogType.Error, $"Tried to override OpcodeHandler of {_packetHandlers[customAttribute.Opcode]} with {methodInfo.Name} (Opcode {customAttribute.Opcode})", "InitializePacketHandlers", "D:\\a\\HermesProxy\\HermesProxy\\World\\Client\\WorldClient.cs");
				}
				else
				{
					ParameterInfo[] parameters = methodInfo.GetParameters();
					if (parameters.Length == 0)
					{
						Log.Print(LogType.Error, "Method: " + methodInfo.Name + " Has no parameters", "InitializePacketHandlers", "D:\\a\\HermesProxy\\HermesProxy\\World\\Client\\WorldClient.cs");
						continue;
					}
					if (parameters[0].ParameterType != typeof(WorldPacket))
					{
						Log.Print(LogType.Error, "Method: " + methodInfo.Name + " has wrong BaseType", "InitializePacketHandlers", "D:\\a\\HermesProxy\\HermesProxy\\World\\Client\\WorldClient.cs");
						continue;
					}
					Action<WorldPacket> action = (Action<WorldPacket>)Delegate.CreateDelegate(typeof(Action<WorldPacket>), this, methodInfo);
					_packetHandlers[customAttribute.Opcode] = action;
				}
			}
		}
	}
}
