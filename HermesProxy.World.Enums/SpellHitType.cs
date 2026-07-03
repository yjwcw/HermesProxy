using System;

namespace HermesProxy.World.Enums;

[Flags]
public enum SpellHitType
{
	CritDebug = 1,
	Crit = 2,
	HitDebug = 4,
	Split = 8,
	VictimIsAttacker = 0x10,
	AttackTableDebug = 0x20,
	Unk = 0x40,
	NoAttacker = 0x80
}
