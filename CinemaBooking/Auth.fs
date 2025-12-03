module Auth

open Models
open Session
open BookingFunctions

type AuthResult =
    | Success of User
    | IncorrectPassword
    | UserNotFound
    | UserAlreadyExists
    | SignUpSuccess of User

let mutable lastUserId = 0

let generateNewUserId () =
    lastUserId <- lastUserId + 1
    lastUserId


let signIn (username: string) (password: string) : AuthResult =
    match findUserByUsername username with
    | Some user when user.Password = password ->
        Session.currentUser <- Some user
        Success user
    | Some _ -> IncorrectPassword
    | None -> UserNotFound

let signUp (username: string) (password: string) : AuthResult =
    match findUserByUsername username with
    | Some _ -> UserAlreadyExists
    | None ->
        let newUser =
            { UserId = generateNewUserId ()
              Username = username
              Password = password }

        saveUser newUser |> ignore
        SignUpSuccess newUser

let signOut () = Session.currentUser <- None
