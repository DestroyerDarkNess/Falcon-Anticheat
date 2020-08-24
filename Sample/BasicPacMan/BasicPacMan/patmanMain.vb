Imports BasicPacMan.DynamicBitmap
Imports BasicPacMan.FPS

Public Class patmanMain

    Public Structure CharacterData
        Dim IsPatman As Boolean
        Dim CurrentDirection As Direction
        Dim Speed As Integer
        Dim Sequence As Integer '0-4
        Dim Location As Point
        Dim AITarget As Integer
        Dim AIMemory As Integer
    End Structure
    Public Patman As CharacterData
    Public Ghost(3) As CharacterData
    Dim GhostsCanDie As Boolean
    Dim endMessage As String

    Private Sub patmanMain_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        gameRunning = False
        SequenceTimer.Enabled = False
        SpriteImage.Dispose()
        grid.Dispose()
        ScreenBuffer.Dispose()
        End
    End Sub
    Private Sub patmanMain_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles Me.KeyDown

        'IF THE GAME HAS NOT STARTED YET... START THE GAME!
        If gameRunning = False Then
            gameRunning = True
            SequenceTimer.Enabled = True
        End If

        'Change the parameters of Patman to move in the new direction
        Dim thisTile As Integer
        Dim moveToTile As Integer

        'Patman's current location
        thisTile = grid.Tile(Patman.Location)


        Select Case e.KeyValue
            Case Keys.Up
                'Next tile Patman will move to
                moveToTile = grid.TileNeighbor(thisTile, DynamicBitmap.Direction.North)
                'If outside grid area, don't move
                If moveToTile = 0 Then Exit Sub
                'If a wall tile, don't move
                If gridMovement(moveToTile) <> 1 Then Exit Sub
                'Change direction
                Patman.CurrentDirection = DynamicBitmap.Direction.North
            Case Keys.Down
                moveToTile = grid.TileNeighbor(thisTile, DynamicBitmap.Direction.South)
                If moveToTile = 0 Then Exit Sub
                If gridMovement(moveToTile) <> 1 Then Exit Sub
                Patman.CurrentDirection = DynamicBitmap.Direction.South
            Case Keys.Left
                moveToTile = grid.TileNeighbor(thisTile, DynamicBitmap.Direction.West)
                If moveToTile = 0 Then Exit Sub
                If gridMovement(moveToTile) <> 1 Then Exit Sub
                Patman.CurrentDirection = DynamicBitmap.Direction.West
            Case Keys.Right
                moveToTile = grid.TileNeighbor(thisTile, DynamicBitmap.Direction.East)
                If moveToTile = 0 Then Exit Sub
                If gridMovement(moveToTile) <> 1 Then Exit Sub
                Patman.CurrentDirection = DynamicBitmap.Direction.East
        End Select
    End Sub




    Private Sub patmanMain_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        'MsgBox("Debug Mode!")
        StartFalcon()
        Timer1.Enabled = True
        ResetGame()
        DrawRemainingPellets()
        DrawCharacters()
        UpdateStatsBar()

        Me.Show()

        DrawScreen()
        My.Computer.Audio.Play(My.Resources.sndTheme, AudioPlayMode.Background)
        Main()

    End Sub
    Public Sub Main()

        'User input comes from the KeyDown subroutine and AI moves with the SequenceTimer

        Do
            Application.DoEvents()

            If gameRunning Then

                'Restore the background grid 
                grid.ImageRestore()
                'Check for collisions based on movement made by player
                CollisionCheck()
                'Check to see if game is over
                If EndOfGameCheck() = True Then Exit Do
                'Draw the yellow pellets still on the grid 
                DrawRemainingPellets()
                'Draw the characters (Patman and Ghosts)
                DrawCharacters()
                'Update the lives and score data
                UpdateStatsBar()
                'Draw the screen 
                DrawScreen()

            End If
        Loop

        'Game is over
        grid.Text(endMessage, grid.Rectangle, New Font("Ms Sans Serif", 48, FontStyle.Bold, GraphicsUnit.Pixel), New SolidBrush(Color.Red))
        DrawScreen()
        My.Computer.Audio.Play(My.Resources.sndTheme, AudioPlayMode.Background)
        Pause()
        Pause()
        Pause()

        ResetGame()
        DrawRemainingPellets()
        DrawCharacters()
        UpdateStatsBar()
        DrawScreen()
        Main()


    End Sub




    Public Sub ResetGame()

        'Setup form
        Me.Width = 975
        Me.Height = 833


        'Setup grid
        grid.CreateGrid(32, 32, 23, 29)
        grid.Clear(Color.Black)
        If DEBUG_MODE = True Then grid.DrawTileLines(Color.Blue)

        For i As Integer = 1 To grid.TileCount
            If DEBUG_MODE = True Then grid.Text(i, grid.Rectangle(i), New Font("Small Fonts", 7), New SolidBrush(Color.White))
        Next

        'Create StatBar
        StatBar.CreateBitmap(grid.Bitmap.Width, 15)

        'Create Screen Buffer
        ScreenBuffer.CreateBitmap(grid.Bitmap.Width, grid.Bitmap.Height + 15)

        'Load Sprite Bitmap
        SpriteImage.CreateBitmap(My.Resources.patman.Width, My.Resources.patman.Height)
        SpriteImage.DrawOnSurface(My.Resources.patman, New Rectangle(New Point(0, 0), New Size(My.Resources.patman.Width, My.Resources.patman.Height)), SpriteImage.Rectangle)
        SpriteImage.MakeTransparent()
        SpriteImage.TileWidth = 32
        SpriteImage.TileHeight = 32

        'Set variables based on constant values
        LivesRemaining = STARTING_LIVES
        PlayerScore = 0

        'Initialize game arrays
        ReDim gridMovement(grid.TileCount)
        ReDim gridPellets(grid.TileCount)


        'Reset movement and pellets for game
        For i As Integer = 1 To grid.TileCount
            gridMovement(i) = 1
            gridPellets(i) = TileContents.Pellete
        Next

        'Create the walls (subroutine in PatmanMod.vb)
        CreateWalls()

        'Draw walls on the grid
        For i As Integer = 1 To grid.TileCount
            If gridMovement(i) = 0 Then grid.FillTile(i, Color.FromArgb(0, 0, 150))
            If i = 304 Or i = 305 Or i = 306 Then
                gridMovement(i) = 2
                grid.FillTile(i, Color.Blue)
            End If
        Next

        'Add the super pellets to the game
        gridPellets(89) = TileContents.SuperPellete
        gridPellets(115) = TileContents.SuperPellete
        gridPellets(495) = TileContents.SuperPellete
        gridPellets(521) = TileContents.SuperPellete

        'Remove the pellets from all wall tiles
        For i As Integer = 1 To grid.TileCount
            If gridMovement(i) <> 1 Then gridPellets(i) = TileContents.None
        Next

        'Remove the pellets from the middle tiles (where the ghosts appear)
        For i As Integer = 331 To 337
            gridPellets(i) = TileContents.None
        Next

        'Remove the pellet from where patman begins
        gridPellets(392) = TileContents.None

        'At this point, the grid is built without any objects drawn onto it.
        'Capture this image so we can restore it later as things move around and change on the grid.
        grid.ImageStore()

        'Set the character positions to default in their respective variables
        ResetCharacterLocations()

    End Sub
    Public Sub ResetCharacterLocations()
        Patman.CurrentDirection = DynamicBitmap.Direction.West
        Patman.Speed = PATMAN_SPEED
        Patman.Location = grid.Point(392)
        Patman.IsPatman = True

        For i As Integer = 0 To 3
            Ghost(i).Speed = GHOST_SPEED
            Ghost(i).CurrentDirection = DynamicBitmap.Direction.North
        Next
        Ghost(0).Location = grid.Point(331)
        Ghost(1).Location = grid.Point(333)
        Ghost(2).Location = grid.Point(335)
        Ghost(3).Location = grid.Point(337)
    End Sub
    Public Sub DrawRemainingPellets()
        'Draw pellets in all open tiles that haven't been eaten by Patman
        For i As Integer = 1 To grid.TileCount

            If gridPellets(i) = TileContents.Pellete Then
                If DEBUG_MODE = False Then grid.Surface.FillEllipse(New SolidBrush(Color.Yellow), New Rectangle(New Point(grid.Point(i).X + 12, grid.Point(i).Y + 12), New Size(grid.TileWidth - 24, grid.TileHeight - 24)))
            End If

            'Draw the super pellets larger
            If gridPellets(i) = TileContents.SuperPellete Then
                If DEBUG_MODE = False Then grid.Surface.FillEllipse(New SolidBrush(Color.Yellow), New Rectangle(New Point(grid.Point(i).X + 8, grid.Point(i).Y + 8), New Size(grid.TileWidth - 16, grid.TileHeight - 16)))
            End If
        Next
    End Sub
    Public Sub MoveCharacter(ByRef MoveChr As CharacterData)

        Dim startTile As Integer
        Dim thisTile As Integer
        Dim nextTile As Integer

        startTile = grid.Tile(MoveChr.Location)
        nextTile = grid.TileNeighbor(startTile, MoveChr.CurrentDirection)

        'If there is no natural obstacle or outside grid area, increase sequence number by 1
        MoveChr.Sequence = MoveChr.Sequence + 1
        If MoveChr.Sequence = 4 Then MoveChr.Sequence = 0

        'Based on the direction, adjust the location of the character
        Select Case MoveChr.CurrentDirection
            Case Direction.North
                'Adjust the position of the character
                MoveChr.Location.Y = MoveChr.Location.Y - 8
                'Get the new tile location
                thisTile = grid.Tile(MoveChr.Location)
                'Adjustment to center in the path
                MoveChr.Location.X = grid.Point(thisTile).X


            Case Direction.South
                'Adjust the position of the character
                MoveChr.Location.Y = MoveChr.Location.Y + 8
                'Get the new tile location
                thisTile = grid.Tile(MoveChr.Location)
                'If the character has moved to the next tile and the next tile is invalid or a wall, stay in same spot
                If (thisTile = nextTile) And (nextTile = 0) Or (gridMovement(nextTile) <> 1) Then
                    MoveChr.Location.Y = MoveChr.Location.Y - 8
                End If
                'Adjustment to center in the path
                MoveChr.Location.X = grid.Point(thisTile).X


            Case Direction.East
                'Adjust the position of the character
                If startTile = 348 And MoveChr.Location = grid.Point(348) Then
                    'Adjust to opposite end of the board if in the middle far right tile (tunnel)
                    MoveChr.Location = grid.Point(320)
                    'Fixes hault bug
                    nextTile = grid.TileNeighbor(320, MoveChr.CurrentDirection)
                Else
                    MoveChr.Location.X = MoveChr.Location.X + 8
                End If
                'Get the new tile location
                thisTile = grid.Tile(MoveChr.Location)
                'If the character has moved to the next tile and the next tile is invalid or a wall, stay in same spot
                If (thisTile = nextTile) And (nextTile = 0) Or (gridMovement(nextTile) <> 1) Then
                    MoveChr.Location.X = MoveChr.Location.X - 8
                End If
                'Adjustment to center in the path
                MoveChr.Location.Y = grid.Point(thisTile).Y


            Case Direction.West
                'Adjust the position of the character
                If startTile = 320 And MoveChr.Location = grid.Point(320) Then
                    'Adjust to opposite end of the board if in the middle far left tile (tunnel)
                    MoveChr.Location = grid.Point(348)
                Else
                    MoveChr.Location.X = MoveChr.Location.X - 8
                End If
                'Get the new tile location
                thisTile = grid.Tile(MoveChr.Location)
                'Adjustment to center in the path
                MoveChr.Location.Y = grid.Point(thisTile).Y


        End Select

        'Do not allow this character to move into the next tile (north or left) if it's bad or a wall
        If gridMovement(nextTile) = 2 Then
            'Allow ghosts to move through door leading out the middle room (but prevent Patman)
        ElseIf nextTile = 0 Or gridMovement(nextTile) <> 1 Then
            'If the character has left the start tile, move it back
            If thisTile = nextTile Then
                MoveChr.Location = grid.Point(startTile)
            End If
        End If


        'If over a pellete.. eat it!
        If MoveChr.Location = grid.Point(thisTile) And MoveChr.IsPatman = True Then
            If gridPellets(thisTile) = TileContents.Pellete Then
                PlayerScore += EAT_PELLET_POINTS
                My.Computer.Audio.Play(My.Resources.sndChomp, AudioPlayMode.Background)
            End If
            If gridPellets(thisTile) = TileContents.SuperPellete Then
                PlayerScore += EAT_SUPERPELLET_POINTS
                'Flash screen to signal it's okay to kill ghosts 
                My.Computer.Audio.Play(My.Resources.sndKillMode, AudioPlayMode.Background)
                GhostsCanDie = True
            End If

            gridPellets(thisTile) = TileContents.None
        End If

    End Sub
    Private Sub CollisionCheck()

        'Check for collision with ghosts (ghosts travel through each other - kill or get eaten by Patman)
        'Go through each ghost to see if it's collided with Patman
        Dim rect1 As Rectangle
        Dim rect2 As Rectangle
        For i As Integer = 0 To 3
            'This is a very basic collision detection - it could be improved later
            'Decreased dimensions to get a tighter frame of the characters 
            rect1 = New Rectangle(Ghost(i).Location.X + 10, Ghost(i).Location.Y - 10, 12, 12)
            rect2 = New Rectangle(Patman.Location.X + 10, Patman.Location.Y - 10, 12, 12)

            If rect1.IntersectsWith(rect2) = True Then
                'If enemy is blinking, kill it.
                If GhostsCanDie = True Then
                    'Return ghost to center box
                    Ghost(i).Location = grid.Point(331)
                    PlayerScore += EAT_GHOST_POINTS
                    My.Computer.Audio.Play(My.Resources.sndGhostDies, AudioPlayMode.Background)
                Else
                    'Player dies
                    My.Computer.Audio.Play(My.Resources.sndPatmanDies, AudioPlayMode.Background)
                    Pause()
                    PlayerScore -= DIE_LOST_POINTS
                    LivesRemaining -= 1
                    ResetCharacterLocations()
                    gameRunning = False
                    SequenceTimer.Enabled = False

                End If
            End If
        Next
    End Sub
    Public Function EndOfGameCheck() As Boolean
        EndOfGameCheck = False

        'Check to see if player has lost
        If LivesRemaining = 0 Then
            EndOfGameCheck = True
            endMessage = "SORRY, YOU HAVE LOST! YOUR SCORE: " & PlayerScore
        End If

        Dim aPelleteFound As Boolean
        aPelleteFound = False
        'Check to see if player has won
        For aPel As Integer = 1 To grid.TileCount
            If gridPellets(aPel) <> TileContents.None Then
                aPelleteFound = True
                Exit For
            End If
        Next

        If aPelleteFound = False Then
            EndOfGameCheck = True
            endMessage = "CONGRATULATIONS!  YOU HAVE WON. GG! FINAL SCORE: " & PlayerScore
        End If
    End Function
    Public Sub DrawCharacters()

        Dim tmpSprite As New DynamicBitmap
        tmpSprite.CreateBitmap(32, 32)
        tmpSprite.ImageStore()

        'Draw Patman

        'Get correct sprite image based on current location and direction
        tmpSprite.DrawOnSurface(SpriteImage.Bitmap, SpriteImage.Rectangle(SpriteImage.Tile((Patman.CurrentDirection * 128) + (Patman.Sequence * 32), 0)), tmpSprite.Rectangle)

        'Draw patman at his current location
        grid.DrawOnSurface(tmpSprite.Bitmap, tmpSprite.Rectangle, New Rectangle(Patman.Location, New Size(tmpSprite.Bitmap.Width, tmpSprite.Bitmap.Height)))


        'Draw Ghosts

        Static switch As Boolean
        switch = Not switch

        For i As Integer = 0 To 3
            tmpSprite.ImageRestore()

            'Get correct sprite image based on current location and direction
            tmpSprite.DrawOnSurface(SpriteImage.Bitmap, SpriteImage.Rectangle(SpriteImage.Tile((Ghost(i).CurrentDirection * 128) + (Ghost(i).Sequence * 32), (i + 1) * 32)), tmpSprite.Rectangle)

            'If ghost can be eaten, draw with effects
            If GhostsCanDie = True And switch = True Then tmpSprite.AlphaBlend()

            'Draw ghost at its current location
            grid.DrawOnSurface(tmpSprite.Bitmap, tmpSprite.Rectangle, New Rectangle(Ghost(i).Location, New Size(tmpSprite.Bitmap.Width, tmpSprite.Bitmap.Height)))
        Next
    End Sub
    Public Sub UpdateStatsBar()
        StatBar.Clear(Color.Black)
        StatBar.Text(LivesRemaining & " LIVES REMAINING", New Rectangle(New Point(10, 0), New Size(150, 15)), New Font("Ms Sans Serif", 14, FontStyle.Bold, GraphicsUnit.Pixel), New SolidBrush(Color.Red))
        If gameRunning = False Then StatBar.Text("Click arrow keys to begin", New Rectangle(New Point(335, 0), New Size(250, 15)), New Font("Ms Sans Serif", 12, FontStyle.Regular, GraphicsUnit.Pixel), New SolidBrush(Color.Red))
        StatBar.Text("SCORE: " & PlayerScore, New Rectangle(New Point(grid.Bitmap.Width - 150, 0), New Size(150, 15)), New Font("Ms Sans Serif", 15, FontStyle.Bold, GraphicsUnit.Pixel), New SolidBrush(Color.Red))
        If gameRunning = True Then StatBar.Text("The Framerate or FPS is " & FrameRate.CalculateFrameRate(), New Rectangle(New Point(355, 0), New Size(250, 15)), New Font("Ms Sans Serif", 12, FontStyle.Regular, GraphicsUnit.Pixel), New SolidBrush(Color.Red))
    End Sub
    Public Sub DrawScreen()
        Dim G As Graphics = Me.CreateGraphics
        ScreenBuffer.DrawOnSurface(StatBar.Bitmap, StatBar.Rectangle, ScreenBuffer.Rectangle, False)
        ScreenBuffer.DrawOnSurface(grid.Bitmap, grid.Rectangle, New Rectangle(New Point(0, 15), New Size(grid.Bitmap.Width, grid.Bitmap.Height)))
        G.DrawImage(ScreenBuffer.Bitmap, New Point(15, 15))
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Me.Text = "The PacMan Game | Falcon Anticheat | @FPS = " & FPS.FrameRate.CalculateFrameRate()
    End Sub

    Private Sub patmanMain_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles Me.Paint
        DrawScreen()
    End Sub
    Private Sub patmanMain_ResizeEnd(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.ResizeEnd
        Me.Width = 975
        Me.Height = 833
    End Sub
    Private Sub SequenceTimer_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SequenceTimer.Tick
        Static gameTicker As Long
        Static currentTick As Long

        gameTicker = gameTicker + 1

        If gameTicker Mod Patman.Speed = 0 Then
            MoveCharacter(Patman)
        End If

        For t As Integer = 0 To 3
            If gameTicker Mod Ghost(t).Speed = 0 Then
                DoAI(t)
                MoveCharacter(Ghost(t))
            End If

        Next

        'Time limit for super pellets
        If GhostsCanDie = False Then
            currentTick = gameTicker
        Else
            If currentTick + (KILL_GHOST_TIMELIMIT - 25) = gameTicker Then My.Computer.Audio.Play(My.Resources.sndSiren, AudioPlayMode.Background)
            If currentTick + KILL_GHOST_TIMELIMIT = gameTicker Then GhostsCanDie = False
        End If

    End Sub

    Public Sub DoAI(ByVal whichGhost As Integer)
        'Based on the current ghost, execute his strategy
        Dim ghostTile As Integer
        Dim patmanTile As Integer
        Dim goodPath As Boolean

        ghostTile = grid.Tile(Ghost(whichGhost).Location)
        patmanTile = grid.Tile(Patman.Location)

        If ghostTile = patmanTile Then Exit Sub

        Select Case whichGhost
            Case 0
                'THE GO-GETTER!
                'The RED ghost will plan a route directly to Patman and charge after him

                goodPath = grid.FindPath(gridMovement, ghostTile, patmanTile, False)
                If goodPath = True Then
                    'Determine direction to path(1)
                    If UBound(grid.Path) < 1 Then Exit Sub
                    If grid.TileNeighbor(ghostTile, Direction.North) = grid.Path(1) Then Ghost(0).CurrentDirection = Direction.North
                    If grid.TileNeighbor(ghostTile, Direction.South) = grid.Path(1) Then Ghost(0).CurrentDirection = Direction.South
                    If grid.TileNeighbor(ghostTile, Direction.East) = grid.Path(1) Then Ghost(0).CurrentDirection = Direction.East
                    If grid.TileNeighbor(ghostTile, Direction.West) = grid.Path(1) Then Ghost(0).CurrentDirection = Direction.West
                End If
            Case 1
                'THE PATROLLER!
                'The green ghost will patrol to each of the super pellets starting with the bottom left
                If Ghost(1).AITarget = 0 Then Ghost(1).AITarget = 495

                'If green ghost has arrived at this super pellet, go to the next one
                If Ghost(1).AITarget = ghostTile Then
                    Select Case Ghost(1).AITarget
                        Case 89 : Ghost(1).AITarget = 495
                        Case 115 : Ghost(1).AITarget = 89
                        Case 495 : Ghost(1).AITarget = 521
                        Case 521 : Ghost(1).AITarget = 115
                    End Select
                End If

                goodPath = grid.FindPath(gridMovement, ghostTile, Ghost(1).AITarget, False)
                If goodPath = True Then
                    'Determine direction to path(1)
                    If UBound(grid.Path) < 1 Then Exit Sub
                    If grid.TileNeighbor(ghostTile, Direction.North) = grid.Path(1) Then Ghost(1).CurrentDirection = Direction.North
                    If grid.TileNeighbor(ghostTile, Direction.South) = grid.Path(1) Then Ghost(1).CurrentDirection = Direction.South
                    If grid.TileNeighbor(ghostTile, Direction.East) = grid.Path(1) Then Ghost(1).CurrentDirection = Direction.East
                    If grid.TileNeighbor(ghostTile, Direction.West) = grid.Path(1) Then Ghost(1).CurrentDirection = Direction.West
                End If
            Case 2
                'THE ANTICIPATOR!
                'The purple ghost will try to guess where patman is heading and go there

                'Define the game grid into four quadrants
                Dim quad(3) As Rectangle
                quad(0) = New Rectangle(New Point(0, 0), New Size(grid.Bitmap.Width / 2, grid.Bitmap.Height / 2)) 'top left quadrant 
                quad(1) = New Rectangle(New Point(grid.Bitmap.Width / 2, 0), New Size(grid.Bitmap.Width / 2, grid.Bitmap.Height / 2)) 'top right quadrant 
                quad(2) = New Rectangle(New Point(0, grid.Bitmap.Height / 2), New Size(grid.Bitmap.Width / 2, grid.Bitmap.Height / 2)) 'bottom left quadrant 
                quad(3) = New Rectangle(New Point(grid.Bitmap.Width / 2, grid.Bitmap.Height / 2), New Size(grid.Bitmap.Width / 2, grid.Bitmap.Height / 2)) 'bottom right quadrant 

                'Figure out which quadrant patman is in
                Dim patmanRect As Rectangle = New Rectangle(Patman.Location, New Size(32, 32))
                Dim thisQuad As Integer
                For tQ As Integer = 0 To 3
                    If quad(tQ).IntersectsWith(patmanRect) Then thisQuad = tQ
                Next

                'Based on patman's direction of movement and current quadrant, pick a grid to travel to
                Select Case Patman.CurrentDirection
                    Case Direction.North
                        If thisQuad = 0 Then Ghost(2).AITarget = 31
                        If thisQuad = 1 Then Ghost(2).AITarget = 57
                        If thisQuad = 2 Then Ghost(2).AITarget = 153
                        If thisQuad = 3 Then Ghost(2).AITarget = 167
                    Case Direction.South
                        If thisQuad = 0 Then Ghost(2).AITarget = 501
                        If thisQuad = 1 Then Ghost(2).AITarget = 515
                        If thisQuad = 2 Then Ghost(2).AITarget = 611
                        If thisQuad = 3 Then Ghost(2).AITarget = 637
                    Case Direction.East
                        If thisQuad = 0 Then Ghost(2).AITarget = 276
                        If thisQuad = 1 Then Ghost(2).AITarget = 57
                        If thisQuad = 2 Then Ghost(2).AITarget = 392
                        If thisQuad = 3 Then Ghost(2).AITarget = 637
                    Case Direction.West
                        If thisQuad = 0 Then Ghost(2).AITarget = 31
                        If thisQuad = 1 Then Ghost(2).AITarget = 153
                        If thisQuad = 2 Then Ghost(2).AITarget = 611
                        If thisQuad = 3 Then Ghost(2).AITarget = 501
                End Select


                'If ghost reaches target, save it in memory
                If Ghost(2).AITarget = ghostTile Then Ghost(2).AIMemory = ghostTile

                'If current target is the same as memory tile, change target to patman
                If Ghost(2).AIMemory = Ghost(2).AITarget Then
                    Ghost(2).AITarget = patmanTile
                End If


                'Get path to objective tile 
                goodPath = grid.FindPath(gridMovement, ghostTile, Ghost(2).AITarget, False)
                If goodPath = True Then
                    'Determine direction to path(1)
                    If UBound(grid.Path) < 1 Then Exit Sub
                    If grid.TileNeighbor(ghostTile, Direction.North) = grid.Path(1) Then Ghost(2).CurrentDirection = Direction.North
                    If grid.TileNeighbor(ghostTile, Direction.South) = grid.Path(1) Then Ghost(2).CurrentDirection = Direction.South
                    If grid.TileNeighbor(ghostTile, Direction.East) = grid.Path(1) Then Ghost(2).CurrentDirection = Direction.East
                    If grid.TileNeighbor(ghostTile, Direction.West) = grid.Path(1) Then Ghost(2).CurrentDirection = Direction.West
                End If

            Case 3
                'THE FOLLOWER!
                'The blue ghost will pick a random target between patman and the other ghosts and follow that target for a period of time

                Dim aRnd As New Random
                Dim aTarget, targetLocation As Integer

                'Use AIMemory to track time ghost is chasing target (time = timelimit to kill ghosts)

                If Ghost(3).AIMemory = 0 Then
                    'If time has expired, pick a new target and reset the time 
                    Randomize()
                    aTarget = aRnd.Next(4)
                    Ghost(3).AITarget = aTarget
                    Ghost(3).AIMemory = KILL_GHOST_TIMELIMIT
                Else
                    Ghost(3).AIMemory -= 1
                End If

                'Get targets location
                If Ghost(3).AITarget = 3 Then targetLocation = grid.Tile(Patman.Location)
                If Ghost(3).AITarget < 3 Then targetLocation = grid.Tile(Ghost(Ghost(3).AITarget).Location)

                'Get path to objective tile 
                goodPath = grid.FindPath(gridMovement, ghostTile, targetLocation, False)
                If goodPath = True Then
                    'Determine direction to path(1)
                    If UBound(grid.Path) < 1 Then Exit Sub
                    If grid.TileNeighbor(ghostTile, Direction.North) = grid.Path(1) Then Ghost(3).CurrentDirection = Direction.North
                    If grid.TileNeighbor(ghostTile, Direction.South) = grid.Path(1) Then Ghost(3).CurrentDirection = Direction.South
                    If grid.TileNeighbor(ghostTile, Direction.East) = grid.Path(1) Then Ghost(3).CurrentDirection = Direction.East
                    If grid.TileNeighbor(ghostTile, Direction.West) = grid.Path(1) Then Ghost(3).CurrentDirection = Direction.West
                End If

        End Select


    End Sub

    Public Sub Pause()
        Dim timeKeep As Long
        timeKeep = 0
        Do
            timeKeep += 1
            If timeKeep = 270000000 Then Exit Do
        Loop
    End Sub

#Region " Anticheat "

    Dim AntiCheatClient As New ShareNet.AntiCheat.MainProc
    Dim DetectEx As Boolean = False
    Dim FAC As New ShareNet.Core.FalconWrapper
    Private Sub StartFalcon()

        AntiCheatClient.Rare_Character_scanning = False ' Detecta caracteres chinos y simbolos, Dejarlo en False, no esta terminado al 100% , esta bug ;.;
        AntiCheatClient.Enable_AntiDump = False  ' Poner en TRUE si quieres activar el AntiDump
        AntiCheatClient.Antidll_injection = False ' Tener q probar si no se bugea el juego con esta funcion. 
        AntiCheatClient.CreateDevice()
        AntiCheatClient.HashScanner()
        AnticheatMonitor.Enabled = True
    End Sub

    Private Sub AnticheatMonitor_Tick(sender As Object, e As EventArgs) Handles AnticheatMonitor.Tick
        If DetectEx = False Then
            If AntiCheatClient.DetectionState = ShareNet.AntiCheat.MainProc.ResultType.Danger Then
                DetectEx = True
                EndApp()
                AnticheatMonitor.Enabled = False
            End If
        End If
     End Sub

    Private Sub EndApp()
        MsgBox("Description: " & AntiCheatClient.DetectDescription & vbNewLine & _
               "Log : " & AntiCheatClient.LogResult)
        End
    End Sub

#End Region

  

End Class
