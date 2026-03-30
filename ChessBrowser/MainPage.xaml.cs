namespace ChessBrowser;

public partial class MainPage : ContentPage
{
  private int numWorkItems = 0;
  private int workItemsCompleted = 0;

  public MainPage()
  {
    InitializeComponent();
    // System.Diagnostics.Debug.WriteLine("MainPage constructor hit!");
	}

  /// <summary>
  /// Handler for the upload button.
  /// Picks a file and passes it to the database controller
  /// </summary>
  /// <param name="sender"></param>
  /// <param name="e"></param>
  private async void OnFileUpload(object sender, EventArgs e)
{
    try
    {
        FileResult fileResult = await FilePicker.Default.PickAsync();
        if (fileResult != null)
        {
            string filePath = fileResult.FullPath;

            
            List<ChessGame> games = PgnReader.ReadGames(filePath);

            if (games.Count > 0)
            {
                ChessGame g = games[0];
                outputText.Text =
                    $"Loaded {games.Count} games\n\n" +
                    $"First Game:\n" +
                    $"Event: {g.Event}\n" +
                    $"White: {g.White} ({g.WhiteElo})\n" +
                    $"Black: {g.Black} ({g.BlackElo})\n" +
                    $"Result: {g.Result}\n" +
                    $"Date: {g.EventDate.ToShortDateString()}\n" +
                    $"Moves: {(g.Moves.Length > 300 ? g.Moves.Substring(0, 300) + "..." : g.Moves)}";
            }
            else
            {
                outputText.Text = "No games found in file.";
            }

            // this will insert the game data into the database
            await Queries.InsertGameData(filePath, this);
        }
    }
    catch (Exception ex)
    {
        outputText.Text = $"Error (in OnFileUpload): {ex.Message}";
        Console.WriteLine(ex);
    }
}


  /// <summary>
  /// Handler for the go button.
  /// Passes the query parameters to the database controller and displays the result
  /// </summary>
  /// <param name="sender"></param>
  /// <param name="e"></param>
  private void OnGoClicked( object sender, EventArgs e )
  {
    Console.WriteLine("Go button clicked!");
    

    string winner = null;
    if ( whiteWon.IsChecked )
    {
      winner = "W";
    }
    else if ( blackWon.IsChecked )
    {
      winner = "B";
    }
    else if ( drawGame.IsChecked )
    {
      winner = "D";
    }
    string wp = whiteplayer.Text == "" ? null : whiteplayer.Text;
    string bp = blackplayer.Text == "" ? null : blackplayer.Text;
    string open = openingmove.Text == "" ? null : openingmove.Text;
    var res = Queries.PerformQuery(wp, bp, open,
            winner, filterByDate.IsChecked, startDate.Date, endDate.Date,
            showMoves.IsChecked, this);
    // Console.WriteLine("Query performed, result: " + res);
    outputText.Text = res;
  }

  /// <summary>
  /// Tell the progress bar how many "work items" you're going to perform
  /// </summary>
  /// <param name="numItems"></param>
  public void SetNumWorkItems( int numItems )
  {
    numWorkItems = numItems;
    workItemsCompleted = 0;
  }

  /// <summary>
  /// Tell the progress bar that you've completed one of the "work items"
  /// so it will update the bar
  /// </summary>
  public async Task NotifyWorkItemCompleted()
  {
    workItemsCompleted++;
    double newProgress = ((double)workItemsCompleted) / numWorkItems;
    await progressbar.ProgressTo( newProgress, 1, Easing.Linear ).ContinueWith( ( res ) => { } );
  }

  /// <summary>
  /// Returns a mysql connection string using the inputs entered for username and password
  /// </summary>
  /// <returns></returns>
  internal string GetConnectionString()
  {
    return "server=cs-db.eng.utah.edu;database=u0675702" + ";uid=" + username.Text + ";password=" + password.Text;
  }
}

