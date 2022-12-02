namespace FsAdvent2022

module App =

    open System
    open FsAdvent2022.Simulation

    [<EntryPoint>]
    let main (args: string []) =
        printfn "F# Advent 2022"

        let causes =
            [|
                {
                    Name = "Ribbon"
                    TimeToFailure = fun rng -> TimeSpan.FromHours (rng.NextDouble())
                    TimeToFix = fun rng ->  TimeSpan.FromHours (rng.NextDouble())
                }
                {
                    Name = "Paper"
                    TimeToFailure = fun rng -> TimeSpan.FromHours (rng.NextDouble())
                    TimeToFix = fun rng ->  TimeSpan.FromHours (rng.NextDouble())
                }
            |]

        let rng = Random 0

        let tape =
            causes
            |> simulate rng

        let sample =
            tape
            |> Seq.take 10
            |> Seq.iter (fun incident -> printfn "%A" incident)

        0
