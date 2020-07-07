namespace ViewModel

//open System
//open System

//(* A type representing given amount of money in specific currency. Very bare bones, could be extended in various ways. Some examples:
//1. Multiplication by float so that $1 * 100 = $100.
//2. Addition to other Money instance so that $1 + $2 = $3, but 1 zl + $1 = <exception thrown> *)
////type Money =    //thats a record (structure in C)
////    {
////        Value : float;
////        Currency : string;
////    }
////    override this.ToString() = sprintf "%.2f (%s)" this.Value this.Currency

//(* Model for Option trade. *)
//type OptionRecord =    //record as well
//    {
//        OptionName : string
//        Expiry    : DateTime
//        Currency  : string
//        Strike : float
//        BSput : float
//        BSputDelta : float
//        BScall : float
//        BScallDelta : float
//    }


//    (* Simple utility method for creating a random option. *)
//    static member sysRandom = System.Random()
//    static member Random(configuration : CalculationConfiguration) = 
//        (* We pick a random currency either from given short list, or from valuation::knownCurrencies config key *)
//        let knownCurrenciesDefault = [| "EUR"; "USD"; "PLN"; |]
        
//        let knownCurrencies = if configuration.ContainsKey "valuation::knownCurrencies" 
//                              then configuration.["valuation::knownCurrencies"].Split([|' '|])
//                              else knownCurrenciesDefault


//        let rnd  = System.Random()
//        //let r = rnd.Next()%2 |> System.Convert.ToBoolean

        
//        {
//            OptionName  = sprintf "Option%04d" (OptionRecord.sysRandom.Next(9999))
//            Expiry      = (DateTime.Now.AddMonths (OptionRecord.sysRandom.Next(2, 12))).Date
//            Currency    = knownCurrencies.[ OptionRecord.sysRandom.Next(knownCurrencies.Length) ]
//            Strike      = OptionRecord.sysRandom.NextDouble()
//            BSput       = OptionRecord.sysRandom.NextDouble()
//            BSputDelta  = OptionRecord.sysRandom.NextDouble()
//            BScall      = OptionRecord.sysRandom.NextDouble()
//            BScallDelta = OptionRecord.sysRandom.NextDouble()
//        }