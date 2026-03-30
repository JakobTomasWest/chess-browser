using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.IO;
using System.Text.RegularExpressions;

public static class PgnReader
{
    // read the pgn and and turn the files lines into a list of chess game objects
    public static List<ChessGame> ReadGames(string filePath)
    {

        var games = new List<ChessGame>();
        var lines = File.ReadAllLines(filePath);
        var tagData = new Dictionary<string, string>();
        var movesBuilder = new List<string>();
        //for each line that we are reading from the Pgn; Lines, lets first see if the line is whitespace, if there is an empty line lets make sure there are tags or moves after it

        // and parse them to create our list of games
        foreach (var line in lines)
        {   
            // once we hit a blank line, we know that we have reached the end of a game entry so wecan parse the tags and moves into a ChessGame object
            if (string.IsNullOrWhiteSpace(line))
            {
                if (tagData.Count > 0 && movesBuilder.Count > 0)
                {
                    try
                    {
                        // if we have a tag and moves, parse them into a game object using the ParseGame method
                        // and add it to the games list
                        games.Add(ParseGame(tagData, movesBuilder));
                        // Console.WriteLine($"Parsed game: {CleanTag(tagData, "White")} vs {CleanTag(tagData, "Black")}");

                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine("Skipping the invalid ChessGame Entry " + exception.Message);

                    }
                    // clear the tag and moves so that we can start fresh for the next game
                    tagData.Clear();
                    movesBuilder.Clear();
                }
                continue;
            }
            // deconstruct line and parse ------- (.*) captures everything inside the quotes for the tag value 
            if (line.StartsWith("["))
            //^ we got a valid entry for tag 
            {
                // \w+ matches one or more word characters like "Event" or "Site"
                // \s lends us the whitespace between the tag name and the value
                // and ""(.*)"" gives us "Australia Open 2019" or "Melbourne" etc
                var match = Regex.Match(line, @"\[(\w+)\s+""(.*)""\]");
                if (match.Success)
                {
                    tagData[match.Groups[1].Value] = match.Groups[2].Value;
                }
            }
            else
            {
                movesBuilder.Add(line);
            }

        } 
        // make sure to get last game even if there is no blank line at the end of the file 
        if (tagData.Count > 0 && movesBuilder.Count > 0)
        {
            try
            {
                games.Add(ParseGame(tagData, movesBuilder));
                // Console.WriteLine($"Parsed game: {CleanTag(tagData, "White")} vs {CleanTag(tagData, "Black")}");

            }
            catch (Exception ex)
            {
                Console.WriteLine("Skipping invalid game: " + ex.Message);
            }
        }
        return games;
    }


    // given a dictionary of tags and a list of moves, parse them into a cleaned up ChessGame object. 
    private static ChessGame ParseGame(Dictionary<string, string> tags, List<string> moves)
    {
        return new ChessGame
        {
            Event = CleanTag(tags, "Event"),
            Site = CleanTag(tags, "Site"),
            Round = CleanTag(tags, "Round"),
            
            White = CleanTag(tags, "White"),
            Black = CleanTag(tags, "Black"),
            WhiteElo = ParseInt(CleanTag(tags, "WhiteElo")),
            BlackElo = ParseInt(CleanTag(tags, "BlackElo")),
            EventDate = ParseDate(CleanTag(tags, "EventDate")),
            
            Result = ConvertResult(CleanTag(tags, "Result")),
            Moves = string.Join(" ", moves).Trim() 
        };
    }

    private static string CleanTag(Dictionary<string, string> tags, string key)
    {
        // try to get the value associated with the given key in the tags dictionary.
        // for example, if key is "Event" and tags contains ["Event" => "Australia Open 2019"], it will return "Australia Open 2019".
        // if the key is not found in Tags, return an empty string instead of throwing an error.

        if (tags.TryGetValue(key, out string value))
            return value;
        else
            return "";
    }

    private static int ParseInt(string value)
    {
        //parse string, if it's an int then return it as the result, if not return 0 for things like null or ???
        return int.TryParse(value, out int result) ? result : 0;
    }

    private static string ConvertResult(string result)
    {
        return result switch
        {
            "1-0" => "W",
            "0-1" => "B",
            "1/2-1/2" => "D",
            _ => "?"
        };
    }

    private static DateTime ParseDate(string rawDate)
    {
        // handle dates like "2007.??.??"
        try
        {
            var cleaned = rawDate.Replace("??", "01").Replace('.', '-');
            return DateTime.Parse(cleaned);
        }
        catch
        {
            return DateTime.MinValue;
        }
    }
}
