namespace ViewModel

open System
open System.Collections.ObjectModel
open LiveCharts;
open LiveCharts.Wpf;

//Strating point of the viewmodel that drives the UI
//It aggregates all relevant parts of the UI, and exposes them via properties
type MainViewModel() = 
    inherit ViewModelBase()

    let trades                  = ObservableCollection<PaymentViewModel>()
    let data                    = ObservableCollection<ConfigurationViewModel>()
    let calculationParameters   = ObservableCollection<ConfigurationViewModel>()
    let options                 = ObservableCollection<OptionViewModel>()

    let getDataConfiguration () = data |> Seq.map (fun conf -> (conf.Key , conf.Value)) |> Map.ofSeq
    let getCalculationConfiguration () = calculationParameters |> Seq.map (fun conf -> (conf.Key , conf.Value)) |> Map.ofSeq
    
    (* add some dummy data rows *)
    do
        data.Add(ConfigurationViewModel { Key = "FX::USDPLN"; Value = "3.76" })
        data.Add(ConfigurationViewModel { Key = "FX::USDEUR"; Value = "0.87" })
        data.Add(ConfigurationViewModel { Key = "FX::EURGBP"; Value = "0.90" })
        data.Add(ConfigurationViewModel { Key = "interestRate::percentage"; Value = "5" })
        data.Add(ConfigurationViewModel { Key = "stock::price"; Value = "4.20" })
        //data.Add(ConfigurationViewModel { Key = "stock::drift"; Value = "4.20" }) //thats interestrate
        data.Add(ConfigurationViewModel { Key = "stock::volatility"; Value = "0.20" })

        calculationParameters.Add(ConfigurationViewModel { Key = "monteCarlo::runs"; Value = "100" })
        calculationParameters.Add(ConfigurationViewModel { Key = "valuation::baseCurrency"; Value = "USD" })
        calculationParameters.Add(ConfigurationViewModel { Key = "valuation::knownCurrencies"; Value = "USD PLN EUR GBP" })
        calculationParameters.Add(ConfigurationViewModel { Key = "methodology::bumpRisk"; Value = "True" })
        calculationParameters.Add(ConfigurationViewModel { Key = "methodology::bumpSize"; Value = "0.0001" })
        calculationParameters.Add(ConfigurationViewModel { Key = "valuation::deferredHaircut"; Value = "1.5" })
        calculationParameters.Add(ConfigurationViewModel { Key = "option::steps"; Value = "200" })
        calculationParameters.Add(ConfigurationViewModel { Key = "option::seed"; Value = "5" })

    let summary = ObservableCollection<SummaryRow>()

    (* trade commands *)
    let refreshSummary() = 
        summary.Clear()
        trades 
        |> Seq.choose(fun t -> t.Value) // find correctly evaluated trades
        |> Seq.groupBy(fun m -> m.Currency)  // group by currency
        |> Seq.map(fun (ccy, v) -> { Currency = ccy; Value = v |> Seq.map (fun m -> m.Value) |> Seq.sum }) // extract values, calculate a sum
        |> Seq.iter(summary.Add) // add to summary page

    let calculateFun _ = do
            trades |> Seq.iter(fun trade -> trade.Calculate(getDataConfiguration (), getCalculationConfiguration ()))
            refreshSummary()

    let calculate = SimpleCommand calculateFun
    let addTrade = SimpleCommand(fun _ -> 
            let currentConfig = getCalculationConfiguration ()
            PaymentRecord.Random currentConfig |> PaymentViewModel |> trades.Add
            )
    let removeTrade = SimpleCommand(fun trade -> trades.Remove (trade :?> PaymentViewModel) |> ignore)
    let clearTrades = SimpleCommand(fun _ -> trades.Clear () )
    
    (* option commands *)
    //let refreshSummary() = 
    //    summary.Clear()
    //    trades 
    //    |> Seq.choose(fun t -> t.Value) // find correctly evaluated trades
    //    |> Seq.groupBy(fun m -> m.Currency)  // group by currency
    //    |> Seq.map(fun (ccy, v) -> { Currency = ccy; Value = v |> Seq.map (fun m -> m.Value) |> Seq.sum }) // extract values, calculate a sum
    //    |> Seq.iter(summary.Add) // add to summary page
    let calculateOptionsFun _ = do
            options |> Seq.iter(fun option -> option.Calculate(getDataConfiguration (), getCalculationConfiguration ()))
            //refreshSummary()

    let calculateOptions = SimpleCommand calculateOptionsFun
    let addOption = SimpleCommand(fun _ -> 
            let currentConfig = getCalculationConfiguration ()
            OptionRecord.Random currentConfig |> OptionViewModel |> options.Add
            )
    let removeOption = SimpleCommand(fun option -> options.Remove (option :?> OptionViewModel) |> ignore)
    let clearOptions = SimpleCommand(fun _ -> options.Clear () )

    (* charting *)
    let chartSeries = SeriesCollection()
    let predefinedChartFunctions = [| (fun x -> sin x); (fun x -> x); (fun x -> 2.*x); (fun x -> 2.*x - 3.) |] 
    let addChartSeriesFun _ = do
                let ls = LineSeries()
                let multiplier = System.Random().NextDouble()
                let mapFun = predefinedChartFunctions.[ System.Random().Next(predefinedChartFunctions.Length) ]
                ls.Title <- sprintf "Test series %0.2f" multiplier
                //let series = seq { for i in 1 .. 100 do yield (0.01 * multiplier * double i) }
                let series = seq { for i in 1 .. 250 do float i }
                let a = (Seq.map mapFun series)
                ls.Values <- ChartValues<float> a
                chartSeries.Add(ls)
    let addChartSeries = SimpleCommand addChartSeriesFun

    //let clearSeries _ = chartSeries.Clear()
    let clearChartSeries = SimpleCommand (fun _ -> chartSeries.Clear ())

    let addGBMSeriesFun _ = do
        let ls = LineSeries()
        
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
            let normalRV = normalizeRec (genRandomNumbersNominalInterval steps seed) steps
            //build stock prices list
            let rec buildStockPricesList (currentStockPricesList:float list) (steps:int) (normalId:int) : float list =
                if normalId = steps-1 then currentStockPricesList
                else
                    let firstExpTerm =  (drift - (vol**2.)/2.) * (float(years)/float(steps))
                    let secondExpTerm =  vol * sqrt(float(years)/float(steps)) * normalRV.[normalId]
                    let newStockPrice = currentStockPricesList.[normalId] * Math.E ** (firstExpTerm + secondExpTerm)
                    buildStockPricesList (currentStockPricesList@[newStockPrice]) steps (normalId+1)
            let stockPricesList = buildStockPricesList [price] steps 0
            stockPricesList

        //let count = 1000
        let steps = 250 //must be EVEN!
        let price = System.Random().NextDouble() * 10.
        let drift = System.Random().NextDouble()
        let vol = System.Random().NextDouble()
        let years = System.Random().NextDouble()
        let seed = System.Random().Next()
        let series = simulateGBM 1 steps price drift vol years seed
        ls.Values <- ChartValues<float> series
        chartSeries.Add(ls)
        




    let addGBMSeries = SimpleCommand addGBMSeriesFun

    (* add a few series for a good measure *)
    do
        //addChartSeriesFun ()
        //addChartSeriesFun ()
        ()

    (* market data commands *)
    let addMarketDataRecord = SimpleCommand (fun _ -> data.Add(ConfigurationViewModel { Key = ""; Value = "" }))
    let removeMarketDataRecord = SimpleCommand (fun record -> data.Remove(record :?> ConfigurationViewModel) |> ignore)
    let clearMarketDataRecord = SimpleCommand (fun _ -> data.Clear ())

    (* calculation parameters commands *)
    let addCalcParameterRecord = SimpleCommand (fun _ -> calculationParameters.Add(ConfigurationViewModel { Key = ""; Value = "" }))
    let removeCalcParameterRecord = SimpleCommand (fun record -> calculationParameters.Remove(record :?> ConfigurationViewModel) |> ignore)
    let clearCalcParameterRecord = SimpleCommand (fun _ -> calculationParameters.Clear ())

    (* automatically update summary when dependency data changes (entries added/removed)  *)
    do
        trades.CollectionChanged.Add calculateFun
        data.CollectionChanged.Add calculateFun
        calculationParameters.CollectionChanged.Add calculateFun

    (* commands *)
    member this.AddTrade = addTrade 
    member this.RemoveTrade = removeTrade
    member this.ClearTrades = clearTrades
    member this.Calculate = calculate

    member this.AddMarketData = addMarketDataRecord
    member this.RemoveMarketData = removeMarketDataRecord
    member this.ClearMarketData = clearMarketDataRecord
    
    member this.AddCalcParameter = addCalcParameterRecord 
    member this.RemoveCalcParameter = removeCalcParameterRecord 
    member this.ClearCalcParameter = clearCalcParameterRecord 

    member this.AddOption = addOption
    member this.RemoveOption = removeOption
    member this.ClearOptions = clearOptions
    member this.CalculateOptions = calculateOptions

    (* data fields *)
    member this.Trades = trades
    member this.Data = data
    member this.CalculationParameters = calculationParameters
    member this.Summary = summary
    member this.Options = options

    (* charting *)
    member this.ChartSeries = chartSeries
    member this.AddChartSeries = addChartSeries
    member this.AddGBMSeries = addGBMSeries
    member this.ClearChartSeries = clearChartSeries