# GROD2

## Game Resource Object Data Structure 2

GROD2 is a data structure designed to manage game resources in interactive fiction games. It provides a flexible and extensible way to handle various game assets.

## Features

- Supports multiple types of game resources in a text-based format.
- Provides a simple interface for accessing and manipulating game resources.
- Allows for easy addition of new items and updates to existing items.
- Designed to be lightweight and efficient, minimizing resource usage while maximizing performance.
- Supports multiple platforms, including Windows, Linux, and macOS.

## Structure

The GROD2 data structure consists of the following components:

- **Item**: Represents a single game value. Each item has a unique key and a text value. Keys are not case-sensitive and cannot be empty or null.
- **Data**: Represents a collection of items that can be accessed by their unique identifiers. This allows for efficient retrieval and manipulation of game values.
- **Level**: Represents data that belong to a specific game level or context. Levels have names and can be used to organize items within the game.
- **Parent**: Represents a hierarchical relationship of levels. Each level can have a parent level for retriving base items which don't exist in the current level.
