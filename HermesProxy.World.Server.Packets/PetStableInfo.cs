namespace HermesProxy.World.Server.Packets;

internal class PetStableInfo
{
	public uint PetNumber;

	public uint CreatureID;

	public uint DisplayID;

	public uint ExperienceLevel;

	public byte LoyaltyLevel = 1;

	public byte PetFlags;

	public string PetName;
}
