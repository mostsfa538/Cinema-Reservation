module Database
open Microsoft.Data.Sqlite


let connectionString = "Data Source=cinema.db"

let getConnection() =
    new SqliteConnection(connectionString)

let initializeDatabase() =
    use connection = getConnection()
    connection.Open()
    

    let createUsersTable = """
        CREATE TABLE IF NOT EXISTS Users (
            UserId INTEGER PRIMARY KEY AUTOINCREMENT,
            Username TEXT NOT NULL UNIQUE,
            Password TEXT NOT NULL
        )
    """
    
    let createSeatsTable = """
        CREATE TABLE IF NOT EXISTS Seats (
            SeatId INTEGER PRIMARY KEY AUTOINCREMENT,
            RowNumber INTEGER NOT NULL,
            SeatNumber INTEGER NOT NULL,
            IsReserved INTEGER NOT NULL DEFAULT 0,
            UNIQUE(RowNumber, SeatNumber)
        )
    """


    let createTicketsTable = """
        CREATE TABLE IF NOT EXISTS Tickets (
            TicketId TEXT PRIMARY KEY,
            SeatId INTEGER NOT NULL,
            UserId INTEGER NOT NULL,
            FOREIGN KEY (SeatId) REFERENCES Seats(SeatId),
            FOREIGN KEY (UserId) REFERENCES Users(UserId)
        )
    """
    
    use cmd = new SqliteCommand(createUsersTable, connection)
    cmd.ExecuteNonQuery() |> ignore
    
    use cmd2 = new SqliteCommand(createSeatsTable, connection)
    cmd2.ExecuteNonQuery() |> ignore
    
    use cmd3 = new SqliteCommand(createTicketsTable, connection)
    cmd3.ExecuteNonQuery() |> ignore
    

    printfn "Database Created Successfully"

