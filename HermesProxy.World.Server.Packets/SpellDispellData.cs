namespace HermesProxy.World.Server.Packets;

internal struct SpellDispellData
{
	public uint SpellID;

	public bool Harmful;

	public int? Rolled;

	public int? Needed;
}
