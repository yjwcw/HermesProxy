using System;

namespace Framework.Constants;

[Flags]
public enum RealmFlags
{
	None = 0,
	VersionMismatch = 1,
	Offline = 2,
	SpecifyBuild = 4,
	Unk1 = 8,
	Unk2 = 0x10,
	Recommended = 0x20,
	New = 0x40,
	Full = 0x80
}
