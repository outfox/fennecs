---
title: 1. Tsubasa (Basics)
outline: [2, 3]
---

# Nankatsu High School Team's first Practice

::: info :neofox_floof_mug: MMMH, REAL CODE
This **RUNS**! *Playful premises aside*, this is a functioning showcase of **fenn**ecs features.

Get comfy, grab a cup of ~~Java~~ ~~CoffeeScript~~ ~~Visual J#~~ whatever, and get your paws dirty playing around in the code! It's good fun!

All `.csproj` and `.cs` files are [over here on Github!](https://github.com/thygrrr/fennecs/blob/main/cookbook) 

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
          Kojiro runs towards the ball! ..... d = 1,79m
           Genzo runs towards the ball! ..... d = 2,73m
            Taro runs towards the ball! ..... d = 3,66m
          Hikaru runs towards the ball! ..... d = 1,62m
             Jun runs towards the ball! ..... d = 3,22m
          Shingo runs towards the ball! ..... d = 4,87m
             Ryo runs towards the ball! ..... d = 2,49m
         Takeshi runs towards the ball! ..... d = 1,77m
           Masao runs towards the ball! ..... d = 1,79m
           Kazuo runs towards the ball! ..... d = 2,33m
>>>>> Tsubasa kicks the ball!
***** TSUBASA SCORES!!!
```