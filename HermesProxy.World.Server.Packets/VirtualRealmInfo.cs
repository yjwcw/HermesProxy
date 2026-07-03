namespace HermesProxy.World.Server.Packets;

internal struct VirtualRealmInfo
{
	public uint RealmAddress;

	public VirtualRealmNameInfo RealmNameInfo;

	public VirtualRealmInfo(uint realmAddress, bool isHomeRealm, bool isInternalRealm, string realmNameActual, string realmNameNormalized)
	{
		RealmAddress = realmAddress;
		RealmNameInfo = new VirtualRealmNameInfo(isHomeRealm, isInternalRealm, realmNameActual, realmNameNormalized);
	}

	public void Write(WorldPacket data)
	{
		data.WriteUInt32(RealmAddress);
		RealmNameInfo.Write(data);
	}
}
