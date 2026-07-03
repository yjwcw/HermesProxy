using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class PlaySpellVisualKit : ServerPacket
{
	public WowGuid128 Unit;

	public uint KitRecID;

	public uint KitType;

	public uint Duration;

	public bool MountedVisual;

	public PlaySpellVisualKit()
		: base(Opcode.SMSG_PLAY_SPELL_VISUAL_KIT)
	{
	}

	public override void Write()
	{
		_worldPacket.WritePackedGuid128(Unit);
		_worldPacket.WriteUInt32(KitRecID);
		_worldPacket.WriteUInt32(KitType);
		_worldPacket.WriteUInt32(Duration);
		_worldPacket.WriteBit(MountedVisual);
		_worldPacket.FlushBits();
	}
}
