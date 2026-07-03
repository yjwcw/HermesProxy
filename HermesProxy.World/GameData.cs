using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Framework.IO;
using Framework.Logging;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using Microsoft.VisualBasic.FileIO;

namespace HermesProxy.World;

public static class GameData
{
	public static Dictionary<uint, Dictionary<string, byte[]>> BuildAuthSeeds = new Dictionary<uint, Dictionary<string, byte[]>>();

	public static SortedDictionary<uint, BroadcastText> BroadcastTextStore = new SortedDictionary<uint, BroadcastText>();

	public static Dictionary<uint, uint> ItemDisplayIdStore = new Dictionary<uint, uint>();

	public static Dictionary<uint, uint> ItemDisplayIdToFileDataIdStore = new Dictionary<uint, uint>();

	public static Dictionary<uint, ItemSpellsData> ItemSpellsDataStore = new Dictionary<uint, ItemSpellsData>();

	public static Dictionary<uint, ItemRecord> ItemRecordsStore = new Dictionary<uint, ItemRecord>();

	public static Dictionary<uint, ItemSparseRecord> ItemSparseRecordsStore = new Dictionary<uint, ItemSparseRecord>();

	public static Dictionary<uint, ItemAppearance> ItemAppearanceStore = new Dictionary<uint, ItemAppearance>();

	public static Dictionary<uint, ItemModifiedAppearance> ItemModifiedAppearanceStore = new Dictionary<uint, ItemModifiedAppearance>();

	public static Dictionary<uint, ItemEffect> ItemEffectStore = new Dictionary<uint, ItemEffect>();

	public static Dictionary<uint, Battleground> Battlegrounds = new Dictionary<uint, Battleground>();

	public static Dictionary<uint, ChatChannel> ChatChannels = new Dictionary<uint, ChatChannel>();

	public static Dictionary<uint, Dictionary<uint, byte>> ItemEffects = new Dictionary<uint, Dictionary<uint, byte>>();

	public static Dictionary<uint, uint> ItemEnchantVisuals = new Dictionary<uint, uint>();

	public static Dictionary<uint, uint> SpellVisuals = new Dictionary<uint, uint>();

	public static Dictionary<uint, uint> LearnSpells = new Dictionary<uint, uint>();

	public static Dictionary<uint, uint> TotemSpells = new Dictionary<uint, uint>();

	public static Dictionary<uint, uint> Gems = new Dictionary<uint, uint>();

	public static Dictionary<uint, CreatureDisplayInfo> CreatureDisplayInfos = new Dictionary<uint, CreatureDisplayInfo>();

	public static Dictionary<uint, CreatureModelCollisionHeight> CreatureModelCollisionHeights = new Dictionary<uint, CreatureModelCollisionHeight>();

	public static Dictionary<uint, uint> TransportPeriods = new Dictionary<uint, uint>();

	public static Dictionary<uint, string> AreaNames = new Dictionary<uint, string>();

	public static Dictionary<uint, uint> RaceFaction = new Dictionary<uint, uint>();

	public static HashSet<uint> DispellSpells = new HashSet<uint>();

	public static Dictionary<uint, List<float>> SpellEffectPoints = new Dictionary<uint, List<float>>();

	public static HashSet<uint> StackableAuras = new HashSet<uint>();

	public static HashSet<uint> MountAuras = new HashSet<uint>();

	public static HashSet<uint> NextMeleeSpells = new HashSet<uint>();

	public static HashSet<uint> AutoRepeatSpells = new HashSet<uint>();

	public static HashSet<uint> AuraSpells = new HashSet<uint>();

	public static Dictionary<uint, TaxiPath> TaxiPaths = new Dictionary<uint, TaxiPath>();

	public static int[,] TaxiNodesGraph = new int[250, 250];

	public static Dictionary<uint, uint> QuestBits = new Dictionary<uint, uint>();

	public static Dictionary<uint, ItemTemplate> ItemTemplates = new Dictionary<uint, ItemTemplate>();

	public static Dictionary<uint, CreatureTemplate> CreatureTemplates = new Dictionary<uint, CreatureTemplate>();

	public static Dictionary<uint, QuestTemplate> QuestTemplates = new Dictionary<uint, QuestTemplate>();

	public static Dictionary<uint, string> ItemNames = new Dictionary<uint, string>();

	public const uint HotfixAreaTriggerBegin = 100000u;

	public const uint HotfixSkillLineBegin = 110000u;

	public const uint HotfixSkillRaceClassInfoBegin = 120000u;

	public const uint HotfixSkillLineAbilityBegin = 130000u;

	public const uint HotfixSpellBegin = 140000u;

	public const uint HotfixSpellNameBegin = 150000u;

	public const uint HotfixSpellLevelsBegin = 160000u;

	public const uint HotfixSpellAuraOptionsBegin = 170000u;

	public const uint HotfixSpellMiscBegin = 180000u;

	public const uint HotfixSpellEffectBegin = 190000u;

	public const uint HotfixSpellXSpellVisualBegin = 200000u;

	public const uint HotfixItemBegin = 210000u;

	public const uint HotfixItemSparseBegin = 220000u;

	public const uint HotfixItemAppearanceBegin = 230000u;

	public const uint HotfixItemModifiedAppearanceBegin = 240000u;

	public const uint HotfixItemEffectBegin = 250000u;

	public const uint HotfixItemDisplayInfoBegin = 260000u;

	public const uint HotfixCreatureDisplayInfoBegin = 270000u;

	public const uint HotfixCreatureDisplayInfoExtraBegin = 280000u;

	public const uint HotfixCreatureDisplayInfoOptionBegin = 290000u;

	public static Dictionary<uint, HotfixRecord> Hotfixes = new Dictionary<uint, HotfixRecord>();

	public static void StoreItemName(uint entry, string name)
	{
		if (ItemNames.ContainsKey(entry))
		{
			ItemNames[entry] = name;
		}
		else
		{
			ItemNames.Add(entry, name);
		}
	}

	public static string GetItemName(uint entry)
	{
		if (ItemNames.TryGetValue(entry, out var value))
		{
			return value;
		}
		ItemTemplate itemTemplate = GetItemTemplate(entry);
		if (itemTemplate != null)
		{
			return itemTemplate.Name[0];
		}
		return "";
	}

	public static void StoreItemTemplate(uint entry, ItemTemplate template)
	{
		if (ItemTemplates.ContainsKey(entry))
		{
			ItemTemplates[entry] = template;
		}
		else
		{
			ItemTemplates.Add(entry, template);
		}
	}

	public static ItemTemplate GetItemTemplate(uint entry)
	{
		if (ItemTemplates.TryGetValue(entry, out var value))
		{
			return value;
		}
		return null;
	}

	public static void StoreQuestTemplate(uint entry, QuestTemplate template)
	{
		if (QuestTemplates.ContainsKey(entry))
		{
			QuestTemplates[entry] = template;
		}
		else
		{
			QuestTemplates.Add(entry, template);
		}
	}

	public static QuestTemplate GetQuestTemplate(uint entry)
	{
		if (QuestTemplates.TryGetValue(entry, out var value))
		{
			return value;
		}
		return null;
	}

	public static QuestObjective GetQuestObjectiveForItem(uint entry)
	{
		foreach (KeyValuePair<uint, QuestTemplate> questTemplate in QuestTemplates)
		{
			foreach (QuestObjective objective in questTemplate.Value.Objectives)
			{
				if (objective.ObjectID == entry && objective.Type == QuestObjectiveType.Item)
				{
					return objective;
				}
			}
		}
		return null;
	}

	public static uint? GetUniqueQuestBit(uint questId)
	{
		if (!QuestBits.TryGetValue(questId, out var value))
		{
			return null;
		}
		return value;
	}

	public static void StoreCreatureTemplate(uint entry, CreatureTemplate template)
	{
		if (CreatureTemplates.ContainsKey(entry))
		{
			CreatureTemplates[entry] = template;
		}
		else
		{
			CreatureTemplates.Add(entry, template);
		}
	}

	public static CreatureTemplate GetCreatureTemplate(uint entry)
	{
		if (CreatureTemplates.TryGetValue(entry, out var value))
		{
			return value;
		}
		return null;
	}

	public static uint GetItemDisplayId(uint entry)
	{
		if (ItemDisplayIdStore.TryGetValue(entry, out var value))
		{
			return value;
		}
		return 0u;
	}

	public static uint GetItemIdWithDisplayId(uint displayId)
	{
		foreach (KeyValuePair<uint, uint> item in ItemDisplayIdStore)
		{
			if (item.Value == displayId)
			{
				return item.Key;
			}
		}
		return 0u;
	}

	public static ItemAppearance GetItemAppearanceByDisplayId(uint displayId)
	{
		foreach (KeyValuePair<uint, ItemAppearance> item in ItemAppearanceStore)
		{
			if (item.Value.ItemDisplayInfoID == (int)displayId)
			{
				return item.Value;
			}
		}
		return null;
	}

	public static ItemAppearance GetItemAppearanceByItemId(uint itemId)
	{
		ItemModifiedAppearance itemModifiedAppearanceByItemId = GetItemModifiedAppearanceByItemId(itemId);
		if (itemModifiedAppearanceByItemId == null)
		{
			return null;
		}
		if (ItemAppearanceStore.TryGetValue((uint)itemModifiedAppearanceByItemId.ItemAppearanceID, out var value))
		{
			return value;
		}
		return null;
	}

	public static uint GetItemIconFileDataIdByDisplayId(uint displayId)
	{
		if (ItemDisplayIdToFileDataIdStore.TryGetValue(displayId, out var value))
		{
			return value;
		}
		return 0u;
	}

	public static ItemModifiedAppearance GetItemModifiedAppearanceByDisplayId(uint displayId)
	{
		ItemAppearance itemAppearanceByDisplayId = GetItemAppearanceByDisplayId(displayId);
		if (itemAppearanceByDisplayId != null)
		{
			foreach (KeyValuePair<uint, ItemModifiedAppearance> item in ItemModifiedAppearanceStore)
			{
				if (item.Value.ItemAppearanceID == itemAppearanceByDisplayId.Id)
				{
					return item.Value;
				}
			}
		}
		return null;
	}

	public static ItemModifiedAppearance GetItemModifiedAppearanceByItemId(uint itemId)
	{
		foreach (KeyValuePair<uint, ItemModifiedAppearance> item in ItemModifiedAppearanceStore)
		{
			if (item.Value.ItemID == (int)itemId)
			{
				return item.Value;
			}
		}
		return null;
	}

	public static ItemEffect GetItemEffectByItemId(uint itemId, byte slot)
	{
		foreach (KeyValuePair<uint, ItemEffect> item in ItemEffectStore)
		{
			if (item.Value.ParentItemID == itemId && item.Value.LegacySlotIndex == slot)
			{
				return item.Value;
			}
		}
		return null;
	}

	public static uint GetFirstFreeId(IDictionary dict, uint after = 0u)
	{
		uint num = 0u;
		foreach (object item in dict)
		{
			object value = item.GetType().GetProperty("Key").GetValue(item, null);
			if (after == 0 || (uint)value > after)
			{
				num = (uint)value;
				break;
			}
		}
		for (; dict.Contains(num); num++)
		{
		}
		return num;
	}

	public static void SaveItemEffectSlot(uint itemId, uint spellId, byte slot)
	{
		if (ItemEffects.ContainsKey(itemId))
		{
			if (ItemEffects[itemId].ContainsKey(spellId))
			{
				ItemEffects[itemId][spellId] = slot;
			}
			else
			{
				ItemEffects[itemId].Add(spellId, slot);
			}
		}
		else
		{
			Dictionary<uint, byte> dictionary = new Dictionary<uint, byte>();
			dictionary.Add(spellId, slot);
			ItemEffects.Add(itemId, dictionary);
		}
	}

	public static byte GetItemEffectSlot(uint itemId, uint spellId)
	{
		if (ItemEffects.ContainsKey(itemId) && ItemEffects[itemId].ContainsKey(spellId))
		{
			return ItemEffects[itemId][spellId];
		}
		return 0;
	}

	public static uint GetItemEnchantVisual(uint enchantId)
	{
		if (ItemEnchantVisuals.TryGetValue(enchantId, out var value))
		{
			return value;
		}
		return 0u;
	}

	public static uint GetSpellVisual(uint spellId)
	{
		if (SpellVisuals.TryGetValue(spellId, out var value))
		{
			return value;
		}
		return 0u;
	}

	public static int GetTotemSlotForSpell(uint spellId)
	{
		if (TotemSpells.TryGetValue(spellId, out var value))
		{
			return (int)value;
		}
		return -1;
	}

	public static uint GetRealSpell(uint learnSpellId)
	{
		if (LearnSpells.TryGetValue(learnSpellId, out var value))
		{
			return value;
		}
		return learnSpellId;
	}

	public static uint GetGemFromEnchantId(uint enchantId)
	{
		if (Gems.TryGetValue(enchantId, out var value))
		{
			return value;
		}
		return 0u;
	}

	public static uint GetEnchantIdFromGem(uint itemId)
	{
		foreach (KeyValuePair<uint, uint> gem in Gems)
		{
			if (gem.Value == itemId)
			{
				return gem.Key;
			}
		}
		return 0u;
	}

	public static float GetUnitCompleteDisplayScale(uint displayId)
	{
		CreatureDisplayInfo displayInfo = GetDisplayInfo(displayId);
		if (displayInfo.ModelId == 0)
		{
			return 1f;
		}
		CreatureModelCollisionHeight modelData = GetModelData(displayId);
		return displayInfo.DisplayScale * modelData.ModelScale;
	}

	public static CreatureDisplayInfo GetDisplayInfo(uint displayId)
	{
		if (CreatureDisplayInfos.TryGetValue(displayId, out var value))
		{
			return value;
		}
		return new CreatureDisplayInfo(0u, 1f);
	}

	public static CreatureModelCollisionHeight GetModelData(uint modelId)
	{
		if (CreatureModelCollisionHeights.TryGetValue(modelId, out var value))
		{
			return value;
		}
		return new CreatureModelCollisionHeight(1f, 0f, 0f);
	}

	public static uint GetTransportPeriod(uint entry)
	{
		if (TransportPeriods.TryGetValue(entry, out var value))
		{
			return value;
		}
		return 0u;
	}

	public static string GetAreaName(uint id)
	{
		if (AreaNames.TryGetValue(id, out var value))
		{
			return value;
		}
		return "";
	}

	public static uint GetFactionForRace(uint race)
	{
		if (RaceFaction.TryGetValue(race, out var value))
		{
			return value;
		}
		return 1u;
	}

	public static uint GetBattlegroundIdFromMapId(uint mapId)
	{
		foreach (KeyValuePair<uint, Battleground> battleground in Battlegrounds)
		{
			if (battleground.Value.MapIds.Contains(mapId))
			{
				return battleground.Key;
			}
		}
		return 0u;
	}

	public static uint GetMapIdFromBattlegroundId(uint bgId)
	{
		if (Battlegrounds.TryGetValue(bgId, out var value))
		{
			return value.MapIds[0];
		}
		return 0u;
	}

	public static uint GetChatChannelIdFromName(string name)
	{
		foreach (KeyValuePair<uint, ChatChannel> chatChannel in ChatChannels)
		{
			if (name.Contains(chatChannel.Value.Name))
			{
				return chatChannel.Key;
			}
		}
		return 0u;
	}

	public static List<ChatChannel> GetChatChannelsWithFlags(ChannelFlags flags)
	{
		List<ChatChannel> list = new List<ChatChannel>();
		foreach (KeyValuePair<uint, ChatChannel> chatChannel in ChatChannels)
		{
			if ((chatChannel.Value.Flags & flags) == flags)
			{
				list.Add(chatChannel.Value);
			}
		}
		return list;
	}

	public static bool IsAllianceRace(Race raceId)
	{
		switch (raceId)
		{
		case Race.Human:
		case Race.Dwarf:
		case Race.NightElf:
		case Race.Gnome:
		case Race.Draenei:
		case Race.Worgen:
			return true;
		default:
			return false;
		}
	}

	public static bool IsHordeRace(Race raceId)
	{
		switch (raceId)
		{
		case Race.Orc:
		case Race.Undead:
		case Race.Tauren:
		case Race.Troll:
		case Race.Goblin:
		case Race.BloodElf:
			return true;
		default:
			return false;
		}
	}

	public static int GetFactionByRace(Race race)
	{
		if (IsAllianceRace(race))
		{
			return 1;
		}
		if (IsHordeRace(race))
		{
			return 2;
		}
		return 0;
	}

	public static BroadcastText GetBroadcastText(uint entry)
	{
		if (BroadcastTextStore.TryGetValue(entry, out var result))
		{
			return result;
		}
		return null;
	}

	public static uint GetBroadcastTextId(string maleText, string femaleText, uint language, ushort[] emoteDelays, ushort[] emotes)
	{
		foreach (KeyValuePair<uint, BroadcastText> item in BroadcastTextStore)
		{
			if (((!string.IsNullOrEmpty(maleText) && item.Value.MaleText == maleText) || (!string.IsNullOrEmpty(femaleText) && item.Value.FemaleText == femaleText)) && item.Value.Language == language && item.Value.EmoteDelays.SequenceEqual(emoteDelays) && item.Value.Emotes.SequenceEqual(emotes))
			{
				return item.Key;
			}
		}
		BroadcastText broadcastText = new BroadcastText();
		broadcastText.Entry = BroadcastTextStore.Keys.Last() + 1;
		broadcastText.MaleText = maleText;
		broadcastText.FemaleText = femaleText;
		broadcastText.Language = language;
		broadcastText.EmoteDelays = emoteDelays;
		broadcastText.Emotes = emotes;
		BroadcastTextStore.Add(broadcastText.Entry, broadcastText);
		return broadcastText.Entry;
	}

	public static void LoadEverything()
	{
		Log.Print(LogType.Storage, "正在加载数据文件...", "LoadEverything", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
		LoadBuildAuthSeeds();
		LoadBroadcastTexts();
		LoadItemDisplayIds();
		LoadItemRecords();
		LoadItemSparseRecords();
		LoadItemAppearance();
		LoadItemModifiedAppearance();
		LoadItemEffect();
		LoadItemSpellsData();
		LoadItemDisplayIdToFileDataId();
		LoadBattlegrounds();
		LoadChatChannels();
		LoadItemEnchantVisuals();
		LoadSpellVisuals();
		LoadLearnSpells();
		LoadTotemSpells();
		LoadGems();
		LoadCreatureDisplayInfo();
		LoadCreatureModelCollisionHeights();
		LoadTransports();
		LoadAreaNames();
		LoadRaceFaction();
		LoadDispellSpells();
		LoadSpellEffectPoints();
		LoadStackableAuras();
		LoadMountAuras();
		LoadMeleeSpells();
		LoadAutoRepeatSpells();
		LoadAuraSpells();
		LoadTaxiPaths();
		LoadTaxiPathNodesGraph();
		LoadQuestBits();
		LoadHotfixes();
		Log.Print(LogType.Storage, "数据加载完成.", "LoadEverything", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
	}

	public static void LoadBuildAuthSeeds()
	{
		using TextFieldParser textFieldParser = new TextFieldParser(Path.Combine("CSV", "BuildAuthSeeds.csv"));
		textFieldParser.CommentTokens = new string[1] { "#" };
		textFieldParser.SetDelimiters(",");
		textFieldParser.HasFieldsEnclosedInQuotes = true;
		textFieldParser.ReadLine();
		while (!textFieldParser.EndOfData)
		{
			string[] array = textFieldParser.ReadFields();
			uint key = uint.Parse(array[0]);
			string key2 = array[1];
			byte[] value = array[2].ParseAsByteArray();
			if (!BuildAuthSeeds.TryGetValue(key, out var value2))
			{
				value2 = new Dictionary<string, byte[]>();
				BuildAuthSeeds.Add(key, value2);
			}
			value2.Add(key2, value);
		}
	}

	public static void LoadBroadcastTexts()
	{
		using TextFieldParser textFieldParser = new TextFieldParser(Path.Combine("CSV", $"BroadcastTexts{LegacyVersion.ExpansionVersion}.csv"));
		textFieldParser.CommentTokens = new string[1] { "#" };
		textFieldParser.SetDelimiters(",");
		textFieldParser.HasFieldsEnclosedInQuotes = true;
		textFieldParser.ReadLine();
		while (!textFieldParser.EndOfData)
		{
			string[] array = textFieldParser.ReadFields();
			BroadcastText broadcastText = new BroadcastText();
			broadcastText.Entry = uint.Parse(array[0]);
			broadcastText.MaleText = array[1].TrimEnd().Replace("\0", "").Replace("~", "\n");
			broadcastText.FemaleText = array[2].TrimEnd().Replace("\0", "").Replace("~", "\n");
			broadcastText.Language = uint.Parse(array[3]);
			broadcastText.Emotes[0] = ushort.Parse(array[4]);
			broadcastText.Emotes[1] = ushort.Parse(array[5]);
			broadcastText.Emotes[2] = ushort.Parse(array[6]);
			broadcastText.EmoteDelays[0] = ushort.Parse(array[7]);
			broadcastText.EmoteDelays[1] = ushort.Parse(array[8]);
			broadcastText.EmoteDelays[2] = ushort.Parse(array[9]);
			BroadcastTextStore.Add(broadcastText.Entry, broadcastText);
		}
	}

	public static void LoadItemDisplayIds()
	{
		using TextFieldParser textFieldParser = new TextFieldParser(Path.Combine("CSV", $"ItemIdToDisplayId{ModernVersion.ExpansionVersion}.csv"));
		textFieldParser.CommentTokens = new string[1] { "#" };
		textFieldParser.SetDelimiters(",");
		textFieldParser.HasFieldsEnclosedInQuotes = false;
		textFieldParser.ReadLine();
		while (!textFieldParser.EndOfData)
		{
			string[] array = textFieldParser.ReadFields();
			uint key = uint.Parse(array[0]);
			uint value = uint.Parse(array[1]);
			ItemDisplayIdStore.Add(key, value);
		}
	}

	public static void LoadItemRecords()
	{
		using TextFieldParser textFieldParser = new TextFieldParser(Path.Combine("CSV", $"Item{ModernVersion.ExpansionVersion}.csv"));
		textFieldParser.CommentTokens = new string[1] { "#" };
		textFieldParser.SetDelimiters(",");
		textFieldParser.HasFieldsEnclosedInQuotes = false;
		textFieldParser.ReadLine();
		uint num = 0u;
		while (!textFieldParser.EndOfData)
		{
			num++;
			string[] array = textFieldParser.ReadFields();
			ItemRecord itemRecord = new ItemRecord();
			itemRecord.Id = int.Parse(array[0]);
			itemRecord.ClassId = byte.Parse(array[1]);
			itemRecord.SubclassId = byte.Parse(array[2]);
			itemRecord.Material = byte.Parse(array[3]);
			itemRecord.InventoryType = sbyte.Parse(array[4]);
			itemRecord.RequiredLevel = int.Parse(array[5]);
			itemRecord.SheatheType = byte.Parse(array[6]);
			itemRecord.RandomProperty = ushort.Parse(array[7]);
			itemRecord.ItemRandomSuffixGroupId = ushort.Parse(array[8]);
			itemRecord.SoundOverrideSubclassId = sbyte.Parse(array[9]);
			itemRecord.ScalingStatDistributionId = ushort.Parse(array[10]);
			itemRecord.IconFileDataId = int.Parse(array[11]);
			itemRecord.ItemGroupSoundsId = byte.Parse(array[12]);
			itemRecord.ContentTuningId = int.Parse(array[13]);
			itemRecord.MaxDurability = uint.Parse(array[14]);
			itemRecord.AmmoType = byte.Parse(array[15]);
			itemRecord.DamageType[0] = byte.Parse(array[16]);
			itemRecord.DamageType[1] = byte.Parse(array[17]);
			itemRecord.DamageType[2] = byte.Parse(array[18]);
			itemRecord.DamageType[3] = byte.Parse(array[19]);
			itemRecord.DamageType[4] = byte.Parse(array[20]);
			itemRecord.Resistances[0] = short.Parse(array[21]);
			itemRecord.Resistances[1] = short.Parse(array[22]);
			itemRecord.Resistances[2] = short.Parse(array[23]);
			itemRecord.Resistances[3] = short.Parse(array[24]);
			itemRecord.Resistances[4] = short.Parse(array[25]);
			itemRecord.Resistances[5] = short.Parse(array[26]);
			itemRecord.Resistances[6] = short.Parse(array[27]);
			itemRecord.MinDamage[0] = ushort.Parse(array[28]);
			itemRecord.MinDamage[1] = ushort.Parse(array[29]);
			itemRecord.MinDamage[2] = ushort.Parse(array[30]);
			itemRecord.MinDamage[3] = ushort.Parse(array[31]);
			itemRecord.MinDamage[4] = ushort.Parse(array[32]);
			itemRecord.MaxDamage[0] = ushort.Parse(array[33]);
			itemRecord.MaxDamage[1] = ushort.Parse(array[34]);
			itemRecord.MaxDamage[2] = ushort.Parse(array[35]);
			itemRecord.MaxDamage[3] = ushort.Parse(array[36]);
			itemRecord.MaxDamage[4] = ushort.Parse(array[37]);
			ItemRecordsStore.Add((uint)itemRecord.Id, itemRecord);
		}
	}

	public static void LoadItemSparseRecords()
	{
		using TextFieldParser textFieldParser = new TextFieldParser(Path.Combine("CSV", $"ItemSparse{ModernVersion.ExpansionVersion}.csv"));
		textFieldParser.CommentTokens = new string[1] { "#" };
		textFieldParser.SetDelimiters(",");
		textFieldParser.HasFieldsEnclosedInQuotes = true;
		textFieldParser.ReadLine();
		uint num = 0u;
		while (!textFieldParser.EndOfData)
		{
			num++;
			string[] array = textFieldParser.ReadFields();
			ItemSparseRecord itemSparseRecord = new ItemSparseRecord();
			itemSparseRecord.Id = int.Parse(array[0]);
			itemSparseRecord.AllowableRace = long.Parse(array[1]);
			itemSparseRecord.Description = array[2];
			itemSparseRecord.Name4 = array[3];
			itemSparseRecord.Name3 = array[4];
			itemSparseRecord.Name2 = array[5];
			itemSparseRecord.Name1 = array[6];
			itemSparseRecord.DmgVariance = float.Parse(array[7]);
			itemSparseRecord.DurationInInventory = uint.Parse(array[8]);
			itemSparseRecord.QualityModifier = float.Parse(array[9]);
			itemSparseRecord.BagFamily = uint.Parse(array[10]);
			itemSparseRecord.RangeMod = float.Parse(array[11]);
			itemSparseRecord.StatPercentageOfSocket[0] = float.Parse(array[12]);
			itemSparseRecord.StatPercentageOfSocket[1] = float.Parse(array[13]);
			itemSparseRecord.StatPercentageOfSocket[2] = float.Parse(array[14]);
			itemSparseRecord.StatPercentageOfSocket[3] = float.Parse(array[15]);
			itemSparseRecord.StatPercentageOfSocket[4] = float.Parse(array[16]);
			itemSparseRecord.StatPercentageOfSocket[5] = float.Parse(array[17]);
			itemSparseRecord.StatPercentageOfSocket[6] = float.Parse(array[18]);
			itemSparseRecord.StatPercentageOfSocket[7] = float.Parse(array[19]);
			itemSparseRecord.StatPercentageOfSocket[8] = float.Parse(array[20]);
			itemSparseRecord.StatPercentageOfSocket[9] = float.Parse(array[21]);
			itemSparseRecord.StatPercentEditor[0] = int.Parse(array[22]);
			itemSparseRecord.StatPercentEditor[1] = int.Parse(array[23]);
			itemSparseRecord.StatPercentEditor[2] = int.Parse(array[24]);
			itemSparseRecord.StatPercentEditor[3] = int.Parse(array[25]);
			itemSparseRecord.StatPercentEditor[4] = int.Parse(array[26]);
			itemSparseRecord.StatPercentEditor[5] = int.Parse(array[27]);
			itemSparseRecord.StatPercentEditor[6] = int.Parse(array[28]);
			itemSparseRecord.StatPercentEditor[7] = int.Parse(array[29]);
			itemSparseRecord.StatPercentEditor[8] = int.Parse(array[30]);
			itemSparseRecord.StatPercentEditor[9] = int.Parse(array[31]);
			itemSparseRecord.Stackable = int.Parse(array[32]);
			itemSparseRecord.MaxCount = int.Parse(array[33]);
			itemSparseRecord.RequiredAbility = uint.Parse(array[34]);
			itemSparseRecord.SellPrice = uint.Parse(array[35]);
			itemSparseRecord.BuyPrice = uint.Parse(array[36]);
			itemSparseRecord.VendorStackCount = uint.Parse(array[37]);
			itemSparseRecord.PriceVariance = float.Parse(array[38]);
			itemSparseRecord.PriceRandomValue = float.Parse(array[39]);
			itemSparseRecord.Flags[0] = uint.Parse(array[40]);
			itemSparseRecord.Flags[1] = uint.Parse(array[41]);
			itemSparseRecord.Flags[2] = uint.Parse(array[42]);
			itemSparseRecord.Flags[3] = uint.Parse(array[43]);
			itemSparseRecord.OppositeFactionItemId = int.Parse(array[44]);
			itemSparseRecord.MaxDurability = uint.Parse(array[45]);
			itemSparseRecord.ItemNameDescriptionId = ushort.Parse(array[46]);
			itemSparseRecord.RequiredTransmogHoliday = ushort.Parse(array[47]);
			itemSparseRecord.RequiredHoliday = ushort.Parse(array[48]);
			itemSparseRecord.LimitCategory = ushort.Parse(array[49]);
			itemSparseRecord.GemProperties = ushort.Parse(array[50]);
			itemSparseRecord.SocketMatchEnchantmentId = ushort.Parse(array[51]);
			itemSparseRecord.TotemCategoryId = ushort.Parse(array[52]);
			itemSparseRecord.InstanceBound = ushort.Parse(array[53]);
			itemSparseRecord.ZoneBound[0] = ushort.Parse(array[54]);
			itemSparseRecord.ZoneBound[1] = ushort.Parse(array[55]);
			itemSparseRecord.ItemSet = ushort.Parse(array[56]);
			itemSparseRecord.LockId = ushort.Parse(array[57]);
			itemSparseRecord.StartQuestId = ushort.Parse(array[58]);
			itemSparseRecord.PageText = ushort.Parse(array[59]);
			itemSparseRecord.Delay = ushort.Parse(array[60]);
			itemSparseRecord.RequiredReputationId = ushort.Parse(array[61]);
			itemSparseRecord.RequiredSkillRank = ushort.Parse(array[62]);
			itemSparseRecord.RequiredSkill = ushort.Parse(array[63]);
			itemSparseRecord.ItemLevel = ushort.Parse(array[64]);
			itemSparseRecord.AllowableClass = short.Parse(array[65]);
			itemSparseRecord.ItemRandomSuffixGroupId = ushort.Parse(array[66]);
			itemSparseRecord.RandomProperty = ushort.Parse(array[67]);
			itemSparseRecord.MinDamage[0] = ushort.Parse(array[68]);
			itemSparseRecord.MinDamage[1] = ushort.Parse(array[69]);
			itemSparseRecord.MinDamage[2] = ushort.Parse(array[70]);
			itemSparseRecord.MinDamage[3] = ushort.Parse(array[71]);
			itemSparseRecord.MinDamage[4] = ushort.Parse(array[72]);
			itemSparseRecord.MaxDamage[0] = ushort.Parse(array[73]);
			itemSparseRecord.MaxDamage[1] = ushort.Parse(array[74]);
			itemSparseRecord.MaxDamage[2] = ushort.Parse(array[75]);
			itemSparseRecord.MaxDamage[3] = ushort.Parse(array[76]);
			itemSparseRecord.MaxDamage[4] = ushort.Parse(array[77]);
			itemSparseRecord.Resistances[0] = short.Parse(array[78]);
			itemSparseRecord.Resistances[1] = short.Parse(array[79]);
			itemSparseRecord.Resistances[2] = short.Parse(array[80]);
			itemSparseRecord.Resistances[3] = short.Parse(array[81]);
			itemSparseRecord.Resistances[4] = short.Parse(array[82]);
			itemSparseRecord.Resistances[5] = short.Parse(array[83]);
			itemSparseRecord.Resistances[6] = short.Parse(array[84]);
			itemSparseRecord.ScalingStatDistributionId = ushort.Parse(array[85]);
			itemSparseRecord.ExpansionId = byte.Parse(array[86]);
			itemSparseRecord.ArtifactId = byte.Parse(array[87]);
			itemSparseRecord.SpellWeight = byte.Parse(array[88]);
			itemSparseRecord.SpellWeightCategory = byte.Parse(array[89]);
			itemSparseRecord.SocketType[0] = byte.Parse(array[90]);
			itemSparseRecord.SocketType[1] = byte.Parse(array[91]);
			itemSparseRecord.SocketType[2] = byte.Parse(array[92]);
			itemSparseRecord.SheatheType = byte.Parse(array[93]);
			itemSparseRecord.Material = byte.Parse(array[94]);
			itemSparseRecord.PageMaterial = byte.Parse(array[95]);
			itemSparseRecord.PageLanguage = byte.Parse(array[96]);
			itemSparseRecord.Bonding = byte.Parse(array[97]);
			itemSparseRecord.DamageType = byte.Parse(array[98]);
			itemSparseRecord.StatType[0] = sbyte.Parse(array[99]);
			itemSparseRecord.StatType[1] = sbyte.Parse(array[100]);
			itemSparseRecord.StatType[2] = sbyte.Parse(array[101]);
			itemSparseRecord.StatType[3] = sbyte.Parse(array[102]);
			itemSparseRecord.StatType[4] = sbyte.Parse(array[103]);
			itemSparseRecord.StatType[5] = sbyte.Parse(array[104]);
			itemSparseRecord.StatType[6] = sbyte.Parse(array[105]);
			itemSparseRecord.StatType[7] = sbyte.Parse(array[106]);
			itemSparseRecord.StatType[8] = sbyte.Parse(array[107]);
			itemSparseRecord.StatType[9] = sbyte.Parse(array[108]);
			itemSparseRecord.ContainerSlots = byte.Parse(array[109]);
			itemSparseRecord.RequiredReputationRank = byte.Parse(array[110]);
			itemSparseRecord.RequiredCityRank = byte.Parse(array[111]);
			itemSparseRecord.RequiredHonorRank = byte.Parse(array[112]);
			itemSparseRecord.InventoryType = byte.Parse(array[113]);
			itemSparseRecord.OverallQualityId = byte.Parse(array[114]);
			itemSparseRecord.AmmoType = byte.Parse(array[115]);
			itemSparseRecord.StatValue[0] = sbyte.Parse(array[116]);
			itemSparseRecord.StatValue[1] = sbyte.Parse(array[117]);
			itemSparseRecord.StatValue[2] = sbyte.Parse(array[118]);
			itemSparseRecord.StatValue[3] = sbyte.Parse(array[119]);
			itemSparseRecord.StatValue[4] = sbyte.Parse(array[120]);
			itemSparseRecord.StatValue[5] = sbyte.Parse(array[121]);
			itemSparseRecord.StatValue[6] = sbyte.Parse(array[122]);
			itemSparseRecord.StatValue[7] = sbyte.Parse(array[123]);
			itemSparseRecord.StatValue[8] = sbyte.Parse(array[124]);
			itemSparseRecord.StatValue[9] = sbyte.Parse(array[125]);
			itemSparseRecord.RequiredLevel = sbyte.Parse(array[126]);
			ItemSparseRecordsStore.Add((uint)itemSparseRecord.Id, itemSparseRecord);
		}
	}

	public static void LoadItemAppearance()
	{
		using TextFieldParser textFieldParser = new TextFieldParser(Path.Combine("CSV", $"ItemAppearance{ModernVersion.ExpansionVersion}.csv"));
		textFieldParser.CommentTokens = new string[1] { "#" };
		textFieldParser.SetDelimiters(",");
		textFieldParser.HasFieldsEnclosedInQuotes = false;
		textFieldParser.ReadLine();
		while (!textFieldParser.EndOfData)
		{
			string[] array = textFieldParser.ReadFields();
			ItemAppearance itemAppearance = new ItemAppearance();
			itemAppearance.Id = int.Parse(array[0]);
			itemAppearance.DisplayType = byte.Parse(array[1]);
			itemAppearance.ItemDisplayInfoID = int.Parse(array[2]);
			itemAppearance.DefaultIconFileDataID = int.Parse(array[3]);
			itemAppearance.UiOrder = int.Parse(array[4]);
			ItemAppearanceStore.Add((uint)itemAppearance.Id, itemAppearance);
		}
	}

	public static void LoadItemModifiedAppearance()
	{
		using TextFieldParser textFieldParser = new TextFieldParser(Path.Combine("CSV", $"ItemModifiedAppearance{ModernVersion.ExpansionVersion}.csv"));
		textFieldParser.CommentTokens = new string[1] { "#" };
		textFieldParser.SetDelimiters(",");
		textFieldParser.HasFieldsEnclosedInQuotes = false;
		textFieldParser.ReadLine();
		while (!textFieldParser.EndOfData)
		{
			string[] array = textFieldParser.ReadFields();
			ItemModifiedAppearance itemModifiedAppearance = new ItemModifiedAppearance();
			itemModifiedAppearance.Id = int.Parse(array[0]);
			itemModifiedAppearance.ItemID = int.Parse(array[1]);
			itemModifiedAppearance.ItemAppearanceModifierID = int.Parse(array[2]);
			itemModifiedAppearance.ItemAppearanceID = int.Parse(array[3]);
			itemModifiedAppearance.OrderIndex = int.Parse(array[4]);
			itemModifiedAppearance.TransmogSourceTypeEnum = int.Parse(array[5]);
			ItemModifiedAppearanceStore.Add((uint)itemModifiedAppearance.Id, itemModifiedAppearance);
		}
	}

	public static void LoadItemEffect()
	{
		using TextFieldParser textFieldParser = new TextFieldParser(Path.Combine("CSV", $"ItemEffect{ModernVersion.ExpansionVersion}.csv"));
		textFieldParser.CommentTokens = new string[1] { "#" };
		textFieldParser.SetDelimiters(",");
		textFieldParser.HasFieldsEnclosedInQuotes = false;
		textFieldParser.ReadLine();
		while (!textFieldParser.EndOfData)
		{
			string[] array = textFieldParser.ReadFields();
			ItemEffect itemEffect = new ItemEffect();
			itemEffect.Id = int.Parse(array[0]);
			itemEffect.LegacySlotIndex = byte.Parse(array[1]);
			itemEffect.TriggerType = sbyte.Parse(array[2]);
			itemEffect.Charges = short.Parse(array[3]);
			itemEffect.CoolDownMSec = int.Parse(array[4]);
			itemEffect.CategoryCoolDownMSec = int.Parse(array[5]);
			itemEffect.SpellCategoryID = ushort.Parse(array[6]);
			itemEffect.SpellID = int.Parse(array[7]);
			itemEffect.ChrSpecializationID = ushort.Parse(array[8]);
			itemEffect.ParentItemID = int.Parse(array[9]);
			ItemEffectStore.Add((uint)itemEffect.Id, itemEffect);
		}
	}

	public static void LoadItemSpellsData()
	{
		using TextFieldParser textFieldParser = new TextFieldParser(Path.Combine("CSV", $"ItemSpellsData{ModernVersion.ExpansionVersion}.csv"));
		textFieldParser.CommentTokens = new string[1] { "#" };
		textFieldParser.SetDelimiters(",");
		textFieldParser.HasFieldsEnclosedInQuotes = false;
		textFieldParser.ReadLine();
		while (!textFieldParser.EndOfData)
		{
			string[] array = textFieldParser.ReadFields();
			ItemSpellsData itemSpellsData = new ItemSpellsData();
			itemSpellsData.Id = int.Parse(array[0]);
			itemSpellsData.Category = int.Parse(array[1]);
			itemSpellsData.RecoveryTime = int.Parse(array[2]);
			itemSpellsData.CategoryRecoveryTime = int.Parse(array[3]);
			ItemSpellsDataStore.Add((uint)itemSpellsData.Id, itemSpellsData);
		}
	}

	public static void LoadItemDisplayIdToFileDataId()
	{
		using TextFieldParser textFieldParser = new TextFieldParser(Path.Combine("CSV", $"ItemDisplayIdToFileDataId{ModernVersion.ExpansionVersion}.csv"));
		textFieldParser.CommentTokens = new string[1] { "#" };
		textFieldParser.SetDelimiters(",");
		textFieldParser.HasFieldsEnclosedInQuotes = false;
		textFieldParser.ReadLine();
		while (!textFieldParser.EndOfData)
		{
			string[] array = textFieldParser.ReadFields();
			uint key = uint.Parse(array[0]);
			uint value = uint.Parse(array[1]);
			ItemDisplayIdToFileDataIdStore.Add(key, value);
		}
	}

	public static void LoadBattlegrounds()
	{
		using TextFieldParser textFieldParser = new TextFieldParser(Path.Combine("CSV", "Battlegrounds.csv"));
		textFieldParser.CommentTokens = new string[1] { "#" };
		textFieldParser.SetDelimiters(",");
		textFieldParser.HasFieldsEnclosedInQuotes = false;
		textFieldParser.ReadLine();
		while (!textFieldParser.EndOfData)
		{
			string[] array = textFieldParser.ReadFields();
			Battleground battleground = new Battleground();
			uint key = uint.Parse(array[0]);
			battleground.IsArena = byte.Parse(array[1]) != 0;
			for (int i = 0; i < 6; i++)
			{
				uint num = uint.Parse(array[2 + i]);
				if (num != 0)
				{
					battleground.MapIds.Add(num);
				}
			}
			Battlegrounds.Add(key, battleground);
		}
	}

	public static void LoadChatChannels()
	{
		using TextFieldParser textFieldParser = new TextFieldParser(Path.Combine("CSV", "ChatChannels.csv"));
		textFieldParser.CommentTokens = new string[1] { "#" };
		textFieldParser.SetDelimiters(",");
		textFieldParser.HasFieldsEnclosedInQuotes = true;
		textFieldParser.ReadLine();
		while (!textFieldParser.EndOfData)
		{
			string[] array = textFieldParser.ReadFields();
			ChatChannel chatChannel = new ChatChannel();
			chatChannel.Id = uint.Parse(array[0]);
			chatChannel.Flags = (ChannelFlags)uint.Parse(array[1]);
			chatChannel.Name = array[2];
			ChatChannels.Add(chatChannel.Id, chatChannel);
		}
	}

	public static void LoadItemEnchantVisuals()
	{
		using TextFieldParser textFieldParser = new TextFieldParser(Path.Combine("CSV", $"ItemEnchantVisuals{ModernVersion.ExpansionVersion}.csv"));
		textFieldParser.CommentTokens = new string[1] { "#" };
		textFieldParser.SetDelimiters(",");
		textFieldParser.HasFieldsEnclosedInQuotes = false;
		textFieldParser.ReadLine();
		while (!textFieldParser.EndOfData)
		{
			string[] array = textFieldParser.ReadFields();
			uint key = uint.Parse(array[0]);
			uint value = uint.Parse(array[1]);
			ItemEnchantVisuals.Add(key, value);
		}
	}

	public static void LoadSpellVisuals()
	{
		using TextFieldParser textFieldParser = new TextFieldParser(Path.Combine("CSV", $"SpellVisuals{ModernVersion.ExpansionVersion}.csv"));
		textFieldParser.CommentTokens = new string[1] { "#" };
		textFieldParser.SetDelimiters(",");
		textFieldParser.HasFieldsEnclosedInQuotes = false;
		textFieldParser.ReadLine();
		while (!textFieldParser.EndOfData)
		{
			string[] array = textFieldParser.ReadFields();
			uint key = uint.Parse(array[0]);
			uint value = uint.Parse(array[1]);
			SpellVisuals.Add(key, value);
		}
	}

	public static void LoadLearnSpells()
	{
		using TextFieldParser textFieldParser = new TextFieldParser(Path.Combine("CSV", "LearnSpells.csv"));
		textFieldParser.CommentTokens = new string[1] { "#" };
		textFieldParser.SetDelimiters(",");
		textFieldParser.HasFieldsEnclosedInQuotes = false;
		textFieldParser.ReadLine();
		while (!textFieldParser.EndOfData)
		{
			string[] array = textFieldParser.ReadFields();
			uint num = uint.Parse(array[0]);
			uint value = uint.Parse(array[1]);
			if (!LearnSpells.ContainsKey(num))
			{
				LearnSpells.Add(num, value);
			}
		}
	}

	public static void LoadTotemSpells()
	{
		if (LegacyVersion.ExpansionVersion > 1)
		{
			return;
		}
		using TextFieldParser textFieldParser = new TextFieldParser(Path.Combine("CSV", "TotemSpells.csv"));
		textFieldParser.CommentTokens = new string[1] { "#" };
		textFieldParser.SetDelimiters(",");
		textFieldParser.HasFieldsEnclosedInQuotes = false;
		textFieldParser.ReadLine();
		while (!textFieldParser.EndOfData)
		{
			string[] array = textFieldParser.ReadFields();
			uint key = uint.Parse(array[0]);
			uint value = uint.Parse(array[1]);
			TotemSpells.Add(key, value);
		}
	}

	public static void LoadGems()
	{
		if (ModernVersion.ExpansionVersion <= 1)
		{
			return;
		}
		using TextFieldParser textFieldParser = new TextFieldParser(Path.Combine("CSV", $"Gems{ModernVersion.ExpansionVersion}.csv"));
		textFieldParser.CommentTokens = new string[1] { "#" };
		textFieldParser.SetDelimiters(",");
		textFieldParser.HasFieldsEnclosedInQuotes = false;
		textFieldParser.ReadLine();
		while (!textFieldParser.EndOfData)
		{
			string[] array = textFieldParser.ReadFields();
			uint key = uint.Parse(array[0]);
			uint value = uint.Parse(array[1]);
			Gems.Add(key, value);
		}
	}

	public static void LoadCreatureDisplayInfo()
	{
		using TextFieldParser textFieldParser = new TextFieldParser(Path.Combine("CSV", "CreatureDisplayInfo.csv"));
		textFieldParser.CommentTokens = new string[1] { "#" };
		textFieldParser.SetDelimiters(",");
		textFieldParser.HasFieldsEnclosedInQuotes = false;
		textFieldParser.ReadLine();
		while (!textFieldParser.EndOfData)
		{
			string[] array = textFieldParser.ReadFields();
			uint key = uint.Parse(array[0]);
			uint modelId = uint.Parse(array[1]);
			float displayScale = float.Parse(array[2]);
			CreatureDisplayInfos.Add(key, new CreatureDisplayInfo(modelId, displayScale));
		}
	}

	public static void LoadCreatureModelCollisionHeights()
	{
		using TextFieldParser textFieldParser = new TextFieldParser(Path.Combine("CSV", $"CreatureModelCollisionHeightsModern{LegacyVersion.ExpansionVersion}.csv"));
		textFieldParser.CommentTokens = new string[1] { "#" };
		textFieldParser.SetDelimiters(",");
		textFieldParser.HasFieldsEnclosedInQuotes = false;
		textFieldParser.ReadLine();
		while (!textFieldParser.EndOfData)
		{
			string[] array = textFieldParser.ReadFields();
			uint key = uint.Parse(array[0]);
			float modelScale = float.Parse(array[1]);
			float height = float.Parse(array[2]);
			float mountHeight = float.Parse(array[3]);
			CreatureModelCollisionHeights.Add(key, new CreatureModelCollisionHeight(modelScale, height, mountHeight));
		}
	}

	public static void LoadTransports()
	{
		using TextFieldParser textFieldParser = new TextFieldParser(Path.Combine("CSV", $"Transports{LegacyVersion.ExpansionVersion}.csv"));
		textFieldParser.CommentTokens = new string[1] { "#" };
		textFieldParser.SetDelimiters(",");
		textFieldParser.HasFieldsEnclosedInQuotes = false;
		textFieldParser.ReadLine();
		while (!textFieldParser.EndOfData)
		{
			string[] array = textFieldParser.ReadFields();
			uint key = uint.Parse(array[0]);
			uint value = uint.Parse(array[1]);
			TransportPeriods.Add(key, value);
		}
	}

	public static void LoadAreaNames()
	{
		using TextFieldParser textFieldParser = new TextFieldParser(Path.Combine("CSV", "AreaNames.csv"));
		textFieldParser.CommentTokens = new string[1] { "#" };
		textFieldParser.SetDelimiters(",");
		textFieldParser.HasFieldsEnclosedInQuotes = true;
		textFieldParser.ReadLine();
		while (!textFieldParser.EndOfData)
		{
			string[] array = textFieldParser.ReadFields();
			uint key = uint.Parse(array[0]);
			string value = array[1];
			AreaNames.Add(key, value);
		}
	}

	public static void LoadRaceFaction()
	{
		using TextFieldParser textFieldParser = new TextFieldParser(Path.Combine("CSV", "RaceFaction.csv"));
		textFieldParser.CommentTokens = new string[1] { "#" };
		textFieldParser.SetDelimiters(",");
		textFieldParser.HasFieldsEnclosedInQuotes = false;
		textFieldParser.ReadLine();
		while (!textFieldParser.EndOfData)
		{
			string[] array = textFieldParser.ReadFields();
			uint key = uint.Parse(array[0]);
			uint value = uint.Parse(array[1]);
			RaceFaction.Add(key, value);
		}
	}

	public static void LoadDispellSpells()
	{
		if (LegacyVersion.ExpansionVersion > 1)
		{
			return;
		}
		using TextFieldParser textFieldParser = new TextFieldParser(Path.Combine("CSV", "DispellSpells.csv"));
		textFieldParser.CommentTokens = new string[1] { "#" };
		textFieldParser.SetDelimiters(",");
		textFieldParser.HasFieldsEnclosedInQuotes = false;
		textFieldParser.ReadLine();
		while (!textFieldParser.EndOfData)
		{
			uint num = uint.Parse(textFieldParser.ReadFields()[0]);
			DispellSpells.Add(num);
		}
	}

	public static void LoadSpellEffectPoints()
	{
		using TextFieldParser textFieldParser = new TextFieldParser(Path.Combine("CSV", $"SpellEffectPoints{LegacyVersion.ExpansionVersion}.csv"));
		textFieldParser.CommentTokens = new string[1] { "#" };
		textFieldParser.SetDelimiters(",");
		textFieldParser.HasFieldsEnclosedInQuotes = false;
		textFieldParser.ReadLine();
		while (!textFieldParser.EndOfData)
		{
			string[] array = textFieldParser.ReadFields();
			uint key = uint.Parse(array[0]);
			int num = int.Parse(array[2]);
			if (num != 0)
			{
				num++;
			}
			int num2 = int.Parse(array[3]);
			if (num2 != 0)
			{
				num2++;
			}
			int num3 = int.Parse(array[4]);
			if (num3 != 0)
			{
				num3++;
			}
			SpellEffectPoints.Add(key, new List<float> { num, num2, num3 });
		}
	}

	public static void LoadStackableAuras()
	{
		if (LegacyVersion.ExpansionVersion > 2)
		{
			return;
		}
		using TextFieldParser textFieldParser = new TextFieldParser(Path.Combine("CSV", $"StackableAuras{LegacyVersion.ExpansionVersion}.csv"));
		textFieldParser.CommentTokens = new string[1] { "#" };
		textFieldParser.SetDelimiters(",");
		textFieldParser.HasFieldsEnclosedInQuotes = false;
		textFieldParser.ReadLine();
		while (!textFieldParser.EndOfData)
		{
			uint num = uint.Parse(textFieldParser.ReadFields()[0]);
			StackableAuras.Add(num);
		}
	}

	public static void LoadMountAuras()
	{
		if (LegacyVersion.ExpansionVersion > 1)
		{
			return;
		}
		using TextFieldParser textFieldParser = new TextFieldParser(Path.Combine("CSV", "MountAuras.csv"));
		textFieldParser.CommentTokens = new string[1] { "#" };
		textFieldParser.SetDelimiters(",");
		textFieldParser.HasFieldsEnclosedInQuotes = false;
		textFieldParser.ReadLine();
		while (!textFieldParser.EndOfData)
		{
			uint num = uint.Parse(textFieldParser.ReadFields()[0]);
			MountAuras.Add(num);
		}
	}

	public static void LoadMeleeSpells()
	{
		using TextFieldParser textFieldParser = new TextFieldParser(Path.Combine("CSV", $"MeleeSpells{ModernVersion.ExpansionVersion}.csv"));
		textFieldParser.CommentTokens = new string[1] { "#" };
		textFieldParser.SetDelimiters(",");
		textFieldParser.HasFieldsEnclosedInQuotes = false;
		textFieldParser.ReadLine();
		while (!textFieldParser.EndOfData)
		{
			uint num = uint.Parse(textFieldParser.ReadFields()[0]);
			NextMeleeSpells.Add(num);
		}
	}

	public static void LoadAutoRepeatSpells()
	{
		using TextFieldParser textFieldParser = new TextFieldParser(Path.Combine("CSV", $"AutoRepeatSpells{ModernVersion.ExpansionVersion}.csv"));
		textFieldParser.CommentTokens = new string[1] { "#" };
		textFieldParser.SetDelimiters(",");
		textFieldParser.HasFieldsEnclosedInQuotes = false;
		textFieldParser.ReadLine();
		while (!textFieldParser.EndOfData)
		{
			uint num = uint.Parse(textFieldParser.ReadFields()[0]);
			AutoRepeatSpells.Add(num);
		}
	}

	public static void LoadAuraSpells()
	{
		using TextFieldParser textFieldParser = new TextFieldParser(Path.Combine("CSV", $"AuraSpells{LegacyVersion.ExpansionVersion}.csv"));
		textFieldParser.CommentTokens = new string[1] { "#" };
		textFieldParser.SetDelimiters(",");
		textFieldParser.HasFieldsEnclosedInQuotes = false;
		textFieldParser.ReadLine();
		while (!textFieldParser.EndOfData)
		{
			uint num = uint.Parse(textFieldParser.ReadFields()[0]);
			AuraSpells.Add(num);
		}
	}

	public static void LoadTaxiPaths()
	{
		using TextFieldParser textFieldParser = new TextFieldParser(Path.Combine("CSV", $"TaxiPath{ModernVersion.ExpansionVersion}.csv"));
		textFieldParser.CommentTokens = new string[1] { "#" };
		textFieldParser.SetDelimiters(",");
		textFieldParser.HasFieldsEnclosedInQuotes = true;
		textFieldParser.ReadLine();
		uint num = 0u;
		while (!textFieldParser.EndOfData)
		{
			string[] array = textFieldParser.ReadFields();
			TaxiPath taxiPath = new TaxiPath();
			taxiPath.Id = uint.Parse(array[0]);
			taxiPath.From = uint.Parse(array[1]);
			taxiPath.To = uint.Parse(array[2]);
			taxiPath.Cost = int.Parse(array[3]);
			TaxiPaths.Add(num, taxiPath);
			num++;
		}
	}

	public static void LoadTaxiPathNodesGraph()
	{
		Dictionary<uint, TaxiNode> dictionary = new Dictionary<uint, TaxiNode>();
		using (TextFieldParser textFieldParser = new TextFieldParser(Path.Combine("CSV", $"TaxiNodes{ModernVersion.ExpansionVersion}.csv")))
		{
			textFieldParser.CommentTokens = new string[1] { "#" };
			textFieldParser.SetDelimiters(",");
			textFieldParser.HasFieldsEnclosedInQuotes = false;
			textFieldParser.ReadLine();
			while (!textFieldParser.EndOfData)
			{
				string[] array = textFieldParser.ReadFields();
				TaxiNode taxiNode = new TaxiNode();
				taxiNode.Id = uint.Parse(array[0]);
				taxiNode.mapId = uint.Parse(array[1]);
				taxiNode.x = float.Parse(array[2]);
				taxiNode.y = float.Parse(array[3]);
				taxiNode.z = float.Parse(array[4]);
				dictionary.Add(taxiNode.Id, taxiNode);
			}
		}
		Dictionary<uint, TaxiPathNode> TaxiPathNodes = new Dictionary<uint, TaxiPathNode>();
		using (TextFieldParser textFieldParser2 = new TextFieldParser(Path.Combine("CSV", $"TaxiPathNode{ModernVersion.ExpansionVersion}.csv")))
		{
			textFieldParser2.CommentTokens = new string[1] { "#" };
			textFieldParser2.SetDelimiters(",");
			textFieldParser2.HasFieldsEnclosedInQuotes = true;
			textFieldParser2.ReadLine();
			while (!textFieldParser2.EndOfData)
			{
				string[] array2 = textFieldParser2.ReadFields();
				TaxiPathNode taxiPathNode = new TaxiPathNode();
				taxiPathNode.Id = uint.Parse(array2[0]);
				taxiPathNode.pathId = uint.Parse(array2[1]);
				taxiPathNode.nodeIndex = uint.Parse(array2[2]);
				taxiPathNode.mapId = uint.Parse(array2[3]);
				taxiPathNode.x = float.Parse(array2[4]);
				taxiPathNode.y = float.Parse(array2[5]);
				taxiPathNode.z = float.Parse(array2[6]);
				taxiPathNode.flags = uint.Parse(array2[7]);
				taxiPathNode.delay = uint.Parse(array2[8]);
				TaxiPathNodes.Add(taxiPathNode.Id, taxiPathNode);
			}
		}
		for (uint num = 0u; num < TaxiPaths.Count; num++)
		{
			if (!TaxiPaths.ContainsKey(num))
			{
				continue;
			}
			float num2 = 0f;
			TaxiPath taxiPath = TaxiPaths[num];
			TaxiNode taxiNode2 = dictionary[TaxiPaths[num].From];
			TaxiNode taxiNode3 = dictionary[TaxiPaths[num].To];
			if ((taxiNode2.x == 0f && taxiNode2.x == 0f && taxiNode2.z == 0f) || (taxiNode3.x == 0f && taxiNode3.x == 0f && taxiNode3.z == 0f))
			{
				continue;
			}
			HashSet<uint> hashSet = new HashSet<uint>();
			foreach (KeyValuePair<uint, TaxiPathNode> item in TaxiPathNodes)
			{
				TaxiPathNode value = item.Value;
				if (value.pathId == taxiPath.Id)
				{
					hashSet.Add(value.Id);
				}
			}
			IOrderedEnumerable<uint> orderedEnumerable = hashSet.OrderBy((uint node) => TaxiPathNodes[node].nodeIndex);
			uint num3 = 0u;
			foreach (uint item2 in orderedEnumerable)
			{
				TaxiPathNode taxiPathNode2 = TaxiPathNodes[item2];
				if (taxiPathNode2.nodeIndex == 0)
				{
					num2 += (float)Math.Sqrt(Math.Pow(taxiNode2.x - taxiPathNode2.x, 2.0) + Math.Pow(taxiNode2.y - taxiPathNode2.y, 2.0));
				}
				else if (num3 == 0)
				{
					num3 = taxiPathNode2.Id;
				}
				else if (num3 != 0)
				{
					TaxiPathNode taxiPathNode3 = TaxiPathNodes[num3];
					num3 = taxiPathNode2.Id;
					if (taxiPathNode3.mapId == taxiPathNode2.mapId)
					{
						num2 += (float)Math.Sqrt(Math.Pow(taxiPathNode3.x - taxiPathNode2.x, 2.0) + Math.Pow(taxiPathNode3.y - taxiPathNode2.y, 2.0));
					}
				}
			}
			if (num3 != 0)
			{
				TaxiPathNode taxiPathNode4 = TaxiPathNodes[num3];
				num2 += (float)Math.Sqrt(Math.Pow(taxiNode3.x - taxiPathNode4.x, 2.0) + Math.Pow(taxiNode3.y - taxiPathNode4.y, 2.0));
			}
			TaxiNodesGraph[TaxiPaths[num].From, TaxiPaths[num].To] = ((num2 > 0f) ? ((int)num2) : 0);
		}
	}

	public static void LoadQuestBits()
	{
		using TextFieldParser textFieldParser = new TextFieldParser(Path.Combine("CSV", $"QuestV2_{ModernVersion.ExpansionVersion}.csv"));
		textFieldParser.CommentTokens = new string[1] { "#" };
		textFieldParser.SetDelimiters(",");
		textFieldParser.HasFieldsEnclosedInQuotes = false;
		textFieldParser.ReadLine();
		while (!textFieldParser.EndOfData)
		{
			string[] array = textFieldParser.ReadFields();
			uint key = uint.Parse(array[0]);
			if (!array[1].StartsWith("-"))
			{
				uint value = uint.Parse(array[1]);
				QuestBits.Add(key, value);
			}
		}
	}

	public static void LoadHotfixes()
	{
		LoadAreaTriggerHotfixes();
		LoadSkillLineHotfixes();
		LoadSkillRaceClassInfoHotfixes();
		LoadSkillLineAbilityHotfixes();
		LoadSpellHotfixes();
		LoadSpellNameHotfixes();
		LoadSpellLevelsHotfixes();
		LoadSpellAuraOptionsHotfixes();
		LoadSpellMiscHotfixes();
		LoadSpellEffectHotfixes();
		LoadSpellXSpellVisualHotfixes();
		LoadItemSparseHotfixes();
		LoadItemHotfixes();
		LoadItemEffectHotfixes();
		LoadItemDisplayInfoHotfixes();
		LoadCreatureDisplayInfoHotfixes();
		LoadCreatureDisplayInfoExtraHotfixes();
		LoadCreatureDisplayInfoOptionHotfixes();
	}

	public static void LoadAreaTriggerHotfixes()
	{
		using TextFieldParser textFieldParser = new TextFieldParser(Path.Combine("CSV", "Hotfix", $"AreaTrigger{ModernVersion.ExpansionVersion}.csv"));
		textFieldParser.CommentTokens = new string[1] { "#" };
		textFieldParser.SetDelimiters(",");
		textFieldParser.HasFieldsEnclosedInQuotes = true;
		textFieldParser.ReadLine();
		uint num = 0u;
		while (!textFieldParser.EndOfData)
		{
			num++;
			string[] array = textFieldParser.ReadFields();
			AreaTrigger areaTrigger = new AreaTrigger();
			areaTrigger.Message = array[0];
			areaTrigger.PositionX = float.Parse(array[1]);
			areaTrigger.PositionY = float.Parse(array[2]);
			areaTrigger.PositionZ = float.Parse(array[3]);
			areaTrigger.Id = uint.Parse(array[4]);
			areaTrigger.MapId = ushort.Parse(array[5]);
			areaTrigger.PhaseUseFlags = byte.Parse(array[6]);
			areaTrigger.PhaseId = ushort.Parse(array[7]);
			areaTrigger.PhaseGroupId = ushort.Parse(array[8]);
			areaTrigger.Radius = float.Parse(array[9]);
			areaTrigger.BoxLength = float.Parse(array[10]);
			areaTrigger.BoxWidth = float.Parse(array[11]);
			areaTrigger.BoxHeight = float.Parse(array[12]);
			areaTrigger.BoxYaw = float.Parse(array[13]);
			areaTrigger.ShapeType = byte.Parse(array[14]);
			areaTrigger.ShapeId = ushort.Parse(array[15]);
			areaTrigger.ActionSetId = ushort.Parse(array[16]);
			areaTrigger.Flags = byte.Parse(array[17]);
			HotfixRecord hotfixRecord = new HotfixRecord();
			hotfixRecord.TableHash = DB2Hash.AreaTrigger;
			hotfixRecord.HotfixId = 100000 + num;
			hotfixRecord.UniqueId = hotfixRecord.HotfixId;
			hotfixRecord.RecordId = areaTrigger.Id;
			hotfixRecord.Status = HotfixStatus.Valid;
			hotfixRecord.HotfixContent.WriteCString(areaTrigger.Message);
			hotfixRecord.HotfixContent.WriteFloat(areaTrigger.PositionX);
			hotfixRecord.HotfixContent.WriteFloat(areaTrigger.PositionY);
			hotfixRecord.HotfixContent.WriteFloat(areaTrigger.PositionZ);
			hotfixRecord.HotfixContent.WriteUInt32(areaTrigger.Id);
			hotfixRecord.HotfixContent.WriteUInt16(areaTrigger.MapId);
			hotfixRecord.HotfixContent.WriteUInt8(areaTrigger.PhaseUseFlags);
			hotfixRecord.HotfixContent.WriteUInt16(areaTrigger.PhaseId);
			hotfixRecord.HotfixContent.WriteUInt16(areaTrigger.PhaseGroupId);
			hotfixRecord.HotfixContent.WriteFloat(areaTrigger.Radius);
			hotfixRecord.HotfixContent.WriteFloat(areaTrigger.BoxLength);
			hotfixRecord.HotfixContent.WriteFloat(areaTrigger.BoxWidth);
			hotfixRecord.HotfixContent.WriteFloat(areaTrigger.BoxHeight);
			hotfixRecord.HotfixContent.WriteFloat(areaTrigger.BoxYaw);
			hotfixRecord.HotfixContent.WriteUInt8(areaTrigger.ShapeType);
			hotfixRecord.HotfixContent.WriteUInt16(areaTrigger.ShapeId);
			hotfixRecord.HotfixContent.WriteUInt16(areaTrigger.ActionSetId);
			hotfixRecord.HotfixContent.WriteUInt8(areaTrigger.Flags);
			Hotfixes.Add(hotfixRecord.HotfixId, hotfixRecord);
		}
	}

	public static void LoadSkillLineHotfixes()
	{
		using TextFieldParser textFieldParser = new TextFieldParser(Path.Combine("CSV", "Hotfix", $"SkillLine{ModernVersion.ExpansionVersion}.csv"));
		textFieldParser.CommentTokens = new string[1] { "#" };
		textFieldParser.SetDelimiters(",");
		textFieldParser.HasFieldsEnclosedInQuotes = true;
		textFieldParser.ReadLine();
		uint num = 0u;
		while (!textFieldParser.EndOfData)
		{
			num++;
			string[] array = textFieldParser.ReadFields();
			string str = array[0];
			string str2 = array[1];
			string str3 = array[2];
			string str4 = array[3];
			string str5 = array[4];
			uint num2 = uint.Parse(array[5]);
			byte data = byte.Parse(array[6]);
			uint data2 = uint.Parse(array[7]);
			byte data3 = byte.Parse(array[8]);
			uint data4 = uint.Parse(array[9]);
			uint data5 = uint.Parse(array[10]);
			ushort data6 = ushort.Parse(array[11]);
			uint data7 = uint.Parse(array[12]);
			HotfixRecord hotfixRecord = new HotfixRecord();
			hotfixRecord.TableHash = DB2Hash.SkillLine;
			hotfixRecord.HotfixId = 110000 + num;
			hotfixRecord.UniqueId = hotfixRecord.HotfixId;
			hotfixRecord.RecordId = num2;
			hotfixRecord.Status = HotfixStatus.Valid;
			hotfixRecord.HotfixContent.WriteCString(str);
			hotfixRecord.HotfixContent.WriteCString(str2);
			hotfixRecord.HotfixContent.WriteCString(str3);
			hotfixRecord.HotfixContent.WriteCString(str4);
			hotfixRecord.HotfixContent.WriteCString(str5);
			hotfixRecord.HotfixContent.WriteUInt32(num2);
			hotfixRecord.HotfixContent.WriteUInt8(data);
			hotfixRecord.HotfixContent.WriteUInt32(data2);
			hotfixRecord.HotfixContent.WriteUInt8(data3);
			hotfixRecord.HotfixContent.WriteUInt32(data4);
			hotfixRecord.HotfixContent.WriteUInt32(data5);
			hotfixRecord.HotfixContent.WriteUInt16(data6);
			hotfixRecord.HotfixContent.WriteUInt32(data7);
			Hotfixes.Add(hotfixRecord.HotfixId, hotfixRecord);
		}
	}

	public static void LoadSkillRaceClassInfoHotfixes()
	{
		using TextFieldParser textFieldParser = new TextFieldParser(Path.Combine("CSV", "Hotfix", $"SkillRaceClassInfo{ModernVersion.ExpansionVersion}.csv"));
		textFieldParser.CommentTokens = new string[1] { "#" };
		textFieldParser.SetDelimiters(",");
		textFieldParser.HasFieldsEnclosedInQuotes = false;
		textFieldParser.ReadLine();
		uint num = 0u;
		while (!textFieldParser.EndOfData)
		{
			num++;
			string[] array = textFieldParser.ReadFields();
			uint recordId = uint.Parse(array[0]);
			ulong data = ulong.Parse(array[1]);
			ushort data2 = ushort.Parse(array[2]);
			uint data3 = uint.Parse(array[3]);
			ushort data4 = ushort.Parse(array[4]);
			byte data5 = byte.Parse(array[5]);
			byte data6 = byte.Parse(array[6]);
			ushort data7 = ushort.Parse(array[7]);
			HotfixRecord hotfixRecord = new HotfixRecord();
			hotfixRecord.TableHash = DB2Hash.SkillRaceClassInfo;
			hotfixRecord.HotfixId = 120000 + num;
			hotfixRecord.UniqueId = hotfixRecord.HotfixId;
			hotfixRecord.RecordId = recordId;
			hotfixRecord.Status = HotfixStatus.Valid;
			hotfixRecord.HotfixContent.WriteUInt64(data);
			hotfixRecord.HotfixContent.WriteUInt16(data2);
			hotfixRecord.HotfixContent.WriteUInt32(data3);
			hotfixRecord.HotfixContent.WriteUInt16(data4);
			hotfixRecord.HotfixContent.WriteUInt8(data5);
			hotfixRecord.HotfixContent.WriteUInt8(data6);
			hotfixRecord.HotfixContent.WriteUInt16(data7);
			Hotfixes.Add(hotfixRecord.HotfixId, hotfixRecord);
		}
	}

	public static void LoadSkillLineAbilityHotfixes()
	{
		using TextFieldParser textFieldParser = new TextFieldParser(Path.Combine("CSV", "Hotfix", $"SkillLineAbility{ModernVersion.ExpansionVersion}.csv"));
		textFieldParser.CommentTokens = new string[1] { "#" };
		textFieldParser.SetDelimiters(",");
		textFieldParser.HasFieldsEnclosedInQuotes = false;
		textFieldParser.ReadLine();
		uint num = 0u;
		while (!textFieldParser.EndOfData)
		{
			num++;
			string[] array = textFieldParser.ReadFields();
			ulong data = ulong.Parse(array[0]);
			uint num2 = uint.Parse(array[1]);
			ushort data2 = ushort.Parse(array[2]);
			uint data3 = uint.Parse(array[3]);
			ushort data4 = ushort.Parse(array[4]);
			uint data5 = uint.Parse(array[5]);
			uint data6 = uint.Parse(array[6]);
			byte data7 = byte.Parse(array[7]);
			ushort data8 = ushort.Parse(array[8]);
			ushort data9 = ushort.Parse(array[9]);
			byte data10 = byte.Parse(array[10]);
			byte data11 = byte.Parse(array[11]);
			ushort data12 = ushort.Parse(array[12]);
			ushort data13 = ushort.Parse(array[13]);
			ushort data14 = ushort.Parse(array[14]);
			uint data15 = uint.Parse(array[15]);
			uint data16 = uint.Parse(array[16]);
			HotfixRecord hotfixRecord = new HotfixRecord();
			hotfixRecord.TableHash = DB2Hash.SkillLineAbility;
			hotfixRecord.HotfixId = 130000 + num;
			hotfixRecord.UniqueId = hotfixRecord.HotfixId;
			hotfixRecord.RecordId = num2;
			hotfixRecord.Status = HotfixStatus.Valid;
			hotfixRecord.HotfixContent.WriteUInt64(data);
			hotfixRecord.HotfixContent.WriteUInt32(num2);
			hotfixRecord.HotfixContent.WriteUInt16(data2);
			hotfixRecord.HotfixContent.WriteUInt32(data3);
			hotfixRecord.HotfixContent.WriteUInt16(data4);
			hotfixRecord.HotfixContent.WriteUInt32(data5);
			hotfixRecord.HotfixContent.WriteUInt32(data6);
			hotfixRecord.HotfixContent.WriteUInt8(data7);
			hotfixRecord.HotfixContent.WriteUInt16(data8);
			hotfixRecord.HotfixContent.WriteUInt16(data9);
			hotfixRecord.HotfixContent.WriteUInt8(data10);
			hotfixRecord.HotfixContent.WriteUInt8(data11);
			hotfixRecord.HotfixContent.WriteUInt16(data12);
			hotfixRecord.HotfixContent.WriteUInt16(data13);
			hotfixRecord.HotfixContent.WriteUInt16(data14);
			hotfixRecord.HotfixContent.WriteUInt32(data15);
			hotfixRecord.HotfixContent.WriteUInt32(data16);
			Hotfixes.Add(hotfixRecord.HotfixId, hotfixRecord);
		}
	}

	public static void LoadSpellHotfixes()
	{
		using TextFieldParser textFieldParser = new TextFieldParser(Path.Combine("CSV", "Hotfix", $"Spell{ModernVersion.ExpansionVersion}.csv"));
		textFieldParser.CommentTokens = new string[1] { "#" };
		textFieldParser.SetDelimiters(",");
		textFieldParser.HasFieldsEnclosedInQuotes = true;
		textFieldParser.ReadLine();
		uint num = 0u;
		while (!textFieldParser.EndOfData)
		{
			num++;
			string[] array = textFieldParser.ReadFields();
			uint recordId = uint.Parse(array[0]);
			string str = array[1];
			string str2 = array[2];
			string str3 = array[3];
			HotfixRecord hotfixRecord = new HotfixRecord();
			hotfixRecord.TableHash = DB2Hash.Spell;
			hotfixRecord.HotfixId = 140000 + num;
			hotfixRecord.UniqueId = hotfixRecord.HotfixId;
			hotfixRecord.RecordId = recordId;
			hotfixRecord.Status = HotfixStatus.Valid;
			hotfixRecord.HotfixContent.WriteCString(str);
			hotfixRecord.HotfixContent.WriteCString(str2);
			hotfixRecord.HotfixContent.WriteCString(str3);
			Hotfixes.Add(hotfixRecord.HotfixId, hotfixRecord);
		}
	}

	public static void LoadSpellNameHotfixes()
	{
		using TextFieldParser textFieldParser = new TextFieldParser(Path.Combine("CSV", "Hotfix", $"SpellName{ModernVersion.ExpansionVersion}.csv"));
		textFieldParser.CommentTokens = new string[1] { "#" };
		textFieldParser.SetDelimiters(",");
		textFieldParser.HasFieldsEnclosedInQuotes = true;
		textFieldParser.ReadLine();
		uint num = 0u;
		while (!textFieldParser.EndOfData)
		{
			num++;
			string[] array = textFieldParser.ReadFields();
			uint recordId = uint.Parse(array[0]);
			string str = array[1];
			HotfixRecord hotfixRecord = new HotfixRecord();
			hotfixRecord.TableHash = DB2Hash.SpellName;
			hotfixRecord.HotfixId = 150000 + num;
			hotfixRecord.UniqueId = hotfixRecord.HotfixId;
			hotfixRecord.RecordId = recordId;
			hotfixRecord.Status = HotfixStatus.Valid;
			hotfixRecord.HotfixContent.WriteCString(str);
			Hotfixes.Add(hotfixRecord.HotfixId, hotfixRecord);
		}
	}

	public static void LoadSpellLevelsHotfixes()
	{
		using TextFieldParser textFieldParser = new TextFieldParser(Path.Combine("CSV", "Hotfix", $"SpellLevels{ModernVersion.ExpansionVersion}.csv"));
		textFieldParser.CommentTokens = new string[1] { "#" };
		textFieldParser.SetDelimiters(",");
		textFieldParser.HasFieldsEnclosedInQuotes = false;
		textFieldParser.ReadLine();
		uint num = 0u;
		while (!textFieldParser.EndOfData)
		{
			num++;
			string[] array = textFieldParser.ReadFields();
			uint recordId = uint.Parse(array[0]);
			byte data = byte.Parse(array[1]);
			ushort data2 = ushort.Parse(array[2]);
			ushort data3 = ushort.Parse(array[3]);
			ushort data4 = ushort.Parse(array[4]);
			byte data5 = byte.Parse(array[5]);
			uint data6 = uint.Parse(array[6]);
			HotfixRecord hotfixRecord = new HotfixRecord();
			hotfixRecord.TableHash = DB2Hash.SpellLevels;
			hotfixRecord.HotfixId = 160000 + num;
			hotfixRecord.UniqueId = hotfixRecord.HotfixId;
			hotfixRecord.RecordId = recordId;
			hotfixRecord.Status = HotfixStatus.Valid;
			hotfixRecord.HotfixContent.WriteUInt8(data);
			hotfixRecord.HotfixContent.WriteUInt16(data2);
			hotfixRecord.HotfixContent.WriteUInt16(data3);
			hotfixRecord.HotfixContent.WriteUInt16(data4);
			hotfixRecord.HotfixContent.WriteUInt8(data5);
			hotfixRecord.HotfixContent.WriteUInt32(data6);
			Hotfixes.Add(hotfixRecord.HotfixId, hotfixRecord);
		}
	}

	public static void LoadSpellAuraOptionsHotfixes()
	{
		using TextFieldParser textFieldParser = new TextFieldParser(Path.Combine("CSV", "Hotfix", $"SpellAuraOptions{ModernVersion.ExpansionVersion}.csv"));
		textFieldParser.CommentTokens = new string[1] { "#" };
		textFieldParser.SetDelimiters(",");
		textFieldParser.HasFieldsEnclosedInQuotes = false;
		textFieldParser.ReadLine();
		uint num = 0u;
		while (!textFieldParser.EndOfData)
		{
			num++;
			string[] array = textFieldParser.ReadFields();
			uint recordId = uint.Parse(array[0]);
			byte data = byte.Parse(array[1]);
			uint data2 = uint.Parse(array[2]);
			uint data3 = uint.Parse(array[3]);
			byte data4 = byte.Parse(array[4]);
			uint data5 = uint.Parse(array[5]);
			ushort data6 = ushort.Parse(array[6]);
			uint data7 = uint.Parse(array[7]);
			uint data8 = uint.Parse(array[8]);
			uint data9 = uint.Parse(array[9]);
			HotfixRecord hotfixRecord = new HotfixRecord();
			hotfixRecord.TableHash = DB2Hash.SpellAuraOptions;
			hotfixRecord.HotfixId = 170000 + num;
			hotfixRecord.UniqueId = hotfixRecord.HotfixId;
			hotfixRecord.RecordId = recordId;
			hotfixRecord.Status = HotfixStatus.Valid;
			hotfixRecord.HotfixContent.WriteUInt8(data);
			hotfixRecord.HotfixContent.WriteUInt32(data2);
			hotfixRecord.HotfixContent.WriteUInt32(data3);
			hotfixRecord.HotfixContent.WriteUInt8(data4);
			hotfixRecord.HotfixContent.WriteUInt32(data5);
			hotfixRecord.HotfixContent.WriteUInt16(data6);
			hotfixRecord.HotfixContent.WriteUInt32(data7);
			hotfixRecord.HotfixContent.WriteUInt32(data8);
			hotfixRecord.HotfixContent.WriteUInt32(data9);
			Hotfixes.Add(hotfixRecord.HotfixId, hotfixRecord);
		}
	}

	public static void LoadSpellMiscHotfixes()
	{
		using TextFieldParser textFieldParser = new TextFieldParser(Path.Combine("CSV", "Hotfix", $"SpellMisc{ModernVersion.ExpansionVersion}.csv"));
		textFieldParser.CommentTokens = new string[1] { "#" };
		textFieldParser.SetDelimiters(",");
		textFieldParser.HasFieldsEnclosedInQuotes = false;
		textFieldParser.ReadLine();
		uint num = 0u;
		while (!textFieldParser.EndOfData)
		{
			num++;
			string[] array = textFieldParser.ReadFields();
			uint recordId = uint.Parse(array[0]);
			byte data = byte.Parse(array[1]);
			ushort data2 = ushort.Parse(array[2]);
			ushort data3 = ushort.Parse(array[3]);
			ushort data4 = ushort.Parse(array[4]);
			byte data5 = byte.Parse(array[5]);
			float data6 = float.Parse(array[6]);
			float data7 = float.Parse(array[7]);
			float data8 = float.Parse(array[8]);
			uint data9 = uint.Parse(array[9]);
			uint data10 = uint.Parse(array[10]);
			uint data11 = uint.Parse(array[11]);
			uint data12 = uint.Parse(array[12]);
			uint data13 = uint.Parse(array[13]);
			uint data14 = uint.Parse(array[14]);
			uint data15 = uint.Parse(array[15]);
			uint data16 = uint.Parse(array[16]);
			uint data17 = uint.Parse(array[17]);
			uint data18 = uint.Parse(array[18]);
			uint data19 = uint.Parse(array[19]);
			uint data20 = uint.Parse(array[20]);
			uint data21 = uint.Parse(array[21]);
			uint data22 = uint.Parse(array[22]);
			uint data23 = uint.Parse(array[23]);
			uint data24 = uint.Parse(array[24]);
			uint data25 = uint.Parse(array[25]);
			HotfixRecord hotfixRecord = new HotfixRecord();
			hotfixRecord.TableHash = DB2Hash.SpellMisc;
			hotfixRecord.HotfixId = 180000 + num;
			hotfixRecord.UniqueId = hotfixRecord.HotfixId;
			hotfixRecord.RecordId = recordId;
			hotfixRecord.Status = HotfixStatus.Valid;
			hotfixRecord.HotfixContent.WriteUInt8(data);
			hotfixRecord.HotfixContent.WriteUInt16(data2);
			hotfixRecord.HotfixContent.WriteUInt16(data3);
			hotfixRecord.HotfixContent.WriteUInt16(data4);
			hotfixRecord.HotfixContent.WriteUInt8(data5);
			hotfixRecord.HotfixContent.WriteFloat(data6);
			hotfixRecord.HotfixContent.WriteFloat(data7);
			hotfixRecord.HotfixContent.WriteFloat(data8);
			hotfixRecord.HotfixContent.WriteUInt32(data9);
			hotfixRecord.HotfixContent.WriteUInt32(data10);
			hotfixRecord.HotfixContent.WriteUInt32(data11);
			hotfixRecord.HotfixContent.WriteUInt32(data12);
			hotfixRecord.HotfixContent.WriteUInt32(data13);
			hotfixRecord.HotfixContent.WriteUInt32(data14);
			hotfixRecord.HotfixContent.WriteUInt32(data15);
			hotfixRecord.HotfixContent.WriteUInt32(data16);
			hotfixRecord.HotfixContent.WriteUInt32(data17);
			hotfixRecord.HotfixContent.WriteUInt32(data18);
			hotfixRecord.HotfixContent.WriteUInt32(data19);
			hotfixRecord.HotfixContent.WriteUInt32(data20);
			hotfixRecord.HotfixContent.WriteUInt32(data21);
			hotfixRecord.HotfixContent.WriteUInt32(data22);
			hotfixRecord.HotfixContent.WriteUInt32(data23);
			hotfixRecord.HotfixContent.WriteUInt32(data24);
			hotfixRecord.HotfixContent.WriteUInt32(data25);
			Hotfixes.Add(hotfixRecord.HotfixId, hotfixRecord);
		}
	}

	public static void LoadSpellEffectHotfixes()
	{
		using TextFieldParser textFieldParser = new TextFieldParser(Path.Combine("CSV", "Hotfix", $"SpellEffect{ModernVersion.ExpansionVersion}.csv"));
		textFieldParser.CommentTokens = new string[1] { "#" };
		textFieldParser.SetDelimiters(",");
		textFieldParser.HasFieldsEnclosedInQuotes = false;
		textFieldParser.ReadLine();
		uint num = 0u;
		while (!textFieldParser.EndOfData)
		{
			num++;
			string[] array = textFieldParser.ReadFields();
			uint recordId = uint.Parse(array[0]);
			uint data = uint.Parse(array[1]);
			uint data2 = uint.Parse(array[2]);
			uint data3 = uint.Parse(array[3]);
			float data4 = float.Parse(array[4]);
			uint data5 = uint.Parse(array[5]);
			short data6 = short.Parse(array[6]);
			int data7 = int.Parse(array[7]);
			int data8 = int.Parse(array[8]);
			float data9 = float.Parse(array[9]);
			float data10 = float.Parse(array[10]);
			int data11 = int.Parse(array[11]);
			int data12 = int.Parse(array[12]);
			int data13 = int.Parse(array[13]);
			int data14 = int.Parse(array[14]);
			float data15 = float.Parse(array[15]);
			float data16 = float.Parse(array[16]);
			float data17 = float.Parse(array[17]);
			int data18 = int.Parse(array[18]);
			float data19 = float.Parse(array[19]);
			float data20 = float.Parse(array[20]);
			float data21 = float.Parse(array[21]);
			float data22 = float.Parse(array[22]);
			float data23 = float.Parse(array[23]);
			float data24 = float.Parse(array[24]);
			int data25 = int.Parse(array[25]);
			int data26 = int.Parse(array[26]);
			uint data27 = uint.Parse(array[27]);
			uint data28 = uint.Parse(array[28]);
			int data29 = int.Parse(array[29]);
			int data30 = int.Parse(array[30]);
			int data31 = int.Parse(array[31]);
			int data32 = int.Parse(array[32]);
			short data33 = short.Parse(array[33]);
			short data34 = short.Parse(array[34]);
			uint data35 = uint.Parse(array[35]);
			HotfixRecord hotfixRecord = new HotfixRecord();
			hotfixRecord.TableHash = DB2Hash.SpellEffect;
			hotfixRecord.HotfixId = 190000 + num;
			hotfixRecord.UniqueId = hotfixRecord.HotfixId;
			hotfixRecord.RecordId = recordId;
			hotfixRecord.Status = HotfixStatus.Valid;
			hotfixRecord.HotfixContent.WriteUInt32(data);
			hotfixRecord.HotfixContent.WriteUInt32(data2);
			hotfixRecord.HotfixContent.WriteUInt32(data3);
			hotfixRecord.HotfixContent.WriteFloat(data4);
			hotfixRecord.HotfixContent.WriteUInt32(data5);
			hotfixRecord.HotfixContent.WriteInt16(data6);
			hotfixRecord.HotfixContent.WriteInt32(data7);
			hotfixRecord.HotfixContent.WriteInt32(data8);
			hotfixRecord.HotfixContent.WriteFloat(data9);
			hotfixRecord.HotfixContent.WriteFloat(data10);
			hotfixRecord.HotfixContent.WriteInt32(data11);
			hotfixRecord.HotfixContent.WriteInt32(data12);
			hotfixRecord.HotfixContent.WriteInt32(data13);
			hotfixRecord.HotfixContent.WriteInt32(data14);
			hotfixRecord.HotfixContent.WriteFloat(data15);
			hotfixRecord.HotfixContent.WriteFloat(data16);
			hotfixRecord.HotfixContent.WriteFloat(data17);
			hotfixRecord.HotfixContent.WriteInt32(data18);
			hotfixRecord.HotfixContent.WriteFloat(data19);
			hotfixRecord.HotfixContent.WriteFloat(data20);
			hotfixRecord.HotfixContent.WriteFloat(data21);
			hotfixRecord.HotfixContent.WriteFloat(data22);
			hotfixRecord.HotfixContent.WriteFloat(data23);
			hotfixRecord.HotfixContent.WriteFloat(data24);
			hotfixRecord.HotfixContent.WriteInt32(data25);
			hotfixRecord.HotfixContent.WriteInt32(data26);
			hotfixRecord.HotfixContent.WriteUInt32(data27);
			hotfixRecord.HotfixContent.WriteUInt32(data28);
			hotfixRecord.HotfixContent.WriteInt32(data29);
			hotfixRecord.HotfixContent.WriteInt32(data30);
			hotfixRecord.HotfixContent.WriteInt32(data31);
			hotfixRecord.HotfixContent.WriteInt32(data32);
			hotfixRecord.HotfixContent.WriteInt16(data33);
			hotfixRecord.HotfixContent.WriteInt16(data34);
			hotfixRecord.HotfixContent.WriteUInt32(data35);
			Hotfixes.Add(hotfixRecord.HotfixId, hotfixRecord);
		}
	}

	public static void LoadSpellXSpellVisualHotfixes()
	{
		using TextFieldParser textFieldParser = new TextFieldParser(Path.Combine("CSV", "Hotfix", $"SpellXSpellVisual{ModernVersion.ExpansionVersion}.csv"));
		textFieldParser.CommentTokens = new string[1] { "#" };
		textFieldParser.SetDelimiters(",");
		textFieldParser.HasFieldsEnclosedInQuotes = false;
		textFieldParser.ReadLine();
		uint num = 0u;
		while (!textFieldParser.EndOfData)
		{
			num++;
			string[] array = textFieldParser.ReadFields();
			uint num2 = uint.Parse(array[0]);
			byte data = byte.Parse(array[1]);
			uint data2 = uint.Parse(array[2]);
			float data3 = float.Parse(array[3]);
			byte data4 = byte.Parse(array[4]);
			byte data5 = byte.Parse(array[5]);
			int data6 = int.Parse(array[6]);
			int data7 = int.Parse(array[7]);
			ushort data8 = ushort.Parse(array[8]);
			uint data9 = uint.Parse(array[9]);
			ushort data10 = ushort.Parse(array[10]);
			uint data11 = uint.Parse(array[11]);
			uint num3 = uint.Parse(array[12]);
			if (SpellVisuals.ContainsKey(num3))
			{
				SpellVisuals[num3] = num2;
			}
			else
			{
				SpellVisuals.Add(num3, num2);
			}
			HotfixRecord hotfixRecord = new HotfixRecord();
			hotfixRecord.TableHash = DB2Hash.SpellXSpellVisual;
			hotfixRecord.HotfixId = 200000 + num;
			hotfixRecord.UniqueId = hotfixRecord.HotfixId;
			hotfixRecord.RecordId = num2;
			hotfixRecord.Status = HotfixStatus.Valid;
			hotfixRecord.HotfixContent.WriteUInt32(num2);
			hotfixRecord.HotfixContent.WriteUInt8(data);
			hotfixRecord.HotfixContent.WriteUInt32(data2);
			hotfixRecord.HotfixContent.WriteFloat(data3);
			hotfixRecord.HotfixContent.WriteUInt8(data4);
			hotfixRecord.HotfixContent.WriteUInt8(data5);
			hotfixRecord.HotfixContent.WriteInt32(data6);
			hotfixRecord.HotfixContent.WriteInt32(data7);
			hotfixRecord.HotfixContent.WriteUInt16(data8);
			hotfixRecord.HotfixContent.WriteUInt32(data9);
			hotfixRecord.HotfixContent.WriteUInt16(data10);
			hotfixRecord.HotfixContent.WriteUInt32(data11);
			hotfixRecord.HotfixContent.WriteUInt32(num3);
			Hotfixes.Add(hotfixRecord.HotfixId, hotfixRecord);
		}
	}

	public static void LoadItemSparseHotfixes()
	{
		using TextFieldParser textFieldParser = new TextFieldParser(Path.Combine("CSV", "Hotfix", $"ItemSparse{ModernVersion.ExpansionVersion}.csv"));
		textFieldParser.CommentTokens = new string[1] { "#" };
		textFieldParser.SetDelimiters(",");
		textFieldParser.HasFieldsEnclosedInQuotes = true;
		textFieldParser.ReadLine();
		uint num = 0u;
		while (!textFieldParser.EndOfData)
		{
			num++;
			string[] array = textFieldParser.ReadFields();
			uint recordId = uint.Parse(array[0]);
			long data = long.Parse(array[1]);
			string str = array[2];
			string str2 = array[3];
			string str3 = array[4];
			string str4 = array[5];
			string str5 = array[6];
			float data2 = float.Parse(array[7]);
			uint data3 = uint.Parse(array[8]);
			float data4 = float.Parse(array[9]);
			uint data5 = uint.Parse(array[10]);
			float data6 = float.Parse(array[11]);
			float data7 = float.Parse(array[12]);
			float data8 = float.Parse(array[13]);
			float data9 = float.Parse(array[14]);
			float data10 = float.Parse(array[15]);
			float data11 = float.Parse(array[16]);
			float data12 = float.Parse(array[17]);
			float data13 = float.Parse(array[18]);
			float data14 = float.Parse(array[19]);
			float data15 = float.Parse(array[20]);
			float data16 = float.Parse(array[21]);
			int data17 = int.Parse(array[22]);
			int data18 = int.Parse(array[23]);
			int data19 = int.Parse(array[24]);
			int data20 = int.Parse(array[25]);
			int data21 = int.Parse(array[26]);
			int data22 = int.Parse(array[27]);
			int data23 = int.Parse(array[28]);
			int data24 = int.Parse(array[29]);
			int data25 = int.Parse(array[30]);
			int data26 = int.Parse(array[31]);
			int data27 = int.Parse(array[32]);
			int data28 = int.Parse(array[33]);
			uint data29 = uint.Parse(array[34]);
			uint data30 = uint.Parse(array[35]);
			uint data31 = uint.Parse(array[36]);
			uint data32 = uint.Parse(array[37]);
			float data33 = float.Parse(array[38]);
			float data34 = float.Parse(array[39]);
			int data35 = int.Parse(array[40]);
			int data36 = int.Parse(array[41]);
			int data37 = int.Parse(array[42]);
			int data38 = int.Parse(array[43]);
			int data39 = int.Parse(array[44]);
			uint data40 = uint.Parse(array[45]);
			ushort data41 = ushort.Parse(array[46]);
			ushort data42 = ushort.Parse(array[47]);
			ushort data43 = ushort.Parse(array[48]);
			ushort data44 = ushort.Parse(array[49]);
			ushort data45 = ushort.Parse(array[50]);
			ushort data46 = ushort.Parse(array[51]);
			ushort data47 = ushort.Parse(array[52]);
			ushort data48 = ushort.Parse(array[53]);
			ushort data49 = ushort.Parse(array[54]);
			ushort data50 = ushort.Parse(array[55]);
			ushort data51 = ushort.Parse(array[56]);
			ushort data52 = ushort.Parse(array[57]);
			ushort data53 = ushort.Parse(array[58]);
			ushort data54 = ushort.Parse(array[59]);
			ushort data55 = ushort.Parse(array[60]);
			ushort data56 = ushort.Parse(array[61]);
			ushort data57 = ushort.Parse(array[62]);
			ushort data58 = ushort.Parse(array[63]);
			ushort data59 = ushort.Parse(array[64]);
			short data60 = short.Parse(array[65]);
			ushort data61 = ushort.Parse(array[66]);
			ushort data62 = ushort.Parse(array[67]);
			ushort data63 = ushort.Parse(array[68]);
			ushort data64 = ushort.Parse(array[69]);
			ushort data65 = ushort.Parse(array[70]);
			ushort data66 = ushort.Parse(array[71]);
			ushort data67 = ushort.Parse(array[72]);
			ushort data68 = ushort.Parse(array[73]);
			ushort data69 = ushort.Parse(array[74]);
			ushort data70 = ushort.Parse(array[75]);
			ushort data71 = ushort.Parse(array[76]);
			ushort data72 = ushort.Parse(array[77]);
			short data73 = short.Parse(array[78]);
			short data74 = short.Parse(array[79]);
			short data75 = short.Parse(array[80]);
			short data76 = short.Parse(array[81]);
			short data77 = short.Parse(array[82]);
			short data78 = short.Parse(array[83]);
			short data79 = short.Parse(array[84]);
			ushort data80 = ushort.Parse(array[85]);
			byte data81 = byte.Parse(array[86]);
			byte data82 = byte.Parse(array[87]);
			byte data83 = byte.Parse(array[88]);
			byte data84 = byte.Parse(array[89]);
			byte data85 = byte.Parse(array[90]);
			byte data86 = byte.Parse(array[91]);
			byte data87 = byte.Parse(array[92]);
			byte data88 = byte.Parse(array[93]);
			byte data89 = byte.Parse(array[94]);
			byte data90 = byte.Parse(array[95]);
			byte data91 = byte.Parse(array[96]);
			byte data92 = byte.Parse(array[97]);
			byte data93 = byte.Parse(array[98]);
			sbyte data94 = sbyte.Parse(array[99]);
			sbyte data95 = sbyte.Parse(array[100]);
			sbyte data96 = sbyte.Parse(array[101]);
			sbyte data97 = sbyte.Parse(array[102]);
			sbyte data98 = sbyte.Parse(array[103]);
			sbyte data99 = sbyte.Parse(array[104]);
			sbyte data100 = sbyte.Parse(array[105]);
			sbyte data101 = sbyte.Parse(array[106]);
			sbyte data102 = sbyte.Parse(array[107]);
			sbyte data103 = sbyte.Parse(array[108]);
			byte data104 = byte.Parse(array[109]);
			byte data105 = byte.Parse(array[110]);
			byte data106 = byte.Parse(array[111]);
			byte data107 = byte.Parse(array[112]);
			byte data108 = byte.Parse(array[113]);
			byte data109 = byte.Parse(array[114]);
			byte data110 = byte.Parse(array[115]);
			sbyte data111 = sbyte.Parse(array[116]);
			sbyte data112 = sbyte.Parse(array[117]);
			sbyte data113 = sbyte.Parse(array[118]);
			sbyte data114 = sbyte.Parse(array[119]);
			sbyte data115 = sbyte.Parse(array[120]);
			sbyte data116 = sbyte.Parse(array[121]);
			sbyte data117 = sbyte.Parse(array[122]);
			sbyte data118 = sbyte.Parse(array[123]);
			sbyte data119 = sbyte.Parse(array[124]);
			sbyte data120 = sbyte.Parse(array[125]);
			sbyte data121 = sbyte.Parse(array[126]);
			HotfixRecord hotfixRecord = new HotfixRecord();
			hotfixRecord.Status = HotfixStatus.Valid;
			hotfixRecord.TableHash = DB2Hash.ItemSparse;
			hotfixRecord.HotfixId = 220000 + num;
			hotfixRecord.UniqueId = hotfixRecord.HotfixId;
			hotfixRecord.RecordId = recordId;
			hotfixRecord.HotfixContent.WriteInt64(data);
			hotfixRecord.HotfixContent.WriteCString(str);
			hotfixRecord.HotfixContent.WriteCString(str2);
			hotfixRecord.HotfixContent.WriteCString(str3);
			hotfixRecord.HotfixContent.WriteCString(str4);
			hotfixRecord.HotfixContent.WriteCString(str5);
			hotfixRecord.HotfixContent.WriteFloat(data2);
			hotfixRecord.HotfixContent.WriteUInt32(data3);
			hotfixRecord.HotfixContent.WriteFloat(data4);
			hotfixRecord.HotfixContent.WriteUInt32(data5);
			hotfixRecord.HotfixContent.WriteFloat(data6);
			hotfixRecord.HotfixContent.WriteFloat(data7);
			hotfixRecord.HotfixContent.WriteFloat(data8);
			hotfixRecord.HotfixContent.WriteFloat(data9);
			hotfixRecord.HotfixContent.WriteFloat(data10);
			hotfixRecord.HotfixContent.WriteFloat(data11);
			hotfixRecord.HotfixContent.WriteFloat(data12);
			hotfixRecord.HotfixContent.WriteFloat(data13);
			hotfixRecord.HotfixContent.WriteFloat(data14);
			hotfixRecord.HotfixContent.WriteFloat(data15);
			hotfixRecord.HotfixContent.WriteFloat(data16);
			hotfixRecord.HotfixContent.WriteInt32(data17);
			hotfixRecord.HotfixContent.WriteInt32(data18);
			hotfixRecord.HotfixContent.WriteInt32(data19);
			hotfixRecord.HotfixContent.WriteInt32(data20);
			hotfixRecord.HotfixContent.WriteInt32(data21);
			hotfixRecord.HotfixContent.WriteInt32(data22);
			hotfixRecord.HotfixContent.WriteInt32(data23);
			hotfixRecord.HotfixContent.WriteInt32(data24);
			hotfixRecord.HotfixContent.WriteInt32(data25);
			hotfixRecord.HotfixContent.WriteInt32(data26);
			hotfixRecord.HotfixContent.WriteInt32(data27);
			hotfixRecord.HotfixContent.WriteInt32(data28);
			hotfixRecord.HotfixContent.WriteUInt32(data29);
			hotfixRecord.HotfixContent.WriteUInt32(data30);
			hotfixRecord.HotfixContent.WriteUInt32(data31);
			hotfixRecord.HotfixContent.WriteUInt32(data32);
			hotfixRecord.HotfixContent.WriteFloat(data33);
			hotfixRecord.HotfixContent.WriteFloat(data34);
			hotfixRecord.HotfixContent.WriteInt32(data35);
			hotfixRecord.HotfixContent.WriteInt32(data36);
			hotfixRecord.HotfixContent.WriteInt32(data37);
			hotfixRecord.HotfixContent.WriteInt32(data38);
			hotfixRecord.HotfixContent.WriteInt32(data39);
			hotfixRecord.HotfixContent.WriteUInt32(data40);
			hotfixRecord.HotfixContent.WriteUInt16(data41);
			hotfixRecord.HotfixContent.WriteUInt16(data42);
			hotfixRecord.HotfixContent.WriteUInt16(data43);
			hotfixRecord.HotfixContent.WriteUInt16(data44);
			hotfixRecord.HotfixContent.WriteUInt16(data45);
			hotfixRecord.HotfixContent.WriteUInt16(data46);
			hotfixRecord.HotfixContent.WriteUInt16(data47);
			hotfixRecord.HotfixContent.WriteUInt16(data48);
			hotfixRecord.HotfixContent.WriteUInt16(data49);
			hotfixRecord.HotfixContent.WriteUInt16(data50);
			hotfixRecord.HotfixContent.WriteUInt16(data51);
			hotfixRecord.HotfixContent.WriteUInt16(data52);
			hotfixRecord.HotfixContent.WriteUInt16(data53);
			hotfixRecord.HotfixContent.WriteUInt16(data54);
			hotfixRecord.HotfixContent.WriteUInt16(data55);
			hotfixRecord.HotfixContent.WriteUInt16(data56);
			hotfixRecord.HotfixContent.WriteUInt16(data57);
			hotfixRecord.HotfixContent.WriteUInt16(data58);
			hotfixRecord.HotfixContent.WriteUInt16(data59);
			hotfixRecord.HotfixContent.WriteInt16(data60);
			hotfixRecord.HotfixContent.WriteUInt16(data61);
			hotfixRecord.HotfixContent.WriteUInt16(data62);
			hotfixRecord.HotfixContent.WriteUInt16(data63);
			hotfixRecord.HotfixContent.WriteUInt16(data64);
			hotfixRecord.HotfixContent.WriteUInt16(data65);
			hotfixRecord.HotfixContent.WriteUInt16(data66);
			hotfixRecord.HotfixContent.WriteUInt16(data67);
			hotfixRecord.HotfixContent.WriteUInt16(data68);
			hotfixRecord.HotfixContent.WriteUInt16(data69);
			hotfixRecord.HotfixContent.WriteUInt16(data70);
			hotfixRecord.HotfixContent.WriteUInt16(data71);
			hotfixRecord.HotfixContent.WriteUInt16(data72);
			hotfixRecord.HotfixContent.WriteInt16(data73);
			hotfixRecord.HotfixContent.WriteInt16(data74);
			hotfixRecord.HotfixContent.WriteInt16(data75);
			hotfixRecord.HotfixContent.WriteInt16(data76);
			hotfixRecord.HotfixContent.WriteInt16(data77);
			hotfixRecord.HotfixContent.WriteInt16(data78);
			hotfixRecord.HotfixContent.WriteInt16(data79);
			hotfixRecord.HotfixContent.WriteUInt16(data80);
			hotfixRecord.HotfixContent.WriteUInt8(data81);
			hotfixRecord.HotfixContent.WriteUInt8(data82);
			hotfixRecord.HotfixContent.WriteUInt8(data83);
			hotfixRecord.HotfixContent.WriteUInt8(data84);
			hotfixRecord.HotfixContent.WriteUInt8(data85);
			hotfixRecord.HotfixContent.WriteUInt8(data86);
			hotfixRecord.HotfixContent.WriteUInt8(data87);
			hotfixRecord.HotfixContent.WriteUInt8(data88);
			hotfixRecord.HotfixContent.WriteUInt8(data89);
			hotfixRecord.HotfixContent.WriteUInt8(data90);
			hotfixRecord.HotfixContent.WriteUInt8(data91);
			hotfixRecord.HotfixContent.WriteUInt8(data92);
			hotfixRecord.HotfixContent.WriteUInt8(data93);
			hotfixRecord.HotfixContent.WriteInt8(data94);
			hotfixRecord.HotfixContent.WriteInt8(data95);
			hotfixRecord.HotfixContent.WriteInt8(data96);
			hotfixRecord.HotfixContent.WriteInt8(data97);
			hotfixRecord.HotfixContent.WriteInt8(data98);
			hotfixRecord.HotfixContent.WriteInt8(data99);
			hotfixRecord.HotfixContent.WriteInt8(data100);
			hotfixRecord.HotfixContent.WriteInt8(data101);
			hotfixRecord.HotfixContent.WriteInt8(data102);
			hotfixRecord.HotfixContent.WriteInt8(data103);
			hotfixRecord.HotfixContent.WriteUInt8(data104);
			hotfixRecord.HotfixContent.WriteUInt8(data105);
			hotfixRecord.HotfixContent.WriteUInt8(data106);
			hotfixRecord.HotfixContent.WriteUInt8(data107);
			hotfixRecord.HotfixContent.WriteUInt8(data108);
			hotfixRecord.HotfixContent.WriteUInt8(data109);
			hotfixRecord.HotfixContent.WriteUInt8(data110);
			hotfixRecord.HotfixContent.WriteInt8(data111);
			hotfixRecord.HotfixContent.WriteInt8(data112);
			hotfixRecord.HotfixContent.WriteInt8(data113);
			hotfixRecord.HotfixContent.WriteInt8(data114);
			hotfixRecord.HotfixContent.WriteInt8(data115);
			hotfixRecord.HotfixContent.WriteInt8(data116);
			hotfixRecord.HotfixContent.WriteInt8(data117);
			hotfixRecord.HotfixContent.WriteInt8(data118);
			hotfixRecord.HotfixContent.WriteInt8(data119);
			hotfixRecord.HotfixContent.WriteInt8(data120);
			hotfixRecord.HotfixContent.WriteInt8(data121);
			Hotfixes.Add(hotfixRecord.HotfixId, hotfixRecord);
		}
	}

	public static void WriteItemSparseHotfix(ItemTemplate item, ByteBuffer buffer)
	{
		int[] array = new int[10];
		for (int i = 0; i < item.StatsCount; i++)
		{
			array[i] = item.StatValues[i];
			if (array[i] > 127)
			{
				array[i] = 127;
			}
			if (array[i] < -127)
			{
				array[i] = -127;
			}
		}
		buffer.WriteInt64(item.AllowedRaces);
		buffer.WriteCString(item.Description);
		buffer.WriteCString(item.Name[3]);
		buffer.WriteCString(item.Name[2]);
		buffer.WriteCString(item.Name[1]);
		buffer.WriteCString(item.Name[0]);
		buffer.WriteFloat(1f);
		buffer.WriteUInt32(item.Duration);
		buffer.WriteFloat(0f);
		buffer.WriteUInt32(item.BagFamily);
		buffer.WriteFloat(item.RangedMod);
		buffer.WriteFloat(0f);
		buffer.WriteFloat(0f);
		buffer.WriteFloat(0f);
		buffer.WriteFloat(0f);
		buffer.WriteFloat(0f);
		buffer.WriteFloat(0f);
		buffer.WriteFloat(0f);
		buffer.WriteFloat(0f);
		buffer.WriteFloat(0f);
		buffer.WriteFloat(0f);
		buffer.WriteInt32(0);
		buffer.WriteInt32(0);
		buffer.WriteInt32(0);
		buffer.WriteInt32(0);
		buffer.WriteInt32(0);
		buffer.WriteInt32(0);
		buffer.WriteInt32(0);
		buffer.WriteInt32(0);
		buffer.WriteInt32(0);
		buffer.WriteInt32(0);
		buffer.WriteInt32(item.MaxStackSize);
		buffer.WriteInt32(item.MaxCount);
		buffer.WriteUInt32(item.RequiredSpell);
		buffer.WriteUInt32(item.SellPrice);
		buffer.WriteUInt32(item.BuyPrice);
		buffer.WriteUInt32(item.BuyCount);
		buffer.WriteFloat(1f);
		buffer.WriteFloat(1f);
		buffer.WriteUInt32(item.Flags);
		buffer.WriteUInt32(item.FlagsExtra);
		buffer.WriteInt32(0);
		buffer.WriteInt32(0);
		buffer.WriteInt32(0);
		buffer.WriteUInt32(item.MaxDurability);
		buffer.WriteUInt16(0);
		buffer.WriteUInt16(0);
		buffer.WriteUInt16((ushort)item.HolidayID);
		buffer.WriteUInt16((ushort)item.ItemLimitCategory);
		buffer.WriteUInt16((ushort)item.GemProperties);
		buffer.WriteUInt16((ushort)item.SocketBonus);
		buffer.WriteUInt16((ushort)item.TotemCategory);
		buffer.WriteUInt16((ushort)item.MapID);
		buffer.WriteUInt16((ushort)item.AreaID);
		buffer.WriteUInt16(0);
		buffer.WriteUInt16((ushort)item.ItemSet);
		buffer.WriteUInt16((ushort)item.LockId);
		buffer.WriteUInt16((ushort)item.StartQuestId);
		buffer.WriteUInt16((ushort)item.PageText);
		buffer.WriteUInt16((ushort)item.Delay);
		buffer.WriteUInt16((ushort)item.RequiredRepFaction);
		buffer.WriteUInt16((ushort)item.RequiredSkillLevel);
		buffer.WriteUInt16((ushort)item.RequiredSkillId);
		buffer.WriteUInt16((ushort)item.ItemLevel);
		buffer.WriteInt16((short)item.AllowedClasses);
		buffer.WriteUInt16((ushort)item.RandomSuffix);
		buffer.WriteUInt16((ushort)item.RandomProperty);
		buffer.WriteUInt16((ushort)item.DamageMins[0]);
		buffer.WriteUInt16((ushort)item.DamageMins[1]);
		buffer.WriteUInt16((ushort)item.DamageMins[2]);
		buffer.WriteUInt16((ushort)item.DamageMins[3]);
		buffer.WriteUInt16((ushort)item.DamageMins[4]);
		buffer.WriteUInt16((ushort)item.DamageMaxs[0]);
		buffer.WriteUInt16((ushort)item.DamageMaxs[1]);
		buffer.WriteUInt16((ushort)item.DamageMaxs[2]);
		buffer.WriteUInt16((ushort)item.DamageMaxs[3]);
		buffer.WriteUInt16((ushort)item.DamageMaxs[4]);
		buffer.WriteInt16((short)item.Armor);
		buffer.WriteInt16((short)item.HolyResistance);
		buffer.WriteInt16((short)item.FireResistance);
		buffer.WriteInt16((short)item.NatureResistance);
		buffer.WriteInt16((short)item.FrostResistance);
		buffer.WriteInt16((short)item.ShadowResistance);
		buffer.WriteInt16((short)item.ArcaneResistance);
		buffer.WriteUInt16((ushort)item.ScalingStatDistribution);
		buffer.WriteUInt8(254);
		buffer.WriteUInt8(0);
		buffer.WriteUInt8(0);
		buffer.WriteUInt8(0);
		buffer.WriteUInt8((byte)item.ItemSocketColors[0]);
		buffer.WriteUInt8((byte)item.ItemSocketColors[1]);
		buffer.WriteUInt8((byte)item.ItemSocketColors[2]);
		buffer.WriteUInt8((byte)item.SheathType);
		buffer.WriteUInt8((byte)item.Material);
		buffer.WriteUInt8((byte)item.PageMaterial);
		buffer.WriteUInt8((byte)item.Language);
		buffer.WriteUInt8((byte)item.Bonding);
		buffer.WriteUInt8((byte)item.DamageTypes[0]);
		buffer.WriteInt8((sbyte)item.StatTypes[0]);
		buffer.WriteInt8((sbyte)item.StatTypes[1]);
		buffer.WriteInt8((sbyte)item.StatTypes[2]);
		buffer.WriteInt8((sbyte)item.StatTypes[3]);
		buffer.WriteInt8((sbyte)item.StatTypes[4]);
		buffer.WriteInt8((sbyte)item.StatTypes[5]);
		buffer.WriteInt8((sbyte)item.StatTypes[6]);
		buffer.WriteInt8((sbyte)item.StatTypes[7]);
		buffer.WriteInt8((sbyte)item.StatTypes[8]);
		buffer.WriteInt8((sbyte)item.StatTypes[9]);
		buffer.WriteUInt8((byte)item.ContainerSlots);
		buffer.WriteUInt8((byte)item.RequiredRepValue);
		buffer.WriteUInt8((byte)item.RequiredCityRank);
		buffer.WriteUInt8((byte)item.RequiredHonorRank);
		buffer.WriteUInt8((byte)item.InventoryType);
		buffer.WriteUInt8((byte)item.Quality);
		buffer.WriteUInt8((byte)item.AmmoType);
		buffer.WriteInt8((sbyte)array[0]);
		buffer.WriteInt8((sbyte)array[1]);
		buffer.WriteInt8((sbyte)array[2]);
		buffer.WriteInt8((sbyte)array[3]);
		buffer.WriteInt8((sbyte)array[4]);
		buffer.WriteInt8((sbyte)array[5]);
		buffer.WriteInt8((sbyte)array[6]);
		buffer.WriteInt8((sbyte)array[7]);
		buffer.WriteInt8((sbyte)array[8]);
		buffer.WriteInt8((sbyte)array[9]);
		buffer.WriteInt8((sbyte)item.RequiredLevel);
	}

	public static void WriteItemSparseHotfix(ItemSparseRecord row, ByteBuffer buffer)
	{
		int[] array = new int[10];
		for (int i = 0; i < 10; i++)
		{
			array[i] = row.StatValue[i];
			if (array[i] > 127)
			{
				array[i] = 127;
			}
			if (array[i] < -127)
			{
				array[i] = -127;
			}
		}
		buffer.WriteInt64(row.AllowableRace);
		buffer.WriteCString(row.Description);
		buffer.WriteCString(row.Name4);
		buffer.WriteCString(row.Name3);
		buffer.WriteCString(row.Name2);
		buffer.WriteCString(row.Name1);
		buffer.WriteFloat(row.DmgVariance);
		buffer.WriteUInt32(row.DurationInInventory);
		buffer.WriteFloat(row.QualityModifier);
		buffer.WriteUInt32(row.BagFamily);
		buffer.WriteFloat(row.RangeMod);
		buffer.WriteFloat(row.StatPercentageOfSocket[0]);
		buffer.WriteFloat(row.StatPercentageOfSocket[1]);
		buffer.WriteFloat(row.StatPercentageOfSocket[2]);
		buffer.WriteFloat(row.StatPercentageOfSocket[3]);
		buffer.WriteFloat(row.StatPercentageOfSocket[4]);
		buffer.WriteFloat(row.StatPercentageOfSocket[5]);
		buffer.WriteFloat(row.StatPercentageOfSocket[6]);
		buffer.WriteFloat(row.StatPercentageOfSocket[7]);
		buffer.WriteFloat(row.StatPercentageOfSocket[8]);
		buffer.WriteFloat(row.StatPercentageOfSocket[9]);
		buffer.WriteInt32(row.StatPercentEditor[0]);
		buffer.WriteInt32(row.StatPercentEditor[1]);
		buffer.WriteInt32(row.StatPercentEditor[2]);
		buffer.WriteInt32(row.StatPercentEditor[3]);
		buffer.WriteInt32(row.StatPercentEditor[4]);
		buffer.WriteInt32(row.StatPercentEditor[5]);
		buffer.WriteInt32(row.StatPercentEditor[6]);
		buffer.WriteInt32(row.StatPercentEditor[7]);
		buffer.WriteInt32(row.StatPercentEditor[8]);
		buffer.WriteInt32(row.StatPercentEditor[9]);
		buffer.WriteInt32(row.Stackable);
		buffer.WriteInt32(row.MaxCount);
		buffer.WriteUInt32(row.RequiredAbility);
		buffer.WriteUInt32(row.SellPrice);
		buffer.WriteUInt32(row.BuyPrice);
		buffer.WriteUInt32(row.VendorStackCount);
		buffer.WriteFloat(row.PriceVariance);
		buffer.WriteFloat(row.PriceRandomValue);
		buffer.WriteUInt32(row.Flags[0]);
		buffer.WriteUInt32(row.Flags[1]);
		buffer.WriteUInt32(row.Flags[2]);
		buffer.WriteUInt32(row.Flags[3]);
		buffer.WriteInt32(row.OppositeFactionItemId);
		buffer.WriteUInt32(row.MaxDurability);
		buffer.WriteUInt16(row.ItemNameDescriptionId);
		buffer.WriteUInt16(row.RequiredTransmogHoliday);
		buffer.WriteUInt16(row.RequiredHoliday);
		buffer.WriteUInt16(row.LimitCategory);
		buffer.WriteUInt16(row.GemProperties);
		buffer.WriteUInt16(row.SocketMatchEnchantmentId);
		buffer.WriteUInt16(row.TotemCategoryId);
		buffer.WriteUInt16(row.InstanceBound);
		buffer.WriteUInt16(row.ZoneBound[0]);
		buffer.WriteUInt16(row.ZoneBound[1]);
		buffer.WriteUInt16(row.ItemSet);
		buffer.WriteUInt16(row.LockId);
		buffer.WriteUInt16(row.StartQuestId);
		buffer.WriteUInt16(row.PageText);
		buffer.WriteUInt16(row.Delay);
		buffer.WriteUInt16(row.RequiredReputationId);
		buffer.WriteUInt16(row.RequiredSkillRank);
		buffer.WriteUInt16(row.RequiredSkill);
		buffer.WriteUInt16(row.ItemLevel);
		buffer.WriteInt16(row.AllowableClass);
		buffer.WriteUInt16(row.ItemRandomSuffixGroupId);
		buffer.WriteUInt16(row.RandomProperty);
		buffer.WriteUInt16(row.MinDamage[0]);
		buffer.WriteUInt16(row.MinDamage[1]);
		buffer.WriteUInt16(row.MinDamage[2]);
		buffer.WriteUInt16(row.MinDamage[3]);
		buffer.WriteUInt16(row.MinDamage[4]);
		buffer.WriteUInt16(row.MaxDamage[0]);
		buffer.WriteUInt16(row.MaxDamage[1]);
		buffer.WriteUInt16(row.MaxDamage[2]);
		buffer.WriteUInt16(row.MaxDamage[3]);
		buffer.WriteUInt16(row.MaxDamage[4]);
		buffer.WriteInt16(row.Resistances[0]);
		buffer.WriteInt16(row.Resistances[1]);
		buffer.WriteInt16(row.Resistances[2]);
		buffer.WriteInt16(row.Resistances[3]);
		buffer.WriteInt16(row.Resistances[4]);
		buffer.WriteInt16(row.Resistances[5]);
		buffer.WriteInt16(row.Resistances[6]);
		buffer.WriteUInt16(row.ScalingStatDistributionId);
		buffer.WriteUInt8(row.ExpansionId);
		buffer.WriteUInt8(row.ArtifactId);
		buffer.WriteUInt8(row.SpellWeight);
		buffer.WriteUInt8(row.SpellWeightCategory);
		buffer.WriteUInt8(row.SocketType[0]);
		buffer.WriteUInt8(row.SocketType[1]);
		buffer.WriteUInt8(row.SocketType[2]);
		buffer.WriteUInt8(row.SheatheType);
		buffer.WriteUInt8(row.Material);
		buffer.WriteUInt8(row.PageMaterial);
		buffer.WriteUInt8(row.PageLanguage);
		buffer.WriteUInt8(row.Bonding);
		buffer.WriteUInt8(row.DamageType);
		buffer.WriteInt8(row.StatType[0]);
		buffer.WriteInt8(row.StatType[1]);
		buffer.WriteInt8(row.StatType[2]);
		buffer.WriteInt8(row.StatType[3]);
		buffer.WriteInt8(row.StatType[4]);
		buffer.WriteInt8(row.StatType[5]);
		buffer.WriteInt8(row.StatType[6]);
		buffer.WriteInt8(row.StatType[7]);
		buffer.WriteInt8(row.StatType[8]);
		buffer.WriteInt8(row.StatType[9]);
		buffer.WriteUInt8(row.ContainerSlots);
		buffer.WriteUInt8(row.RequiredReputationRank);
		buffer.WriteUInt8(row.RequiredCityRank);
		buffer.WriteUInt8(row.RequiredHonorRank);
		buffer.WriteUInt8(row.InventoryType);
		buffer.WriteUInt8(row.OverallQualityId);
		buffer.WriteUInt8(row.AmmoType);
		buffer.WriteInt8((sbyte)array[0]);
		buffer.WriteInt8((sbyte)array[1]);
		buffer.WriteInt8((sbyte)array[2]);
		buffer.WriteInt8((sbyte)array[3]);
		buffer.WriteInt8((sbyte)array[4]);
		buffer.WriteInt8((sbyte)array[5]);
		buffer.WriteInt8((sbyte)array[6]);
		buffer.WriteInt8((sbyte)array[7]);
		buffer.WriteInt8((sbyte)array[8]);
		buffer.WriteInt8((sbyte)array[9]);
		buffer.WriteInt8(row.RequiredLevel);
	}

	public static void LoadItemHotfixes()
	{
		using TextFieldParser textFieldParser = new TextFieldParser(Path.Combine("CSV", "Hotfix", $"Item{ModernVersion.ExpansionVersion}.csv"));
		textFieldParser.CommentTokens = new string[1] { "#" };
		textFieldParser.SetDelimiters(",");
		textFieldParser.HasFieldsEnclosedInQuotes = false;
		textFieldParser.ReadLine();
		uint num = 0u;
		while (!textFieldParser.EndOfData)
		{
			num++;
			string[] array = textFieldParser.ReadFields();
			uint recordId = uint.Parse(array[0]);
			byte data = byte.Parse(array[1]);
			byte data2 = byte.Parse(array[2]);
			byte data3 = byte.Parse(array[3]);
			sbyte data4 = sbyte.Parse(array[4]);
			uint data5 = uint.Parse(array[5]);
			byte data6 = byte.Parse(array[6]);
			ushort data7 = ushort.Parse(array[7]);
			ushort data8 = ushort.Parse(array[8]);
			sbyte data9 = sbyte.Parse(array[9]);
			ushort data10 = ushort.Parse(array[10]);
			int data11 = int.Parse(array[11]);
			byte data12 = byte.Parse(array[12]);
			int data13 = int.Parse(array[13]);
			uint data14 = uint.Parse(array[14]);
			byte data15 = byte.Parse(array[15]);
			byte data16 = byte.Parse(array[16]);
			byte data17 = byte.Parse(array[17]);
			byte data18 = byte.Parse(array[18]);
			byte data19 = byte.Parse(array[19]);
			byte data20 = byte.Parse(array[20]);
			short data21 = short.Parse(array[21]);
			short data22 = short.Parse(array[22]);
			short data23 = short.Parse(array[23]);
			short data24 = short.Parse(array[24]);
			short data25 = short.Parse(array[25]);
			short data26 = short.Parse(array[26]);
			short data27 = short.Parse(array[27]);
			ushort data28 = ushort.Parse(array[28]);
			ushort data29 = ushort.Parse(array[29]);
			ushort data30 = ushort.Parse(array[30]);
			ushort data31 = ushort.Parse(array[31]);
			ushort data32 = ushort.Parse(array[32]);
			ushort data33 = ushort.Parse(array[33]);
			ushort data34 = ushort.Parse(array[34]);
			ushort data35 = ushort.Parse(array[35]);
			ushort data36 = ushort.Parse(array[36]);
			ushort data37 = ushort.Parse(array[37]);
			HotfixRecord hotfixRecord = new HotfixRecord();
			hotfixRecord.Status = HotfixStatus.Valid;
			hotfixRecord.TableHash = DB2Hash.Item;
			hotfixRecord.HotfixId = 210000 + num;
			hotfixRecord.UniqueId = hotfixRecord.HotfixId;
			hotfixRecord.RecordId = recordId;
			hotfixRecord.HotfixContent.WriteUInt8(data);
			hotfixRecord.HotfixContent.WriteUInt8(data2);
			hotfixRecord.HotfixContent.WriteUInt8(data3);
			hotfixRecord.HotfixContent.WriteInt8(data4);
			hotfixRecord.HotfixContent.WriteUInt32(data5);
			hotfixRecord.HotfixContent.WriteUInt8(data6);
			hotfixRecord.HotfixContent.WriteUInt16(data7);
			hotfixRecord.HotfixContent.WriteUInt16(data8);
			hotfixRecord.HotfixContent.WriteInt8(data9);
			hotfixRecord.HotfixContent.WriteUInt16(data10);
			hotfixRecord.HotfixContent.WriteInt32(data11);
			hotfixRecord.HotfixContent.WriteUInt8(data12);
			hotfixRecord.HotfixContent.WriteInt32(data13);
			hotfixRecord.HotfixContent.WriteUInt32(data14);
			hotfixRecord.HotfixContent.WriteUInt8(data15);
			hotfixRecord.HotfixContent.WriteUInt8(data16);
			hotfixRecord.HotfixContent.WriteUInt8(data17);
			hotfixRecord.HotfixContent.WriteUInt8(data18);
			hotfixRecord.HotfixContent.WriteUInt8(data19);
			hotfixRecord.HotfixContent.WriteUInt8(data20);
			hotfixRecord.HotfixContent.WriteInt16(data21);
			hotfixRecord.HotfixContent.WriteInt16(data22);
			hotfixRecord.HotfixContent.WriteInt16(data23);
			hotfixRecord.HotfixContent.WriteInt16(data24);
			hotfixRecord.HotfixContent.WriteInt16(data25);
			hotfixRecord.HotfixContent.WriteInt16(data26);
			hotfixRecord.HotfixContent.WriteInt16(data27);
			hotfixRecord.HotfixContent.WriteUInt16(data28);
			hotfixRecord.HotfixContent.WriteUInt16(data29);
			hotfixRecord.HotfixContent.WriteUInt16(data30);
			hotfixRecord.HotfixContent.WriteUInt16(data31);
			hotfixRecord.HotfixContent.WriteUInt16(data32);
			hotfixRecord.HotfixContent.WriteUInt16(data33);
			hotfixRecord.HotfixContent.WriteUInt16(data34);
			hotfixRecord.HotfixContent.WriteUInt16(data35);
			hotfixRecord.HotfixContent.WriteUInt16(data36);
			hotfixRecord.HotfixContent.WriteUInt16(data37);
			Hotfixes.Add(hotfixRecord.HotfixId, hotfixRecord);
		}
	}

	public static void WriteItemHotfix(ItemTemplate item, ByteBuffer buffer)
	{
		int itemIconFileDataIdByDisplayId = (int)GetItemIconFileDataIdByDisplayId(item.DisplayID);
		buffer.WriteUInt8((byte)item.Class);
		buffer.WriteUInt8((byte)item.SubClass);
		buffer.WriteUInt8((byte)item.Material);
		buffer.WriteInt8((sbyte)item.InventoryType);
		buffer.WriteInt32((int)item.RequiredLevel);
		buffer.WriteUInt8((byte)item.SheathType);
		buffer.WriteUInt16((ushort)item.RandomProperty);
		buffer.WriteUInt16((ushort)item.RandomSuffix);
		buffer.WriteInt8(-1);
		buffer.WriteUInt16(0);
		buffer.WriteInt32(itemIconFileDataIdByDisplayId);
		buffer.WriteUInt8(0);
		buffer.WriteInt32(0);
		buffer.WriteUInt32(item.MaxDurability);
		buffer.WriteUInt8((byte)item.AmmoType);
		buffer.WriteUInt8((byte)item.DamageTypes[0]);
		buffer.WriteUInt8((byte)item.DamageTypes[1]);
		buffer.WriteUInt8((byte)item.DamageTypes[2]);
		buffer.WriteUInt8((byte)item.DamageTypes[3]);
		buffer.WriteUInt8((byte)item.DamageTypes[4]);
		buffer.WriteInt16((short)item.Armor);
		buffer.WriteInt16((short)item.HolyResistance);
		buffer.WriteInt16((short)item.FireResistance);
		buffer.WriteInt16((short)item.NatureResistance);
		buffer.WriteInt16((short)item.FrostResistance);
		buffer.WriteInt16((short)item.ShadowResistance);
		buffer.WriteInt16((short)item.ArcaneResistance);
		buffer.WriteUInt16((ushort)item.DamageMins[0]);
		buffer.WriteUInt16((ushort)item.DamageMins[1]);
		buffer.WriteUInt16((ushort)item.DamageMins[2]);
		buffer.WriteUInt16((ushort)item.DamageMins[3]);
		buffer.WriteUInt16((ushort)item.DamageMins[4]);
		buffer.WriteUInt16((ushort)item.DamageMaxs[0]);
		buffer.WriteUInt16((ushort)item.DamageMaxs[1]);
		buffer.WriteUInt16((ushort)item.DamageMaxs[2]);
		buffer.WriteUInt16((ushort)item.DamageMaxs[3]);
		buffer.WriteUInt16((ushort)item.DamageMaxs[4]);
	}

	public static void WriteItemHotfix(ItemRecord row, ByteBuffer buffer)
	{
		buffer.WriteUInt8(row.ClassId);
		buffer.WriteUInt8(row.SubclassId);
		buffer.WriteUInt8(row.Material);
		buffer.WriteInt8(row.InventoryType);
		buffer.WriteInt32(row.RequiredLevel);
		buffer.WriteUInt8(row.SheatheType);
		buffer.WriteUInt16(row.RandomProperty);
		buffer.WriteUInt16(row.ItemRandomSuffixGroupId);
		buffer.WriteInt8(row.SoundOverrideSubclassId);
		buffer.WriteUInt16(row.ScalingStatDistributionId);
		buffer.WriteInt32(row.IconFileDataId);
		buffer.WriteUInt8(row.ItemGroupSoundsId);
		buffer.WriteInt32(row.ContentTuningId);
		buffer.WriteUInt32(row.MaxDurability);
		buffer.WriteUInt8(row.AmmoType);
		buffer.WriteUInt8(row.DamageType[0]);
		buffer.WriteUInt8(row.DamageType[1]);
		buffer.WriteUInt8(row.DamageType[2]);
		buffer.WriteUInt8(row.DamageType[3]);
		buffer.WriteUInt8(row.DamageType[4]);
		buffer.WriteInt16(row.Resistances[0]);
		buffer.WriteInt16(row.Resistances[1]);
		buffer.WriteInt16(row.Resistances[2]);
		buffer.WriteInt16(row.Resistances[3]);
		buffer.WriteInt16(row.Resistances[4]);
		buffer.WriteInt16(row.Resistances[5]);
		buffer.WriteInt16(row.Resistances[6]);
		buffer.WriteUInt16(row.MinDamage[0]);
		buffer.WriteUInt16(row.MinDamage[1]);
		buffer.WriteUInt16(row.MinDamage[2]);
		buffer.WriteUInt16(row.MinDamage[3]);
		buffer.WriteUInt16(row.MinDamage[4]);
		buffer.WriteUInt16(row.MaxDamage[0]);
		buffer.WriteUInt16(row.MaxDamage[1]);
		buffer.WriteUInt16(row.MaxDamage[2]);
		buffer.WriteUInt16(row.MaxDamage[3]);
		buffer.WriteUInt16(row.MaxDamage[4]);
	}

	public static void WriteItemAppearanceHotfix(ItemAppearance appearance, ByteBuffer buffer)
	{
		buffer.WriteUInt8(appearance.DisplayType);
		buffer.WriteInt32(appearance.ItemDisplayInfoID);
		buffer.WriteInt32(appearance.DefaultIconFileDataID);
		buffer.WriteInt32(appearance.UiOrder);
	}

	public static void WriteItemModifiedAppearanceHotfix(ItemModifiedAppearance modAppearance, ByteBuffer buffer)
	{
		buffer.WriteInt32(modAppearance.Id);
		buffer.WriteInt32(modAppearance.ItemID);
		buffer.WriteInt32(modAppearance.ItemAppearanceModifierID);
		buffer.WriteInt32(modAppearance.ItemAppearanceID);
		buffer.WriteInt32(modAppearance.OrderIndex);
		buffer.WriteInt32(modAppearance.TransmogSourceTypeEnum);
	}

	public static void WriteItemEffectHotfix(ItemEffect effect, ByteBuffer buffer)
	{
		buffer.WriteUInt8(effect.LegacySlotIndex);
		buffer.WriteInt8(effect.TriggerType);
		buffer.WriteInt16(effect.Charges);
		buffer.WriteInt32(effect.CoolDownMSec);
		buffer.WriteInt32(effect.CategoryCoolDownMSec);
		buffer.WriteUInt16(effect.SpellCategoryID);
		buffer.WriteInt32(effect.SpellID);
		buffer.WriteUInt16(effect.ChrSpecializationID);
		buffer.WriteInt32(effect.ParentItemID);
	}

	public static List<HotfixRecord> FindHotfixesByRecordIdAndTable(uint id, DB2Hash table, uint startId = 0u)
	{
		return Hotfixes.Values.Where((HotfixRecord hotfix) => hotfix.HotfixId >= startId && hotfix.TableHash == table && hotfix.RecordId == id).ToList();
	}

	public static void UpdateHotfix(object obj, bool remove = false)
	{
		if (obj is ItemRecord)
		{
			ItemRecord item = (ItemRecord)obj;
			DoStuff((uint)item.Id, DB2Hash.Item, delegate(ByteBuffer hotfixContentTargetBuffer)
			{
				WriteItemHotfix(item, hotfixContentTargetBuffer);
			});
		}
		if (obj is ItemSparseRecord)
		{
			ItemSparseRecord itemSparse = (ItemSparseRecord)obj;
			DoStuff((uint)itemSparse.Id, DB2Hash.ItemSparse, delegate(ByteBuffer hotfixContentTargetBuffer)
			{
				WriteItemSparseHotfix(itemSparse, hotfixContentTargetBuffer);
			});
		}
		if (obj is ItemEffect)
		{
			ItemEffect effect = (ItemEffect)obj;
			DoStuff((uint)effect.Id, DB2Hash.ItemEffect, delegate(ByteBuffer hotfixContentTargetBuffer)
			{
				WriteItemEffectHotfix(effect, hotfixContentTargetBuffer);
			});
		}
		if (obj is ItemAppearance)
		{
			ItemAppearance appearance = (ItemAppearance)obj;
			DoStuff((uint)appearance.Id, DB2Hash.ItemAppearance, delegate(ByteBuffer hotfixContentTargetBuffer)
			{
				WriteItemAppearanceHotfix(appearance, hotfixContentTargetBuffer);
			});
		}
		if (obj is ItemModifiedAppearance)
		{
			ItemModifiedAppearance modAppearance = (ItemModifiedAppearance)obj;
			DoStuff((uint)modAppearance.Id, DB2Hash.ItemModifiedAppearance, delegate(ByteBuffer hotfixContentTargetBuffer)
			{
				WriteItemModifiedAppearanceHotfix(modAppearance, hotfixContentTargetBuffer);
			});
		}
		static void DoStuff(uint recordId, DB2Hash table, Action<ByteBuffer> writer)
		{
			List<HotfixRecord> list = FindHotfixesByRecordIdAndTable(recordId, table, 210000u);
			if (list.Count == 0)
			{
				HotfixRecord hotfixRecord = new HotfixRecord();
				hotfixRecord.Status = HotfixStatus.Valid;
				hotfixRecord.TableHash = table;
				hotfixRecord.HotfixId = GetFirstFreeId(Hotfixes, 210000u);
				hotfixRecord.UniqueId = hotfixRecord.HotfixId;
				hotfixRecord.RecordId = recordId;
				writer(hotfixRecord.HotfixContent);
				Hotfixes.Add(hotfixRecord.HotfixId, hotfixRecord);
			}
			else
			{
				foreach (HotfixRecord item2 in list.SkipLast(1))
				{
					item2.Status = HotfixStatus.Invalid;
					item2.HotfixContent = new ByteBuffer();
					Log.Print(LogType.Storage, $"Got duplicate record for record {item2.RecordId} in {item2.TableHash}", "UpdateHotfix", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				HotfixRecord hotfixRecord2 = list.Last();
				hotfixRecord2.HotfixContent = new ByteBuffer();
				writer(hotfixRecord2.HotfixContent);
				Hotfixes[hotfixRecord2.HotfixId] = hotfixRecord2;
			}
		}
	}

	public static HotFixMessage? GenerateItemUpdateIfNeeded(ItemTemplate item)
	{
		ItemRecordsStore.TryGetValue(item.Entry, out var value);
		if (value != null)
		{
			int itemIconFileDataIdByDisplayId = (int)GetItemIconFileDataIdByDisplayId(item.DisplayID);
			if (value.ClassId != (byte)item.Class || value.SubclassId != (byte)item.SubClass || value.Material != (byte)item.Material || value.InventoryType != (sbyte)item.InventoryType || value.RequiredLevel != (int)item.RequiredLevel || value.SheatheType != (byte)item.SheathType || value.RandomProperty != (ushort)item.RandomProperty || value.ItemRandomSuffixGroupId != (ushort)item.RandomSuffix || (value.IconFileDataId != itemIconFileDataIdByDisplayId && itemIconFileDataIdByDisplayId != 0) || value.MaxDurability != item.MaxDurability || value.AmmoType != (byte)item.AmmoType || value.DamageType[0] != (byte)item.DamageTypes[0] || value.DamageType[1] != (byte)item.DamageTypes[1] || value.DamageType[2] != (byte)item.DamageTypes[2] || value.DamageType[3] != (byte)item.DamageTypes[3] || value.DamageType[4] != (byte)item.DamageTypes[4] || value.Resistances[1] != (short)item.HolyResistance || value.Resistances[2] != (short)item.FireResistance || value.Resistances[3] != (short)item.NatureResistance || value.Resistances[4] != (short)item.FrostResistance || value.Resistances[5] != (short)item.ShadowResistance || value.Resistances[6] != (short)item.ArcaneResistance)
			{
				Log.Print(LogType.Storage, $"Item #{item.Entry} needs to be updated.", "GenerateItemUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				if (value.ClassId != (byte)item.Class)
				{
					Log.Print(LogType.Storage, $"ClassId {value.ClassId} vs {item.Class}", "GenerateItemUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (value.SubclassId != (byte)item.SubClass)
				{
					Log.Print(LogType.Storage, $"SubclassId {value.SubclassId} vs {item.SubClass}", "GenerateItemUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (value.Material != (byte)item.Material)
				{
					Log.Print(LogType.Storage, $"Material {value.Material} vs {item.Material}", "GenerateItemUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (value.InventoryType != (sbyte)item.InventoryType)
				{
					Log.Print(LogType.Storage, $"InventoryType {value.InventoryType} vs {item.InventoryType}", "GenerateItemUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (value.RequiredLevel != (int)item.RequiredLevel)
				{
					Log.Print(LogType.Storage, $"RequiredLevel {value.RequiredLevel} vs {item.RequiredLevel}", "GenerateItemUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (value.SheatheType != (byte)item.SheathType)
				{
					Log.Print(LogType.Storage, $"SheatheType {value.SheatheType} vs {item.SheathType}", "GenerateItemUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (value.RandomProperty != (ushort)item.RandomProperty)
				{
					Log.Print(LogType.Storage, $"RandomProperty {value.RandomProperty} vs {item.RandomProperty}", "GenerateItemUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (value.ItemRandomSuffixGroupId != (ushort)item.RandomSuffix)
				{
					Log.Print(LogType.Storage, $"ItemRandomSuffixGroupId {value.ItemRandomSuffixGroupId} vs {item.RandomSuffix}", "GenerateItemUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (value.IconFileDataId != itemIconFileDataIdByDisplayId)
				{
					Log.Print(LogType.Storage, $"IconFileDataId {value.IconFileDataId} vs {itemIconFileDataIdByDisplayId}", "GenerateItemUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (value.MaxDurability != item.MaxDurability)
				{
					Log.Print(LogType.Storage, $"MaxDurability {value.MaxDurability} vs {item.MaxDurability}", "GenerateItemUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (value.AmmoType != (byte)item.AmmoType)
				{
					Log.Print(LogType.Storage, $"AmmoType {value.AmmoType} vs {item.AmmoType}", "GenerateItemUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				for (int i = 0; i < 5; i++)
				{
					if (value.DamageType[i] != (byte)item.DamageTypes[i])
					{
						Log.Print(LogType.Storage, $"DamageType[{i}] {value.DamageType[i]} vs {item.DamageTypes[i]}", "GenerateItemUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
					}
				}
				if (value.Resistances[1] != (short)item.HolyResistance)
				{
					Log.Print(LogType.Storage, $"Resistances[1] {value.Resistances[1]} vs {item.HolyResistance}", "GenerateItemUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (value.Resistances[2] != (short)item.FireResistance)
				{
					Log.Print(LogType.Storage, $"Resistances[2] {value.Resistances[2]} vs {item.FireResistance}", "GenerateItemUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (value.Resistances[3] != (short)item.NatureResistance)
				{
					Log.Print(LogType.Storage, $"Resistances[3] {value.Resistances[3]} vs {item.NatureResistance}", "GenerateItemUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (value.Resistances[4] != (short)item.FrostResistance)
				{
					Log.Print(LogType.Storage, $"Resistances[4] {value.Resistances[4]} vs {item.FrostResistance}", "GenerateItemUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (value.Resistances[5] != (short)item.ShadowResistance)
				{
					Log.Print(LogType.Storage, $"Resistances[5] {value.Resistances[5]} vs {item.ShadowResistance}", "GenerateItemUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (value.Resistances[6] != (short)item.ArcaneResistance)
				{
					Log.Print(LogType.Storage, $"Resistances[6] {value.Resistances[6]} vs {item.ArcaneResistance}", "GenerateItemUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				UpdateItemRecord(value, item);
				UpdateHotfix(value);
				return GenerateHotFixMessage(value);
			}
			return null;
		}
		value = AddItemRecord(item);
		if (value == null)
		{
			return null;
		}
		UpdateHotfix(value);
		return GenerateHotFixMessage(value);
	}

	public static HotFixMessage? GenerateItemSparseUpdateIfNeeded(ItemTemplate item)
	{
		ItemSparseRecordsStore.TryGetValue(item.Entry, out var value);
		if (value != null)
		{
			if (!value.Description.Equals(item.Description) || !value.Name4.Equals(item.Name[3]) || !value.Name3.Equals(item.Name[2]) || !value.Name2.Equals(item.Name[1]) || !value.Name1.Equals(item.Name[0]) || value.DurationInInventory != item.Duration || value.BagFamily != item.BagFamily || value.RangeMod != item.RangedMod || value.RequiredAbility != item.RequiredSpell || value.SellPrice != item.SellPrice || value.BuyPrice != item.BuyPrice || value.MaxDurability != item.MaxDurability || value.RequiredHoliday != (ushort)item.HolidayID || value.LimitCategory != (ushort)item.ItemLimitCategory || value.GemProperties != (ushort)item.GemProperties || value.SocketMatchEnchantmentId != (ushort)item.SocketBonus || value.TotemCategoryId != (ushort)item.TotemCategory || value.InstanceBound != (ushort)item.MapID || value.ZoneBound[0] != (ushort)item.AreaID || value.ItemSet != (ushort)item.ItemSet || value.LockId != (ushort)item.LockId || value.StartQuestId != (ushort)item.StartQuestId || value.PageText != (ushort)item.PageText || value.Delay != (ushort)item.Delay || value.RequiredReputationId != (ushort)item.RequiredRepFaction || value.RequiredSkillRank != (ushort)item.RequiredSkillLevel || value.RequiredSkill != (ushort)item.RequiredSkillId || value.ItemLevel != (ushort)item.ItemLevel || value.ItemRandomSuffixGroupId != (ushort)item.RandomSuffix || value.RandomProperty != (ushort)item.RandomProperty || value.Resistances[1] != (short)item.HolyResistance || value.Resistances[2] != (short)item.FireResistance || value.Resistances[3] != (short)item.NatureResistance || value.Resistances[4] != (short)item.FrostResistance || value.Resistances[5] != (short)item.ShadowResistance || value.Resistances[6] != (short)item.ArcaneResistance || value.ScalingStatDistributionId != (ushort)item.ScalingStatDistribution || value.SocketType[0] != ModernVersion.ConvertSocketColor((byte)item.ItemSocketColors[0]) || value.SocketType[1] != ModernVersion.ConvertSocketColor((byte)item.ItemSocketColors[1]) || value.SocketType[2] != ModernVersion.ConvertSocketColor((byte)item.ItemSocketColors[2]) || value.SheatheType != (byte)item.SheathType || value.Material != (byte)item.Material || value.PageMaterial != (byte)item.PageMaterial || value.PageLanguage != (byte)item.Language || value.Bonding != (byte)item.Bonding || value.DamageType != (byte)item.DamageTypes[0] || (value.StatType[0] != (sbyte)item.StatTypes[0] && (value.StatValue[0] != 0 || item.StatValues[0] != 0)) || (value.StatType[1] != (sbyte)item.StatTypes[1] && (value.StatValue[1] != 0 || item.StatValues[1] != 0)) || (value.StatType[2] != (sbyte)item.StatTypes[2] && (value.StatValue[2] != 0 || item.StatValues[2] != 0)) || (value.StatType[3] != (sbyte)item.StatTypes[3] && (value.StatValue[3] != 0 || item.StatValues[3] != 0)) || (value.StatType[4] != (sbyte)item.StatTypes[4] && (value.StatValue[4] != 0 || item.StatValues[4] != 0)) || (value.StatType[5] != (sbyte)item.StatTypes[5] && (value.StatValue[5] != 0 || item.StatValues[5] != 0)) || (value.StatType[6] != (sbyte)item.StatTypes[6] && (value.StatValue[6] != 0 || item.StatValues[6] != 0)) || (value.StatType[7] != (sbyte)item.StatTypes[7] && (value.StatValue[7] != 0 || item.StatValues[7] != 0)) || (value.StatType[8] != (sbyte)item.StatTypes[8] && (value.StatValue[8] != 0 || item.StatValues[8] != 0)) || (value.StatType[9] != (sbyte)item.StatTypes[9] && (value.StatValue[9] != 0 || item.StatValues[9] != 0)) || value.ContainerSlots != (byte)item.ContainerSlots || value.RequiredReputationRank != (byte)item.RequiredRepValue || value.RequiredCityRank != (byte)item.RequiredCityRank || value.RequiredHonorRank != (byte)item.RequiredHonorRank || value.InventoryType != (byte)item.InventoryType || value.OverallQualityId != (byte)item.Quality || value.AmmoType != (byte)item.AmmoType || value.StatValue[0] != (sbyte)item.StatValues[0] || value.StatValue[1] != (sbyte)item.StatValues[1] || value.StatValue[2] != (sbyte)item.StatValues[2] || value.StatValue[3] != (sbyte)item.StatValues[3] || value.StatValue[4] != (sbyte)item.StatValues[4] || value.StatValue[5] != (sbyte)item.StatValues[5] || value.StatValue[6] != (sbyte)item.StatValues[6] || value.StatValue[7] != (sbyte)item.StatValues[7] || value.StatValue[8] != (sbyte)item.StatValues[8] || value.StatValue[9] != (sbyte)item.StatValues[9] || value.RequiredLevel != (sbyte)item.RequiredLevel)
			{
				Log.Print(LogType.Storage, $"ItemSparse #{item.Entry} needs to be updated.", "GenerateItemSparseUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				if (!value.Description.Equals(item.Description))
				{
					Log.Print(LogType.Storage, $"Description \"{value.Description}\" vs \"{item.Description}\"", "GenerateItemSparseUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (!value.Name4.Equals(item.Name[3]))
				{
					Log.Print(LogType.Storage, $"Name4 \"{value.Name4}\" vs \"{item.Name[3]}\"", "GenerateItemSparseUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (!value.Name3.Equals(item.Name[2]))
				{
					Log.Print(LogType.Storage, $"Name3 \"{value.Name3}\" vs \"{item.Name[2]}\"", "GenerateItemSparseUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (!value.Name2.Equals(item.Name[1]))
				{
					Log.Print(LogType.Storage, $"Name2 \"{value.Name2}\" vs \"{item.Name[1]}\"", "GenerateItemSparseUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (!value.Name1.Equals(item.Name[0]))
				{
					Log.Print(LogType.Storage, $"Name1 \"{value.Name1}\" vs \"{item.Name[0]}\"", "GenerateItemSparseUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (value.DurationInInventory != item.Duration)
				{
					Log.Print(LogType.Storage, $"DurationInInventory {value.DurationInInventory} vs {item.Duration}", "GenerateItemSparseUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (value.BagFamily != item.BagFamily)
				{
					Log.Print(LogType.Storage, $"BagFamily {value.BagFamily} vs {item.BagFamily}", "GenerateItemSparseUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (value.RangeMod != item.RangedMod)
				{
					Log.Print(LogType.Storage, $"RangeMod {value.RangeMod} vs {item.RangedMod}", "GenerateItemSparseUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (value.RequiredAbility != item.RequiredSpell)
				{
					Log.Print(LogType.Storage, $"RequiredAbility {value.RequiredAbility} vs {item.RequiredSpell}", "GenerateItemSparseUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (value.SellPrice != item.SellPrice)
				{
					Log.Print(LogType.Storage, $"SellPrice {value.SellPrice} vs {item.SellPrice}", "GenerateItemSparseUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (value.BuyPrice != item.BuyPrice)
				{
					Log.Print(LogType.Storage, $"BuyPrice {value.BuyPrice} vs {item.BuyPrice}", "GenerateItemSparseUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (value.MaxDurability != item.MaxDurability)
				{
					Log.Print(LogType.Storage, $"MaxDurability {value.MaxDurability} vs {item.MaxDurability}", "GenerateItemSparseUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (value.RequiredHoliday != (ushort)item.HolidayID)
				{
					Log.Print(LogType.Storage, $"RequiredHoliday {value.RequiredHoliday} vs {item.HolidayID}", "GenerateItemSparseUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (value.LimitCategory != (ushort)item.ItemLimitCategory)
				{
					Log.Print(LogType.Storage, $"LimitCategory {value.LimitCategory} vs {item.ItemLimitCategory}", "GenerateItemSparseUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (value.GemProperties != (ushort)item.GemProperties)
				{
					Log.Print(LogType.Storage, $"GemProperties {value.GemProperties} vs {item.GemProperties}", "GenerateItemSparseUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (value.SocketMatchEnchantmentId != (ushort)item.SocketBonus)
				{
					Log.Print(LogType.Storage, $"SocketMatchEnchantmentId {value.SocketMatchEnchantmentId} vs {item.SocketBonus}", "GenerateItemSparseUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (value.TotemCategoryId != (ushort)item.TotemCategory)
				{
					Log.Print(LogType.Storage, $"TotemCategoryId {value.TotemCategoryId} vs {item.TotemCategory}", "GenerateItemSparseUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (value.InstanceBound != (ushort)item.MapID)
				{
					Log.Print(LogType.Storage, $"InstanceBound {value.InstanceBound} vs {item.MapID}", "GenerateItemSparseUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (value.ZoneBound[0] != (ushort)item.AreaID)
				{
					Log.Print(LogType.Storage, $"ZoneBound[0] {value.ZoneBound[0]} vs {item.AreaID}", "GenerateItemSparseUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (value.ItemSet != (ushort)item.ItemSet)
				{
					Log.Print(LogType.Storage, $"ItemSet {value.ItemSet} vs {item.ItemSet}", "GenerateItemSparseUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (value.LockId != (ushort)item.LockId)
				{
					Log.Print(LogType.Storage, $"LockId {value.LockId} vs {item.LockId}", "GenerateItemSparseUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (value.StartQuestId != (ushort)item.StartQuestId)
				{
					Log.Print(LogType.Storage, $"StartQuestId {value.StartQuestId} vs {item.StartQuestId}", "GenerateItemSparseUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (value.PageText != (ushort)item.PageText)
				{
					Log.Print(LogType.Storage, $"PageText {value.PageText} vs {item.PageText}", "GenerateItemSparseUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (value.Delay != (ushort)item.Delay)
				{
					Log.Print(LogType.Storage, $"Delay {value.Delay} vs {item.Delay}", "GenerateItemSparseUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (value.RequiredReputationId != (ushort)item.RequiredRepFaction)
				{
					Log.Print(LogType.Storage, $"RequiredReputationId {value.RequiredReputationId} vs {item.RequiredRepFaction}", "GenerateItemSparseUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (value.RequiredSkillRank != (ushort)item.RequiredSkillLevel)
				{
					Log.Print(LogType.Storage, $"RequiredSkillRank {value.RequiredSkillRank} vs {item.RequiredSkillLevel}", "GenerateItemSparseUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (value.RequiredSkill != (ushort)item.RequiredSkillId)
				{
					Log.Print(LogType.Storage, $"RequiredSkill {value.RequiredSkill} vs {item.RequiredSkillId}", "GenerateItemSparseUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (value.ItemLevel != (ushort)item.ItemLevel)
				{
					Log.Print(LogType.Storage, $"ItemLevel {value.ItemLevel} vs {item.ItemLevel}", "GenerateItemSparseUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (value.ItemRandomSuffixGroupId != (ushort)item.RandomSuffix)
				{
					Log.Print(LogType.Storage, $"ItemRandomSuffixGroupId {value.ItemRandomSuffixGroupId} vs {item.RandomSuffix}", "GenerateItemSparseUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (value.RandomProperty != (ushort)item.RandomProperty)
				{
					Log.Print(LogType.Storage, $"RandomProperty {value.RandomProperty} vs {item.RandomProperty}", "GenerateItemSparseUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (value.Resistances[1] != (short)item.HolyResistance)
				{
					Log.Print(LogType.Storage, $"Resistances[1] {value.Resistances[1]} vs {item.HolyResistance}", "GenerateItemSparseUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (value.Resistances[2] != (short)item.FireResistance)
				{
					Log.Print(LogType.Storage, $"Resistances[2] {value.Resistances[2]} vs {item.FireResistance}", "GenerateItemSparseUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (value.Resistances[3] != (short)item.NatureResistance)
				{
					Log.Print(LogType.Storage, $"Resistances[3]  {value.Resistances[3]} vs {item.NatureResistance}", "GenerateItemSparseUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (value.Resistances[4] != (short)item.FrostResistance)
				{
					Log.Print(LogType.Storage, $"Resistances[4] {value.Resistances[4]} vs {item.FrostResistance}", "GenerateItemSparseUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (value.Resistances[5] != (short)item.ShadowResistance)
				{
					Log.Print(LogType.Storage, $"Resistances[5] {value.Resistances[5]} vs {item.ShadowResistance}", "GenerateItemSparseUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (value.Resistances[6] != (short)item.ArcaneResistance)
				{
					Log.Print(LogType.Storage, $"Resistances[6] {value.Resistances[6]} vs {item.ArcaneResistance}", "GenerateItemSparseUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (value.ScalingStatDistributionId != (ushort)item.ScalingStatDistribution)
				{
					Log.Print(LogType.Storage, $"ScalingStatDistributionId {value.ScalingStatDistributionId} vs {item.ScalingStatDistribution}", "GenerateItemSparseUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				for (int i = 0; i < 3; i++)
				{
					if (value.SocketType[i] != ModernVersion.ConvertSocketColor((byte)item.ItemSocketColors[i]))
					{
						Log.Print(LogType.Storage, $"SocketType[{i}] {value.SocketType[i]} vs {ModernVersion.ConvertSocketColor((byte)item.ItemSocketColors[i])}", "GenerateItemSparseUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
					}
				}
				if (value.SheatheType != (byte)item.SheathType)
				{
					Log.Print(LogType.Storage, $"SheatheType {value.SheatheType} vs {item.SheathType}", "GenerateItemSparseUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (value.Material != (byte)item.Material)
				{
					Log.Print(LogType.Storage, $"Material {value.Material} vs {item.Material}", "GenerateItemSparseUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (value.PageMaterial != (byte)item.PageMaterial)
				{
					Log.Print(LogType.Storage, $"PageMaterial {value.PageMaterial} vs {item.PageMaterial}", "GenerateItemSparseUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (value.PageLanguage != (byte)item.Language)
				{
					Log.Print(LogType.Storage, $"PageLanguage {value.PageLanguage} vs {item.Language}", "GenerateItemSparseUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (value.Bonding != (byte)item.Bonding)
				{
					Log.Print(LogType.Storage, $"Bonding {value.Bonding} vs {item.Bonding}", "GenerateItemSparseUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (value.DamageType != (byte)item.DamageTypes[0])
				{
					Log.Print(LogType.Storage, $"DamageType {value.DamageType} vs {item.DamageTypes[0]}", "GenerateItemSparseUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				for (int j = 0; j < 10; j++)
				{
					if (value.StatType[j] != (sbyte)item.StatTypes[j] && (value.StatValue[j] != 0 || item.StatValues[j] != 0))
					{
						Log.Print(LogType.Storage, $"StatType[{j}] {value.StatType[j]} vs {item.StatTypes[j]}", "GenerateItemSparseUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
					}
				}
				if (value.ContainerSlots != (byte)item.ContainerSlots)
				{
					Log.Print(LogType.Storage, $"ContainerSlots {value.ContainerSlots} vs {item.ContainerSlots}", "GenerateItemSparseUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (value.RequiredReputationRank != (byte)item.RequiredRepValue)
				{
					Log.Print(LogType.Storage, $"RequiredReputationRank {value.RequiredReputationRank} vs {item.RequiredRepValue}", "GenerateItemSparseUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (value.RequiredCityRank != (byte)item.RequiredCityRank)
				{
					Log.Print(LogType.Storage, $"RequiredCityRank {value.RequiredCityRank} vs {item.RequiredCityRank}", "GenerateItemSparseUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (value.RequiredHonorRank != (byte)item.RequiredHonorRank)
				{
					Log.Print(LogType.Storage, $"RequiredHonorRank {value.RequiredHonorRank} vs {item.RequiredHonorRank}", "GenerateItemSparseUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (value.InventoryType != (byte)item.InventoryType)
				{
					Log.Print(LogType.Storage, $"InventoryType {value.InventoryType} vs {item.InventoryType}", "GenerateItemSparseUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (value.OverallQualityId != (byte)item.Quality)
				{
					Log.Print(LogType.Storage, $"OverallQualityId {value.OverallQualityId} vs {item.Quality}", "GenerateItemSparseUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				if (value.AmmoType != (byte)item.AmmoType)
				{
					Log.Print(LogType.Storage, $"AmmoType {value.AmmoType} vs {item.AmmoType}", "GenerateItemSparseUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				for (int k = 0; k < 10; k++)
				{
					if (value.StatValue[0] != (sbyte)item.StatValues[0])
					{
						Log.Print(LogType.Storage, $"StatValue[{k}] {value.StatValue[k]} vs {item.StatValues[k]}", "GenerateItemSparseUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
					}
				}
				if (value.RequiredLevel != (sbyte)item.RequiredLevel)
				{
					Log.Print(LogType.Storage, $"RequiredLevel {value.RequiredLevel} vs {item.RequiredLevel}", "GenerateItemSparseUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				UpdateItemSparseRecord(value, item);
				UpdateHotfix(value);
				return null;
			}
			return null;
		}
		value = AddItemSparseRecord(item);
		if (value == null)
		{
			return null;
		}
		UpdateHotfix(value);
		return GenerateHotFixMessage(value);
	}

	public static HotFixMessage? GenerateItemEffectUpdateIfNeeded(ItemTemplate item, byte slot)
	{
		ItemEffect itemEffectByItemId = GetItemEffectByItemId(item.Entry, slot);
		if (itemEffectByItemId != null)
		{
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			if (item.TriggeredSpellIds[slot] > 0)
			{
				ItemSpellsDataStore.TryGetValue((uint)item.TriggeredSpellIds[slot], out var value);
				if (value != null)
				{
					if (itemEffectByItemId.SpellCategoryID != item.TriggeredSpellCategories[slot])
					{
						flag = value.Category != item.TriggeredSpellCategories[slot];
					}
					if (Math.Abs(itemEffectByItemId.CoolDownMSec - item.TriggeredSpellCooldowns[slot]) > 1)
					{
						flag2 = value.RecoveryTime != item.TriggeredSpellCooldowns[slot];
					}
					if (Math.Abs(itemEffectByItemId.CategoryCoolDownMSec - item.TriggeredSpellCategoryCooldowns[slot]) > 1)
					{
						flag3 = value.CategoryRecoveryTime != item.TriggeredSpellCategoryCooldowns[slot];
					}
				}
			}
			if (itemEffectByItemId.TriggerType != item.TriggeredSpellTypes[slot] || itemEffectByItemId.Charges != item.TriggeredSpellCharges[slot] || flag2 || flag3 || flag || itemEffectByItemId.SpellID != item.TriggeredSpellIds[slot])
			{
				if (item.TriggeredSpellIds[slot] > 0)
				{
					Log.Print(LogType.Storage, $"ItemEffect for item #{item.Entry} slot #{slot} needs to be updated.", "GenerateItemEffectUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
					if (itemEffectByItemId.TriggerType != item.TriggeredSpellTypes[slot])
					{
						Log.Print(LogType.Storage, $"TriggerType {itemEffectByItemId.TriggerType} vs {item.TriggeredSpellTypes[slot]}", "GenerateItemEffectUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
					}
					if (itemEffectByItemId.Charges != item.TriggeredSpellCharges[slot])
					{
						Log.Print(LogType.Storage, $"Charges {itemEffectByItemId.Charges} vs {item.TriggeredSpellCharges[slot]}", "GenerateItemEffectUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
					}
					if (flag2)
					{
						Log.Print(LogType.Storage, $"CoolDownMSec {itemEffectByItemId.CoolDownMSec} vs {item.TriggeredSpellCooldowns[slot]}", "GenerateItemEffectUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
					}
					if (flag3)
					{
						Log.Print(LogType.Storage, $"CategoryCoolDownMSec {itemEffectByItemId.CategoryCoolDownMSec} vs {item.TriggeredSpellCategoryCooldowns[slot]}", "GenerateItemEffectUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
					}
					if (flag)
					{
						Log.Print(LogType.Storage, $"SpellCategoryId {itemEffectByItemId.SpellCategoryID} vs {item.TriggeredSpellCategories[slot]}", "GenerateItemEffectUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
					}
					if (itemEffectByItemId.SpellID != item.TriggeredSpellIds[slot])
					{
						Log.Print(LogType.Storage, $"SpellId {itemEffectByItemId.SpellID} vs {item.TriggeredSpellIds[slot]}", "GenerateItemEffectUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
					}
					itemEffectByItemId.TriggerType = (sbyte)item.TriggeredSpellTypes[slot];
					itemEffectByItemId.Charges = (short)item.TriggeredSpellCharges[slot];
					itemEffectByItemId.CoolDownMSec = (flag2 ? item.TriggeredSpellCooldowns[slot] : (-1));
					itemEffectByItemId.CategoryCoolDownMSec = (flag3 ? item.TriggeredSpellCategoryCooldowns[slot] : (-1));
					itemEffectByItemId.SpellCategoryID = (ushort)(flag ? ((ushort)item.TriggeredSpellCategories[slot]) : 0);
					itemEffectByItemId.SpellID = item.TriggeredSpellIds[slot];
					UpdateItemEffectRecord(itemEffectByItemId, item);
					UpdateHotfix(itemEffectByItemId);
					return GenerateHotFixMessage(itemEffectByItemId);
				}
				RemoveItemEffectRecord(itemEffectByItemId);
				UpdateHotfix(itemEffectByItemId, remove: true);
				return GenerateHotFixMessage(itemEffectByItemId, remove: true);
			}
		}
		else if (item.TriggeredSpellIds[slot] > 0)
		{
			itemEffectByItemId = AddItemEffectRecord(item, slot);
			if (itemEffectByItemId == null)
			{
				return null;
			}
			UpdateHotfix(itemEffectByItemId);
			return GenerateHotFixMessage(itemEffectByItemId);
		}
		return null;
	}

	public static HotFixMessage? GenerateItemAppearanceUpdateIfNeeded(ItemTemplate item)
	{
		ItemAppearance itemAppearanceByDisplayId = GetItemAppearanceByDisplayId(item.DisplayID);
		if (itemAppearanceByDisplayId == null)
		{
			itemAppearanceByDisplayId = AddItemAppearanceRecord(item);
			if (itemAppearanceByDisplayId == null)
			{
				return null;
			}
			UpdateHotfix(itemAppearanceByDisplayId);
			return GenerateHotFixMessage(itemAppearanceByDisplayId);
		}
		return null;
	}

	public static HotFixMessage? GenerateItemModifiedAppearanceUpdateIfNeeded(ItemTemplate item)
	{
		ItemModifiedAppearance itemModifiedAppearanceByItemId = GetItemModifiedAppearanceByItemId(item.Entry);
		if (itemModifiedAppearanceByItemId != null)
		{
			ItemAppearanceStore.TryGetValue((uint)itemModifiedAppearanceByItemId.ItemAppearanceID, out var value);
			if (value == null || value.ItemDisplayInfoID != item.DisplayID)
			{
				Log.Print(LogType.Storage, $"ItemModifiedAppearance #{itemModifiedAppearanceByItemId.Id} for item #{item.Entry} needs to be updated.", "GenerateItemModifiedAppearanceUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				if (value == null)
				{
					Log.Print(LogType.Storage, $"ItemAppearance #{itemModifiedAppearanceByItemId.ItemAppearanceID} missing.", "GenerateItemModifiedAppearanceUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				else if (value.ItemDisplayInfoID != item.DisplayID)
				{
					Log.Print(LogType.Storage, $"DisplayID {value.ItemDisplayInfoID} vs {item.DisplayID}", "GenerateItemModifiedAppearanceUpdateIfNeeded", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				}
				UpdateItemModifiedAppearanceRecord(itemModifiedAppearanceByItemId, item);
				UpdateHotfix(itemModifiedAppearanceByItemId);
				return GenerateHotFixMessage(itemModifiedAppearanceByItemId);
			}
			return null;
		}
		itemModifiedAppearanceByItemId = AddItemModifiedAppearanceRecord(item);
		if (itemModifiedAppearanceByItemId == null)
		{
			return null;
		}
		UpdateHotfix(itemModifiedAppearanceByItemId);
		return GenerateHotFixMessage(itemModifiedAppearanceByItemId);
	}

	public static HotFixMessage? GenerateHotFixMessage(object obj, bool remove = false)
	{
		HotFixMessage hotFixMessage = new HotFixMessage();
		if (obj == null)
		{
			Log.Print(LogType.Error, "DBReply for NULL object requested!", "GenerateHotFixMessage", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
			return null;
		}
		Type type = obj.GetType();
		if (obj is ItemRecord)
		{
			List<HotfixRecord> list = FindHotfixesByRecordIdAndTable((uint)((ItemRecord)obj).Id, DB2Hash.Item);
			hotFixMessage.Hotfixes.AddRange(list);
		}
		else if (obj is ItemSparseRecord)
		{
			List<HotfixRecord> list2 = FindHotfixesByRecordIdAndTable((uint)((ItemSparseRecord)obj).Id, DB2Hash.ItemSparse);
			hotFixMessage.Hotfixes.AddRange(list2);
		}
		else if (obj is ItemEffect)
		{
			List<HotfixRecord> list3 = FindHotfixesByRecordIdAndTable((uint)((ItemEffect)obj).Id, DB2Hash.ItemEffect);
			hotFixMessage.Hotfixes.AddRange(list3);
		}
		else if (obj is ItemAppearance)
		{
			List<HotfixRecord> list4 = FindHotfixesByRecordIdAndTable((uint)((ItemAppearance)obj).Id, DB2Hash.ItemAppearance);
			hotFixMessage.Hotfixes.AddRange(list4);
		}
		else
		{
			if (!(obj is ItemModifiedAppearance))
			{
				Log.Print(LogType.Error, $"Unsupported DBReply requested! ({type})", "GenerateHotFixMessage", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
				return null;
			}
			List<HotfixRecord> list5 = FindHotfixesByRecordIdAndTable((uint)((ItemModifiedAppearance)obj).Id, DB2Hash.ItemModifiedAppearance);
			hotFixMessage.Hotfixes.AddRange(list5);
		}
		return hotFixMessage;
	}

	public static ItemRecord AddItemRecord(ItemTemplate item)
	{
		ItemRecord itemRecord = new ItemRecord();
		itemRecord.Id = (int)item.Entry;
		UpdateItemRecord(itemRecord, item);
		ItemRecordsStore.Add((uint)itemRecord.Id, itemRecord);
		Log.Print(LogType.Storage, $"Item #{itemRecord.Id} created.", "AddItemRecord", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
		return itemRecord;
	}

	public static void UpdateItemRecord(ItemRecord row, ItemTemplate item)
	{
		row.ClassId = (byte)item.Class;
		row.SubclassId = (byte)item.SubClass;
		row.Material = (byte)item.Material;
		row.InventoryType = (sbyte)item.InventoryType;
		row.RequiredLevel = (int)item.RequiredLevel;
		row.SheatheType = (byte)item.SheathType;
		row.RandomProperty = (ushort)item.RandomProperty;
		row.ItemRandomSuffixGroupId = (ushort)item.RandomSuffix;
		row.SoundOverrideSubclassId = -1;
		row.ScalingStatDistributionId = 0;
		row.IconFileDataId = (int)GetItemIconFileDataIdByDisplayId(item.DisplayID);
		row.ItemGroupSoundsId = 0;
		row.ContentTuningId = 0;
		row.MaxDurability = item.MaxDurability;
		row.AmmoType = (byte)item.AmmoType;
		row.DamageType[0] = (byte)item.DamageTypes[0];
		row.DamageType[1] = (byte)item.DamageTypes[1];
		row.DamageType[2] = (byte)item.DamageTypes[2];
		row.DamageType[3] = (byte)item.DamageTypes[3];
		row.DamageType[4] = (byte)item.DamageTypes[4];
		row.Resistances[0] = (short)item.Armor;
		row.Resistances[1] = (short)item.HolyResistance;
		row.Resistances[2] = (short)item.FireResistance;
		row.Resistances[3] = (short)item.NatureResistance;
		row.Resistances[4] = (short)item.FrostResistance;
		row.Resistances[5] = (short)item.ShadowResistance;
		row.Resistances[6] = (short)item.ArcaneResistance;
		row.MinDamage[0] = (ushort)item.DamageMins[0];
		row.MinDamage[1] = (ushort)item.DamageMins[1];
		row.MinDamage[2] = (ushort)item.DamageMins[2];
		row.MinDamage[3] = (ushort)item.DamageMins[3];
		row.MinDamage[4] = (ushort)item.DamageMins[4];
		row.MaxDamage[0] = (ushort)item.DamageMaxs[0];
		row.MaxDamage[1] = (ushort)item.DamageMaxs[1];
		row.MaxDamage[2] = (ushort)item.DamageMaxs[2];
		row.MaxDamage[3] = (ushort)item.DamageMaxs[3];
		row.MaxDamage[4] = (ushort)item.DamageMaxs[4];
		if (ItemRecordsStore.ContainsKey(item.Entry))
		{
			ItemRecordsStore[item.Entry] = row;
		}
	}

	public static ItemSparseRecord AddItemSparseRecord(ItemTemplate item)
	{
		ItemSparseRecord itemSparseRecord = new ItemSparseRecord();
		itemSparseRecord.Id = (int)item.Entry;
		UpdateItemSparseRecord(itemSparseRecord, item);
		ItemSparseRecordsStore.Add((uint)itemSparseRecord.Id, itemSparseRecord);
		Log.Print(LogType.Storage, $"ItemSparse #{itemSparseRecord.Id} created.", "AddItemSparseRecord", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
		return itemSparseRecord;
	}

	public static void UpdateItemSparseRecord(ItemSparseRecord row, ItemTemplate item)
	{
		int[] array = new int[10];
		for (int i = 0; i < item.StatsCount; i++)
		{
			array[i] = item.StatValues[i];
			if (array[i] > 127)
			{
				array[i] = 127;
			}
			if (array[i] < -127)
			{
				array[i] = -127;
			}
		}
		row.AllowableRace = item.AllowedRaces;
		row.Description = item.Description;
		row.Name4 = item.Name[3];
		row.Name3 = item.Name[2];
		row.Name2 = item.Name[1];
		row.Name1 = item.Name[0];
		row.DurationInInventory = item.Duration;
		row.BagFamily = item.BagFamily;
		row.RangeMod = item.RangedMod;
		row.Stackable = item.MaxStackSize;
		row.MaxCount = item.MaxCount;
		row.RequiredAbility = item.RequiredSpell;
		row.SellPrice = item.SellPrice;
		row.BuyPrice = item.BuyPrice;
		row.Flags[0] = item.Flags;
		row.Flags[1] = item.FlagsExtra;
		row.MaxDurability = item.MaxDurability;
		row.RequiredHoliday = (ushort)item.HolidayID;
		row.LimitCategory = (ushort)item.ItemLimitCategory;
		row.GemProperties = (ushort)item.GemProperties;
		row.SocketMatchEnchantmentId = (ushort)item.SocketBonus;
		row.TotemCategoryId = (ushort)item.TotemCategory;
		row.InstanceBound = (ushort)item.MapID;
		row.ZoneBound[0] = (ushort)item.AreaID;
		row.ItemSet = (ushort)item.ItemSet;
		row.LockId = (ushort)item.LockId;
		row.StartQuestId = (ushort)item.StartQuestId;
		row.PageText = (ushort)item.PageText;
		row.Delay = (ushort)item.Delay;
		row.RequiredReputationId = (ushort)item.RequiredRepFaction;
		row.RequiredSkillRank = (ushort)item.RequiredSkillLevel;
		row.RequiredSkill = (ushort)item.RequiredSkillId;
		row.ItemLevel = (ushort)item.ItemLevel;
		row.AllowableClass = (short)item.AllowedClasses;
		row.ItemRandomSuffixGroupId = (ushort)item.RandomSuffix;
		row.RandomProperty = (ushort)item.RandomProperty;
		row.MinDamage[0] = (ushort)item.DamageMins[0];
		row.MinDamage[1] = (ushort)item.DamageMins[1];
		row.MinDamage[2] = (ushort)item.DamageMins[2];
		row.MinDamage[3] = (ushort)item.DamageMins[3];
		row.MinDamage[4] = (ushort)item.DamageMins[4];
		row.MaxDamage[0] = (ushort)item.DamageMaxs[0];
		row.MaxDamage[1] = (ushort)item.DamageMaxs[1];
		row.MaxDamage[2] = (ushort)item.DamageMaxs[2];
		row.MaxDamage[3] = (ushort)item.DamageMaxs[3];
		row.MaxDamage[4] = (ushort)item.DamageMaxs[4];
		row.Resistances[0] = (short)item.Armor;
		row.Resistances[1] = (short)item.HolyResistance;
		row.Resistances[2] = (short)item.FireResistance;
		row.Resistances[3] = (short)item.NatureResistance;
		row.Resistances[4] = (short)item.FrostResistance;
		row.Resistances[5] = (short)item.ShadowResistance;
		row.Resistances[6] = (short)item.ArcaneResistance;
		row.ScalingStatDistributionId = (ushort)item.ScalingStatDistribution;
		row.SocketType[0] = ModernVersion.ConvertSocketColor((byte)item.ItemSocketColors[0]);
		row.SocketType[1] = ModernVersion.ConvertSocketColor((byte)item.ItemSocketColors[1]);
		row.SocketType[2] = ModernVersion.ConvertSocketColor((byte)item.ItemSocketColors[2]);
		row.SheatheType = (byte)item.SheathType;
		row.Material = (byte)item.Material;
		row.PageMaterial = (byte)item.PageMaterial;
		row.PageLanguage = (byte)item.Language;
		row.Bonding = (byte)item.Bonding;
		row.DamageType = (byte)item.DamageTypes[0];
		row.StatType[0] = (sbyte)item.StatTypes[0];
		row.StatType[1] = (sbyte)item.StatTypes[1];
		row.StatType[2] = (sbyte)item.StatTypes[2];
		row.StatType[3] = (sbyte)item.StatTypes[3];
		row.StatType[4] = (sbyte)item.StatTypes[4];
		row.StatType[5] = (sbyte)item.StatTypes[5];
		row.StatType[6] = (sbyte)item.StatTypes[6];
		row.StatType[7] = (sbyte)item.StatTypes[7];
		row.StatType[8] = (sbyte)item.StatTypes[8];
		row.StatType[9] = (sbyte)item.StatTypes[9];
		row.ContainerSlots = (byte)item.ContainerSlots;
		row.RequiredReputationRank = (byte)item.RequiredRepValue;
		row.RequiredCityRank = (byte)item.RequiredCityRank;
		row.RequiredHonorRank = (byte)item.RequiredHonorRank;
		row.InventoryType = (byte)item.InventoryType;
		row.OverallQualityId = (byte)item.Quality;
		row.AmmoType = (byte)item.AmmoType;
		row.StatValue[0] = (sbyte)array[0];
		row.StatValue[1] = (sbyte)array[1];
		row.StatValue[2] = (sbyte)array[2];
		row.StatValue[3] = (sbyte)array[3];
		row.StatValue[4] = (sbyte)array[4];
		row.StatValue[5] = (sbyte)array[5];
		row.StatValue[6] = (sbyte)array[6];
		row.StatValue[7] = (sbyte)array[7];
		row.StatValue[8] = (sbyte)array[8];
		row.StatValue[9] = (sbyte)array[9];
		row.RequiredLevel = (sbyte)item.RequiredLevel;
		if (ItemSparseRecordsStore.ContainsKey(item.Entry))
		{
			ItemSparseRecordsStore[item.Entry] = row;
		}
	}

	public static ItemEffect AddItemEffectRecord(ItemTemplate item, byte slot)
	{
		ItemEffect itemEffect = new ItemEffect();
		itemEffect.Id = (int)GetFirstFreeId(ItemEffectStore);
		itemEffect.LegacySlotIndex = slot;
		UpdateItemEffectRecord(itemEffect, item);
		ItemEffectStore.Add((uint)itemEffect.Id, itemEffect);
		Log.Print(LogType.Storage, $"ItemEffect #{itemEffect.Id} created for item #{item.Entry} slot #{slot}.", "AddItemEffectRecord", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
		return itemEffect;
	}

	public static void UpdateItemEffectRecord(ItemEffect effect, ItemTemplate item)
	{
		byte legacySlotIndex = effect.LegacySlotIndex;
		effect.TriggerType = (sbyte)item.TriggeredSpellTypes[legacySlotIndex];
		effect.Charges = (short)item.TriggeredSpellCharges[legacySlotIndex];
		effect.CoolDownMSec = item.TriggeredSpellCooldowns[legacySlotIndex];
		effect.CategoryCoolDownMSec = item.TriggeredSpellCategoryCooldowns[legacySlotIndex];
		effect.SpellCategoryID = (ushort)item.TriggeredSpellCategories[legacySlotIndex];
		effect.SpellID = item.TriggeredSpellIds[legacySlotIndex];
		effect.ChrSpecializationID = 0;
		effect.ParentItemID = (int)item.Entry;
		if (ItemEffectStore.ContainsKey((uint)effect.Id))
		{
			ItemEffectStore[(uint)effect.Id] = effect;
		}
	}

	public static void RemoveItemEffectRecord(ItemEffect effect)
	{
		ItemEffectStore.Remove((uint)effect.Id);
		Log.Print(LogType.Storage, $"ItemEffect #{effect.Id} removed for item #{effect.ParentItemID} slot #{effect.LegacySlotIndex}.", "RemoveItemEffectRecord", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
	}

	public static ItemAppearance AddItemAppearanceRecord(ItemTemplate item)
	{
		ItemAppearance itemAppearance = new ItemAppearance();
		itemAppearance.Id = (int)GetFirstFreeId(ItemAppearanceStore);
		UpdateItemAppearanceRecord(itemAppearance, item);
		ItemAppearanceStore.Add((uint)itemAppearance.Id, itemAppearance);
		Log.Print(LogType.Storage, $"ItemAppearance #{itemAppearance.Id} created for DisplayID #{item.DisplayID}.", "AddItemAppearanceRecord", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
		return itemAppearance;
	}

	public static void UpdateItemAppearanceRecord(ItemAppearance appearance, ItemTemplate item)
	{
		int itemIconFileDataIdByDisplayId = (int)GetItemIconFileDataIdByDisplayId(item.DisplayID);
		appearance.DisplayType = 11;
		appearance.ItemDisplayInfoID = (int)item.DisplayID;
		appearance.DefaultIconFileDataID = itemIconFileDataIdByDisplayId;
		appearance.UiOrder = 0;
		if (ItemAppearanceStore.ContainsKey((uint)appearance.Id))
		{
			ItemAppearanceStore[(uint)appearance.Id] = appearance;
		}
	}

	public static ItemModifiedAppearance AddItemModifiedAppearanceRecord(ItemTemplate item)
	{
		ItemModifiedAppearance itemModifiedAppearance = new ItemModifiedAppearance();
		itemModifiedAppearance.Id = (int)GetFirstFreeId(ItemModifiedAppearanceStore);
		UpdateItemModifiedAppearanceRecord(itemModifiedAppearance, item);
		if (itemModifiedAppearance.ItemID != item.Entry)
		{
			Log.Print(LogType.Error, $"ItemModifiedAppearance #{itemModifiedAppearance.Id} create failed for item #{itemModifiedAppearance.ItemID}.", "AddItemModifiedAppearanceRecord", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
			return null;
		}
		ItemModifiedAppearanceStore.Add((uint)itemModifiedAppearance.Id, itemModifiedAppearance);
		Log.Print(LogType.Storage, $"ItemModifiedAppearance #{itemModifiedAppearance.Id} created for item #{itemModifiedAppearance.ItemID}.", "AddItemModifiedAppearanceRecord", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
		return itemModifiedAppearance;
	}

	public static void UpdateItemModifiedAppearanceRecord(ItemModifiedAppearance modAppearance, ItemTemplate item)
	{
		ItemAppearance itemAppearanceByDisplayId = GetItemAppearanceByDisplayId(item.DisplayID);
		if (itemAppearanceByDisplayId == null)
		{
			Log.Print(LogType.Error, $"ItemModifiedAppearance #{modAppearance.Id} update failed: no ItemAppearance for DisplayID #{item.DisplayID}", "UpdateItemModifiedAppearanceRecord", "D:\\a\\HermesProxy\\HermesProxy\\World\\GameData.cs");
			return;
		}
		modAppearance.ItemID = (int)item.Entry;
		modAppearance.ItemAppearanceModifierID = 0;
		modAppearance.ItemAppearanceID = itemAppearanceByDisplayId.Id;
		modAppearance.OrderIndex = 0;
		modAppearance.TransmogSourceTypeEnum = 0;
		if (ItemModifiedAppearanceStore.ContainsKey((uint)modAppearance.Id))
		{
			ItemModifiedAppearanceStore[(uint)modAppearance.Id] = modAppearance;
		}
	}

	public static bool ItemCanHaveModel(ItemTemplate item)
	{
		if (item.Class == 2)
		{
			return true;
		}
		if (item.Class == 4 && item.SubClass != 7 && item.SubClass != 8 && item.SubClass != 9 && item.InventoryType != 0 && item.InventoryType != 2 && item.InventoryType != 11 && item.InventoryType != 12 && item.InventoryType != 18 && item.InventoryType != 28)
		{
			return true;
		}
		if (item.Class == 11 && item.SubClass == 2)
		{
			return true;
		}
		return false;
	}

	public static void LoadCreatureDisplayInfoHotfixes()
	{
		using TextFieldParser textFieldParser = new TextFieldParser(Path.Combine("CSV", "Hotfix", $"CreatureDisplayInfo{ModernVersion.ExpansionVersion}.csv"));
		textFieldParser.CommentTokens = new string[1] { "#" };
		textFieldParser.SetDelimiters(",");
		textFieldParser.HasFieldsEnclosedInQuotes = false;
		textFieldParser.ReadLine();
		uint num = 0u;
		while (!textFieldParser.EndOfData)
		{
			num++;
			string[] array = textFieldParser.ReadFields();
			uint num2 = uint.Parse(array[0]);
			ushort data = ushort.Parse(array[1]);
			ushort data2 = ushort.Parse(array[2]);
			sbyte data3 = sbyte.Parse(array[3]);
			float data4 = float.Parse(array[4]);
			byte data5 = byte.Parse(array[5]);
			byte data6 = byte.Parse(array[6]);
			int data7 = int.Parse(array[7]);
			ushort data8 = ushort.Parse(array[8]);
			ushort data9 = ushort.Parse(array[9]);
			int data10 = int.Parse(array[10]);
			int data11 = int.Parse(array[11]);
			ushort data12 = ushort.Parse(array[12]);
			ushort data13 = ushort.Parse(array[13]);
			byte data14 = byte.Parse(array[14]);
			int data15 = int.Parse(array[15]);
			float data16 = float.Parse(array[16]);
			float data17 = float.Parse(array[17]);
			sbyte data18 = sbyte.Parse(array[18]);
			int data19 = int.Parse(array[19]);
			int data20 = int.Parse(array[20]);
			sbyte data21 = sbyte.Parse(array[21]);
			int data22 = int.Parse(array[22]);
			sbyte data23 = sbyte.Parse(array[23]);
			int data24 = int.Parse(array[24]);
			int data25 = int.Parse(array[25]);
			int data26 = int.Parse(array[26]);
			HotfixRecord hotfixRecord = new HotfixRecord();
			hotfixRecord.TableHash = DB2Hash.CreatureDisplayInfo;
			hotfixRecord.HotfixId = 270000 + num;
			hotfixRecord.UniqueId = hotfixRecord.HotfixId;
			hotfixRecord.RecordId = num2;
			hotfixRecord.Status = HotfixStatus.Valid;
			hotfixRecord.HotfixContent.WriteUInt32(num2);
			hotfixRecord.HotfixContent.WriteUInt16(data);
			hotfixRecord.HotfixContent.WriteUInt16(data2);
			hotfixRecord.HotfixContent.WriteInt8(data3);
			hotfixRecord.HotfixContent.WriteFloat(data4);
			hotfixRecord.HotfixContent.WriteUInt8(data5);
			hotfixRecord.HotfixContent.WriteUInt8(data6);
			hotfixRecord.HotfixContent.WriteInt32(data7);
			hotfixRecord.HotfixContent.WriteUInt16(data8);
			hotfixRecord.HotfixContent.WriteUInt16(data9);
			hotfixRecord.HotfixContent.WriteInt32(data10);
			hotfixRecord.HotfixContent.WriteInt32(data11);
			hotfixRecord.HotfixContent.WriteUInt16(data12);
			hotfixRecord.HotfixContent.WriteUInt16(data13);
			hotfixRecord.HotfixContent.WriteUInt8(data14);
			hotfixRecord.HotfixContent.WriteInt32(data15);
			hotfixRecord.HotfixContent.WriteFloat(data16);
			hotfixRecord.HotfixContent.WriteFloat(data17);
			hotfixRecord.HotfixContent.WriteInt8(data18);
			hotfixRecord.HotfixContent.WriteInt32(data19);
			hotfixRecord.HotfixContent.WriteInt32(data20);
			hotfixRecord.HotfixContent.WriteInt8(data21);
			hotfixRecord.HotfixContent.WriteInt32(data22);
			hotfixRecord.HotfixContent.WriteInt8(data23);
			hotfixRecord.HotfixContent.WriteInt32(data24);
			hotfixRecord.HotfixContent.WriteInt32(data25);
			hotfixRecord.HotfixContent.WriteInt32(data26);
			Hotfixes.Add(hotfixRecord.HotfixId, hotfixRecord);
		}
	}

	public static void LoadCreatureDisplayInfoExtraHotfixes()
	{
		using TextFieldParser textFieldParser = new TextFieldParser(Path.Combine("CSV", "Hotfix", $"CreatureDisplayInfoExtra{ModernVersion.ExpansionVersion}.csv"));
		textFieldParser.CommentTokens = new string[1] { "#" };
		textFieldParser.SetDelimiters(",");
		textFieldParser.HasFieldsEnclosedInQuotes = false;
		textFieldParser.ReadLine();
		uint num = 0u;
		while (!textFieldParser.EndOfData)
		{
			num++;
			string[] array = textFieldParser.ReadFields();
			uint num2 = uint.Parse(array[0]);
			sbyte data = sbyte.Parse(array[1]);
			sbyte data2 = sbyte.Parse(array[2]);
			sbyte data3 = sbyte.Parse(array[3]);
			sbyte data4 = sbyte.Parse(array[4]);
			sbyte data5 = sbyte.Parse(array[5]);
			sbyte data6 = sbyte.Parse(array[6]);
			sbyte data7 = sbyte.Parse(array[7]);
			sbyte data8 = sbyte.Parse(array[8]);
			sbyte data9 = sbyte.Parse(array[9]);
			int data10 = int.Parse(array[10]);
			int data11 = int.Parse(array[11]);
			byte data12 = byte.Parse(array[12]);
			byte data13 = byte.Parse(array[13]);
			byte data14 = byte.Parse(array[14]);
			HotfixRecord hotfixRecord = new HotfixRecord();
			hotfixRecord.TableHash = DB2Hash.CreatureDisplayInfoExtra;
			hotfixRecord.HotfixId = 280000 + num;
			hotfixRecord.UniqueId = hotfixRecord.HotfixId;
			hotfixRecord.RecordId = num2;
			hotfixRecord.Status = HotfixStatus.Valid;
			hotfixRecord.HotfixContent.WriteUInt32(num2);
			hotfixRecord.HotfixContent.WriteInt8(data);
			hotfixRecord.HotfixContent.WriteInt8(data2);
			hotfixRecord.HotfixContent.WriteInt8(data3);
			hotfixRecord.HotfixContent.WriteInt8(data4);
			hotfixRecord.HotfixContent.WriteInt8(data5);
			hotfixRecord.HotfixContent.WriteInt8(data6);
			hotfixRecord.HotfixContent.WriteInt8(data7);
			hotfixRecord.HotfixContent.WriteInt8(data8);
			hotfixRecord.HotfixContent.WriteInt8(data9);
			hotfixRecord.HotfixContent.WriteInt32(data10);
			hotfixRecord.HotfixContent.WriteInt32(data11);
			hotfixRecord.HotfixContent.WriteUInt8(data12);
			hotfixRecord.HotfixContent.WriteUInt8(data13);
			hotfixRecord.HotfixContent.WriteUInt8(data14);
			Hotfixes.Add(hotfixRecord.HotfixId, hotfixRecord);
		}
	}

	public static void LoadCreatureDisplayInfoOptionHotfixes()
	{
		using TextFieldParser textFieldParser = new TextFieldParser(Path.Combine("CSV", "Hotfix", $"CreatureDisplayInfoOption{ModernVersion.ExpansionVersion}.csv"));
		textFieldParser.CommentTokens = new string[1] { "#" };
		textFieldParser.SetDelimiters(",");
		textFieldParser.HasFieldsEnclosedInQuotes = false;
		textFieldParser.ReadLine();
		uint num = 0u;
		while (!textFieldParser.EndOfData)
		{
			num++;
			string[] array = textFieldParser.ReadFields();
			uint recordId = uint.Parse(array[0]);
			int data = int.Parse(array[1]);
			int data2 = int.Parse(array[2]);
			int data3 = int.Parse(array[3]);
			HotfixRecord hotfixRecord = new HotfixRecord();
			hotfixRecord.Status = HotfixStatus.Valid;
			hotfixRecord.TableHash = DB2Hash.CreatureDisplayInfoOption;
			hotfixRecord.HotfixId = 290000 + num;
			hotfixRecord.UniqueId = hotfixRecord.HotfixId;
			hotfixRecord.RecordId = recordId;
			hotfixRecord.HotfixContent.WriteInt32(data);
			hotfixRecord.HotfixContent.WriteInt32(data2);
			hotfixRecord.HotfixContent.WriteInt32(data3);
			Hotfixes.Add(hotfixRecord.HotfixId, hotfixRecord);
		}
	}

	public static void LoadItemEffectHotfixes()
	{
		using TextFieldParser textFieldParser = new TextFieldParser(Path.Combine("CSV", "Hotfix", $"ItemEffect{ModernVersion.ExpansionVersion}.csv"));
		textFieldParser.CommentTokens = new string[1] { "#" };
		textFieldParser.SetDelimiters(",");
		textFieldParser.HasFieldsEnclosedInQuotes = false;
		textFieldParser.ReadLine();
		uint num = 0u;
		while (!textFieldParser.EndOfData)
		{
			num++;
			string[] array = textFieldParser.ReadFields();
			uint recordId = uint.Parse(array[0]);
			byte data = byte.Parse(array[1]);
			byte data2 = byte.Parse(array[2]);
			short data3 = short.Parse(array[3]);
			int data4 = int.Parse(array[4]);
			int data5 = int.Parse(array[5]);
			short data6 = short.Parse(array[6]);
			int data7 = int.Parse(array[7]);
			short data8 = short.Parse(array[8]);
			int data9 = int.Parse(array[9]);
			HotfixRecord hotfixRecord = new HotfixRecord();
			hotfixRecord.Status = HotfixStatus.Valid;
			hotfixRecord.TableHash = DB2Hash.ItemEffect;
			hotfixRecord.HotfixId = 250000 + num;
			hotfixRecord.UniqueId = hotfixRecord.HotfixId;
			hotfixRecord.RecordId = recordId;
			hotfixRecord.HotfixContent.WriteUInt8(data);
			hotfixRecord.HotfixContent.WriteUInt8(data2);
			hotfixRecord.HotfixContent.WriteInt16(data3);
			hotfixRecord.HotfixContent.WriteInt32(data4);
			hotfixRecord.HotfixContent.WriteInt32(data5);
			hotfixRecord.HotfixContent.WriteInt16(data6);
			hotfixRecord.HotfixContent.WriteInt32(data7);
			hotfixRecord.HotfixContent.WriteInt16(data8);
			hotfixRecord.HotfixContent.WriteInt32(data9);
			Hotfixes.Add(hotfixRecord.HotfixId, hotfixRecord);
		}
	}

	public static void LoadItemDisplayInfoHotfixes()
	{
		using TextFieldParser textFieldParser = new TextFieldParser(Path.Combine("CSV", "Hotfix", $"ItemDisplayInfo{ModernVersion.ExpansionVersion}.csv"));
		textFieldParser.CommentTokens = new string[1] { "#" };
		textFieldParser.SetDelimiters(",");
		textFieldParser.HasFieldsEnclosedInQuotes = false;
		textFieldParser.ReadLine();
		uint num = 0u;
		while (!textFieldParser.EndOfData)
		{
			num++;
			string[] array = textFieldParser.ReadFields();
			uint recordId = uint.Parse(array[0]);
			int data = int.Parse(array[1]);
			int data2 = int.Parse(array[2]);
			uint data3 = uint.Parse(array[3]);
			uint data4 = uint.Parse(array[4]);
			int data5 = int.Parse(array[5]);
			int data6 = int.Parse(array[6]);
			int data7 = int.Parse(array[7]);
			uint data8 = uint.Parse(array[8]);
			int data9 = int.Parse(array[9]);
			uint data10 = uint.Parse(array[10]);
			uint data11 = uint.Parse(array[11]);
			int data12 = int.Parse(array[12]);
			int data13 = int.Parse(array[13]);
			int data14 = int.Parse(array[14]);
			int data15 = int.Parse(array[15]);
			int data16 = int.Parse(array[16]);
			int data17 = int.Parse(array[17]);
			int data18 = int.Parse(array[18]);
			int data19 = int.Parse(array[19]);
			int data20 = int.Parse(array[20]);
			int data21 = int.Parse(array[21]);
			int data22 = int.Parse(array[22]);
			int data23 = int.Parse(array[23]);
			int data24 = int.Parse(array[24]);
			int data25 = int.Parse(array[25]);
			int data26 = int.Parse(array[26]);
			int data27 = int.Parse(array[27]);
			int data28 = int.Parse(array[28]);
			int data29 = int.Parse(array[29]);
			HotfixRecord hotfixRecord = new HotfixRecord();
			hotfixRecord.Status = HotfixStatus.Valid;
			hotfixRecord.TableHash = DB2Hash.ItemDisplayInfo;
			hotfixRecord.HotfixId = 260000 + num;
			hotfixRecord.UniqueId = hotfixRecord.HotfixId;
			hotfixRecord.RecordId = recordId;
			hotfixRecord.HotfixContent.WriteInt32(data);
			hotfixRecord.HotfixContent.WriteInt32(data2);
			hotfixRecord.HotfixContent.WriteUInt32(data3);
			hotfixRecord.HotfixContent.WriteUInt32(data4);
			hotfixRecord.HotfixContent.WriteInt32(data5);
			hotfixRecord.HotfixContent.WriteInt32(data6);
			hotfixRecord.HotfixContent.WriteInt32(data7);
			hotfixRecord.HotfixContent.WriteUInt32(data8);
			hotfixRecord.HotfixContent.WriteInt32(data9);
			hotfixRecord.HotfixContent.WriteUInt32(data10);
			hotfixRecord.HotfixContent.WriteUInt32(data11);
			hotfixRecord.HotfixContent.WriteInt32(data12);
			hotfixRecord.HotfixContent.WriteInt32(data13);
			hotfixRecord.HotfixContent.WriteInt32(data14);
			hotfixRecord.HotfixContent.WriteInt32(data15);
			hotfixRecord.HotfixContent.WriteInt32(data16);
			hotfixRecord.HotfixContent.WriteInt32(data17);
			hotfixRecord.HotfixContent.WriteInt32(data18);
			hotfixRecord.HotfixContent.WriteInt32(data19);
			hotfixRecord.HotfixContent.WriteInt32(data20);
			hotfixRecord.HotfixContent.WriteInt32(data21);
			hotfixRecord.HotfixContent.WriteInt32(data22);
			hotfixRecord.HotfixContent.WriteInt32(data23);
			hotfixRecord.HotfixContent.WriteInt32(data24);
			hotfixRecord.HotfixContent.WriteInt32(data25);
			hotfixRecord.HotfixContent.WriteInt32(data26);
			hotfixRecord.HotfixContent.WriteInt32(data27);
			hotfixRecord.HotfixContent.WriteInt32(data28);
			hotfixRecord.HotfixContent.WriteInt32(data29);
			Hotfixes.Add(hotfixRecord.HotfixId, hotfixRecord);
		}
	}
}
