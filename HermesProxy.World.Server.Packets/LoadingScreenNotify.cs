namespace HermesProxy.World.Server.Packets;

public class LoadingScreenNotify : ClientPacket
{
	public uint MapID;

	public bool Showing;

	public LoadingScreenNotify(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		MapID = _worldPacket.ReadUInt32();
		Showing = _worldPacket.HasBit();
	}
}
