module Eventing
open System

type IEventBus =
    abstract member Stream : IEvent<obj>
    abstract member Notify : obj -> unit

[<AutoOpen>]
module private Internal =
    type BusCommand = FireEvents | QueueEvent of obj

type EventBus() =
    let e = new Event<obj>()
    let fire = e.Publish
    let run events = events |> Seq.iter (fun event -> e.Trigger event)
    let f events = function | FireEvents -> run events; Seq.empty
                            | QueueEvent event -> Seq.append events [ event ]

    let actor = MailboxProcessor.Start(fun inbox ->
        let rec loop events =
            async { let! msg = inbox.Receive()
                    let next = f events msg
                    return! loop next }
        loop Seq.empty)

    member this.Tick() = actor.Post(FireEvents)

    interface IEventBus with
        member this.Notify event = actor.Post (QueueEvent(event))
        member this.Stream = fire