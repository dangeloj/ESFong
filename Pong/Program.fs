open System
open System.Diagnostics
open System.Drawing
open System.Windows.Forms

open EntityManagement
open EntitySystems
open EntitySystems.Templating
open Eventing
open GameEvents
open GameView

[<STAThread>]
do
    use source = new System.Threading.CancellationTokenSource()

    Application.EnableVisualStyles()
    Application.SetCompatibleTextRenderingDefault(false)

    let bus = EventBus()
    let manager = EntityManager()
    let player = createPlayer manager Human
    let computer = createPlayer manager Computer
    let systems = [| Systems.CreateBusSystem bus |]

    Systems.CreateLoop systems
        |> Systems.RunLoop source.Token

    use form = new GameForm(bus, player.Id)
    Application.Run(form)

    source.Cancel()
    systems |> EntitySystem.ShutdownAll