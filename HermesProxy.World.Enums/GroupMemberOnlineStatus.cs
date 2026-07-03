using System;

namespace HermesProxy.World.Enums;

[Flags]
public enum GroupMemberOnlineStatus
{
	Offline = 0,
	Online = 1,
	PVP = 2,
	Dead = 4,
	Ghost = 8,
	PVPFFA = 0x10,
	Unk3 = 0x20,
	AFK = 0x40,
	DND = 0x80,
	RAF = 0x100,
	Vehicle = 0x200
}
