using System.Collections.Generic;
using Framework.Collections;

namespace HermesProxy.World.Objects;

public class CreatureTemplate
{
	public string Title;

	public string TitleAlt;

	public string CursorName;

	public int Type;

	public int Family;

	public int Classification;

	public uint PetSpellDataId;

	public CreatureDisplayStats Display = new CreatureDisplayStats();

	public float HpMulti;

	public float EnergyMulti;

	public bool Civilian;

	public bool Leader;

	public List<uint> QuestItems = new List<uint>();

	public uint MovementInfoID;

	public int HealthScalingExpansion;

	public uint RequiredExpansion;

	public uint VignetteID;

	public int Class;

	public int DifficultyID;

	public int WidgetSetID;

	public int WidgetSetUnitConditionID;

	public uint[] Flags = new uint[2];

	public uint[] ProxyCreatureID = new uint[2];

	public StringArray Name = new StringArray(4);

	public StringArray NameAlt = new StringArray(4);
}
