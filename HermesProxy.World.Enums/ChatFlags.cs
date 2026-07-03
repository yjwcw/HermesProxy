using System;

namespace HermesProxy.World.Enums;

[Flags]
public enum ChatFlags
{
	None = 0,
	AFK = 1,
	DND = 2,
	GM = 4,
	Com = 8,
	Dev = 0x10,
	BossSound = 0x20,
	Mobile = 0x40
}
