namespace ViewModel

open System
open System
//open MathNet.Numerics.LinearAlgebra
//open MathNet.Numerics.Distributions

//open Extreme.Mathematics
//open Extreme.Statistics
//open Extreme.Statistics.Distributions

//open Troschuetz.Random.Distributions.Continuous

(* A type representing given amount of money in specific currency. Very bare bones, could be extended in various ways. Some examples:
1. Multiplication by float so that $1 * 100 = $100.
2. Addition to other Money instance so that $1 + $2 = $3, but 1 zl + $1 = <exception thrown> *)
type Money =    //thats a record (structure in C)
    {
        Value : float;
        Currency : string;
    }

    override this.ToString() = sprintf "%.2f (%s)" this.Value this.Currency

(* Model for Payment trade. *)
type PaymentRecord =    //record as well
    {
        TradeName : string
        Expiry    : DateTime
        Currency  : string
        Principal : int
        CanBeDeferred : bool
    }


    (* Simple utility method for creating a random payment. *)
    static member sysRandom = System.Random()
    static member Random(configuration : CalculationConfiguration) = 
        (* We pick a random currency either from given short list, or from valuation::knownCurrencies config key *)
        let knownCurrenciesDefault = [| "EUR"; "USD"; "PLN"; |]
        
        let knownCurrencies = if configuration.ContainsKey "valuation::knownCurrencies" 
                              then configuration.["valuation::knownCurrencies"].Split([|' '|])
                              else knownCurrenciesDefault


        let rnd  = System.Random()
        let r = rnd.Next()%2 |> System.Convert.ToBoolean

        
        {
            TradeName = sprintf "Payment%04d" (PaymentRecord.sysRandom.Next(9999))
            Expiry    = (DateTime.Now.AddMonths (PaymentRecord.sysRandom.Next(1, 6))).Date
            Currency  = knownCurrencies.[ PaymentRecord.sysRandom.Next(knownCurrencies.Length) ]
            Principal = PaymentRecord.sysRandom.Next()

            CanBeDeferred = r
            //PaymentRecord.sysRandom.Next(1,6)
        }

(* Complete set of data required for valuation *)
type PaymentValuationInputs = 
    {
        Trade : PaymentRecord
        Data : DataConfiguration
        CalculationsParameters: CalculationConfiguration
    }

(* The valuation model for Payment. We may have multiple valuation models implementations per given trade type, or have a valuation model that handles multiple trade types. *)
type PaymentValuationModel (inputs:PaymentValuationInputs) = 
    (* Calculate() method returns a value of given trade. This one is very simple, yet demonstrates some concepts.
    
    It will try to return the result in the global default currency as configured by valuation::baseCurrency key.

    If the valuation::baseCurrency is not defined or we are unable to obtain the FX rate FX::<targetCcy><tradeCcy>, 
    we simply return the value using the trade currency.

    *)
    member this.Calculate() : Money = 

        let tradeCcy = inputs.Trade.Currency

        let targetCcy = match inputs.CalculationsParameters.TryFind "valuation::baseCurrency" with
                         | Some ccy -> ccy
                         | None -> tradeCcy

        let fxRateKey = sprintf "FX::%s%s" targetCcy tradeCcy

        let fxRate = if inputs.Data.ContainsKey fxRateKey then float inputs.Data.[ fxRateKey ] else 1.0 // lookup FX rate
        let finalCcy = if inputs.Data.ContainsKey fxRateKey then targetCcy else tradeCcy
        
        let haircutS = match inputs.CalculationsParameters.TryFind "valuation::deferredHaircut" with
                         | Some haircut -> haircut
                         | None -> "9"

        let haircutN = 
            try float haircutS
            with
            | _ -> 0.1
        


        if inputs.CalculationsParameters.ContainsKey "valuation::deferredHaircut" && inputs.Trade.CanBeDeferred.Equals true
        then
            { Value = (float inputs.Trade.Principal)  / fxRate * haircutN; Currency = finalCcy }
        else
        { Value = (float inputs.Trade.Principal)  / fxRate; Currency = finalCcy }

    member this.CalculatePlusInterest() : Money = 
        let tradeCcy = inputs.Trade.Currency
        let targetCcy = match inputs.CalculationsParameters.TryFind "valuation::baseCurrency" with
        | Some ccy -> ccy
        | None -> tradeCcy
        let fxRateKey = sprintf "FX::%s%s" targetCcy tradeCcy
        let fxRate = if inputs.Data.ContainsKey fxRateKey then float inputs.Data.[ fxRateKey ] else 1.0 // lookup FX rate
        let finalCcy = if inputs.Data.ContainsKey fxRateKey then targetCcy else tradeCcy
        
        let haircutS = match inputs.CalculationsParameters.TryFind "valuation::deferredHaircut" with
                         | Some haircut -> haircut
                         | None -> "9"
        let haircutN = 
            try float haircutS
            with
            | _ -> 0.1

        //interestRate as a string
        let interestRateS = match inputs.Data.TryFind "interestRate::percentage" with
                            | Some rate -> rate
                            | None -> "0"
        //try convert as a number
        let interestRateN =
            try float interestRateS
            with
            | _ -> 0.
            

        if inputs.CalculationsParameters.ContainsKey "valuation::deferredHaircut" && inputs.Trade.CanBeDeferred.Equals true
        then
            let ValuePlusRate = (float inputs.Trade.Principal*(1.+interestRateN/100.))  / fxRate * haircutN
            //let ValuePlusInterest = Value + Value
            { Value = ValuePlusRate ; Currency = finalCcy }
        else

        { Value = (float inputs.Trade.Principal)*(1.+interestRateN/100.)  / fxRate; Currency = finalCcy }



    


(* Model for Option trade. *)
type OptionRecord =    //record as well
    {
        OptionName : string
        Expiry    : DateTime
        Currency  : string
        Strike : float
        //BSput : float
        //BSputDelta : float
        //BScall : float
        //BScallDelta : float
    }


    (* Simple utility method for creating a random option. *)
    static member sysRandom = System.Random()
    static member Random(configuration : CalculationConfiguration) = 
        (* We pick a random currency either from given short list, or from valuation::knownCurrencies config key *)
        let knownCurrenciesDefault = [| "EUR"; "USD"; "PLN"; |]
        
        let knownCurrencies = if configuration.ContainsKey "valuation::knownCurrencies" 
                              then configuration.["valuation::knownCurrencies"].Split([|' '|])
                              else knownCurrenciesDefault


        let rnd  = System.Random()
        //let r = rnd.Next()%2 |> System.Convert.ToBoolean

        
        {
            OptionName  = sprintf "Option%04d" (OptionRecord.sysRandom.Next(9999))
            Expiry      = (DateTime.Now.AddMonths (OptionRecord.sysRandom.Next(2, 12))).Date
            Currency    = knownCurrencies.[ OptionRecord.sysRandom.Next(knownCurrencies.Length) ]
            Strike      = OptionRecord.sysRandom.NextDouble()
            //BSput       = OptionRecord.sysRandom.NextDouble()
            //BSputDelta  = OptionRecord.sysRandom.NextDouble()
            //BScall      = OptionRecord.sysRandom.NextDouble()
            //BScallDelta = OptionRecord.sysRandom.NextDouble()
        }

(* Complete set of data required for option valuation *)
type OptionValuationInputs = 
    {
        OptionType : OptionRecord
        Data : DataConfiguration
        CalculationsParameters: CalculationConfiguration
    }

//Params for Geometric Brownian Motion used for simulating stock prices
type GBMParams = 
    {
        //count:int
        steps:int   //must be even
        price:float
        drift:float
        vol:float
        years:float
        seed:int
    }
//Params for Black-Scholes Model for pricing an option
type BSParams = 
    {
        k: float    //strike
        m: float    //maturity
    }

type ChartInputs =
    {
        Data : DataConfiguration
        CalculationsParameters: CalculationConfiguration
    }

type ChartValuationModel (inputs: ChartInputs) =
    member this.SimulateGBM() =
        //generates list of n Uniform RVs from interval [0,1]; here it's [0,1) I guess
        let genRandomNumbersNominalInterval (count:int) (seed:int) : float list=
            let rnd = System.Random(seed)
            List.init count (fun _ -> rnd.NextDouble())

        //input: UniformRM need to be from interval (0,1]
        //input: steps MUST BE EVEN!
        //output: NormalRV have mean=0 and standard_deviation=1
        let normalizeRec (uniformList:float list) (n:int) : float list =
            let rec buildNormalList (normalList:float list) =
                if normalList.Length = n then normalList
                else
                    let currentNIdOne = normalList.Length
                    let currentNIdTwo = currentNIdOne + 1
                    let oneU = uniformList.[currentNIdOne]
                    let twoU = uniformList.[currentNIdTwo]
                    let oneN = sqrt(-2.*Math.Log(oneU, Math.E))*sin(2.*Math.PI*twoU)
                    let twoN = sqrt(-2.*Math.Log(oneU, Math.E))*cos(2.*Math.PI*twoU)
                    let newUniforms = [oneN; twoN]
                    buildNormalList (normalList@newUniforms)
            buildNormalList []

        let simulateGBM (count:int) (steps:int) (price:float) (drift:float) (vol:float) (years:int) (seed:int) =
            //start counting t(trajectories)
            let rec buildResult currentResult t =
                if t = count+1 then currentResult
                else
                    let normalRV = normalizeRec (genRandomNumbersNominalInterval steps t) steps
            
                    //build stock prices list
                    let rec buildStockPricesList (currentStockPricesList:float list) (steps:int) (normalId:int) : float list =
                        if normalId = steps-1 then currentStockPricesList
                        else
                            let firstExpTerm =  (drift - (vol**2.)/2.) * (float(years)/float(steps))
                            let secondExpTerm =  vol * sqrt(float(years)/float(steps)) * normalRV.[normalId]
                            let newStockPrice = currentStockPricesList.[normalId] * Math.E ** (firstExpTerm + secondExpTerm)
                            buildStockPricesList (currentStockPricesList@[newStockPrice]) steps (normalId+1)
                    let stockPricesList = buildStockPricesList [price] steps 0
                    //printfn "StockPricesList: %A" stockPricesList

                    let finalStockPrice = stockPricesList.[stockPricesList.Length - 1]
                    //calculate historical (realized) volatility
                    let rec buildRList (rList:float list) (index:int) =
                        if index = steps-1 then rList
                        else
                            let currentR =  Math.Log((stockPricesList.[index+1])/(stockPricesList.[index]), Math.E)
                            buildRList (rList@[currentR]) (index+1)

                    let rList = buildRList [] 0
                    let rAvg = List.average rList
                    let sumOfSquares : float = List.fold (fun acc elem -> acc  + (elem - rAvg)**2.) 0. rList
                    let historicalVolatilitySquared = float(steps)/(float(years)*(float(steps)-1.)) * sumOfSquares
                    //prepare final result being tuple: (finalStockPrice, realizedVolatility)
                    let newResult = finalStockPrice
                    buildResult (currentResult@[newResult]) (t+1)
            let result = buildResult [] 1
            result

        let count = 1000
        let steps = 250 //must be EVEN!
        let price = 4.20
        let drift = 0.12
        let vol = 0.2
        let years = 1
        let seed = 5

        simulateGBM count steps price drift vol years seed

type OptionValuationModel (inputs:OptionValuationInputs) = 
    member this.SimulateGBM() =
        //generates list of n Uniform RVs from interval [0,1]; here it's [0,1) I guess
        let genRandomNumbersNominalInterval (count:int) (seed:int) : float list=
            let rnd = System.Random(seed)
            List.init count (fun _ -> rnd.NextDouble())

        //input: UniformRM need to be from interval (0,1]
        //input: steps MUST BE EVEN!
        //output: NormalRV have mean=0 and standard_deviation=1
        let normalizeRec (uniformList:float list) (n:int) : float list =
            let rec buildNormalList (normalList:float list) =
                if normalList.Length = n then normalList
                else
                    let currentNIdOne = normalList.Length
                    let currentNIdTwo = currentNIdOne + 1
                    let oneU = uniformList.[currentNIdOne]
                    let twoU = uniformList.[currentNIdTwo]
                    let oneN = sqrt(-2.*Math.Log(oneU, Math.E))*sin(2.*Math.PI*twoU)
                    let twoN = sqrt(-2.*Math.Log(oneU, Math.E))*cos(2.*Math.PI*twoU)
                    let newUniforms = [oneN; twoN]
                    buildNormalList (normalList@newUniforms)
            buildNormalList []

        let simulateGBM (count:int) (steps:int) (price:float) (drift:float) (vol:float) (years:int) (seed:int) =
            //start counting t(trajectories)
            let rec buildResult currentResult t =
                if t = count+1 then currentResult
                else
                    let normalRV = normalizeRec (genRandomNumbersNominalInterval steps t) steps
            
                    //build stock prices list
                    let rec buildStockPricesList (currentStockPricesList:float list) (steps:int) (normalId:int) : float list =
                        if normalId = steps-1 then currentStockPricesList
                        else
                            let firstExpTerm =  (drift - (vol**2.)/2.) * (float(years)/float(steps))
                            let secondExpTerm =  vol * sqrt(float(years)/float(steps)) * normalRV.[normalId]
                            let newStockPrice = currentStockPricesList.[normalId] * Math.E ** (firstExpTerm + secondExpTerm)
                            buildStockPricesList (currentStockPricesList@[newStockPrice]) steps (normalId+1)
                    let stockPricesList = buildStockPricesList [price] steps 0
                    //printfn "StockPricesList: %A" stockPricesList

                    let finalStockPrice = stockPricesList.[stockPricesList.Length - 1]
                    //calculate historical (realized) volatility
                    let rec buildRList (rList:float list) (index:int) =
                        if index = steps-1 then rList
                        else
                            let currentR =  Math.Log((stockPricesList.[index+1])/(stockPricesList.[index]), Math.E)
                            buildRList (rList@[currentR]) (index+1)

                    let rList = buildRList [] 0
                    let rAvg = List.average rList
                    let sumOfSquares : float = List.fold (fun acc elem -> acc  + (elem - rAvg)**2.) 0. rList
                    let historicalVolatilitySquared = float(steps)/(float(years)*(float(steps)-1.)) * sumOfSquares
                    //prepare final result being tuple: (finalStockPrice, realizedVolatility)
                    let newResult = finalStockPrice
                    buildResult (currentResult@[newResult]) (t+1)
            let result = buildResult [] 1
            result

        let count = 1000
        let steps = 250 //must be EVEN!
        let price = 4.20
        let drift = 0.12
        let vol = 0.2
        let years = 1
        let seed = 5

        simulateGBM count steps price drift vol years seed

    member this.Calculate() (*: (Money list)*) =
        let optionCcy = inputs.OptionType.Currency
        let targetCcy = match inputs.CalculationsParameters.TryFind "valuation::baseCurrency" with
        | Some ccy -> ccy
        | None -> optionCcy
        let fxRateKey = sprintf "FX::%s%s" targetCcy optionCcy
        let fxRate = if inputs.Data.ContainsKey fxRateKey then float inputs.Data.[ fxRateKey ] else 1.0 // lookup FX rate
        let finalCcy = if inputs.Data.ContainsKey fxRateKey then targetCcy else optionCcy


        //generates list of n Uniform RVs from interval [0,1]; here it's [0,1) I guess
        let genRandomNumbersNominalInterval (count:int) (seed:int) : float list=
            let rnd = System.Random(seed)
            List.init count (fun _ -> rnd.NextDouble())

        //input: UniformRM need to be from interval (0,1]
        //input: steps MUST BE EVEN!
        //output: NormalRV have mean=0 and standard_deviation=1
        let normalizeRec (uniformList:float list) (n:int) : float list =
            let rec buildNormalList (normalList:float list) =
                if normalList.Length = n then normalList
                else
                    let currentNIdOne = normalList.Length
                    let currentNIdTwo = currentNIdOne + 1
                    let oneU = uniformList.[currentNIdOne]
                    let twoU = uniformList.[currentNIdTwo]
                    let oneN = sqrt(-2.*Math.Log(oneU, Math.E))*sin(2.*Math.PI*twoU)
                    let twoN = sqrt(-2.*Math.Log(oneU, Math.E))*cos(2.*Math.PI*twoU)
                    let newUniforms = [oneN; twoN]
                    buildNormalList (normalList@newUniforms)
            buildNormalList []

        let simulateGBM (count:int) (steps:int) (price:float) (drift:float) (vol:float) (years:float) (seed:int) =
            //start counting t(trajectories)
            let rec buildResult currentResult t =
                if t = count+1 then currentResult
                else
                    let normalRV = normalizeRec (genRandomNumbersNominalInterval steps t) steps
            
                    //build stock prices list
                    let rec buildStockPricesList (currentStockPricesList:float list) (steps:int) (normalId:int) : float list =
                        if normalId = steps-1 then currentStockPricesList
                        else
                            let firstExpTerm =  (drift - (vol**2.)/2.) * (float(years)/float(steps))
                            let secondExpTerm =  vol * sqrt(float(years)/float(steps)) * normalRV.[normalId]
                            let newStockPrice = currentStockPricesList.[normalId] * Math.E ** (firstExpTerm + secondExpTerm)
                            buildStockPricesList (currentStockPricesList@[newStockPrice]) steps (normalId+1)
                    let stockPricesList = buildStockPricesList [price] steps 0
                    //printfn "StockPricesList: %A" stockPricesList

                    let finalStockPrice = stockPricesList.[stockPricesList.Length - 1]
                    //calculate historical (realized) volatility
                    let rec buildRList (rList:float list) (index:int) =
                        if index = steps-1 then rList
                        else
                            let currentR =  Math.Log((stockPricesList.[index+1])/(stockPricesList.[index]), Math.E)
                            buildRList (rList@[currentR]) (index+1)

                    let rList = buildRList [] 0
                    let rAvg = List.average rList
                    let sumOfSquares : float = List.fold (fun acc elem -> acc  + (elem - rAvg)**2.) 0. rList
                    let historicalVolatilitySquared = float(steps)/(float(years)*(float(steps)-1.)) * sumOfSquares
                    //prepare final result being tuple: (finalStockPrice, realizedVolatility)
                    let newResult = finalStockPrice
                    buildResult (currentResult@[newResult]) (t+1)
            let result = buildResult [] 1
            result

        let simulateGBM_old (count:int) (steps:int) (price:float) (drift:float) (vol:float) (years:int) (seed:int) =
            //start counting t(trajectories)
            let rec buildResult currentResult t =
                if t = count+1 then currentResult
                else
                    let normalRV = normalizeRec (genRandomNumbersNominalInterval steps t) steps
            
                    //build stock prices list
                    let rec buildStockPricesList (currentStockPricesList:float list) (steps:int) (normalId:int) : float list =
                        if normalId = steps-1 then currentStockPricesList
                        else
                            let firstExpTerm =  (drift - (vol**2.)/2.) * (float(years)/float(steps))
                            let secondExpTerm =  vol * sqrt(float(years)/float(steps)) * normalRV.[normalId]
                            let newStockPrice = currentStockPricesList.[normalId] * Math.E ** (firstExpTerm + secondExpTerm)
                            buildStockPricesList (currentStockPricesList@[newStockPrice]) steps (normalId+1)
                    let stockPricesList = buildStockPricesList [price] steps 0
                    //printfn "StockPricesList: %A" stockPricesList

                    let finalStockPrice = stockPricesList.[stockPricesList.Length - 1]
                    //calculate historical (realized) volatility
                    let rec buildRList (rList:float list) (index:int) =
                        if index = steps-1 then rList
                        else
                            let currentR =  Math.Log((stockPricesList.[index+1])/(stockPricesList.[index]), Math.E)
                            buildRList (rList@[currentR]) (index+1)

                    let rList = buildRList [] 0
                    let rAvg = List.average rList
                    let sumOfSquares : float = List.fold (fun acc elem -> acc  + (elem - rAvg)**2.) 0. rList
                    let historicalVolatilitySquared = float(steps)/(float(years)*(float(steps)-1.)) * sumOfSquares
                    //prepare final result being tuple: (finalStockPrice, realizedVolatility)
                    let newResult = [finalStockPrice; historicalVolatilitySquared]
                    buildResult (currentResult@[newResult]) (t+1)
            let result = buildResult [] 1
            result

        //let count = 1000
        //let steps = 250 //must be EVEN!
        //let price = 4.20
        //let drift = 0.12
        //let vol = 0.2
        //let years = 1.
        //let seed = 5

        //simulateGBM count steps price drift vol years seed

        let cfd (mean,stdev,point) = 
            point * 0.87

        //simulate and predict option price
        let simulateBlackScholesPutOptionPriceAndDelta (gbm:GBMParams) (bs:BSParams) =
            let normalRV = normalizeRec (genRandomNumbersNominalInterval gbm.steps gbm.seed) gbm.steps
            //build stock prices list
            let rec buildStockPricesList (currentStockPricesList:float list) (steps:int) (normalId:int) : float list =
                if normalId = steps-1 then currentStockPricesList
                else
                    let firstExpTerm =  (gbm.drift - (gbm.vol**2.)/2.) * (float(gbm.years)/float(steps))
                    let secondExpTerm =  gbm.vol * sqrt(float(gbm.years)/float(steps)) * normalRV.[normalId]
                    let newStockPrice = currentStockPricesList.[normalId] * Math.E ** (firstExpTerm + secondExpTerm)
                    buildStockPricesList (currentStockPricesList@[newStockPrice]) steps (normalId+1)
            let stockPricesList = buildStockPricesList [gbm.price] gbm.steps 0
            let finalStockPrice = stockPricesList.[stockPricesList.Length - 1]

            let d1 = (Math.Log(gbm.price/bs.k, Math.E) + (gbm.drift + 0.5*(gbm.vol**2.))*bs.m) / (gbm.vol*sqrt(bs.m))
            let BScall = 
                let d2 = d1 - gbm.vol*sqrt(bs.m)
                let BScallPrice = gbm.price * cfd(0, 1, d1) - (bs.k/Math.E**(gbm.drift*bs.m) * cfd(0, 1, d1))
                BScallPrice
            //MathNet.Numerics.Distributions.Normal.CFD(mean,stdev,point)   //I would this package
            //stockPricesList

            let BScallDelta =
                cfd(0,1,d1)
            let BSputDelta = BScallDelta - 1.

            let BSput =
                BScall + bs.k/(Math.E**(gbm.drift*bs.m)) - gbm.price

            [BScall; BScallDelta; BSput; BSputDelta]
        
        //interestRate as a string
        let interestRateS = match inputs.Data.TryFind "interestRate::percentage" with
                            | Some rate -> rate
                            | None -> "0"
        //try convert as a number
        let interestRateN =
            try float interestRateS
            with
            | _ -> 0.

        let priceS = match inputs.Data.TryFind "stock::price" with
                            | Some price -> price
                            | None -> "4.21"
        let priceN =
            try float priceS
            with
            | _ -> 4.21

        let volS = match inputs.Data.TryFind "stock::volatility" with
                            | Some volatility -> volatility
                            | None -> "0.21"
        let volN =
            try float volS
            with
            | _ -> 0.21

        let stepsS = match inputs.CalculationsParameters.TryFind "option::steps" with
                            | Some steps -> steps
                            | None -> "200"
        let stepsN =
            try int stepsS
            with
            | _ -> 202

        let seedN =
            let seedS = match inputs.CalculationsParameters.TryFind "option::seed" with
                                | Some seed -> seed
                                | None -> "7"
            try int seedS
            with
            | _ -> 7

        let strike = 
            match inputs.OptionType.Strike with
            | strike -> strike
            | _ -> 10.

        let maturity =
            match inputs.OptionType.Expiry with
            | date ->
                let date = System.DateTime.Now.AddYears(1)
                let yearInTicks = System.DateTime.Now.AddYears(1).Ticks - System.DateTime.Now.Ticks
                let oneTickToYears = (double (1.))/(double yearInTicks)
                float (double (date.Ticks - System.DateTime.Now.Ticks) * oneTickToYears) 
            | _ -> 1.

        let g = {
            years=maturity
            steps=stepsN
            price=priceN
            drift=interestRateN/100. //from 5"%" do .05
            vol=volN
            seed=seedN}
        let b = {
            k=strike
            m=maturity}

        let r = simulateBlackScholesPutOptionPriceAndDelta g b

        let money1 = {Value = r.[0] / fxRate; Currency=finalCcy}
        let money2 = r.[1](*{Value = r.[1] / fxRate; Currency=finalCcy}*)
        let money3 = {Value = r.[2] / fxRate; Currency=finalCcy}
        let money4 = r.[2](*{Value = r.[3] / fxRate; Currency=finalCcy}*)


        let gbmResult = simulateGBM 1 stepsN priceN interestRateN volN maturity seedN


        (money1,money2,money3,money4, gbmResult)

        

