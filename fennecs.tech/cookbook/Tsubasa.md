---
title: 1. Tsubasa (Basics)
outline: [2, 3]
---

# Nankatsu High School Team's first Practice

::: info :neofox_floof_mug: MMMH, REAL CODE
This **RUNS**! *Playful premises aside*, this is a functioning showcase of **fenn**ecs features.

Get comfy, grab a cup of ~~Java~~ ~~CoffeeScript~~ ~~Visual J#~~ whatever, and get your paws dirty playing around in the code! It's good fun!

All `.csproj` and `.cs` files are [over here on Github!](https://github.com/thygrrr/fennecs/blob/main/examples/cookbook) 

:::

### Premise
Japan's least coordinated soccer team tries to score a goal. They're just kids, all running after the ball at once. They have no ball control whatsoever. Only one of them is kind of a good shot, some kid named "Tsubasa".

We create a team of 11 Entities with a `Player` and `Name` Component, and a ball entity with a `Ball`. The match has been going on for a long time, so as soon as the only player with a `Talent` Component scores, the match ends with a golden goal.

In our "game" loop, we get the current position of our ball Entity, and let each player Entity run after it. If they get close enough, they kick the ball to a new position if they have no talent; and finally when a player with the `Talent` component hits the ball, the game ends.

### Implementation
<<< ../../cookbook/Tsubasa.cs {cs:line-numbers}

### Outcome
The above code is the actual code, yet this output is copy-pasta and may not be re-generated each time the code is updated. *(mumble CI-CD mumble TODO mumble)*
```shell
dotnet run --project Tsubasa.csproj
```
```txt 
Kojiro runs towards the ball! <13,888162. 29,777138> -> <14,477499. 30,98985>
Genzo runs towards the ball! <13,095423. 28,202503> -> <14,08113. 30,202532>
Taro runs towards the ball! <13,2696. 28,601097> -> <14,168219. 30,401829>
Hikaru runs towards the ball! <11,8074665. 25,506947> -> <13,437152. 28,854753>
Jun runs towards the ball! <13,2663355. 28,452217> -> <14,166586. 30,327389>
Shingo runs towards the ball! <13,918681. 29,014359> -> <14,49276. 30,60846>
Ryo runs towards the ball! <14,421239. 30,882528> -> <14,744038. 31,542545>
Takeshi runs towards the ball! <14,542307. 31,07476> -> <14,804572. 31,63866>
Masao runs towards the ball! <13,980301. 29,96994> -> <14,523569. 31,08625>
Kazuo runs towards the ball! <13,885527. 29,74327> -> <14,476182. 30,972916>
>>> Tsubasa kicks the ball!
>>> TSUBASA SCORES!!!
```