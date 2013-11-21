module EntitySystems
open System.Diagnostics
open EntityManagement

[<Measure>] type second
let toSecond f = f * 1.0<second>

type EntitySystem (init, frame, shutdown) =
    member this.Init():unit = init()
    member this.Frame (tick:float<second>):unit = frame tick
    member this.Shutdown():unit = shutdown()
    static member Create init frame shutdown = EntitySystem(init, frame, shutdown)
    static member InitAll (systems:EntitySystem seq) = systems |> Seq.iter (fun system -> system.Init())
    static member ShutdownAll (systems:EntitySystem seq) = systems |> Seq.iter (fun system -> system.Shutdown())

    module Systems =
        open System.Threading
        open Eventing
    
        let CreateBusSystem (bus:EventBus) =
            let init() = ()
            let shutdown() = ()
            let frame tick =
                Thread.Sleep 1
                bus.Tick()

            EntitySystem.Create init frame shutdown
        
        let CreateLoop systems =
            async {
                systems |> EntitySystem.InitAll

                let watch = Stopwatch.StartNew()
                let rec loop lastTick =
                    let nextTick = watch.Elapsed.TotalSeconds
                    let tick = toSecond(nextTick - lastTick)
            
                    systems |> Seq.iter (fun system -> system.Frame tick)

                    loop nextTick
                loop 0.  
            }

        let RunLoop token async = Async.Start(async, token)

    module Templating =
        open EntityManagement

        type PlayerTypes = Human | Computer | Remote

        let createPlayer (manager : #IEntityManager) playerType =
            let player = manager.CreateEntity()

            match playerType with
            | Human -> ()
            | Computer -> ()
            | Remote -> ()

            player