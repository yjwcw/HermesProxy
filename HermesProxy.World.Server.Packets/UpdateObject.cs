using System;
using System.Collections.Generic;
using Framework.Constants;
using HermesProxy.Enums;
using HermesProxy.World.Client;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects.Version.V1_14_0_40237;
using HermesProxy.World.Objects.Version.V1_14_1_40688;
using HermesProxy.World.Objects.Version.V2_5_2_39570;
using HermesProxy.World.Objects.Version.V2_5_3_41750;

namespace HermesProxy.World.Server.Packets;

public class UpdateObject : ServerPacket
{
    //private GameSessionData _gameState;
    public GameSessionData _gameState;

    public uint NumObjUpdates;

	public ushort MapID;

	public byte[] Data;

	public List<WowGuid128> OutOfRangeGuids = new List<WowGuid128>();

	public List<WowGuid128> DestroyedGuids = new List<WowGuid128>();

	public List<ObjectUpdate> ObjectUpdates = new List<ObjectUpdate>();

	public UpdateObject(GameSessionData gameState)
		: base(Opcode.SMSG_UPDATE_OBJECT, ConnectionType.Instance)
	{
        _gameState = gameState;
	}

	public override void Write()
	{
		NumObjUpdates = (uint)ObjectUpdates.Count;
		MapID = (ushort)_gameState.CurrentMapId.Value;
		_worldPacket.WriteUInt32(NumObjUpdates);
		_worldPacket.WriteUInt16(MapID);
		WorldPacket worldPacket = new WorldPacket();
		if (worldPacket.WriteBit(!OutOfRangeGuids.Empty() || !DestroyedGuids.Empty()))
		{
			worldPacket.WriteUInt16((ushort)DestroyedGuids.Count);
			worldPacket.WriteInt32(DestroyedGuids.Count + OutOfRangeGuids.Count);
			foreach (WowGuid128 destroyedGuid in DestroyedGuids)
			{
				worldPacket.WritePackedGuid128(destroyedGuid);
			}
			foreach (WowGuid128 outOfRangeGuid in OutOfRangeGuids)
			{
				worldPacket.WritePackedGuid128(outOfRangeGuid);
			}
		}
		WorldPacket worldPacket2 = new WorldPacket();
		foreach (ObjectUpdate objectUpdate in ObjectUpdates)
		{
			objectUpdate.InitializePlaceholders();
			switch (ModernVersion.GetUpdateFieldsDefiningBuild())
			{
			case ClientVersionBuild.V1_14_0_40237:
				new HermesProxy.World.Objects.Version.V1_14_0_40237.ObjectUpdateBuilder(objectUpdate, _gameState).WriteToPacket(worldPacket2);
				break;
			case ClientVersionBuild.V1_14_1_40688:
				new HermesProxy.World.Objects.Version.V1_14_1_40688.ObjectUpdateBuilder(objectUpdate, _gameState).WriteToPacket(worldPacket2);
				break;
			case ClientVersionBuild.V2_5_2_39570:
				new HermesProxy.World.Objects.Version.V2_5_2_39570.ObjectUpdateBuilder(objectUpdate, _gameState).WriteToPacket(worldPacket2);
				break;
			case ClientVersionBuild.V2_5_3_41750:
				new HermesProxy.World.Objects.Version.V2_5_3_41750.ObjectUpdateBuilder(objectUpdate, _gameState).WriteToPacket(worldPacket2);
				break;
			default:
				throw new ArgumentOutOfRangeException("No object update builder defined for current build.");
			}
		}
		byte[] data = worldPacket2.GetData();
		worldPacket.WriteInt32(data.Length);
		worldPacket.WriteBytes(data);
		Data = worldPacket.GetData();
		_worldPacket.WriteBytes(Data);
	}
}
