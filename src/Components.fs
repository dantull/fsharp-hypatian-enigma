namespace App

open Feliz

type Tile = {
    Id: int
    Value: int
    CenterX: int
    CenterY: int
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
        let xOffset = 15 * s // x offset for the first column
        let yStride = 13 * s // y spacing between rows
        let yOffset = 15 * s // y offset for the first row

        let HexRow = fun count x y -> [ for i in 0 .. count - 1 -> (x + i * xStride, y) ]

        let centers =
            List.concat [
                HexRow 3 xOffset yOffset
                HexRow 4 (xOffset - rowShift) (yOffset + yStride)
                HexRow 5 (xOffset - 2 * rowShift) (yOffset + 2 * yStride)
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

        let HexAt t =
            Svg.polygon [
                svg.points (HexPointsString (t.CenterX + 100) t.CenterY s)
                svg.fill "#88c0d0"
                svg.stroke "#2e3440"
                svg.strokeWidth 3
                svg.onClick (fun e -> printfn "Clicked at %d" t.Id)
            ]


        Html.div [
            prop.className "flex min-h-screen bg-gray-100"
            prop.children [
                Html.div [
                    prop.className "container flex flex-col gap-2 [&_h1]:text-4xl items-center mx-auto pt-12"
                    prop.children [
                        Html.h1 [ prop.text "Hypatian Enigma" ]
                        Svg.svg [ svg.width 800; svg.height 800; svg.children (List.map HexAt board) ]
                    ]
                ]
            ]
        ]