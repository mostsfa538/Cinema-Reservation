open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Giraffe
open Database
open Repository
open WebApi

let initializeSampleData() =
    printfn "Initializing database..."
    initializeDatabase()
    
    let existingMovies = getAllMovies()
    if existingMovies.IsEmpty then
        printfn "Adding sample movies..."
        
        createMovie "The Matrix" (Some "https://images.unsplash.com/photo-1489599849927-2ee91cede3ba?w=400&h=600&fit=crop") (Some "A computer hacker learns from mysterious rebels about the true nature of his reality.") 136
        createMovie "Inception" (Some "https://images.unsplash.com/photo-1440404653325-ab127d49abc1?w=400&h=600&fit=crop") (Some "A skilled thief is given a chance at redemption.") 148
        createMovie "Interstellar" (Some "https://images.unsplash.com/photo-1485846234645-a62644f84728?w=400&h=600&fit=crop") (Some "A team of explorers travel through a wormhole in space.") 169
        createMovie "The Dark Knight" (Some "https://images.unsplash.com/photo-1478720568477-152d9b164e26?w=400&h=600&fit=crop") (Some "When the menace known as the Joker wreaks havoc on Gotham.") 152
        createMovie "Pulp Fiction" (Some "https://images.unsplash.com/photo-1518676590629-3dcbd9c5a5c9?w=400&h=600&fit=crop") (Some "The lives of two mob hitmen, a boxer, and a pair of diner bandits intertwine.") 154
        
        printfn "Sample movies added!"
    
    let existingRooms = getAllRooms()
    if existingRooms.IsEmpty then
        printfn "Creating cinema rooms..."
        
        createRoom "Main Hall" 10 10 |> ignore
        createRoom "VIP Theater" 8 8 |> ignore
        createRoom "Small Screen" 6 6 |> ignore
        
        printfn "Cinema rooms created!"
    
    let existingScreenings = getAllScreenings()
    if existingScreenings.IsEmpty then
        printfn "Scheduling screenings..."
        
        let movies = getAllMovies()
        let rooms = getAllRooms()
        
        if not movies.IsEmpty && not rooms.IsEmpty then
            let today = DateTime.Now
            
            createScreening movies.[0].MovieId rooms.[0].RoomId (today.AddHours(2.0)) |> ignore
            createScreening movies.[0].MovieId rooms.[1].RoomId (today.AddHours(6.0)) |> ignore
            createScreening movies.[1].MovieId rooms.[0].RoomId (today.AddHours(4.0)) |> ignore
            createScreening movies.[1].MovieId rooms.[2].RoomId (today.AddHours(8.0)) |> ignore
            createScreening movies.[2].MovieId rooms.[1].RoomId (today.AddHours(3.0)) |> ignore
            createScreening movies.[0].MovieId rooms.[0].RoomId (today.AddDays(1.0).AddHours(2.0)) |> ignore
            createScreening movies.[1].MovieId rooms.[1].RoomId (today.AddDays(1.0).AddHours(4.0)) |> ignore
            createScreening movies.[2].MovieId rooms.[2].RoomId (today.AddDays(1.0).AddHours(6.0)) |> ignore
            
            printfn "Screenings scheduled!"
    
    printfn "Database initialization complete!\n"

let configureServices (services: IServiceCollection) =
    services.AddCors(fun options ->
        options.AddPolicy("AllowAll", fun builder ->
            builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
            |> ignore
        )
    ) |> ignore
    services.AddGiraffe() |> ignore

let configureApp (app: IApplicationBuilder) =
    app.UseCors("AllowAll") |> ignore
    app.UseGiraffe(webApp)

[<EntryPoint>]
let main args =
    initializeSampleData()
    
    printfn "Starting Cinema Booking API Server..."
    printfn "Server running at: http://localhost:5000"
    printfn "API Documentation:"
    printfn "   - POST   /api/register"
    printfn "   - POST   /api/login"
    printfn "   - GET    /api/movies"
    printfn "   - GET    /api/rooms"
    printfn "   - GET    /api/screenings"
    printfn "   - GET    /api/screenings/upcoming"
    printfn "   - GET    /api/screenings/{id}/seats"
    printfn "   - POST   /api/book"
    printfn "   - GET    /api/users/{id}/tickets"
    printfn "   - Admin routes: /api/admin/*"
    printfn "\nPress Ctrl+C to stop the server\n"
    
    let port = 
        match System.Environment.GetEnvironmentVariable("PORT") with
        | null | "" -> "5000"
        | p -> p
    
    let url = sprintf "http://0.0.0.0:%s" port
    
    Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(fun webBuilder ->
            webBuilder
                .UseUrls(url)
                .Configure(configureApp)
                .ConfigureServices(configureServices)
            |> ignore
        )
        .Build()
        .Run()
    
    0