namespace ViewModel
 
////Representation of an Option to the UI
//type OptionViewModel(input : OptionRecord) = 
//    inherit ViewModelBase()

//    let mutable userInput = input
//    let mutable value : Money option = None

//    member this.OptionName 
//        with get() = userInput.OptionName
//        and set(x) = 
//            userInput <- {userInput with OptionName = x }
//            base.Notify("OptionName")

//    member this.Expiry 
//        with get() = userInput.Expiry
//        and set(x) = 
//            userInput <- {userInput with Expiry = x }
//            base.Notify("Expiry")

//    member this.Currency 
//        with get() = userInput.Currency
//        and set(x) = 
//            userInput <- {userInput with Currency = x }
//            base.Notify("Currency")

//    member this.Strike
//        with get() = userInput.Strike
//        and set(x) =
//            userInput <- {userInput with Strike = x }
//            base.Notify("Strike")
