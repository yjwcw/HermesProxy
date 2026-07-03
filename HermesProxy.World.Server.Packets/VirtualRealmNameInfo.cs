using System;

namespace HermesProxy.World.Server.Packets;

internal struct VirtualRealmNameInfo
{
	public bool IsLocal;

	public bool IsInternalRealm;

	public string RealmNameActual;

	public string RealmNameNormalized;

	public VirtualRealmNameInfo(bool isHomeRealm, bool isInternalRealm, string realmNameActual, string realmNameNormalized)
	{
		IsLocal = isHomeRealm;
		IsInternalRealm = isInternalRealm;
		RealmNameActual = realmNameActual;
		RealmNameNormalized = realmNameNormalized;
	}

	public void Write(WorldPacket data)
	{
		data.WriteBit(IsLocal);
		data.WriteBit(IsInternalRealm);
		data.WriteBits(RealmNameActual.GetByteCount(), 8);
		data.WriteBits(RealmNameNormalized.GetByteCount(), 8);
		data.FlushBits();
		data.WriteString(RealmNameActual);
		data.WriteString(RealmNameNormalized);
	}
}
