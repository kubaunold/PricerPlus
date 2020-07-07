namespace ViewModel
open System

type SimulationInputs = 
    {
        //Trade : PaymentRecord
        //Data : DataConfiguration
        SimulationParameters: SimulationConfiguration
    }

type SimulationOutput =
    {
        BSPut: float
        BSCall: float
        sigma: float
    }

type SimulationModel (inputs:SimulationInputs) =

    //here I can simulate BS Model based on it's inputs stored in Simulation Inputs
    member this.Calculate() : SimulationOutput = 
        let r = {BSPut=2.; BSCall=3.; sigma=1.}
        r
    
