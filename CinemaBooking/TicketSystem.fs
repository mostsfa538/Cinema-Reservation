module TicketSystem

open Models
open Session
open BookingFunctions

let generateTicketId () : string = System.Guid.NewGuid().ToString()

let createTicket (user: User) (seat: Seat) : Ticket =
    { TicketId = generateTicketId ()
      SeatId = seat.SeatId
      UserId = user.UserId }

let addTicket (seat: Seat) : Ticket =
    match Session.currentUser with
    | Some user ->
        match saveTicket (createTicket user seat) with
        | Some ticket ->
            printfn "Ticket booked! ID: %s" ticket.TicketId
            ticket
        | None -> failwith "Failed to save ticket"
    | None ->
        printfn "You must sign in first!"
        failwith "No signed-in user"
