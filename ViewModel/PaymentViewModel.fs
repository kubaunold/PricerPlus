namespace ViewModel
 
//Representation of a Payment to the UI
type PaymentViewModel(input : PaymentRecord) = 
    inherit ViewModelBase()

    let mutable userInput = input
    let mutable value : Money option = None

    member this.TradeName 
        with get() = userInput.TradeName
        and set(x) = 
            userInput <- {userInput with TradeName = x }
            base.Notify("TradeName")

    member this.Expiry 
        with get() = userInput.Expiry
        and set(x) = 
            userInput <- {userInput with Expiry = x }
            base.Notify("Expiry")

    member this.Currency 
        with get() = userInput.Currency
        and set(x) = 
            userInput <- {userInput with Currency = x }
            base.Notify("Currency")

    member this.Principal 
        with get() = userInput.Principal
        and set(x) = 
            userInput <- {userInput with Principal = x }
            base.Notify("Principal")
    
    member this.Value
        with get() = value
        and set(x) = 
            value <- x
            base.Notify("Value")

    member this.CanBeDeferred
        with get() = userInput.CanBeDeferred
        and set(x) =
            userInput <- {userInput with CanBeDeferred = x }
            base.Notify("CanBeDeferred")

    member this.ValuePlusInterest
        with get() = value
        and set(x) = 
            value <- x
            base.Notify("ValuePlusInterest")

    // Invoke the valuation based on user input
    member this.Calculate(data : DataConfiguration, calculationParameters : CalculationConfiguration) = 
        
        //capture inputs
        let paymentInputs : PaymentValuationInputs = 
            {
                Trade = 
                         {
                             TradeName = this.TradeName
                             Expiry    = this.Expiry
                             Currency  = this.Currency
                             Principal = this.Principal
                             CanBeDeferred = this.CanBeDeferred
                         }
                Data = data
                CalculationsParameters = calculationParameters
            }
        //calculate
        let calc = PaymentValuationModel(paymentInputs).Calculate()
        let calcPlusInterest = PaymentValuationModel(paymentInputs).CalculatePlusInterest()

        //present to the user
        this.Value <- Option.Some (calc)
        this.ValuePlusInterest <- Option.Some (calcPlusInterest)

(* summary row. there is little functionality here, so this is very brief. *)
type SummaryRow = 
    {
        Currency: string
        Value : float
    }

//Representation of an Option to the UI
type OptionViewModel(input : OptionRecord) = 
    inherit ViewModelBase()

    let mutable userInput = input
    let mutable _BScall : Money option = None
    let mutable _BScallDelta : Money option = None
    let mutable _BSput : Money option = None
    let mutable _BSputDelta : Money option = None


    member this.OptionName 
        with get() = userInput.OptionName
        and set(x) = 
            userInput <- {userInput with OptionName = x }
            base.Notify("OptionName")

    member this.Expiry 
        with get() = userInput.Expiry
        and set(x) = 
            userInput <- {userInput with Expiry = x }
            base.Notify("Expiry")

    member this.Currency 
        with get() = userInput.Currency
        and set(x) = 
            userInput <- {userInput with Currency = x }
            base.Notify("Currency")

    member this.Strike
        with get() = userInput.Strike
        and set(x) =
            userInput <- {userInput with Strike = x }
            base.Notify("Strike")

    member this.BScall
        with get() = _BScall
        and set(x) =
            _BScall <- x
            base.Notify("BScall")

    member this.BScallDelta
        with get() = _BScallDelta
        and set(x) =
            _BScallDelta <- x
            base.Notify("BScallDelta")
    member this.BSput
        with get() = _BSput
        and set(x) =
            _BSput <- x
            base.Notify("BSput")

    member this.BSputDelta
        with get() = _BSputDelta
        and set(x) =
            _BSputDelta <- x
            base.Notify("BSputDelta")


    // Invoke the valuation based on user input
    member this.Calculate(data : DataConfiguration, calculationParameters : CalculationConfiguration) = 
        
        //capture inputs
        let optionInputs : OptionValuationInputs = 
            {
                OptionType = 
                         {
                             OptionName  = this.OptionName
                             Expiry      = this.Expiry
                             Currency    = this.Currency
                             Strike      = this.Strike
                         }
                Data = data
                CalculationsParameters = calculationParameters
            }
        //calculate
        let calc  = OptionValuationModel(optionInputs).Calculate()

        //present to the user
        this.BScall <- Option.Some (calc)
        this.BScallDelta <- Option.Some (calc)
        this.BSput <- Option.Some (calc)
        this.BSputDelta <- Option.Some (calc)
