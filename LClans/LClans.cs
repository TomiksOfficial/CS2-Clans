using Clans.API;
using Clans.components;
using ClansSharedAPI;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Capabilities;
using Microsoft.Extensions.Logging;
using MenuManager;

namespace Clans;

[MinimumApiVersion(264)]
public partial class LClans : BasePlugin, IPluginConfig<CFG>
{
	public override string ModuleName => "LClans";
	public override string ModuleVersion => "0.1.0";
	public override string ModuleDescription => "LClans System";
	public override string ModuleAuthor => "Tomiks(vk.com/tomiksofficial)";

	public CFG Config { get; set; }
	
	private static PluginCapability<IEvents> APIEvents { get; } = new("clans:events");
	private static PluginCapability<IAPISkills> APISkills { get; } = new("clans:skills");
	private static PlayerCapability<ILPlayer> APIPlayer { get; } = new("clans:player");
	private static PluginCapability<IAPIClan> APIClan { get; } = new("clans:clan");
	
	public IMenuApi? menuAPI;
	private readonly PluginCapability<IMenuApi?> _menuAPI = new("menu:nfcore");

	public override void Load(bool hotReload)
	{
		Logger.LogInformation("[LClans] Plugin Start");

		DB.InitTable();

		LBaseInfo.Plugin = this;
		LBaseInfo.APISkills = new APISkills();
		LBaseInfo.APIEvent = new APIEvent();
		LBaseInfo.APIClan = new APIClan();
		
		Capabilities.RegisterPluginCapability(APIEvents, () => LBaseInfo.APIEvent);
		Capabilities.RegisterPluginCapability(APISkills, () => LBaseInfo.APISkills);
		
		Capabilities.RegisterPluginCapability(APIClan, () => LBaseInfo.APIClan);

		Capabilities.RegisterPlayerCapability(APIPlayer, player =>
		{
			if (!LStock.IsValidPlayer(player) || !LBaseInfo.LPlayers.ContainsKey(player.SteamID)) return null;

			return LBaseInfo.LPlayers[player.SteamID];
		});
		
		SkillsInfo.GetConfigsInfo();

		RegisterListener<Listeners.OnClientDisconnectPost>(OnPlayerDisconnectPost);
	}

	public override void OnAllPluginsLoaded(bool hotReload)
	{
		
		
		menuAPI = _menuAPI.Get();
		if (menuAPI == null) Logger.LogInformation("[LClans] MenuManager Core not found...");
	}

	public override void Unload(bool hotReload)
	{
		Logger.LogInformation("[LClans] Plugin UnLoad");
	}

	public void OnConfigParsed(CFG config)
	{
		LBaseInfo.Config = new CFGMain(config);

		Logger.LogInformation("[LClans] Config has been loaded");
	}
}