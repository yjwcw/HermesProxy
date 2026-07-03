using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets;

internal class AuctionListItems : ClientPacket
{
	public uint Offset;

	public WowGuid128 Auctioneer;

	public byte MinLevel;

	public byte MaxLevel;

	public int Quality;

	public byte MaxPetLevel;

	public List<byte> KnownPets = new List<byte>();

	public string Name;

	public bool OnlyUsable;

	public bool ExactMatch;

	public List<ClassFilter> ClassFilters = new List<ClassFilter>();

	public List<AuctionSort> Sorts = new List<AuctionSort>();

	public AuctionListItems(WorldPacket packet)
		: base(packet)
	{
	}

	public override void Read()
	{
		Offset = _worldPacket.ReadUInt32();
		Auctioneer = _worldPacket.ReadPackedGuid128();
		MinLevel = _worldPacket.ReadUInt8();
		MaxLevel = _worldPacket.ReadUInt8();
		Quality = _worldPacket.ReadInt32();
		byte b = _worldPacket.ReadUInt8();
		uint num = _worldPacket.ReadUInt32();
		MaxPetLevel = _worldPacket.ReadUInt8();
		for (int i = 0; i < num; i++)
		{
			KnownPets.Add(_worldPacket.ReadUInt8());
		}
		uint length = _worldPacket.ReadBits<uint>(8);
		Name = _worldPacket.ReadString(length);
		uint num2 = _worldPacket.ReadBits<uint>(3);
		OnlyUsable = _worldPacket.HasBit();
		ExactMatch = _worldPacket.HasBit();
		_worldPacket.ResetBitPos();
		for (int j = 0; j < num2; j++)
		{
			ClassFilter classFilter = new ClassFilter();
			classFilter.ItemClass = _worldPacket.ReadInt32();
			uint num3 = _worldPacket.ReadBits<uint>(5);
			for (uint num4 = 0u; num4 < num3; num4++)
			{
				SubClassFilter subClassFilter = default(SubClassFilter);
				subClassFilter.ItemSubclass = _worldPacket.ReadInt32();
				subClassFilter.InvTypeMask = _worldPacket.ReadUInt32();
				classFilter.SubClassFilters.Add(subClassFilter);
			}
			ClassFilters.Add(classFilter);
		}
		uint count = _worldPacket.ReadUInt32();
		byte[] data = _worldPacket.ReadBytes(count);
		WorldPacket worldPacket = new WorldPacket(_worldPacket.GetOpcode(), data);
		for (int k = 0; k < b; k++)
		{
			AuctionSort auctionSort = default(AuctionSort);
			auctionSort.Type = worldPacket.ReadUInt8();
			auctionSort.Direction = worldPacket.ReadUInt8();
			Sorts.Add(auctionSort);
		}
	}
}
