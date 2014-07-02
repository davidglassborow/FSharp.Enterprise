//#I @"D:\dev\FSharp.Enterprise\packages\FSharp.Charting.0.90.6"
//#load "FSharp.Charting.fsx"
//open FSharp.Charting
//
//#I @"D:\dev\FSharp.Enterprise\packages\MathNet.Numerics.3.0.2\lib\net40"
//#r "MathNet.Numerics.dll"
//open MathNet.Numerics.Distributions
//
//#I @"D:\dev\FSharp.Enterprise\bin"
//#r "FSharp.Enterprise.dll"
//open FSharp.Enterprise
//
//open System
//
//let toChartLine line =
//    Line.toPoints line
//    |> Seq.map (fun p -> Point.x p, Point.y p)
//    |> Chart.Line
//
//let plotLine line =
//    (toChartLine line).ShowChart()
//
//let plotLines lines =
//    Chart.Combine(lines |> Seq.map toChartLine).ShowChart()
//
///// Generates points using geometric Brownian motion
/////  - 'seed' specifies the seed for random number generator
/////  - 'drift' and 'volatility' set properties of the value movement
/////  - 'initial' and 'start' specify the initial value and date
/////  - 'span' specifies time span between individual observations
/////  - 'count' is the number of required values to generate
//let randomPoints seed drift volatility initial start span count =
//    let dist = Normal(0.0, 1.0, RandomSource=Random(seed))  
//    let dt = (span:TimeSpan).TotalDays / 250.0
//    let driftExp = (drift - 0.5 * pown volatility 2) * dt
//    let randExp = volatility * (sqrt dt)
//    ((start:DateTimeOffset), initial) |> Seq.unfold (fun (dt, price) ->
//        let price = price * exp (driftExp + randExp * dist.Sample()) 
//        Some((dt, price), (dt + span, price))) |> Seq.take count
//
//let testLine lineGen span count =
//    lineGen span count
//    |> Seq.map Point.make
//    |> Seq.pairwise
//    |> Line.make (Segment.Continuous)
//
//let today = DateTimeOffset(DateTime.Today)
//let lineGen1 = randomPoints 1 5.0 100.0 20.0 today
//let lineGen2 = randomPoints 2 200.0 100.0 20.0 today
//let flatLineGen = randomPoints 3 0.0 0.0 20.0 today
//
//let lineHorizontalFlat = testLine flatLineGen (TimeSpan.FromMinutes 5.) 20
//let lineHorizontalFlat' = Line.Time.reducePoints 0.0 lineHorizontalFlat
//plotLines [lineHorizontalFlat; lineHorizontalFlat']
//
//let line1 = testLine lineGen2 (TimeSpan.FromMinutes 5.) 200
//let line1' = Line.Time.reducePoints 0.0 line1
//Line.segments line1 |> Array.length
//Line.segments line1' |> Array.length
//plotLines [line1; line1']
//        