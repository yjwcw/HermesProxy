using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets;

public class TaskProgress
{
	public uint TaskID;

	public uint FailureTime;

	public uint Flags;

	public uint Unk;

	public List<ushort> Progress = new List<ushort>();

	public void Write(WorldPacket data)
	{
		data.WriteUInt32(TaskID);
		data.WriteUInt32(FailureTime);
		data.WriteUInt32(Flags);
		data.WriteUInt32(Unk);
		data.WriteInt32(Progress.Count);
		foreach (ushort item in Progress)
		{
			data.WriteUInt16(item);
		}
	}
}
