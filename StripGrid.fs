namespace StripGrid

open System
open System.Windows
open System.Windows.Controls
open System.Windows.Media

[<AutoOpen>]
module Rendering = 
    let run rate update =
        let rate = TimeSpan.FromSeconds(rate)
        let lastUpdate = ref DateTime.Now
        let residual = ref (TimeSpan())
        CompositionTarget.Rendering.Subscribe (fun _ -> 
            let now = DateTime.Now
            residual := !residual + (now - !lastUpdate)
            while !residual > rate do
                update(); residual := !residual - rate
            lastUpdate := now
        )

open System.Windows.Controls.Primitives

type AppControl() =
  inherit UserControl(Width = 1600.0, Height = 900.0)
  let colors = [Colors.Yellow; Colors.White; Colors.LightGray; Colors.Cyan; Colors.Orange]

  let canvas = Canvas()

  let bar = ScrollBar(Width=1600.0)
  do  bar.ViewportSize <- 1600.0 * 2.0
  do  bar.Value <- 1.0
  do  bar.Maximum <- 4000.0

  do  bar.Orientation <- Orientation.Horizontal
  do  Canvas.SetTop(bar, 800.0)

  let rowHeader = Canvas(Background=SolidColorBrush(Colors.Black), Width=40.0,Height=1200.0)
  do  for y = 1 to 50 do
        let text = TextBlock(Text="Row " + y.ToString())
        text.Foreground <- SolidColorBrush(Colors.White)
        Canvas.SetTop(text, 16.0* float y)
        rowHeader.Children.Add text

  let colHeader = Canvas(Background=SolidColorBrush(Colors.Black), Width=1600.0*4.0,Height=16.0)
  do  for x = 1 to 160 do
        let text = TextBlock(Text="Col " + x.ToString())
        text.Foreground <- SolidColorBrush(Colors.White)
        Canvas.SetLeft(text, 40.0 * float x)
        colHeader.Children.Add text

  let clip = RectangleGeometry(Rect=Rect(Width=1000.0,Height=800.0))
  //do canvas.Clip <- clip
  let rand = Random()
  let strips =
    [for i in 0..42 -> 
        let brush = SolidColorBrush(colors.[i%colors.Length])
        let strip = Canvas(Background = brush, Width=40.0, Height=1200.0)
        let r = System.Windows.Shapes.Rectangle()
        r.Width <- 1.0
        r.Height <- 1200.0
        r.Fill <- SolidColorBrush(Colors.Black)
        strip.Children.Add(r)
        let clip = RectangleGeometry(Rect=Rect(Width=1000.0,Height=800.0))
        strip.CacheMode <- BitmapCache()
        Canvas.SetLeft(strip, float i * 40.0)
        let prices =
            [for y = 1 to 50 do
                let text = TextBlock()
                text.Text <- i.ToString() + "  " + y.ToString()
                Canvas.SetTop(text, 16.0* float y)
                
                let r = System.Windows.Shapes.Rectangle()
                r.Width <- 40.0
                r.Height <- 1.0
                r.Fill <- SolidColorBrush(Colors.Black)
                Canvas.SetTop(r, float y * 16.0)
                
                strip.Children.Add(text)
                strip.Children.Add(r)
                yield text

                ]
        strip, prices]
  do strips |> List.iter (fst >> canvas.Children.Add)

  do  canvas.Children.Add rowHeader
  do  canvas.Children.Add colHeader
  do  canvas.Children.Add bar

  do  base.Content <- canvas

  let mutable x = 0.0
  let rand = Random()
  do  run (1.0/100.0) (fun () ->
            for strip,_ in strips do
                let x = Canvas.GetLeft(strip) - 8.0
                let x = if x < -80.0 then 1600.0 else x
                Canvas.SetLeft(strip,x)
            Canvas.SetLeft(colHeader, x)
            x <- x - 8.0
            for i = 1 to 5 do
                let strip, prices = strips.[rand.Next(strips.Length)]
                for i = 1 to 4 do
                    prices.[rand.Next(prices.Length)].Text <- rand.Next(5000).ToString()
      )
      |> ignore