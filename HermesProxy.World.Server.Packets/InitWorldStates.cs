using System.Collections.Generic;
using Framework.Constants;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class InitWorldStates : ServerPacket
{
	private struct WorldStateInfo
	{
		public uint VariableID;

		public int Value;

		public WorldStateInfo(uint variableID, int value)
		{
			VariableID = variableID;
			Value = value;
		}
	}

	public uint ZoneID;

	public uint AreaID;

	public uint MapID;

	private List<WorldStateInfo> Worldstates = new List<WorldStateInfo>();

	public InitWorldStates()
		: base(Opcode.SMSG_INIT_WORLD_STATES, ConnectionType.Instance)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteUInt32(MapID);
		_worldPacket.WriteUInt32(ZoneID);
		_worldPacket.WriteUInt32(AreaID);
		_worldPacket.WriteInt32(Worldstates.Count);
		foreach (WorldStateInfo worldstate in Worldstates)
		{
			_worldPacket.WriteUInt32(worldstate.VariableID);
			_worldPacket.WriteInt32(worldstate.Value);
		}
	}

	public void AddState(uint variableID, int value)
	{
		Worldstates.Add(new WorldStateInfo(variableID, value));
	}

	public void AddState(uint variableID, bool value)
	{
		Worldstates.Add(new WorldStateInfo(variableID, value ? 1 : 0));
	}

	public void AddMissingState(uint variableID, int value)
	{
		foreach (WorldStateInfo worldstate in Worldstates)
		{
			if (worldstate.VariableID == variableID)
			{
				return;
			}
		}
		Worldstates.Add(new WorldStateInfo(variableID, value));
	}

	public void AddClassicStates()
	{
		if (ModernVersion.ExpansionVersion == 1)
		{
			AddMissingState(17101u, 1);
			AddMissingState(17222u, 1);
			AddMissingState(17223u, 1);
			AddMissingState(17224u, 1);
			AddMissingState(17225u, 1);
			AddMissingState(17226u, 1);
			AddMissingState(17227u, 1);
			AddMissingState(17228u, 1);
			AddMissingState(17229u, 1);
			AddMissingState(17230u, 1);
			AddMissingState(17231u, 1);
			AddMissingState(17232u, 1);
			AddMissingState(17233u, 1);
			AddMissingState(17234u, 1);
			AddMissingState(17424u, 1);
			AddMissingState(17430u, 1);
			AddMissingState(17478u, 1);
			AddMissingState(17560u, 1);
			AddMissingState(17640u, 1);
			AddMissingState(17641u, 1);
			AddMissingState(17642u, 1);
			AddMissingState(17643u, 1);
			AddMissingState(17647u, 1);
			AddMissingState(17648u, 1);
			AddMissingState(17687u, 1);
			AddMissingState(17697u, 1);
			AddMissingState(17698u, 1);
			AddMissingState(17704u, 1);
			AddMissingState(17705u, 1);
			AddMissingState(17706u, 1);
			AddMissingState(17707u, 1);
			AddMissingState(18261u, 1);
			AddMissingState(19361u, 1);
			AddMissingState(20281u, 1);
			AddMissingState(20470u, 1);
			AddMissingState(21260u, 1);
		}
		else
		{
			AddMissingState(17223u, 1);
			AddMissingState(17647u, 1);
			AddMissingState(17648u, 1);
			AddMissingState(20445u, 0);
			AddMissingState(20446u, 0);
			AddMissingState(20447u, 1);
			AddMissingState(20487u, 1);
			AddMissingState(20488u, 1);
			AddMissingState(20489u, 1);
			AddMissingState(20491u, 1);
			AddMissingState(20492u, 1);
			AddMissingState(20493u, 1);
			AddMissingState(20494u, 0);
			AddMissingState(20495u, 0);
			AddMissingState(20496u, 0);
			AddMissingState(20497u, 0);
			AddMissingState(20518u, 0);
			AddMissingState(20560u, 0);
			AddMissingState(20562u, 1);
			AddMissingState(20563u, 1);
			AddMissingState(20567u, 0);
			AddMissingState(20738u, 0);
			AddMissingState(20882u, 0);
			AddMissingState(21125u, 1);
			AddMissingState(21126u, 1);
			AddMissingState(21195u, 2725);
			AddMissingState(21196u, 2542);
			AddMissingState(21197u, 2203);
			AddMissingState(21198u, 1898);
			AddMissingState(21199u, 1453);
			AddMissingState(21200u, 2548);
			AddMissingState(21201u, 2391);
			AddMissingState(21202u, 2086);
			AddMissingState(21203u, 1777);
			AddMissingState(21204u, 1431);
			AddMissingState(21205u, 2354);
			AddMissingState(21206u, 2181);
			AddMissingState(21207u, 1922);
			AddMissingState(21208u, 1686);
			AddMissingState(21209u, 1408);
			AddMissingState(21238u, 2);
		}
	}
}
