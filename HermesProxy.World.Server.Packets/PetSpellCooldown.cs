namespace HermesProxy.World.Server.Packets;

public class PetSpellCooldown
{
	public uint SpellID;

	public uint Duration;

	public uint CategoryDuration;

	public float ModRate = 1f;

	public ushort Category;
}
