namespace App

open Browser.Dom
open Thoth.Json
open Feliz
open App.HypatianEngine // Open your new module!

type Components =

    [<ReactComponent>]
    static member HexGrid() =
        // Bring in your sizing constants from Geometry
        let s = Geometry.s
        let width = Geometry.width
        let height = Geometry.height

        // Helpers for mapping core state into JSON
        let toSavedTile (t: Tile) = { SavedId = t.Id; SavedValue = t.Value }
        let toSavedTiles (tiles: Tile list) = tiles |> List.map toSavedTile

        let guideLines board =
            Board.getGuideLinePaths board
            |> List.map (fun points ->
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

        let toPointsString (points: (int * int) list) =
            points |> List.map (fun (x, y) -> sprintf "%d,%d" x y) |> String.concat " "

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
                    | Ok tiles -> Board.updateFromSavedTiles tiles state
                    | Error err ->
                        console.error ("Failed to decode saved state:", err)
                        state

        let defaultState = Board.initialState ()
        let state, setState = React.useState (maybeUpdate defaultState)

        let persistState state =
            let json = Encode.Auto.toString (0, toSavedTiles state.Tiles)

            match tryLocalStorage () with
            | Some storage -> storage.setItem (persistKey, json)
            | None -> ()

            state

        let update msg state =
            match msg, state.Selected with
            | TileClicked id, Some selectedId -> persistState (Board.swapHelper id selectedId state)
            | TileClicked id, _ -> { state with Selected = Some id }

        let dispatch msg = setState (update msg state)

        let actionButton (text: string) onClick =
            Html.button [
                prop.className "px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600"
                prop.onClick onClick
                prop.children [ Html.text text ]
            ]

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
                        svg.points ((Geometry.hexPoints t.CenterX t.CenterY s) |> toPointsString)
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
                                                setState defaultState
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
                                                setState (Board.shuffleTiles defaultState)
                                            )
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]