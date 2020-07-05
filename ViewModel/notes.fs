module notes
(*
module klasy =
    type MyClass(x: int) =
        //primary constructor
        let y = x + 1
        do printfn "constructing MyClass obj %d" x
        
        
        let mutable myState = 0
        new() = MyClass(3)

        member this.X = x
        member this.Y with get() = x
        member this.State
            with get() = myState
            and set(newValue) = myState <- newValue
        member this.MyMethod() = printfn "My method"

    let a = MyClass(4)
    a.X
    a.Y //???
    a.State //???
    a.MyMethod  //???
    a.MyMethod()    //???


    type MyClass2 (dataIn) as self=
        let data = dataIn
        do
            self.PrintMessage()
        member x.PrintMessage() =   //why this? why "x." works?
            printfn "Hello, thats me, Constructor! %A" data        
        

    let x = MyClass2("Kuba")
    let y = MyClass2(7)

    type MyGenericClass<'a> (x: 'a) =
        do printfn "%A" x

    let g1 = MyGenericClass( seq { for i in 1 .. 10 -> (i, i*i) } )



module dziedziczenie =

    type BaseType() =
        member x.BaseMethod() = printfn "Base method"
    
    type DerivedType() =
        inherit BaseType()
        member x.DerivedMethod() = printfn "Derived method"
        member x.CallBase() =
            base.BaseMethod()
            printfn "CallBase() done"

    let d = DerivedType()
    d.DerivedMethod()
    d.CallBase()
    d.BaseMethod()

module interfejsy =
    type MyInterface =
        abstract Calculate : int -> int -> int
        abstract GiveName : unit -> string

    type MyImpl() =
        interface MyInterface with
            member this.Calculate x1 x2 = x1 + x2
            member this.GiveName () = "I am MyImpl"
    
    let z = MyImpl()
    (z :> MyInterface).GiveName()


*)

let haircutS = "hello"

let haircutN = float "9.8"