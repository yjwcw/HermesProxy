using System;

namespace HermesProxy.World.Enums;

[Flags]
public enum SpellCastTargetFlags
{
	None = 0,
	Unused1 = 1,
	Unit = 2,
	UnitRaid = 4,
	UnitParty = 8,
	Item = 0x10,
	SourceLocation = 0x20,
	DestLocation = 0x40,
	UnitEnemy = 0x80,
	UnitAlly = 0x100,
	CorpseEnemy = 0x200,
	UnitDead = 0x400,
	GameObject = 0x800,
	TradeItem = 0x1000,
	String = 0x2000,
	GameobjectItem = 0x4000,
	CorpseAlly = 0x8000,
	UnitMinipet = 0x10000,
	GlyphSlot = 0x20000,
	DestTarget = 0x40000,
	ExtraTargets = 0x80000,
	UnitPassenger = 0x100000,
	Unk400000 = 0x400000,
	Unk1000000 = 0x1000000,
	Unk4000000 = 0x4000000,
	Unk10000000 = 0x10000000,
	Unk40000000 = 0x40000000,
	UnitMask = 0x11058E,
	GameobjectMask = 0x4800,
	CorpseMask = 0x8200,
	ItemMask = 0x5010
}
