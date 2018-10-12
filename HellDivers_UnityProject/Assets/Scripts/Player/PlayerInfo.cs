using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class PlayerInfo
{
    public string username;
    public string nickname;
    public int rank;
    public int totalMissionFought;
    public int totalMissionWon;
    public int totalDeaths;
    public int totalShotsFired;
    public int totalEnemiesKilled;
    public List<int> weapons;
    public List<int> stratagems;

    public PlayerInfo Clone()
    {
        PlayerInfo clone = new PlayerInfo();
        CopyTo(clone);
        return clone;
    }

    public void CopyTo(PlayerInfo other)
    {
        other.username = this.username;
        other.nickname = this.nickname;
        other.rank = this.rank;
        other.totalMissionFought = this.totalMissionFought;
        other.totalMissionWon = this.totalMissionWon;
        other.totalDeaths = this.totalDeaths;
        other.totalShotsFired = this.totalShotsFired;
        other.totalEnemiesKilled = this.totalEnemiesKilled;
        other.weapons = this.weapons;
        other.stratagems = this.stratagems;
    }
}