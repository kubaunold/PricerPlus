namespace ViewModel

open System
open System

(* A type representing given amount of money in specific currency. Very bare bones, could be extended in various ways. Some examples:
1. Multiplication by float so that $1 * 100 = $100.
2. Addition to other Money instance so that $1 + $2 = $3, but 1 zl + $1 = <exception thrown> *)
//type Money =    //thats a record (structure in C)
//    {
//        Value : float;
//        Currency : string;
//    }

//    override this.ToString() = sprintf "%.2f (%s)" this.Value this.Currency

(* Model for Option trade. *)
type OptionRecord =    //record as well
    {
        OptionName : string
        Expiry    : DateTime
        Currency  : string
        Strike : float
        BSput : float
        BSputDelta : float
        BScall : float
        BScallDelta : float
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
            BSput       = OptionRecord.sysRandom.NextDouble()
            BSputDelta  = OptionRecord.sysRandom.NextDouble()
            BScall      = OptionRecord.sysRandom.NextDouble()
            BScallDelta = OptionRecord.sysRandom.NextDouble()
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