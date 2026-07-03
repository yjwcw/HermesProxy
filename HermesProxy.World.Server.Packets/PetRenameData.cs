namespace HermesProxy.World.Server.Packets;

internal struct PetRenameData
{
	public WowGuid128 PetGUID;

	public int PetNumber;

	public string NewName;

	public bool HasDeclinedNames;

	public DeclinedName DeclinedNames;
}
