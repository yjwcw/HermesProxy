using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BNetServer;
using BNetServer.Networking;
using Framework;
using Framework.Logging;
using Framework.Networking;
using HermesProxy.Configuration;
using HermesProxy.World;
using HermesProxy.World.Server;

namespace HermesProxy;

internal class Server
{
	private static readonly string? _buildTag;

	static Server()
	{
		_buildTag = "*official* ";
	}

	public static void ServerMain(CommandLineArguments args)
	{
		Log.Print(LogType.Server, "开始 Hermes Proxy...", "ServerMain", "D:\\a\\HermesProxy\\HermesProxy\\Server.cs");
		Log.Print(LogType.Server, "Version 2024-3-20", "ServerMain", "D:\\a\\HermesProxy\\HermesProxy\\Server.cs");
		Log.Start();
		if (Environment.CurrentDirectory != Path.GetDirectoryName(AppContext.BaseDirectory))
		{
			Log.Print(LogType.Storage, "切换工作目录", "ServerMain", "D:\\a\\HermesProxy\\HermesProxy\\Server.cs");
			Log.Print(LogType.Storage, "Old: " + Environment.CurrentDirectory, "ServerMain", "D:\\a\\HermesProxy\\HermesProxy\\Server.cs");
			Environment.CurrentDirectory = Path.GetDirectoryName(AppContext.BaseDirectory);
			Log.Print(LogType.Storage, "New: " + Environment.CurrentDirectory, "ServerMain", "D:\\a\\HermesProxy\\HermesProxy\\Server.cs");
			Thread.Sleep(TimeSpan.FromSeconds(1.0));
		}
		ConfigurationParser config;
		try
		{
            //config = ConfigurationParser.ParseFromFile(args.ConfigFileLocation, args.OverwrittenConfigValues);
            string configFileName = "HermesProxy.config";
            Dictionary<string, string> overwrittenValues2 = new Dictionary<string, string>();
            config = ConfigurationParser.ParseFromFile(configFileName, overwrittenValues2);
        }
		catch (FileNotFoundException)
		{
			Log.Print(LogType.Error, "配置加载失败", "ServerMain", "D:\\a\\HermesProxy\\HermesProxy\\Server.cs");
			return;
		}
		if (!Settings.LoadAndVerifyFrom(config))
		{
			Log.Print(LogType.Error, "配置验证失败", "ServerMain", "D:\\a\\HermesProxy\\HermesProxy\\Server.cs");
			return;
		}
		Log.DebugLogEnabled = Settings.DebugOutput;
		Log.Print(LogType.Debug, "启用调试日志记录", "ServerMain", "D:\\a\\HermesProxy\\HermesProxy\\Server.cs");
		if (!AesGcm.IsSupported)
		{
			Log.Print(LogType.Error, "您的平台不支持 AesGcm", "ServerMain", "D:\\a\\HermesProxy\\HermesProxy\\Server.cs");
			if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			{
				Log.Print(LogType.Error, "Since you are on MacOS, you can install openssl@3 via homebrew", "ServerMain", "D:\\a\\HermesProxy\\HermesProxy\\Server.cs");
				Log.Print(LogType.Error, "Run this:      brew install openssl@3", "ServerMain", "D:\\a\\HermesProxy\\HermesProxy\\Server.cs");
				Log.Print(LogType.Error, "Start Hermes:  DYLD_LIBRARY_PATH=/opt/homebrew/opt/openssl@3/lib ./HermesProxy", "ServerMain", "D:\\a\\HermesProxy\\HermesProxy\\Server.cs");
			}
			return;
		}
		Log.Print(LogType.Server, $"客户端版本: {Settings.ClientBuild}", "ServerMain", "D:\\a\\HermesProxy\\HermesProxy\\Server.cs");
		Log.Print(LogType.Server, $"服务器版本: {Settings.ServerBuild}", "ServerMain", "D:\\a\\HermesProxy\\HermesProxy\\Server.cs");
		
		// 加载游戏数据文件
		GameData.LoadEverything();

        // 本地主机上监听 IP
        IPAddress iPAddress = NetworkUtils.ResolveOrDirectIPv64(Settings.ExternalAddress);
		if (!IPAddress.IsLoopback(iPAddress))
		{
			iPAddress = IPAddress.Any;
		}
		Log.Print(LogType.Network, "本地 IP: " + Settings.ExternalAddress, "ServerMain", "D:\\a\\HermesProxy\\HermesProxy\\Server.cs");

        // 登陆服务管理器初始化  (保存我们的本地 IP)
        Singleton<LoginServiceManager>.Instance.Initialize();

        // 1. 启动二进制 bnet RPC 服务连接的侦听器
        SocketManager<BnetTcpSession> socketManager = StartServer<BnetTcpSession>(new IPEndPoint(iPAddress, Settings.BNetPort));
        
		// 2. 启动 http(s) bnet RPC 服务的侦听器，例如 auth/"realm" 连接
        SocketManager<BnetRestApiSession> socketManager2 = StartServer<BnetRestApiSession>(new IPEndPoint(iPAddress, Settings.RestPort));
        
		// 3. 启动服务连接的侦听器
        SocketManager<RealmSocket> socketManager3 = StartServer<RealmSocket>(new IPEndPoint(iPAddress, Settings.RealmPort));
        
		// 4. 启动世界连接的监听器
        SocketManager<WorldSocket> socketManager4 = StartServer<WorldSocket>(new IPEndPoint(iPAddress, Settings.InstancePort));
		
		// 开始监听
		while (
			socketManager2.IsListening || 
			socketManager.IsListening || 
			socketManager3.IsListening || 
			socketManager4.IsListening
			)
		{
			Thread.Sleep(TimeSpan.FromSeconds(10.0));
		}
        /*
        Console.WriteLine($"(restSocketServer.IsListening: {socketManager2.IsListening}");
		Console.WriteLine($"(bnetSocketServer.IsListening: {socketManager.IsListening}");
		Console.WriteLine($"(realmSocketServer.IsListening: {socketManager3.IsListening}");
		Console.WriteLine($"(worldSocketServer.IsListening: {socketManager4.IsListening}");
        */

    }

	private static SocketManager<TSocketType> StartServer<TSocketType>(IPEndPoint bindIp) where TSocketType : ISocket
	{
		SocketManager<TSocketType> socketManager = new SocketManager<TSocketType>();
		Log.Print(LogType.Server, $"启动 {typeof(TSocketType).Name} 服务 {bindIp}...", "StartServer", "D:\\a\\HermesProxy\\HermesProxy\\Server.cs");
		if (!socketManager.StartNetwork(bindIp.Address.ToString(), bindIp.Port))
		{
			throw new Exception("未能启动 " + typeof(TSocketType).Name + " 服务");
		}
		Thread.Sleep(50);
		return socketManager;
	}

}
