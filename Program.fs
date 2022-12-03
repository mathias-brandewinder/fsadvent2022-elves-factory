﻿namespace FsAdvent2022

module App =

    open System
    open FsAdvent2022.Weibull
    open FsAdvent2022.Simulation
    open Plotly.NET

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

        // What do we observe?

        let sample =
            tape
            |> Seq.pairwise
            |> Seq.filter (fun (_, incident) -> incident.Cause = 0)
            |> Seq.take 1000
            |> Seq.map (fun (previousIncident, incident) ->
                (incident.FailureTime - previousIncident.RestartTime).TotalHours
                )

        sample
        |> Chart.Histogram
        |> Chart.withTitle "Ribbon: time to failure"
        |> Chart.withXAxisStyle "time (hours)"
        |> Chart.withYAxisStyle "number of failures"
        |> Chart.show

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

        let cumulative (weibull: Weibull) =
            fun time ->
                1.0
                - exp (- ((time / weibull.Lambda) ** weibull.K))

        let density (weibull: Weibull) =
            fun (time: float) ->
                (weibull.K / weibull.Lambda)
                *
                ((time / weibull.Lambda) ** (weibull.K - 1.0))
                *
                exp (- ((time / weibull.Lambda) ** weibull.K))

        let times = [ 0.2 .. 0.2 .. 5.0 ]

        [
            Chart.Line(xy = (times |> List.map (fun t -> t, cumulative weibullRibbon t)), Name = "Ribbon")
            Chart.Line(xy = (times |> List.map (fun t -> t, cumulative weibullPaper t)), Name = "Paper")

        ]
        |> Chart.combine
        |> Chart.withXAxisStyle "Time (hours)"
        |> Chart.withYAxisStyle "Proba already failed"
        |> Chart.show

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

        // Maximum Likelihood Estimation

        let sample =
            tape
            |> Seq.take 1000
            |> Seq.toArray

        let likelihood (k, lambda) sample =
            let weibull = { K = k; Lambda = lambda }
            sample
            |> Array.sumBy (fun (observed, time) ->
                if observed
                then density weibull time |> log
                else (1.0 - cumulative weibull time) |> log
                )

        let ribbonSample =
            sample
            |> Estimation.prepare 0
            |> Array.take 100

        let ks = [ 0.5 .. 0.05 .. 2.0 ]
        let lambdas = [ 0.5 .. 0.05 .. 2.0 ]

        let z =
            [
                for k in ks ->
                    [ for lambda in lambdas -> likelihood (k, lambda) ribbonSample ]
            ]

        Chart.Surface (
            z,
            X = ks,
            Y = lambdas,
            Contours = TraceObjects.Contours.initXyz(Show = true)
            )
        |> Chart.withTitle "Ribbon: log likelihood"
        |> Chart.withXAxisStyle ("k", Id = StyleParam.SubPlotId.Scene 1)
        |> Chart.withYAxisStyle ("lambda", Id = StyleParam.SubPlotId.Scene 1)
        |> Chart.withZAxisStyle "log likelihood"
        |> Chart.show

        let paperSample =
            sample
            |> Estimation.prepare 1
            |> Array.take 100

        let z =
            [
                for k in ks ->
                    [ for lambda in lambdas -> likelihood (k, lambda) paperSample ]
            ]

        Chart.Surface (
            z,
            X = ks,
            Y = lambdas,
            Contours = TraceObjects.Contours.initXyz(Show = true)
            )
        |> Chart.withTitle "Paper: log likelihood"
        |> Chart.withXAxisStyle ("k", Id = StyleParam.SubPlotId.Scene 1)
        |> Chart.withYAxisStyle ("lambda", Id = StyleParam.SubPlotId.Scene 1)
        |> Chart.withZAxisStyle "log likelihood"
        |> Chart.show

        let ribbon =
            sample
            |> Estimation.prepare 0
            |> Estimation.estimate

        printfn "Ribbon: %A" ribbon

        let paper =
            sample
            |> Estimation.prepare 1
            |> Estimation.estimate

        printfn "Paper: %A" paper

        0
