namespace App

open Feliz

type Components =

    /// <summary>
    /// Hypatian Enigma Hex Grid
    /// </summary>
    [<ReactComponent>]
    static member HexGrid() =
        let x = 200
        let y = 150
        let s = 10

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

        let HexAt x y s =
            Svg.polygon [
                svg.points (HexPointsString (x + 100) y s)
                svg.fill "#88c0d0"
                svg.stroke "#2e3440"
                svg.strokeWidth 3
            ]


        Html.div [
            prop.className "flex min-h-screen bg-gray-100"
            prop.children [
                Html.div [
                    prop.className "container flex flex-col gap-2 [&_h1]:text-4xl items-center mx-auto pt-12"
                    prop.children [
                        Html.h1 [ prop.text "Hypatian Enigma" ]
                        Svg.svg [
                            svg.width 600
                            svg.height 600
                            svg.children [ HexAt (x - 100) y s; HexAt (x + 100) y s ]
                        ]
                    ]
                ]
            ]
        ]