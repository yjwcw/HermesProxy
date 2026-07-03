using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class TrainerListSpell
{
	public uint SpellID;

	public uint MoneyCost;

	public uint ReqSkillLine;

	public uint ReqSkillRank;

	public uint[] ReqAbility = new uint[3];

	public TrainerSpellStateModern Usable;

	public byte ReqLevel;
}
