module TicketSystem

open Models
open Session
open Repository
open System

type TicketDetails =
    { TicketId: int
      SeatId: int
      RowNumber: int
      SeatNumber: int
      ScreeningId: int
      UserId: int
      Username: string
      CreatedAt: System.DateTime }


let createTicket (user: User) (seat: Seat) (screening: Screening) : Ticket =
    { TicketId = 0
      SeatId = seat.SeatId
      ScreeningId = screening.ScreeningId
      UserId = user.UserId
      CreatedAt = DateTime.UtcNow }

let addTicket (seat: Seat) (screening: Screening) : TicketDetails =
    match Session.currentUser with
    | Some user ->
        match saveTicket (createTicket user seat screening) with
        | Some ticket ->
            printfn "Ticket booked! ID: %d" ticket.TicketId

            { TicketId = ticket.TicketId
              SeatId = seat.SeatId
              RowNumber = seat.RowNumber
              SeatNumber = seat.SeatNumber
              ScreeningId = screening.ScreeningId
              UserId = user.UserId
              Username = user.Username
              CreatedAt = ticket.CreatedAt }
        | None -> failwith "Failed to save ticket"
    | None ->
        printfn "You must sign in first!"
        failwith "No signed-in user"
