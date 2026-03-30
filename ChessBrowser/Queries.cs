using Microsoft.Maui.Controls;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessBrowser
{
  internal class Queries
  {

    /// <summary>
    /// This function runs when the upload button is pressed.
    /// Given a filename, parses the PGN file, and uploads
    /// each chess game to the user's database.
    /// </summary>
    /// <param name="PGNfilename">The path to the PGN file</param>
    internal static async Task InsertGameData(string PGNfilename, MainPage mainPage)
    {
      // This will build a connection string to your user's database on atr,
      // assuimg you've typed a user and password in the GUI
      string connection = mainPage.GetConnectionString();
      // using this connection string, we will get into the mysql database
      using (MySqlConnection conn = new MySqlConnection(connection))
      {
        try
        {
          // Open a connection
          conn.Open();

          // Take in teh kb#.pgn file and make a List of chessGame objects
          List<ChessGame> games = PgnReader.ReadGames(PGNfilename);
          // Provide the length of the list of games to the UI for loading bar
          mainPage.SetNumWorkItems(games.Count);

          foreach (ChessGame game in games)
          {
            // if there is an error lets catch it, client shouldnt recieve the error - move the logic to server side
            try
            {
              // insert or update white player. -- Use Duplicate: if the name is the same then update it but only if the new elo is greater than the other prior
              // MySqlCommand cmd = new MySqlCommand(@"
              //     INSERT INTO Players (Name, Elo)
              //     VALUES (@name, @elo) 
              //     ON DUPLICATE KEY UPDATE Elo = GREATEST(Elo, @elo);", conn);
              //     //@val/@name are placeholders to prevent sql attacks @identifies placeholder
              // cmd.Parameters.AddWithValue("@name", game.White);  //assign value to placeholder
              // cmd.Parameters.AddWithValue("@elo", game.WhiteElo);
              // // executes the SQL command asynchronously without blocking the UI thread,
              // // awaiting this task ensures the command finishes before continuing the method,
              // // but other parts of the app (- the GUI) are still avaiable for use to make new selections or to interact with
              // await cmd.ExecuteNonQueryAsync();

              // // optional to not create another cmd and use foreach loop here (cmd.Parameters["@name"].Value = name;) -- create arraylist of players
              // cmd = new MySqlCommand(@"
              //     INSERT INTO Players (Name, Elo)
              //     VALUES (@name, @elo)
              //     ON DUPLICATE KEY UPDATE Elo = GREATEST(Elo, @elo);", conn);
              // cmd.Parameters.AddWithValue("@name", game.Black);
              // cmd.Parameters.AddWithValue("@elo", game.BlackElo);
              // await cmd.ExecuteNonQueryAsync();

              // insert or update players - Use Duplicate: if the name is the same then update it but only if the new elo is greater than the other prior
              var cmd = new MySqlCommand(@"
                    INSERT INTO Players (Name, Elo)
                    VALUES (@name, @elo)
                    ON DUPLICATE KEY UPDATE Elo = GREATEST(Elo, @elo);", conn);
              // @val/@name are placeholders to prevent sql attacks @identifies placeholder
              cmd.Parameters.Add("@name", MySqlDbType.VarChar); 
              cmd.Parameters.Add("@elo", MySqlDbType.Int32);

              (string, int)[] players = { (game.White, game.WhiteElo), (game.Black, game.BlackElo) };

              foreach (var (name, elo) in players)
              {
                cmd.Parameters["@name"].Value = name;
                cmd.Parameters["@elo"].Value = elo;
                await cmd.ExecuteNonQueryAsync();
              }


              // insert event - Use INSERT IGNORE to skip the insert if the insert violates a pk or unique(if a duplicate--> ignore it and continue )- speed up the process 
              cmd = new MySqlCommand(@"
                  INSERT IGNORE INTO Events (Name, Site, Date)
                  VALUES (@name, @site, @date);", conn);
              cmd.Parameters.AddWithValue("@name", game.Event);
              cmd.Parameters.AddWithValue("@site", game.Site);
              cmd.Parameters.AddWithValue("@date", game.EventDate);
              await cmd.ExecuteNonQueryAsync();

              // get EID from current event
              cmd = new MySqlCommand("SELECT eID FROM Events WHERE Name = @name AND Site = @site AND Date = @date;", conn);
              cmd.Parameters.AddWithValue("@name", game.Event);
              cmd.Parameters.AddWithValue("@site", game.Site);
              cmd.Parameters.AddWithValue("@date", game.EventDate);
              int eventID = Convert.ToInt32(await cmd.ExecuteScalarAsync());

              // get players
              int whitePlayerID = GetPlayerID(conn, game.White);
              int blackPlayerID = GetPlayerID(conn, game.Black);
              // insert game 
              cmd = new MySqlCommand(@"
                  INSERT INTO Games (eID, WhitePlayer, BlackPlayer, Result, Moves, Round)
                  VALUES (@eID, @WhitePlayer, @BlackPlayer, @Result, @Moves, @Round);", conn);
              cmd.Parameters.AddWithValue("@eID", eventID);
              cmd.Parameters.AddWithValue("@WhitePlayer", whitePlayerID);
              cmd.Parameters.AddWithValue("@BlackPlayer", blackPlayerID);
              cmd.Parameters.AddWithValue("@Result", game.Result);
              cmd.Parameters.AddWithValue("@Moves", game.Moves);
              cmd.Parameters.AddWithValue("@Round", game.Round);

              await cmd.ExecuteNonQueryAsync();
              // Console.WriteLine("Inserted Game: " + game.White + " vs " + game.Black);
              // Console.WriteLine("Inserted Game: " + whitePlayerID + " vs " + blackPlayerID);


              await mainPage.NotifyWorkItemCompleted();
            }
            catch (Exception ex)
            {
              Console.WriteLine($"Error inserting game: {game.White} vs {game.Black}: look at 124 Queries.cs");
              Console.WriteLine(ex.Message);
            }
          }

        }
        catch (Exception e)
        {
          Console.WriteLine(e.Message);
        }
      }

    }
    private static int GetPlayerID(MySqlConnection conn, string playerName)
    {
      // we need to actually get pID for the (black and white) players
      var cmd = new MySqlCommand("SELECT pID FROM Players WHERE Name = @name;", conn);
      cmd.Parameters.AddWithValue("@name", playerName);
      var result = cmd.ExecuteScalar();
      // if we dont have players update
      if (result == null)
      {
        var insertCmd = new MySqlCommand("INSERT INTO Players (Name) VALUES (@name); SELECT LAST_INSERT_ID();", conn);
        insertCmd.Parameters.AddWithValue("@name", playerName);
        return Convert.ToInt32(insertCmd.ExecuteScalar());
      }
      return Convert.ToInt32(result);
    }


    /// <summary>
    /// Queries the database for games that match all the given filters.
    /// The filters are taken from the various controls in the GUI.
    /// </summary>
    /// <param name="white">The white player, or null if none</param>
    /// <param name="black">The black player, or null if none</param>
    /// <param name="opening">The first move, e.g. "1.e4", or null if none</param>
    /// <param name="winner">The winner as "W", "B", "D", or null if none</param>
    /// <param name="useDate">True if the filter includes a date range, False otherwise</param>
    /// <param name="start">The start of the date range</param>
    /// <param name="end">The end of the date range</param>
    /// <param name="showMoves">True if the returned data should include the PGN moves</param>
    /// <returns>A string separated by newlines containing the filtered games</returns>
    internal static string PerformQuery(string white, string black, string opening,
      string winner, bool useDate, DateTime start, DateTime end, bool showMoves,
      MainPage mainPage)
    {
      // This will build a connection string to your user's database on atr,
      // assuimg you've typed a user and password in the GUI
      string connection = mainPage.GetConnectionString();

      // Build up this string containing the results from your query
      string parsedResult = "";

      // Use this to count the number of rows returned by your query
      // (see below return statement)
      int numResults = 0;

      using (MySqlConnection conn = new MySqlConnection(connection))
      {
        try
        {
          // Open a connection
          conn.Open();

          List<string> conditions = new();
          MySqlCommand cmd = new MySqlCommand();
          cmd.Connection = conn;

          if (!string.IsNullOrWhiteSpace(white))
          {
            conditions.Add("P1.Name = @white");
            cmd.Parameters.AddWithValue("@white", white);
          }
          if (!string.IsNullOrWhiteSpace(black))
          {
            conditions.Add("P2.Name = @black");
            cmd.Parameters.AddWithValue("@black", black);
          }
          if (!string.IsNullOrWhiteSpace(winner))
          {
            conditions.Add("G.Result = @winner");
            cmd.Parameters.AddWithValue("@winner", winner);
          }
          if (!string.IsNullOrWhiteSpace(opening))
          {
            conditions.Add("G.Moves LIKE @opening");
            cmd.Parameters.AddWithValue("@opening", opening + "%");
          }
          if (useDate)
          {
            conditions.Add("E.Date BETWEEN @start AND @end");
            cmd.Parameters.AddWithValue("@start", start);
            cmd.Parameters.AddWithValue("@end", end);
            Console.WriteLine($"Start: {start.ToShortDateString()}, End: {end.ToShortDateString()}");

          }
          // build the SQL query to be executed onclick of the Go button
          // get the all features from the database
          
          string selectSpecifications = @"
              SELECT E.Name AS eName, E.Site, E.Date,
                    P1.Name AS WhitePlayer, P1.Elo AS WhiteElo,
                    P2.Name AS BlackPlayer, P2.Elo AS BlackElo,
                    G.Result" +
                    // if the user has selected to show moves, then add the Moves column to the select
                    (showMoves ? ", G.Moves" : "") + @"
              FROM Games G
              JOIN Events E ON G.eID = E.eID
              JOIN Players P1 ON G.WhitePlayer = P1.pID
              JOIN Players P2 ON G.BlackPlayer = P2.pID";
          // if there are any conditions, add a WHERE specifications to the query
          string whereSpecifications = conditions.Count > 0
              ? " WHERE " + string.Join(" AND ", conditions)
              : "";

          cmd.CommandText = selectSpecifications + whereSpecifications;
          cmd.Prepare();

          using MySqlDataReader reader = cmd.ExecuteReader();
          // build the string ParsedResult += reader Name end with \n
          while (reader.Read())
          {
            numResults++;
            parsedResult += $"Event: {reader["eName"]}\n";
            parsedResult += $"Site: {reader["Site"]}\n";
            parsedResult += $"Date: {Convert.ToDateTime(reader["Date"]).ToShortDateString()}\n";
            parsedResult += $"White: {reader["WhitePlayer"]} ({reader["WhiteElo"]})\n";
            parsedResult += $"Black: {reader["BlackPlayer"]} ({reader["BlackElo"]})\n";
            parsedResult += $"Result: {reader["Result"]}\n";
            if (showMoves)
              parsedResult += $"Moves: {reader["Moves"]}\n";
            parsedResult += "\n";
          }

        }
        catch (Exception e)
        {
          Console.WriteLine(e.Message);
        }
      }
      //return for Onclicked
      return numResults + " results\n" + parsedResult;
    }

  }
}
