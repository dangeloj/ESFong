module GameEvents
open System

type GameCommands =
    | MoveUp of Player:Guid
    | MoveDown of Player:Guid
    | Pause | Resume | Quit

type GameEvents =
    | PointScored of Player:Guid
    | Paused
    | Resumed
    | GameOver of Winner:Guid