open System
open Models
open Database
open Repository
open Auth
open TicketSystem
open Session
open SeatLayout
open ScreeningEngine


[<EntryPoint>]

initializeDatabase ()

match signUp "mokhtar" "1234" with
| SignUpSuccess user -> printfn "User signed up: %s" user.Username
| UserAlreadyExists -> printfn "User already exists"
| _ -> ()

match signIn "mokhtar" "1234" with
| Success user -> printfn "Signed in as: %s" user.Username
| _ -> failwith "Sign-in failed"


let seat =
    { SeatId = 1
      RoomId = 1
      RowNumber = 1
      SeatNumber = 1
      IsReserved = false }

//let screening =
//    { ScreeningId = 1
//      MovieId = 1
//      RoomId = 1
//      StartTime = DateTime.Now
//      EndTime = DateTime.Now.AddHours 2.0 }

//let ticket = addTicket seat screening
//printfn "%A" ticket


printfn "\n=== ScreeningEngine Tests ==="

let movieId =
    match getAllMovies () with
    | m :: _ -> m.MovieId
    | [] ->
        createMovie "Engine Movie" None None 100
        (getAllMovies () |> List.head).MovieId

let roomId =
    match getAllRooms () with
    | r :: _ -> r.RoomId
    | [] -> createRoom 4 4

let startTime = DateTime.Now.AddHours 1.0
createScreening movieId roomId startTime |> ignore

let screening =
    getAllScreenings ()
    |> List.find (fun s -> s.MovieName = "Engine Movie")

// -----------------------------
// UPCOMING
// -----------------------------
let upcoming = getUpcomingScreenings ()
if upcoming.Length > 0 then
    printfn "✔ Upcoming screenings: PASS"
else
    printfn "✘ Upcoming screenings: FAIL"

// -----------------------------
// SEATS
// -----------------------------
let seats = getScreeningSeats screening.ScreeningId
if seats.Length > 0 then
    printfn "✔ Get screening seats: PASS"
else
    printfn "✘ Get screening seats: FAIL"

// -----------------------------
// BOOKING
// -----------------------------
let userId =
    match findUserByUsername "admin" with
    | Some u -> u.UserId
    | None -> failwith "Admin user missing"

let seatId = seats.Head.SeatId

match bookSeat screening.ScreeningId seatId userId with
| Ok ticketId ->
    printfn "✔ Book seat: PASS (Ticket %d)" ticketId
| Error msg ->
    printfn "✘ Book seat: FAIL (%s)" msg

// Booking same seat again → should fail
match bookSeat screening.ScreeningId seatId userId with
| Ok _ ->
    printfn "✘ Double booking prevention: FAIL"
| Error _ ->
    printfn "✔ Double booking prevention: PASS"

printfn "\n=== ScreeningEngine Tests ==="

initializeDatabase ()

let movieId2 =
    match getAllMovies () with
    | m :: _ -> m.MovieId
    | [] ->
        createMovie "Engine Movie" None None 100
        (getAllMovies () |> List.head).MovieId

let roomId2 =
    match getAllRooms () with
    | r :: _ -> r.RoomId
    | [] -> createRoom 4 4

let startTime2 = DateTime.Now.AddHours 1.0
createScreening movieId2 roomId2 startTime2 |> ignore

let screening2 =
    getAllScreenings ()
    |> List.find (fun s -> s.MovieName = "Engine Movie")

// -----------------------------
// UPCOMING
// -----------------------------
let upcoming2 = getUpcomingScreenings ()
if upcoming2.Length > 0 then
    printfn "✔ Upcoming screenings: PASS"
else
    printfn "✘ Upcoming screenings: FAIL"

// -----------------------------
// SEATS
// -----------------------------
let seats2 = getScreeningSeats screening2.ScreeningId
if seats2.Length > 0 then
    printfn "✔ Get screening seats: PASS"
else
    printfn "✘ Get screening seats: FAIL"

// -----------------------------
// BOOKING
// -----------------------------
let userId2 =
    match findUserByUsername "admin" with
    | Some u -> u.UserId
    | None -> failwith "Admin user missing"

let seatId2 = seats2.Head.SeatId

match bookSeat screening2.ScreeningId seatId2 userId2 with
| Ok ticketId ->
    printfn "✔ Book seat: PASS (Ticket %d)" ticketId
| Error msg ->
    printfn "✘ Book seat: FAIL (%s)" msg

// Booking same seat again → should fail
match bookSeat screening2.ScreeningId seatId2 userId2 with
| Ok _ ->
    printfn "✘ Double booking prevention: FAIL"
| Error _ ->
    printfn "✔ Double booking prevention: PASS"