# NBA-Downloader
Download play-by-play data directly from basketball-reference.com.
Also includes some statistical data for a school project I was working on. You can ignore that information because it probably won't make sense to you, and this document won't attempt to explain it.

Basic instructions are included in the program.

To download play-by-play data for a single game:
Enter a game URL in the "Website URL" field and click "Download Single Game Play by Play" button.
The Website URL should be in the format https://www.basketball-reference.com/boxscores/201804150BOS.html.
The data will download to a .csv in the same folder as the executable. Download location cannot bechanged.

To download play-by-play data for an entire season:
Enter a team page URL in the "Website URL" field and the team location (such as "Minnesota" or "Boston") into the Team Location field. Then click "Download Full Season Play by Play for a Team". Entering the Team Location is critical because basketball-reference does not have a consistent naming scheme for team location (the Lakers, for example are "LA Lakers" rather than "Los Angeles", while the Celtics are simply "Boston") and there is nothing on the team page which denotes the naming scheme. Please look at boxscore data to find the appropriate Team Location.
The Website URL should be in the format https://www.basketball-reference.com/teams/BOS/2018.html.
The data will download .csvs of games.

Results of the download will show in the scroll field on the bottom of the window. Results are purely cosmetic.
