module Drawing
open Components
open EntityManagement
open EntitySystems
open GameView

let MakeDrawingSystem (manager:#IEntityManager) (form:GameForm) =
    let toF32 x = x / 1.f<unit>
    let drawBall (g:System.Drawing.Graphics) (p:Position) (r:float32<unit>) =
        let r' = toF32 r
        let x, y = toF32 p.X, toF32 p.Y
        g.FillEllipse(System.Drawing.Brushes.DarkBlue, x, y, r', r')

    let drawPaddle (g:System.Drawing.Graphics) (p:Position) (w, h) =
        let w', h' = toF32 w, toF32 h
        let x, y = toF32 p.X, toF32 p.Y
        g.FillRectangle(System.Drawing.Brushes.DarkBlue, x, y, w', h')

    let init() = ()
    let shutdown() = ()
    let frame tick =
        let getPointF (entity:Entity) =
            let p = entity.GetComponent<Position>().Value
            let x, y = p.X / 1.f<unit>, p.Y / 1.f<unit>
            System.Drawing.PointF(x, y)

        
        form.RenderFrame (fun g ->
            manager.GetEntitiesWithComponent<Drawable>()
            |> Seq.iter (fun e ->
                let d = e.GetComponent<Drawable>()
                let p = e.GetComponent<Position>()
                match (d, p) with
                | (Some d', Some p') ->
                    match d' with
                    | Ball(r) -> drawBall g p.Value r
                    | Paddle(w, h) -> drawPaddle g p.Value (w, h)
                | (None, _) | (_, None) -> ()))
        
    EntitySystem(init, frame, shutdown)