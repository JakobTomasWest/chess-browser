# Chess Browser

Chess Browser is a cross-platform C# .NET MAUI application for importing chess games from PGN files, storing them in a MySQL database, and querying them through a desktop-style user interface.

## Why This Project Matters

Chess data is commonly stored in PGN text files, which are portable but not ideal for fast filtering or structured analysis. This project converts raw PGN game data into a relational database so users can search games by player, result, opening prefix, and date range more efficiently.

## Features

- Import and parse chess games from PGN files
- Store parsed games in a MySQL relational database
- Query games by:
  - player name
  - result
  - opening prefix
  - date range
- Interactive UI built with .NET MAUI
- Parameterized SQL queries for safer database interaction

## Tech Stack

- C#
- .NET MAUI
- MySQL
- SQL
- PGN parsing

## How It Works

1. The user selects a PGN file containing chess games.
2. The application parses each game into structured C# objects.
3. Parsed game data is inserted into relational database tables.
4. The user can search stored games through the interface using multiple filters.

## Project Structure

- `ChessBrowser/ChessGame.cs` — model representing a chess game
- `ChessBrowser/PgnReader.cs` — PGN parsing logic
- `ChessBrowser/Queries.cs` — database insert and query logic
- `ChessBrowser/MainPage.xaml` — UI layout
- `ChessBrowser/MainPage.xaml.cs` — UI event handling and app flow

## Setup

1. Clone the repository.
2. Open the solution in Visual Studio.
3. Configure access to a MySQL database.
4. Build and run the project.
5. Import a sample PGN file from `ChessBrowser/sample-data/`.

## Notes

This project was originally developed as part of coursework in the University of Utah Master of Software Development program and was later cleaned up for portfolio presentation.

## Future Improvements

- Add screenshots of the interface
- Support richer filtering and sorting options
- Improve validation and error handling
- Add database schema documentation