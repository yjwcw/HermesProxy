using System;

namespace HermesProxy.World.Enums;

[Flags]
public enum NPCFlagsVanilla
{
	None = 0,
	Gossip = 1,
	QuestGiver = 2,
	Vendor = 4,
	FlightMaster = 8,
	Trainer = 0x10,
	SpiritHealer = 0x20,
	SpiritGuide = 0x40,
	Innkeeper = 0x80,
	Banker = 0x100,
	Petitioner = 0x200,
	TabardDesigner = 0x400,
	BattleMaster = 0x800,
	Auctioneer = 0x1000,
	StableMaster = 0x2000,
	Repair = 0x4000
}
