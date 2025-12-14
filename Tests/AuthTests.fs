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

let testEmptyUsername() =
    printfn "Testing empty username..."
    
    match signUp "" "password123" (Some "Empty User") with
    | SignUpSuccess _ ->
        printfn "FAIL: Should reject empty username"
        false
    | _ ->
        printfn "PASS: Empty username rejected"
        true

let testEmptyPassword() =
    printfn "Testing empty password..."
    
    let username = sprintf "emptypass_%d" (DateTime.Now.Ticks)
    
    match signUp username "" (Some "Empty Pass") with
    | SignUpSuccess _ ->
        printfn "FAIL: Should reject empty password"
        false
    | _ ->
        printfn "PASS: Empty password rejected"
        true

let testWhitespaceUsername() =
    printfn "Testing whitespace-only username..."
    
    match signUp "   " "password123" (Some "Space User") with
    | SignUpSuccess _ ->
        printfn "FAIL: Should reject whitespace username"
        false
    | _ ->
        printfn "PASS: Whitespace username rejected"
        true

let testNullDisplayName() =
    printfn "Testing null display name handling..."
    
    let username = sprintf "nullname_%d" (DateTime.Now.Ticks)
    
    match signUp username "pass123" None with
    | SignUpSuccess user ->
        if user.DisplayName = username then
            printfn "PASS: Null display name defaults to username"
            true
        else
            printfn "FAIL: Display name should default to username"
            false
    | _ ->
        printfn "FAIL: Should allow null display name"
        false

let testSqlInjectionUsername() =
    printfn "Testing SQL injection in username..."
    
    let maliciousUsername = "admin'; DROP TABLE Users; --"
    
    match signUp maliciousUsername "pass123" (Some "Hacker") with
    | SignUpSuccess _ ->
        // Try to login to verify database wasn't corrupted
        match signIn maliciousUsername "pass123" with
        | Success _ ->
            printfn "PASS: SQL injection prevented"
            true
        | _ ->
            printfn "FAIL: Database may be corrupted"
            false
    | _ ->
        printfn "PASS: Malicious username rejected"
        true

let testVeryLongUsername() =
    printfn "Testing very long username..."
    
    let longUsername = String.replicate 1000 "a"
    
    match signUp longUsername "pass123" (Some "Long User") with
    | SignUpSuccess _ ->
        printfn "FAIL: Should reject extremely long username"
        false
    | _ ->
        printfn "PASS: Long username rejected"
        true

let testVeryLongPassword() =
    printfn "Testing very long password..."
    
    let username = sprintf "longpass_%d" (DateTime.Now.Ticks)
    let longPassword = String.replicate 10000 "a"
    
    match signUp username longPassword (Some "Long Pass") with
    | SignUpSuccess _ ->
        printfn "FAIL: Should reject extremely long password"
        false
    | _ ->
        printfn "PASS: Long password rejected"
        true

let testCaseSensitiveUsername() =
    printfn "Testing case sensitivity in username..."
    
    let username = sprintf "CaseTest_%d" (DateTime.Now.Ticks)
    
    match signUp username "pass123" (Some "Case Test") with
    | SignUpSuccess _ ->
        match signUp (username.ToLower()) "pass123" (Some "Case Test 2") with
        | UserAlreadyExists ->
            printfn "FAIL: Usernames should be case-sensitive (or implementation is inconsistent)"
            false
        | SignUpSuccess _ ->
            printfn "PASS: Case-sensitive usernames allowed"
            true
        | _ ->
            printfn "FAIL: Unexpected result"
            false
    | _ ->
        printfn "FAIL: Setup failed"
        false

let testSpecialCharactersUsername() =
    printfn "Testing special characters in username..."
    
    let specialUsername = "user@#$%^&*()"
    
    match signUp specialUsername "pass123" (Some "Special User") with
    | SignUpSuccess _ ->
        printfn "WARN: Special characters allowed in username (may be intentional)"
        true
    | _ ->
        printfn "PASS: Special characters rejected"
        true

let testUnicodeUsername() =
    printfn "Testing unicode in username..."
    
    let unicodeUsername = sprintf "用户_%d" (DateTime.Now.Ticks)
    
    match signUp unicodeUsername "pass123" (Some "Unicode User") with
    | SignUpSuccess user ->
        match signIn unicodeUsername "pass123" with
        | Success _ ->
            printfn "PASS: Unicode username supported"
            true
        | _ ->
            printfn "FAIL: Unicode username breaks login"
            false
    | _ ->
        printfn "WARN: Unicode username not supported"
        true

let testConcurrentRegistration() =
    printfn "Testing concurrent registration..."
    
    let username = sprintf "concurrent_%d" (DateTime.Now.Ticks)
    
    let task1 = async { return signUp username "pass1" (Some "User 1") }
    let task2 = async { return signUp username "pass2" (Some "User 2") }
    
    let results = 
        [task1; task2]
        |> Async.Parallel
        |> Async.RunSynchronously
    
    let successes = results |> Array.filter (function SignUpSuccess _ -> true | _ -> false) |> Array.length
    
    if successes = 1 then
        printfn "PASS: Only one concurrent registration succeeded"
        true
    else
        printfn "FAIL: Race condition detected - %d registrations succeeded" successes
        false

let testPasswordHashConsistency() =
    printfn "Testing password hash consistency..."
    
    let username = sprintf "hashtest_%d" (DateTime.Now.Ticks)
    let password = "consistent123"
    
    match signUp username password (Some "Hash Test") with
    | SignUpSuccess _ ->
        let user1 = getUserByUsername username
        let user2 = getUserByUsername username
        
        match user1, user2 with
        | Some u1, Some u2 ->
            if u1.PasswordHash = u2.PasswordHash then
                printfn "PASS: Password hash consistent"
                true
            else
                printfn "FAIL: Password hash inconsistent"
                false
        | _ ->
            printfn "FAIL: Could not retrieve user"
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
        testEmptyUsername()
        testEmptyPassword()
        testWhitespaceUsername()
        testNullDisplayName()
        testSqlInjectionUsername()
        testVeryLongUsername()
        testVeryLongPassword()
        testCaseSensitiveUsername()
        testSpecialCharactersUsername()
        testUnicodeUsername()
        testConcurrentRegistration()
        testPasswordHashConsistency()
    ]
    
    let passed = results |> List.filter id |> List.length
    let total = results.Length
    
    printfn "\n=== Results: %d/%d tests passed ===" passed total
    passed = total