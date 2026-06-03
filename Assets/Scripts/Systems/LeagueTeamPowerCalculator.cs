using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class LeagueTeamPowerCalculator
{
    private const int RequiredForwards = 3;
    private const int RequiredMidfielders = 3;
    private const int RequiredDefenders = 3;
    private const int RequiredGoalkeepers = 1;
    private const int RequiredLineupSize = 10;
    private const float MissingPlayerPenalty = 5f;
    private const float NoPlayerPower = 10f;

    public static float CalculateUserTeamPower()
    {
        MarketPanelRuntimePopulator marketPopulator = Object.FindFirstObjectByType<MarketPanelRuntimePopulator>(FindObjectsInactive.Include);
        if (marketPopulator == null)
        {
            return NoPlayerPower;
        }

        float baseTeamPower = CalculatePlayersPower(marketPopulator.OwnedPlayers);
        float managerBonus = CalculateBestManagerBonus(marketPopulator.OwnedManagers);
        return Mathf.Clamp(baseTeamPower + managerBonus, 1f, 99f);
    }

    private static float CalculatePlayersPower(IReadOnlyList<FootballPlayerMarketData> ownedPlayers)
    {
        if (ownedPlayers == null || ownedPlayers.Count == 0)
        {
            return NoPlayerPower;
        }

        List<float> selectedPowers = new List<float>(RequiredLineupSize);
        AddBestPlayers(selectedPowers, ownedPlayers, FootballPlayerType.Forward, RequiredForwards);
        AddBestPlayers(selectedPowers, ownedPlayers, FootballPlayerType.Midfielder, RequiredMidfielders);
        AddBestPlayers(selectedPowers, ownedPlayers, FootballPlayerType.Defender, RequiredDefenders);
        AddBestPlayers(selectedPowers, ownedPlayers, FootballPlayerType.Goalkeeper, RequiredGoalkeepers);

        if (selectedPowers.Count == 0)
        {
            return NoPlayerPower;
        }

        float averagePower = selectedPowers.Average();
        int missingPlayers = Mathf.Max(0, RequiredLineupSize - selectedPowers.Count);
        return averagePower - (missingPlayers * MissingPlayerPenalty);
    }

    private static void AddBestPlayers(
        List<float> selectedPowers,
        IReadOnlyList<FootballPlayerMarketData> ownedPlayers,
        FootballPlayerType playerType,
        int count)
    {
        IEnumerable<float> bestPowers = ownedPlayers
            .Where(player => player != null && player.PlayerType == playerType)
            .Select(CalculatePlayerPower)
            .OrderByDescending(power => power)
            .Take(count);

        selectedPowers.AddRange(bestPowers);
    }

    private static float CalculatePlayerPower(FootballPlayerMarketData player)
    {
        return FootballPlayerStatGenerator.CalculateOverallScore(player.PlayerType, player.Stats);
    }

    private static float CalculateBestManagerBonus(IReadOnlyList<FootballManagerMarketData> ownedManagers)
    {
        if (ownedManagers == null || ownedManagers.Count == 0)
        {
            return 0f;
        }

        return ownedManagers
            .Where(manager => manager != null)
            .Select(CalculateManagerBonus)
            .DefaultIfEmpty(0f)
            .Max();
    }

    private static float CalculateManagerBonus(FootballManagerMarketData manager)
    {
        ManagerStats stats = manager.Stats;
        return (stats.trainingBoost * 0.25f) +
               (stats.tacticBoost * 0.35f) +
               (stats.footballIQ * 0.25f) +
               (stats.experience * 0.15f);
    }
}
