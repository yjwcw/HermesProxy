using HermesProxy.World.Enums;

namespace HermesProxy.World.Server;

/*
 * 玩家系统设置
 */
public class PlayerSettings
{
	public class InternalStorage
	{
		public bool AutoBlockGuildInvites { get; set; }
	}

	private InternalStorage _internalStorage;

	private PlayerFlags _lastCapturedFlags;

	public bool NeedToForcePatchFlags { get; private set; }

	public GlobalSessionData Session { get; }

	public PlayerSettings(GlobalSessionData globalSession)
	{
		Session = globalSession;
	}

	public void SetAutoBlockGuildInvites(bool value)
	{
		_internalStorage.AutoBlockGuildInvites = value;
		NeedToForcePatchFlags = true;
		Save();
	}

	public void PatchFlags(ref PlayerFlags flags)
	{
		_lastCapturedFlags = flags;
		NeedToForcePatchFlags = false;
		if (_internalStorage.AutoBlockGuildInvites)
		{
			flags |= PlayerFlags.AutoDeclineGuild;
		}
		else
		{
			flags &= ~PlayerFlags.AutoDeclineGuild;
		}
	}

	public PlayerFlags CreateNewFlags()
	{
		PlayerFlags flags = _lastCapturedFlags;
		PatchFlags(ref flags);
		return flags;
	}

	private void Save()
	{
		Session.AccountMetaDataMgr.SaveCharacterSettingsStorage(Session.GameState.CurrentPlayerInfo.Realm.Name, Session.GameState.CurrentPlayerInfo.Name, _internalStorage);
	}

	public void Reload()
	{
		_internalStorage = Session.AccountMetaDataMgr.LoadCharacterSettingsStorage(Session.GameState.CurrentPlayerInfo.Realm.Name, Session.GameState.CurrentPlayerInfo.Name);
	}
}
