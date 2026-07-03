using System.Net;
using System.Security.Cryptography.X509Certificates;
using Framework;
using Framework.Logging;
using Framework.Web;

namespace BNetServer;

/*
 *  登陆服务管理器
 */
public class LoginServiceManager : Singleton<LoginServiceManager>
{
	private FormInputs formInputs;

	private IPEndPoint externalAddress;

	private IPEndPoint localAddress;

	private X509Certificate2 certificate;

	private LoginServiceManager()
	{
		formInputs = new FormInputs();
	}

	public void Initialize()
	{
		int num = Settings.RestPort;
		if (num < 0 || num > 65535)
		{
			Log.Print(LogType.Error, $"指定的登录服务端口 ({num}) 不能超出指定范围 (1-65535), defaulting to 8081", "Initialize", "D:\\a\\HermesProxy\\HermesProxy\\BnetServer\\Managers\\LoginServiceManager.cs");
			num = 8081;
		}
		string text = Settings.ExternalAddress;
		if (!IPAddress.TryParse(text, out IPAddress iPAddress))
		{
			Log.Print(LogType.Error, "无法解析 LoginREST.ExternalAddress " + text, "Initialize", "D:\\a\\HermesProxy\\HermesProxy\\BnetServer\\Managers\\LoginServiceManager.cs");
			return;
		}
		externalAddress = new IPEndPoint(iPAddress, num);
		text = "127.0.0.1";
		if (!IPAddress.TryParse(text, out iPAddress))
		{
			Log.Print(LogType.Error, "无法解析本地地址.", "Initialize", "D:\\a\\HermesProxy\\HermesProxy\\BnetServer\\Managers\\LoginServiceManager.cs");
			return;
		}
		localAddress = new IPEndPoint(iPAddress, num);
		formInputs.Type = "LOGIN_FORM";
		FormInput formInput = new FormInput();
		formInput.Id = "account_name";
		formInput.Type = "text";
		formInput.Label = "E-mail";
		formInput.MaxLength = 320;
		formInputs.Inputs.Add(formInput);
		formInput = new FormInput();
		formInput.Id = "password";
		formInput.Type = "password";
		formInput.Label = "Password";
		formInput.MaxLength = 16;
		formInputs.Inputs.Add(formInput);
		formInput = new FormInput();
		formInput.Id = "log_in_submit";
		formInput.Type = "submit";
		formInput.Label = "Log In";
		formInputs.Inputs.Add(formInput);
	}

	public IPEndPoint GetAddressForClient(IPAddress address)
	{
		if (IPAddress.IsLoopback(address))
		{
			return localAddress;
		}
		return externalAddress;
	}

	public FormInputs GetFormInput()
	{
		return formInputs;
	}
}
