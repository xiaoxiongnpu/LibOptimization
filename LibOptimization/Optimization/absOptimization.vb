﻿Imports LibOptimization.Util
Imports LibOptimization.Util.Random

Namespace Optimization
    ''' <summary>
    ''' Abstarct optimization Class rev2
    ''' </summary>
    ''' <remarks></remarks>
    Public MustInherit Class absOptimization
#Region "Member"
        ''' <summary>Objective function</summary>
        Public Property ObjectiveFunction As absObjectiveFunction = Nothing

        ''' <summary>Random object</summary>
        Public Property Random As System.Random = New Xorshift(BitConverter.ToUInt32(BitConverter.GetBytes(Environment.TickCount + Util.Util.GlobalCounter()), 0))

        ''' <summary>Max iteration count(Default:2000)</summary>
        Public Property Iteration As Integer = 2000

        ''' <summary>Iteration count</summary>
        Protected Property _IterationCount As Integer = 0

        ''' <summary>Epsilon(Default:1e-8) for Criterion</summary>
        Public Property EPS As Double = 0.00000001

        ''' <summary>Population Size(Default:100)</summary>
        Public Property PopulationSize As Integer = 100

        ''' <summary>Range of initial value(This parameters to use when generate a variable)</summary>
        Public Property InitialValueRange As Double = 5

        ''' <summary>population</summary>
        Protected _populations As New List(Of Point)

        ''' <summary>criterion index</summary>
        Protected _criterionIndex As Integer = 0

        ''' <summary>Use criterion flag</summary>
        Public Property UseCriterion As Boolean = True

        ''' <summary>criterion ratio(Converge when the population has the same solution)</summary>
        Public Property CriterionRatio As Double = 0.7

        ''' <summary>Use adaptive population flag(population = 50*ln(variable)+15))</summary>
        Public Property UseAdaptivePopulationSize As Boolean = False

        ''' <summary>Use bounds flag</summary>
        Public Property UseBounds As Boolean = False

        ''' <summary>Upper bound(limit solution space)</summary>
        Public Property UpperBounds As Double() = Nothing

        ''' <summary>Lower bound(limit solution space)</summary>
        Public Property LowerBounds As Double() = Nothing
#End Region

#Region "Public"
        ''' <summary>
        ''' Default constructor
        ''' </summary>
        Public Sub New()
            'nop
        End Sub

        ''' <summary>
        ''' Initialize optimizer
        ''' </summary>
        ''' <returns></returns>
        Public Overridable Function Init(Optional ByVal anyPoint() As Double = Nothing, Optional ByVal isReuseBestResult As Boolean = False) As Boolean
            Try
                'check
                If ObjectiveFunction Is Nothing Then
                    ErrorManage.SetError(ErrorManage.ErrorType.ERR_INIT, "Not exist ObjectiveFunction")
                    Return False
                End If

                'Bound check
                If UseBounds = True Then
                    If UpperBounds Is Nothing OrElse LowerBounds Is Nothing Then
                        ErrorManage.SetError(ErrorManage.ErrorType.ERR_INIT, "Bounds setting error")
                        Return False
                    End If
                    If ObjectiveFunction.NumberOfVariable <> UpperBounds.Length Then
                        ErrorManage.SetError(ErrorManage.ErrorType.ERR_INIT, "Bounds setting error")
                        Return False
                    End If
                    If ObjectiveFunction.NumberOfVariable <> LowerBounds.Length Then
                        ErrorManage.SetError(ErrorManage.ErrorType.ERR_INIT, "Bounds setting error")
                        Return False
                    End If
                End If

                'reuse
                Dim best As Point = Nothing
                If isReuseBestResult = True AndAlso _populations.Count > 0 Then
                    best = Me.Result
                End If

                'init
                _IterationCount = 0
                _populations.Clear()

                'error manage reset
                ErrorManage.Clear()

                'adaptive population size
                If UseAdaptivePopulationSize = True Then
                    PopulationSize = CInt(50 * Math.Log(ObjectiveFunction.NumberOfVariable) + 15)
                End If

                'generate initial position
                Dim temp(ObjectiveFunction.NumberOfVariable - 1) As Double
                For i As Integer = 0 To PopulationSize - 1
                    For j As Integer = 0 To ObjectiveFunction.NumberOfVariable - 1
                        temp(j) = Util.Util.GenRandomRange(Random, -InitialValueRange, InitialValueRange)
                    Next

                    Dim tempPoint = New Point(New Point(ObjectiveFunction, temp))
                    If UseBounds = True Then
                        LimitSolutionSpace(tempPoint)
                    End If

                    _populations.Add(tempPoint)
                Next

                'calc criterion index
                If UseCriterion = True Then
                    _criterionIndex = CInt(PopulationSize * CriterionRatio)
                End If

                'reuse
                If isReuseBestResult = True AndAlso best IsNot Nothing Then
                    _populations(0) = best
                End If

                'set anypoint
                If anyPoint IsNot Nothing Then
                    _populations(0) = New Point(ObjectiveFunction, anyPoint)
                End If

                'Sort Evaluate
                _populations.Sort()
            Catch ex As Exception
                ErrorManage.SetError(ErrorManage.ErrorType.ERR_INIT, ex.Message)
            End Try

            Return True
        End Function

        '''' <summary>
        '''' Initialize optimizer with any point 
        '''' </summary>
        '''' <param name="anyPoint"></param>
        '''' <returns></returns>
        'Public Function Init(ByVal anyPoint() As Double) As Boolean
        '    If anyPoint Is Nothing Then
        '        ErrorManage.SetError(ErrorManage.ErrorType.ERR_INIT)
        '        Return False
        '    End If

        '    If ObjectiveFunction.NumberOfVariable <> anyPoint.Length Then
        '        ErrorManage.SetError(ErrorManage.ErrorType.ERR_INIT)
        '        Return False
        '    End If

        '    Dim flg As Boolean = Init()
        '    If flg = True Then
        '        _populations(0) = New Point(ObjectiveFunction, anyPoint) 'replace
        '    End If
        '    Return flg
        'End Function

        '''' <summary>
        '''' Initialize optimizer with best result (Initialize leave best result. This method Is an Elite strategy.)
        '''' </summary>
        '''' <param name="isReuseBestResult">Reuse best result</param>
        '''' <returns></returns>
        'Public Function Init(ByVal isReuseBestResult As Boolean) As Boolean
        '    'false
        '    If isReuseBestResult = False Then
        '        Return Init()
        '    End If

        '    'not init
        '    If _populations.Count = 0 Then
        '        Return Init()
        '    End If

        '    'reuse
        '    Dim best = Util.Util.GetBestPoint(_populations)
        '    Dim flg = Init(False)
        '    If flg = True Then
        '        _populations(0) = best
        '    End If
        '    Return flg
        'End Function

        ''' <summary>
        ''' Do Iteration
        ''' </summary>
        ''' <param name="ai_iteration">Iteration count. When you set zero, use the default value.</param>
        ''' <returns>true:Stopping Criterion or Iteration count has been reached. false:Do not Stopping Criterion or remain iteration count.</returns>
        ''' <remarks></remarks>
        Public MustOverride Function DoIteration(Optional ByVal ai_iteration As Integer = 0) As Boolean

        ''' <summary>
        ''' Result
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable ReadOnly Property Result As Point
            Get
                Return Util.Util.GetBestPoint(_populations, True)
            End Get
        End Property

        ''' <summary>
        ''' All Sorted Results
        ''' </summary>
        ''' <returns></returns>
        Public Overridable ReadOnly Property Results As List(Of Point)
            Get
                Return Util.Util.GetSortedResultsByEval(_populations)
            End Get
        End Property

        ''' <summary>
        ''' Iteration count 
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property IterationCount() As Integer
            Get
                Return _IterationCount
            End Get
        End Property

        ''' <summary>
        ''' Reset Iteration count
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub ResetIterationCount()
            _IterationCount = 0
        End Sub

        ''' <summary>
        ''' Criterion/Conversion check
        ''' </summary>
        ''' <returns></returns>
        Public Function IsCriterion() As Boolean
            If UseCriterion = False Then
                Return False
            End If

            'higher N percentage particles are finished at the time of same evaluate value.
            If Util.Util.IsCriterion(EPS, _populations(0).Eval, _populations(_criterionIndex).Eval, EPS) Then
                Return True
            Else
                Return False
            End If
        End Function

        ''' <summary>
        ''' Limit solution space
        ''' </summary>
        ''' <param name="temp"></param>
        ''' <remarks></remarks>
        Protected Sub LimitSolutionSpace(ByRef temp As MathUtil.EasyVector)
            If UseBounds = False Then
                Return
            End If

            If UpperBounds IsNot Nothing AndAlso LowerBounds IsNot Nothing Then
                For ii As Integer = 0 To temp.Count - 1
                    Dim upper As Double = 0
                    Dim lower As Double = 0
                    If UpperBounds(ii) > LowerBounds(ii) Then
                        upper = UpperBounds(ii)
                        lower = LowerBounds(ii)
                    ElseIf UpperBounds(ii) < LowerBounds(ii) Then
                        upper = LowerBounds(ii)
                        lower = UpperBounds(ii)
                    Else
                        Throw New Exception("Error! upper bound and lower bound are same.")
                    End If

                    If temp(ii) > lower AndAlso temp(ii) < upper Then
                        'in
                    ElseIf temp(ii) >= lower AndAlso temp(ii) >= upper Then
                        temp(ii) = upper
                    ElseIf temp(ii) <= lower AndAlso temp(ii) <= upper Then
                        temp(ii) = lower
                    Else
                        Throw New Exception("Error!")
                    End If
                Next
            End If
        End Sub

        ''' <summary>
        ''' Limit solution space
        ''' </summary>
        ''' <param name="temp"></param>
        ''' <remarks></remarks>
        Protected Sub LimitSolutionSpace(ByRef temp As Point)
            If UseBounds = True Then
                LimitSolutionSpace(DirectCast(temp, MathUtil.EasyVector))
            End If
            temp.ReEvaluate()
        End Sub

        ''' <summary>
        ''' Get remaining number of iterarion count
        ''' </summary>
        ''' <param name="iterationCount"></param>
        Protected Function GetRemainingIterationCount(ByRef iterationCount As Integer) As Integer
            Dim remain = Me.Iteration - Me._IterationCount
            If remain <= 0 Then
                Return 0
            End If

            If iterationCount = 0 Then
                Return remain
            End If

            Dim diff = remain - iterationCount
            If diff > 0 Then
                Return iterationCount
            Else
                Return remain
            End If
        End Function
#End Region
    End Class
End Namespace