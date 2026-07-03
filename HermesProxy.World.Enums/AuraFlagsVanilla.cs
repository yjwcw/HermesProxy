using System;

namespace HermesProxy.World.Enums;

[Flags]
public enum AuraFlagsVanilla : ushort
{
	None = 0,
	Cancelable = 1,
	EffectIndex2 = 2,
	EffectIndex1 = 4,
	EffectIndex0 = 8
}
