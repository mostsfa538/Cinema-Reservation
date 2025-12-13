module ScreeningEngine

open System
open Models
open Repository

/// Get available screenings (future only)
let getUpcomingScreenings () =
    getAllScreenings ()
    |> List.filter (fun s -> s.StartTime > DateTime.Now)

/// Get screening details with seats
let getScreeningSeats (screeningId: int) =
    getSeatsForScreening screeningId

/// Check if screening is valid for booking
let canBook screeningId =
    match getScreeningById screeningId with
    | None -> false
    | Some s -> s.StartTime > DateTime.Now

/// Book a seat (Engine-level)
let bookSeat screeningId seatId userId =
    if not (canBook screeningId) then
        Error "Screening is not available for booking"
    else
        match Repository.bookSeat seatId screeningId userId with
        | Some ticketId -> Ok ticketId
        | None -> Error "Seat already booked"
