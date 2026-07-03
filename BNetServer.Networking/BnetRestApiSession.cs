using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using Framework;
using Framework.Networking;
using Framework.Serialization;
using Framework.Web;
using HermesProxy;
using HermesProxy.Auth;
using HermesProxy.Enums;
using HermesProxy.World.Server;

namespace BNetServer.Networking;

public class BnetRestApiSession : SSLSocket
{
	private const string BNET_SERVER_BASE_PATH = "/bnetserver/";

	private const string TICKET_PREFIX = "HP-";

	public BnetRestApiSession(Socket socket)
		: base(socket)
	{
	}

	public override void Accept()
	{
		AsyncHandshake(BnetServerCertificate.Certificate);
	}

	public override async Task ReadHandler(byte[] data, int receivedLength)
	{
		HttpHeader httpHeader = HttpHelper.ParseRequest(data, receivedLength);
		if (httpHeader == null || !RequestRouter(httpHeader))
		{
			CloseSocket();
		}
		else
		{
			await AsyncRead(); // 登陆成功后 异步读取数据
        }
	}

	public bool RequestRouter(HttpHeader httpRequest)
	{
		if (!httpRequest.Path.StartsWith("/bnetserver/"))
		{
			SendEmptyResponse(HttpCode.NotFound);
			return false;
		}
		string[] array = httpRequest.Path.Substring("/bnetserver/".Length).Split('/');
		string obj = array[0];
		string method = httpRequest.Method;
		if (obj == "login")
		{
			if (method == "GET")
			{
				SendResponse(HttpCode.Ok, Singleton<LoginServiceManager>.Instance.GetFormInput());
				return true;
			}
			if (method == "POST")
			{
				HandleLoginRequest(array, httpRequest);
				return true;
			}
		}
		SendEmptyResponse(HttpCode.NotFound);
		return false;
	}

	public Task HandleLoginRequest(string[] pathElements, HttpHeader request)
	{
		LogonData logonData = Json.CreateObject<LogonData>(request.Content);
		if (logonData == null)
		{
			return SendEmptyResponse(HttpCode.InternalServerError);
		}
		GlobalSessionData globalSessionData = new GlobalSessionData();
		globalSessionData.OS = pathElements[1];
		globalSessionData.Build = uint.Parse(pathElements[2]);
		globalSessionData.Locale = pathElements[3];
		if (Settings.ClientBuild != (ClientVersionBuild)globalSessionData.Build)
		{
			return SendAuthError(AuthResult.FAIL_WRONG_MODERN_VER);
		}
		string text = "";
		string password = "";
		foreach (FormInputValue input in logonData.Inputs)
		{
			string id = input.Id;
			if (!(id == "account_name"))
			{
				if (id == "password")
				{
					password = input.Value;
				}
			}
			else
			{
				text = input.Value.Trim().ToUpperInvariant();
			}
		}

		//创建 登陆 连接信息
		globalSessionData.AuthClient = new AuthClient(globalSessionData);

		//验证用户名密码
		AuthResult authResult = globalSessionData.AuthClient.ConnectToAuthServer(text, password, globalSessionData.Locale);
		if (authResult != 0)
		{
			return SendAuthError(authResult);
		}
		globalSessionData.AuthClient.SendRealmListUpdateRequest();

		// 创建登陆 Result
		LogonResult logonResult = new LogonResult();
        // 生成随机密钥
        byte[] array = Array.Empty<byte>().GenerateRandomKey(20);
		//登陆票证
		string loginTicket = (globalSessionData.LoginTicket = "HP-" + array.ToHexString());

		globalSessionData.Username = text;
		globalSessionData.AccountMetaDataMgr = new AccountMetaDataManager(text);
		BnetSessionTicketStorage.AddNewSessionByName(text, globalSessionData);
		BnetSessionTicketStorage.AddNewSessionByTicket(loginTicket, globalSessionData); 
		logonResult.LoginTicket = loginTicket;
		logonResult.AuthenticationState = "DONE";

		//发送
		return SendResponse(HttpCode.Ok, logonResult); 
	}

	private async Task SendResponse<T>(HttpCode code, T response)
	{
		Console.WriteLine(Json.CreateString(response));
        await AsyncWrite(HttpHelper.CreateResponse(code, Json.CreateString(response)));
	}

	private async Task SendAuthError(AuthResult response)
	{
		LogonResult logonResult = new LogonResult();
		(logonResult.AuthenticationState, logonResult.ErrorCode, logonResult.ErrorMessage) = response switch
		{
			AuthResult.FAIL_UNKNOWN_ACCOUNT => ("LOGIN", "UNABLE_TO_DECODE", "Invalid username or password."), 
			AuthResult.FAIL_INCORRECT_PASSWORD => ("LOGIN", "UNABLE_TO_DECODE", "Invalid password."), 
			AuthResult.FAIL_BANNED => ("LOGIN", "UNABLE_TO_DECODE", "This account has been closed and is no longer available for use."), 
			AuthResult.FAIL_SUSPENDED => ("LOGIN", "UNABLE_TO_DECODE", "This account has been temporarily suspended."), 
			AuthResult.FAIL_VERSION_INVALID => ("LOGIN", "UNABLE_TO_DECODE", "Your version is not supported by this server.\nMake sure you are using the latest HermesProxy version from GitHub.\n(Maybe HermesProxy is blocked on the server)\n"), 
			AuthResult.FAIL_INTERNAL_ERROR => ("LOGON", "UNABLE_TO_DECODE", "There was an internal error. Please try again later."), 
			_ => ("LOGON", "UNABLE_TO_DECODE", $"Error: {response}"), 
		};
		await SendResponse(HttpCode.BadRequest, logonResult);
	}

	private async Task SendEmptyResponse(HttpCode code)
	{
		await SendResponse(code, (object)new { });
	}
}
