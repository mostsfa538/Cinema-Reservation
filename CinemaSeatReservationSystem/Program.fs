open System
open Models
open Database
open BookingFunctions

[<EntryPoint>]
let main argv =
    printfn "╔════════════════════════════════════════════════════════════╗"
    printfn "║     CINEMA SEAT RESERVATION SYSTEM - DATABASE VERSION      ║"
    printfn "╚════════════════════════════════════════════════════════════╝"
    printfn ""
    
    // Initialize database and tables
    printfn "Step 1: Initializing database..."
    initializeDatabase()
    printfn ""
    
    // ============================================================
    // TEST 1: SAVE USER - Testing saveUser function
    // ============================================================
    printfn "╔════════════════════════════════════════════════════════════╗"
    printfn "║ TEST 1: SAVE USER                                          ║"
    printfn "╚════════════════════════════════════════════════════════════╝"
    
    let user1 = { UserId = 0; Username = "moaz"; Password = "moaz123" }
    match saveUser user1 with
    | Some savedUser ->
        printfn "User saved successfully"
        printfn "  - User ID: %d" savedUser.UserId
        printfn "  - Username: %s" savedUser.Username
        printfn "  - Password: %s" savedUser.Password
    | None ->
        printfn "Failed to save user"
    
    printfn ""
    
    let user2 = { UserId = 0; Username = "mostafa"; Password = "mostafa123" }
    match saveUser user2 with
    | Some savedUser ->
        printfn "User saved successfully"
        printfn "  - User ID: %d" savedUser.UserId
        printfn "  - Username: %s" savedUser.Username
        printfn "  - Password: %s" savedUser.Password
    | None ->
        printfn "Failed to save user"
    
    printfn ""
    
    let user3 = { UserId = 0; Username = "marwan"; Password = "marwan123" }
    match saveUser user3 with
    | Some savedUser ->
        printfn "User saved successfully"
        printfn "  - User ID: %d" savedUser.UserId
        printfn "  - Username: %s" savedUser.Username
        printfn "  - Password: %s" savedUser.Password
    | None ->
        printfn "Failed to save user"
    
    printfn ""
    System.Threading.Thread.Sleep(1000)
    
    // ============================================================
    // TEST 2: FIND USER BY USERNAME - Testing findUserByUsername
    // ============================================================
    printfn "╔════════════════════════════════════════════════════════════╗"
    printfn "║ TEST 2: FIND USER BY USERNAME                              ║"
    printfn "╚════════════════════════════════════════════════════════════╝"
    
    printfn "Searching for user 'moaz'"
    match findUserByUsername "moaz" with
    | Some user ->
        printfn "User found"
        printfn "  - User ID: %d" user.UserId
        printfn "  - Username: %s" user.Username
        printfn "  - Password: %s" user.Password
    | None ->
        printfn "User not found"
    
    printfn ""
    
    printfn "Searching for user 'mostafa'"
    match findUserByUsername "mostafa" with
    | Some user ->
        printfn "User found"
        printfn "  - User ID: %d" user.UserId
        printfn "  - Username: %s" user.Username
    | None ->
        printfn "User not found"
    
    printfn ""
    
    printfn "Searching for non-existent user 'mohamed'"
    match findUserByUsername "mohamed" with
    | Some user ->
        printfn "User found: %s" user.Username
    | None ->
        printfn "User not found"
    
    printfn ""
    System.Threading.Thread.Sleep(1000)
    
    // ============================================================
    // TEST 3: FIND SEAT BY COORDINATES - Testing findSeatByCoordinates
    // ============================================================
    printfn "╔════════════════════════════════════════════════════════════╗"
    printfn "║ TEST 3: FIND SEAT BY COORDINATES                           ║"
    printfn "╚════════════════════════════════════════════════════════════╝"
    
    printfn "Searching for seat at Row 5, Seat 7"
    match findSeatByCoordinates 5 7 with
    | Some seat ->
        printfn "Seat found"
        printfn "  - Seat ID: %d" seat.SeatId
        printfn "  - Row: %d, Seat: %d" seat.RowNumber seat.SeatNumber
        printfn "  - Reserved: %b" seat.IsReserved
    | None ->
        printfn "Seat not found"
    
    printfn ""
    
    printfn "Searching for seat at Row 1, Seat 1"
    match findSeatByCoordinates 1 1 with
    | Some seat ->
        printfn "Seat found!"
        printfn "  - Seat ID: %d" seat.SeatId
        printfn "  - Row: %d, Seat: %d" seat.RowNumber seat.SeatNumber
        printfn "  - Reserved: %b" seat.IsReserved
    | None ->
        printfn "Seat not found"
    
    printfn ""
    
    printfn "Searching for invalid seat at Row 15, Seat 20"
    match findSeatByCoordinates 15 20 with
    | Some seat ->
        printfn "Seat found: ID=%d" seat.SeatId
    | None ->
        printfn "Seat not found"
    
    printfn ""
    System.Threading.Thread.Sleep(1000)
    
    // ============================================================
    // TEST 4: SAVE SEAT RESERVATION - Testing saveSeatReservation
    // ============================================================
    printfn "╔════════════════════════════════════════════════════════════╗"
    printfn "║ TEST 4: SAVE SEAT RESERVATION                              ║"
    printfn "╚════════════════════════════════════════════════════════════╝"
    
    printfn "Reserving seat at Row 5, Seat 7"
    match findSeatByCoordinates 5 7 with
    | Some seat ->
        match saveSeatReservation seat.SeatId with
        | Some reservedSeatId ->
            printfn "Seat reserved successfully!"
            printfn "  - Seat ID: %d" reservedSeatId
            
            // Verify the reservation
            match findSeatByCoordinates 5 7 with
            | Some updatedSeat ->
                printfn "  -Verification: Seat is now reserved = %b" updatedSeat.IsReserved
            | None -> ()
        | None ->
            printfn "Failed to reserve seat"
    | None ->
        printfn "Seat not found"
    
    printfn ""
    
    printfn "Reserving seat at Row 3, Seat 4"
    match findSeatByCoordinates 3 4 with
    | Some seat ->
        match saveSeatReservation seat.SeatId with
        | Some _ ->
            printfn "Seat reserved successfully!"
        | None ->
            printfn "Failed to reserve seat"
    | None ->
        printfn "Seat not found"
    
    printfn ""
    
    printfn "Trying to reserve the same seat again (Row 5, Seat 7)"
    match findSeatByCoordinates 5 7 with
    | Some seat ->
        match saveSeatReservation seat.SeatId with
        | Some _ ->
            printfn "Seat reserved"
        | None ->
            printfn "Cannot reserve seat already reserved"
    | None ->
        printfn "Seat not found"
    
    printfn ""
    System.Threading.Thread.Sleep(1000)
    
    // ============================================================
    // TEST 5: SAVE TICKET - Testing saveTicket
    // ============================================================
    printfn "╔════════════════════════════════════════════════════════════╗"
    printfn "║ TEST 5: SAVE TICKET                                        ║"
    printfn "╚════════════════════════════════════════════════════════════╝"
    
    printfn "Creating ticket for moaz at Row 5, Seat 7"
    match findUserByUsername "moaz" with
    | Some user ->
        match findSeatByCoordinates 5 7 with
        | Some seat ->
            let ticket1 = {
                TicketId = Guid.NewGuid().ToString()
                SeatId = seat.SeatId
                UserId = user.UserId
            }
            match saveTicket ticket1 with
            | Some savedTicket ->
                printfn "Ticket created successfully"
                printfn "  - Ticket ID: %s" savedTicket.TicketId
                printfn "  - User: %s (ID: %d)" user.Username user.UserId
                printfn "  - Seat: Row %d, Seat %d (ID: %d)" seat.RowNumber seat.SeatNumber seat.SeatId
            | None ->
                printfn "Failed to create ticket"
        | None ->
            printfn "Seat not found"
    | None ->
        printfn "User not found"
    
    printfn ""
    
    printfn "Creating ticket for mostafa at Row 3, Seat 4"
    match findUserByUsername "mostafa" with
    | Some user ->
        match findSeatByCoordinates 3 4 with
        | Some seat ->
            let ticket2 = {
                TicketId = Guid.NewGuid().ToString()
                SeatId = seat.SeatId
                UserId = user.UserId
            }
            match saveTicket ticket2 with
            | Some savedTicket ->
                printfn "Ticket created successfully"
                printfn "  - Ticket ID: %s" savedTicket.TicketId
                printfn "  - User: %s (ID: %d)" user.Username user.UserId
                printfn "  - Seat: Row %d, Seat %d (ID: %d)" seat.RowNumber seat.SeatNumber seat.SeatId
            | None ->
                printfn "Failed to create ticket"
        | None ->
            printfn "Seat not found"
    | None ->
        printfn "User not found"
    
    printfn ""
    System.Threading.Thread.Sleep(1000)
    
    // ============================================================
    // SUMMARY: Display all data from database
    // ============================================================
    printfn "╔════════════════════════════════════════════════════════════╗"
    printfn "║ DATABASE SUMMARY                                           ║"
    printfn "╚════════════════════════════════════════════════════════════╝"
    
    
    printfn "\n--- RESERVED SEATS ---"
    let allSeats = getAllSeats()
    let reservedSeats = allSeats |> List.filter (fun s -> s.IsReserved)
    reservedSeats |> List.iter (fun s ->
        printfn "  Seat ID: %d | Row: %d, Seat: %d" s.SeatId s.RowNumber s.SeatNumber
    )
    
    
    0 // Exit code