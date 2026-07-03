using System;

namespace HermesProxy.World.Enums;

[Flags]
public enum AuraFlagsModern : ushort
{
	None = 0,
	NoCaster = 1,
	Cancelable = 2,
	Duration = 4,
	Scalable = 8,
	Negative = 0x10,
	Unk20 = 0x20,
	Unk40 = 0x40,
	Unk80 = 0x80,
	Positive = 0x100,
	Passive = 0x200
}
