#r "nuget: Akka.FSharp"

open Akka.Actor
open Akka.Configuration
open Akka.FSharp
open System.Collections.Generic
open System
// open Math

let args :string array = fsi.CommandLineArgs
let nodesNum = args.[1] |> int
let topology = args.[2] |> string
let algorithm = args.[3] |> string


type Message =  struct
    val mutable s:float
    val mutable w:float
    new(s:float,w:float) = {s=s;w=w}
end

let endNum = 10 
let roumor = "recieve"
let path = "akka://Project2/user/"
let total = Array.create nodesNum 0
let temp = Array.create nodesNum -1

let statues = Array.create nodesNum false

let mutable alldone = false
let mutable round = 0 
let mutable round_check = false

let devide_count = Array.create 3 0.0
let devide = Array.create nodesNum 0.0


for i = 0 to nodesNum-1 do 
    devide.[i] <- float(i)

let system = ActorSystem.Create("Project2")
 

let worker (name, actorId, endNum) =
    spawn system name <| fun mailbox ->
        let neighbors = new List<int>()
        let col=Math.Pow(float nodesNum,1.0/3.0)|>ceil|>int
        //let col = int(ceil((float(nodesNum)**(1.0/3.0))))
        let col_square = int(col*col)
        let col_cubic = int(col*col*col) // collom number of the cube
        if topology = "full" then
            //printfn("0")
            for i = 0 to nodesNum-1 do
                if i <> actorId then neighbors.Add(i)

        elif topology = "line" then
            //printfn("1")
            if actorId = 0 then neighbors.Add(1)
            elif actorId = nodesNum then neighbors.Add(actorId - 1)
            else neighbors.Add(actorId-1)
                 neighbors.Add(actorId+1)
               


        elif topology = "3D" || topology = "imp3D" then
            //printfn("2")
            // up and down
            if (actorId+1) - col_square > 0 && (actorId+1) + col_square <= col_cubic then
                neighbors.Add(actorId - col_square)
                if (actorId+1) + col_square <= nodesNum then
                    neighbors.Add(actorId + col_square)
            elif (actorId+1) - col_square <= 0 then
                neighbors.Add(actorId + col_square)
            elif (actorId+1) + col_square > col_cubic then
                neighbors.Add(actorId - col_square)

            // left and right
            if (actorId+1) % col = 1 then
                 if (actorId+1) + 1 <= nodesNum then neighbors.Add(actorId + 1)
            elif actorId % col = 0 then
                neighbors.Add(actorId - 1 )
            else
                neighbors.Add(actorId - 1)
                if  (actorId+1) + 1 <= nodesNum then neighbors.Add(actorId + 1)

            // front and back
            if  (actorId+1) % col_square >= 1 &&  (actorId+1) % col_square <= col then
                if  (actorId+1) + col <= nodesNum then neighbors.Add(actorId + col)
            elif  (actorId+1) % col_square >= (col-1)*col+1 ||  (actorId+1) % col_square = 0 then
                neighbors.Add(actorId - col)
            else
                neighbors.Add(actorId - col)
                if  (actorId+1) + col <= nodesNum then neighbors.Add(actorId + col)


            // if choose imp3D
            if topology = "imp3D" then
                //printfn("3")
                let rand = System.Random()
                let mutable id = rand.Next(0, nodesNum)
                while neighbors.Count < nodesNum-1 && (neighbors.Contains(id) || id = actorId) do
                    id <- rand.Next(0, nodesNum)
                neighbors.Add(id)

        let gossip_prop(actorId) = 
            let rand = System.Random()
            let propgateNum = rand.Next(neighbors.Count)

            let  propgate = 
                select (path + (neighbors.[propgateNum]).ToString()) system

            propgate <! roumor


        let mutable info =Message(float(actorId),w=1.0)

        let push_sum_prop = 
            let rand = System.Random()
            let propgateNum = rand.Next(neighbors.Count)

            let  propgate = 
                select (path + (neighbors.[propgateNum]).ToString()) system

            info.s <- info.s /2.0
            info.w <- info.w /2.0

            propgate <! info


        let rec loop() = 
            actor{
                let! msg = mailbox.Receive()
                match box msg with 
                    | :? string as _message ->
                        if _message = roumor then
                            if total.[actorId]< endNum then
                                total.[actorId]<- total.[actorId] + 1 
                            if total.[actorId] >= endNum then 
                                statues.[actorId] <- true

                        elif _message = "Gossipsend" then 
                            if not statues.[actorId] then 
                                gossip_prop(actorId)
                        elif _message ="pushsumsend" then
                            push_sum_prop
                    | :? Message as  rm ->
                        info.s <- rm.s
                        info.w <- rm.w

                    | _-> printfn "error find "
                if total.[actorId] < endNum then
                    gossip_prop(actorId)
            
                return! loop()           
            }
        loop()

let gossip (topology, nodesNum, endNum) =
    let actorList =
        [| for i = 0 to nodesNum-1 do
            worker (i.ToString(), i, endNum) |]

    select (path + "0") system <! roumor
    while (not alldone) do
        let mutable finishNum = 0
        for i = 0 to nodesNum-1 do
            if total.[i]>0 && total.[i]<endNum then
                select (path + (i).ToString()) system <! "Gossipsend"
            if statues.[i] then finishNum <- finishNum + 1
        if finishNum = nodesNum then alldone <- true
        else if total = temp then alldone <- true

        for i=0 to nodesNum-1 do
            temp.[i] <- total.[i]


    for i in 0 .. nodesNum - 1 do
        // printfn "actor%d : %d" i count.[i]
        system.Terminate().Wait()

let pushSum (topology,nodesNum,endNum) =
    let actorList =
        [| for i = 0 to nodesNum-1 do
            worker (i.ToString(), i, endNum) |]

    while (not round_check) do
        let mutable sum = 0.0
        for i = 0 to nodesNum-1 do
            select (path + i.ToString()) system <! "pushsumsend"
        for i=0 to nodesNum - 1 do
            sum <- sum + devide.[i]

        if round>2 && abs(sum-devide_count.[(round-1)%3])<10e-10 then
            if abs(devide_count.[(round-1)%3]-devide_count.[(round-2)%3])<10e-10 then
                if abs(devide_count.[(round-2)%3]-devide_count.[round%3])<10e-10 then
                    round_check <- true

        devide_count.[round%3] <- sum
        sum <- 0.0
        round <- round + 1


let stopWatch = System.Diagnostics.Stopwatch.StartNew()
if algorithm = "gossip" then
    gossip (topology,  nodesNum, endNum)
else if algorithm = "push-sum" then
    pushSum (topology, nodesNum, endNum)

stopWatch.Stop()
printfn "%f" stopWatch.Elapsed.TotalMilliseconds

system.Terminate().Wait()