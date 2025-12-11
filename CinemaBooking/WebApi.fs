module WebApi

open System
open System.Text
open Giraffe
open Microsoft.AspNetCore.Http
open Models
open Database
open BookingFunctions
open Auth
open SeatLayout
open System.Text.Json

// JSON serialization options
let jsonOptions = JsonSerializerOptions(PropertyNamingPolicy = JsonNamingPolicy.CamelCase)

// Helper functions for JSON responses
let jsonResponse (data: 'a) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        ctx.SetContentType "application/json; charset=utf-8"
        ctx.WriteStringAsync(JsonSerializer.Serialize(data, jsonOptions))

let parseJson<'T> (ctx: HttpContext) =
    task {
        let! body = ctx.ReadBodyFromRequestAsync()
        return JsonSerializer.Deserialize<'T>(body, jsonOptions)
    }

// Request/Response DTOs
type RegisterRequest = { Name: string; Email: string; Password: string }
type LoginRequest = { Email: string; Password: string }
type SeatStatusResponse = { SeatId: string; Row: int; Seat: int; IsBooked: bool }

// API Handlers
let registerHandler : HttpHandler =
    fun next ctx ->
        task {
            let! request = parseJson<RegisterRequest> ctx
            
            // Use email as username for authentication
            match signUp request.Email request.Password with
            | SignUpSuccess user ->
                // Update user with name if needed (for now, we'll use email as username)
                let response = {| Success = true; User = {| Id = user.UserId; Name = request.Name; Email = user.Username |} |}
                return! jsonResponse response next ctx
            | UserAlreadyExists ->
                ctx.SetStatusCode 400
                let response = {| Success = false; Message = "User already exists" |}
                return! jsonResponse response next ctx
            | _ ->
                ctx.SetStatusCode 500
                let response = {| Success = false; Message = "Failed to create user" |}
                return! jsonResponse response next ctx
        }

let loginHandler : HttpHandler =
    fun next ctx ->
        task {
            let! request = parseJson<LoginRequest> ctx
            
            // Email is used as username in our system
            match signIn request.Email request.Password with
            | Success user ->
                // For now, use username as name (since we don't store name separately)
                // In a real system, you'd have a separate Name field
                let response = {| Success = true; User = {| Id = user.UserId; Name = user.Username; Email = user.Username |} |}
                return! jsonResponse response next ctx
            | IncorrectPassword ->
                ctx.SetStatusCode 401
                let response = {| Success = false; Message = "Incorrect password" |}
                return! jsonResponse response next ctx
            | UserNotFound ->
                ctx.SetStatusCode 404
                let response = {| Success = false; Message = "User not found" |}
                return! jsonResponse response next ctx
            | _ ->
                ctx.SetStatusCode 500
                let response = {| Success = false; Message = "Login failed" |}
                return! jsonResponse response next ctx
        }

let getSeatsHandler : HttpHandler =
    fun next ctx ->
        let seats = getAllSeats ()
        let seatResponses =
            seats
            |> List.map (fun seat ->
                let rowLabel = char (int 'A' + seat.RowNumber - 1)
                { SeatId = sprintf "%c%d" rowLabel seat.SeatNumber
                  Row = seat.RowNumber
                  Seat = seat.SeatNumber
                  IsBooked = seat.IsReserved })
        jsonResponse seatResponses next ctx

type BookingRequest = { UserId: int; UserEmail: string; Seats: string[] }

let bookSeatsHandler : HttpHandler =
    fun next ctx ->
        task {
            let! request = parseJson<BookingRequest> ctx
            
            let mutable success = true
            let mutable bookedSeatIds = []
            let mutable errors = []
            
            // Find user by email (username in our system)
            let user = findUserByUsername request.UserEmail
            
            match user with
            | None ->
                ctx.SetStatusCode 404
                let response = {| Success = false; Message = "User not found" |}
                return! jsonResponse response next ctx
            | Some user ->
                // Check if user already has a ticket
                if userHasTickets user.UserId then
                    ctx.SetStatusCode 400
                    let response = {| Success = false; Message = "You already have a ticket. Each user can only book one ticket." |}
                    return! jsonResponse response next ctx
                
                // Check if user is trying to book more than one seat (each seat = one ticket)
                if request.Seats.Length > 1 then
                    ctx.SetStatusCode 400
                    let response = {| Success = false; Message = "You can only book one seat at a time. Each user is limited to one ticket." |}
                    return! jsonResponse response next ctx
                
                // Parse seat IDs (e.g., "A1", "B2") to row and seat numbers
                let parseSeatId (seatId: string) =
                    if seatId.Length >= 2 then
                        let rowChar = seatId.[0]
                        let rowNum = int rowChar - int 'A' + 1
                        let seatNum = int (seatId.Substring(1))
                        Some (rowNum, seatNum)
                    else
                        None
                
                // Reserve each seat
                for seatId in request.Seats do
                    match parseSeatId seatId with
                    | Some (rowNum, seatNum) ->
                        match findSeatByCoordinates rowNum seatNum with
                        | Some seat ->
                            if not seat.IsReserved then
                                match saveSeatReservation seat.SeatId with
                                | Some _ ->
                                    // Create ticket
                                    let ticket = { TicketId = Guid.NewGuid().ToString(); SeatId = seat.SeatId; UserId = user.UserId }
                                    match saveTicket ticket with
                                    | Some _ -> bookedSeatIds <- seatId :: bookedSeatIds
                                    | None ->
                                        success <- false
                                        errors <- (sprintf "Failed to create ticket for seat %s" seatId) :: errors
                                | None ->
                                    success <- false
                                    errors <- (sprintf "Failed to reserve seat %s" seatId) :: errors
                            else
                                success <- false
                                errors <- (sprintf "Seat %s is already reserved" seatId) :: errors
                        | None ->
                            success <- false
                            errors <- (sprintf "Seat %s not found" seatId) :: errors
                    | None ->
                        success <- false
                        errors <- (sprintf "Invalid seat ID format: %s" seatId) :: errors
                
                if success && bookedSeatIds.Length = request.Seats.Length then
                    let ticketId = Guid.NewGuid().ToString()
                    let response = {| Success = true; TicketId = ticketId; Seats = bookedSeatIds; Total = bookedSeatIds.Length * 10 |}
                    return! jsonResponse response next ctx
                else
                    ctx.SetStatusCode 400
                    let response = {| Success = false; Message = String.Join(", ", errors); BookedSeats = bookedSeatIds |}
                    return! jsonResponse response next ctx
        }

let getUserBookingsHandler (userId: int) : HttpHandler =
    fun next ctx ->
        use connection = getConnection ()
        connection.Open()
        let sql = """
            SELECT t.TicketId, s.RowNumber, s.SeatNumber, t.UserId
            FROM Tickets t
            INNER JOIN Seats s ON t.SeatId = s.SeatId
            WHERE t.UserId = @userId
        """
        use cmd = new Microsoft.Data.Sqlite.SqliteCommand(sql, connection)
        cmd.Parameters.AddWithValue("@userId", userId) |> ignore
        use reader = cmd.ExecuteReader()
        
        let bookings = [
            while reader.Read() do
                let rowNum = reader.GetInt32(1)
                let seatNum = reader.GetInt32(2)
                let rowLabel = char (int 'A' + rowNum - 1)
                yield {| TicketId = reader.GetString(0); SeatId = sprintf "%c%d" rowLabel seatNum; Row = rowNum; Seat = seatNum |}
        ]
        
        jsonResponse bookings next ctx

// Web application routes
let webApp : HttpHandler =
    choose [
        POST >=> route "/api/register" >=> registerHandler
        POST >=> route "/api/login" >=> loginHandler
        GET >=> route "/api/seats" >=> getSeatsHandler
        POST >=> route "/api/book" >=> bookSeatsHandler
        GET >=> routef "/api/bookings/%i" getUserBookingsHandler
        RequestErrors.NOT_FOUND "Not Found"
    ]

