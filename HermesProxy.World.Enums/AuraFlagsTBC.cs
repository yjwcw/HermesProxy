using System;

namespace HermesProxy.World.Enums;

[Flags]
public enum AuraFlagsTBC : ushort
{
	None = 0,
	EffectIndex0 = 1,
	EffectIndex1 = 2,
	EffectIndex2 = 4,
	Unk4 = 8,
	Cancelable = 0x10,
	NotCancelable = 0x20
}
