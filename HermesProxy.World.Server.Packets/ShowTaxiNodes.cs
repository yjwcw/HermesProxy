using System.Collections.Generic;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

public class ShowTaxiNodes : ServerPacket
{
	public ShowTaxiNodesWindowInfo WindowInfo;

	public List<byte> CanLandNodes = new List<byte>();

	public List<byte> CanUseNodes = new List<byte>();

	public ShowTaxiNodes()
		: base(Opcode.SMSG_SHOW_TAXI_NODES)
	{
	}

	public override void Write()
	{
		_worldPacket.WriteBit(WindowInfo != null);
		_worldPacket.FlushBits();
		List<byte> list = new List<byte>(CanLandNodes);
		CleanupNodes(list);
		_worldPacket.WriteInt32(list.Count);
		List<byte> list2 = new List<byte>(CanUseNodes);
		CleanupNodes(list2);
		_worldPacket.WriteInt32(list2.Count);
		if (WindowInfo != null)
		{
			_worldPacket.WritePackedGuid128(WindowInfo.UnitGUID);
			_worldPacket.WriteUInt32(WindowInfo.CurrentNode);
		}
		foreach (byte item in list)
		{
			_worldPacket.WriteUInt8(item);
		}
		foreach (byte item2 in list2)
		{
			_worldPacket.WriteUInt8(item2);
		}
	}

	private void CleanupNodes(List<byte> nodes)
	{
		int num = -1;
		for (int i = 0; i < nodes.Count; i++)
		{
			if (nodes[i] != 0)
			{
				num = i;
			}
		}
		if (num + 1 != nodes.Count)
		{
			if (num == -1)
			{
				nodes.Clear();
			}
			else
			{
				nodes.RemoveRange(num + 1, nodes.Count - (num + 1));
			}
		}
	}
}
