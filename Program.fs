open System
open Models
open Database
open Repository

let mutable currentUser: User option = None

let clearScreen() = Console.Clear()

let printHeader title =
    printfn "\n=========================================="
    printfn "  %s" title
    printfn "==========================================\n"

// Helper function to safely get values from dynamic objects
let getValue<'T> (obj: obj) (key: string) : 'T =
    let dict = obj :?> System.Collections.Generic.IDictionary<string, obj>
    dict.[key] :?> 'T

let getValueString (obj: obj) (key: string) : string =
    let dict = obj :?> System.Collections.Generic.IDictionary<string, obj>
    string dict.[key]

// Authentication Menu
let rec authMenu() =
    clearScreen()
    printHeader "CINEMA BOOKING SYSTEM"
    printfn "1. Sign In"
    printfn "2. Sign Up"
    printfn "3. Exit"
    printf "\nSelect option: "
    
    match Console.ReadLine() with
    | "1" -> signInMenu()
    | "2" -> signUpMenu()
    | "3" -> 
        printfn "Goodbye!"
        exit 0
    | _ -> 
        printfn "Invalid option!"
        Console.ReadKey() |> ignore
        authMenu()

and signInMenu() =
    clearScreen()
    printHeader "SIGN IN"
    printf "Username: "
    let username = Console.ReadLine()
    printf "Password: "
    let password = Console.ReadLine()
    
    match signIn username password with
    | Some user ->
        currentUser <- Some user
        printfn "\nWelcome, %s!" user.Username
        Console.ReadKey() |> ignore
        if user.Role = "admin" then adminMenu() else userMenu()
    | None ->
        printfn "\nInvalid credentials!"
        Console.ReadKey() |> ignore
        authMenu()

and signUpMenu() =
    clearScreen()
    printHeader "SIGN UP"
    printf "Username: "
    let username = Console.ReadLine()
    printf "Password: "
    let password = Console.ReadLine()
    
    match signUp username password with
    | Some msg ->
        printfn "\n%s" msg
        Console.ReadKey() |> ignore
        authMenu()
    | None ->
        printfn "\nUsername already exists!"
        Console.ReadKey() |> ignore
        authMenu()

// Admin Menu
and adminMenu() =
    clearScreen()
    printHeader "ADMIN PANEL"
    printfn "1. Manage Movies"
    printfn "2. Manage Rooms"
    printfn "3. Manage Screenings"
    printfn "4. View All Bookings"
    printfn "5. Logout"
    printf "\nSelect option: "
    
    match Console.ReadLine() with
    | "1" -> movieManagementMenu()
    | "2" -> roomManagementMenu()
    | "3" -> screeningManagementMenu()
    | "4" -> viewAllBookings()
    | "5" -> 
        currentUser <- None
        authMenu()
    | _ -> 
        printfn "Invalid option!"
        Console.ReadKey() |> ignore
        adminMenu()

and movieManagementMenu() =
    clearScreen()
    printHeader "MOVIE MANAGEMENT"
    printfn "1. View All Movies"
    printfn "2. Add Movie"
    printfn "3. Update Movie"
    printfn "4. Delete Movie"
    printfn "5. Back"
    printf "\nSelect option: "
    
    match Console.ReadLine() with
    | "1" -> viewAllMovies(); movieManagementMenu()
    | "2" -> addMovie(); movieManagementMenu()
    | "3" -> updateMovieMenu(); movieManagementMenu()
    | "4" -> deleteMovieMenu(); movieManagementMenu()
    | "5" -> adminMenu()
    | _ -> movieManagementMenu()

and viewAllMovies() =
    clearScreen()
    printHeader "ALL MOVIES"
    let movies = getAllMovies()
    if movies.Length = 0 then
        printfn "No movies available."
    else
        movies |> List.iter (fun m ->
            printfn "ID: %d | Name: %s | Duration: %d mins" m.MovieId m.MovieName m.Duration
            match m.Description with
            | Some desc -> printfn "   Description: %s" desc
            | None -> ()
            printfn ""
        )
    Console.ReadKey() |> ignore

and addMovie() =
    clearScreen()
    printHeader "ADD MOVIE"
    printf "Movie Name: "
    let name = Console.ReadLine()
    printf "Movie Picture URL (optional, press Enter to skip): "
    let pic = Console.ReadLine()
    printf "Description (optional, press Enter to skip): "
    let desc = Console.ReadLine()
    printf "Duration (minutes): "
    let duration = Int32.Parse(Console.ReadLine())
    
    let picOption = if String.IsNullOrWhiteSpace(pic) then None else Some pic
    let descOption = if String.IsNullOrWhiteSpace(desc) then None else Some desc
    
    createMovie name picOption descOption duration
    Console.ReadKey() |> ignore

and updateMovieMenu() =
    clearScreen()
    printHeader "UPDATE MOVIE"
    viewAllMovies()
    printf "Enter Movie ID: "
    let movieId = Int32.Parse(Console.ReadLine())
    
    match getMovieById movieId with
    | Some movie ->
        printf "New Name (current: %s): " movie.MovieName
        let name = Console.ReadLine()
        let finalName = if String.IsNullOrWhiteSpace(name) then movie.MovieName else name
        
        printf "New Picture URL (press Enter to skip): "
        let pic = Console.ReadLine()
        let picOption = if String.IsNullOrWhiteSpace(pic) then movie.MoviePic else Some pic
        
        printf "New Description (press Enter to skip): "
        let desc = Console.ReadLine()
        let descOption = if String.IsNullOrWhiteSpace(desc) then movie.Description else Some desc
        
        printf "New Duration (current: %d, press Enter to keep): " movie.Duration
        let durationStr = Console.ReadLine()
        let duration = if String.IsNullOrWhiteSpace(durationStr) then movie.Duration else Int32.Parse(durationStr)
        
        if updateMovie movieId finalName picOption descOption duration then
            printfn "Movie updated successfully!"
        else
            printfn "Update failed!"
    | None -> printfn "Movie not found!"
    
    Console.ReadKey() |> ignore

and deleteMovieMenu() =
    clearScreen()
    printHeader "DELETE MOVIE"
    viewAllMovies()
    printf "Enter Movie ID to delete: "
    let movieId = Int32.Parse(Console.ReadLine())
    
    printf "Are you sure you want to delete this movie? (y/n): "
    let confirm = Console.ReadLine()
    
    if confirm.ToLower() = "y" then
        if deleteMovie movieId then
            printfn "Movie deleted successfully!"
        else
            printfn "Delete failed or movie not found!"
    else
        printfn "Deletion cancelled."
    
    Console.ReadKey() |> ignore

and roomManagementMenu() =
    clearScreen()
    printHeader "ROOM MANAGEMENT"
    printfn "1. View All Rooms"
    printfn "2. Add Room"
    printfn "3. Update Room"
    printfn "4. Delete Room"
    printfn "5. Back"
    printf "\nSelect option: "
    
    match Console.ReadLine() with
    | "1" -> viewAllRooms(); roomManagementMenu()
    | "2" -> addRoom(); roomManagementMenu()
    | "3" -> updateRoomMenu(); roomManagementMenu()
    | "4" -> deleteRoomMenu(); roomManagementMenu()
    | "5" -> adminMenu()
    | _ -> roomManagementMenu()

and viewAllRooms() =
    clearScreen()
    printHeader "ALL ROOMS"
    let rooms = getAllRooms()
    if rooms.Length = 0 then
        printfn "No rooms available."
    else
        rooms |> List.iter (fun r ->
            printfn "Room ID: %d | Rows: %d | Columns: %d | Total Seats: %d" 
                r.RoomId r.NoRows r.NoCol (r.NoRows * r.NoCol)
        )
    Console.ReadKey() |> ignore

and addRoom() =
    clearScreen()
    printHeader "ADD ROOM"
    printf "Number of Rows: "
    let rows = Int32.Parse(Console.ReadLine())
    printf "Number of Columns: "
    let cols = Int32.Parse(Console.ReadLine())
    
    createRoom rows cols |> ignore
    Console.ReadKey() |> ignore

and updateRoomMenu() =
    clearScreen()
    printHeader "UPDATE ROOM"
    viewAllRooms()
    printf "Enter Room ID: "
    let roomId = Int32.Parse(Console.ReadLine())
    
    printf "New Number of Rows: "
    let rows = Int32.Parse(Console.ReadLine())
    printf "New Number of Columns: "
    let cols = Int32.Parse(Console.ReadLine())
    
    printf "This will recreate all seats. Are you sure? (y/n): "
    let confirm = Console.ReadLine()
    
    if confirm.ToLower() = "y" then
        if updateRoom roomId rows cols then
            printfn "Room updated successfully!"
        else
            printfn "Update failed!"
    else
        printfn "Update cancelled."
    
    Console.ReadKey() |> ignore

and deleteRoomMenu() =
    clearScreen()
    printHeader "DELETE ROOM"
    viewAllRooms()
    printf "Enter Room ID to delete: "
    let roomId = Int32.Parse(Console.ReadLine())
    
    printf "Are you sure you want to delete this room? (y/n): "
    let confirm = Console.ReadLine()
    
    if confirm.ToLower() = "y" then
        if deleteRoom roomId then
            printfn "Room deleted successfully!"
        else
            printfn "Delete failed!"
    else
        printfn "Deletion cancelled."
    
    Console.ReadKey() |> ignore

and screeningManagementMenu() =
    clearScreen()
    printHeader "SCREENING MANAGEMENT"
    printfn "1. View All Screenings"
    printfn "2. Add Screening"
    printfn "3. Update Screening"
    printfn "4. Delete Screening"
    printfn "5. Back"
    printf "\nSelect option: "
    
    match Console.ReadLine() with
    | "1" -> viewAllScreeningsAdmin(); screeningManagementMenu()
    | "2" -> addScreening(); screeningManagementMenu()
    | "3" -> updateScreeningMenu(); screeningManagementMenu()
    | "4" -> deleteScreeningMenu(); screeningManagementMenu()
    | "5" -> adminMenu()
    | _ -> screeningManagementMenu()

and viewAllScreeningsAdmin() =
    clearScreen()
    printHeader "ALL SCREENINGS"
    let screenings = getAllScreenings()
    if screenings.Length = 0 then
        printfn "No screenings available."
    else
        screenings |> List.iter (fun s ->
            printfn "ID: %d | Movie: %s | Room: %d | Start: %s | End: %s" 
                s.ScreeningId s.MovieName s.RoomId 
                (s.StartTime.ToString("yyyy-MM-dd HH:mm")) 
                (s.EndTime.ToString("HH:mm"))
        )
    Console.ReadKey() |> ignore

and addScreening() =
    clearScreen()
    printHeader "ADD SCREENING"
    
    printfn "Available Movies:"
    let movies = getAllMovies()
    movies |> List.iter (fun m ->
        printfn "  %d. %s (%d mins)" m.MovieId m.MovieName m.Duration
    )
    printf "\nMovie ID: "
    let movieId = Int32.Parse(Console.ReadLine())
    
    printfn "\nAvailable Rooms:"
    let rooms = getAllRooms()
    rooms |> List.iter (fun r ->
        printfn "  %d. Room %d (%d seats)" r.RoomId r.RoomId (r.NoRows * r.NoCol)
    )
    printf "\nRoom ID: "
    let roomId = Int32.Parse(Console.ReadLine())
    
    printf "Start Date and Time (yyyy-MM-dd HH:mm): "
    let startTimeStr = Console.ReadLine()
    
    try
        let startTime = DateTime.Parse(startTimeStr)
        if createScreening movieId roomId startTime then
            printfn "Screening created successfully!"
        else
            printfn "Failed to create screening!"
    with
    | :? FormatException ->
        printfn "Invalid date format!"
    
    Console.ReadKey() |> ignore

and updateScreeningMenu() =
    clearScreen()
    printHeader "UPDATE SCREENING"
    viewAllScreeningsAdmin()
    printf "Enter Screening ID: "
    let screeningId = Int32.Parse(Console.ReadLine())
    
    printf "New Movie ID: "
    let movieId = Int32.Parse(Console.ReadLine())
    printf "New Room ID: "
    let roomId = Int32.Parse(Console.ReadLine())
    printf "New Start Time (yyyy-MM-dd HH:mm): "
    let startTimeStr = Console.ReadLine()
    
    try
        let startTime = DateTime.Parse(startTimeStr)
        if updateScreening screeningId movieId roomId startTime then
            printfn "Screening updated successfully!"
        else
            printfn "Update failed!"
    with
    | :? FormatException ->
        printfn "Invalid date format!"
    
    Console.ReadKey() |> ignore

and deleteScreeningMenu() =
    clearScreen()
    printHeader "DELETE SCREENING"
    viewAllScreeningsAdmin()
    printf "Enter Screening ID to delete: "
    let screeningId = Int32.Parse(Console.ReadLine())
    
    printf "Are you sure you want to delete this screening? (y/n): "
    let confirm = Console.ReadLine()
    
    if confirm.ToLower() = "y" then
        if deleteScreening screeningId then
            printfn "Screening deleted successfully!"
        else
            printfn "Delete failed!"
    else
        printfn "Deletion cancelled."
    
    Console.ReadKey() |> ignore

and viewAllBookings() =
    clearScreen()
    printHeader "ALL BOOKINGS"
    
    let tickets = getAllTickets()
    
    if tickets.Length = 0 then
        printfn "No bookings yet."
    else
        printfn "Total Bookings: %d\n" tickets.Length
        printfn "%-8s | %-15s | %-25s | %-15s | %-6s | %-8s | %-15s" 
            "Ticket" "Username" "Movie" "Showtime" "Room" "Seat" "Booked At"
        printfn "%s" (String.replicate 115 "-")
        
        tickets |> List.iter (fun t ->
            let ticketId = getValueString t "ticket_id"
            let username = getValueString t "username"
            let movieName = getValueString t "movie_name"
            let startTime = getValueString t "start_time"
            let roomId = getValueString t "room_id"
            let rowNumber = getValueString t "row_number"
            let seatNumber = getValueString t "seat_number"
            let createdAt = getValueString t "created_at"
            
            let startTimeParsed = DateTime.Parse(startTime).ToString("yyyy-MM-dd HH:mm")
            let createdAtParsed = DateTime.Parse(createdAt).ToString("yyyy-MM-dd HH:mm")
            
            printfn "%-8s | %-15s | %-25s | %-15s | %-6s | R%-2s S%-2s | %s" 
                ticketId
                username
                (if movieName.Length > 25 then movieName.Substring(0, 22) + "..." else movieName)
                startTimeParsed
                roomId
                rowNumber
                seatNumber
                createdAtParsed
        )
        
        printfn "\n%s" (String.replicate 115 "-")
        printfn "Total Tickets Sold: %d" tickets.Length
    
    Console.ReadKey() |> ignore
    adminMenu()

// User Menu
and userMenu() =
    clearScreen()
    printHeader "USER MENU"
    printfn "1. Browse Movies & Screenings"
    printfn "2. Book Seats"
    printfn "3. My Tickets"
    printfn "4. Logout"
    printf "\nSelect option: "
    
    match Console.ReadLine() with
    | "1" -> browseScreenings(); userMenu()
    | "2" -> bookSeatsMenu(); userMenu()
    | "3" -> viewMyTickets(); userMenu()
    | "4" -> 
        currentUser <- None
        authMenu()
    | _ -> 
        printfn "Invalid option!"
        Console.ReadKey() |> ignore
        userMenu()

and browseScreenings() =
    clearScreen()
    printHeader "AVAILABLE SCREENINGS"
    let screenings = getAllScreenings()
    if screenings.Length = 0 then
        printfn "No screenings available at the moment."
    else
        screenings |> List.iter (fun s ->
            printfn "\n=== Screening ID: %d ===" s.ScreeningId
            printfn "Movie: %s (%d mins)" s.MovieName s.Duration
            printfn "Room: %d" s.RoomId
            printfn "Start Time: %s" (s.StartTime.ToString("yyyy-MM-dd HH:mm"))
            printfn "End Time: %s" (s.EndTime.ToString("HH:mm"))
        )
    Console.ReadKey() |> ignore

and bookSeatsMenu() =
    clearScreen()
    printHeader "BOOK SEATS"
    
    let screenings = getAllScreenings()
    if screenings.Length = 0 then
        printfn "No screenings available."
        Console.ReadKey() |> ignore
    else
        printfn "Available Screenings:\n"
        screenings |> List.iter (fun s ->
            printfn "%d. %s - %s (Room %d)" 
                s.ScreeningId s.MovieName 
                (s.StartTime.ToString("yyyy-MM-dd HH:mm")) 
                s.RoomId
        )
        
        printf "\nEnter Screening ID: "
        try
            let screeningId = Int32.Parse(Console.ReadLine())
            
            let seats = getSeatsForScreening screeningId
            
            if seats.Length = 0 then
                printfn "No seats available for this screening."
            else
                printfn "\n=== SEATING MAP ==="
                printfn "Legend: [X] = Reserved, [Number] = Available\n"
                
                seats 
                |> List.groupBy (fun s -> s.RowNumber)
                |> List.iter (fun (row, rowSeats) ->
                    printf "Row %2d: " row
                    rowSeats |> List.iter (fun s ->
                        let status = if s.IsReserved then " [X] " else sprintf "[%3d]" s.SeatId
                        printf "%s " status
                    )
                    printfn ""
                )
                
                printf "\nEnter Seat ID to book (0 to cancel): "
                let seatId = Int32.Parse(Console.ReadLine())
                
                if seatId > 0 then
                    match currentUser with
                    | Some user ->
                        match bookSeat seatId screeningId user.UserId with
                        | Some ticketId ->
                            printfn "\n✓ Booking successful!"
                            printfn "Your Ticket ID: %d" ticketId
                            printfn "\nPlease save this ticket ID for your records."
                        | None ->
                            printfn "\n✗ Seat already booked or error occurred!"
                    | None -> printfn "Please login first!"
                else
                    printfn "Booking cancelled."
        with
        | :? FormatException ->
            printfn "Invalid input!"
        | ex ->
            printfn "Error: %s" ex.Message
    
    Console.ReadKey() |> ignore
and viewMyTickets() =
    clearScreen()
    printHeader "MY TICKETS"
    
    match currentUser with
    | Some user ->
        let tickets = getUserTickets user.UserId
        if tickets.Length = 0 then
            printfn "You have no tickets yet."
        else
            printfn "You have %d ticket(s):\n" tickets.Length
            tickets |> List.iteri (fun i t ->
                let ticketId = getValueString t "ticket_id"
                let movieName = getValueString t "movie_name"
                let startTime = getValueString t "start_time"
                let rowNumber = getValueString t "row_number"
                let seatNumber = getValueString t "seat_number"
                
                printfn "=== Ticket #%d ===" (i + 1)
                printfn "Ticket ID: %s" ticketId
                printfn "Movie: %s" movieName
                printfn "Showtime: %s" (DateTime.Parse(startTime).ToString("yyyy-MM-dd HH:mm"))
                printfn "Seat Location: Row %s, Seat Number %s" rowNumber seatNumber
                printfn ""
            )
    | None -> printfn "Please login first!"
    
    Console.ReadKey() |> ignore

[<EntryPoint>]
let main argv =
    try
        printfn "Initializing Cinema Booking System Database..."
        initializeDatabase()
        printfn "Database initialized successfully!\n"
        System.Threading.Thread.Sleep(1000)
        authMenu()
        0
    with
    | ex ->
        printfn "Fatal Error: %s" ex.Message
        printfn "Stack Trace: %s" ex.StackTrace
        Console.ReadKey() |> ignore
        1