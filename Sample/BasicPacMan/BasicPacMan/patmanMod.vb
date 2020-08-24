Module PatmanMod

    'Change as desired
    Public Const EAT_GHOST_POINTS As Integer = 25
    Public Const EAT_PELLET_POINTS As Integer = 3
    Public Const EAT_SUPERPELLET_POINTS As Integer = 7
    Public Const DIE_LOST_POINTS As Integer = 12
    Public Const STARTING_LIVES As Integer = 7
    Public Const GHOST_SPEED As Integer = 2 'Lower is faster 
    Public Const PATMAN_SPEED As Integer = 1 'Lower is faster 
    Public Const KILL_GHOST_TIMELIMIT As Integer = 120 'Larger is longer time

    Public Const DEBUG_MODE As Boolean = False

    Public LivesRemaining As Integer
    Public PlayerScore As Integer

    Public gameRunning As Boolean

    Public Enum TileContents
        Pellete = 0
        SuperPellete = 1
        None = 2
    End Enum

    Public grid As New DynamicBitmap
    Public StatBar As New DynamicBitmap
    Public SpriteImage As New DynamicBitmap
    Public ScreenBuffer As New DynamicBitmap

    Public gridMovement() As Integer
    Public gridPellets() As TileContents

    ''' <summary>
    ''' CREATE WALLS SUBROUTINE
    ''' : Don't get too wrapped up around this subroutine.  All it really does is designate each tile that is a wall and gives it a movement cost of 0.
    ''' This can be accomplished much easier by just loading it from a file, but I was lazy and just decided to do it this way.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CreateWalls()
        Dim aTile As Integer

        'Future versions - load map from file


        'Top wall
        For aTile = 1 To 29
            gridMovement(aTile) = 0
        Next

        'Left wall
        For aTile = 30 To 639 Step 29
            gridMovement(aTile) = 0
        Next

        'Right wall
        For aTile = 58 To grid.TileCount Step 29
            gridMovement(aTile) = 0
        Next

        'Bottom wall
        For aTile = 640 To 666
            gridMovement(aTile) = 0
        Next

        'All left top squares
        For aTile = 61 To 65
            gridMovement(aTile) = 0
        Next
        For aTile = 67 To 71
            gridMovement(aTile) = 0
        Next
        For aTile = 90 To 94
            gridMovement(aTile) = 0
        Next
        For aTile = 96 To 100
            gridMovement(aTile) = 0
        Next
        For aTile = 119 To 123
            gridMovement(aTile) = 0
        Next
        For aTile = 125 To 129
            gridMovement(aTile) = 0
        Next
        For aTile = 177 To 181
            gridMovement(aTile) = 0
        Next
        For aTile = 206 To 210
            gridMovement(aTile) = 0
        Next

        'Top T block
        For aTile = 185 To 193
            gridMovement(aTile) = 0
        Next
        gridMovement(218) = 0
        gridMovement(247) = 0


        'Top left T block 
        gridMovement(183) = 0
        gridMovement(212) = 0
        For aTile = 241 To 245
            gridMovement(aTile) = 0
        Next
        gridMovement(270) = 0
        gridMovement(299) = 0

        'Top right T block
        For aTile = 249 To 253
            gridMovement(aTile) = 0
        Next
        gridMovement(195) = 0
        gridMovement(224) = 0
        gridMovement(282) = 0
        gridMovement(311) = 0

        'All right top squares
        For aTile = 75 To 79
            gridMovement(aTile) = 0
        Next
        For aTile = 81 To 85
            gridMovement(aTile) = 0
        Next
        For aTile = 104 To 108
            gridMovement(aTile) = 0
        Next
        For aTile = 110 To 114
            gridMovement(aTile) = 0
        Next
        For aTile = 133 To 137
            gridMovement(aTile) = 0
        Next
        For aTile = 139 To 143
            gridMovement(aTile) = 0
        Next
        For aTile = 197 To 201
            gridMovement(aTile) = 0
        Next
        For aTile = 226 To 230
            gridMovement(aTile) = 0
        Next

        'Middle box
        For aTile = 301 To 309
            gridMovement(aTile) = 0
        Next
        gridMovement(330) = 0
        gridMovement(359) = 0
        For aTile = 360 To 366
            gridMovement(aTile) = 0
        Next
        gridMovement(338) = 0
        gridMovement(367) = 0

        'Left and ride side blocks
        For aTile = 263 To 268
            gridMovement(aTile) = 0
        Next
        For aTile = 292 To 297
            gridMovement(aTile) = 0
        Next

        For aTile = 350 To 355
            gridMovement(aTile) = 0
        Next
        For aTile = 379 To 384
            gridMovement(aTile) = 0
        Next

        For aTile = 284 To 289
            gridMovement(aTile) = 0
        Next
        For aTile = 313 To 318
            gridMovement(aTile) = 0
        Next

        For aTile = 371 To 376
            gridMovement(aTile) = 0
        Next
        For aTile = 400 To 405
            gridMovement(aTile) = 0
        Next

        'Bottom two center T's
        For aTile = 417 To 425
            gridMovement(aTile) = 0
        Next
        gridMovement(450) = 0
        gridMovement(479) = 0

        For aTile = 533 To 541
            gridMovement(aTile) = 0
        Next
        gridMovement(566) = 0
        gridMovement(595) = 0

        'Bottom two row T's
        For aTile = 583 To 593
            gridMovement(aTile) = 0
        Next
        For aTile = 597 To 607
            gridMovement(aTile) = 0
        Next
        gridMovement(560) = 0
        gridMovement(531) = 0
        gridMovement(543) = 0
        gridMovement(572) = 0

        'Bottom straight bars
        For aTile = 473 To 477
            gridMovement(aTile) = 0
        Next
        For aTile = 481 To 485
            gridMovement(aTile) = 0
        Next

        'Bottom section blocks
        For aTile = 438 To 442
            gridMovement(aTile) = 0
        Next
        For aTile = 467 To 471
            gridMovement(aTile) = 0
        Next
        For aTile = 458 To 462
            gridMovement(aTile) = 0
        Next
        For aTile = 487 To 491
            gridMovement(aTile) = 0
        Next

        For aTile = 498 To 500
            gridMovement(aTile) = 0
        Next
        For aTile = 527 To 529
            gridMovement(aTile) = 0
        Next

        For aTile = 516 To 518
            gridMovement(aTile) = 0
        Next
        For aTile = 545 To 547
            gridMovement(aTile) = 0
        Next


        'All others
        gridMovement(44) = 0
        gridMovement(73) = 0
        gridMovement(102) = 0
        gridMovement(131) = 0
        gridMovement(357) = 0
        gridMovement(386) = 0
        gridMovement(415) = 0
        gridMovement(369) = 0
        gridMovement(398) = 0
        gridMovement(427) = 0
        gridMovement(524) = 0
        gridMovement(525) = 0
        gridMovement(549) = 0
        gridMovement(550) = 0

        'Open left and right offscreen paths
        gridMovement(320) = 1
        gridMovement(348) = 1

        'open up and down paths
        gridMovement(611) = 1
        gridMovement(637) = 1

    End Sub





End Module
