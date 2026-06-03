using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LeagueManager : MonoBehaviour
{
    public static LeagueManager Instance { get; private set; }

    public event Action LeagueChanged;

    public string CurrentLeagueName => GetLeagueName(currentLeagueIndex);
    public int CurrentLeagueIndex => currentLeagueIndex;
    public Vector2Int CurrentLeaguePowerRange => GetLeaguePowerRange(currentLeagueIndex);
    public int CurrentMatchNumber => Mathf.Min(currentRoundIndex + 1, rounds.Count);
    public int TotalMatches => rounds.Count;
    public string LastMatchResult { get; private set; } = "";
    public bool IsInLeague => isInLeague;
    public bool SeasonFinished => isInLeague && rounds.Count > 0 && currentRoundIndex >= rounds.Count;
    public LeagueRound CurrentRound => isInLeague && !SeasonFinished && currentRoundIndex >= 0 && currentRoundIndex < rounds.Count
        ? rounds[currentRoundIndex]
        : null;
    public LeagueMatch NextMatch => GetNextUserMatch();

    [SerializeField] private int currentLeagueIndex;
    [SerializeField] private string userClubName = "Barcelona";
    [SerializeField] private bool isInLeague;

    private readonly List<LeagueTeam> teams = new List<LeagueTeam>();
    private readonly List<LeagueRound> rounds = new List<LeagueRound>();
    private int currentRoundIndex;

    private static readonly string[] BaseLeagueNames =
    {
        "G", "F", "E", "D", "C", "B", "A"
    };

    private static readonly string[] DefaultLeagueTeams =
    {
        "Arsenal",
        "Barcelona",
        "Real Madrid",
        "Bayern Munchen",
        "GalataSaray",
        "PSG",
        "Liverpool",
        "Manchester United",
        "Manchester City",
        "Inter Milan"
    };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (isInLeague && teams.Count == 0)
        {
            StartNewSeason();
        }
    }

    public IReadOnlyList<LeagueTeam> GetTable()
    {
        RefreshUserTeamPower();

        List<LeagueTeam> table = teams
            .OrderByDescending(team => team.Points)
            .ThenByDescending(team => team.GoalDifference)
            .ThenByDescending(team => team.GoalsFor)
            .ThenBy(team => team.Name)
            .ToList();

        for (int i = 0; i < table.Count; i++)
        {
            table[i].LeaguePosition = i + 1;
        }

        return table;
    }

    public LeagueMatch GetNextUserMatch()
    {
        if (!isInLeague)
        {
            return null;
        }

        LeagueRound round = CurrentRound;
        return round?.Matches.FirstOrDefault(match => match.ContainsUserTeam);
    }

    public void EnterLeague()
    {
        if (isInLeague && teams.Count > 0 && rounds.Count > 0)
        {
            LeagueChanged?.Invoke();
            return;
        }

        isInLeague = true;
        StartNewSeason();
    }

    public void PlayNextMatch()
    {
        if (!isInLeague)
        {
            return;
        }

        LeagueMatch match = GetNextUserMatch();
        if (match == null)
        {
            if (SeasonFinished)
            {
                FinishSeason("");
            }

            return;
        }

        CompleteCurrentRoundWithGeneratedResult();
    }

    public LeagueRoundResult CompleteCurrentRoundWithGeneratedResult()
    {
        if (!isInLeague)
        {
            return null;
        }

        RefreshUserTeamPower();
        LeagueMatch match = GetNextUserMatch();
        if (match == null)
        {
            return null;
        }

        GenerateMatchScore(match);
        return CompleteCurrentRoundWithUserResult(match.UserGoals, match.OpponentGoals);
    }

    public LeagueRoundResult CompleteCurrentRoundWithUserResult(int userGoals, int opponentGoals)
    {
        if (!isInLeague)
        {
            return null;
        }

        RefreshUserTeamPower();
        LeagueRound round = CurrentRound;
        if (round == null)
        {
            return null;
        }

        LeagueMatch userMatch = round.Matches.FirstOrDefault(match => match.ContainsUserTeam);
        if (userMatch == null)
        {
            return null;
        }

        foreach (LeagueMatch match in round.Matches)
        {
            if (match == userMatch)
            {
                match.SetUserScore(userGoals, opponentGoals);
            }
            else
            {
                GenerateMatchScore(match);
            }

            ApplyMatch(match);
        }

        LastMatchResult = $"{userMatch.UserTeam.Name} {userMatch.UserGoals} - {userMatch.OpponentGoals} {userMatch.OpponentTeam.Name}";
        currentRoundIndex++;

        bool seasonEnded = SeasonFinished;
        string seasonMessage = "";
        if (seasonEnded)
        {
            seasonMessage = FinishSeason(LastMatchResult);
        }
        else
        {
            GetTable();
            LeagueChanged?.Invoke();
        }

        return new LeagueRoundResult(round.RoundNumber, userMatch, round.Matches, seasonEnded, seasonMessage);
    }

    public void StartNewSeason()
    {
        isInLeague = true;
        StartNewSeason("");
    }

    private void StartNewSeason(string preservedResult)
    {
        teams.Clear();
        rounds.Clear();
        currentRoundIndex = 0;
        LastMatchResult = preservedResult;

        List<float> aiTeamPowers = GenerateAiTeamPowers(DefaultLeagueTeams.Length - 1);
        int aiTeamPowerIndex = 0;

        foreach (string teamName in DefaultLeagueTeams)
        {
            bool isUserTeam = string.Equals(teamName, userClubName, StringComparison.OrdinalIgnoreCase);
            float teamPower = isUserTeam
                ? LeagueTeamPowerCalculator.CalculateUserTeamPower()
                : aiTeamPowers[Mathf.Min(aiTeamPowerIndex++, aiTeamPowers.Count - 1)];

            teams.Add(new LeagueTeam(teamName, isUserTeam, teamPower));
        }

        BuildRoundRobinFixture();
        GetTable();
        LeagueChanged?.Invoke();
    }

    private void BuildRoundRobinFixture()
    {
        if (teams.Count % 2 != 0)
        {
            Debug.LogWarning("[League] Round-robin fixture requires an even team count.", this);
            return;
        }

        List<LeagueTeam> rotation = new List<LeagueTeam>(teams);
        int teamCount = rotation.Count;
        int roundsToCreate = teamCount - 1;
        int matchesPerRound = teamCount / 2;

        for (int roundIndex = 0; roundIndex < roundsToCreate; roundIndex++)
        {
            LeagueRound round = new LeagueRound(roundIndex + 1);

            for (int matchIndex = 0; matchIndex < matchesPerRound; matchIndex++)
            {
                LeagueTeam teamA = rotation[matchIndex];
                LeagueTeam teamB = rotation[teamCount - 1 - matchIndex];

                if (roundIndex % 2 == 1)
                {
                    (teamA, teamB) = (teamB, teamA);
                }

                round.Matches.Add(new LeagueMatch(teamA, teamB, round.RoundNumber, matchIndex + 1));
            }

            rounds.Add(round);
            RotateTeams(rotation);
        }
    }

    private static void RotateTeams(List<LeagueTeam> rotation)
    {
        LeagueTeam last = rotation[rotation.Count - 1];
        for (int i = rotation.Count - 1; i > 1; i--)
        {
            rotation[i] = rotation[i - 1];
        }

        rotation[1] = last;
    }

    private static void ApplyMatch(LeagueMatch match)
    {
        if (match.IsPlayed)
        {
            return;
        }

        match.TeamA.ApplyMatch(match.TeamAGoals, match.TeamBGoals);
        match.TeamB.ApplyMatch(match.TeamBGoals, match.TeamAGoals);
        match.IsPlayed = true;
    }

    private static void GenerateMatchScore(LeagueMatch match)
    {
        match.TeamAGoals = GenerateGoals(match.TeamA, match.TeamB);
        match.TeamBGoals = GenerateGoals(match.TeamB, match.TeamA);
    }

    private static int GenerateGoals(LeagueTeam team, LeagueTeam opponent)
    {
        float powerDifference = team.Power - opponent.Power;
        float baseChance = UnityEngine.Random.Range(0f, 3.2f);
        float powerBonus = powerDifference / 35f;
        return Mathf.Clamp(Mathf.RoundToInt(baseChance + powerBonus), 0, 5);
    }

    private List<float> GenerateAiTeamPowers(int teamCount)
    {
        List<float> powers = new List<float>(teamCount);
        Vector2Int powerRange = CurrentLeaguePowerRange;

        for (int i = 0; i < teamCount; i++)
        {
            powers.Add(UnityEngine.Random.Range(powerRange.x, powerRange.y + 1));
        }

        powers.Sort();
        return powers;
    }

    private void RefreshUserTeamPower()
    {
        LeagueTeam userTeam = teams.FirstOrDefault(team => team.IsUserTeam);
        if (userTeam != null)
        {
            userTeam.Power = LeagueTeamPowerCalculator.CalculateUserTeamPower();
        }
    }

    private string FinishSeason(string resultPrefix)
    {
        LeagueTeam userTeam = teams.FirstOrDefault(team => team.IsUserTeam);
        IReadOnlyList<LeagueTeam> finalTable = GetTable();
        int userPosition = 10;
        for (int i = 0; i < finalTable.Count; i++)
        {
            if (finalTable[i] == userTeam)
            {
                userPosition = i + 1;
                break;
            }
        }

        string seasonMessage;

        if (userPosition == 1 || userPosition == 2)
        {
            currentLeagueIndex++;
            seasonMessage = $"Promoted to League {CurrentLeagueName}";
        }
        else
        {
            seasonMessage = $"Stayed in League {CurrentLeagueName}";
        }

        string finalMessage = string.IsNullOrWhiteSpace(resultPrefix)
            ? seasonMessage
            : $"{resultPrefix} | {seasonMessage}";

        StartNewSeason(finalMessage);
        return seasonMessage;
    }

    public static string GetLeagueName(int leagueIndex)
    {
        int safeIndex = Mathf.Max(0, leagueIndex);
        if (safeIndex < BaseLeagueNames.Length)
        {
            return BaseLeagueNames[safeIndex];
        }

        return $"A{safeIndex - BaseLeagueNames.Length + 1}";
    }

    public static Vector2Int GetLeaguePowerRange(int leagueIndex)
    {
        int safeIndex = Mathf.Max(0, leagueIndex);
        switch (safeIndex)
        {
            case 0:
                return new Vector2Int(20, 30);
            case 1:
                return new Vector2Int(30, 40);
            case 2:
                return new Vector2Int(40, 50);
            case 3:
                return new Vector2Int(50, 60);
            case 4:
                return new Vector2Int(60, 70);
            case 5:
                return new Vector2Int(70, 80);
            case 6:
                return new Vector2Int(80, 90);
            default:
                int numberedALeague = safeIndex - BaseLeagueNames.Length + 1;
                int minPower = Mathf.Min(89, 80 + (numberedALeague * 2));
                int maxPower = Mathf.Min(99, 90 + (numberedALeague * 2));
                return new Vector2Int(minPower, maxPower);
        }
    }
}

public class LeagueTeam
{
    public string Name { get; }
    public bool IsUserTeam { get; }
    public float Power { get; set; }
    public int LeaguePosition { get; set; }
    public int PlayedMatches { get; private set; }
    public int Wins { get; private set; }
    public int Draws { get; private set; }
    public int Losses { get; private set; }
    public int GoalsFor { get; private set; }
    public int GoalsAgainst { get; private set; }
    public int GoalDifference => GoalsFor - GoalsAgainst;
    public int Points { get; private set; }

    public LeagueTeam(string name, bool isUserTeam, float power)
    {
        Name = name;
        IsUserTeam = isUserTeam;
        Power = Mathf.Clamp(power, 1f, 99f);
    }

    public void ApplyMatch(int goalsFor, int goalsAgainst)
    {
        PlayedMatches++;
        GoalsFor += goalsFor;
        GoalsAgainst += goalsAgainst;

        if (goalsFor > goalsAgainst)
        {
            Wins++;
            Points += 3;
        }
        else if (goalsFor == goalsAgainst)
        {
            Draws++;
            Points += 1;
        }
        else
        {
            Losses++;
        }
    }
}

public class LeagueRound
{
    public int RoundNumber { get; }
    public List<LeagueMatch> Matches { get; } = new List<LeagueMatch>();

    public LeagueRound(int roundNumber)
    {
        RoundNumber = roundNumber;
    }
}

public class LeagueMatch
{
    public LeagueTeam TeamA { get; }
    public LeagueTeam TeamB { get; }
    public LeagueTeam UserTeam => TeamA.IsUserTeam ? TeamA : TeamB.IsUserTeam ? TeamB : null;
    public LeagueTeam OpponentTeam => TeamA.IsUserTeam ? TeamB : TeamB.IsUserTeam ? TeamA : null;
    public bool ContainsUserTeam => UserTeam != null;
    public int RoundNumber { get; }
    public int MatchNumber { get; }
    public int TeamAGoals { get; set; }
    public int TeamBGoals { get; set; }
    public int UserGoals
    {
        get => TeamA.IsUserTeam ? TeamAGoals : TeamBGoals;
        set
        {
            if (TeamA.IsUserTeam)
            {
                TeamAGoals = value;
            }
            else
            {
                TeamBGoals = value;
            }
        }
    }

    public int OpponentGoals
    {
        get => TeamA.IsUserTeam ? TeamBGoals : TeamAGoals;
        set
        {
            if (TeamA.IsUserTeam)
            {
                TeamBGoals = value;
            }
            else
            {
                TeamAGoals = value;
            }
        }
    }

    public bool IsPlayed { get; set; }

    public LeagueMatch(LeagueTeam teamA, LeagueTeam teamB, int roundNumber, int matchNumber)
    {
        TeamA = teamA;
        TeamB = teamB;
        RoundNumber = roundNumber;
        MatchNumber = matchNumber;
    }

    public void SetUserScore(int userGoals, int opponentGoals)
    {
        UserGoals = userGoals;
        OpponentGoals = opponentGoals;
    }
}

public class LeagueRoundResult
{
    public int RoundNumber { get; }
    public LeagueMatch UserMatch { get; }
    public IReadOnlyList<LeagueMatch> Matches { get; }
    public bool SeasonEnded { get; }
    public string SeasonMessage { get; }

    public LeagueRoundResult(int roundNumber, LeagueMatch userMatch, IReadOnlyList<LeagueMatch> matches, bool seasonEnded, string seasonMessage)
    {
        RoundNumber = roundNumber;
        UserMatch = userMatch;
        Matches = matches;
        SeasonEnded = seasonEnded;
        SeasonMessage = seasonMessage;
    }
}

public class LeagueTableEntry
{
    public LeagueTeam Team { get; }

    public LeagueTableEntry(LeagueTeam team)
    {
        Team = team;
    }
}
