module WebApi

open System
open System.Text
open Giraffe
open Microsoft.AspNetCore.Http
open Models
open Database
open Repository
open Auth
open SeatLayout
open ScreeningEngine
open System.Text.Json
open System.Text.Json.Serialization

// Custom DateTime converter to handle local time without timezone conversion
type LocalDateTimeConverter() =
    inherit JsonConverter<DateTime>()
    
    override _.Read(reader: byref<Utf8JsonReader>, typeToConvert: Type, options: JsonSerializerOptions) =
        let dateString = reader.GetString()
        // Parse datetime string as local time
        if dateString.Contains("T") then
            DateTime.ParseExact(dateString.Replace("T", " "), "yyyy-MM-dd HH:mm:ss", null)
        else
            DateTime.Parse(dateString)
    
    override _.Write(writer: Utf8JsonWriter, value: DateTime, options: JsonSerializerOptions) =
        // Write datetime in local format without timezone
        writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ss"))

// JSON serialization options
let jsonOptions =
    let options = JsonSerializerOptions()
    options.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase
    options.PropertyNameCaseInsensitive <- true
    options.Converters.Add(LocalDateTimeConverter())
    options

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
type RegisterRequest =
    { Name: string
      Email: string
      Password: string }

type LoginRequest = 
    { Email: string
      Password: string }

type BookingRequest =
    { UserId: int
      ScreeningId: int
      SeatIds: int list }

// API Handlers
let registerHandler: HttpHandler =
    fun next ctx ->
        task {
            let! request = parseJson<RegisterRequest> ctx

            match signUp request.Email request.Password (Some request.Name) with
            | SignUpSuccess user ->
                let response =
                    {| Success = true
                       User =
                        {| Id = user.UserId
                           Name = (match user.Name with | Some n -> n | None -> user.Username)
                           Email = user.Username |} |}

                return! jsonResponse response next ctx
            | UserAlreadyExists ->
                ctx.SetStatusCode 400
                let response =
                    {| Success = false
                       Message = "User already exists" |}
                return! jsonResponse response next ctx
            | _ ->
                ctx.SetStatusCode 500
                let response =
                    {| Success = false
                       Message = "Failed to create user" |}
                return! jsonResponse response next ctx
        }

let loginHandler: HttpHandler =
    fun next ctx ->
        task {
            let! request = parseJson<LoginRequest> ctx

            match signIn request.Email request.Password with
            | Success user ->
                let response =
                    {| Success = true
                       User =
                        {| Id = user.UserId
                           Name = (match user.Name with | Some n -> n | None -> user.Username)
                           Email = user.Username
                           Role = user.Role |} |}
                return! jsonResponse response next ctx
            | IncorrectPassword ->
                ctx.SetStatusCode 401
                let response =
                    {| Success = false
                       Message = "Incorrect password" |}
                return! jsonResponse response next ctx
            | UserNotFound ->
                ctx.SetStatusCode 404
                let response =
                    {| Success = false
                       Message = "User not found" |}
                return! jsonResponse response next ctx
            | _ ->
                ctx.SetStatusCode 500
                let response =
                    {| Success = false
                       Message = "Login failed" |}
                return! jsonResponse response next ctx
        }

// Get all movies
let getMoviesHandler: HttpHandler =
    fun next ctx ->
        let movies = getAllMovies()
        jsonResponse movies next ctx

// Get all rooms
let getRoomsHandler: HttpHandler =
    fun next ctx ->
        let rooms = getAllRooms()
        jsonResponse rooms next ctx

// Get all screenings
let getScreeningsHandler: HttpHandler =
    fun next ctx ->
        let screenings = getAllScreenings()
        jsonResponse screenings next ctx

// Get upcoming screenings
let getUpcomingScreeningsHandler: HttpHandler =
    fun next ctx ->
        let screenings = getUpcomingScreenings()
        jsonResponse screenings next ctx

// Get seats for a specific screening
let getScreeningSeatsHandler (screeningId: int): HttpHandler =
    fun next ctx ->
        let seats = getSeatsForScreening screeningId
        jsonResponse seats next ctx

// Book seats
let bookSeatsHandler: HttpHandler =
    fun next ctx ->
        task {
            let! request = parseJson<BookingRequest> ctx
            
            // Validate: Only 1 seat per booking
            if request.SeatIds.Length > 1 then
                ctx.SetStatusCode 400
                let response =
                    {| Success = false
                       Message = "You can only book 1 seat per booking" |}
                return! jsonResponse response next ctx
            elif request.SeatIds.Length = 0 then
                ctx.SetStatusCode 400
                let response =
                    {| Success = false
                       Message = "Please select a seat" |}
                return! jsonResponse response next ctx
            else
                let mutable ticketIds = []
                let mutable errors = []
                
                for seatId in request.SeatIds do
                    match bookSeat request.ScreeningId seatId request.UserId with
                    | Ok ticketId -> ticketIds <- ticketId :: ticketIds
                    | Error msg -> errors <- msg :: errors
                
                if errors.IsEmpty then
                    let response =
                        {| Success = true
                           TicketIds = ticketIds
                           Message = "Booking successful" |}
                    return! jsonResponse response next ctx
                else
                    ctx.SetStatusCode 400
                    let response =
                        {| Success = false
                           Message = String.concat ", " errors
                           TicketIds = ticketIds |}
                    return! jsonResponse response next ctx
        }

// Get user tickets
let getUserTicketsHandler (userId: int): HttpHandler =
    fun next ctx ->
        let tickets = getUserTickets userId
        jsonResponse tickets next ctx

// Admin: Create movie
let createMovieHandler: HttpHandler =
    fun next ctx ->
        task {
            let! movie = parseJson<Movie> ctx
            createMovie movie.MovieName movie.MoviePic movie.Description movie.Duration
            let response = {| Success = true; Message = "Movie created" |}
            return! jsonResponse response next ctx
        }

// Admin: Update movie
let updateMovieHandler (movieId: int): HttpHandler =
    fun next ctx ->
        task {
            let! movie = parseJson<Movie> ctx
            let success = updateMovie movieId movie.MovieName movie.MoviePic movie.Description movie.Duration
            let response = {| Success = success |}
            return! jsonResponse response next ctx
        }

// Admin: Delete movie
let deleteMovieHandler (movieId: int): HttpHandler =
    fun next ctx ->
        let success = deleteMovie movieId
        let response = {| Success = success |}
        jsonResponse response next ctx

// Admin: Create room
let createRoomHandler: HttpHandler =
    fun next ctx ->
        task {
            let! room = parseJson<Room> ctx
            let roomId = createRoom room.RoomName room.NoRows room.NoCol
            let response = {| Success = true; RoomId = roomId |}
            return! jsonResponse response next ctx
        }

// Admin: Update room
let updateRoomHandler (roomId: int): HttpHandler =
    fun next ctx ->
        task {
            let! room = parseJson<Room> ctx
            let success = updateRoom roomId room.RoomName room.NoRows room.NoCol
            let response = {| Success = success |}
            return! jsonResponse response next ctx
        }

// Admin: Delete room
let deleteRoomHandler (roomId: int): HttpHandler =
    fun next ctx ->
        let success = deleteRoom roomId
        let response = {| Success = success |}
        jsonResponse response next ctx

// Admin: Create screening
let createScreeningHandler: HttpHandler =
    fun next ctx ->
        task {
            let! screening = parseJson<Screening> ctx
            let success = createScreening screening.MovieId screening.RoomId screening.StartTime
            let response = {| Success = success |}
            return! jsonResponse response next ctx
        }

// Admin: Update screening
let updateScreeningHandler (screeningId: int): HttpHandler =
    fun next ctx ->
        task {
            let! screening = parseJson<Screening> ctx
            let success = updateScreening screeningId screening.MovieId screening.RoomId screening.StartTime
            let response = {| Success = success |}
            return! jsonResponse response next ctx
        }

// Admin: Delete screening
let deleteScreeningHandler (screeningId: int): HttpHandler =
    fun next ctx ->
        let success = deleteScreening screeningId
        let response = {| Success = success |}
        jsonResponse response next ctx

// Web application routes
let webApp: HttpHandler =
    choose
        [ 
          // Auth routes
          POST >=> route "/api/register" >=> registerHandler
          POST >=> route "/api/login" >=> loginHandler
          
          // Public routes
          GET >=> route "/api/movies" >=> getMoviesHandler
          GET >=> route "/api/rooms" >=> getRoomsHandler
          GET >=> route "/api/screenings" >=> getScreeningsHandler
          GET >=> route "/api/screenings/upcoming" >=> getUpcomingScreeningsHandler
          GET >=> routef "/api/screenings/%i/seats" getScreeningSeatsHandler
          
          // Booking routes
          POST >=> route "/api/book" >=> bookSeatsHandler
          GET >=> routef "/api/users/%i/tickets" getUserTicketsHandler
          
          // Admin routes
          POST >=> route "/api/admin/movies" >=> createMovieHandler
          PUT >=> routef "/api/admin/movies/%i" updateMovieHandler
          DELETE >=> routef "/api/admin/movies/%i" deleteMovieHandler
          
          POST >=> route "/api/admin/rooms" >=> createRoomHandler
          PUT >=> routef "/api/admin/rooms/%i" updateRoomHandler
          DELETE >=> routef "/api/admin/rooms/%i" deleteRoomHandler
          
          POST >=> route "/api/admin/screenings" >=> createScreeningHandler
          PUT >=> routef "/api/admin/screenings/%i" updateScreeningHandler
          DELETE >=> routef "/api/admin/screenings/%i" deleteScreeningHandler
          
          RequestErrors.NOT_FOUND "Not Found" 
        ]
