using System.Collections.Generic;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class CharacterCreateInfo
{
	public Race RaceId;

	public Class ClassId;

	public Gender Sex = Gender.None;

	public Array<ChrCustomizationChoice> Customizations = new Array<ChrCustomizationChoice>(50);

	public uint? TemplateSet;

	public bool IsTrialBoost;

	public bool UseNPE;

	public string Name;

	public byte CharCount;
}
