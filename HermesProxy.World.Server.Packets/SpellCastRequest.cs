using System.Collections.Generic;
using HermesProxy.World.Objects;

namespace HermesProxy.World.Server.Packets;

public class SpellCastRequest
{
	public WowGuid128 CastID;

	public uint SpellID;

	public uint SpellXSpellVisualID;

	public uint SendCastFlags;

	public SpellTargetData Target = new SpellTargetData();

	public MissileTrajectoryRequest MissileTrajectory;

	public WowGuid128 MoverGUID;

	public MovementInfo MoveUpdate;

	public List<SpellWeight> Weight = new List<SpellWeight>();

	public Array<SpellOptionalReagent> OptionalReagents = new Array<SpellOptionalReagent>(3);

	public Array<SpellExtraCurrencyCost> OptionalCurrencies = new Array<SpellExtraCurrencyCost>(5);

	public WowGuid128 CraftingNPC;

	public uint[] Misc = new uint[2];

	public void Read(WorldPacket data)
	{
		CastID = data.ReadPackedGuid128();
		Misc[0] = data.ReadUInt32();
		Misc[1] = data.ReadUInt32();
		SpellID = data.ReadUInt32();
		SpellXSpellVisualID = data.ReadUInt32();
		MissileTrajectory.Read(data);
		CraftingNPC = data.ReadPackedGuid128();
		uint num = data.ReadUInt32();
		uint num2 = data.ReadUInt32();
		for (int i = 0; i < num; i++)
		{
			OptionalReagents[i].Read(data);
		}
		for (int j = 0; j < num2; j++)
		{
			OptionalCurrencies[j].Read(data);
		}
		SendCastFlags = data.ReadBits<uint>(5);
		if (data.HasBit())
		{
			MoveUpdate = new MovementInfo();
		}
		uint num3 = data.ReadBits<uint>(2);
		Target.Read(data);
		if (MoveUpdate != null)
		{
			MoverGUID = data.ReadPackedGuid128();
			MoveUpdate.ReadMovementInfoModern(data);
		}
		SpellWeight spellWeight = default(SpellWeight);
		for (int k = 0; k < num3; k++)
		{
			data.ResetBitPos();
			spellWeight.Type = data.ReadBits<uint>(2);
			spellWeight.ID = data.ReadInt32();
			spellWeight.Quantity = data.ReadUInt32();
			Weight.Add(spellWeight);
		}
	}
}
