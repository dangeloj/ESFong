module Eventing

open System
open System.Collections.Generic

type IEventBus =
    abstract member Stream : IEvent<obj>
    abstract member Notify : obj -> unit

[<AutoOpen>]
module private Internal =
    type BusCommand =
        | FireEvents
        | QueueEvent of Event:obj

type EventBus() =
    let e = new Event<obj>()
    let fire = e.Publish

    let actor = MailboxProcessor.Start(fun inbox ->
        let list = List<obj>()
        let rec loop (events:List<obj>) =
            async { let! msg = inbox.Receive()
                    match msg with
                    | FireEvents ->
                        if events.Count <> 0 then
                            events.ForEach(fun event -> e.Trigger event)
                            events.Clear()
                    | QueueEvent event ->
                        events.Add event
                    return! loop events }
        loop list)

    member this.Tick() = actor.Post(FireEvents)

    interface IEventBus with
        member this.Notify event = actor.Post (QueueEvent(event))
        member this.Stream = fire