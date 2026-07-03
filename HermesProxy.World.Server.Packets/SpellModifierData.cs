namespace HermesProxy.World.Server.Packets;

public struct SpellModifierData
{
	public int ModifierValue;

	public byte ClassIndex;

	public void Write(WorldPacket data)
	{
		data.WriteInt32(ModifierValue);
		data.WriteUInt8(ClassIndex);
	}
}
