module AuthTests

open System
open Models
open Auth
open Database
open Repository

let testUserRegistration() =
    printfn "Testing user registration..."
    
    let username = sprintf "testuser_%d" (DateTime.Now.Ticks)
    let password = "test123"
    
    match signUp username password (Some "Test User") with
    | SignUpSuccess user ->
        printfn "PASS: User registered successfully"
        assert (user.Username = username)
        true
    | _ ->
        printfn "FAIL: User registration failed"
        false

let testUserLogin() =
    printfn "Testing user login..."
    
    let username = sprintf "logintest_%d" (DateTime.Now.Ticks)
    let password = "pass123"
    
    match signUp username password (Some "Login Test") with
    | SignUpSuccess _ ->
        match signIn username password with
        | Success user ->
            printfn "PASS: User login successful"
            assert (user.Username = username)
            true
        | _ ->
            printfn "FAIL: Login failed"
            false
    | _ ->
        printfn "FAIL: Setup failed"
        false

let testInvalidPassword() =
    printfn "Testing invalid password..."
    
    let username = sprintf "passtest_%d" (DateTime.Now.Ticks)
    
    match signUp username "correct" (Some "Pass Test") with
    | SignUpSuccess _ ->
        match signIn username "wrong" with
        | IncorrectPassword ->
            printfn "PASS: Invalid password detected"
            true
        | _ ->
            printfn "FAIL: Should reject wrong password"
            false
    | _ ->
        printfn "FAIL: Setup failed"
        false

let testDuplicateUser() =
    printfn "Testing duplicate user prevention..."
    
    let username = sprintf "duptest_%d" (DateTime.Now.Ticks)
    
    match signUp username "pass" (Some "Dup Test") with
    | SignUpSuccess _ ->
        match signUp username "pass" (Some "Dup Test") with
        | UserAlreadyExists ->
            printfn "PASS: Duplicate user prevented"
            true
        | _ ->
            printfn "FAIL: Should prevent duplicate"
            false
    | _ ->
        printfn "FAIL: Setup failed"
        false

let runAllTests() =
    printfn "\n=== Running Authentication Tests ===\n"
    
    initializeDatabase()
    
    let results = [
        testUserRegistration()
        testUserLogin()
        testInvalidPassword()
        testDuplicateUser()
    ]
    
    let passed = results |> List.filter id |> List.length
    let total = results.Length
    
    printfn "\n=== Results: %d/%d tests passed ===" passed total
    passed = total
