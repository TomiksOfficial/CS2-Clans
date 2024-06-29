using Clans.components;
using ClansSharedAPI;

namespace Clans.API;

public class APIClan : IAPIClan
{
	public IClan? GetClanById(long clanID)
	{
		if (!LBaseInfo.Clans.ContainsKey(clanID))
		{
			return null;
		}

		return LBaseInfo.Clans[clanID];
	}
}