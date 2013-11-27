module EntitySystems
open System.Diagnostics
open EntityManagement
open Eventing
open GameEvents

[<Measure>] type second

type EntitySystem (init, frame, shutdown) =
    member this.Init():unit = init()
    member this.Frame (tick:float32<second>):unit = frame tick
    member this.Shutdown():unit = shutdown()
    static member Create init frame shutdown = EntitySystem(init, frame, shutdown)
    static member InitAll (systems:EntitySystem seq) = systems |> Seq.iter (fun system -> system.Init())
    static member ShutdownAll (systems:EntitySystem seq) = systems |> Seq.iter (fun system -> system.Shutdown())
    static member BasicSystem frame =
        let empty() = ()
        EntitySystem(empty, frame, empty)

    module BasicSystems =
        open System.Threading
    
        let CreateBusSystem (bus:EventBus) =
            let init() = ()
            let shutdown() = ()
            let frame tick =
                Thread.Sleep 1
                bus.Tick()

            EntitySystem.Create init frame shutdown
            
let RunLoop token async = Async.Start(async, token)
let CreateLoop (bus:#IEventBus) systems =
    let getSeconds (watch:Stopwatch) = float32 watch.Elapsed.TotalSeconds * 1.f<second>
    let paused = ref false
            
    async { bus.Stream
            |> Event.filter (fun e -> match e with | :? GameEvents -> true | _ -> false)
            |> Event.map (fun e -> e :?> GameEvents)
            |> Event.add (function
                | Paused -> paused := true
                | Resumed -> paused := false
                | _ -> ())

            systems |> EntitySystem.InitAll
            let watch = Stopwatch.StartNew()
            let rec loop lastTick p =
                let nextTick = getSeconds watch
                let tick = nextTick - lastTick

                if not p then            
                    systems |> Seq.iter (fun system -> system.Frame tick)

                loop nextTick !paused
            loop (getSeconds watch) !paused }