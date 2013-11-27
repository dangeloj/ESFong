open System
open System.Diagnostics
open System.Drawing
open System.Windows.Forms

open Components.Templating
open Drawing
open EntityManagement
open EntitySystems
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
    let humanPlayer = createPlayer manager true
    let computer = createPlayer manager false
    let ball = createBall manager
    use form = new GameForm(bus, humanPlayer.Id)
    let systems =
        [| BasicSystems.CreateBusSystem bus;
           Movement.MakeMovementSystem manager bus;
           Movement.MakeVelocitySystem manager;
           MakeDrawingSystem manager form |]
           
    CreateLoop bus systems
    |> RunLoop source.Token

    Application.Run(form)

    source.Cancel()
    systems |> EntitySystem.ShutdownAll