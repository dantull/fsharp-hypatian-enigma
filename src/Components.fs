namespace App

open Feliz

type Tile = {
    Id: int
    Value: int
    CenterX: int
    CenterY: int
}

type Msg = TileClicked of int

type State = {
    Tiles: Tile list
    Selected: int option
    Sums: Tile list
}

type Components =

    /// <summary>
    /// Hypatian Enigma Hex Grid
    /// </summary>
    [<ReactComponent>]
    static member HexGrid() =
        let s = 10

        let xStride = 15 * s // same row x spacing
        let rowShift = xStride / 2 // x shift for odd rows
        let xOffset = 25 * s // x offset for the first column
        let yStride = 13 * s // y spacing between rows
        let yOffset = 25 * s // y offset for the first row

        let HexRow = fun count x y -> [ for i in 0 .. count - 1 -> (x + i * xStride, y) ]

        let centers =
            List.concat [
                HexRow 3 xOffset yOffset
                HexRow 4 (xOffset - rowShift) (yOffset + yStride)
                HexRow 5 (xOffset - xStride) (yOffset + 2 * yStride)
                HexRow 4 (xOffset - rowShift) (yOffset + 3 * yStride)
                HexRow 3 xOffset (yOffset + 4 * yStride)
            ]

        let board =
            centers
            |> List.mapi (fun i (x, y) -> {
                Id = i
                Value = i + 1
                CenterX = x
                CenterY = y
            })

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
            [ 0; 1; 2 ], (xOffset - xStride, yOffset)
            [ 3; 4; 5; 6 ], (xOffset - rowShift - xStride, yOffset + yStride)
            [ 7; 8; 9; 10; 11 ], (xOffset - 2 * xStride, yOffset + 2 * yStride)
            [ 12; 13; 14; 15 ], (xOffset - rowShift - xStride, yOffset + 3 * yStride)
            [ 16; 17; 18 ], (xOffset - xStride, yOffset + 4 * yStride)
            // Down and to the Right
            [ 0; 4; 9; 14; 18 ], (xOffset + xStride * 2 + rowShift, yOffset + 5 * yStride)
            [ 1; 5; 10; 15 ], (xOffset + xStride * 3, yOffset + 4 * yStride)
            [ 2; 6; 11 ], (xOffset + xStride * 3 + rowShift, yOffset + 3 * yStride)
            [ 3; 8; 13; 17 ], (xOffset + xStride + rowShift, yOffset + 5 * yStride)
            [ 7; 12; 16 ], (xOffset + rowShift, yOffset + 5 * yStride)
            // Down and to the Left
            [ 0; 3; 7 ], (xOffset + rowShift, yOffset - yStride)
            [ 1; 4; 8; 12 ], (xOffset + xStride + rowShift, yOffset - yStride)
            [ 2; 5; 9; 13; 16 ], (xOffset + 2 * xStride + rowShift, yOffset - yStride)
            [ 6; 10; 14; 17 ], (xOffset + 3 * xStride, yOffset)
            [ 11; 15; 18 ], (xOffset + 3 * xStride + rowShift, yOffset + yStride)
        ]

        let makeSums tiles =
            sums
            |> List.map (fun (ids, pos) -> {
                Id = 0
                Value = addUp ids tiles
                CenterX = pos |> fst
                CenterY = pos |> snd
            })

        let guideLines board =
            sums
            |> List.map (fun (ids, pos) ->
                let points = pos :: collectPoints ids board

                let pointString =
                    points
                    |> List.map (fun (x, y) -> sprintf "%d,%d" (x + 100) (y - 20))
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

            {
                state with
                    Tiles = newTiles
                    Selected = None
                    Sums = makeSums newTiles
            }

        let initialState = {
            Tiles = board
            Selected = None
            Sums = makeSums board
        }

        let state, setState = React.useState (initialState)

        let update msg state =
            match msg, state.Selected with
            | TileClicked id, Some selectedId -> swapHelper id selectedId state
            | TileClicked id, _ -> { state with Selected = Some id }

        let dispatch msg = setState (update msg state)

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

        let allSums = sums |> List.map (fun (ids, _) -> addUp ids state.Tiles)
        printfn "All Sums: %A" allSums

        let guideLinesSvg = guideLines state.Tiles

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
                        svg.points (HexPointsString (t.CenterX + 100) t.CenterY s)
                        svg.fill (if isSelected then "#88cfff" else "#88c0d0")
                        svg.stroke "#2e3440"
                        svg.fillOpacity 0.6
                        svg.strokeWidth (if isSelected then 4 else 2)
                    ]
                    Svg.text [
                        svg.x (t.CenterX + 100)
                        svg.y (t.CenterY - 20)
                        svg.textAnchor.middle
                        svg.dominantBaseline.middle
                        svg.fontSize 20
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
                        svg.x (sum.CenterX + 100)
                        svg.y (sum.CenterY - 20)
                        svg.textAnchor.middle
                        svg.dominantBaseline.middle
                        svg.fontSize 16
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
                        Html.h1 [ prop.text "Hypatian Enigma" ]
                        Svg.svg [
                            svg.width 1000
                            svg.height 1000
                            svg.children (
                                List.concat [
                                    guideLinesSvg
                                    List.map (fun t -> HexAt t state.Selected) state.Tiles
                                    List.map SumAt state.Sums
                                ]
                            )
                        ]
                    ]
                ]
            ]
        ]