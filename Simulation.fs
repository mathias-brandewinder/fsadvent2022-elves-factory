namespace FsAdvent2022

module Weibull =

    open System

    // https://en.wikipedia.org/wiki/Weibull_distribution
    type Weibull = {
        /// Shape parameter
        K: float
        /// Scale parameter
        Lambda: float
        }
        with
        member this.Simulate (rng: Random) =
            let p = rng.NextDouble ()
            this.Lambda * (- log (1.0 - p)) ** (1.0 / this.K)

module Simulation =

    open System

    type FailureCause = {
        Name: string
        TimeToFailure: Random -> TimeSpan
        TimeToFix: Random -> TimeSpan
        }

    type Incident = {
        Cause: int
        FailureTime: DateTime
        RestartTime: DateTime
        }

    let nextFailure (rng: Random) (causes: FailureCause []) =
        causes
        |> Array.mapi (fun index failure ->
            index,
            failure.TimeToFailure rng,
            failure.TimeToFix rng
            )
        |> Array.minBy (fun (_, timeToFailure, _) ->
            timeToFailure
            )

    let simulate (rng: Random) (failures: FailureCause []) =

        let startTime = DateTime(2022, 12, 24)

        startTime
        |> Seq.unfold (fun currentTime ->
            let failureIndex, nextFailure, timeToFix =
                nextFailure rng failures
            let failureTime = currentTime + nextFailure
            let restartTime = failureTime + timeToFix
            let incident = {
                Cause = failureIndex
                FailureTime = failureTime
                RestartTime = restartTime
                }
            Some (incident, restartTime)
            )

