namespace ViewModel




type SimulationViewModel () =
    inherit ViewModelBase()

    let mutable result : SimulationOutput option = None

    member this.Result
        with get() = result
        and set(x) =
            result <- x
            base.Notify("ResultOfSimulation")

    member this.Calculate( parameters: SimulationConfiguration ) =
        //capture inputs
        let simulationInputs : SimulationInputs =
            {
                SimulationParameters = parameters
            }

        //calculate     
        let calc = SimulationModel(simulationInputs).Calculate()

        //present to the user
        this.Result <- Option.Some (calc)
