module Auth

open Models
open Session
open Repository

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

let signUp (username: string) (password: string) (name: string option) : AuthResult =
    match findUserByUsername username with
    | Some _ -> UserAlreadyExists
    | None ->
        let role =
            if username = "admin" && password = "admin123" then
                "admin"
            else
                "user"

        let newUser =
            { UserId = generateNewUserId ()
              Username = username
              Password = password
              Name = name
              Role = role }

        match saveUser newUser with
        | Some _ ->
            printfn "User signed up: %s" username
            SignUpSuccess newUser
        | None -> UserAlreadyExists



let signOut () = Session.currentUser <- None
