using System;
using System.Collections.Generic;
using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class AuthResponse : ServerPacket
{
	public enum FactionMasks : byte
	{
		Player = 1,
		Alliance = 2,
		Horde = 4,
		Monster = 8
	}

	public class ClassAvailability
	{
		public byte ClassID;

		public byte ActiveExpansionLevel;

		public byte AccountExpansionLevel;

		public ClassAvailability(byte classId, byte activeExpLevel, byte accountExpLevel)
		{
			ClassID = classId;
			ActiveExpansionLevel = activeExpLevel;
			AccountExpansionLevel = accountExpLevel;
		}
	}

	public class RaceClassAvailability
	{
		public byte RaceID;

		public List<ClassAvailability> Classes = new List<ClassAvailability>();
	}

	public struct CharacterTemplateClass
	{
		public FactionMasks FactionGroup;

		public byte ClassID;

		public CharacterTemplateClass(FactionMasks factionGroup, byte classID)
		{
			FactionGroup = factionGroup;
			ClassID = classID;
		}
	}

	public class CharacterTemplate
	{
		public uint TemplateSetId;

		public List<CharacterTemplateClass> Classes;

		public string Name;

		public string Description;

		public byte Level;
	}

	public class AuthSuccessInfo
	{
		public struct GameTime
		{
			public uint BillingPlan;

			public uint TimeRemain;

			public uint Unknown735;

			public bool InGameRoom;
		}

		public byte ActiveExpansionLevel;

		public byte AccountExpansionLevel;

		public uint TimeRested;

		public uint VirtualRealmAddress;

		public uint TimeSecondsUntilPCKick;

		public uint CurrencyID;

		public long Time;

		public GameTime GameTimeInfo;

		public List<VirtualRealmInfo> VirtualRealms = new List<VirtualRealmInfo>();

		public List<CharacterTemplate> Templates = new List<CharacterTemplate>();

		public List<RaceClassAvailability> AvailableClasses;

		public bool IsExpansionTrial;

		public bool ForceCharacterTemplate;

		public ushort? NumPlayersHorde;

		public ushort? NumPlayersAlliance;

		public int? ExpansionTrialExpiration;
	}

	public AuthSuccessInfo SuccessInfo;

	public AuthWaitInfo WaitInfo;

	public BattlenetRpcErrorCode Result;

	public AuthResponse()
		: base(Opcode.SMSG_AUTH_RESPONSE)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt32((uint)Result);
		_worldPacket.WriteBit(SuccessInfo != null);
		_worldPacket.WriteBit(WaitInfo != null);
		_worldPacket.FlushBits();
		if (SuccessInfo != null)
		{
			_worldPacket.WriteUInt32(SuccessInfo.VirtualRealmAddress);
			_worldPacket.WriteInt32(SuccessInfo.VirtualRealms.Count);
			_worldPacket.WriteUInt32(SuccessInfo.TimeRested);
			_worldPacket.WriteUInt8(SuccessInfo.ActiveExpansionLevel);
			_worldPacket.WriteUInt8(SuccessInfo.AccountExpansionLevel);
			_worldPacket.WriteUInt32(SuccessInfo.TimeSecondsUntilPCKick);
			_worldPacket.WriteInt32(SuccessInfo.AvailableClasses.Count);
			_worldPacket.WriteInt32(SuccessInfo.Templates.Count);
			_worldPacket.WriteUInt32(SuccessInfo.CurrencyID);
			_worldPacket.WriteInt64(SuccessInfo.Time);
			foreach (RaceClassAvailability availableClass in SuccessInfo.AvailableClasses)
			{
				_worldPacket.WriteUInt8(availableClass.RaceID);
				_worldPacket.WriteInt32(availableClass.Classes.Count);
				foreach (ClassAvailability @class in availableClass.Classes)
				{
					_worldPacket.WriteUInt8(@class.ClassID);
					_worldPacket.WriteUInt8(@class.ActiveExpansionLevel);
					_worldPacket.WriteUInt8(@class.AccountExpansionLevel);
				}
			}
			_worldPacket.WriteBit(SuccessInfo.IsExpansionTrial);
			_worldPacket.WriteBit(SuccessInfo.ForceCharacterTemplate);
			_worldPacket.WriteBit(SuccessInfo.NumPlayersHorde.HasValue);
			_worldPacket.WriteBit(SuccessInfo.NumPlayersAlliance.HasValue);
			_worldPacket.WriteBit(SuccessInfo.ExpansionTrialExpiration.HasValue);
			_worldPacket.FlushBits();
			_worldPacket.WriteUInt32(SuccessInfo.GameTimeInfo.BillingPlan);
			_worldPacket.WriteUInt32(SuccessInfo.GameTimeInfo.TimeRemain);
			_worldPacket.WriteUInt32(SuccessInfo.GameTimeInfo.Unknown735);
			_worldPacket.WriteBit(SuccessInfo.GameTimeInfo.InGameRoom);
			_worldPacket.WriteBit(SuccessInfo.GameTimeInfo.InGameRoom);
			_worldPacket.WriteBit(SuccessInfo.GameTimeInfo.InGameRoom);
			_worldPacket.FlushBits();
			if (SuccessInfo.NumPlayersHorde.HasValue)
			{
				_worldPacket.WriteUInt16(SuccessInfo.NumPlayersHorde.Value);
			}
			if (SuccessInfo.NumPlayersAlliance.HasValue)
			{
				_worldPacket.WriteUInt16(SuccessInfo.NumPlayersAlliance.Value);
			}
			if (SuccessInfo.ExpansionTrialExpiration.HasValue)
			{
				_worldPacket.WriteInt32(SuccessInfo.ExpansionTrialExpiration.Value);
			}
			foreach (VirtualRealmInfo virtualRealm in SuccessInfo.VirtualRealms)
			{
				virtualRealm.Write(_worldPacket);
			}
			foreach (CharacterTemplate template in SuccessInfo.Templates)
			{
				_worldPacket.WriteUInt32(template.TemplateSetId);
				_worldPacket.WriteInt32(template.Classes.Count);
				foreach (CharacterTemplateClass class2 in template.Classes)
				{
					_worldPacket.WriteUInt8(class2.ClassID);
					_worldPacket.WriteUInt8((byte)class2.FactionGroup);
				}
				_worldPacket.WriteBits(template.Name.GetByteCount(), 7);
				_worldPacket.WriteBits(template.Description.GetByteCount(), 10);
				_worldPacket.FlushBits();
				_worldPacket.WriteString(template.Name);
				_worldPacket.WriteString(template.Description);
			}
		}
		if (WaitInfo != null)
		{
			WaitInfo.Write(_worldPacket);
		}
	}
}
