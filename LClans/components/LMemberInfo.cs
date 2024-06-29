using ClansSharedAPI;

namespace Clans.components;

public class LMemberInfo : IMemberInfo
{
	public string ClanRole { get; set; } = "";
	public bool AccesToEditClan { get; set; } = false;
	public long ClanID { get; set; } = -1;

	public LMemberInfo(string clanRole, int clanEdit, long clanid)
	{
		ClanRole = clanRole;
		AccesToEditClan = Convert.ToBoolean(clanEdit);
		ClanID = clanid;
	}
}