using System;
using System.Collections.Generic;
using Framework.GameMath;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class EnumCharactersResult : ServerPacket
{
	public class CharacterInfo
	{
		public struct VisualItemInfo
		{
			public uint DisplayId;

			public uint DisplayEnchantId;

			public uint SecondaryItemModifiedAppearanceID;

			public byte InvType;

			public byte Subclass;

			public void Write(WorldPacket data)
			{
				data.WriteUInt32(DisplayId);
				data.WriteUInt32(DisplayEnchantId);
				data.WriteUInt32(SecondaryItemModifiedAppearanceID);
				data.WriteUInt8(InvType);
				data.WriteUInt8(Subclass);
			}
		}

		public struct PetInfo
		{
			public uint CreatureDisplayId;

			public uint Level;

			public uint CreatureFamily;
		}

		public WowGuid128 Guid;

		public ulong GuildClubMemberID;

		public string Name;

		public byte ListPosition;

		public Race RaceId;

		public Class ClassId;

		public Gender SexId;

		public Array<ChrCustomizationChoice> Customizations;

		public byte ExperienceLevel;

		public uint ZoneId;

		public uint MapId;

		public Vector3 PreloadPos;

		public WowGuid128 GuildGuid;

		public CharacterFlags Flags;

		public uint Flags2;

		public uint Flags3;

		public uint Flags4;

		public bool FirstLogin;

		public byte unkWod61x;

		public bool ExpansionChosen;

		public ulong LastPlayedTime;

		public ushort SpecID;

		public uint Unknown703;

		public uint LastLoginVersion;

		public uint OverrideSelectScreenFileDataID;

		public uint PetCreatureDisplayId;

		public uint PetExperienceLevel;

		public uint PetCreatureFamilyId;

		public bool BoostInProgress;

		public uint[] ProfessionIds = new uint[2];

		public VisualItemInfo[] VisualItems = new VisualItemInfo[23];

		public List<string> MailSenders = new List<string>();

		public List<uint> MailSenderTypes = new List<uint>();

		public void Write(WorldPacket data)
		{
			data.WritePackedGuid128(Guid);
			data.WriteUInt64(GuildClubMemberID);
			data.WriteUInt8(ListPosition);
			data.WriteUInt8((byte)RaceId);
			data.WriteUInt8((byte)ClassId);
			data.WriteUInt8((byte)SexId);
			data.WriteInt32(Customizations.Count);
			data.WriteUInt8(ExperienceLevel);
			data.WriteUInt32(ZoneId);
			data.WriteUInt32(MapId);
			data.WriteVector3(PreloadPos);
			data.WritePackedGuid128(GuildGuid);
			data.WriteUInt32((uint)Flags);
			data.WriteUInt32(Flags2);
			data.WriteUInt32(Flags3);
			data.WriteUInt32(PetCreatureDisplayId);
			data.WriteUInt32(PetExperienceLevel);
			data.WriteUInt32(PetCreatureFamilyId);
			data.WriteUInt32(ProfessionIds[0]);
			data.WriteUInt32(ProfessionIds[1]);
			VisualItemInfo[] visualItems = VisualItems;
			foreach (VisualItemInfo visualItemInfo in visualItems)
			{
				visualItemInfo.Write(data);
			}
			data.WriteUInt64(LastPlayedTime);
			data.WriteUInt16(SpecID);
			data.WriteUInt32(Unknown703);
			data.WriteUInt32(LastLoginVersion);
			data.WriteUInt32(Flags4);
			data.WriteInt32(MailSenders.Count);
			data.WriteInt32(MailSenderTypes.Count);
			data.WriteUInt32(OverrideSelectScreenFileDataID);
			foreach (ChrCustomizationChoice customization in Customizations)
			{
				data.WriteUInt32(customization.ChrCustomizationOptionID);
				data.WriteUInt32(customization.ChrCustomizationChoiceID);
			}
			foreach (uint mailSenderType in MailSenderTypes)
			{
				data.WriteUInt32(mailSenderType);
			}
			data.WriteBits(Name.GetByteCount(), 6);
			data.WriteBit(FirstLogin);
			data.WriteBit(BoostInProgress);
			data.WriteBits(unkWod61x, 5);
			data.WriteBit(bit: false);
			data.WriteBit(ExpansionChosen);
			foreach (string mailSender in MailSenders)
			{
				data.WriteBits(mailSender.GetByteCount() + 1, 6);
			}
			data.FlushBits();
			foreach (string mailSender2 in MailSenders)
			{
				if (!mailSender2.IsEmpty())
				{
					data.WriteCString(mailSender2);
				}
			}
			data.WriteString(Name);
		}
	}

	public struct RaceUnlock
	{
		public int RaceID;

		public bool HasExpansion;

		public bool HasAchievement;

		public bool HasHeritageArmor;

		public RaceUnlock(int raceId, bool hasExpansion, bool hasAchievement, bool hasHeritageArmor)
		{
			RaceID = raceId;
			HasExpansion = hasExpansion;
			HasAchievement = hasAchievement;
			HasHeritageArmor = hasHeritageArmor;
		}

		public void Write(WorldPacket data)
		{
			data.WriteInt32(RaceID);
			data.WriteBit(HasExpansion);
			data.WriteBit(HasAchievement);
			data.WriteBit(HasHeritageArmor);
			data.FlushBits();
		}
	}

	public struct UnlockedConditionalAppearance
	{
		public int AchievementID;

		public int Unused;

		public void Write(WorldPacket data)
		{
			data.WriteInt32(AchievementID);
			data.WriteInt32(Unused);
		}
	}

	public bool Success;

	public bool IsDeletedCharacters;

	public bool IsNewPlayerRestrictionSkipped;

	public bool IsNewPlayerRestricted;

	public bool IsNewPlayer;

	public bool IsAlliedRacesCreationAllowed;

	public int MaxCharacterLevel = 1;

	public uint? DisabledClassesMask = 0u;

	public List<CharacterInfo> Characters = new List<CharacterInfo>();

	public List<RaceUnlock> RaceUnlockData = new List<RaceUnlock>();

	public List<UnlockedConditionalAppearance> UnlockedConditionalAppearances = new List<UnlockedConditionalAppearance>();

	public EnumCharactersResult()
		: base(Opcode.SMSG_ENUM_CHARACTERS_RESULT)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteBit(Success);
		_worldPacket.WriteBit(IsDeletedCharacters);
		_worldPacket.WriteBit(IsNewPlayerRestrictionSkipped);
		_worldPacket.WriteBit(IsNewPlayerRestricted);
		_worldPacket.WriteBit(IsNewPlayer);
		_worldPacket.WriteBit(DisabledClassesMask.HasValue);
		_worldPacket.WriteBit(IsAlliedRacesCreationAllowed);
		_worldPacket.WriteInt32(Characters.Count);
		_worldPacket.WriteInt32(MaxCharacterLevel);
		_worldPacket.WriteInt32(RaceUnlockData.Count);
		_worldPacket.WriteInt32(UnlockedConditionalAppearances.Count);
		if (DisabledClassesMask.HasValue)
		{
			_worldPacket.WriteUInt32(DisabledClassesMask.Value);
		}
		foreach (UnlockedConditionalAppearance unlockedConditionalAppearance in UnlockedConditionalAppearances)
		{
			unlockedConditionalAppearance.Write(_worldPacket);
		}
		foreach (CharacterInfo character in Characters)
		{
			character.Write(_worldPacket);
		}
		foreach (RaceUnlock raceUnlockDatum in RaceUnlockData)
		{
			raceUnlockDatum.Write(_worldPacket);
		}
	}
}
