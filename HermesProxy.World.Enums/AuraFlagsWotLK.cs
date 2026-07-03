using System;

namespace HermesProxy.World.Enums;

[Flags]
public enum AuraFlagsWotLK : ushort
{
	None = 0,
	EffectIndex0 = 1,
	EffectIndex1 = 2,
	EffectIndex2 = 4,
	NoCaster = 8,
	Positive = 0x10,
	Duration = 0x20,
	Unk2 = 0x40,
	Negative = 0x80
}
