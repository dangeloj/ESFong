module EntityManagement
open System
open System.Collections.Generic

type IEntityManager =
    abstract member CreateEntity: unit -> Entity
    abstract member RemoveEntity: Entity -> bool
    abstract member AddComponent: Entity -> obj -> unit
    abstract member RemoveComponent: Entity -> obj -> bool
    abstract member GetComponent: Entity -> _
    abstract member GetEntitiesWithComponent<'c> : unit -> Entity seq

and Entity =
    { Id : Guid;  Manager : IEntityManager }
    member this.Remove() = this.Manager.RemoveEntity this
    member this.AddComponent comp = this.Manager.AddComponent this comp
    member this.RemoveComponent comp = this.Manager.RemoveComponent this comp
    member this.GetComponent<'c>() = this.Manager.GetComponent<'c> this

type EntityManager() =
    
    let entities = ResizeArray<Guid>()
    let entityComponents = Dictionary<Guid, ResizeArray<obj>>()

    interface IEntityManager with
        member this.CreateEntity() =
            let id = Guid.NewGuid()
            entities.Add id
            entityComponents.Add(id, ResizeArray())
            { Id = id; Manager = this }

        member this.RemoveEntity entity =
            let id = entity.Id
            entityComponents.Remove id && entities.Remove id

        member this.AddComponent entity comp =
            let components = entityComponents.[entity.Id]
            components.Add comp

        member this.RemoveComponent entity comp =
            let components = entityComponents.[entity.Id]
            components.Remove comp

        member this.GetComponent<'c> entity =
            let components = entityComponents.[entity.Id]
            let c = components.Find (fun c -> match c with | :? 'c -> true | _ -> false)
            c :?> 'c

        member this.GetEntitiesWithComponent<'c>() =
            seq { let ids = entities.FindAll (fun id ->
                      let components = entityComponents.[id]
                      components.Exists (fun c -> match c with | :? 'c -> true | _ -> false))
                  for id in ids do
                      yield { Id = id; Manager = this } }