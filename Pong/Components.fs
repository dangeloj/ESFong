module Components
open System.Drawing

[<Measure>] type second = EntitySystems.second

[<Measure>] type unit
let toUnit f = f * 1.0<unit>

type Position = 
    {
        X : float32<unit>
        Y : float32<unit>
    }
    member this.ToPoint() = PointF(this.X / 1.f<unit>, this.Y / 1.f<unit>)

type Velocity =
    {
        Dx : float32<unit/second>
        Dy : float32<unit/second>
    }

type Score = { Points : int }
type Controlled = Human | Computer | Remote
type Drawable =
    | Paddle of Width:float32<unit> * Height:float32<unit>
    | Ball of Radius:float32<unit>

    module Templating =
        open EntityManagement

        let createPlayer (manager : #IEntityManager) (isHuman:bool) =
            let player = manager.CreateEntity()

            player.AddComponent({ Points = 0 })
            player.AddComponent(Paddle(15.f<unit>, 50.f<unit>))

            if isHuman then
                player.AddComponent(Human)
                player.AddComponent({ X = 25.f<unit>; Y = 225.f<unit> })
            else
                player.AddComponent(Computer)
                player.AddComponent({ X = 590.f<unit>; Y = 225.f<unit> })

            player

        let createBall (manager : #IEntityManager) =
            let ball = manager.CreateEntity()

            ball.AddComponent(Ball(15.f<unit>))
            ball.AddComponent({ X = 320.f<unit>; Y = 240.f<unit> })
            ball.AddComponent({ Dx = -75.f<unit/second>; Dy = 25.f<unit/second> })

            ball