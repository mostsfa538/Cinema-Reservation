module Repository

open System
open Microsoft.Data.Sqlite
open Models
open Database
open Dapper

// ==================== USER OPERATIONS ====================

let signUp username password =
    use connection = getConnection()
    connection.Open()
    
    try
        let sql = "INSERT INTO user (username, password, role) VALUES (@Username, @Password, 'user')"
        let parameters = dict ["Username", box username; "Password", box password]
        connection.Execute(sql, parameters) |> ignore
        Some "Sign up successful!"
    with
    | :? SqliteException as ex when ex.Message.Contains("UNIQUE") ->
        None // Username already exists

let signIn username password =
    use connection = getConnection()
    connection.Open()
    
    let sql = """
        SELECT 
            user_id as UserId, 
            username as Username, 
            password as Password, 
            role as Role 
        FROM user 
        WHERE username = @Username AND password = @Password
    """
    let parameters = dict ["Username", box username; "Password", box password]
    
    let result = connection.Query<User>(sql, parameters) |> Seq.tryHead
    result

// ==================== MOVIE OPERATIONS (ADMIN) ====================

let createMovie movieName moviePic description duration =
    use connection = getConnection()
    connection.Open()
    
    let sql = """
        INSERT INTO movies (movie_name, movie_pic, description, duration) 
        VALUES (@MovieName, @MoviePic, @Description, @Duration)
    """
    
    // Convert F# option to obj (null if None)
    let picValue = match moviePic with | Some v -> box v | None -> box null
    let descValue = match description with | Some v -> box v | None -> box null
    
    let parameters = dict [
        "MovieName", box movieName
        "MoviePic", picValue
        "Description", descValue
        "Duration", box duration
    ]
    
    connection.Execute(sql, parameters) |> ignore
    printfn "Movie '%s' created successfully!" movieName

let getAllMovies() =
    use connection = getConnection()
    connection.Open()
    
    let sql = """
        SELECT 
            movie_id as MovieId, 
            movie_name as MovieName, 
            movie_pic as MoviePic, 
            description as Description, 
            duration as Duration 
        FROM movies
    """
    connection.Query<Movie>(sql) |> Seq.toList

let getMovieById movieId =
    use connection = getConnection()
    connection.Open()
    
    let sql = """
        SELECT 
            movie_id as MovieId, 
            movie_name as MovieName, 
            movie_pic as MoviePic, 
            description as Description, 
            duration as Duration 
        FROM movies 
        WHERE movie_id = @MovieId
    """
    let parameters = dict ["MovieId", box movieId]
    connection.Query<Movie>(sql, parameters) |> Seq.tryHead

let updateMovie movieId movieName moviePic description duration =
    use connection = getConnection()
    connection.Open()
    
    let sql = """
        UPDATE movies 
        SET movie_name = @MovieName, movie_pic = @MoviePic, description = @Description, duration = @Duration
        WHERE movie_id = @MovieId
    """
    
    // Convert F# option to obj (null if None)
    let picValue = match moviePic with | Some v -> box v | None -> box null
    let descValue = match description with | Some v -> box v | None -> box null
    
    let parameters = dict [
        "MovieId", box movieId
        "MovieName", box movieName
        "MoviePic", picValue
        "Description", descValue
        "Duration", box duration
    ]
    
    let rowsAffected = connection.Execute(sql, parameters)
    rowsAffected > 0

let deleteMovie movieId =
    use connection = getConnection()
    connection.Open()
    
    let sql = "DELETE FROM movies WHERE movie_id = @MovieId"
    let parameters = dict ["MovieId", box movieId]
    
    let rowsAffected = connection.Execute(sql, parameters)
    rowsAffected > 0

// ==================== ROOM OPERATIONS (ADMIN) ====================

let createRoom noRows noCol =
    use connection = getConnection()
    connection.Open()
    
    // Insert room
    let roomSql = "INSERT INTO rooms (no_rows, no_col) VALUES (@NoRows, @NoCol); SELECT last_insert_rowid();"
    let roomParams = dict ["NoRows", box noRows; "NoCol", box noCol]
    let roomId = connection.ExecuteScalar<int64>(roomSql, roomParams) |> int
    
    // Create seats for this room
    for row in 1 .. noRows do
        for col in 1 .. noCol do
            let seatSql = "INSERT INTO seats (room_id, row_number, seat_number, is_reserved) VALUES (@RoomId, @Row, @Col, 0)"
            let seatParams = dict ["RoomId", box roomId; "Row", box row; "Col", box col]
            connection.Execute(seatSql, seatParams) |> ignore
    
    printfn "Room %d created with %d seats!" roomId (noRows * noCol)
    roomId

let getAllRooms() =
    use connection = getConnection()
    connection.Open()
    
    let sql = """
        SELECT 
            room_id as RoomId, 
            no_rows as NoRows, 
            no_col as NoCol 
        FROM rooms
    """
    connection.Query<Room>(sql) |> Seq.toList

let getRoomById roomId =
    use connection = getConnection()
    connection.Open()
    
    let sql = """
        SELECT 
            room_id as RoomId, 
            no_rows as NoRows, 
            no_col as NoCol 
        FROM rooms 
        WHERE room_id = @RoomId
    """
    let parameters = dict ["RoomId", box roomId]
    connection.Query<Room>(sql, parameters) |> Seq.tryHead

let updateRoom roomId noRows noCol =
    use connection = getConnection()
    connection.Open()
    
    // Update room
    let sql = "UPDATE rooms SET no_rows = @NoRows, no_col = @NoCol WHERE room_id = @RoomId"
    let parameters = dict ["RoomId", box roomId; "NoRows", box noRows; "NoCol", box noCol]
    let rowsAffected = connection.Execute(sql, parameters)
    
    if rowsAffected > 0 then
        // Delete old seats
        let deleteSql = "DELETE FROM seats WHERE room_id = @RoomId"
        connection.Execute(deleteSql, dict ["RoomId", box roomId]) |> ignore
        
        // Create new seats
        for row in 1 .. noRows do
            for col in 1 .. noCol do
                let seatSql = "INSERT INTO seats (room_id, row_number, seat_number, is_reserved) VALUES (@RoomId, @Row, @Col, 0)"
                let seatParams = dict ["RoomId", box roomId; "Row", box row; "Col", box col]
                connection.Execute(seatSql, seatParams) |> ignore
        
        true
    else
        false

let deleteRoom roomId =
    use connection = getConnection()
    connection.Open()
    
    // Delete associated seats first
    let deleteSeatsSql = "DELETE FROM seats WHERE room_id = @RoomId"
    connection.Execute(deleteSeatsSql, dict ["RoomId", box roomId]) |> ignore
    
    // Delete room
    let sql = "DELETE FROM rooms WHERE room_id = @RoomId"
    let parameters = dict ["RoomId", box roomId]
    
    let rowsAffected = connection.Execute(sql, parameters)
    rowsAffected > 0

// ==================== SCREENING OPERATIONS (ADMIN) ====================

let createScreening movieId roomId (startTime: DateTime) =
    use connection = getConnection()
    connection.Open()
    
    // Get movie duration
    let movieOpt = getMovieById movieId
    match movieOpt with
    | Some movie ->
        let endTime = startTime.AddMinutes(float movie.Duration)
        
        let sql = """
            INSERT INTO screenings (movie_id, room_id, start_time, end_time) 
            VALUES (@MovieId, @RoomId, @StartTime, @EndTime)
        """
        let parameters = dict [
            "MovieId", box movieId
            "RoomId", box roomId
            "StartTime", box (startTime.ToString("yyyy-MM-dd HH:mm:ss"))
            "EndTime", box (endTime.ToString("yyyy-MM-dd HH:mm:ss"))
        ]
        
        connection.Execute(sql, parameters) |> ignore
        printfn "Screening created successfully!"
        true
    | None ->
        printfn "Movie not found!"
        false

let getAllScreenings() =
    use connection = getConnection()
    connection.Open()
    
    let sql = """
        SELECT 
            s.screening_id as ScreeningId, 
            m.movie_name as MovieName, 
            s.room_id as RoomId, 
            s.start_time as StartTime, 
            s.end_time as EndTime, 
            m.duration as Duration
        FROM screenings s
        JOIN movies m ON s.movie_id = m.movie_id
        ORDER BY s.start_time
    """
    
    connection.Query<ScreeningView>(sql) |> Seq.toList

let getScreeningById screeningId =
    use connection = getConnection()
    connection.Open()
    
    let sql = """
        SELECT 
            screening_id as ScreeningId, 
            movie_id as MovieId, 
            room_id as RoomId, 
            start_time as StartTime, 
            end_time as EndTime 
        FROM screenings 
        WHERE screening_id = @ScreeningId
    """
    let parameters = dict ["ScreeningId", box screeningId]
    connection.Query<Screening>(sql, parameters) |> Seq.tryHead

let updateScreening screeningId movieId roomId (startTime: DateTime) =
    use connection = getConnection()
    connection.Open()
    
    let movieOpt = getMovieById movieId
    match movieOpt with
    | Some movie ->
        let endTime = startTime.AddMinutes(float movie.Duration)
        
        let sql = """
            UPDATE screenings 
            SET movie_id = @MovieId, room_id = @RoomId, start_time = @StartTime, end_time = @EndTime
            WHERE screening_id = @ScreeningId
        """
        let parameters = dict [
            "ScreeningId", box screeningId
            "MovieId", box movieId
            "RoomId", box roomId
            "StartTime", box (startTime.ToString("yyyy-MM-dd HH:mm:ss"))
            "EndTime", box (endTime.ToString("yyyy-MM-dd HH:mm:ss"))
        ]
        
        let rowsAffected = connection.Execute(sql, parameters)
        rowsAffected > 0
    | None -> false

let deleteScreening screeningId =
    use connection = getConnection()
    connection.Open()
    
    let sql = "DELETE FROM screenings WHERE screening_id = @ScreeningId"
    let parameters = dict ["ScreeningId", box screeningId]
    
    let rowsAffected = connection.Execute(sql, parameters)
    rowsAffected > 0

// ==================== SEAT & BOOKING OPERATIONS ====================

let getSeatsForScreening screeningId =
    use connection = getConnection()
    connection.Open()
    
    let screeningOpt = getScreeningById screeningId
    match screeningOpt with
    | Some screening ->
        let sql = """
            SELECT 
                s.seat_id as SeatId, 
                s.row_number as RowNumber, 
                s.seat_number as SeatNumber,
                CASE WHEN t.ticket_id IS NOT NULL THEN 1 ELSE 0 END as IsReserved
            FROM seats s
            LEFT JOIN ticket t ON s.seat_id = t.seat_id AND t.screening_id = @ScreeningId
            WHERE s.room_id = @RoomId
            ORDER BY s.row_number, s.seat_number
        """
        let parameters = dict ["ScreeningId", box screeningId; "RoomId", box screening.RoomId]
        connection.Query<SeatView>(sql, parameters) |> Seq.toList
    | None -> []

let bookSeat seatId screeningId userId =
    use connection = getConnection()
    connection.Open()
    
    // Check if seat is already booked for this screening
    let checkSql = """
        SELECT COUNT(*) FROM ticket 
        WHERE seat_id = @SeatId AND screening_id = @ScreeningId
    """
    let checkParams = dict ["SeatId", box seatId; "ScreeningId", box screeningId]
    let count = connection.ExecuteScalar<int64>(checkSql, checkParams)
    
    if count > 0L then
        None // Seat already booked
    else
        let sql = """
            INSERT INTO ticket (seat_id, screening_id, user_id, created_at) 
            VALUES (@SeatId, @ScreeningId, @UserId, @CreatedAt);
            SELECT last_insert_rowid();
        """
        let parameters = dict [
            "SeatId", box seatId
            "ScreeningId", box screeningId
            "UserId", box userId
            "CreatedAt", box (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
        ]
        
        let ticketId = connection.ExecuteScalar<int64>(sql, parameters) |> int
        Some ticketId

 // ==================== TICKET OPERATIONS ====================

let getAllTickets() =
    use connection = getConnection()
    connection.Open()
    
    let sql = """
        SELECT t.ticket_id, t.seat_id, t.screening_id, t.user_id, t.created_at,
               u.username,
               m.movie_name,
               sc.start_time,
               se.row_number,
               se.seat_number,
               r.room_id
        FROM ticket t
        JOIN user u ON t.user_id = u.user_id
        JOIN screenings sc ON t.screening_id = sc.screening_id
        JOIN movies m ON sc.movie_id = m.movie_id
        JOIN seats se ON t.seat_id = se.seat_id
        JOIN rooms r ON se.room_id = r.room_id
        ORDER BY sc.start_time DESC, t.created_at DESC
    """
    
    connection.Query(sql) |> Seq.toList

    
let getTicketById ticketId =
    use connection = getConnection()
    connection.Open()
    
    let sql = """
        SELECT 
            t.ticket_id as TicketId, 
            s.seat_id as SeatId, 
            t.screening_id as ScreeningId, 
            t.user_id as UserId, 
            t.created_at as CreatedAt
        FROM ticket t
        JOIN seats s ON t.seat_id = s.seat_id
        WHERE t.ticket_id = @TicketId
    """
    let parameters = dict ["TicketId", box ticketId]
    connection.Query<Ticket>(sql, parameters) |> Seq.tryHead

let getUserTickets userId =
    use connection = getConnection()
    connection.Open()
    
    let sql = """
        SELECT t.ticket_id, t.seat_id, t.screening_id, t.user_id, t.created_at,
               m.movie_name, sc.start_time, se.row_number, se.seat_number
        FROM ticket t
        JOIN screenings sc ON t.screening_id = sc.screening_id
        JOIN movies m ON sc.movie_id = m.movie_id
        JOIN seats se ON t.seat_id = se.seat_id
        WHERE t.user_id = @UserId
        ORDER BY sc.start_time DESC
    """
    let parameters = dict ["UserId", box userId]
    connection.Query(sql, parameters) |> Seq.toList

let getTicketsByScreening screeningId =
    use connection = getConnection()
    connection.Open()
    
    let sql = """
        SELECT t.ticket_id, t.seat_id, t.screening_id, t.user_id, t.created_at,
               u.username,
               m.movie_name,
               sc.start_time,
               se.row_number,
               se.seat_number
        FROM ticket t
        JOIN user u ON t.user_id = u.user_id
        JOIN screenings sc ON t.screening_id = sc.screening_id
        JOIN movies m ON sc.movie_id = m.movie_id
        JOIN seats se ON t.seat_id = se.seat_id
        WHERE t.screening_id = @ScreeningId
        ORDER BY se.row_number, se.seat_number
    """
    let parameters = dict ["ScreeningId", box screeningId]
    
    connection.Query(sql, parameters) |> Seq.toList


