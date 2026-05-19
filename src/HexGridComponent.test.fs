module Tests.Components

open Vitest

Vitest.describe (
    "HexGrid",
    fun () ->
        Vitest.test (
            "renders the hex grid svg",
            fun () -> promise {
                let result = RTL.render (App.HexGridComponent.HexGrid())
                let container = result.container
                let svg = container.querySelector ("svg")

                Vitest.expect(svg).toBeTruthy ()
            }
        )
)