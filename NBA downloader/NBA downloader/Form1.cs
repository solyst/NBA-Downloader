using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HtmlAgilityPack;
using System.Net;
using System.Globalization;

namespace NBA_downloader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnDownloadGame_Click(object sender, EventArgs e)
        {
            string websiteUrl = "https://www.basketball-reference.com/boxscores/pbp/201711150MIN.html";

            if (txtUrl.Text != "")
            {
                websiteUrl = txtUrl.Text;
            }

            GameData gameData = createGameData(websiteUrl);

            exportGameData(gameData);
        }

        private void btnDownloadSeason_Click(object sender, EventArgs e)
        {
            txtOutput.Clear();
            
            //string websiteUrl = "https://www.basketball-reference.com/teams/BOS/2017_games.html";

            //Error handling for missing URL
            if (txtUrl.Text == "")
            {
                MessageBox.Show("Requires a website URL.");
                return;
            }
            string websiteUrl = txtUrl.Text;

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            ///
            /// FIND INFORMATION FOR ALL GAMES IN THE SEASON TO CREATE SUMMARY STATISTICS
            ///
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            //Error handler for a missing team location
            if (txtTeam.Text == "")
            {
                MessageBox.Show("Requires a team location.");
                return;
            }
            string teamLocation = txtTeam.Text;

            //Find all games for an individual season
            List<string> regularSeasonUrls = new List<string>();
            List<string> postSeasonUrls = new List<string>();

            findGamesByTeamSeason(websiteUrl, out regularSeasonUrls, out postSeasonUrls);


            //Download data for all regular season games
            List<GameData> regularSeasonGames = new List<GameData>();
            foreach (string url in regularSeasonUrls)
            {
                GameData g = createGameData(url, teamLocation);
                regularSeasonGames.Add(g);
                txtOutput.AppendText(Environment.NewLine + g.url);
                txtOutput.Refresh();
            }

            //Download data for all post season games (may be empty)
            List<GameData> postSeasonGames = new List<GameData>();
            foreach (string url in postSeasonUrls)
            {
                GameData g = createGameData(url, teamLocation);
                postSeasonGames.Add(g);
                txtOutput.AppendText(Environment.NewLine + g.url);
                txtOutput.Refresh();
            }

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            ///
            /// GAME BY GAME SUMMARY STATS
            ///
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            //Create game by game summary file
            /*
            Opponent
            Largest Lead
            Largest Deficit
            Spread
            Ties
            Lead Changes
            Most Consecutive Points
            Most Consecutive Points Opp
            Highest Velocity Average
            Lowest Velocity Average
            Velocity Full Game Average
            Margin of Victory
            High/Low Velocity
            High/Avg Velocity
            High Velocity/Margin
            Url of Game
            */
            StringBuilder gameSummary = new StringBuilder();
            gameSummary.AppendLine(@"Team,Opponent,Largest Lead,Largest Deficit,Spread,Ties,Lead Changes,Most Consecutive Points " + teamLocation + ",Most Consecutive Points Opp,Highest Velocity Average,Lowest Velocity Average,Velocity Full Game Average,Margin of Victory,Points For,Points Against,High/Low Velocity,High/Avg Velocity,High Velocity/Margin,High Velocity/Points,Url of Game");
            gameSummary.AppendLine("REGULAR SEASON");
            foreach (GameData g in regularSeasonGames)
            {
                gameSummary.AppendLine(teamLocation + "," + g.Opponent + "," + g.TargetLargestLead + "," + g.TargetLargestDeficit + "," + g.TargetSpread + "," + g.Ties + "," + g.LeadChanges + "," + g.TargetConsecutivePoints + "," + g.TargetConsecutivePointsOpp + "," + g.AvgOfHighestVelocityRanges + "," + g.AvgOfLowestVelocityRanges + "," + g.VelocityFullGame + "," + g.TargetMarginOfVictory + "," + g.TargetPointsFor + "," + g.TargetPointsAgainst + "," + g.AvgOfHighestVelocityRanges / g.AvgOfLowestVelocityRanges + "," + g.AvgOfHighestVelocityRanges / g.VelocityFullGame + "," + g.AvgOfHighestVelocityRanges / Math.Abs(g.Team1GamePlusMinus) + "," + g.VelocityHighOverPoints + "," + g.url);
            }
            gameSummary.AppendLine("POST SEASON");
            foreach (GameData g in postSeasonGames)
            {
                gameSummary.AppendLine(teamLocation + "," + g.Opponent + "," + g.TargetLargestLead + "," + g.TargetLargestDeficit + "," + g.TargetSpread + "," + g.Ties + "," + g.LeadChanges + "," + g.TargetConsecutivePoints + "," + g.TargetConsecutivePointsOpp + "," + g.AvgOfHighestVelocityRanges + "," + g.AvgOfLowestVelocityRanges + "," + g.VelocityFullGame + "," + g.TargetMarginOfVictory + "," + g.TargetPointsFor + "," + g.TargetPointsAgainst + "," + g.AvgOfHighestVelocityRanges / g.AvgOfLowestVelocityRanges + "," + g.AvgOfHighestVelocityRanges / g.VelocityFullGame + "," + g.AvgOfHighestVelocityRanges / Math.Abs(g.Team1GamePlusMinus) + "," + g.VelocityHighOverPoints + "," + g.url);
            }

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            ///
            /// SEASON SUMMARY STATS
            ///
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            StringBuilder seasonSummary = new StringBuilder();

            //Multiple lines (easy to read, harder to combine as a summary)
            seasonSummary.AppendLine(@"Team,Segment,Games,Stat,Largest Lead,Largest Deficit,Spread,Ties,Lead Changes,Most Consecutive Points " + teamLocation + ",Most Consecutive Points Opp,Highest Velocity Average,Lowest Velocity Average,Velocity Full Game Average,Margin of Victory,Points For,Points Against,High/Low Velocity,High/Avg Velocity,High Velocity/Margin,High Velocity/Points");
            seasonSummary.AppendLine("REGULAR SEASON");
            SeasonSummaryExportMultiLine(regularSeasonGames, teamLocation, "Regular Season", seasonSummary);
            seasonSummary.AppendLine("POST SEASON");
            SeasonSummaryExportMultiLine(postSeasonGames, teamLocation, "Post Season", seasonSummary);
            seasonSummary.AppendLine("COMBINED REGULAR AND POST SEASON");
            SeasonSummaryExportMultiLine(regularSeasonGames.Concat(postSeasonGames).ToList(), teamLocation, "Combined Season", seasonSummary);
            seasonSummary.AppendLine(); //spacer

            //Single line export used to combine later into a single file
            seasonSummary.AppendLine(@"Team,Segment,Games,Largest Lead,StdDev,Largest Deficit,StdDev,Spread,StdDev,Ties,StdDev,Lead Changes,StdDev,Most Consecutive Points " + teamLocation + ",StdDev,Most Consecutive Points Opp,StdDev,Highest Velocity Average,StdDev,Lowest Velocity Average,StdDev,Velocity Full Game Average,StdDev,Margin of Victory,StdDev,Points For,StdDev,Points Against,StdDev,High/Low Velocity,StdDev,High/Avg Velocity,StdDev,High Velocity/Margin,StdDev,High Velocity/Points,StdDev");
            SeasonSummaryExportSingleLine(regularSeasonGames, teamLocation, "Regular Season", seasonSummary);
            SeasonSummaryExportSingleLine(postSeasonGames, teamLocation, "Post Season", seasonSummary);
            SeasonSummaryExportSingleLine(regularSeasonGames.Concat(postSeasonGames).ToList(), teamLocation, "Combined Season", seasonSummary);
            

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            ///
            /// EXPORT SEASON GAMES AND SUMMARY DATA
            ///
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            string exportPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            exportPath += "\\" + teamLocation;
            System.IO.Directory.CreateDirectory(exportPath);

            System.IO.File.WriteAllText(exportPath + "\\Game Summary " + teamLocation + ".csv", gameSummary.ToString());

            System.IO.File.WriteAllText(exportPath + "\\Season Summary " + teamLocation + ".csv", seasonSummary.ToString());


        }

        private void SeasonSummaryExportMultiLine(List<GameData> games, string teamLocation, string segment, StringBuilder stringBuilder)
        {
            if (games.Count > 0)
            {
                //Averages
                stringBuilder.AppendLine(teamLocation
                    + "," + segment
                    + "," + games.Count
                    + "," + "Mean"
                    + "," + games.Average(x => x.TargetLargestLead)
                    + "," + games.Average(x => x.TargetLargestDeficit)
                    + "," + games.Average(x => x.TargetSpread)
                    + "," + games.Average(x => x.Ties)
                    + "," + games.Average(x => x.LeadChanges)
                    + "," + games.Average(x => x.TargetConsecutivePoints)
                    + "," + games.Average(x => x.TargetConsecutivePointsOpp)
                    + "," + games.Average(x => x.AvgOfHighestVelocityRanges)
                    + "," + games.Average(x => x.AvgOfLowestVelocityRanges)
                    + "," + games.Average(x => x.VelocityFullGame)
                    + "," + games.Average(x => x.TargetMarginOfVictory)
                    + "," + games.Average(x => x.TargetPointsFor)
                    + "," + games.Average(x => x.TargetPointsAgainst)
                    + "," + games.Average(x => x.VelocityHighOverLow)
                    + "," + games.Average(x => x.VelocityHighOverFull)
                    + "," + games.Average(x => x.VelocityHighOverMargin)
                    + "," + games.Average(x => x.VelocityHighOverPoints)
                    );
                //Std Dev          
                stringBuilder.AppendLine(teamLocation
                    + "," + segment
                    + "," + games.Count
                    + "," + "Std Dev"
                    + "," + CalculateStdDev(games.Select(x => (double)x.TargetLargestLead))
                    + "," + CalculateStdDev(games.Select(x => (double)x.TargetLargestDeficit))
                    + "," + CalculateStdDev(games.Select(x => (double)x.TargetSpread))
                    + "," + CalculateStdDev(games.Select(x => (double)x.Ties))
                    + "," + CalculateStdDev(games.Select(x => (double)x.LeadChanges))
                    + "," + CalculateStdDev(games.Select(x => (double)x.TargetConsecutivePoints))
                    + "," + CalculateStdDev(games.Select(x => (double)x.TargetConsecutivePointsOpp))
                    + "," + CalculateStdDev(games.Select(x => x.AvgOfHighestVelocityRanges))
                    + "," + CalculateStdDev(games.Select(x => x.AvgOfLowestVelocityRanges))
                    + "," + CalculateStdDev(games.Select(x => x.VelocityFullGame))
                    + "," + CalculateStdDev(games.Select(x => (double)x.TargetMarginOfVictory))
                    + "," + CalculateStdDev(games.Select(x => (double)x.TargetPointsFor))
                    + "," + CalculateStdDev(games.Select(x => (double)x.TargetPointsAgainst))
                    + "," + CalculateStdDev(games.Select(x => x.VelocityHighOverLow))
                    + "," + CalculateStdDev(games.Select(x => x.VelocityHighOverFull))
                    + "," + CalculateStdDev(games.Select(x => x.VelocityHighOverMargin))
                    + "," + CalculateStdDev(games.Select(x => x.VelocityHighOverPoints))
                    );
            }
        }

        private void SeasonSummaryExportSingleLine(List<GameData> games, string teamLocation, string segment, StringBuilder stringBuilder)
        {
            if (games.Count > 0)
            {
                stringBuilder.AppendLine(teamLocation
                + "," + segment
                + "," + games.Count
                + "," + games.Average(x => x.TargetLargestLead) + "," + CalculateStdDev(games.Select(x => (double)x.TargetLargestLead))
                + "," + games.Average(x => x.TargetLargestDeficit) + "," + CalculateStdDev(games.Select(x => (double)x.TargetLargestDeficit))
                + "," + games.Average(x => x.TargetSpread) + "," + CalculateStdDev(games.Select(x => (double)x.TargetSpread))
                + "," + games.Average(x => x.Ties) + "," + CalculateStdDev(games.Select(x => (double)x.Ties))
                + "," + games.Average(x => x.LeadChanges) + "," + CalculateStdDev(games.Select(x => (double)x.LeadChanges))
                + "," + games.Average(x => x.TargetConsecutivePoints) + "," + CalculateStdDev(games.Select(x => (double)x.TargetConsecutivePoints))
                + "," + games.Average(x => x.TargetConsecutivePointsOpp) + "," + CalculateStdDev(games.Select(x => (double)x.TargetConsecutivePointsOpp))
                + "," + games.Average(x => x.AvgOfHighestVelocityRanges) + "," + CalculateStdDev(games.Select(x => x.AvgOfHighestVelocityRanges))
                + "," + games.Average(x => x.AvgOfLowestVelocityRanges) + "," + CalculateStdDev(games.Select(x => x.AvgOfLowestVelocityRanges))
                + "," + games.Average(x => x.VelocityFullGame) + "," + CalculateStdDev(games.Select(x => x.VelocityFullGame))
                + "," + games.Average(x => x.TargetMarginOfVictory) + "," + CalculateStdDev(games.Select(x => (double)x.TargetMarginOfVictory))
                + "," + games.Average(x => x.TargetPointsFor) + "," + CalculateStdDev(games.Select(x => (double)x.TargetPointsFor))
                + "," + games.Average(x => x.TargetPointsAgainst) + "," + CalculateStdDev(games.Select(x => (double)x.TargetPointsAgainst))
                + "," + games.Average(x => x.VelocityHighOverLow) + "," + CalculateStdDev(games.Select(x => x.VelocityHighOverLow))
                + "," + games.Average(x => x.VelocityHighOverFull) + "," + CalculateStdDev(games.Select(x => x.VelocityHighOverFull))
                + "," + games.Average(x => x.VelocityHighOverMargin) + "," + CalculateStdDev(games.Select(x => x.VelocityHighOverMargin))
                + "," + games.Average(x => x.VelocityHighOverPoints) + "," + CalculateStdDev(games.Select(x => (double)x.VelocityHighOverPoints))
                );
            }
        }

        //CalculateStdDev was taken from someplace online -- thanks someplace!
        private double CalculateStdDev(IEnumerable<double> values)
        {
            double ret = 0;
            if (values.Count() > 0)
            {
                //Compute the Average      
                double avg = values.Average();
                //Perform the Sum of (value-avg)_2_2      
                double sum = values.Sum(d => Math.Pow(d - avg, 2));
                //Put it all together      
                ret = Math.Sqrt((sum) / (values.Count() - 1));
            }
            return ret;
        }


        private void findGamesByTeamSeason(string websiteUrl, out List<string> regularSeasonGames, out List<string> postSeasonGames)
        {
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            ///
            /// FIND ALL GAMES IN A SEASON FOR AN INDIVIDUAL TEAM
            ///
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            const string BASE_URL = "https://www.basketball-reference.com";

            regularSeasonGames = new List<string>();
            postSeasonGames = new List<string>();

            try
            {
                HtmlWeb web = new HtmlWeb();
                HtmlAgilityPack.HtmlDocument doc = web.Load(websiteUrl);

                //Regular Season
                HtmlNode tableNode = doc.DocumentNode.SelectSingleNode("//table[@id='games']//tbody");
                List<HtmlNode> nodes = tableNode.Descendants("a").ToList().Where(x => x.InnerText == "Box Score").ToList();
                foreach (HtmlNode n in nodes)
                {
                    string url = BASE_URL + n.Attributes["href"].Value;
                    url = url.Insert(url.LastIndexOf('/'), "/pbp");
                    regularSeasonGames.Add(url);
                }
                
                //Post Season (if it exists)
                tableNode = doc.DocumentNode.SelectSingleNode("//table[@id='games_playoffs']//tbody");

                if (tableNode != null)
                {
                    nodes = tableNode.Descendants("a").ToList().Where(x => x.InnerText == "Box Score").ToList();

                    foreach (HtmlNode n in nodes)
                    {
                        string url = BASE_URL + n.Attributes["href"].Value;
                        url = url.Insert(url.LastIndexOf('/'), "/pbp");
                        postSeasonGames.Add(url);
                    }
                }
            }
            catch (Exception e)
            {
                txtOutput.Text += Environment.NewLine + "Could not find all games for the season. Games found="+ (regularSeasonGames.Count + postSeasonGames.Count) + ". Error: " + e.Message;
            }

            txtOutput.Text += Environment.NewLine + "Regular Season Games = " + regularSeasonGames.Count + ". Post Season Games = " + postSeasonGames.Count + ".";

        }
    

        private GameData createGameData(string url, string targetTeam = "")
        {
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            ///
            /// EXTRACT DATA FROM WEBSITE
            ///
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            /*
            //Version 1
            WebClient webClient = new WebClient();
            string page = webClient.DownloadString("https://www.basketball-reference.com/boxscores/pbp/201711150MIN.html");
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(page);
            */

            GameData gameData = new GameData();
            gameData.url = url;

            HtmlWeb web = new HtmlWeb();
            HtmlAgilityPack.HtmlDocument doc = web.Load(url);


            //-------------------------------
            //Find Ties and Consecutive Points
            //-------------------------------
            int ties = -1;
            int leadChanges = -1;
            int consecutivePoints = -1;
            int consecutivePointsOpp = -1;
            try
            {
                //Find the appropriate commented node
                HtmlNode commentNode = doc.DocumentNode.SelectSingleNode("//div[@id='all_game-summary']//comment()");

                //Remove the "comment" part of the node, and reload as a fresh webpage
                string page = commentNode.InnerHtml;
                page = page.Substring(5);
                page = page.Substring(0, page.Length - 3);
                doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(page);

                //Find the tables holding the data I need
                HtmlNodeCollection tableNodes = doc.DocumentNode.SelectNodes("//div[@id='div_game-summary']//table[@class='floated stats_table']");

                //Ties and lead changes
                List<HtmlNode> statNodes = tableNodes[0].Descendants("tr").ToList();
                ties = int.Parse(statNodes[1].ChildNodes[1].InnerText);
                leadChanges = int.Parse(statNodes[2].ChildNodes[1].InnerText);

                //Consecutive Points
                statNodes = tableNodes[1].Descendants("tr").ToList();
                consecutivePoints = int.Parse(statNodes[1].ChildNodes[1].InnerText);
                consecutivePointsOpp = int.Parse(statNodes[2].ChildNodes[1].InnerText);
            }
            catch (Exception)
            {
                txtOutput.AppendText(Environment.NewLine + "Could not find data on ties, lead changes, and consecutive points.");
            }
            //Update game data
            gameData.Ties = ties;
            gameData.LeadChanges = leadChanges;
            gameData.Team1ConsecutivePoints = consecutivePoints;
            gameData.Team2ConsecutivePoints = consecutivePointsOpp;


            //-------------------------------
            //Find Play by Play data
            //-------------------------------
            doc = web.Load(url);
            List<HtmlNode> nodes = doc.DocumentNode.SelectSingleNode("//div[@id='all_pbp']//div[@class='table_outer_container']//div[@id='div_pbp']").Descendants("tr").ToList();

            int secondsToAddToQuarter = 3 * 60 * 12; //adjust the time each quarter to be time until end of game
            int currentQuarter = 1;

            PlayData startOfGame = new PlayData();
            startOfGame.Quarter = 1;
            startOfGame.SecondsSinceGameStart = 4 * 60 * 12;
            gameData.Plays.Add(startOfGame);

            foreach (HtmlNode node in nodes)
            {
                //Find team names if they are not found yet
                if (gameData.Team1Name == null)
                {
                    List<HtmlNode> headerNodes = node.Descendants("th").ToList();
                    if (headerNodes[0].InnerText == "Time")
                    {
                        gameData.Team1Name = headerNodes[1].InnerText;
                        gameData.Team2Name = headerNodes[5].InnerText;
                    }
                    continue;
                }

                //Find Play by Play Data
                List<HtmlNode> subNodes = node.Descendants("td").ToList();

                PlayData pd = new PlayData();
                TimeSpan tempTime = new TimeSpan();
                CultureInfo cultureBase = new CultureInfo("en-US");

                //Set the current quarter
                if (subNodes.Count > 0)
                {
                    {
                        if (subNodes[1].InnerText.Contains("1st quarter"))
                        {
                            currentQuarter = 1;
                        }
                        else if (subNodes[1].InnerText.Contains("2nd quarter"))
                        {
                            currentQuarter = 2;
                        }
                        else if (subNodes[1].InnerText.Contains("3rd quarter"))
                        {
                            currentQuarter = 3;
                        }
                        else if (subNodes[1].InnerText.Contains("4th quarter"))
                        {
                            currentQuarter = 4;
                        }
                        secondsToAddToQuarter = (4 - currentQuarter) * 60 * 12;
                    }
                }

                //Find the play-by-play data
                if (subNodes.Count > 3 && TimeSpan.TryParseExact(subNodes[0].InnerText, @"m\:ss\.f", cultureBase, out tempTime))
                {
                    //Time
                    pd.SecondsSinceGameStart = (int)(tempTime.TotalSeconds) + secondsToAddToQuarter;
                    pd.Quarter = currentQuarter;

                    //Actions
                    pd.Team1Action = subNodes[1].InnerText;
                    if (pd.Team1Action == "&nbsp;")
                    {
                        pd.Team1Action = "";
                    }
                    pd.Team2Action = subNodes[5].InnerText;
                    if (pd.Team2Action == "&nbsp;")
                    {
                        pd.Team2Action = "";
                    }

                    //Scoring
                    int i = 0;
                    int.TryParse(subNodes[2].InnerText, out i);
                    pd.Team1PlusPoints = i;
                    i = 0;
                    int.TryParse(subNodes[4].InnerText, out i);
                    pd.Team2PlusPoints = i;

                    //Current Score
                    string[] scores = subNodes[3].InnerText.Split('-');
                    if (int.TryParse(scores[0], out i))
                    {
                        pd.Team1Points = i;
                    }
                    if (int.TryParse(scores[1], out i))
                    {
                        pd.Team2Points = i;
                    }

                    //Add data to gameData
                    gameData.Plays.Add(pd);
                }
            }

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            ///
            /// CONVERT DATA INTO INTERVALS AND RANGES
            ///
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            const int MIN_RANGE_SIZE = 60 * 4; //4 minutes

            //Create Intervals (in groups of 15 seconds)
            TimeInterval lastTimeInterval = new TimeInterval();
            lastTimeInterval.SecondsSinceGameStart = 4 * 12 * 60;
            lastTimeInterval.Team1Points = 0;
            lastTimeInterval.Team2Points = 0;

            for (int n = 4 * 12 * 60; n >= 15; n -= 15)
            {
                PlayData p = gameData.Plays.FindLast(x => x.SecondsSinceGameStart > n - 15);

                TimeInterval t = new TimeInterval();
                t.Quarter = p.Quarter;
                t.SecondsSinceGameStart = n;
                t.Team1Points = p.Team1Points;
                t.Team2Points = p.Team2Points;
                t.PlusMinus = (t.Team1Points - lastTimeInterval.Team1Points) - (t.Team2Points - lastTimeInterval.Team2Points);

                lastTimeInterval = t;

                gameData.Intervals.Add(t);
            }

            //Create Ranges of two minutes in 15 second increments
            for (int n = 4 * 12 * 60; n >= MIN_RANGE_SIZE; n -= 15)
            {
                int startSeconds = n;
                int endSeconds = n - MIN_RANGE_SIZE;

                TimeRange r = new TimeRange();
                r.StartSeconds = startSeconds;
                r.EndSeconds = endSeconds;
                //Interval PlusMinus is always what occurs over the 15 seconds following the SecondsSinceGameStart.
                r.PlusMinus = gameData.Intervals.Where(x => x.SecondsSinceGameStart > endSeconds && x.SecondsSinceGameStart <= startSeconds).ToList().Sum(x => x.PlusMinus);

                gameData.TimeRanges.Add(r);
            }

            //Create Ranges of even minutes increments with no overlap
            for (int n = 4 * 12 * 60; n >= MIN_RANGE_SIZE; n -= MIN_RANGE_SIZE)
            {
                int startSeconds = n;
                int endSeconds = n - MIN_RANGE_SIZE;
                //Check for overtime games, since those last 5 minutes ruin the divisibility of the game length
                if (endSeconds < 0)
                {
                    endSeconds = 0;
                }
                TimeRange r = new TimeRange();
                r.StartSeconds = startSeconds;
                r.EndSeconds = endSeconds;
                //Interval PlusMinus is always what occurs over the 15 seconds following the SecondsSinceGameStart.
                r.PlusMinus = gameData.Intervals.Where(x => x.SecondsSinceGameStart > endSeconds && x.SecondsSinceGameStart <= startSeconds).ToList().Sum(x => x.PlusMinus);

                gameData.EvenRanges.Add(r);
            }

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            ///
            /// FIND HIGHEST VELOCITIES
            ///
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            //Clone the data
            List<TimeRange> tempRanges = new List<TimeRange>();
            tempRanges = gameData.TimeRanges.ToList();

            List<TimeInterval> intervals = new List<TimeInterval>();
            intervals = gameData.Intervals.ToList();

            //Create all large time ranges (> 2 min)
            do
            {
                //Find the highest remaining velocity
                double benchmarkVelocity = tempRanges.Max(x => x.AbsVelocity);
                TimeRange benchmarkRange = tempRanges.Find(x => x.AbsVelocity == benchmarkVelocity);

                /*
                //Search for any nearby velocity that's higher
                //If any is found, append it to the existing range and repeat the process
                List<TimeInterval> higherIntervals = new List<TimeInterval>();
                do
                {
                    higherIntervals = intervals.Where(x =>
                        x.AbsVelocity >= benchmarkVelocity && Math.Sign(x.Velocity) == Math.Sign(benchmarkRange.Velocity)
                        && (x.SecondsSinceGameStart == benchmarkRange.StartSeconds + 15 || x.SecondsSinceGameStart == benchmarkRange.EndSeconds)).ToList();

                    //Append those higher velocities to the benchmarkRange
                    foreach (TimeInterval i in higherIntervals)
                    {
                        benchmarkRange.PlusMinus += i.PlusMinus;
                        if (i.SecondsSinceGameStart < benchmarkRange.StartSeconds)
                        {
                            benchmarkRange.StartSeconds += 15;
                        }
                        else
                        {
                            benchmarkRange.EndSeconds -= 15;
                        }
                    }
                } while (higherIntervals.Count > 0);
                */
                
                //Add the benchmark range to the list of highest velocities
                gameData.HighestVelocityRanges.Add(benchmarkRange);

                //Remove any ranges which conflict with the benchmark range
                tempRanges = tempRanges.Where(x =>
                    !(x.StartSeconds <= benchmarkRange.StartSeconds && x.StartSeconds > benchmarkRange.EndSeconds) &&
                    !(x.EndSeconds < benchmarkRange.StartSeconds && x.EndSeconds >= benchmarkRange.EndSeconds)).ToList();

                //Remove any intervals which were included in the benchmarkRange
                intervals = intervals.Where(x => x.SecondsSinceGameStart > benchmarkRange.StartSeconds || x.SecondsSinceGameStart <= benchmarkRange.EndSeconds).ToList();

            } while (gameData.HighestVelocityRanges.Sum(x => x.Duration) <= 12 * 60 - MIN_RANGE_SIZE); //repeat until at least 12 minutes of time are accounted for

            //Expand the intervals until the full 12 minutes are filled
            //Do this by finding how much time is left to fill the 12 minutes, and then making intervals (in 15 second increments) of that length. Then just grab the largest.

            //Find all intervals not in one of the highest velocity ranges
            int targetSeconds = intervals.Max(x => x.SecondsSinceGameStart);
            do
            {
                TimeRange r = new TimeRange();
                r.StartSeconds = targetSeconds;
                r.PlusMinus = 0;
                foreach (TimeInterval i in intervals)
                {
                    if (i.SecondsSinceGameStart == targetSeconds)
                    {
                        targetSeconds -= 15;
                        r.PlusMinus += i.PlusMinus;
                    }
                }
                r.EndSeconds = targetSeconds;
                gameData.LowestVelocityRanges.Add(r);

                TimeInterval nextInterval = intervals.Find(x => x.SecondsSinceGameStart < targetSeconds);
                if (nextInterval != null)
                {
                    targetSeconds = nextInterval.SecondsSinceGameStart;
                }
                else
                {
                    targetSeconds = 0;
                }

            } while (targetSeconds > 0);

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            ///
            /// RETURN GAME DATA
            ///
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            if (targetTeam == "")
            {
                gameData.TargetTeam = gameData.Team1Name;
            }
            else
            {
                gameData.TargetTeam = targetTeam;
            }

            return gameData;

        }

        private bool exportGameData(GameData gameData, string exportPreface = "", string exportPath = "")
        {
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            ///
            /// EXPORT THE DATA
            ///
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            if (exportPath == "")
            {
                exportPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            }
            System.IO.Directory.CreateDirectory(exportPath);

            if (exportPreface == "")
            {
                exportPreface = "Game";
            }

            try
            {
                StringBuilder exportText = new StringBuilder();

                //Header text
                StringBuilder headerText = new StringBuilder();
                headerText.AppendLine(gameData.url);
                headerText.AppendLine("Largest Lead for " + gameData.Team1Name + ":," + gameData.LargestLead);
                headerText.AppendLine("Largest Deficit for " + gameData.Team1Name + ":," + gameData.LargestDeficit);
                headerText.AppendLine("Velocity is for a game, not a team, and is an absolute value calculation.");
                headerText.AppendLine("Velocity represents the point differential for a 48 minute game if the velocity was all one directional.");
                headerText.AppendLine("Velocity of highest velocity " + gameData.HighestVelocityRangesDuration + " seconds:," + gameData.AvgOfHighestVelocityRanges);
                headerText.AppendLine("Velocity of lower velocity " + gameData.LowestVelocityRangesDuration + " seconds:," + gameData.AvgOfLowestVelocityRanges);
                headerText.AppendLine("Velocity of game using even two-minute intervals (ABS):," + gameData.VelocityFullGame);
                headerText.AppendLine("PlusMinus of game for " + gameData.Team1Name + " (sum):," + gameData.Team1GamePlusMinus);

                //Export Play-by-play
                exportText.Clear();
                exportText.AppendLine(headerText.ToString());
                exportText.AppendLine("Quarter,SecondsSinceGameStart," + gameData.Team1Name + " Action,+,Points,Points,+," + gameData.Team2Name + " Action," + gameData.Team1Name + " ScoreDiff");
                foreach (PlayData p in gameData.Plays)
                {
                    exportText.AppendLine(p.ExportCSV());
                }
                System.IO.File.WriteAllText(exportPath + "\\" + exportPreface + " Plays.csv", exportText.ToString());

                //Export Time Intervals
                exportText.Clear();
                exportText.AppendLine(headerText.ToString());
                exportText.AppendLine("Quarter,SecondsSinceGameStart," + gameData.Team1Name + " Points," + gameData.Team2Name + " Points," + gameData.Team1Name + " ScoreDiff," + gameData.Team1Name + " PlusMinus");
                foreach (TimeInterval i in gameData.Intervals)
                {
                    exportText.AppendLine(i.ExportCSV());
                }
                System.IO.File.WriteAllText(exportPath + "\\" + exportPreface + " Intervals.csv", exportText.ToString());


                //Export Time Ranges
                exportText.Clear();
                exportText.AppendLine(headerText.ToString());
                exportText.AppendLine();
                exportText.AppendLine("Two-minute velocity ranges.");
                foreach (TimeRange r in gameData.EvenRanges)
                {
                    exportText.AppendLine(r.ExportCSV());
                }
                exportText.AppendLine();
                exportText.AppendLine("Highest velocity ranges.");
                foreach (TimeRange r in gameData.HighestVelocityRanges)
                {
                    exportText.AppendLine(r.ExportCSV());
                }
                exportText.AppendLine();
                exportText.AppendLine("Lowest velocity ranges.");
                foreach (TimeRange r in gameData.LowestVelocityRanges)
                {
                    exportText.AppendLine(r.ExportCSV());
                }
                System.IO.File.WriteAllText(exportPath + "\\" + exportPreface + " Velocities.csv", exportText.ToString());

                /*
                List<List<string>> table = doc.DocumentNode.SelectSingleNode("//table[id='pbp']")
                            .Descendants("tr")
                            .Skip(1)
                            .Where(tr => tr.Elements("td").Count() > 1)
                            .Select(tr => tr.Elements("td").Select(td => td.InnerText.Trim()).ToList())
                            .ToList();
                  */
            }
            catch (Exception e)
            {
                txtOutput.Text += Environment.NewLine + "One or more files failed to export. Error: " + e.Message;
                return false;
            }

            txtOutput.Text += Environment.NewLine + "Files exported to " + exportPath;

            return true;
        }
    }

    public class GameData
    {
        public string url { get; set; }

        public string TargetTeam { get; set; }
        public string Opponent { get { return (TargetTeam == Team1Name) ? Team2Name : Team1Name; } }

        public string Team1Name { get; set; }
        public string Team2Name { get; set; }

        public List<PlayData> Plays { get; set; }
        public List<TimeInterval> Intervals { get; set; }
        public List<TimeRange> TimeRanges { get; set; }
        public List<TimeRange> EvenRanges { get; set; } //TimeRanges of even two minute intervals

        public List<TimeRange> HighestVelocityRanges { get; set; }
        public List<TimeRange> LowestVelocityRanges { get; set; }

        public GameData()
        {
            Plays = new List<PlayData>();
            Intervals = new List<TimeInterval>();
            TimeRanges = new List<TimeRange>();
            EvenRanges = new List<TimeRange>();

            HighestVelocityRanges = new List<TimeRange>();
            LowestVelocityRanges = new List<TimeRange>();
        }

        public int Ties { get; set; }
        public int LeadChanges { get; set; }
        public int Team1ConsecutivePoints { get; set; }
        public int Team2ConsecutivePoints { get; set; }

        public int LargestLead { get { return Plays.Max(x => x.ScoreDiff); } }
        public int LargestDeficit { get { return Plays.Min(x => x.ScoreDiff); } }

        public int Team1GamePlusMinus { get { return Plays.Last().ScoreDiff; } }
        public int Team2GamePlusMinus { get { return -1 * Plays.Last().ScoreDiff; } }

        public double VelocityFullGame { get { return EvenRanges.Sum(x => Math.Abs(x.PlusMinus)); } } //Average of the absolute value of the velocity of even two-minute intervals for the full game

        public int HighestVelocityRangesDuration { get { return HighestVelocityRanges.Sum(x => x.Duration); } }
        public int LowestVelocityRangesDuration { get { return LowestVelocityRanges.Sum(x => x.Duration); } }

        public double AvgOfHighestVelocityRanges { get { return HighestVelocityRanges.Sum(x => Math.Abs(x.PlusMinus)) * 48.0 * 60.0 / HighestVelocityRangesDuration; } }
        public double AvgOfLowestVelocityRanges { get { return LowestVelocityRanges.Sum(x => Math.Abs(x.PlusMinus)) * 48.0 * 60.0 / LowestVelocityRangesDuration; } }

        public double VelocityHighOverLow { get
            {
                if (AvgOfLowestVelocityRanges <= 4)
                {
                    return AvgOfHighestVelocityRanges;
                }
                else
                {
                    return AvgOfHighestVelocityRanges / AvgOfLowestVelocityRanges;
                }
            } }
        public double VelocityHighOverFull { get { return AvgOfHighestVelocityRanges / VelocityFullGame; } }
        public double VelocityHighOverMargin { get { return AvgOfHighestVelocityRanges / Math.Abs(Team1GamePlusMinus); } }
        public double VelocityHighOverPoints { get { return AvgOfHighestVelocityRanges / (Plays.Last().Team1Points + Plays.Last().Team2Points); } }

        public int TargetConsecutivePoints { get { return (TargetTeam == Team1Name) ? Team1ConsecutivePoints : Team2ConsecutivePoints; } }
        public int TargetConsecutivePointsOpp { get { return (TargetTeam == Team1Name) ? Team2ConsecutivePoints : Team1ConsecutivePoints; } }
        public int TargetLargestLead { get { return (TargetTeam == Team1Name) ? LargestLead : LargestDeficit * -1; } }
        public int TargetLargestDeficit { get { return (TargetTeam == Team1Name) ? LargestDeficit : LargestLead * -1; } }
        public int TargetMarginOfVictory { get { return (TargetTeam == Team1Name) ? Team1GamePlusMinus : Team2GamePlusMinus; } }
        public int TargetSpread { get { return TargetLargestLead - TargetLargestDeficit; } }
        public int TargetPointsFor { get { return (TargetTeam == Team1Name) ? Plays.Last().Team1Points : Plays.Last().Team2Points; } }
        public int TargetPointsAgainst { get { return (TargetTeam == Team1Name) ? Plays.Last().Team2Points : Plays.Last().Team1Points; } }
    }

    public class PlayData
    {
        public int Quarter { get; set; }
        public int SecondsSinceGameStart { get; set; }
        public int Team1Points { get; set; }
        public int Team2Points { get; set; }
        public string Team1Action { get; set; }
        public string Team2Action { get; set; }
        public int Team1PlusPoints { get; set; } //increase in score for Team 1 for the play
        public int Team2PlusPoints { get; set; }
        public int ScoreDiff { get { return Team1Points - Team2Points; } } //for Team 1 (just take the inverse sign for Team 2)

        public string ExportCSV()
        {
            return Quarter + "," + SecondsSinceGameStart + "," + Team1Action + "," + Team1PlusPoints + "," + Team1Points + "," + Team2Points + "," + Team2PlusPoints + "," + Team2Action + "," + ScoreDiff;
        }
    }

    public class TimeInterval
    {
        //Always in 15 second intervals
        public int Quarter { get; set; }
        public int SecondsSinceGameStart { get; set; }
        public int Team1Points { get; set; }
        public int Team2Points { get; set; }
        public int PlusMinus { get; set; } //for Team 1 (just take the inverse sign for Team 2)
        public int ScoreDiff { get { return Team1Points - Team2Points; } } //for Team 1 (just take the inverse sign for Team 2)
        public int AbsScoreDiff { get { return Math.Abs(Team1Points - Team2Points); } }
        public double Velocity { get { return 48 * 60 * PlusMinus / 15.0; } } //return the velocity for the time range
        public double AbsVelocity { get { return Math.Abs(Velocity); } }

        public string ExportCSV()
        {
            return Quarter + "," + SecondsSinceGameStart + "," + Team1Points + "," + Team2Points + "," + ScoreDiff + "," + PlusMinus;
        }
    }

    public class TimeRange
    {
        public int StartSeconds { get; set; }
        public int EndSeconds { get; set; }
        public int Duration { get { return StartSeconds - EndSeconds; } }
        public int PlusMinus { get; set; } //for Team 1 (just take the inverse sign for Team 2)
        public double Velocity { get { return 48 * 60 * PlusMinus * 1.0 / (StartSeconds - EndSeconds); } } //return the velocity for the time range
        public double AbsVelocity { get { return Math.Abs(Velocity); } }

        public string ExportCSV()
        {
            return StartSeconds + "," + EndSeconds + "," + PlusMinus + "," + Velocity.ToString("0.0000");
        }
    }
}
