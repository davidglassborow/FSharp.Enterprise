#load "Option.fs"
open FSharp.Enterprise
#load "DateTime.fs"
open FSharp.Enterprise
#load "Interval.fs"
open FSharp.Enterprise

let intervals = [
        Interval.make(0,100)
        Interval.make(100,200)
        Interval.make(200,300)
        Interval.make(300,400)
        Interval.make(400,500)
    ]

intervals
|> List.filter 
    (Interval.Integer.intersects 
        IntervalType.T.LeftClosedRightOpen (Interval.make(100,300)) 
        IntervalType.T.LeftClosedRightOpen) 
