using System;

namespace HermesProxy.World.Enums;

[Flags]
public enum ItemSearchLocation
{
	Equipment = 1,
	Inventory = 2,
	Bank = 4,
	ReagentBank = 8,
	Default = 3,
	Everywhere = 0xF
}
