# Chess Browser

A cross-platform C# MAUI application for parsing, storing, and querying chess games from PGN files using a MySQL database.

## Features

- Upload and parse PGN files
- Store chess games in a relational database
- Query games by:
  - Player name
  - Game result
  - Opening prefix
  - Date range
- Interactive UI for filtering results

## Tech Stack

- C# / .NET MAUI
- MySQL
- SQL (parameterized queries)
- PGN parsing

## How It Works

1. Upload a PGN file
2. The app parses games into structured objects
3. Data is inserted into a MySQL database
4. Users can query and filter games through the UI

## Setup

1. Clone the repo
2. Open in Visual Studio or VS Code
3. Configure your MySQL connection string
4. Run the application

