namespace App

open Browser.Dom
open Thoth.Json
open Feliz

type Tile = {
    Id: int
    Value: int
    CenterX: int
    CenterY: int
}

type SavedTile = { SavedId: int; SavedValue: int }

type Msg = TileClicked of int

type State = {
    Tiles: Tile list
    Selected: int option
    Sums: Tile list
    Remaining: int
}

type Components =

    /// <summary>
    /// Hypatian Enigma Hex Grid
    /// </summary>
    [<ReactComponent>]
    static member HexGrid() =
        let s = 8

        let xStride = 18 * s // same row x spacing
        let rowShift = xStride / 2 // x shift for odd rows
        let xOffset = 25 * s // x offset for the first column
        let yStride = 16 * s // y spacing between rows
        let yOffset = 25 * s // y offset for the first row
        let width = xStride * 7
        let height = yStride * 7

        let axialCoord q r =
            let x = xOffset + (q * xStride) + (r * rowShift)
            let y = yOffset + (r * yStride)
            (x, y)


        let HexRow = fun count q r -> [ for i in 0 .. count - 1 -> axialCoord (q + i) r ]

        let centers =
            List.concat [ HexRow 3 0 0; HexRow 4 -1 1; HexRow 5 -2 2; HexRow 4 -2 3; HexRow 3 -2 4 ]

        let centerHex = centers |> List.item 9
        let boardCenterOffset = (width / 2 - fst centerHex, height / 2 - snd centerHex)

        let shiftedCenters =
            centers
            |> List.map (fun (x, y) -> (x + fst boardCenterOffset, y + snd boardCenterOffset))

        let board =
            shiftedCenters
            |> List.mapi (fun i (x, y) -> {
                Id = i
                Value = i + 1
                CenterX = x
                CenterY = y
            })

        let hexCoord q r =
            let x, y = axialCoord q r
            (x + fst boardCenterOffset, y + snd boardCenterOffset)

        let toSavedTile (t: Tile) = { SavedId = t.Id; SavedValue = t.Value }
        let toSavedTiles (tiles: Tile list) = tiles |> List.map toSavedTile

        let addUp ids tiles =
            ids
            |> List.map (fun id -> tiles |> List.find (fun t -> t.Id = id))
            |> List.sumBy (fun t -> t.Value)

        let collectPoints =
            fun ids tiles ->
                ids
                |> List.map (fun id -> tiles |> List.find (fun t -> t.Id = id))
                |> List.map (fun t -> (t.CenterX, t.CenterY))

        let sums = [
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

        let makeSums tiles =
            sums
            |> List.map (fun (ids, pos) -> {
                Id = 0
                Value = addUp ids tiles
                CenterX = pos |> fst
                CenterY = pos |> snd
            })

        let computeRemaining sums =
            sums
            |> List.sumBy (fun s -> if s.Value >= 38 then s.Value - 38 else 38 - s.Value)

        let updateFromSavedTiles (savedTiles: SavedTile list) (state: State) =
            let savedTileMap =
                savedTiles |> List.map (fun st -> (st.SavedId, st.SavedValue)) |> Map.ofList

            let newTiles =
                state.Tiles
                |> List.map (fun t ->
                    match Map.tryFind t.Id savedTileMap with
                    | Some savedValue -> { t with Value = savedValue }
                    | None -> t
                )

            let sums = makeSums newTiles

            {
                state with
                    Tiles = newTiles
                    Sums = sums
                    Remaining = computeRemaining sums

            }

        let guideLines board =
            sums
            |> List.map (fun (ids, pos) ->
                let points = pos :: collectPoints ids board

                let pointString =
                    points
                    |> List.map (fun (x, y) -> sprintf "%d,%d" x (y - 20))
                    |> String.concat " "

                Svg.g [
                    svg.children [
                        Svg.polyline [
                            svg.points pointString
                            svg.fill "none"
                            svg.stroke "#2e3440"
                            svg.strokeWidth 60
                            svg.strokeLineCap "round"
                            svg.strokeLineJoin "round"
                        ]
                        Svg.polyline [
                            svg.points pointString
                            svg.fill "none"
                            svg.stroke "#d8dee9"
                            svg.strokeWidth 56
                            svg.strokeLineCap "round"
                            svg.strokeLineJoin "round"
                        ]
                    ]
                ]
            )

        let swapHelper id1 id2 state =
            let tiles = state.Tiles
            let tile1 = tiles |> List.find (fun t -> t.Id = id1)
            let tile2 = tiles |> List.find (fun t -> t.Id = id2)

            let newTiles =
                tiles
                |> List.map (fun t ->
                    if t.Id = id1 then { t with Value = tile2.Value }
                    elif t.Id = id2 then { t with Value = tile1.Value }
                    else t
                )

            let sums = makeSums newTiles

            {
                state with
                    Tiles = newTiles
                    Selected = None
                    Sums = sums
                    Remaining = computeRemaining sums
            }

        let shuffleTiles state =
            let rnd = System.Random()

            let shuffledValues =
                state.Tiles |> List.map (fun t -> t.Value) |> List.sortBy (fun _ -> rnd.Next())

            let savedTiles =
                state.Tiles
                |> List.map (fun t -> t.Id)
                |> List.zip shuffledValues
                |> List.map (fun (value, id) -> { SavedId = id; SavedValue = value })

            updateFromSavedTiles savedTiles { state with Selected = None }

        let sums = makeSums board

        let initialState = {
            Tiles = board
            Selected = None
            Sums = sums
            Remaining = computeRemaining sums
        }

        let persistKey = "hypatian-enigma-state"

        let tryLocalStorage () =
            try
                let storage = window.localStorage
                if isNull storage then None else Some storage
            with _ ->
                None

        let maybeUpdate state =
            match tryLocalStorage () with
            | None -> state
            | Some storage ->
                match storage.getItem (persistKey) with
                | null -> state
                | json ->
                    match Decode.Auto.fromString<SavedTile list> json with
                    | Ok tiles -> updateFromSavedTiles tiles state
                    | Error err ->
                        console.error ("Failed to decode saved state:", err)
                        state

        let state, setState = React.useState (maybeUpdate initialState)

        let persistState state =
            let json = Encode.Auto.toString (0, toSavedTiles state.Tiles)

            match tryLocalStorage () with
            | Some storage -> storage.setItem (persistKey, json)
            | None -> ()

            state

        let update msg state =
            match msg, state.Selected with
            | TileClicked id, Some selectedId -> persistState (swapHelper id selectedId state)
            | TileClicked id, _ -> { state with Selected = Some id }

        let dispatch msg = setState (update msg state)

        let actionButton (text: string) onClick =
            Html.button [
                prop.className "px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600"
                prop.onClick onClick
                prop.children [ Html.text text ]
            ]

        let HexPointsString x y s =
            sprintf
                "%d,%d %d,%d %d,%d %d,%d %d,%d %d,%d"
                (x)
                (y - s * 10)
                (x + s * 7)
                (y - s * 6)
                (x + s * 7)
                (y + s * 2)
                (x)
                (y + s * 6)
                (x - s * 7)
                (y + s * 2)
                (x - s * 7)
                (y - s * 6)

        let HexAt t selected =
            let isSelected =
                match selected with
                | Some id when id = t.Id -> true
                | _ -> false

            Svg.g [
                svg.key (string t.Id)
                svg.onClick (fun _ -> dispatch (TileClicked t.Id))
                svg.children [
                    Svg.polygon [
                        svg.points (HexPointsString t.CenterX t.CenterY s)
                        svg.fill (if isSelected then "#88cfff" else "#88c0d0")
                        svg.stroke "#2e3440"
                        svg.fillOpacity 0.6
                        svg.strokeWidth (if isSelected then 4 else 2)
                    ]
                    Svg.text [
                        svg.x t.CenterX
                        svg.y (t.CenterY - 20)
                        svg.textAnchor.middle
                        svg.dominantBaseline.middle
                        svg.fontSize 30
                        svg.fill "#2e3440"
                        svg.children [ Html.text (string t.Value) ]
                    ]
                ]
            ]

        let SumAt sum =
            Svg.g [
                svg.key (string sum.CenterX + "," + string sum.CenterY)
                svg.children [
                    Svg.text [
                        svg.x sum.CenterX
                        svg.y (sum.CenterY - 20)
                        svg.textAnchor.middle
                        svg.dominantBaseline.middle
                        svg.fontSize 30
                        svg.fill (if sum.Value = 38 then "#073553" else "#bf616a")
                        svg.children [ Html.text (string sum.Value) ]
                    ]
                ]
            ]

        Html.div [
            prop.className "flex min-h-screen bg-gray-100"
            prop.children [
                Html.div [
                    prop.className "container flex flex-col gap-2 [&_h1]:text-4xl items-center mx-auto pt-12"
                    prop.children [
                        Html.div [
                            prop.className "flex flex-col items-center gap-4"
                            prop.children [
                                Html.div [
                                    prop.className "flex justify-center w-full px-4 sm:px-6"
                                    prop.children [
                                        Svg.svg [
                                            svg.className "max-w-full h-auto"
                                            svg.viewBox (sprintf "0 0 %d %d" width height)
                                            svg.width width
                                            svg.height height
                                            svg.children (
                                                List.concat [
                                                    guideLines state.Tiles
                                                    List.map (fun t -> HexAt t state.Selected) state.Tiles
                                                    List.map SumAt state.Sums
                                                ]
                                            )
                                        ]
                                    ]
                                ]
                                Html.div [
                                    prop.className "flex flex-wrap justify-center gap-2"
                                    prop.children [
                                        actionButton
                                            "Reset"
                                            (fun _ ->
                                                window.localStorage.removeItem persistKey
                                                setState initialState
                                            )
                                        Html.span [
                                            prop.className "blue-500 font-bold"
                                            prop.style [ style.fontSize 24 ]
                                            prop.children [ Html.text (sprintf "%d" state.Remaining) ]
                                        ]
                                        actionButton
                                            "Shuffle"
                                            (fun _ ->
                                                window.localStorage.removeItem persistKey
                                                setState (shuffleTiles initialState)
                                            )
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]