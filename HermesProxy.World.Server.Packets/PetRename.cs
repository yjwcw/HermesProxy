namespace HermesProxy.World.Server.Packets;

internal class PetRename : ClientPacket
{
	public PetRenameData RenameData;

	public PetRename(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		RenameData.PetGUID = _worldPacket.ReadPackedGuid128();
		RenameData.PetNumber = _worldPacket.ReadInt32();
		uint length = _worldPacket.ReadBits<uint>(8);
		RenameData.HasDeclinedNames = _worldPacket.HasBit();
		if (RenameData.HasDeclinedNames)
		{
			RenameData.DeclinedNames = new DeclinedName();
			uint[] array = new uint[5];
			for (int i = 0; i < 5; i++)
			{
				array[i] = _worldPacket.ReadBits<uint>(7);
			}
			for (int j = 0; j < 5; j++)
			{
				RenameData.DeclinedNames.name[j] = _worldPacket.ReadString(array[j]);
			}
		}
		RenameData.NewName = _worldPacket.ReadString(length);
	}
}
