module Movement
open Components
open EntityManagement
open EntitySystems
open Eventing
open GameEvents
open GameView

let MakeVelocitySystem (manager:#IEntityManager) =
    let init() = ()
    let shutdown() = ()
    let frame tick =
        let getVelocity (entity:Entity) = entity.GetComponent<Velocity>()
        let getPosition (entity:Entity) = entity.GetComponent<Position>()
        let withPosition entity =
            let velocity = getVelocity entity
            let position = getPosition entity
            entity, velocity, position
        let hasVelocity (e, v, p) =
            match v with
            | Some v -> true
            | None -> false
        let removeOption (e, v:Option<Velocity>, p:Option<Position>) =
            let v', p' = v.Value, p.Value
            (e, v', p')
        let applyVelocity (e:Entity, v:Velocity, p:Position) =
            let dx = v.Dx * tick
            let dy = v.Dy * tick
            let p' = { X = p.X + dx; Y = p.Y + dy }
            e.RemoveComponentType<Position>() |> ignore
            e.AddComponent(p')

        manager.GetEntitiesWithComponent<Position>()
        |> Seq.map withPosition
        |> Seq.filter hasVelocity
        |> Seq.map removeOption
        |> Seq.iter applyVelocity

    EntitySystem(init, frame, shutdown)

let MakeMovementSystem (manager:#IEntityManager) (bus:#IEventBus) =
    let init() =
        let removeVelocity id =
            let e = manager.ToEntity id
            e.RemoveComponentType<Velocity>() |> ignore
        let tryAddVelocity mult id =
            let dy = mult * 350.f<unit/second>
            let v' = { Dx = 0.f<unit/second>; Dy = dy }
            let e = manager.ToEntity id
            removeVelocity id
            e.AddComponent v'

        bus.Stream
        |> (Event.filter <| function | :? GameCommands -> true | _ -> false)
        |> (Event.map <| fun e -> e :?> GameCommands)
        |> (Event.add <| fun e ->
            match e with
            | MoveUp(id) -> tryAddVelocity -1.f id
            | StopMoveUp(id) -> removeVelocity id
            | MoveDown(id) -> tryAddVelocity 1.f id
            | StopMoveDown(id) -> removeVelocity id
            | _ -> ())

    let shutdown() = ()
    let frame tick = ()

    EntitySystem(init, frame, shutdown)