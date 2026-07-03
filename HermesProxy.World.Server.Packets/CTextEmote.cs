namespace HermesProxy.World.Server.Packets;

public class CTextEmote : ClientPacket
{
	public WowGuid128 Target;

	public int EmoteID;

	public int SoundIndex;

	public int SequenceVariation;

	public uint[] SpellVisualKitIDs;

	public CTextEmote(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		Target = _worldPacket.ReadPackedGuid128();
		EmoteID = _worldPacket.ReadInt32();
		SoundIndex = _worldPacket.ReadInt32();
		if (ModernVersion.AddedInVersion(9, 0, 5, 1, 14, 0, 2, 5, 1))
		{
			SpellVisualKitIDs = new uint[_worldPacket.ReadUInt32()];
			if (ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 2, 2, 5, 3))
			{
				SequenceVariation = _worldPacket.ReadInt32();
			}
			for (int i = 0; i < SpellVisualKitIDs.Length; i++)
			{
				SpellVisualKitIDs[i] = _worldPacket.ReadUInt32();
			}
		}
	}
}
