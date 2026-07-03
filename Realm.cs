using System;
using System.Net;
using Framework;
using Framework.Constants;
using Framework.Realm;

public class Realm : IEquatable<Realm>
{
	private uint[] ConfigIdByType = new uint[14]
	{
		1u, 2u, 3u, 4u, 5u, 6u, 7u, 8u, 9u, 10u,
		11u, 12u, 13u, 14u
	};

	public RealmId Id;

	public uint Build;

	public string ExternalAddress;

	public ushort Port;

	public string Name;

	public string NormalizedName;

	public byte Type;

	public RealmFlags Flags;

	public byte CharacterCount;

	public byte Timezone;

	public float PopulationLevel;

	public void SetName(string name)
	{
		Name = name;
		NormalizedName = name;
		NormalizedName = NormalizedName.Replace(" ", "");
	}

	public IPEndPoint GetAddressForClient(IPAddress clientAddr)
	{
		IPAddress iPAddress = ((!IPAddress.IsLoopback(clientAddr)) ? IPAddress.Parse(Settings.ExternalAddress) : IPAddress.Parse("127.0.0.1"));
		return new IPEndPoint(iPAddress, Settings.RealmPort);
	}

	public uint GetConfigId()
	{
		return ConfigIdByType[Type];
	}

	public override bool Equals(object obj)
	{
		if (obj != null && obj is Realm)
		{
			return Equals((Realm)obj);
		}
		return false;
	}

	public bool Equals(Realm other)
	{
		if (other.ExternalAddress.Equals(ExternalAddress) && other.Port == Port && other.Name == Name && other.Type == Type && other.Flags == Flags && other.CharacterCount == CharacterCount && other.Timezone == Timezone)
		{
			return other.PopulationLevel == PopulationLevel;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return new { ExternalAddress, Port, Name, Type, Flags, CharacterCount, Timezone, PopulationLevel }.GetHashCode();
	}
}
