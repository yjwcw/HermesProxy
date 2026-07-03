using System;

namespace HermesProxy.World.Server.Packets;

public class ArenaTeamEmblem
{
	public uint TeamId;

	public uint TeamSize;

	public uint BackgroundColor;

	public uint EmblemStyle;

	public uint EmblemColor;

	public uint BorderStyle;

	public uint BorderColor;

	public string TeamName;

	public void Write(WorldPacket data)
	{
		data.WriteUInt32(TeamId);
		data.WriteUInt32(TeamSize);
		data.WriteUInt32(BackgroundColor);
		data.WriteUInt32(EmblemStyle);
		data.WriteUInt32(EmblemColor);
		data.WriteUInt32(BorderStyle);
		data.WriteUInt32(BorderColor);
		data.WriteBits(TeamName.GetByteCount(), 7);
		data.FlushBits();
		data.WriteString(TeamName);
	}
}
