﻿namespace FSharp.Enterprise

open System
open System.Runtime.CompilerServices
open System.Linq

module DateTime = 
    
    type TimeBoundary =
        | Minute
        | Halfhour
        | Day
        | Month

    type ClockChangeType =
          | NoClockChange
          | Short
          | Long


    let zero = TimeSpan.FromMinutes(0.0)
    let thirtyMinutes = TimeSpan.FromMinutes(30.0)
    

    type System.DateTime with
        
        [<Extension>]
        member x.ToUnixTicks() = 
            let epoch = new System.DateTime(1970, 1, 1, 0, 0, 0)
            let current = x.ToUniversalTime()
            let result = current.Subtract(epoch)
            result.TotalMilliseconds |> int64;
    
        [<Extension>]
        member x.ToMonthStartDate() =
            new DateTime(x.Year, x.Month, 1, 0, 0, 0)
        
        [<Extension>]
        member x.ToMonthEndDate() =
            new DateTime(x.Year, x.Month, DateTime.DaysInMonth(x.Year, x.Month), 23, 59, 59) 
        
        [<Extension>]
        member x.LastDayOfWeekInMonth(day) =
             let monthEnd = x.ToMonthEndDate()
             let wantedDay = int day
             let lastDay = int monthEnd.DayOfWeek
             let offset = 
                 let diff = wantedDay - lastDay
                 if diff > 0 then diff - 7 else diff            
             monthEnd.AddDays(float offset)
        
        [<Extension>]
        member x.LastDayOfWeekInMonth(day) =
            let monthEnd = x.ToMonthEndDate()
            let wantedDay = int day
            let lastDay = int monthEnd.DayOfWeek
            let offset = 
                if lastDay > wantedDay 
                then wantedDay - lastDay 
                else wantedDay - lastDay - 7
            monthEnd.AddDays(float offset)
    
        /// Returns the number of half-hour periods in the day taking into account
        /// long and short days caused by autumn and spring clock changes.
        [<Extension>]
        member x.HalfHoursInDay =
            if x.IsShortDay then 46
            elif x.IsLongDay then 50
            else 48
    
        [<Extension>]
        member x.HoursInDay = 
            x.HalfHoursInDay / 2
        
        [<Extension>]
        member x.IsShortDay
            with get() = x.Date = DateTime.ShortDay(x.Year).Date
        
        [<Extension>]
        member x.IsLongDay
            with get() = x.Date = DateTime.LongDay(x.Year).Date
        
        [<Extension>]
        member x.ClockChange
            with get() =
                if x.IsShortDay then Short
                elif x.IsLongDay then Long
                else NoClockChange
         
        [<Extension>]                           
        member x.ToHalfHourStart() =
            if x.Minute >= 30 then
                DateTime(x.Year, x.Month, x.Day, x.Hour, 30, 0, x.Kind)                
            else
                DateTime(x.Year, x.Month, x.Day, x.Hour, 0, 0, x.Kind)
    
        [<Extension>]
        member x.ToHalfHourEnd() =
            if x.Minute >= 30 then
                DateTime(x.Year, x.Month, x.Day, x.Hour, 0, 0, x.Kind).AddHours(1.)                
            else
                DateTime(x.Year, x.Month, x.Day, x.Hour, 30, 0, x.Kind)
        
        [<Extension>]
        member x.ToDayStart() =
            DateTime(x.Year, x.Month, x.Day, 0, 0, 0, x.Kind)
        
        [<Extension>]
        member x.ToDayEnd() =
            DateTime(x.Year, x.Month, x.Day, 23, 59, 59, 999)
    
        /// Returns the time of the next gate closure given the gate closure
        /// duration (in minutes). 
        [<Extension>]           
        member x.ToNextGateClosure(gateClosureDuration) =
            x.ToHalfHourEnd().AddMinutes(float gateClosureDuration)
    
        [<Extension>]             
        static member ShortDay(year) : DateTime =
            let march = DateTime(year, 3, 1)
            march.LastDayOfWeekInMonth(DayOfWeek.Sunday)
    
        [<Extension>]
        static member LongDay(year) : DateTime =
            let october = DateTime(year, 10, 1)
            october.LastDayOfWeekInMonth(DayOfWeek.Sunday)
        
        [<Extension>]
        static member YearStart(year) =
            DateTime(year, 1, 1)
    
        [<Extension>]
        static member YearEnd(year) =
            DateTime(year, 12, 31)
    
        [<Extension>]
        static member TotalDaysInYear(year) =
            (DateTime.YearEnd(year) - DateTime.YearStart(year)).TotalDays
    
        [<Extension>]
        static member DatesInYear(year) =
            let daysInYear = DateTime.TotalDaysInYear(year)
            let start = DateTime.YearStart(year)
            seq { for i in 0. .. daysInYear - 1. do 
                    yield start.AddDays(i) }

        [<Extension>]
        member x.Ceil(timeBoundary:TimeBoundary) =
            match timeBoundary with
            | Minute ->
                let d = DateTime(x.Year, x.Month, x.Day, x.Hour, x.Minute, 0)
                let delta = x - d
                if delta = zero then
                    d
                else
                    d.AddMinutes(1.0)            
            | Halfhour ->
                let d = DateTime(x.Year, x.Month, x.Day, x.Hour, 0, 0)
                let delta = x - d
                if delta = zero then
                    d
                elif delta <= thirtyMinutes then
                    d.AddMinutes(30.0)
                else
                    d.AddMinutes(60.0)
            | Day ->
                let d = DateTime(x.Year, x.Month, x.Day, 0, 0, 0)
                let delta = x - d
                if delta = zero then
                    d
                else
                    d.AddDays(1.0) 
            | Month ->
                let d = DateTime(x.Year, x.Month, 1, 0, 0, 0)
                let delta = x - d
                if delta = zero 
                then d
                else d.AddMonths(1)

        [<Extension>]
        member x.Floor(timeBoundary:TimeBoundary) =
            match timeBoundary with
            | Minute ->
                DateTime(x.Year, x.Month, x.Day, x.Hour, x.Minute, 0)                        
            | Halfhour ->
                let d = DateTime(x.Year, x.Month, x.Day, x.Hour, 0, 0)
                let delta = x - d
                if delta < thirtyMinutes then
                    d
                else
                    d.AddMinutes(30.0)
            | Day ->
                DateTime(x.Year, x.Month, x.Day, 0, 0, 0)
            | Month ->
                DateTime(x.Year, x.Month, 1, 0, 0, 0)

        [<Extension>]
        member x.Round(timeBoundary:TimeBoundary) =
            match timeBoundary with
            | Minute ->
                let d = DateTime(x.Year, x.Month, x.Day, x.Hour, x.Minute, 0)
                let delta = x - d
                if delta <= TimeSpan.FromSeconds(30.0)
                then d
                else d.AddMinutes(1.0) 
            | Halfhour ->
                if (x.Minute % 30) <= 15
                then x.Floor(timeBoundary)
                else x.Ceil(timeBoundary) 
            | Day ->
                let d = DateTime(x.Year, x.Month, x.Day, 0, 0, 0)
                let delta = x - d
                if delta <= TimeSpan.FromHours(12.0)
                then d
                else d.AddDays(1.0) 
            | Month ->
                let thisMonth = DateTime(x.Year, x.Month, 1, 0, 0, 0)
                let nextMonth = thisMonth.AddMonths(1)
                let split = TimeSpan((nextMonth - thisMonth).Ticks / 2L)
                let delta = x - thisMonth
                if delta <= split
                then thisMonth
                else nextMonth
                
                
    let roundMinute (d:DateTime) = d.Round(TimeBoundary.Minute)
    let ceilMinute (d:DateTime) = d.Ceil(TimeBoundary.Minute)
    let floorMinute (d:DateTime) = d.Floor(TimeBoundary.Minute)
    
    let roundHalfhour (d:DateTime) = d.Round(TimeBoundary.Halfhour)
    let ceilHalfhour (d:DateTime) = d.Ceil(TimeBoundary.Halfhour)
    let floorHalfhour (d:DateTime) = d.Floor(TimeBoundary.Halfhour)

    let roundDay (d:DateTime) = d.Round(TimeBoundary.Day)
    let ceilDay (d:DateTime) = d.Ceil(TimeBoundary.Day)
    let floorDay (d:DateTime) = d.Floor(TimeBoundary.Day)

    let roundMonth (d:DateTime) = d.Round(TimeBoundary.Month)
    let ceilMonth (d:DateTime) = d.Ceil(TimeBoundary.Month)
    let floorMonth (d:DateTime) = d.Floor(TimeBoundary.Month)


module DateTimeOffset = 
        
    open DateTime

    type System.DateTimeOffset with
            
        [<Extension>]
        member x.ToMonthStartDate() =
            new DateTimeOffset(x.Year, x.Month, 1, 0, 0, 0, x.Offset)
        
        [<Extension>]
        member x.ToMonthEndDate() =
            new DateTimeOffset(x.Year, x.Month, DateTime.DaysInMonth(x.Year, x.Month), 23, 59, 59, 999, x.Offset) 
        
        [<Extension>]
        member x.LastDayOfWeekInMonth(day) =
             let monthEnd = x.ToMonthEndDate()
             let wantedDay = int day
             let lastDay = int monthEnd.DayOfWeek
             let offset = 
                 let diff = wantedDay - lastDay
                 if diff > 0 then diff - 7 else diff            
             monthEnd.AddDays(float offset)
    
        /// Returns the number of half-hour periods in the day taking into account
        /// long and short days caused by autumn and spring clock changes.
        [<Extension>]
        member x.HalfHoursInDay =
            if x.IsShortDay then 46
            elif x.IsLongDay then 50
            else 48
    
        [<Extension>]
        member x.HoursInDay = 
            x.HalfHoursInDay / 2
    
        [<Extension>]             
        static member ShortDay(year) =
            let march = DateTimeOffset(DateTime(year, 3, 1))
            march.LastDayOfWeekInMonth(DayOfWeek.Sunday)
    
        [<Extension>]
        static member LongDay(year) =
            let october = DateTimeOffset(DateTime(year, 10, 1))
            october.LastDayOfWeekInMonth(DayOfWeek.Sunday)
    
        [<Extension>]
        member x.IsShortDay
            with get() = x.Date = (DateTimeOffset.ShortDay(x.Year)).Date
        
        [<Extension>]
        member x.IsLongDay
            with get() = x.Date = (DateTimeOffset.LongDay(x.Year)).Date
        
        [<Extension>]
        member x.ClockChange
            with get() =
                if x.IsShortDay then Short
                elif x.IsLongDay then Long
                else NoClockChange
         
        [<Extension>]                           
        member x.ToHalfHourStart() =
            if x.Minute >= 30 then
                DateTimeOffset(x.Year, x.Month, x.Day, x.Hour, 30, 0, x.Offset)                
            else
                DateTimeOffset(x.Year, x.Month, x.Day, x.Hour, 0, 0, x.Offset)
    
        [<Extension>]
        member x.ToHalfHourEnd() =
            if x.Minute >= 30 then
                DateTimeOffset(x.Year, x.Month, x.Day, x.Hour, 0, 0, x.Offset).AddHours(1.)                
            else
                DateTimeOffset(x.Year, x.Month, x.Day, x.Hour, 30, 0, x.Offset)
        
        [<Extension>]
        member x.ToDayStart() =
            DateTimeOffset(x.Year, x.Month, x.Day, 0, 0, 0, x.Offset)
        
        [<Extension>]
        member x.ToDayEnd() =
            DateTimeOffset(x.Year, x.Month, x.Day, 23, 59, 59, x.Offset)
    
        /// Returns the time of the next gate closure given the gate closure
        /// duration (in minutes). 
        [<Extension>]           
        member x.ToNextGateClosure(gateClosureDuration) =
            x.ToHalfHourEnd().AddMinutes(float gateClosureDuration)
    
        [<Extension>]
        static member YearStart(year) =
            DateTimeOffset(DateTime(year, 1, 1))
    
        [<Extension>]
        static member YearEnd(year) =
            DateTimeOffset(DateTime(year, 12, 31))
    
        [<Extension>]
        static member TotalDaysInYear(year) =
            (DateTimeOffset.YearEnd(year) - DateTimeOffset.YearStart(year)).TotalDays
    
        [<Extension>]
        static member DatesInYear(year) =
            let daysInYear = DateTimeOffset.TotalDaysInYear(year)
            let start = DateTimeOffset.YearStart(year)
            seq { for i in 0. .. daysInYear - 1. do 
                    yield start.AddDays(i) }
        
        [<Extension>]
        member x.Ceil(timeBoundary:TimeBoundary) =
            match timeBoundary with
            | Minute ->
                let d = DateTimeOffset(x.Year, x.Month, x.Day, x.Hour, x.Minute, 0, x.Offset)
                let delta = x - d
                if delta = zero then
                    d
                else
                    d.AddMinutes(1.0)            
            | Halfhour ->
                let d = DateTimeOffset(x.Year, x.Month, x.Day, x.Hour, 0, 0, x.Offset)
                let delta = x - d
                if delta = zero then
                    d
                elif delta <= thirtyMinutes then
                    d.AddMinutes(30.0)
                else
                    d.AddMinutes(60.0)
            | Day ->
                let d = DateTimeOffset(x.Year, x.Month, x.Day, 0, 0, 0, x.Offset)
                let delta = x - d
                if delta = zero then
                    d
                else
                    d.AddDays(1.0) 
            | Month ->
                let d = DateTimeOffset(x.Year, x.Month, 1, 0, 0, 0, x.Offset)
                let delta = x - d
                if delta = zero 
                then d
                else d.AddMonths(1)
    
        [<Extension>]
        member x.Floor(timeBoundary:TimeBoundary) =
            match timeBoundary with
            | Minute ->
                DateTimeOffset(x.Year, x.Month, x.Day, x.Hour, x.Minute, 0, x.Offset)                        
            | Halfhour ->
                let d = DateTimeOffset(x.Year, x.Month, x.Day, x.Hour, 0, 0, x.Offset)
                let delta = x - d
                if delta < thirtyMinutes then
                    d
                else
                    d.AddMinutes(30.0)
            | Day ->
                DateTimeOffset(x.Year, x.Month, x.Day, 0, 0, 0, x.Offset)
            | Month ->
                DateTimeOffset(x.Year, x.Month, 1, 0, 0, 0, x.Offset)
    
        [<Extension>]
        member x.Round(timeBoundary:TimeBoundary) =
            match timeBoundary with
            | Minute ->
                let d = DateTimeOffset(x.Year, x.Month, x.Day, x.Hour, x.Minute, 0, x.Offset)
                let delta = x - d
                if delta <= TimeSpan.FromSeconds(30.0)
                then d
                else d.AddMinutes(1.0) 
            | Halfhour ->
                if (x.Minute % 30) <= 15
                then x.Floor(timeBoundary)
                else x.Ceil(timeBoundary) 
            | Day ->
                let d = DateTimeOffset(x.Year, x.Month, x.Day, 0, 0, 0, x.Offset)
                let delta = x - d
                if delta <= TimeSpan.FromHours(12.0)
                then d
                else d.AddDays(1.0) 
            | Month ->
                let thisMonth = DateTimeOffset(x.Year, x.Month, 1, 0, 0, 0, x.Offset)
                let nextMonth = thisMonth.AddMonths(1)
                let split = TimeSpan((nextMonth - thisMonth).Ticks / 2L)
                let delta = x - thisMonth
                if delta <= split
                then thisMonth
                else nextMonth

        [<Extension>]
        member x.Next(timeBoundary: TimeBoundary) =
            match timeBoundary with
            | Minute -> x.Floor(timeBoundary).AddMinutes(1.0)
            | Halfhour -> x.Floor(timeBoundary).AddMinutes(30.0)
            | Day -> x.Floor(timeBoundary).AddDays(1.0)
            | Month -> x.Floor(timeBoundary).AddMonths(1)

        [<Extension>]
        member x.Previous(timeBoundary: TimeBoundary) =
            match timeBoundary with
            | Minute -> x.Ceil(timeBoundary).AddMinutes(-1.0)
            | Halfhour -> x.Ceil(timeBoundary).AddMinutes(-30.0)
            | Day -> x.Ceil(timeBoundary).AddDays(-1.0)
            | Month -> x.Ceil(timeBoundary).AddMonths(-1)

        [<Extension>]
        member x.Bounding(defaultToLowerBound: bool, timeBoundary: TimeBoundary) =
            let startTime = x.Floor(timeBoundary)
            let endTime = x.Ceil(timeBoundary)
            if startTime = endTime then
                if defaultToLowerBound 
                then startTime,startTime.Next(timeBoundary)
                else endTime.Previous(timeBoundary),endTime
            else
                startTime,endTime
    
    let roundMinute (d:DateTimeOffset) = d.Round(TimeBoundary.Minute)
    let ceilMinute (d:DateTimeOffset) = d.Ceil(TimeBoundary.Minute)
    let floorMinute (d:DateTimeOffset) = d.Floor(TimeBoundary.Minute)
    let boundingMinute defaultToLowerBound (d:DateTimeOffset) = d.Bounding(defaultToLowerBound, TimeBoundary.Minute)
    let nextMinute (d:DateTimeOffset) = d.Next(TimeBoundary.Minute)
    let previousMinute (d:DateTimeOffset) = d.Previous(TimeBoundary.Minute)

    let roundHalfhour (d:DateTimeOffset) = d.Round(TimeBoundary.Halfhour)
    let ceilHalfhour (d:DateTimeOffset) = d.Ceil(TimeBoundary.Halfhour)
    let floorHalfhour (d:DateTimeOffset) = d.Floor(TimeBoundary.Halfhour)
    let boundingHalfhour defaultToLowerBound (d:DateTimeOffset) = d.Bounding(defaultToLowerBound, TimeBoundary.Halfhour)
    let nextHalfhour (d:DateTimeOffset) = d.Next(TimeBoundary.Halfhour)
    let previousHalfhour (d:DateTimeOffset) = d.Previous(TimeBoundary.Halfhour)

    let roundDay (d:DateTimeOffset) = d.Round(TimeBoundary.Day)
    let ceilDay (d:DateTimeOffset) = d.Ceil(TimeBoundary.Day)
    let floorDay (d:DateTimeOffset) = d.Floor(TimeBoundary.Day)
    let boundingDay defaultToLowerBound (d:DateTimeOffset) = d.Bounding(defaultToLowerBound, TimeBoundary.Day)
    let nextDay (d:DateTimeOffset) = d.Next(TimeBoundary.Day)
    let previousDay (d:DateTimeOffset) = d.Previous(TimeBoundary.Day)

    let roundMonth (d:DateTimeOffset) = d.Round(TimeBoundary.Month)
    let ceilMonth (d:DateTimeOffset) = d.Ceil(TimeBoundary.Month)
    let floorMonth (d:DateTimeOffset) = d.Floor(TimeBoundary.Month)
    let boundingMonth defaultToLowerBound (d:DateTimeOffset) = d.Bounding(defaultToLowerBound, TimeBoundary.Month)
    let nextMonth (d:DateTimeOffset) = d.Next(TimeBoundary.Month)
    let previousMonth (d:DateTimeOffset) = d.Previous(TimeBoundary.Month)

    /// Returns the halfhours surrounding the given time. 
    /// When the give time is on the halfhour then if defaultToLowerBound
    /// is true then the given time is used as the lower bound otherwise
    /// it is used as the upper bound.
    let boundingHalfhours defaultToLowerBound (d:DateTimeOffset) =
        let halfhourStart = floorHalfhour d
        let halfhourEnd = ceilHalfhour d
        if halfhourStart = halfhourEnd then
            if defaultToLowerBound then
                halfhourStart,halfhourEnd.AddMinutes(30.0)
            else
                halfhourStart.AddMinutes(-30.0),halfhourEnd
        else
            halfhourStart,halfhourEnd

    let computeShortLongHourIndicies (date : DateTimeOffset) (granularity : TimeSpan) = 
        let toOption (i : int) = if i >= 0 then Some(i) else None
        if granularity.TotalMinutes >= 1440.  //if it at day granularity or bigger we don't have to worry 
        then (None, None)
        else
            (DateTime.ShortDay(date.Year).AddHours(2.).Subtract(date.DateTime).TotalMinutes / granularity.TotalMinutes |> int |> toOption, 
             DateTime.LongDay(date.Year).AddHours(2.).Subtract(date.DateTime).TotalMinutes / granularity.TotalMinutes |> int |> toOption)

    let normaliseForClockChange (startDate : DateTimeOffset) (granularity : TimeSpan) (seq : seq<'a>) = 
        let sindx, lindx = computeShortLongHourIndicies startDate granularity
        let series = new ResizeArray<_>(seq)
        Option.iter (fun longDayIndex ->
                         if  (longDayIndex < series.Count)
                         then series.RemoveRange(longDayIndex, (TimeSpan.FromHours(1.).TotalMinutes / granularity.TotalMinutes) |> int)
                    ) lindx
        Option.iter (fun shortDayIndex -> 
                        if  (shortDayIndex < series.Count)
                        then series.InsertRange(shortDayIndex, Seq.init (TimeSpan.FromHours(1.).TotalMinutes / granularity.TotalMinutes |> int) (fun _ -> new 'a())) 
                    ) sindx
        series :> seq<_>
