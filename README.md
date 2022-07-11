RBush
=====

RBush is a high-performance .NET library for 2D **spatial indexing** of points and rectangles.
It's based on an optimized **R-tree** data structure with **bulk insertion** support.

*Spatial index* is a special data structure for points and rectangles
that allows you to perform queries like "all items within this bounding box" very efficiently
(e.g. hundreds of times faster than looping over all items).
It's most commonly used in maps and data visualizations.

This code has been copied over from the Javascript [RBush](https://github.com/mourner/rbush) library.

[![Build status](https://github.com/viceroypenguin/RBush/actions/workflows/build.yml/badge.svg)](https://github.com/viceroypenguin/RBush/actions)
[![License](https://img.shields.io/github/license/viceroypenguin/RBush)](license.txt)

## Install

Install with Nuget (`Install-Package RBush`).

## Usage

### Creating a Tree

First, define the data item class to implement `ISpatialData`. Then the class can be used as such:

```csharp
class Point : ISpatialData
{
  public Point(Envelope envelope) =>
    _envelope = envelope;
  private readonly Envelope _envelope;
  public public ref readonly Envelope Envelope => _envelope;
}

var tree = new RBush<Point>()
```

An optional argument (`maxEntries`) to the constructor defines the maximum number 
of entries in a tree node. `9` (used by default) is a reasonable choice for most 
applications. Higher value means faster insertion and slower search, and vice versa.

```csharp
var tree = new RBush<Point>(maxEntries: 16)
```

### Adding Data

Insert an item:

```csharp
var item = new Point(
  new Envelope(
    MinX: 0,
    MinY: 0,
    MaxX: 0,
    MaxY: 0));
tree.Insert(item);
```

### Bulk-Inserting Data

Bulk-insert the given data into the tree:

```csharp
var points = new List<Point>();
tree.BulkLoad(points);
```

Bulk insertion is usually ~2-3 times faster than inserting items one by one.
After bulk loading (bulk insertion into an empty tree),
subsequent query performance is also ~20-30% better.

Note that when you do bulk insertion into an existing tree,
it bulk-loads the given data into a separate tree
and inserts the smaller tree into the larger tree.
This means that bulk insertion works very well for clustered data
(where items in one update are close to each other),
but makes query performance worse if the data is scattered.

### Search

```csharp
var result = tree.Search(
    new Envelope
    (
        minX: 40,
        minY: 20,
        maxX: 80,
        maxY: 70
    );
```

Returns an `IEnumerable<T>` of data items (points or rectangles) that the given bounding box intersects.

```csharp
var allItems = tree.Search();
```

Returns all items of the tree.

### Removing Data

#### Remove a previously inserted item:

```csharp
tree.Delete(item);
```

Unless provided an `IComparer<T>`, RBush uses `EqualityComparer<T>.Default` 
to select the item. If the item being passed in is not the same reference 
value, ensure that the class supports `EqualityComparer<T>.Default` 
equality testing.

#### Remove all items:

```csharp
tree.Clear();
```

## Credit

This code was adapted from a Javascript library called [RBush](https://github.com/mourner/rbush). The only
changes made were to adapt coding styles and preferences. 

## Algorithms Used

* single insertion: non-recursive R-tree insertion with overlap minimizing split routine from R\*-tree (split is very effective in JS, while other R\*-tree modifications like reinsertion on overflow and overlap minimizing subtree search are too slow and not worth it)
* single deletion: non-recursive R-tree deletion using depth-first tree traversal with free-at-empty strategy (entries in underflowed nodes are not reinserted, instead underflowed nodes are kept in the tree and deleted only when empty, which is a good compromise of query vs removal performance)
* bulk loading: OMT algorithm (Overlap Minimizing Top-down Bulk Loading) combined with Floyd–Rivest selection algorithm
* bulk insertion: STLT algorithm (Small-Tree-Large-Tree)
* search: standard non-recursive R-tree search

## Papers

* [R-trees: a Dynamic Index Structure For Spatial Searching](http://www-db.deis.unibo.it/courses/SI-LS/papers/Gut84.pdf)
* [The R*-tree: An Efficient and Robust Access Method for Points and Rectangles+](http://dbs.mathematik.uni-marburg.de/publications/myPapers/1990/BKSS90.pdf)
* [OMT: Overlap Minimizing Top-down Bulk Loading Algorithm for R-tree](http://ftp.informatik.rwth-aachen.de/Publications/CEUR-WS/Vol-74/files/FORUM_18.pdf)
* [Bulk Insertions into R-Trees Using the Small-Tree-Large-Tree Approach](http://www.cs.arizona.edu/~bkmoon/papers/dke06-bulk.pdf)
* [R-Trees: Theory and Applications (book)](http://www.apress.com/9781852339777)

## Development

Clone the repository and open `RBush.sln` in Visual Studio.

## Compatibility

RBush should run on any .NET system that supports .NET Standard 1.2 (.NET Framework 4.5.1 or later; .NET Core 1.0 or later).


