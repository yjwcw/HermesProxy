using System;
using System.Collections.Generic;
using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class PVPMatchStatisticsMessage : ServerPacket
{
	public class ArenaTeamsInfo
	{
		public WowGuid128[] Guids = new WowGuid128[2];

		public string[] Names = new string[2];

		public void Write(WorldPacket data)
		{
			string[] names = Names;
			foreach (string str in names)
			{
				data.WriteBits(str.GetByteCount(), 7);
			}
			data.FlushBits();
			for (int j = 0; j < 2; j++)
			{
				data.WritePackedGuid128(Guids[j]);
				data.WriteString(Names[j]);
			}
		}
	}

	public class RatingData
	{
		public uint[] Prematch = new uint[2];

		public uint[] Postmatch = new uint[2];

		public uint[] PrematchMMR = new uint[2];

		public void Write(WorldPacket data)
		{
			uint[] prematch = Prematch;
			foreach (uint data2 in prematch)
			{
				data.WriteUInt32(data2);
			}
			prematch = Postmatch;
			foreach (uint data3 in prematch)
			{
				data.WriteUInt32(data3);
			}
			prematch = PrematchMMR;
			foreach (uint data4 in prematch)
			{
				data.WriteUInt32(data4);
			}
		}
	}

	public class HonorData
	{
		public uint HonorKills;

		public uint Deaths;

		public uint ContributionPoints;

		public void Write(WorldPacket data)
		{
			data.WriteUInt32(HonorKills);
			data.WriteUInt32(Deaths);
			data.WriteUInt32(ContributionPoints);
		}
	}

	public class PVPMatchPlayerStatistics
	{
		public WowGuid128 PlayerGUID;

		public uint Kills;

		public bool Faction;

		public bool IsInWorld = true;

		public HonorData Honor;

		public uint DamageDone;

		public uint HealingDone;

		public uint? PreMatchRating;

		public int? RatingChange;

		public uint? PreMatchMMR;

		public int? MmrChange;

		public List<uint> Stats = new List<uint>();

		public int PrimaryTalentTree;

		public Gender Sex;

		public Race PlayerRace;

		public Class PlayerClass;

		public int CreatureID;

		public int HonorLevel = 1;

		public int Rank;

		public void Write(WorldPacket data)
		{
			data.WritePackedGuid128(PlayerGUID);
			data.WriteUInt32(Kills);
			data.WriteUInt32(DamageDone);
			data.WriteUInt32(HealingDone);
			data.WriteInt32(Stats.Count);
			data.WriteInt32(PrimaryTalentTree);
			data.WriteUInt32((uint)Sex);
			data.WriteUInt32((uint)PlayerRace);
			data.WriteUInt32((uint)PlayerClass);
			data.WriteInt32(CreatureID);
			data.WriteInt32(HonorLevel);
			data.WriteInt32(Rank);
			foreach (uint stat in Stats)
			{
				data.WriteUInt32(stat);
			}
			data.WriteBit(Faction);
			data.WriteBit(IsInWorld);
			data.WriteBit(Honor != null);
			data.WriteBit(PreMatchRating.HasValue);
			data.WriteBit(RatingChange.HasValue);
			data.WriteBit(PreMatchMMR.HasValue);
			data.WriteBit(MmrChange.HasValue);
			data.FlushBits();
			if (Honor != null)
			{
				Honor.Write(data);
			}
			if (PreMatchRating.HasValue)
			{
				data.WriteUInt32(PreMatchRating.Value);
			}
			if (RatingChange.HasValue)
			{
				data.WriteInt32(RatingChange.Value);
			}
			if (PreMatchMMR.HasValue)
			{
				data.WriteUInt32(PreMatchMMR.Value);
			}
			if (MmrChange.HasValue)
			{
				data.WriteInt32(MmrChange.Value);
			}
		}
	}

	public RatingData Ratings;

	public ArenaTeamsInfo ArenaTeams;

	public byte? Winner;

	public List<PVPMatchPlayerStatistics> Statistics = new List<PVPMatchPlayerStatistics>();

	public sbyte[] PlayerCount = new sbyte[2];

	public PVPMatchStatisticsMessage()
		: base(Opcode.SMSG_PVP_MATCH_STATISTICS, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteBit(Ratings != null);
		_worldPacket.WriteBit(ArenaTeams != null);
		_worldPacket.WriteBit(Winner.HasValue);
		if (ArenaTeams != null)
		{
			ArenaTeams.Write(_worldPacket);
		}
		_worldPacket.WriteInt32(Statistics.Count);
		sbyte[] playerCount = PlayerCount;
		foreach (sbyte data in playerCount)
		{
			_worldPacket.WriteInt8(data);
		}
		if (Ratings != null)
		{
			Ratings.Write(_worldPacket);
		}
		if (Winner.HasValue)
		{
			_worldPacket.WriteUInt8(Winner.Value);
		}
		foreach (PVPMatchPlayerStatistics statistic in Statistics)
		{
			statistic.Write(_worldPacket);
		}
	}
}
