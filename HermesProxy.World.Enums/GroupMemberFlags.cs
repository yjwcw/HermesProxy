using System;

namespace HermesProxy.World.Enums;

[Flags]
public enum GroupMemberFlags
{
	None = 0,
	Assistant = 1,
	MainTank = 2,
	MainAssist = 4
}
