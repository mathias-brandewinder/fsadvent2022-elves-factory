namespace FsAdvent2022

module App =

    open System
    open FsAdvent2022.Weibull
    open FsAdvent2022.Simulation

    [<EntryPoint>]
    let main (args: string []) =
        printfn "F# Advent 2022"

        // Simple tape: uniform distribution
        printfn "Incidents: Uniform distribution"

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

        // Weibull tape
        printfn "Incidents: Weibull distribution"

        let hours = TimeSpan.FromHours

        let weibullRibbon = {
            K = 1.2
            Lambda = 0.8
            }

        let weibullPaper = {
            K = 0.6
            Lambda = 1.2
            }

        let causes =
            [|
                {
                    Name = "Ribbon"
                    TimeToFailure = weibullRibbon.Simulate >> hours
                    TimeToFix = fun rng ->  TimeSpan.FromHours (rng.NextDouble())
                }
                {
                    Name = "Paper"
                    TimeToFailure = weibullPaper.Simulate >> hours
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
