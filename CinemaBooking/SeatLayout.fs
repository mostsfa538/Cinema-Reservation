module SeatLayout

open System
open Microsoft.Data.Sqlite
open Models
open Database
open BookingFunctions

/// Create seats in-memory (SeatId = 0 because DB will assign AUTOINCREMENT on insert)
let generateSeatLayout (rows:int) (cols:int) : Seat list =
    [ for r in 1..rows do
        for c in 1..cols do
            { SeatId = 0
              RowNumber = r
              SeatNumber = c
              IsReserved = false } ]

/// Seed seats to DB only if Seats table is empty.
/// This uses Database.getConnection() from Database.fs
let seedSeatsIfEmpty (rows:int) (cols:int) =
    use conn = getConnection ()
    conn.Open()

    // check if any seats exist
    use checkCmd = new SqliteCommand("SELECT COUNT(*) FROM Seats", conn)
    let count = checkCmd.ExecuteScalar() :?> int64 |> int

    if count = 0 then
        // Insert all seats
        let insertSql = "INSERT INTO Seats (RowNumber, SeatNumber, IsReserved) VALUES (@row, @seat, 0)"
        use tran = conn.BeginTransaction()
        use cmd = new SqliteCommand(insertSql, conn, tran)
        cmd.Parameters.Add("@row", Microsoft.Data.Sqlite.SqliteType.Integer) |> ignore
        cmd.Parameters.Add("@seat", Microsoft.Data.Sqlite.SqliteType.Integer) |> ignore

        for r in 1..rows do
            for c in 1..cols do
                cmd.Parameters.["@row"].Value <- r
                cmd.Parameters.["@seat"].Value <- c
                cmd.ExecuteNonQuery() |> ignore

        tran.Commit()
        printfn "Seeded %dx%d seats into DB." rows cols
    else
        printfn "Seats already exist in database (%d seats). No seeding performed." count

/// Get seat list from DB and return as a list
/// This simply wraps the existing getAllSeats() from BookingFunctions
let getSeatsFromDb () : Seat list =
    getAllSeats ()

/// Convert a flat seat list into a grid (list of rows, each row is list of seats sorted by seat number)
let getSeatGrid (seats: Seat list) : Seat list list =
    seats
    |> List.groupBy (fun s -> s.RowNumber)
    |> List.map (fun (rowNum, rowSeats) -> rowSeats |> List.sortBy (fun s -> s.SeatNumber))
    |> List.sortBy (fun row -> row.Head.RowNumber)

/// Convenience: fetch grid directly from DB
let getSeatLayout ( ) : Seat list list =
    getSeatsFromDb () |> getSeatGrid

/// Console preview of the grid — useful for quick testing.
/// Symbols: [ ] = available, [X] = reserved
/// Colors: green for available, red for reserved (console).
let printSeatGridToConsole (grid: Seat list list) =
    for row in grid do
        // print row number at start
        let rowNumber = row.Head.RowNumber
        Console.Write(sprintf "Row %2d: " rowNumber)
        for seat in row do
            if seat.IsReserved then
                Console.ForegroundColor <- ConsoleColor.Red
                Console.Write("[X] ")
            else
                Console.ForegroundColor <- ConsoleColor.Green
                Console.Write("[ ] ")
            Console.ResetColor()
        Console.WriteLine()
    Console.WriteLine()

/// Find a seat by row and seat number using existing DB function
let findSeatInDb row seatNumber =
    findSeatByCoordinates row seatNumber


