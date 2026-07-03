using System.Collections.Generic;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server;

public class CompletedQuestTracker
{
	private Dictionary<int, ulong> _cachedQuestCompleted = new Dictionary<int, ulong>();

	public GlobalSessionData Session { get; }

	public CompletedQuestTracker(GlobalSessionData globalSession)
	{
		Session = globalSession;
	}

	public void MarkQuestAsNotCompleted(uint questQuestId)
	{
		Session.AccountMetaDataMgr.MarkQuestAsNotCompleted(Session.GameState.CurrentPlayerInfo.Realm.Name, Session.GameState.CurrentPlayerInfo.Name, questQuestId);
		uint? uniqueQuestBit = GameData.GetUniqueQuestBit(questQuestId);
		if (uniqueQuestBit.HasValue)
		{
			SendSingleUpdateToClient(uniqueQuestBit.Value, isSet: false);
		}
	}

	public void MarkQuestAsCompleted(uint questQuestId)
	{
		Session.AccountMetaDataMgr.MarkQuestAsCompleted(Session.GameState.CurrentPlayerInfo.Realm.Name, Session.GameState.CurrentPlayerInfo.Name, questQuestId);
		uint? uniqueQuestBit = GameData.GetUniqueQuestBit(questQuestId);
		if (uniqueQuestBit.HasValue)
		{
			SendSingleUpdateToClient(uniqueQuestBit.Value, isSet: true);
		}
	}

	public void Reload()
	{
		List<uint> allCompletedQuests = Session.AccountMetaDataMgr.GetAllCompletedQuests(Session.GameState.CurrentPlayerInfo.Realm.Name, Session.GameState.CurrentPlayerInfo.Name);
		_cachedQuestCompleted = new Dictionary<int, ulong>();
		foreach (uint item in allCompletedQuests)
		{
			uint? uniqueQuestBit = GameData.GetUniqueQuestBit(item);
			if (uniqueQuestBit.HasValue)
			{
				int value = (int)(uniqueQuestBit - 1 >> 6).Value;
				int value2 = (int)((uniqueQuestBit - 1) & 0x3F).Value;
				_cachedQuestCompleted.TryAdd(value, 0uL);
				_cachedQuestCompleted[value] |= (ulong)(1L << value2);
			}
		}
	}

    //向客户端发送单个更新
    private void SendSingleUpdateToClient(uint questBit, bool isSet)
	{
		int num = (int)(questBit - 1 >> 6);
		int num2 = (int)((questBit - 1) & 0x3F);
		_cachedQuestCompleted.TryAdd(num, 0uL);
		if (isSet)
		{
			_cachedQuestCompleted[num] |= (ulong)(1L << num2);
		}
		else
		{
			_cachedQuestCompleted[num] &= (ulong)(~(1L << num2));
		}
		ObjectUpdate objectUpdate = new ObjectUpdate(Session.GameState.CurrentPlayerGuid, UpdateTypeModern.Values, Session);
		objectUpdate.ActivePlayerData.QuestCompleted[num] = _cachedQuestCompleted[num];
		UpdateObject updateObject = new UpdateObject(Session.GameState);
		updateObject.ObjectUpdates.Add(objectUpdate);
		Session.WorldClient.SendPacketToClient(updateObject);
	}

	public void WriteAllCompletedIntoArray(ulong?[] dest)
	{
		foreach (KeyValuePair<int, ulong> item in _cachedQuestCompleted)
		{
			dest[item.Key] = item.Value;
		}
	}
}
