using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Text;
using Bgs.Protocol;
using Bgs.Protocol.Account.V1;
using Bgs.Protocol.Authentication.V1;
using Bgs.Protocol.Challenge.V1;
using Bgs.Protocol.Connection.V1;
using Bgs.Protocol.GameUtilities.V1;
using BNetServer.Networking;
using Framework.Constants;
using Framework.Logging;
using Framework.Serialization;
using Framework.Util;
using Framework.Web;
using Google.Protobuf;
using HermesProxy;

namespace BNetServer.Services;

public class BnetServices
{
	public class BnetServiceHandlerInfo
	{
		public readonly ServiceRequirement Requirement;

		public readonly Delegate MethodCaller;

		public readonly Type RequestType;

		public readonly Type ResponseType;

		public BnetServiceHandlerInfo(ServiceRequirement requirement, MethodInfo info, ParameterInfo[] parameters)
		{
			Requirement = requirement;
			RequestType = parameters[0].ParameterType;
			if (parameters.Length > 1)
			{
				ResponseType = parameters[1].ParameterType;
			}
			MethodCaller = info.CreateDelegate((ResponseType != null) ? Expression.GetDelegateType(typeof(BnetServices), RequestType, ResponseType, info.ReturnType) : Expression.GetDelegateType(typeof(BnetServices), RequestType, info.ReturnType));
		}
	}

	public interface INetwork
	{
		void SendRpcMessage(uint serviceId, OriginalHash service, uint methodId, uint token, BattlenetRpcErrorCode status, IMessage? message);

		void CloseSocket();

		IPEndPoint GetRemoteIpEndPoint();
	}

	public class ServiceManager
	{
		private static readonly ConcurrentDictionary<(OriginalHash Service, uint MethodId), BnetServiceHandlerInfo> _serviceHandlers;

		private readonly BnetServices _serviceHolder;

		static ServiceManager()
		{
			_serviceHandlers = new ConcurrentDictionary<(OriginalHash, uint), BnetServiceHandlerInfo>();
			Type[] types = Assembly.GetExecutingAssembly().GetTypes();
			for (int i = 0; i < types.Length; i++)
			{
				MethodInfo[] methods = types[i].GetMethods(BindingFlags.Instance | BindingFlags.NonPublic);
				foreach (MethodInfo methodInfo in methods)
				{
					foreach (ServiceAttribute customAttribute in methodInfo.GetCustomAttributes<ServiceAttribute>())
					{
						if (customAttribute == null)
						{
							continue;
						}
						(OriginalHash, uint) tuple = (customAttribute.ServiceHash, customAttribute.MethodId);
						if (_serviceHandlers.ContainsKey(tuple))
						{
							Log.Print(LogType.Error, $"Tried to override ServiceHandler: {_serviceHandlers[tuple]} with {methodInfo.Name} (ServiceHash: {customAttribute.ServiceHash} MethodId: {customAttribute.MethodId})", ".cctor", "D:\\a\\HermesProxy\\HermesProxy\\BnetServer\\Services\\BnetServices.ServiceManager.cs");
						}
						else
						{
							ParameterInfo[] parameters = methodInfo.GetParameters();
							if (parameters.Length == 0)
							{
								Log.Print(LogType.Error, "Method: " + methodInfo.Name + " needs atleast one parameter", ".cctor", "D:\\a\\HermesProxy\\HermesProxy\\BnetServer\\Services\\BnetServices.ServiceManager.cs");
							}
							else
							{
								_serviceHandlers[tuple] = new BnetServiceHandlerInfo(customAttribute.Requirement, methodInfo, parameters);
							}
						}
					}
				}
			}
		}

		public ServiceManager(string connectionPath, INetwork net, GlobalSessionData? initialSession)
		{
			_serviceHolder = new BnetServices(connectionPath, net, initialSession);
		}

		public void SetClientSecret(byte[] key)
		{
			for (int i = 0; i < Math.Min(_serviceHolder._clientSecret.Length, key.Length); i++)
			{
				_serviceHolder._clientSecret[i] = key[i];
			}
		}

		public void Invoke(uint serviceId, OriginalHash serviceHash, uint methodId, uint requestToken, CodedInputStream stream)
		{
			if (!_serviceHandlers.TryGetValue((serviceHash, methodId), out var bnetServiceHandlerInfo))
			{
				_serviceHolder.ServiceLog(LogType.Warn, $"Client requested service {serviceHash}/m:{methodId} but this service is not implemented?");
				SendErrorResponse(BattlenetRpcErrorCode.RpcNotImplemented);
				return;
			}
			if (bnetServiceHandlerInfo.Requirement != ServiceRequirement.Always && bnetServiceHandlerInfo.Requirement != _serviceHolder.CurrentMatchingRequirement())
			{
				_serviceHolder.ServiceLog(LogType.Warn, $"Client requested service {serviceHash}/m:{methodId} but with invalid state, required: {bnetServiceHandlerInfo.Requirement} but only has {_serviceHolder.CurrentMatchingRequirement()}!");
				SendErrorResponse(BattlenetRpcErrorCode.Denied);
				return;
			}
			_serviceHolder.ServiceLog(LogType.Debug, $"Client requested service {serviceHash}/m:{methodId}");
			IMessage message2 = (IMessage)Activator.CreateInstance(bnetServiceHandlerInfo.RequestType);
			message2.MergeFrom(stream);
			if (bnetServiceHandlerInfo.ResponseType != null)
			{
				IMessage message3 = (IMessage)Activator.CreateInstance(bnetServiceHandlerInfo.ResponseType);
				BattlenetRpcErrorCode battlenetRpcErrorCode = (BattlenetRpcErrorCode)bnetServiceHandlerInfo.MethodCaller.DynamicInvoke(_serviceHolder, message2, message3);
				if (battlenetRpcErrorCode == BattlenetRpcErrorCode.Ok)
				{
					SendResponse(message3);
				}
				else
				{
					SendErrorResponse(battlenetRpcErrorCode);
				}
			}
			else
			{
				BattlenetRpcErrorCode battlenetRpcErrorCode = (BattlenetRpcErrorCode)bnetServiceHandlerInfo.MethodCaller.DynamicInvoke(_serviceHolder, message2);
				if (battlenetRpcErrorCode != 0)
				{
					SendErrorResponse(battlenetRpcErrorCode);
				}
			}
			void SendErrorResponse(BattlenetRpcErrorCode errorCode)
			{
				SendRpcMessage(errorCode, null);
			}
			void SendResponse(IMessage response)
			{
				SendRpcMessage(BattlenetRpcErrorCode.Ok, response);
			}
			void SendRpcMessage(BattlenetRpcErrorCode status, IMessage? message)
			{
				if (_serviceHolder._connectionPath == "WorldSocket")
				{
					_serviceHolder._net.SendRpcMessage(serviceId, serviceHash, methodId, requestToken, status, message);
				}
				else
				{
					_serviceHolder._net.SendRpcMessage(254u, serviceHash, methodId, requestToken, status, message);
				}
			}
		}
	}

	private static uint _serverInvokedRequestToken;

	private Dictionary<uint, Action<CodedInputStream>> _callbackHandlers = new Dictionary<uint, Action<CodedInputStream>>();

	private GlobalSessionData _globalSession;

	private readonly byte[] _clientSecret = new byte[32];

	private readonly string _connectionPath;

	private readonly INetwork _net;

	public GlobalSessionData Session => _globalSession;

	private BnetServices()
	{
	}

	private BnetServices(string connectionPath, INetwork net, GlobalSessionData? initialSession)
	{
		_connectionPath = connectionPath;
		_net = net;
		_globalSession = initialSession;
	}

	public GlobalSessionData GetSession()
	{
		return _globalSession;
	}

	private void SendRequest(OriginalHash service, uint methodId, IMessage? data)
	{
		_serverInvokedRequestToken++;
		_net.SendRpcMessage(0u, service, methodId, _serverInvokedRequestToken, BattlenetRpcErrorCode.Ok, data);
	}

	private void CloseSocket()
	{
		_net.CloseSocket();
	}

	private IPEndPoint GetRemoteIpEndPoint()
	{
		return _net.GetRemoteIpEndPoint();
	}

	private void ServiceLog(LogType type, string message)
	{
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder3 = stringBuilder2;
		StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(2, 1, stringBuilder2);
		handler.AppendLiteral("[");
		handler.AppendFormatted(_connectionPath);
		handler.AppendLiteral("]");
		stringBuilder3.Append(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder4 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(1, 1, stringBuilder2);
		handler.AppendLiteral("[");
		handler.AppendFormatted(GetRemoteIpEndPoint());
		stringBuilder4.Append(ref handler);
		if (GetSession() != null)
		{
			if (GetSession().AccountInfo != null && !GetSession().AccountInfo.Login.IsEmpty())
			{
				stringBuilder.Append(", Account: " + GetSession().AccountInfo.Login);
			}
			if (GetSession().GameAccountInfo != null)
			{
				stringBuilder.Append(", Game account: " + GetSession().GameAccountInfo.Name);
			}
		}
		stringBuilder.Append(']');
		Log.Print(type, $"{stringBuilder} {message}", "ServiceLog", "D:\\a\\HermesProxy\\HermesProxy\\BnetServer\\Services\\BnetServices.cs");
	}

	public ServiceRequirement CurrentMatchingRequirement()
	{
		if (_globalSession != null)
		{
			return ServiceRequirement.LoggedIn;
		}
		return ServiceRequirement.Unauthorized;
	}

	[Service(ServiceRequirement.LoggedIn, OriginalHash.AccountService, 30u)]
	private BattlenetRpcErrorCode HandleGetAccountState(GetAccountStateRequest request, GetAccountStateResponse response)
	{
		if (request.Options.FieldPrivacyInfo)
		{
			response.State = new AccountState();
			response.State.PrivacyInfo = new PrivacyInfo();
			response.State.PrivacyInfo.IsUsingRid = false;
			response.State.PrivacyInfo.IsVisibleForViewFriends = false;
			response.State.PrivacyInfo.IsHiddenFromFriendFinder = true;
			response.Tags = new AccountFieldTags();
			response.Tags.PrivacyInfoTag = 3620373325u;
		}
		return BattlenetRpcErrorCode.Ok;
	}

	[Service(ServiceRequirement.LoggedIn, OriginalHash.AccountService, 31u)]
	private BattlenetRpcErrorCode HandleGetGameAccountState(GetGameAccountStateRequest request, GetGameAccountStateResponse response)
	{
		if (request.Options.FieldGameLevelInfo)
		{
			GameAccountInfo gameAccountInfo = GetSession().AccountInfo.GameAccounts.LookupByKey(request.GameAccountId.Low);
			if (gameAccountInfo != null)
			{
				response.State = new GameAccountState();
				response.State.GameLevelInfo = new GameLevelInfo();
				response.State.GameLevelInfo.Name = gameAccountInfo.DisplayName;
				response.State.GameLevelInfo.Program = 5730135u;
			}
			response.Tags = new GameAccountFieldTags();
			response.Tags.GameLevelInfoTag = 1548145795u;
		}
		if (request.Options.FieldGameStatus)
		{
			if (response.State == null)
			{
				response.State = new GameAccountState();
			}
			response.State.GameStatus = new GameStatus();
			GameAccountInfo gameAccountInfo2 = GetSession().AccountInfo.GameAccounts.LookupByKey(request.GameAccountId.Low);
			if (gameAccountInfo2 != null)
			{
				response.State.GameStatus.IsSuspended = gameAccountInfo2.IsBanned;
				response.State.GameStatus.IsBanned = gameAccountInfo2.IsPermanenetlyBanned;
				response.State.GameStatus.SuspensionExpires = gameAccountInfo2.UnbanDate * 1000000;
			}
			response.State.GameStatus.Program = 5730135u;
			response.Tags.GameStatusTag = 2562154393u;
		}
		return BattlenetRpcErrorCode.Ok;
	}

	[Service(ServiceRequirement.Unauthorized, OriginalHash.AuthenticationService, 1u)]
	private BattlenetRpcErrorCode HandleLogon(LogonRequest logonRequest, NoData response)
	{
		if (logonRequest.Program != "WoW")
		{
			ServiceLog(LogType.Error, "Battlenet.LogonRequest: Attempted to log in with game other than WoW (using " + logonRequest.Program + ")!");
			return BattlenetRpcErrorCode.BadProgram;
		}
		if (logonRequest.ApplicationVersion != ModernVersion.BuildInt)
		{
			ServiceLog(LogType.Error, $"Battlenet.LogonRequest: Attempted to log in with wrong game version (using {logonRequest.ApplicationVersion})!");
			return BattlenetRpcErrorCode.BadVersion;
		}
		if (logonRequest.Platform != "Win" && logonRequest.Platform != "Wn64" && logonRequest.Platform != "Mc64" && logonRequest.Platform != "MacA")
		{
			ServiceLog(LogType.Error, "Battlenet.LogonRequest: Attempted to log in from an unsupported platform (using " + logonRequest.Platform + ")!");
			return BattlenetRpcErrorCode.BadPlatform;
		}
		if (!LocaleChecker.IsValidLocale(logonRequest.Locale.ToEnum<Locale>()))
		{
			ServiceLog(LogType.Error, "Battlenet.LogonRequest: Attempted to log in with unsupported locale (using " + logonRequest.Locale + ")!");
			return BattlenetRpcErrorCode.BadLocale;
		}
		IPEndPoint addressForClient = Singleton<LoginServiceManager>.Instance.GetAddressForClient(GetRemoteIpEndPoint().Address);
		ChallengeExternalRequest challengeExternalRequest = new ChallengeExternalRequest();
		challengeExternalRequest.PayloadType = "web_auth_url";
		challengeExternalRequest.Payload = ByteString.CopyFromUtf8($"https://{addressForClient.Address}:{addressForClient.Port}/bnetserver/login/{logonRequest.Platform}/{logonRequest.ApplicationVersion}/{logonRequest.Locale}/");
		SendRequest(OriginalHash.ChallengeListener, 3u, challengeExternalRequest);
		return BattlenetRpcErrorCode.Ok;
	}

	[Service(ServiceRequirement.Unauthorized, OriginalHash.AuthenticationService, 7u)]
	private BattlenetRpcErrorCode HandleVerifyWebCredentials(VerifyWebCredentialsRequest verifyWebCredentialsRequest)
	{
		if (!BnetSessionTicketStorage.SessionsByTicket.TryGetValue(verifyWebCredentialsRequest.WebCredentials.ToStringUtf8(), out var value))
		{
			return BattlenetRpcErrorCode.Denied;
		}
		value.AccountInfo = new AccountInfo(value.Username);
		if (value.AccountInfo.LoginTicketExpiry < Time.UnixTime)
		{
			return BattlenetRpcErrorCode.TimedOut;
		}
		if (value.AccountInfo.IsBanned)
		{
			if (value.AccountInfo.IsPermanenetlyBanned)
			{
				ServiceLog(LogType.Debug, "Session.HandleVerifyWebCredentials: Banned account " + value.AccountInfo.Login + " tried to login!");
				return BattlenetRpcErrorCode.GameAccountBanned;
			}
			ServiceLog(LogType.Debug, "Session.HandleVerifyWebCredentials: Temporarily banned account " + value.AccountInfo.Login + " tried to login!");
			return BattlenetRpcErrorCode.GameAccountSuspended;
		}
		Bgs.Protocol.Authentication.V1.LogonResult logonResult = new Bgs.Protocol.Authentication.V1.LogonResult();
		logonResult.ErrorCode = 0u;
		logonResult.AccountId = new EntityId();
		logonResult.AccountId.Low = value.AccountInfo.Id;
		logonResult.AccountId.High = 72057594037927936uL;
		foreach (GameAccountInfo value2 in value.AccountInfo.GameAccounts.Values)
		{
			EntityId entityId = new EntityId();
			entityId.Low = value2.Id;
			entityId.High = 144115196671520593uL;
			logonResult.GameAccountId.Add(entityId);
		}
		value.SessionKey = new byte[64].GenerateRandomKey(64);
		logonResult.SessionKey = ByteString.CopyFrom(value.SessionKey);
		_globalSession = value;
		SendRequest(OriginalHash.AuthenticationListener, 5u, logonResult);
		return BattlenetRpcErrorCode.Ok;
	}

	[Service(ServiceRequirement.Unauthorized, OriginalHash.ConnectionService, 1u)]
	private BattlenetRpcErrorCode HandleConnect(ConnectRequest request, ConnectResponse response)
	{
		if (request.ClientId != null)
		{
			response.ClientId.MergeFrom(request.ClientId);
		}
		response.ServerId = new ProcessId();
		response.ServerId.Label = (uint)Environment.ProcessId;
		response.ServerId.Epoch = (uint)Time.UnixTime;
		response.ServerTime = (ulong)Time.UnixTimeMilliseconds;
		response.UseBindlessRpc = request.UseBindlessRpc;
		return BattlenetRpcErrorCode.Ok;
	}

	[Service(ServiceRequirement.Always, OriginalHash.ConnectionService, 5u)]
	private BattlenetRpcErrorCode HandleKeepAlive(NoData request)
	{
		return BattlenetRpcErrorCode.Ok;
	}

	[Service(ServiceRequirement.Always, OriginalHash.ConnectionService, 7u)]
	private BattlenetRpcErrorCode HandleRequestDisconnect(DisconnectRequest request)
	{
		if (GetSession() != null && GetSession().AuthClient != null)
		{
			GetSession().AuthClient.Disconnect();
		}
		DisconnectNotification disconnectNotification = new DisconnectNotification();
		disconnectNotification.ErrorCode = request.ErrorCode;
		SendRequest(OriginalHash.ConnectionService, 4u, disconnectNotification);
		CloseSocket();
		return BattlenetRpcErrorCode.Ok;
	}

	private string GetCommandEndingForVersion()
	{
		if (ModernVersion.ExpansionVersion == 1)
		{
			return "c1";
		}
		if (ModernVersion.ExpansionVersion == 2)
		{
			return "bcc1";
		}
		return "b9";
	}

	[Service(ServiceRequirement.LoggedIn, OriginalHash.GameUtilitiesService, 1u)]
	private BattlenetRpcErrorCode HandleProcessClientRequest(ClientRequest request, ClientResponse response)
	{
		Bgs.Protocol.Attribute attribute = null;
		Dictionary<string, Variant> dictionary = new Dictionary<string, Variant>();
		for (int i = 0; i < request.Attribute.Count; i++)
		{
			Bgs.Protocol.Attribute attribute2 = request.Attribute[i];
			dictionary[attribute2.Name] = attribute2.Value;
			if (attribute2.Name.Contains("Command_"))
			{
				attribute = attribute2;
			}
		}
		if (attribute == null)
		{
			ServiceLog(LogType.Error, "Sent ClientRequest with no command.");
			return BattlenetRpcErrorCode.RpcMalformedRequest;
		}
		ServiceLog(LogType.Debug, "GameUtilitiesService method: " + attribute.Name);
		if (attribute.Name == "Command_RealmListTicketRequest_v1_" + GetCommandEndingForVersion())
		{
			return GetRealmListTicket(dictionary, response);
		}
		if (attribute.Name == "Command_LastCharPlayedRequest_v1_" + GetCommandEndingForVersion())
		{
			return GetLastCharPlayed(dictionary, response);
		}
		if (attribute.Name == "Command_RealmListRequest_v1_" + GetCommandEndingForVersion())
		{
			return GetRealmList(dictionary, response);
		}
		if (attribute.Name == "Command_RealmJoinRequest_v1_" + GetCommandEndingForVersion())
		{
			return JoinRealm(dictionary, response);
		}
		ServiceLog(LogType.Warn, "Sent unhandled command '" + attribute.Name + "'.");
		return BattlenetRpcErrorCode.RpcNotImplemented;
	}

	[Service(ServiceRequirement.LoggedIn, OriginalHash.GameUtilitiesService, 10u)]
	private BattlenetRpcErrorCode HandleGetAllValuesForAttribute(GetAllValuesForAttributeRequest request, GetAllValuesForAttributeResponse response)
	{
		if (request.AttributeKey == "Command_RealmListRequest_v1_" + GetCommandEndingForVersion())
		{
			GetSession().AuthClient.WaitOrRequestRealmList();
			GetSession().RealmManager.WriteSubRegions(response);
			return BattlenetRpcErrorCode.Ok;
		}
		return BattlenetRpcErrorCode.RpcNotImplemented;
	}

	private BattlenetRpcErrorCode GetRealmListTicket(Dictionary<string, Variant> Params, ClientResponse response)
	{
		Variant variant = Params.LookupByKey("Param_Identity");
		if (variant != null)
		{
			RealmListTicketIdentity realmListTicketIdentity = Json.CreateObject<RealmListTicketIdentity>(variant.BlobValue.ToStringUtf8(), split: true);
			GameAccountInfo gameAccountInfo = GetSession().AccountInfo.GameAccounts.LookupByKey(realmListTicketIdentity.GameAccountId);
			if (gameAccountInfo != null)
			{
				GetSession().GameAccountInfo = gameAccountInfo;
			}
		}
		if (GetSession().GameAccountInfo == null)
		{
			return BattlenetRpcErrorCode.UtilServerInvalidIdentityArgs;
		}
		if (GetSession().GameAccountInfo.IsPermanenetlyBanned)
		{
			return BattlenetRpcErrorCode.GameAccountBanned;
		}
		if (GetSession().GameAccountInfo.IsBanned)
		{
			return BattlenetRpcErrorCode.GameAccountSuspended;
		}
		bool flag = false;
		Variant variant2 = Params.LookupByKey("Param_ClientInfo");
		if (variant2 != null)
		{
			RealmListTicketClientInformation realmListTicketClientInformation = Json.CreateObject<RealmListTicketClientInformation>(variant2.BlobValue.ToStringUtf8(), split: true);
			flag = true;
			for (int i = 0; i < Math.Min(_clientSecret.Length, realmListTicketClientInformation.Info.Secret.Count); i++)
			{
				_clientSecret[i] = (byte)realmListTicketClientInformation.Info.Secret[i];
			}
		}
		if (!flag)
		{
			return BattlenetRpcErrorCode.WowServicesDeniedRealmListTicket;
		}
		response.Attribute.AddBlob("Param_RealmListTicket", ByteString.CopyFrom("AuthRealmListTicket", Encoding.UTF8));
		return BattlenetRpcErrorCode.Ok;
	}

	private BattlenetRpcErrorCode GetLastCharPlayed(Dictionary<string, Variant> Params, ClientResponse response)
	{
		if (Params.LookupByKey("Command_LastCharPlayedRequest_v1_" + GetCommandEndingForVersion()) == null)
		{
			return BattlenetRpcErrorCode.UtilServerUnknownRealm;
		}
		(string, string, ulong, long)? lastSelectedCharacter = GetSession().AccountMetaDataMgr.GetLastSelectedCharacter();
		if (!lastSelectedCharacter.HasValue)
		{
			return BattlenetRpcErrorCode.Ok;
		}
		(string realmName, string charName, ulong charLowerGuid, long lastLoginUnixSec) lastPlayedChar = lastSelectedCharacter.Value;
		GetSession().AuthClient.WaitOrRequestRealmList();
		Realm realm = GetSession().RealmManager.GetRealms().FirstOrDefault((Realm r) => r.Name == lastPlayedChar.realmName && !r.Flags.HasFlag(RealmFlags.Offline));
		if (realm == null)
		{
			return BattlenetRpcErrorCode.UtilServerFailedToSerializeResponse;
		}
		byte[] compressdRealmEntryJSON = GetSession().RealmManager.GetCompressdRealmEntryJSON(realm, GetSession().Build);
		if (compressdRealmEntryJSON.Length == 0)
		{
			return BattlenetRpcErrorCode.UtilServerFailedToSerializeResponse;
		}
		response.Attribute.AddBlob("Param_RealmEntry", ByteString.CopyFrom(compressdRealmEntryJSON));
		response.Attribute.AddString("Param_CharacterName", lastPlayedChar.charName);
		response.Attribute.AddBlob("Param_CharacterGUID", ByteString.CopyFrom(BitConverter.GetBytes(lastPlayedChar.charLowerGuid)));
		response.Attribute.AddInt("Param_LastPlayedTime", lastPlayedChar.lastLoginUnixSec);
		return BattlenetRpcErrorCode.Ok;
	}

	private BattlenetRpcErrorCode GetRealmList(Dictionary<string, Variant> Params, ClientResponse response)
	{
		if (GetSession().GameAccountInfo == null)
		{
			return BattlenetRpcErrorCode.UserServerBadWowAccount;
		}
		if (!GetSession().AuthClient.IsConnected())
		{
			return BattlenetRpcErrorCode.UtilServerMissingRealmList;
		}
		string subRegion = "";
		Variant variant = Params.LookupByKey("Command_RealmListRequest_v1_" + GetCommandEndingForVersion());
		if (variant != null)
		{
			subRegion = variant.StringValue;
		}
		byte[] realmList = GetSession().RealmManager.GetRealmList(GetSession().Build, subRegion);
		if (realmList.Length == 0)
		{
			return BattlenetRpcErrorCode.UtilServerFailedToSerializeResponse;
		}
		response.Attribute.AddBlob("Param_RealmList", ByteString.CopyFrom(realmList));
		RealmCharacterCountList realmCharacterCountList = new RealmCharacterCountList();
		foreach (Realm realm in GetSession().RealmManager.GetRealms())
		{
			RealmCharacterCountEntry realmCharacterCountEntry = new RealmCharacterCountEntry();
			realmCharacterCountEntry.WowRealmAddress = (int)realm.Id.GetAddress();
			realmCharacterCountEntry.Count = realm.CharacterCount;
			realmCharacterCountList.Counts.Add(realmCharacterCountEntry);
		}
		byte[] bytes = Json.Deflate("JSONRealmCharacterCountList", realmCharacterCountList);
		response.Attribute.AddBlob("Param_CharacterCountList", ByteString.CopyFrom(bytes));
		return BattlenetRpcErrorCode.Ok;
	}

	private BattlenetRpcErrorCode JoinRealm(Dictionary<string, Variant> Params, ClientResponse response)
	{
		Variant variant = Params.LookupByKey("Param_RealmAddress");
		if (variant == null)
		{
			return BattlenetRpcErrorCode.WowServicesInvalidJoinTicket;
		}
		return GetSession().RealmManager.JoinRealm(GetSession(), (uint)variant.UintValue, GetSession().Build, GetRemoteIpEndPoint().Address, _clientSecret, GetSession().GameAccountInfo.Name, response);
	}
}
