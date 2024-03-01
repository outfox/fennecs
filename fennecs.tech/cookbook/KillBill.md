---
title: 2. Kill Bill (Relations)
---

# Paying a visit to everyone who crossed you
Assume we wanted to get even with every ~~old friend~~ Entity that once crossed us...

```cs
//  We make simple components, for a simple plot.
struct Grudge;
struct Crossed;
struct Position
{
    Vector3 where;
}

// Set the stage.
var world = new fennecs.World();

// This is us. Here. Now.
var myself = world.Spawn().Add<Position>();

// Five cross us. This is how it went:
for (var i = 0; i < 5; i++)
{
    var entity = world.Spawn()
        .Add<Position>();
        .AddRelation<Crossed>(myself);

    // And we will never forget.
    myself.AddRelation<Grudge>(entity);
}

// 4 years in a coma?!
Thread.Sleep(TimeSpan.FromDays(365 * 4));

// Well rested, we query for their positions. To pay that visit.
// And maybe know who they are. So we can prepare better.
var query = world.Query<Identity, Position>()
                .Has<Crossed>(myself)
                .Build();

// This would get them all in one fell swoop. 
// But nah... that's not enough for a movie!
// query.Clear();

// Remember where we came from.
ref var myPosition = myself.Ref<Position>();
var home = myposition;

// Visit each one. Then leave alone, unseen.
query.For((ref Identity identity, ref Position position,) => {
    myPosition = position;
    world.Despawn(identity);
});

// Roll credits.
myPosition = home;
```
