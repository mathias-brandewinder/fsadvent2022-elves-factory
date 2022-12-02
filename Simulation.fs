namespace FsAdvent2022

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

        let startTime = DateTime.Now

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

