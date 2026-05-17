namespace App

open Feliz

type Components =

    /// <summary>
    /// Hypatian Enigma Hex Grid
    /// </summary>
    [<ReactComponent>]
    static member HexGrid() =
        let x = 150
        let y = 150

        let HexPointsString x y =
            sprintf
                "%d,%d %d,%d %d,%d %d,%d %d,%d %d,%d"
                (x)
                (y - 100)
                (x + 70)
                (y - 60)
                (x + 70)
                (y + 20)
                (x)
                (y + 60)
                (x - 70)
                (y + 20)
                (x - 70)
                (y - 60)

        Html.div [
            prop.className "flex min-h-screen bg-gray-100"
            prop.children [
                Html.div [
                    prop.className "container flex flex-col gap-2 [&_h1]:text-4xl items-center mx-auto pt-12"
                    prop.children [
                        Html.h1 [ prop.text "Hypatian Enigma" ]
                        Svg.svg [
                            svg.width 300
                            svg.height 300
                            svg.children [
                                Svg.polygon [
                                    svg.points (HexPointsString x y)
                                    svg.fill "#88c0d0"
                                    svg.stroke "#2e3440"
                                    svg.strokeWidth 3
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]