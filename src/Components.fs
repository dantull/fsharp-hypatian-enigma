namespace App

open Feliz

type Components =

    /// <summary>
    /// Hypatian Enigma Hex Grid
    /// </summary>
    [<ReactComponent>]
    static member HexGrid() =
        let xStride = 150 // same row x spacing
        let rowShift = 75 // x shift for odd rows
        let xOffset = 50 // x offset for the first column
        let yStride = 130 // y spacing between rows
        let yOffset = 150 // y offset for the first row

        let centers = [
            (xOffset, yOffset)
            (xOffset + xStride, yOffset)
            (xOffset + 2 * xStride, yOffset)
            (xOffset + rowShift, yOffset + yStride)
            (xOffset + xStride + rowShift, yOffset + yStride)
        ]

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
                            svg.children (List.map (fun (x, y) -> HexAt x y s) centers)
                        ]
                    ]
                ]
            ]
        ]