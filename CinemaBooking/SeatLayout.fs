module SeatLayout

open System
open Models
open Repository

// ================================
// Get Seat Layout for Screening
// ================================

/// Get flat seat list for a screening (from Repository)
let getSeatsForScreeningLayout (screeningId: int) : SeatView list =
    getSeatsForScreening screeningId

/// Convert flat seat list into grid (rows -> seats)
let buildSeatGrid (seats: SeatView list) : SeatView list list =
    seats
    |> List.groupBy (fun s -> s.RowNumber)
    |> List.map (fun (_, rowSeats) ->
        rowSeats |> List.sortBy (fun s -> s.SeatNumber))
    |> List.sortBy (fun row -> row.Head.RowNumber)

/// Convenience function
let getSeatLayout (screeningId: int) : SeatView list list =
    screeningId
    |> getSeatsForScreeningLayout
    |> buildSeatGrid

// ================================
// Console Preview (Testing)
// ================================

/// [ ] available
/// [X] reserved
let printSeatLayout (grid: SeatView list list) =
    for row in grid do
        Console.Write(sprintf "Row %2d: " row.Head.RowNumber)

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
