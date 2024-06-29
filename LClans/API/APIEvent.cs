using Clans.components;
using ClansSharedAPI;

namespace Clans.API;

public class APIEvent : IEvents
{
	public event Action<ILPlayer>? OnPlayerLoaded;
	public event Action<ILPlayer>? OnPlayerDisconnectFull;
	public event Action<ILPlayer>? OnPlayerSpawned;
	public event Action<ILPlayer, string>? OnPlayerUpgradeSkill;
	public event Action<ILPlayer, string>? OnPlayerSellSkill;

	public void IOnPlayerLoaded(ILPlayer? player)
	{
		if(OnPlayerLoaded is not null && player != null && LBaseInfo.LPlayers.ContainsKey(player.player.SteamID))
		{
			OnPlayerLoaded?.Invoke(player);
		}
	}
	
	public void IOnPlayerDisconnectFull(ILPlayer? player)
	{
		if(OnPlayerDisconnectFull is not null && player != null && LBaseInfo.LPlayers.ContainsKey(player.player.SteamID))
		{
			OnPlayerDisconnectFull?.Invoke(player);
		}
	}
	
	public void IOnPlayerSpawned(ILPlayer? player)
	{
		if(OnPlayerSpawned is not null && player != null && LBaseInfo.LPlayers.ContainsKey(player.player.SteamID))
		{
			OnPlayerSpawned?.Invoke(player);
		}
	}
	
	public void IOnPlayerUpgradeSkill(ILPlayer? player, string skillName)
	{
		if(OnPlayerUpgradeSkill is not null && player != null && LBaseInfo.LPlayers.ContainsKey(player.player.SteamID))
		{
			OnPlayerUpgradeSkill?.Invoke(player, skillName);
		}
	}
	
	public void IOnPlayerSellSkill(ILPlayer? player, string skillName)
	{
		if(OnPlayerSellSkill is not null && player != null && LBaseInfo.LPlayers.ContainsKey(player.player.SteamID))
		{
			OnPlayerSellSkill?.Invoke(player, skillName);
		}
	}
}