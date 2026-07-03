namespace HermesProxy.World.Server.Packets;

public class QuestRewards
{
	public uint ChoiceItemCount;

	public uint ItemCount;

	public uint Money;

	public uint XP;

	public uint ArtifactXP;

	public uint ArtifactCategoryID;

	public uint Honor;

	public uint Title;

	public uint FactionFlags;

	public int[] SpellCompletionDisplayID = new int[3];

	public uint SpellCompletionID;

	public uint SkillLineID;

	public uint NumSkillUps;

	public uint TreasurePickerID;

	public QuestChoiceItem[] ChoiceItems = new QuestChoiceItem[6];

	public uint[] ItemID = new uint[4];

	public uint[] ItemQty = new uint[4];

	public uint[] FactionID = new uint[5];

	public int[] FactionValue = new int[5];

	public int[] FactionOverride = new int[5];

	public int[] FactionCapIn = new int[5];

	public uint[] CurrencyID = new uint[4];

	public uint[] CurrencyQty = new uint[4];

	public bool IsBoostSpell;

	public QuestRewards()
	{
		for (int i = 0; i < 6; i++)
		{
			ChoiceItems[i] = new QuestChoiceItem();
		}
	}

	public void Write(WorldPacket data)
	{
		data.WriteUInt32(ChoiceItemCount);
		data.WriteUInt32(ItemCount);
		for (int i = 0; i < 4; i++)
		{
			data.WriteUInt32(ItemID[i]);
			data.WriteUInt32(ItemQty[i]);
		}
		data.WriteUInt32(Money);
		data.WriteUInt32(XP);
		data.WriteUInt64(ArtifactXP);
		data.WriteUInt32(ArtifactCategoryID);
		data.WriteUInt32(Honor);
		data.WriteUInt32(Title);
		data.WriteUInt32(FactionFlags);
		for (int j = 0; j < 5; j++)
		{
			data.WriteUInt32(FactionID[j]);
			data.WriteInt32(FactionValue[j]);
			data.WriteInt32(FactionOverride[j]);
			data.WriteInt32(FactionCapIn[j]);
		}
		int[] spellCompletionDisplayID = SpellCompletionDisplayID;
		foreach (int data2 in spellCompletionDisplayID)
		{
			data.WriteInt32(data2);
		}
		data.WriteUInt32(SpellCompletionID);
		for (int l = 0; l < 4; l++)
		{
			data.WriteUInt32(CurrencyID[l]);
			data.WriteUInt32(CurrencyQty[l]);
		}
		data.WriteUInt32(SkillLineID);
		data.WriteUInt32(NumSkillUps);
		data.WriteUInt32(TreasurePickerID);
		QuestChoiceItem[] choiceItems = ChoiceItems;
		for (int k = 0; k < choiceItems.Length; k++)
		{
			choiceItems[k].Write(data);
		}
		data.WriteBit(IsBoostSpell);
		data.FlushBits();
	}
}
