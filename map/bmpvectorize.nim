import nimBMP
import sets
import hashes
import os
import strutils

type
    Position* = object
        x*, y*: int
    
    Color* = array[4, char]

    Line* = object
        src*: Position
        dest*: Position

proc hash(pos: Position): Hash =
    var h: Hash = 0
    h = h !& hash(pos.x)
    h = h !& hash(pos.y)
    return !$h

proc hash(v: Line): Hash =
    var h: Hash = 0
    h = h !& hash(v.src)
    h = h !& hash(v.dest)
    return !$h

proc log(msg: string): void =
    echo msg

proc reverse(v: Line): Line =
    return Line(src: v.dest, dest: v.src)

proc `$`(pos: Position): string =
    return "[" & $pos.x & "," & $pos.y & "]"

proc `$`(pos: Line): string =
    return $pos.src & " => " & $pos.dest

proc `-`(p1: Position, p2: Position): Position =
    return Position(x: p1.x - p2.x, y: p1.y - p2.y)

proc toHex(color: Color): string =
    result = "#"
    result.add(toHex(int(color[0]), 2))
    result.add(toHex(int(color[1]), 2))
    result.add(toHex(int(color[2]), 2))

# Get a power of two which is larger than `lower`.
proc powerOfTwoAbove(lower: int, current: int = 1): int =
    if current > lower:
        return current
    return powerOfTwoAbove(lower, current * 2)

# Get the neighboring positions to `pos`, ignoring
# positions outside the boundary defined by `w` and `h` (origin [0, 0]).
proc getNeighbors(pos: Position, w: int, h: int): seq[Position] =
    const dirs = [
        # [-1, -1], [1, 1], [-1, 1], [1, -1],
        [0, 1], [0, -1], [1, 0], [-1, 0]
    ]

    result = @[]
    
    for dir in dirs:
        let neighbor = Position(x: pos.x + dir[0], y: pos.y + dir[1])
        if neighbor.x > -1 and neighbor.y > -1 and neighbor.x < w and neighbor.y < h:
            result.add(neighbor)

proc getColor(pos: Position, img: BMPResult): Color =
    # `data` is a 1d-array with four values for each pixel, r-g-b-a.
    let offset = pos.x * 4 + pos.y * img.width * 4
    # I can't do splicing, because it turns it into a string (???)
    result[0] = img.data[offset]
    result[1] = img.data[offset + 1]
    result[2] = img.data[offset + 2]
    result[3] = img.data[offset + 3]

# Returns the border for the shape containing `start`.
# All positions contained in the shape is added to 
proc searchForBorder(start: Position, visited: var HashSet[Position], img: BMPResult): HashSet[Position] =
    var queue : seq[Position] = @[start]
    let color = getColor(start, img)

    visited.incl(start)
    result = initSet[Position]()

    log "Starting expanding..."
    log "Color: " & $int(color[0]) & ", " & $int(color[1]) & ", " & $int(color[2])  

    # breadth-first-search for the borders
    while len(queue) > 0:
        let current = queue[0]
        var added = false
        queue = queue[1..^1]

        var neighbors = getNeighbors(current, img.width, img.height)
        log $current & " :: " & $neighbors

        # if we don't have 8 valid neighbors, we are on the edge of the image,
        # which means we are on the border.
        if not added and len(neighbors) < 8 :
            result.incl(current)
            added = true

        for neighbor in neighbors:
            if color == getColor(neighbor, img):
                if not visited.contains(neighbor):
                    queue.add(neighbor)
                    visited.incl(neighbor)
            elif not added:
                result.incl(current)
                added = true

proc nextUnvisitedPosition(current: Position, visited: HashSet[Position], img: BMPResult): Position =
    result = current
    while visited.contains(result):
        if result.x == img.width - 1:
            result = Position(x: 0, y: result.y + 1)
        else:
            result = Position(x: result.x + 1, y: result.y)

proc simplifyPaths(paths: seq[seq[Line]]): seq[seq[Line]] = 
    result = @[]

    for polyIndex, path in paths:
        result.add(@[])
        var index = 0
        while index < len(path):
            let line = path[index]
            let dir = line.src - line.dest
            var dest = line.dest

            index += 1
            while index < len(path) and dir == path[index].src - path[index].dest:
                dest = path[index].dest
                index += 1

            result[polyIndex].add(Line(src: line.src, dest: dest))

proc createLines(border: HashSet[Position], img: BMPResult, color: Color): seq[Line] =
    result = @[]

    for borderPos in border:
        let right  = Position(x: borderPos.x + 1, y: borderPos.y)
        let left   = Position(x: borderPos.x - 1, y: borderPos.y)
        let bottom = Position(x: borderPos.x, y: borderPos.y + 1)
        let top    = Position(x: borderPos.x, y: borderPos.y - 1)

        # Each pixel consist of four "points": topleft, topright, bottomleft, bottomright.
        # The lines are all between those points.
        if borderPos.x >= img.width - 1 or color != getColor(right, img):
            result.add(Line(src: right, dest: Position(x: borderPos.x + 1, y: borderPos.y + 1)))
        if borderPos.x == 0 or color != getColor(left, img):
            result.add(Line(src: borderPos, dest: bottom))
        if borderPos.y >= img.height - 1 or color != getColor(bottom, img):
            result.add(Line(src: bottom, dest: Position(x: borderPos.x + 1, y: borderPos.y + 1)))
        if borderPos.y == 0 or color != getColor(top, img):
            result.add(Line(src: borderPos, dest: right))


proc appendSvgText(svg: var string, paths: seq[seq[Line]], color: Color): void =
    svg.add("    <g fill-rule=\"evenodd\" fill=\"" & color.toHex() & "\">\n")
    svg.add("         <path d=\"")
    for pindex, pathlines in paths:
        for index, line in pathlines:
            if index == 0:
                svg.add("M " & $line.src.x & "," & $line.src.y & " L ")
                svg.add($line.dest.x & "," & $line.dest.y & " ")
            else:
                svg.add($line.dest.x & "," & $line.dest.y & " ")
        if pindex != len(paths) - 1:
            svg.add("\n                  ")
    svg.add("\"/>\n")
    svg.add("    </g>\n")

let bmpFile = paramStr(1)
let svgFile = paramStr(2)

let img = loadBMP32(bmpFile)
var pos = Position(x: 0, y: 0)
var visited = initSet[Position](powerOfTwoAbove(img.width * img.height))

# Steps:
# - find borders
# - find lines
# - enforce orientation
# - simplyfy lines

var svg = ""
svg.add("<svg xmlns=\"http://www.w3.org/2000/svg\" version=\"1.1\">\n")

var nAreas = 0

while pos.y < img.height:
    nAreas += 1
    echo "Starting to process area " & $nAreas

    let border = searchForBorder(pos, visited, img)
    let color = getColor(pos, img)
    log "Done expanding..."
    log "Border :: " & $border
    var lines = createLines(border, img, color)
    log "lines :: " & $lines

    # Enforce orientation
    var paths : seq[seq[Line]] = @[]
    var polyIndex = 0
    var done = initSet[Line]()

    while card(done) < len(lines):
        paths.add(@[])

        var startIndex = 0
        while done.contains(lines[startIndex]):
            startIndex += 1
        let start = lines[startIndex]
        paths[polyIndex].add(start)
        done.incl(start)

        var current = start
        while current.dest != start.src:
            # there is an issue somewhere. sometimes, there exist four lines in a single point.
            # this shouldn't happen.
            for vindex, line in lines:
                if line != current and line.dest == current.dest and not done.contains(line):
                    lines[vindex] = line.reverse()
                    current = lines[vindex]
                    done.incl(current)
                    paths[polyIndex].add(lines[vindex])
                    break
                if line != current and line.src == current.dest and not done.contains(line):
                    current = line
                    done.incl(current)
                    paths[polyIndex].add(line)
                    break
        
        polyIndex += 1

    log "NSubshapes = " & $len(paths)
    for index, lines in paths:
        log $index & " = " & $lines

    # Merge lines where possible
    let mergedPaths = simplifyPaths(paths)

    appendSvgText(svg, mergedPaths, color)

    log "\n\n"

    pos = nextUnvisitedPosition(pos, visited, img)

svg.add("</svg>")

writeFile(svgFile, svg)