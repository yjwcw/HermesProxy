namespace HermesProxy.World.Server.Packets;

public class SpellCooldownStruct
{
	public uint SpellID;

	public uint ForcedCooldown;

	public float ModRate = 1f;

	public void Write(WorldPacket data)
	{
		data.WriteUInt32(SpellID);
		data.WriteUInt32(ForcedCooldown);
		data.WriteFloat(ModRate);
	}
}
