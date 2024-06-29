using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace Clans.components;

public class CFG : BasePluginConfig
{
	public override int Version { get; set; } = 1;
	// [JsonPropertyName("HookPermission")] public string hookPerm { get; set; } = "";
	
	[JsonPropertyName("MaxClanMembersPerLevel")] public int MaxClanMembersPerLevel { get; set; } = 4;
	[JsonPropertyName("NeedExpPerLevel")] public int NeedExpPerLevel { get; set; } = 500;
	
	[JsonPropertyName("WinExp")] public int WinExp { get; set; } = 200;
	[JsonPropertyName("LoseExp")] public int LoseExp { get; set; } = 100;
	[JsonPropertyName("KillExp")] public int KillExp { get; set; } = 100;
	[JsonPropertyName("AssistExp")] public int AssistExp { get; set; } = 50;
	[JsonPropertyName("AliveInRoundEnd")] public int AliveInRoundEnd { get; set; } = 100;
	
	[JsonPropertyName("Database")] public string Database { get; set; } = "";
	[JsonPropertyName("UserName")] public string UserName { get; set; } = "";
	[JsonPropertyName("Password")] public string Password { get; set; } = "";
	[JsonPropertyName("Host")] public string Host { get; set; } = "";
	[JsonPropertyName("Port")] public int Port { get; set; } = 5432;
}

public class CFGMain
{
	public int MaxClanMembersPerLevel { get; set; } = 4;
	public int NeedExpPerLevel { get; set; } = 500;
	
	public int WinExp { get; set; } = 200;
	public int LoseExp { get; set; } = 100;
	public int KillExp { get; set; } = 100;
	public int AssistExp { get; set; } = 50;
	public int AliveInRoundEnd { get; set; } = 100;
	
	public string Database { get; set; } = "";
	public string UserName { get; set; } = "";
	public string Password { get; set; } = "";
	public string Host { get; set; } = "";
	public int Port { get; set; } = 5432;

	public CFGMain(CFG Config)
	{
		MaxClanMembersPerLevel = Config.MaxClanMembersPerLevel;
		NeedExpPerLevel = Config.NeedExpPerLevel;
		
		WinExp = Config.WinExp;
		LoseExp = Config.LoseExp;
		KillExp = Config.KillExp;
		AssistExp = Config.AssistExp;
		AliveInRoundEnd = Config.AliveInRoundEnd;
		
		Database = Config.Database;
		UserName = Config.UserName;
		Password = Config.Password;
		Host = Config.Host;
		Port = Config.Port;
	}
}