module Models

open System

// User model
[<CLIMutable>]
type User =
    { UserId: int
      Username: string
      Password: string
      Name: string option
      Role: string } // "admin" or "user"

// Movie model
[<CLIMutable>]
type Movie =
    { MovieId: int
      MovieName: string
      MoviePic: string option
      Description: string option
      Duration: int } // in minutes

// Room model
[<CLIMutable>]
type Room =
    { RoomId: int
      RoomName: string
      NoRows: int
      NoCol: int }

// Seat model
[<CLIMutable>]
type Seat =
    { SeatId: int
      RoomId: int
      RowNumber: int
      SeatNumber: int
      IsReserved: bool }

// Screening model
[<CLIMutable>]
type Screening =
    { ScreeningId: int
      MovieId: int
      RoomId: int
      StartTime: DateTime
      EndTime: DateTime }

// Ticket model
[<CLIMutable>]
type Ticket =
    { TicketId: int
      SeatId: int
      ScreeningId: int
      UserId: int
      CreatedAt: DateTime }

// View models for displaying combined data
[<CLIMutable>]
type ScreeningView =
    { ScreeningId: int
      MovieName: string
      RoomId: int
      StartTime: DateTime
      EndTime: DateTime
      Duration: int }

[<CLIMutable>]
type SeatView =
    { SeatId: int
      RowNumber: int
      SeatNumber: int
      IsReserved: bool }
