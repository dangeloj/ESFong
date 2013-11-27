module GameView

open System
open System.Drawing
open System.Windows.Forms

open GameEvents
open Eventing

type GameForm(bus:IEventBus, player) as this =
    inherit Form(Text = "Pong", Width = 640, Height = 480, MaximizeBox = false)
    
    let buffer = BufferedGraphicsManager.Current.Allocate(this.CreateGraphics(), this.DisplayRectangle)
    let width, height = this.Width, this.Height
    let invoke f =
        let action = Action(fun() -> f this)
        if this.IsDisposed then ()
        else this.Invoke action |> ignore
    let mutable paused = false

    do
        this.BackColor <- Color.CornflowerBlue
        this.KeyDown
        |> Event.add (fun e ->
            match e.KeyCode with
            | Keys.Up -> MoveUp(player) |> bus.Notify
            | Keys.Down -> MoveDown(player) |> bus.Notify
            | Keys.Escape -> this.RequestShutdown()
            | Keys.P -> bus.Notify (if paused then Resume else Pause)
            | _ -> ())
        this.KeyUp
        |> Event.add (fun e ->
            match e.KeyCode with
            | Keys.Up -> StopMoveUp(player) |> bus.Notify
            | Keys.Down -> StopMoveDown(player) |> bus.Notify
            | _ -> ())
        bus.Stream
        |> Event.filter (function | :? GameCommands -> true | _ -> false)
        |> Event.map (fun e -> e :?> GameCommands)
        |> Event.add (fun e ->
            match e with
            | Pause -> paused <- true; invoke (fun f -> f.Text <- "Pong [Paused]")
            | Resume -> paused <- false; invoke (fun f -> f.Text <- "Pong")
            | Quit -> invoke (fun f -> f.Close())
            | _ -> ())

    override this.Dispose(disposing) =
        buffer.Dispose()
        base.Dispose(disposing)

    member this.RenderFrame (f:Graphics->unit) =
        invoke <|
            fun form ->
                buffer.Graphics.FillRectangle(Brushes.CornflowerBlue, 0, 0, width, height)
                f buffer.Graphics
                buffer.Render()

    member this.RequestShutdown() =
        bus.Notify Pause
        if MessageBox.Show("Are you sure?", "Quit", MessageBoxButtons.YesNo) = DialogResult.Yes then
            bus.Notify Quit