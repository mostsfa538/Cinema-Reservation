module Admin

open System
open Models
open Repository


let getMovies () : Movie list = getAllMovies ()

let AddMovie (movie: Movie) =
    createMovie movie.MovieName movie.MoviePic movie.Description movie.Duration

let getAllMoviesAdmin () : Movie list = getAllMovies ()


let getMovie (movieId: int) : Movie option = getMovieById movieId

let UpdateMovie (movie: Movie) =
    match getMovie movie.MovieId with
    | Some _ -> updateMovie movie.MovieId movie.MovieName movie.MoviePic movie.Description movie.Duration
    | None -> false

let DeleteMovie (movieId: int) =
    match getMovie movieId with
    | Some movie -> deleteMovie movieId
    | None -> false

let getAllScreeningsAdmin () : ScreeningView list = getAllScreenings ()

let AddScreening (screening: Screening) =
    createScreening screening.MovieId screening.RoomId screening.StartTime

let getScreening (screeningId: int) : Screening option = getScreeningById screeningId

let UpdateScreening (screening: Screening) = updateScreening screening

let DeleteScreening (screeningId: int) =
    match getScreening screeningId with
    | Some _ -> deleteScreening screeningId
    | None -> false
