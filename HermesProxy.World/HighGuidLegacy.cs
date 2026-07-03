using System;
using System.Collections.Generic;
using HermesProxy.World.Enums;

namespace HermesProxy.World;

public class HighGuidLegacy : HighGuid
{
	private HighGuidTypeLegacy high;

	private static readonly Dictionary<HighGuidTypeLegacy, HighGuidType> HighLegacyToHighType = new Dictionary<HighGuidTypeLegacy, HighGuidType>
	{
		{
			HighGuidTypeLegacy.None,
			HighGuidType.Null
		},
		{
			HighGuidTypeLegacy.Player,
			HighGuidType.Player
		},
		{
			HighGuidTypeLegacy.Group,
			HighGuidType.RaidGroup
		},
		{
			HighGuidTypeLegacy.Group2,
			HighGuidType.RaidGroup
		},
		{
			HighGuidTypeLegacy.MOTransport,
			HighGuidType.MOTransport
		},
		{
			HighGuidTypeLegacy.Item,
			HighGuidType.Item
		},
		{
			HighGuidTypeLegacy.DynamicObject,
			HighGuidType.DynamicObject
		},
		{
			HighGuidTypeLegacy.GameObject,
			HighGuidType.GameObject
		},
		{
			HighGuidTypeLegacy.Transport,
			HighGuidType.Transport
		},
		{
			HighGuidTypeLegacy.Creature,
			HighGuidType.Creature
		},
		{
			HighGuidTypeLegacy.Pet,
			HighGuidType.Pet
		},
		{
			HighGuidTypeLegacy.Vehicle,
			HighGuidType.Vehicle
		},
		{
			HighGuidTypeLegacy.Corpse,
			HighGuidType.Corpse
		}
	};

	public HighGuidLegacy(HighGuidTypeLegacy high)
	{
		this.high = high;
		if (!HighLegacyToHighType.ContainsKey(high))
		{
			throw new ArgumentOutOfRangeException("0x" + high.ToString("X"));
		}
		highGuidType = HighLegacyToHighType[high];
	}
}
