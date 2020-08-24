Public Class DynamicBitmap

    'Completed 28 May 2010
    'Updated to Version 2.4 on 18 June 2010
    'Current Version 2.4.0

    Private clsBitmapImage As Bitmap
    Private clsBitmapMemory As Bitmap
    Private clsMemoryBuffer As Graphics
    Private clsGraphicsBuffer As Graphics

    Private isGrid As Boolean
    Private tlWidth As Integer
    Private tlHeight As Integer
    Private rowCnt As Integer
    Private columnCnt As Integer
    Private bmpFileName As String

    Private doesExist As Boolean

    Public Path() As Integer

    Public Enum Direction
        North
        South
        East
        West
        NorthEast
        SouthEast
        NorthWest
        SouthWest
    End Enum

    ''' <summary>
    ''' Draw onto graphics surface of this class' bitmap
    ''' </summary>
    ''' <param name="source"></param>
    ''' <param name="srcRectangle"></param>
    ''' <param name="destRectangle"></param>
    ''' <param name="InterpolMode"></param>
    ''' <remarks></remarks>
    Public Sub DrawOnSurface(ByVal source As Bitmap, ByVal srcRectangle As Rectangle, ByVal destRectangle As Rectangle, _
                             Optional ByVal Stretch As Boolean = True, Optional ByVal Center As Boolean = False, _
                             Optional ByVal InterpolMode As Drawing2D.InterpolationMode = Drawing2D.InterpolationMode.Default)

        Dim texBrush As TextureBrush = New TextureBrush(source)
        Dim tmpL As Integer, tmpT As Integer

        clsGraphicsBuffer.InterpolationMode = InterpolMode
        If Stretch = True Then
            clsGraphicsBuffer.DrawImage(source, destRectangle, srcRectangle, GraphicsUnit.Pixel)
        Else
            If Center = False Then
                clsGraphicsBuffer.DrawImage(source, destRectangle.X, destRectangle.Y, srcRectangle, GraphicsUnit.Pixel)
            Else
                tmpL = destRectangle.X + ((destRectangle.Width - srcRectangle.Width) / 2)
                tmpT = destRectangle.Y + ((destRectangle.Height - srcRectangle.Height) / 2)
                clsGraphicsBuffer.DrawImage(source, tmpL, tmpT, srcRectangle, GraphicsUnit.Pixel)
            End If

        End If
        texBrush.Dispose()
    End Sub




    ''' <summary>
    ''' INITIALIZING CLASS SECTION 
    ''' : 3 Options to initialize this class - load a bitmap, create a blank bitmap, or create parameters for a grid and subsequent blank bitmap
    ''' </summary>
    ''' <param name="fromFileName"></param>
    ''' <remarks></remarks>
    Public Sub LoadBitmap(ByVal fromFileName As String, Optional ByVal tilesWidth As Integer = 0, Optional ByVal tilesHeight As Integer = 0)
        isGrid = False 'Only set to true if "CreateGrid" is called
        clsBitmapImage = New Bitmap(fromFileName)
        clsGraphicsBuffer = Graphics.FromImage(clsBitmapImage)
        bmpFileName = fromFileName
        TileHeight = tilesHeight
        TileWidth = tilesWidth
        doesExist = True
    End Sub
    Public Sub CreateBitmap(ByVal bmpWidth As Integer, ByVal bmpHeight As Integer)
        isGrid = False 'Only set to true if "CreateGrid" is called
        clsBitmapImage = New Bitmap(bmpWidth, bmpHeight)
        clsGraphicsBuffer = Graphics.FromImage(clsBitmapImage)
        TileWidth = bmpWidth
        TileHeight = bmpHeight
        doesExist = True
    End Sub
    Public Sub CreateGrid(ByVal tilesWidth As Integer, ByVal tilesHeight As Integer, ByVal rowCount As Integer, ByVal columnCount As Integer)

        'The isGrid variable tells the TileWidth/TileHeight properties not to automatically
        'determine the row/column count since the user has specified those variables in this subroutine
        isGrid = True

        'Creates a blank bitmap equal to the size of the grid
        CreateBitmap(columnCount * tilesWidth, rowCount * tilesHeight)

        'Sets the dimensions of the specified grid
        TileWidth = tilesWidth
        TileHeight = tilesHeight

        'Sets the row and column count based on what the user has input
        rowCnt = rowCount
        columnCnt = columnCount



        doesExist = True
    End Sub





    ''' <summary>
    ''' TILE METHODS SECTION
    '''  : Tiles can be either grid cells or sprite blocks, depending on how the user is using the class
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TileHeight As Integer
        'Sets the value of the tile height (for the user, this may be a cell property 
        'or sprite property if LoadBitmap was called and a tile sheet file was loaded
        Get
            TileHeight = tlHeight
        End Get
        Set(ByVal value As Integer)
            tlHeight = value
            If tlHeight = 0 Then tlHeight = clsBitmapImage.Height
            'If the user is setting parameters for a sprite map or tile sheet, 
            'the property automatically determines the row count 
            If isGrid = False Then
                rowCnt = Math.Truncate(clsBitmapImage.Height / tlHeight)
            End If
        End Set
    End Property
    Public Property TileWidth As Integer
        'Sets the value of the tile width (for the user, this may be a cell property 
        'or sprite property if LoadBitmap was called and a tile sheet file was loaded
        Get
            TileWidth = tlWidth
        End Get
        Set(ByVal value As Integer)
            tlWidth = value
            If tlWidth = 0 Then tlWidth = clsBitmapImage.Width
            'If the user is setting parameters for a sprite map or tile sheet, 
            'the property automatically determines the column count 
            If isGrid = False Then
                columnCnt = Math.Truncate(clsBitmapImage.Width / tlWidth)
            End If
        End Set
    End Property

    Public ReadOnly Property TileCount As Integer
        Get
            TileCount = rowCnt * columnCnt
            Return TileCount
        End Get
    End Property
    Public ReadOnly Property Point(Optional ByVal TileIndex As Integer = 0) As Point
        Get
            'Sets default point to upper left/top position of bitmap 
            Point.X = 0
            Point.Y = 0

            'Error Handling
            If TileIndex < 1 Then Exit Property
            If TileIndex > TileCount Then Exit Property
            If TileIndex = 1 And TileCount = 1 Then Exit Property

            'Determine X point
            Point.X = TileIndex
            Do While Point.X > columnCnt
                Point.X -= columnCnt
            Loop
            Point.X -= 1
            Point.X = (TileWidth * Point.X)

            'Determine Y point
            Point.Y = TileIndex - 1
            Point.Y = Math.Truncate(Point.Y / columnCnt)
            Point.Y = Point.Y * TileHeight

            Return Point
        End Get
    End Property
    Public ReadOnly Property Rectangle(Optional ByVal TileIndex As Integer = 0) As Rectangle
        Get
            'Sets default rectangle to entire bitmap
            Rectangle = New Rectangle(0, 0, clsBitmapImage.Width, clsBitmapImage.Height)

            'Error Handling
            If TileIndex < 1 Then Exit Property
            If TileIndex > TileCount Then Exit Property
            If TileIndex = 1 And TileCount = 1 Then Exit Property

            'Sets rectangle to the specified tile
            Rectangle = New Rectangle(Point(TileIndex), New Size(TileWidth, TileHeight))

            Return Rectangle
        End Get
    End Property

    Public Function Tile(ByVal AtPoint As Point) As Integer
        'Passes parameters to primary Tile Function below
        Return Tile(AtPoint.X, AtPoint.Y)
    End Function
    Public Function Tile(ByVal X As Integer, ByVal Y As Integer) As Integer
        'Determines which tile the X,Y values reside in.  If 0 is returned, X,Y values are outside of bitmap area
        Tile = 0

        'Error Handling
        If X < 0 Or Y < 0 Then Exit Function
        'In the event the bitmap is larger than the tiled area, class does not check against bitmap.height/width properties
        If X >= (TileWidth * columnCnt) Or Y >= (TileHeight * rowCnt) Then Exit Function

        'At this point, we know that a valid point within the bitmap was passed
        Tile = 1
        If TileCount = 1 Then Exit Function

        Dim thisColumn As Integer
        Dim thisRow As Integer

        thisColumn = Math.Truncate(X / TileWidth)
        thisRow = Math.Truncate(Y / TileHeight)

        Tile = (thisRow * columnCnt) + thisColumn + 1
    End Function

    Public Function TileNeighbor(ByVal TileIndex As Integer, ByVal MoveTo As Direction, Optional ByVal StepCount As Integer = 1) As Integer
        'Return the tile index of the tile directly next to this position 
        'StepCount is how many tiles it steps in the specified direction
        Dim pntInTile As Point

        TileNeighbor = 0

        'Error Handling
        If TileIndex < 1 Then Exit Function
        If TileIndex > TileCount Then Exit Function
        If TileIndex = 1 And TileCount = 1 Then Exit Function
        If StepCount > columnCnt And StepCount > rowCnt Then Exit Function
        If StepCount < 1 Then StepCount = 1

        'Get the point value of the TileIndex 
        pntInTile = Point(TileIndex)

        'Add to either the X or Y values of the point value to move to a neighboring tile
        For clsI As Integer = 1 To StepCount
            Select Case MoveTo
                Case Direction.North
                    pntInTile.Y -= TileHeight
                Case Direction.NorthEast
                    pntInTile.X += TileWidth
                    pntInTile.Y -= TileHeight
                Case Direction.East
                    pntInTile.X += TileWidth
                Case Direction.SouthEast
                    pntInTile.X += TileWidth
                    pntInTile.Y += TileHeight
                Case Direction.South
                    pntInTile.Y += TileHeight
                Case Direction.SouthWest
                    pntInTile.X -= TileWidth
                    pntInTile.Y += TileHeight
                Case Direction.West
                    pntInTile.X -= TileWidth
                Case Direction.NorthWest
                    pntInTile.X -= TileWidth
                    pntInTile.Y -= TileHeight
            End Select
        Next

        'Check to see if is a valid tile (will return 0 if not)
        TileNeighbor = Tile(pntInTile)

    End Function




    ''' <summary>
    ''' Functions and Subroutines
    '''  : Primary workings of the class module
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub DrawTileLines()
        'Passes parameters to primary function below
        DrawTileLines(Color.Black)
    End Sub
    Public Sub DrawTileLines(ByVal LineColor As Color, Optional ByVal HideBorder As Boolean = False)
        'Will draw lines onto the bitmap

        'Tiles are considered directly next to each other, so technically there is no boundary between tiles
        'The border pixels of each tile will be drawn over when this subroutine is called

        Dim defPen As Pen
        Dim point1 As Point
        Dim point2 As Point

        'Default Pen is defined
        defPen = New Pen(LineColor, 1)

        'Draw column lines
        For c As Long = 0 To columnCnt
            If HideBorder = True And (c = 0 Or c = columnCnt) Then
                'Do not draw first or last column lines
            Else
                'Set starting point to draw column lines
                point1.X = (TileWidth * c)
                point1.Y = 0

                'Set ending point to draw column lines
                point2.X = point1.X
                point2.Y = (TileHeight * rowCnt)

                'Draw each column line
                If c <> columnCnt Then
                    clsGraphicsBuffer.DrawLine(defPen, point1.X, point1.Y, point2.X, point2.Y - 1)
                Else
                    'Draw last line 1 pixel to the left
                    clsGraphicsBuffer.DrawLine(defPen, point1.X - 1, point1.Y, point2.X - 1, point2.Y - 1)
                End If
            End If
        Next

        'Draw row lines
        For r As Long = 0 To rowCnt
            If HideBorder = True And (r = 0 Or r = rowCnt) Then
                'Do not draw first or last row lines
            Else
                'Set starting point to draw row lines
                point1.X = 0
                point1.Y = (TileHeight * r)

                'Set ending point to draw row lines
                point2.X = (TileWidth * columnCnt)
                point2.Y = point1.Y

                'Draw each row line 
                If r <> rowCnt Then
                    clsGraphicsBuffer.DrawLine(defPen, point1.X, point1.Y, point2.X - 1, point2.Y)
                Else
                    'Draw last line 1 pixel higher
                    clsGraphicsBuffer.DrawLine(defPen, point1.X, point1.Y - 1, point2.X - 1, point2.Y - 1)
                End If
            End If
        Next
    End Sub
    Public Sub MakeTransparent()
        MakeTransparent(Color.Magenta)
    End Sub
    Public Sub MakeTransparent(ByVal transparentColor As Color)
        clsBitmapImage.MakeTransparent(transparentColor)
    End Sub
    Public Sub AlphaBlend(Optional ByVal Alpha As Integer = 75)
        Dim i As Integer
        Dim j As Integer
        Dim clr As Color
        Dim newClr As Color
        For i = 0 To clsBitmapImage.Width - 1
            For j = 0 To clsBitmapImage.Height - 1
                clr = clsBitmapImage.GetPixel(i, j)
                newClr = Color.FromArgb(Alpha, clr.R, clr.G, clr.B)
                clsBitmapImage.SetPixel(i, j, newClr)
            Next
        Next
    End Sub
    Public Sub Text(ByVal txtString As String, ByVal printBox As Rectangle, ByVal useFont As Font, ByVal useBrush As Brush)
        Dim strFormat As New StringFormat

        strFormat.Alignment = StringAlignment.Center
        strFormat.LineAlignment = StringAlignment.Center

        clsGraphicsBuffer.DrawString(txtString, useFont, useBrush, printBox, strFormat)
    End Sub
    Public Sub FillTile(ByVal X As Integer, ByVal Y As Integer, ByVal fillColor As Color, Optional ByVal OutlineOnly As Boolean = False)
        'Passes parameters to primary FillTile Function below
        FillTile(Tile(X, Y), fillColor, OutlineOnly)
    End Sub
    Public Sub FillTile(ByVal TileIndex As Integer, ByVal fillColor As Color, Optional ByVal OutlineOnly As Boolean = False)
        'Fills the specified tile with a solid color; optional outline only
        Dim outlinePen As Pen

        'Error Handling
        If TileIndex < 1 Then Exit Sub
        If TileIndex > TileCount Then Exit Sub

        outlinePen = New Pen(fillColor)

        clsGraphicsBuffer.DrawRectangle(outlinePen, Rectangle(TileIndex))

        If OutlineOnly = True Then Exit Sub

        clsGraphicsBuffer.FillRectangle(New SolidBrush(fillColor), Rectangle(TileIndex))

    End Sub
    Public Sub FloodFill(ByVal X As Integer, ByVal Y As Integer, ByVal newColor As Color)
        'Passes parameters to primary FloodFill subroutine below
        Dim clsAPoint As Point
        clsAPoint.X = X
        clsAPoint.Y = Y
        FloodFill(clsAPoint, newColor)
    End Sub
    Public Sub FloodFill(ByVal AtPoint As Point, ByVal newColor As Color)
        Dim oldColor As Color = clsBitmapImage.GetPixel(AtPoint.X, AtPoint.Y)

        If oldColor.ToArgb <> newColor.ToArgb Then
            Dim pts As New Stack(1000)
            pts.Push(New Point(AtPoint.X, AtPoint.Y))
            clsBitmapImage.SetPixel(AtPoint.X, AtPoint.Y, newColor)

            Do While pts.Count > 0
                Dim pt As Point = DirectCast(pts.Pop(), Point)

                If pt.X > 0 Then ProcessPoint(pts, pt.X - 1, pt.Y, oldColor, newColor)
                If pt.Y > 0 Then ProcessPoint(pts, pt.X, pt.Y - 1, oldColor, newColor)
                If pt.X < clsBitmapImage.Width - 1 Then ProcessPoint(pts, pt.X + 1, pt.Y, oldColor, newColor)
                If pt.Y < clsBitmapImage.Height - 1 Then ProcessPoint(pts, pt.X, pt.Y + 1, oldColor, newColor)
            Loop

        End If
    End Sub
    Private Sub ProcessPoint(ByVal pts As Stack, ByVal X As Integer, ByVal Y As Integer, ByVal oldColor As Color, ByVal newColor As Color)
        Dim clr As Color = clsBitmapImage.GetPixel(X, Y)
        If clr.Equals(oldColor) Then
            pts.Push(New Point(X, Y))
            clsBitmapImage.SetPixel(X, Y, newColor)
        End If
    End Sub


    Public Sub ImageStore()
        clsBitmapMemory = New Bitmap(clsBitmapImage.Width, clsBitmapImage.Height)
        clsMemoryBuffer = Graphics.FromImage(clsBitmapMemory)
        clsMemoryBuffer.DrawImage(clsBitmapImage, New Point(0, 0))
    End Sub
    Public Sub ImageRestore()
        clsBitmapImage = New Bitmap(clsBitmapMemory.Width, clsBitmapMemory.Height)
        clsGraphicsBuffer = Graphics.FromImage(clsBitmapImage)
        clsGraphicsBuffer.DrawImage(clsBitmapMemory, New Point(0, 0))
    End Sub

    ''' <summary>
    ''' Basic Class Properties
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Exists() As Boolean
        Get
            Exists = doesExist
        End Get
    End Property
    Public ReadOnly Property Bitmap As Bitmap
        Get
            Bitmap = clsBitmapImage
        End Get
    End Property
    Public ReadOnly Property Surface As Graphics
        Get
            Surface = clsGraphicsBuffer
        End Get
    End Property
    Public ReadOnly Property FileName As String
        Get
            FileName = vbNullString
            If bmpFileName <> vbNullString Then FileName = bmpFileName
        End Get
    End Property
    Public Sub Clear()
        Clear(Color.White)
    End Sub
    Public Sub Clear(ByVal clrColor As Color, Optional ByVal transparentFill As Boolean = False)
        Dim fillPen As Pen
        fillPen = New Pen(clrColor)
        clsGraphicsBuffer.FillRectangle(New SolidBrush(clrColor), New Rectangle(0, 0, clsBitmapImage.Width, clsBitmapImage.Height))
        If transparentFill = True Then FloodFill(Point(1), Color.FromArgb(0, 255, 255, 255))
    End Sub
    Public Sub Dispose()
        clsGraphicsBuffer.Dispose()
        clsBitmapImage.Dispose()
        doesExist = False
    End Sub

    ''' <summary>
    ''' PATH FINDING VARIABLES, ALGORITHM, AND FUNCTIONS START HERE
    ''' </summary>
    ''' <remarks></remarks>
    Private Enum NodeStatus
        Closed = 0
        Open = 1
        NotEvaluated = 2
    End Enum
    Private Structure Node
        Dim ParentNode As Integer 'Index of parent node
        Dim MoveCost As Integer 'Pulled from MovementCost() 
        Dim ScoreG As Integer 'Movement cost from start point to this node
        Dim ScoreH As Integer 'Best guess movement cost from this node to end point
        Dim ScoreF As Integer 'Sum of ScoreG and ScoreH (estimated total movement cost from start to end point)
        Dim Status As NodeStatus 'Determines if node is in the closed or opened list
    End Structure
    Private ANode() As Node

    Public Function FindPath(ByVal MovementCost() As Integer, ByVal StartNode As Integer, ByVal EndNode As Integer, Optional ByVal allowDiagonalMoves As Boolean = True) As Boolean

        FindPath = False 'If false, no path has been found or bad data has been passed to FindPath function

        'Error catching
        If TileCount <= 1 Then Exit Function 'no grid properties have been defined
        If StartNode < 1 Then Exit Function 'node outside the grid area
        If StartNode > TileCount Then Exit Function 'node outside the grid area
        If EndNode < 1 Then Exit Function 'node outside the grid area
        If EndNode > TileCount Then Exit Function 'node outside the grid area
        If UBound(MovementCost) <> TileCount Then Exit Function 'movement cost count doesn't match tile count

        'Create nodes based on grid dimensions and transfer data from MovementCost()
        ReDim ANode(TileCount)
        'Ignore ANode(0) to ensure nodes equal the grid dimensions exactly as used in the DynamicBitmap class (grid tiles start on 1, not 0)
        For i As Integer = 1 To TileCount
            ANode(i).MoveCost = MovementCost(i)
            ANode(i).Status = NodeStatus.NotEvaluated
        Next

        'Reset solution path in case it was previously populated
        ReDim Path(0)
        Path(0) = 0

        'Add the start node to the open list
        ANode(StartNode).Status = NodeStatus.Open

        'Determine the F score for the start node
        'The ComputeScore function "by references" the Node, so values are changed without having to use the '=' sign
        ComputeScore(StartNode, EndNode, allowDiagonalMoves)


        'Determine which neighbors to view (vertical/horizontal only or include diagonal neighbors)
        'Values relate directly to "Direction" enum (See "Public Enum Direction" at top of DymamicBitmap class for more info)
        Dim neighborCnt As Integer = 3 'Directions 0 to 3
        If allowDiagonalMoves = True Then neighborCnt = 7 'Directions 0 to 7

        'Initalize variables
        Dim thisNode As Integer 'The current node we are working with
        Dim thisNeighbor As Integer 'The current neighbor of current node we are working with


        'Begin the main A* pathfinding operations
        Do
            'Get the node with the lowest F score
            thisNode = LowestF()

            'If thisNode = 0, then there are no more open nodes (no path found)
            If thisNode = 0 Then Exit Function

            'Move thisNode to the closed list 
            ANode(thisNode).Status = NodeStatus.Closed

            'Check to see if thisNode is the target node.  If it is, exit the loop (path found)
            If thisNode = EndNode Then Exit Do

            'Go through each neighbor of 'thisNode'
            For aNeighbor = 0 To neighborCnt
                thisNeighbor = TileNeighbor(thisNode, aNeighbor)

                'If thisNeighbor = 0, then it's outside the grid
                'If the movement cost of thisNeighbor = 0, then it's a wall or other obstacle that can't be passed over
                'If thisNeighbor is on the closed list, ignore it
                If thisNeighbor <> 0 And ANode(thisNeighbor).MoveCost <> 0 And ANode(thisNeighbor).Status <> NodeStatus.Closed Then


                    'If this neighbor is already on the open list, check to see if this route would be better
                    'In other words, check to see if it's G score would be lower using thisNode as it's parent
                    If ANode(thisNeighbor).Status = NodeStatus.Open Then

                        'Temporary save the data from thisNeighbor so we can evaluate it against thisNode as it's parent
                        Dim tempNode As Node
                        tempNode = ANode(thisNeighbor)

                        'Calcuate the new scores based on thisNode as it's parent
                        ANode(thisNeighbor).ParentNode = thisNode
                        ComputeScore(thisNeighbor, EndNode, allowDiagonalMoves)

                        'Compare the two G values
                        'If the new G score is higher than the original G score (longer route), restore thisNeighbor's original properties from tempNode
                        'If the new G score is lower (faster route) then leave the changes that were made in the previous step
                        If ANode(thisNeighbor).ScoreG >= tempNode.ScoreG Then ANode(thisNeighbor) = tempNode

                    Else
                        'This neighbor has not yet been examined by any other node 
                        'This is a possible route; add this neighbor to the open list, set thisNode as it's parent, and calculate F score
                        ANode(thisNeighbor).Status = NodeStatus.Open
                        ANode(thisNeighbor).ParentNode = thisNode
                        ComputeScore(thisNeighbor, EndNode, allowDiagonalMoves)

                    End If

                End If
            Next
        Loop


        'PATH HAS BEEN FOUND


        'Work backwards from the end node to determine our path and populate it into a temporary array
        Dim ReversePath() As Integer
        Dim Xcounter As Integer = 0
        thisNode = EndNode
        Do
            'Count each node and populate it into a temporary array
            Xcounter = Xcounter + 1
            ReDim Preserve ReversePath(Xcounter - 1)
            ReversePath(Xcounter - 1) = thisNode

            'If we've reached the start node, exit loop
            If thisNode = StartNode Then Exit Do

            'Set the next 'thisNode' to the parent of the node we are on (work backwards until the start node is reached)
            thisNode = ANode(thisNode).ParentNode
        Loop


        'Reverse the order of our path so that Path(0) = startNode and Ubound(Path) = endNode
        ReDim Path(Xcounter - 1)
        For theNextNode As Integer = 0 To Xcounter - 1
            Path(theNextNode) = ReversePath(UBound(ReversePath) - theNextNode)
        Next

        FindPath = True

    End Function
    Private Function LowestF() As Integer
        'Returns the node with the lowest F score on the Open list
        Dim chkScore As Integer
        Dim lowScore As Integer

        LowestF = 0

        'Set our lowScore large enough that the first node evaluated will become our default first low score
        lowScore = 32767


        'Evaluate each of the nodes
        For iNode As Integer = 1 To TileCount
            'Compare open list nodes with the current lowest score 
            If ANode(iNode).Status = NodeStatus.Open Then

                'If the current nodes F score is lower, change it to the current lowest node
                chkScore = ANode(iNode).ScoreF
                If chkScore <= lowScore Then
                    LowestF = iNode
                    lowScore = chkScore
                End If
            End If
        Next

        'If LowestF = 0, then there were no open nodes
    End Function
    Private Sub ComputeScore(ByRef computeNode As Integer, ByVal ENode As Integer, ByVal ad As Boolean)
        'Determines the G, H, and F values of the argument node

        'DETERMINE G VALUE
        ANode(computeNode).ScoreG = 0
        'If the node has no parent, value G is 0
        If ANode(computeNode).ParentNode <> 0 Then

            'Set the G value of the node equal to it's parents G value
            ANode(computeNode).ScoreG = ANode(ANode(computeNode).ParentNode).ScoreG


            'If the parent is on the same row or column, increase G value
            'by 10 * the movement cost of this node
            If TileRow(ANode(computeNode).ParentNode) = TileRow(computeNode) Or _
                    TileColumn(ANode(computeNode).ParentNode) = TileColumn(computeNode) Then

                ANode(computeNode).ScoreG = ANode(computeNode).ScoreG + (10 * ANode(computeNode).MoveCost)

            Else

                'If the parent isn't on the same row or column, it is diagonal from this node;
                'increase G value by 14 * the movement cost of this node if diagonal moves are allowed
                If ad = True Then ANode(computeNode).ScoreG = ANode(computeNode).ScoreG + (14 * ANode(computeNode).MoveCost)

                'If diagonal moves are not allowed, this movement will take both a horizontal and vertical move to reach (x20)
                If ad = False Then ANode(computeNode).ScoreG = ANode(computeNode).ScoreG + (20 * ANode(computeNode).MoveCost)

            End If

        End If


        'DETERMINE H VALUE
        ANode(computeNode).ScoreH = 0

        Dim tmpRowDist As Integer
        Dim tmpColDist As Integer

        'Get the row and column distance from this node to the end node
        tmpRowDist = Math.Abs(TileRow(computeNode) - TileRow(ENode))
        tmpColDist = Math.Abs(TileColumn(computeNode) - TileColumn(ENode))

        'Compute H value based on estimated distance to end node
        If tmpRowDist < tmpColDist Then
            ANode(computeNode).ScoreH = (4 * tmpRowDist) + (10 * tmpColDist)
        Else
            ANode(computeNode).ScoreH = (10 * tmpRowDist) + (4 * tmpColDist)
        End If

        'DETERMINE F VALUE
        'Add the distance from the start node (G)
        'to the estimated distance to the end node (H)
        ANode(computeNode).ScoreF = ANode(computeNode).ScoreG + ANode(computeNode).ScoreH
    End Sub
    Public Function TileRow(ByVal TileIndex As Integer) As Integer
        'Returns the row this tile is located on
        TileRow = Math.Truncate((TileIndex - 1) / columnCnt) + 1
    End Function
    Public Function TileColumn(ByVal TileIndex As Integer) As Integer
        'Returns the column this tile is located on
        TileColumn = ((TileIndex - 1) Mod columnCnt) + 1
        If TileIndex <= columnCnt Then TileColumn = TileIndex
    End Function




End Class

