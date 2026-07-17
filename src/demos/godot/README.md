# fennecs Godot Demos

Welcome to the official Godot Demo Repository for **fenn**ecs, the tiny, high-energy Entity Component System for C#!

This repository contains a collection of demo projects showcasing how to integrate **fenn**ecs with the Godot game engine. Each demo focuses on different aspects of game development using ECS, from basic setup to advanced techniques.

Whether you're new to ECS or an experienced developer looking to optimize your Godot projects, these demos provide practical examples and best practices to help you get started.

Dive in, explore the code, and unleash the power of **fenn**ecs in your Godot games!

## Running & Navigating

Open the project in Godot 4.6+ (.NET edition) and press Play. The project starts on a **Main Menu** (`zzzShared/Menu/main_menu.tscn`) that lists all demos with a short explanation of the fennecs features each one showcases.

A `DemoNavigator` autoload (`zzzShared/Navigation/demo_navigator.gd`) draws a menu bar along the top of the screen and provides global shortcuts, so you can hop between scenes at any time:

| Key | Scene |
|-----|-------|
| **F1** (or Esc) | Main Menu |
| **F2** | Lots of Cubes — CPU simulation of 100k+ Entities, bulk-rendered via `Query.Raw` into a MultiMesh |
| **F3** | N-Body Problem — Entity-Entity relations; cliques of stars attracting each other |
| **F4** | Battleships — mixed ECS + scene tree; ships, guns, and bullets as Entities |

To add a demo of your own, register its scene in `zzzShared/Navigation/demo_registry.gd` — the menu and the navigation bar both build themselves from that list.
