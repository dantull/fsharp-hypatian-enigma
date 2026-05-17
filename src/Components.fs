namespace App

open Feliz

type Components =

    /// <summary>
    /// Hypatian Enigma Hex Grid
    /// </summary>
    [<ReactComponent>]
    static member HexGrid() =
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
                                    svg.points "150,50 220,90 220,170 150,210 80,170 80,90"
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