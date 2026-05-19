namespace App

open System

// Core types used across the entire game
type Tile = {
    Id: int
    Value: int
    CenterX: int
    CenterY: int
}

type SavedTile = { SavedId: int; SavedValue: int }
type Msg = TileClicked of int

type BoardModel = {
    Tiles: Tile list
    Selected: int option
}

type State = {
    Model: BoardModel
    Sums: Tile list
    Remaining: int
}

module HypatianEngine =

    // 1. Math and grid layout logic
    module Geometry =
        let s = 8
        let xStride = 18 * s
        let rowShift = xStride / 2
        let xOffset = 25 * s
        let yStride = 16 * s
        let yOffset = 25 * s
        let width = xStride * 7
        let height = yStride * 7

        let axialCoord q r =
            let x = xOffset + (q * xStride) + (r * rowShift)
            let y = yOffset + (r * yStride)
            (x, y)

        let hexRow count q r = [ for i in 0 .. count - 1 -> axialCoord (q + i) r ]

        let translate (xShift, yShift) (x, y) = (x + xShift, y + yShift)

        let hexPoints x y s = [
            (x, y - s * 10)
            (x + s * 7, y - s * 6)
            (x + s * 7, y + s * 2)
            (x, y + s * 6)
            (x - s * 7, y + s * 2)
            (x - s * 7, y - s * 6)
        ]

    // 2. Game Board rules and state modifiers
    module Board =
        open Geometry

        let private centers =
            // actually should be in Board, but we need it to compute the center
            List.concat [ hexRow 3 0 0; hexRow 4 -1 1; hexRow 5 -2 2; hexRow 4 -2 3; hexRow 3 -2 4 ]

        let private centerHex = centers |> List.item 9

        let private boardCenterOffset =
            (width / 2 - fst centerHex, height / 2 - snd centerHex)

        let private board =
            centers
            |> List.map (translate boardCenterOffset)
            |> List.mapi (fun i (x, y) -> {
                Id = i
                Value = i + 1
                CenterX = x
                CenterY = y
            })

        let private hexCoord q r =
            axialCoord q r |> translate boardCenterOffset

        let private sumsDefinition = [
            // Rows
            [ 0; 1; 2 ], hexCoord -1 0
            [ 3; 4; 5; 6 ], hexCoord -2 1
            [ 7; 8; 9; 10; 11 ], hexCoord -3 2
            [ 12; 13; 14; 15 ], hexCoord -3 3
            [ 16; 17; 18 ], hexCoord -3 4
            // Down and to the Right
            [ 0; 4; 9; 14; 18 ], hexCoord 0 5
            [ 1; 5; 10; 15 ], hexCoord 1 4
            [ 2; 6; 11 ], hexCoord 2 3
            [ 3; 8; 13; 17 ], hexCoord -1 5
            [ 7; 12; 16 ], hexCoord -2 5
            // Down and to the Left
            [ 0; 3; 7 ], hexCoord 1 -1
            [ 1; 4; 8; 12 ], hexCoord 2 -1
            [ 2; 5; 9; 13; 16 ], hexCoord 3 -1
            [ 6; 10; 14; 17 ], hexCoord 3 0
            [ 11; 15; 18 ], hexCoord 3 1
        ]

        let private addUp ids tiles =
            ids
            |> List.map (fun id -> tiles |> List.find (fun t -> t.Id = id))
            |> List.sumBy (fun t -> t.Value)

        let private makeSums tiles =
            sumsDefinition
            |> List.map (fun (ids, pos) -> {
                Id = 0
                Value = addUp ids tiles
                CenterX = fst pos
                CenterY = snd pos
            })

        let private computeRemaining sums =
            sums |> List.sumBy (fun s -> Math.Abs(s.Value - 38))

        let private hydrate (model: BoardModel) : State =
            let nextSums = makeSums model.Tiles

            {
                Model = model
                Sums = nextSums
                Remaining = computeRemaining nextSums
            }

        let private collectPoints ids tiles =
            ids
            |> List.map (fun id -> tiles |> List.find (fun t -> t.Id = id))
            |> List.map (fun t -> (t.CenterX, t.CenterY))

        let getGuideLinePaths tiles =
            sumsDefinition
            |> List.map (fun (ids, pos) ->
                // Combine the sum label position with the constituent tile centers
                pos :: collectPoints ids tiles
            )

        // State Transitions
        let selectTile id model =
            { model with Selected = Some id } |> hydrate

        let updateFromSavedTiles (savedTiles: SavedTile list) (model: BoardModel) =
            let savedTileMap =
                savedTiles |> List.map (fun st -> (st.SavedId, st.SavedValue)) |> Map.ofList

            let newTiles =
                model.Tiles
                |> List.map (fun t ->
                    match Map.tryFind t.Id savedTileMap with
                    | Some savedValue -> { t with Value = savedValue }
                    | None -> t
                )

            { model with Tiles = newTiles } |> hydrate

        let swapTiles id1 id2 model =
            let tiles = model.Tiles
            let tile1 = tiles |> List.find (fun t -> t.Id = id1)
            let tile2 = tiles |> List.find (fun t -> t.Id = id2)

            let newTiles =
                tiles
                |> List.map (fun t ->
                    if t.Id = id1 then { t with Value = tile2.Value }
                    elif t.Id = id2 then { t with Value = tile1.Value }
                    else t
                )

            {
                model with
                    Tiles = newTiles
                    Selected = None
            }
            |> hydrate

        let shuffleTiles model =
            let rnd = Random()

            let shuffledValues =
                model.Tiles |> List.map (fun t -> t.Value) |> List.sortBy (fun _ -> rnd.Next())

            let savedTiles =
                model.Tiles
                |> List.map (fun t -> t.Id)
                |> List.zip shuffledValues
                |> List.map (fun (v, id) -> { SavedId = id; SavedValue = v })

            updateFromSavedTiles savedTiles { model with Selected = None }

        let initialState () =
            { Tiles = board; Selected = None } |> hydrate