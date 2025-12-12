module Database

open Microsoft.Data.Sqlite
open System.IO

let private connectionString = "Data Source=cinema.db"

let getConnection() =
    new SqliteConnection(connectionString)

let initializeDatabase() =
    use connection = getConnection()
    connection.Open()
    
    let createTables = """
        -- Users table
        CREATE TABLE IF NOT EXISTS user (
            user_id INTEGER PRIMARY KEY AUTOINCREMENT,
            username TEXT UNIQUE NOT NULL,
            password TEXT NOT NULL,
            role TEXT NOT NULL DEFAULT 'user'
        );

        -- Movies table
        CREATE TABLE IF NOT EXISTS movies (
            movie_id INTEGER PRIMARY KEY AUTOINCREMENT,
            movie_name TEXT NOT NULL,
            movie_pic TEXT,
            description TEXT,
            duration INTEGER NOT NULL
        );

        -- Rooms table
        CREATE TABLE IF NOT EXISTS rooms (
            room_id INTEGER PRIMARY KEY AUTOINCREMENT,
            no_rows INTEGER NOT NULL,
            no_col INTEGER NOT NULL
        );

        -- Seats table
        CREATE TABLE IF NOT EXISTS seats (
            seat_id INTEGER PRIMARY KEY AUTOINCREMENT,
            room_id INTEGER NOT NULL,
            row_number INTEGER NOT NULL,
            seat_number INTEGER NOT NULL,
            is_reserved BOOLEAN DEFAULT 0,
            FOREIGN KEY (room_id) REFERENCES rooms(room_id)
        );

        -- Screenings table
        CREATE TABLE IF NOT EXISTS screenings (
            screening_id INTEGER PRIMARY KEY AUTOINCREMENT,
            movie_id INTEGER NOT NULL,
            room_id INTEGER NOT NULL,
            start_time TEXT NOT NULL,
            end_time TEXT NOT NULL,
            FOREIGN KEY (movie_id) REFERENCES movies(movie_id),
            FOREIGN KEY (room_id) REFERENCES rooms(room_id)
        );

        -- Tickets table
        CREATE TABLE IF NOT EXISTS ticket (
            ticket_id INTEGER PRIMARY KEY AUTOINCREMENT,
            seat_id INTEGER NOT NULL,
            screening_id INTEGER NOT NULL,
            user_id INTEGER NOT NULL,
            created_at TEXT NOT NULL,
            FOREIGN KEY (seat_id) REFERENCES seats(seat_id),
            FOREIGN KEY (screening_id) REFERENCES screenings(screening_id),
            FOREIGN KEY (user_id) REFERENCES user(user_id)
        );

        -- Insert default admin user
        INSERT OR IGNORE INTO user (username, password, role) 
        VALUES ('admin', 'admin123', 'admin');
    """
    
    use command = new SqliteCommand(createTables, connection)
    command.ExecuteNonQuery() |> ignore
    
    printfn "Database initialized successfully!"