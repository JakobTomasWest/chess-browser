using System;
using System.Dynamic;

public class ChessGame
{
    // the first 3 are essentially eid
    public string Event { get; set; } //Events.Name in mysql
    public string Site { get; set; } //Events.Site 
    public DateTime EventDate { get; set; } //Events.Date

    public string Round { get; set; } //Games.Round
    public string Result { get; set; } // Games.Result
    public string Moves { get; set; } //Games.Moves

    //white player and black player reference Players pID in sql
    //eID referenes eID in Events and that is the same as the top 3 as events pk

    public string White { get; set; } //Players.Name for white 
    public int WhiteElo { get; set; } //Players.Elo for white
    public string Black { get; set; } //Players.Name for black
    public int BlackElo { get; set; } // Players.Elo for black

}