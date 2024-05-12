using Clans.API;
using Clans.components;
using ClansSharedAPI;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Capabilities;
using Microsoft.Extensions.Logging;

namespace Clans;

[MinimumApiVersion(228)]
public partial class LClans : BasePlugin, IPluginConfig<CFG>
{
	public override string ModuleName => "LClans";
	public override string ModuleVersion => "0.1.0";
	public override string ModuleDescription => "Clans System";
	public override string ModuleAuthor => "Tomiks(vk.com/tomiksofficial)";

	public CFG Config { get; set; }
	
	private static PluginCapability<IAPISkills> APISkills { get; } = new("clans:skills");

	public override void Load(bool hotReload)
	{
		Logger.LogInformation("[LClans] Plugin Start");

		DB.InitTable();

		LBaseInfo.Plugin = this;
		LBaseInfo.APISkills = new APISkills();
		
		Capabilities.RegisterPluginCapability(APISkills, () => LBaseInfo.APISkills);

		RegisterListener<Listeners.OnClientDisconnectPost>(OnPlayerDisconnectPost);
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