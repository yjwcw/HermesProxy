using System;

namespace HermesProxy.World.Enums;

[Flags]
public enum ObjectTypeMask
{
	Object = 1,
	Item = 2,
	Container = 4,
	Unit = 8,
	Player = 0x10,
	ActivePlayer = 0x20,
	GameObject = 0x40,
	DynamicObject = 0x80,
	Corpse = 0x100,
	AreaTrigger = 0x200,
	Sceneobject = 0x400,
	Conversation = 0x800,
	Seer = 0x98
}
