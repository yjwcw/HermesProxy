using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class ConfirmRespecWipe : ClientPacket
{
	public WowGuid128 TrainerGUID;

	public SpecResetType RespecType;

	public ConfirmRespecWipe(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		TrainerGUID = _worldPacket.ReadPackedGuid128();
		RespecType = (SpecResetType)_worldPacket.ReadUInt8();
	}
}
