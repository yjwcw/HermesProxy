using System;
using Framework.Constants;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;

namespace HermesProxy.World.Server.Packets;

public class QueryCreatureResponse : ServerPacket
{
	public bool Allow;

	public CreatureTemplate Stats;

	public uint CreatureID;

	public QueryCreatureResponse()
		: base(Opcode.SMSG_QUERY_CREATURE_RESPONSE, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt32(CreatureID);
		_worldPacket.WriteBit(Allow);
		_worldPacket.FlushBits();
		if (!Allow)
		{
			return;
		}
		_worldPacket.WriteBits((!Stats.Title.IsEmpty()) ? (Stats.Title.GetByteCount() + 1) : 0, 11);
		_worldPacket.WriteBits((!Stats.TitleAlt.IsEmpty()) ? (Stats.TitleAlt.GetByteCount() + 1) : 0, 11);
		_worldPacket.WriteBits((!Stats.CursorName.IsEmpty()) ? (Stats.CursorName.GetByteCount() + 1) : 0, 6);
		_worldPacket.WriteBit(Stats.Civilian);
		_worldPacket.WriteBit(Stats.Leader);
		for (int i = 0; i < 4; i++)
		{
			_worldPacket.WriteBits(Stats.Name[i].GetByteCount() + 1, 11);
			_worldPacket.WriteBits(Stats.NameAlt[i].GetByteCount() + 1, 11);
		}
		for (int j = 0; j < 4; j++)
		{
			if (!string.IsNullOrEmpty(Stats.Name[j]))
			{
				_worldPacket.WriteCString(Stats.Name[j]);
			}
			if (!string.IsNullOrEmpty(Stats.NameAlt[j]))
			{
				_worldPacket.WriteCString(Stats.NameAlt[j]);
			}
		}
		for (int k = 0; k < 2; k++)
		{
			_worldPacket.WriteUInt32(Stats.Flags[k]);
		}
		_worldPacket.WriteInt32(Stats.Type);
		_worldPacket.WriteInt32(Stats.Family);
		_worldPacket.WriteInt32(Stats.Classification);
		_worldPacket.WriteUInt32(Stats.PetSpellDataId);
		for (int l = 0; l < 2; l++)
		{
			_worldPacket.WriteUInt32(Stats.ProxyCreatureID[l]);
		}
		_worldPacket.WriteInt32(Stats.Display.CreatureDisplay.Count);
		_worldPacket.WriteFloat(Stats.Display.TotalProbability);
		foreach (CreatureXDisplay item in Stats.Display.CreatureDisplay)
		{
			_worldPacket.WriteUInt32(item.CreatureDisplayID);
			_worldPacket.WriteFloat(item.Scale);
			_worldPacket.WriteFloat(item.Probability);
		}
		_worldPacket.WriteFloat(Stats.HpMulti);
		_worldPacket.WriteFloat(Stats.EnergyMulti);
		_worldPacket.WriteInt32(Stats.QuestItems.Count);
		_worldPacket.WriteUInt32(Stats.MovementInfoID);
		_worldPacket.WriteInt32(Stats.HealthScalingExpansion);
		_worldPacket.WriteUInt32(Stats.RequiredExpansion);
		_worldPacket.WriteUInt32(Stats.VignetteID);
		_worldPacket.WriteInt32(Stats.Class);
		_worldPacket.WriteInt32(Stats.DifficultyID);
		_worldPacket.WriteInt32(Stats.WidgetSetID);
		_worldPacket.WriteInt32(Stats.WidgetSetUnitConditionID);
		if (!Stats.Title.IsEmpty())
		{
			_worldPacket.WriteCString(Stats.Title);
		}
		if (!Stats.TitleAlt.IsEmpty())
		{
			_worldPacket.WriteCString(Stats.TitleAlt);
		}
		if (!Stats.CursorName.IsEmpty())
		{
			_worldPacket.WriteCString(Stats.CursorName);
		}
		foreach (uint questItem in Stats.QuestItems)
		{
			_worldPacket.WriteUInt32(questItem);
		}
	}
}
