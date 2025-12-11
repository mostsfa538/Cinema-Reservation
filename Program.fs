open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Giraffe
open Database
open SeatLayout
open WebApi

[<EntryPoint>]
let main argv =
    // Initialize database and seed seats
    printfn "Initializing database..."
    initializeDatabase ()
    seedSeatsIfEmpty 10 10
    printfn "Database initialized. Starting web server..."
    printfn ""

    let builder = WebApplication.CreateBuilder(argv)
    
    // Add services
    builder.Services.AddGiraffe() |> ignore
    builder.Services.AddCors(fun options ->
        options.AddPolicy("AllowAll", fun builder ->
            builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
            |> ignore
        )
    ) |> ignore

    let app = builder.Build()

    // Configure pipeline
    app.UseCors("AllowAll") |> ignore
    app.UseGiraffe(webApp)

    printfn "╔════════════════════════════════════════════════════════════╗"
    printfn "║     CINEMA SEAT RESERVATION SYSTEM - WEB API              ║"
    printfn "╚════════════════════════════════════════════════════════════╝"
    printfn ""
    printfn "Server running at: http://localhost:5000"
    printfn "API Endpoints:"
    printfn "  POST /api/register - Register new user"
    printfn "  POST /api/login - User login"
    printfn "  GET  /api/seats - Get all seats"
    printfn "  POST /api/book - Book seats"
    printfn "  GET  /api/bookings/:userId - Get user bookings"
    printfn ""
    printfn "Press Ctrl+C to stop the server."
    printfn ""

    app.Run()
    0
