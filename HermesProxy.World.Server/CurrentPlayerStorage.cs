namespace HermesProxy.World.Server;
/*
 * 当前玩家系统设置和插件设置存储
 */
public class CurrentPlayerStorage
{
	private readonly GlobalSessionData _globalSession;

	public CompletedQuestTracker CompletedQuests { get; private set; }

	public PlayerSettings Settings { get; private set; }

	public CurrentPlayerStorage(GlobalSessionData globalSession)
	{
		_globalSession = globalSession;
	}

	public void LoadCurrentPlayer()
	{
		CompletedQuests = new CompletedQuestTracker(_globalSession);
		Settings = new PlayerSettings(_globalSession);
		CompletedQuests.Reload();
		Settings.Reload();
	}
}
