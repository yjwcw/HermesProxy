using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;

namespace HermesProxy;

public class Program
{
	public static class CommandLineArgumentsTemplate
	{
		public static readonly Option<string?> ConfigFileLocation = new Option<string>("--config", delegate(ArgumentResult result)
		{
			if (result.Tokens.Count == 0)
			{
				return "HermesProxy.config";
			}
			string value = result.Tokens.Single().Value;
			if (!File.Exists(value))
			{
				result.ErrorMessage = "Error: config file '" + value + "' does not exist";
				return (string?)null;
			}
			return value;
		}, true, "The config file that will be used");

		public static readonly Option<bool> DisableVersionCheck = new Option<bool>("--no-version-check", "Disables the initial version update check");

		public static readonly Option<string[]> OverwrittenConfigValues = new Option<string[]>("--set", "Overwrites a specific config value. Example: --set ServerAddress=logon.example.com");
	}

	public static int Main(string[] args)
	{
		CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
		Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
		RootCommand obj = new RootCommand("Hermes Proxy: Allows you to play on legacy WoW server with modern client")
		{
			CommandLineArgumentsTemplate.ConfigFileLocation,
			CommandLineArgumentsTemplate.DisableVersionCheck,
			CommandLineArgumentsTemplate.OverwrittenConfigValues
		};
		Parser parser = new CommandLineBuilder(obj).UseDefaults().Build();
		obj.SetHandler(delegate(InvocationContext ctx)
		{
			ParseResult parseResult = ctx.ParseResult;
			Server.ServerMain(new CommandLineArguments
			{
				ConfigFileLocation = parseResult.GetValueForOption(CommandLineArgumentsTemplate.ConfigFileLocation),
				DisableVersionCheck = parseResult.GetValueForOption(CommandLineArgumentsTemplate.DisableVersionCheck),
				OverwrittenConfigValues = ParseMultiArgument(parseResult.GetValueForOption(CommandLineArgumentsTemplate.OverwrittenConfigValues))
			});
		});
		int result = 1;
		try
		{
			result = parser.Invoke(args);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error occured: {ex}");
		}
		if (OsSpecific.AreWeInOurOwnConsole())
		{
			Thread.Sleep(TimeSpan.FromSeconds(3.0));
			Console.WriteLine("Press enter to close");
			Console.ReadLine();
		}
		return result;
	}

	private static Dictionary<string, string> ParseMultiArgument(string[]? multiArgs)
	{
		if (multiArgs == null)
		{
			return new Dictionary<string, string>();
		}
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (string text in multiArgs)
		{
			string[] array = text.Split('=', 2);
			if (array.Length != 2)
			{
				throw new Exception("Invalid argument '" + text + "'");
			}
			dictionary[array[0]] = array[1];
		}
		return dictionary;
	}
}
