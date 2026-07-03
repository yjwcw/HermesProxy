using System;

namespace HermesProxy.World.Enums;

[Flags]
public enum PetFlags : byte
{
	None = 0,
	CanBeRenamed = 1,
	CanBeAbandoned = 2
}
