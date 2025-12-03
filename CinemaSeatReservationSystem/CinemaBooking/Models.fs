module Models

type User = {
    UserId: int
    Username: string
    Password: string
}

type Seat = {
    SeatId: int
    RowNumber: int
    SeatNumber: int
    IsReserved: bool
}

type Ticket = {
    TicketId: string
    SeatId: int
    UserId: int
}
