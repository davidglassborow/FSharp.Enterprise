namespace FSharp.Enterprise

#if INTERACTIVE
open FSharp.Enterprise
#endif

module Point =

    open System.Collections.Generic

    type T<'x,'y> = {
        X : 'x
        Y : 'y
    }

    let make (x,y) = { X = x; Y = y }
    let empty x = make (x, Unchecked.defaultof<'y>)
    let x p = p.X
    let y p = p.Y
    let mapX f p = make (f p.X,p.Y)
    let mapY f p = make (p.X,f p.Y)

    let inline dx (p1: T<'x,'y>) (p2: T<'x,'y>) =
        p1.X - p2.X

    let inline dy (p1: T<'x,'y>) (p2: T<'x,'y>) =
        p1.Y - p2.Y

    /// An iterative implementation of the Ramer-Douglas-Peucker algorithm.
    /// http://www.mappinghacks.com/code/PolyLineReduction/
    let ramerDouglasPeucker dxF dyF epsilon (points: T<'x,'y> array) =
        let anchorIndex = 0
        let floaterIndex = points.Length - 1
        let usePoints = Array.zeroCreate points.Length            
        let stack = Stack<int*int>()
        stack.Push(anchorIndex, floaterIndex)

        while stack.Count > 0 do            
            let anchorIndex,floaterIndex = stack.Pop()
            let anchor = points.[anchorIndex]
            let floater = points.[floaterIndex]
            let anchorVectorX = dxF floater anchor
            let anchorVectorY = dyF floater anchor
            let segmentLength = sqrt (System.Math.Pow(anchorVectorX,2.0) + System.Math.Pow(anchorVectorY,2.0))
            let anchorUnitVectorX = anchorVectorX / segmentLength
            let anchorUnitVectorY = anchorVectorY / segmentLength
            let mutable maxDistance = 0.0
            let mutable maxDistanceIndex = anchorIndex + 1
            for vertexIndex in anchorIndex + 1 .. floaterIndex - 1 do
                // Compare to anchor
                let vertex = points.[vertexIndex]
                let vertexVectorX = dxF vertex floater
                let vertexVectorY = dyF vertex floater
                let vertexVectorLength = sqrt (System.Math.Pow(vertexVectorX,2.0) + System.Math.Pow(vertexVectorY,2.0))
                let projScalar = vertexVectorX * -anchorUnitVectorX + vertexVectorY * -anchorUnitVectorY
                let vertexDistanceToSegment =
                    if projScalar < 0.0
                    then vertexVectorLength
                    else sqrt (abs (System.Math.Pow(vertexVectorLength,2.0) - System.Math.Pow(projScalar,2.0)))
                if maxDistance < vertexDistanceToSegment then
                    maxDistance <- vertexDistanceToSegment
                    maxDistanceIndex <- vertexIndex
            if Math.TolerantComparisonFunctions.le 0.001 maxDistance epsilon then
                // Use the segment
                usePoints.[anchorIndex] <- true
                usePoints.[floaterIndex] <- true
            else
                // Split the segment
                stack.Push(anchorIndex,maxDistanceIndex)
                stack.Push(maxDistanceIndex,floaterIndex)

        Array.fold2 (fun state usePoint point -> if usePoint then point::state else state) [] usePoints points
        |> List.rev
        |> List.toArray


    module Time =

        open System

        type T<'v> = T<DateTimeOffset,'v>
    
        let time (p:T<'v>) = p.X
        let value (p:T<'v>) = p.Y

        /// Reduce the points using the Ramer-Douglas-Peucker algorithm.
        /// http://www.mappinghacks.com/code/PolyLineReduction/
        let reducePoints epsilon (points: T<float<_>> array) =
            ramerDouglasPeucker (fun (p1: T<float<_>>) p2 -> (dx p1 p2).TotalSeconds) (fun p1 p2 -> dy p1 p2 |> float) epsilon points

