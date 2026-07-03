using System;
using System.Collections.Generic;
using System.Linq;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class ChatPkt : ServerPacket
{
	public ChatMessageTypeModern SlashCmd;

	public uint _Language;

	public WowGuid128 SenderGUID;

	public WowGuid128 SenderGuildGUID;

	public WowGuid128 SenderAccountGUID;

	public WowGuid128 TargetGUID;

	public WowGuid128 PartyGUID;

	public WowGuid128 ChannelGUID;

	public uint SenderVirtualAddress;

	public uint TargetVirtualAddress;

	public string SenderName = "";

	public string TargetName = "";

	public string Prefix = "";

	public string Channel = "";

	public string ChatText = "";

	public uint AchievementID;

	public ChatFlags _ChatFlags;

	public float DisplayTime;

	public uint? Unused_801;

	public bool HideChatLog;

	public bool FakeSenderName;

	public ChatPkt(GlobalSessionData globalSession, ChatMessageTypeModern chatType, string message, uint language = 0u, WowGuid128 sender = null, string senderName = "", WowGuid128 receiver = null, string receiverName = "", string channelName = "", ChatFlags chatFlags = ChatFlags.None, string addonPrefix = "", uint achievementId = 0u)
		: base(Opcode.SMSG_CHAT)
	{
		SlashCmd = chatType;
		_Language = language;
		_ChatFlags = chatFlags;
		ChatText = message;
		Channel = channelName;
		AchievementID = achievementId;
		Prefix = addonPrefix;
		SenderGUID = ((sender != null) ? sender : WowGuid128.Empty);
		if (string.IsNullOrEmpty(senderName) && sender != null)
		{
			SenderName = globalSession.GameState.GetPlayerName(sender);
		}
		else
		{
			SenderName = senderName;
		}
		SenderAccountGUID = ((sender != null) ? globalSession.GetGameAccountGuidForPlayer(sender) : WowGuid128.Empty);
		SenderGuildGUID = WowGuid128.Empty;
		PartyGUID = WowGuid128.Empty;
		TargetGUID = ((receiver != null) ? receiver : WowGuid128.Empty);
		if (string.IsNullOrEmpty(receiverName) && receiver != null)
		{
			TargetName = globalSession.GameState.GetPlayerName(receiver);
		}
		else
		{
			TargetName = receiverName;
		}
		if (!SenderGUID.IsEmpty())
		{
			SenderVirtualAddress = globalSession.RealmId.GetAddress();
		}
		if (!TargetGUID.IsEmpty())
		{
			TargetVirtualAddress = globalSession.RealmId.GetAddress();
		}
	}

	public static bool CheckAddonPrefix(HashSet<string> registeredPrefixes, ref uint language, ref string text, ref string addonPrefix)
	{
		if (language == uint.MaxValue)
		{
			language = 183u;
			char c = '\t';
			if (!text.Contains(c))
			{
				return false;
			}
			string[] array = text.Split(c);
			addonPrefix = array[0];
			text = string.Join(" ", array.Skip(1).ToList());
			if (!registeredPrefixes.Contains(addonPrefix))
			{
				return false;
			}
		}
		return true;
	}

	public override void Write()
	{
		_worldPacket.WriteUInt8((byte)SlashCmd);
		_worldPacket.WriteUInt32(_Language);
		_worldPacket.WritePackedGuid128(SenderGUID);
		_worldPacket.WritePackedGuid128(SenderGuildGUID);
		_worldPacket.WritePackedGuid128(SenderAccountGUID);
		_worldPacket.WritePackedGuid128(TargetGUID);
		_worldPacket.WriteUInt32(TargetVirtualAddress);
		_worldPacket.WriteUInt32(SenderVirtualAddress);
		_worldPacket.WritePackedGuid128(PartyGUID);
		_worldPacket.WriteUInt32(AchievementID);
		_worldPacket.WriteFloat(DisplayTime);
		_worldPacket.WriteBits(SenderName.GetByteCount(), 11);
		_worldPacket.WriteBits(TargetName.GetByteCount(), 11);
		_worldPacket.WriteBits(Prefix.GetByteCount(), 5);
		_worldPacket.WriteBits(Channel.GetByteCount(), 7);
		_worldPacket.WriteBits(ChatText.GetByteCount(), 12);
		_worldPacket.WriteBits((byte)_ChatFlags, 14);
		_worldPacket.WriteBit(HideChatLog);
		_worldPacket.WriteBit(FakeSenderName);
		_worldPacket.WriteBit(Unused_801.HasValue);
		_worldPacket.WriteBit(ChannelGUID != null);
		_worldPacket.FlushBits();
		_worldPacket.WriteString(SenderName);
		_worldPacket.WriteString(TargetName);
		_worldPacket.WriteString(Prefix);
		_worldPacket.WriteString(Channel);
		_worldPacket.WriteString(ChatText);
		if (Unused_801.HasValue)
		{
			_worldPacket.WriteUInt32(Unused_801.Value);
		}
		if (ChannelGUID != null)
		{
			_worldPacket.WritePackedGuid128(ChannelGUID);
		}
	}
}
