using System;
using System.Collections.Generic;
using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

internal class AttackerStateUpdate : ServerPacket
{
	public HitInfo HitInfo;

	public WowGuid128 AttackerGUID;

	public WowGuid128 VictimGUID;

	public int Damage;

	public int OriginalDamage;

	public int OverDamage = -1;

	public List<SubDamage> SubDmg = new List<SubDamage>();

	public byte VictimState;

	public int AttackerState;

	public uint MeleeSpellID;

	public int BlockAmount;

	public int RageGained;

	public UnkAttackerState UnkState;

	public float Unk;

	public ContentTuningParams ContentTuning = new ContentTuningParams();

	public SpellCastLogData LogData;

	public AttackerStateUpdate()
		: base(Opcode.SMSG_ATTACKER_STATE_UPDATE, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		WorldPacket worldPacket = new WorldPacket();
		worldPacket.WriteUInt32((uint)HitInfo);
		worldPacket.WritePackedGuid128(AttackerGUID);
		worldPacket.WritePackedGuid128(VictimGUID);
		worldPacket.WriteInt32(Damage);
		worldPacket.WriteInt32(OriginalDamage);
		worldPacket.WriteInt32(OverDamage);
		worldPacket.WriteUInt8((byte)SubDmg.Count);
		foreach (SubDamage item in SubDmg)
		{
			worldPacket.WriteUInt32(item.SchoolMask);
			worldPacket.WriteFloat(item.FloatDamage);
			worldPacket.WriteInt32(item.IntDamage);
			if (HitInfo.HasAnyFlag(HitInfo.FullAbsorb | HitInfo.PartialAbsorb))
			{
				worldPacket.WriteInt32(item.Absorbed);
			}
			if (HitInfo.HasAnyFlag(HitInfo.FullResist | HitInfo.PartialResist))
			{
				worldPacket.WriteInt32(item.Resisted);
			}
		}
		worldPacket.WriteUInt8(VictimState);
		worldPacket.WriteInt32(AttackerState);
		worldPacket.WriteUInt32(MeleeSpellID);
		if (HitInfo.HasAnyFlag(HitInfo.Block))
		{
			worldPacket.WriteInt32(BlockAmount);
		}
		if (HitInfo.HasAnyFlag(HitInfo.RageGain))
		{
			worldPacket.WriteInt32(RageGained);
		}
		if (HitInfo.HasAnyFlag(HitInfo.Unk0))
		{
			worldPacket.WriteUInt32(UnkState.State1);
			worldPacket.WriteFloat(UnkState.State2);
			worldPacket.WriteFloat(UnkState.State3);
			worldPacket.WriteFloat(UnkState.State4);
			worldPacket.WriteFloat(UnkState.State5);
			worldPacket.WriteFloat(UnkState.State6);
			worldPacket.WriteFloat(UnkState.State7);
			worldPacket.WriteFloat(UnkState.State8);
			worldPacket.WriteFloat(UnkState.State9);
			worldPacket.WriteFloat(UnkState.State10);
			worldPacket.WriteFloat(UnkState.State11);
			worldPacket.WriteUInt32(UnkState.State12);
		}
		if (HitInfo.HasAnyFlag(HitInfo.Unk12 | HitInfo.Block))
		{
			worldPacket.WriteFloat(Unk);
		}
		worldPacket.WriteUInt8((byte)ContentTuning.TuningType);
		worldPacket.WriteUInt8(ContentTuning.TargetLevel);
		worldPacket.WriteUInt8(ContentTuning.Expansion);
		worldPacket.WriteInt16(ContentTuning.PlayerLevelDelta);
		worldPacket.WriteFloat(ContentTuning.PlayerItemLevel);
		worldPacket.WriteFloat(ContentTuning.TargetItemLevel);
		_worldPacket.WriteBit(LogData != null);
		if (LogData != null)
		{
			LogData.Write(_worldPacket);
		}
		_worldPacket.FlushBits();
		_worldPacket.WriteUInt32(worldPacket.GetSize());
		_worldPacket.WriteBytes(worldPacket);
	}
}
