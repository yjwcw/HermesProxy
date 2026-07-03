using System;

namespace HermesProxy.World.Enums;

[Flags]
public enum NPCFlags : uint
{
	None = 0u,
	Gossip = 1u,
	QuestGiver = 2u,
	Unk1 = 4u,
	Unk2 = 8u,
	Trainer = 0x10u,
	TrainerClass = 0x20u,
	TrainerProfession = 0x40u,
	Vendor = 0x80u,
	VendorAmmo = 0x100u,
	VendorFood = 0x200u,
	VendorPoison = 0x400u,
	VendorReagent = 0x800u,
	Repair = 0x1000u,
	FlightMaster = 0x2000u,
	SpiritHealer = 0x4000u,
	SpiritGuide = 0x8000u,
	Innkeeper = 0x10000u,
	Banker = 0x20000u,
	Petitioner = 0x40000u,
	TabardDesigner = 0x80000u,
	BattleMaster = 0x100000u,
	Auctioneer = 0x200000u,
	StableMaster = 0x400000u,
	GuildBanker = 0x800000u,
	SpellClick = 0x1000000u,
	PlayerVehicle = 0x2000000u,
	Mailbox = 0x4000000u,
	ArtifactPowerRespec = 0x8000000u,
	Transmogrifier = 0x10000000u,
	VaultKeeper = 0x20000000u,
	WildBattlePet = 0x40000000u,
	BlackMarket = 0x80000000u
}
