open System
open Models
open Database
open Repository
open Auth
open TicketSystem
open Session

[<EntryPoint>]

initializeDatabase ()

match signUp "mokhtar" "1234" with
| SignUpSuccess user -> printfn "User signed up: %s" user.Username
| UserAlreadyExists -> printfn "User already exists"
| _ -> ()

match signIn "mokhtar" "1234" with
| Success user -> printfn "Signed in as: %s" user.Username
| _ -> failwith "Sign-in failed"


let seat =
    { SeatId = 1
      RoomId = 1
      RowNumber = 1
      SeatNumber = 1
      IsReserved = false }

let screening =
    { ScreeningId = 1
      MovieId = 1
      RoomId = 1
      StartTime = DateTime.Now
      EndTime = DateTime.Now.AddHours 2.0 }

let ticket = addTicket seat screening
printfn "%A" ticket
